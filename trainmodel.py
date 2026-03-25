import os
import json
import time
import copy
import random
import warnings
from pathlib import Path

import joblib
import numpy as np
import pandas as pd
import shap

import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import TensorDataset, DataLoader

from sklearn.linear_model import LogisticRegression
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from sklearn.metrics import (
    accuracy_score,
    classification_report,
    f1_score,
    precision_score,
    recall_score,
    roc_auc_score,
)
from sklearn.utils.class_weight import compute_class_weight
from xgboost import XGBClassifier
from imblearn.over_sampling import RandomOverSampler

warnings.filterwarnings("ignore")

# =========================
# 0. Reproducibility
# =========================
SEED = 42
random.seed(SEED)
np.random.seed(SEED)
torch.manual_seed(SEED)

if torch.cuda.is_available():
    torch.cuda.manual_seed_all(SEED)

# =========================
# 1. Config
# =========================
TRAIN_PATH = "credit_risk_train.csv"
TEST_PATH = "credit_risk_test.csv"
TARGET_COL = "label"   # doi thanh loan_status neu file cua ban dung ten nay

OUTPUT_DIR = Path("deploy_outputs")
OUTPUT_DIR.mkdir(exist_ok=True)

TOP_K_EXPLANATION = 10
EXPLAIN_SAMPLE_SIZE = 200

device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
print("Using device:", device)
print("CUDA available:", torch.cuda.is_available())
if torch.cuda.is_available():
    print("GPU:", torch.cuda.get_device_name(0))


# =========================
# 2. Models
# =========================
class TabTransformer(nn.Module):
    """
    Simple TabTransformer for all-numeric preprocessed tabular data.
    Each feature is treated as one token.
    """
    def __init__(
        self,
        num_features,
        num_classes,
        d_token=32,
        n_heads=4,
        n_layers=2,
        dim_feedforward=128,
        dropout=0.2,
    ):
        super().__init__()
        self.num_features = num_features
        self.d_token = d_token

        self.feature_embed = nn.Linear(1, d_token)
        # Learned identity embedding for each feature token
        self.feature_id_embed = nn.Embedding(num_features, d_token)
        self.register_buffer("feature_ids", torch.arange(num_features), persistent=False)
        self.cls_token = nn.Parameter(torch.zeros(1, 1, d_token))

        encoder_layer = nn.TransformerEncoderLayer(
            d_model=d_token,
            nhead=n_heads,
            dim_feedforward=dim_feedforward,
            dropout=dropout,
            batch_first=True,
            activation="gelu",
        )
        self.transformer = nn.TransformerEncoder(encoder_layer, num_layers=n_layers)

        self.head = nn.Sequential(
            nn.LayerNorm(d_token),
            nn.Linear(d_token, 64),
            nn.ReLU(),
            nn.Dropout(dropout),
            nn.Linear(64, num_classes),
        )

    def forward(self, x):
        # x: [batch, num_features]
        x = x.unsqueeze(-1)                    # [batch, num_features, 1]
        x = self.feature_embed(x)              # [batch, num_features, d_token]
        feat_ids = self.feature_ids.unsqueeze(0).expand(x.size(0), -1)
        x = x + self.feature_id_embed(feat_ids)

        cls = self.cls_token.expand(x.size(0), -1, -1)
        x = torch.cat([cls, x], dim=1)         # [batch, 1+num_features, d_token]

        x = self.transformer(x)
        cls_out = x[:, 0, :]
        return self.head(cls_out)


# =========================
# 3. Helper functions
# =========================
def export_json(path: Path, obj):
    with open(path, "w", encoding="utf-8") as f:
        json.dump(obj, f, ensure_ascii=False, indent=2)


def ensure_numeric(df: pd.DataFrame) -> pd.DataFrame:
    df = df.copy()
    df = df.apply(pd.to_numeric, errors="coerce").fillna(0)

    bool_cols = df.select_dtypes(include=["bool"]).columns
    if len(bool_cols) > 0:
        df[bool_cols] = df[bool_cols].astype(int)

    return df.astype(np.float32)


def align_features(X_train: pd.DataFrame, X_test: pd.DataFrame):
    X_train_aligned, X_test_aligned = X_train.align(
        X_test, join="outer", axis=1, fill_value=0
    )
    return X_train_aligned, X_test_aligned


def evaluate_model(y_true, y_pred, y_proba=None, model_name="Model"):
    result = {
        "Model": model_name,
        "Accuracy": accuracy_score(y_true, y_pred),
        "Precision_weighted": precision_score(y_true, y_pred, average="weighted", zero_division=0),
        "Recall_weighted": recall_score(y_true, y_pred, average="weighted", zero_division=0),
        "F1_weighted": f1_score(y_true, y_pred, average="weighted", zero_division=0),
        "F1_macro": f1_score(y_true, y_pred, average="macro", zero_division=0),
        "ROC_AUC": np.nan,
    }

    if y_proba is not None:
        try:
            if len(np.unique(y_true)) == 2:
                result["ROC_AUC"] = roc_auc_score(y_true, y_proba[:, 1])
            else:
                result["ROC_AUC"] = roc_auc_score(
                    y_true, y_proba, multi_class="ovr", average="weighted"
                )
        except Exception:
            pass

    print(f"\n{'=' * 15} {model_name} {'=' * 15}")
    for k, v in result.items():
        if k != "Model":
            print(f"{k}: {v}")
    print("\nClassification Report:")
    print(classification_report(y_true, y_pred, zero_division=0))

    return result


def logistic_instance_explanation(model, x_row: pd.Series, feature_names, top_k=10):
    coef = model.coef_[0]
    values = x_row.values.astype(float)
    contrib = values * coef

    exp_df = pd.DataFrame({
        "feature": feature_names,
        "feature_value": values,
        "coefficient": coef,
        "impact": contrib,
    })

    exp_df["abs_impact"] = exp_df["impact"].abs()
    exp_df = exp_df.sort_values("abs_impact", ascending=False).head(top_k).copy()
    exp_df["direction"] = np.where(exp_df["impact"] >= 0, "increase_risk", "decrease_risk")
    return exp_df[["feature", "feature_value", "coefficient", "impact", "direction"]]


def xgb_instance_explanation(explainer, x_row_df: pd.DataFrame, top_k=10):
    shap_values = explainer.shap_values(x_row_df)

    if isinstance(shap_values, list):
        shap_row = shap_values[1][0]
    else:
        shap_arr = np.array(shap_values)
        if shap_arr.ndim == 3:
            shap_row = shap_arr[0, :, 1]
        else:
            shap_row = shap_arr[0]

    row_values = x_row_df.iloc[0].values.astype(float)
    feature_names = x_row_df.columns.tolist()

    exp_df = pd.DataFrame({
        "feature": feature_names,
        "feature_value": row_values,
        "impact": shap_row,
    })

    exp_df["abs_impact"] = exp_df["impact"].abs()
    exp_df = exp_df.sort_values("abs_impact", ascending=False).head(top_k).copy()
    exp_df["direction"] = np.where(exp_df["impact"] >= 0, "increase_risk", "decrease_risk")
    return exp_df[["feature", "feature_value", "impact", "direction"]]


def tabtransformer_instance_explanation(model, x_row_df: pd.DataFrame, top_k=10, device="cpu"):
    """
    Gradient-based explanation for one row.
    impact ~ gradient * input
    """
    model.eval()

    x_tensor = torch.tensor(
        x_row_df.values, dtype=torch.float32, device=device, requires_grad=True
    )

    output = model(x_tensor)
    if output.shape[1] == 2:
        target_logit = output[:, 1].sum()
    else:
        pred_class = torch.argmax(output, dim=1)
        target_logit = output[0, pred_class.item()]

    model.zero_grad()
    target_logit.backward()

    grads = x_tensor.grad.detach().cpu().numpy()[0]
    vals = x_row_df.iloc[0].values.astype(float)
    impacts = grads * vals

    exp_df = pd.DataFrame({
        "feature": x_row_df.columns.tolist(),
        "feature_value": vals,
        "gradient": grads,
        "impact": impacts,
    })

    exp_df["abs_impact"] = exp_df["impact"].abs()
    exp_df = exp_df.sort_values("abs_impact", ascending=False).head(top_k).copy()
    exp_df["direction"] = np.where(exp_df["impact"] >= 0, "increase_risk", "decrease_risk")
    return exp_df[["feature", "feature_value", "gradient", "impact", "direction"]]


# =========================
# 4. Load data
# =========================
train_data = pd.read_csv(TRAIN_PATH)
test_data = pd.read_csv(TEST_PATH)

train_data.columns = train_data.columns.str.strip()
test_data.columns = test_data.columns.str.strip()

if TARGET_COL not in train_data.columns:
    raise ValueError(f"Không tìm thấy cột nhãn '{TARGET_COL}' trong file train.")
if TARGET_COL not in test_data.columns:
    raise ValueError(f"Không tìm thấy cột nhãn '{TARGET_COL}' trong file test.")

X_train = train_data.drop(columns=[TARGET_COL]).copy()
y_train = train_data[TARGET_COL].copy()

X_test = test_data.drop(columns=[TARGET_COL]).copy()
y_test = test_data[TARGET_COL].copy()

# optional state column is intentionally excluded in this setup
X_train = X_train.drop(columns=["addr_state"], errors="ignore")
X_test = X_test.drop(columns=["addr_state"], errors="ignore")

# =========================
# 5. Clean labels
# =========================
y_train = pd.to_numeric(y_train, errors="coerce").fillna(0).astype(int)
y_test = pd.to_numeric(y_test, errors="coerce").fillna(0).astype(int)

label_min = min(y_train.min(), y_test.min())
if label_min != 0:
    y_train = y_train - label_min
    y_test = y_test - label_min

num_classes = len(np.unique(y_train))
if num_classes < 2:
    raise ValueError("Dữ liệu train phải có ít nhất 2 class.")

if num_classes != 2:
    raise ValueError("Code giải thích hiện đang thiết kế cho binary classification.")

# =========================
# 6. Align + numeric coercion
# =========================
X_train, X_test = align_features(X_train, X_test)
X_train = ensure_numeric(X_train)
X_test = ensure_numeric(X_test)

feature_names = X_train.columns.tolist()
num_features = len(feature_names)
print("Số lượng features:", num_features)

# =========================
# 7. Imbalance handling
# =========================
# Logistic: class_weight
# XGBoost: oversampling
# TabTransformer: class weights
oversampler = RandomOverSampler(random_state=SEED)
X_train_res, y_train_res = oversampler.fit_resample(X_train, y_train)

X_train_res = pd.DataFrame(X_train_res, columns=feature_names).astype(np.float32)
y_train_res = pd.Series(y_train_res, name=TARGET_COL).astype(int)

print("\nPhân bố class train sau oversampling:")
print(y_train_res.value_counts().sort_index())

classes = np.unique(y_train)
class_weights = compute_class_weight(class_weight="balanced", classes=classes, y=y_train)
class_weight_map = {int(c): float(w) for c, w in zip(classes, class_weights)}
print("\nClass weights:", class_weight_map)

# =========================
# 8. Logistic Regression
# =========================
print("\nTraining Logistic Regression...")
log_train_start = time.perf_counter()

log_model = LogisticRegression(
    max_iter=3000,
    class_weight="balanced",
    random_state=SEED,
    solver="lbfgs",
)

log_model.fit(X_train, y_train)
log_train_time = time.perf_counter() - log_train_start

log_pred_start = time.perf_counter()
log_preds = log_model.predict(X_test)
log_probs = log_model.predict_proba(X_test)
log_pred_time = time.perf_counter() - log_pred_start
log_total_time = log_train_time + log_pred_time

print(f"Logistic train time  : {log_train_time:.2f} sec")
print(f"Logistic predict time: {log_pred_time:.2f} sec")
print(f"Logistic total time  : {log_total_time:.2f} sec")

log_metrics = evaluate_model(y_test, log_preds, log_probs, "Logistic Regression")

# =========================
# 9. XGBoost
# =========================
print("\nTraining XGBoost...")
xgb_train_start = time.perf_counter()

xgb_params = dict(
    n_estimators=300,
    max_depth=8,
    learning_rate=0.05,
    subsample=0.8,
    colsample_bytree=0.9,
    objective="binary:logistic",
    eval_metric="logloss",
    random_state=SEED,
    n_jobs=-1,
    device="cuda",
)

try:
    xgb_model = XGBClassifier(**xgb_params)
    xgb_model.fit(X_train_res, y_train_res)
    xgb_device_used = "cuda"
except Exception as e:
    print(f"XGBoost CUDA không chạy được, fallback về CPU. Lý do: {e}")
    xgb_params["device"] = "cpu"
    xgb_model = XGBClassifier(**xgb_params)
    xgb_model.fit(X_train_res, y_train_res)
    xgb_device_used = "cpu"

xgb_train_time = time.perf_counter() - xgb_train_start

xgb_pred_start = time.perf_counter()
xgb_preds = xgb_model.predict(X_test)
xgb_probs = xgb_model.predict_proba(X_test)
xgb_pred_time = time.perf_counter() - xgb_pred_start
xgb_total_time = xgb_train_time + xgb_pred_time

print(f"XGBoost device       : {xgb_device_used}")
print(f"XGBoost train time   : {xgb_train_time:.2f} sec")
print(f"XGBoost predict time : {xgb_pred_time:.2f} sec")
print(f"XGBoost total time   : {xgb_total_time:.2f} sec")

xgb_metrics = evaluate_model(y_test, xgb_preds, xgb_probs, "XGBoost")

# =========================
# 10. TabTransformer
# =========================
print("\nTraining TabTransformer...")

X_train_tab, X_val_tab, y_train_tab, y_val_tab = train_test_split(
    X_train,
    y_train,
    test_size=0.2,
    random_state=SEED,
    stratify=y_train
)

# TabTransformer uses oversampling only on training split (not on validation split)
oversampler_tab = RandomOverSampler(random_state=SEED)
X_train_tab_res, y_train_tab_res = oversampler_tab.fit_resample(X_train_tab, y_train_tab)

print("\nPhân bố class TabTransformer train sau oversampling:")
print(pd.Series(y_train_tab_res).value_counts().sort_index())

# Scale numeric features for stable neural training
tab_scaler = StandardScaler()
X_train_tab_res = tab_scaler.fit_transform(X_train_tab_res)
X_val_tab_scaled = tab_scaler.transform(X_val_tab)
X_test_tab_scaled = tab_scaler.transform(X_test)

X_train_tab_tensor = torch.tensor(X_train_tab_res, dtype=torch.float32)
y_train_tab_tensor = torch.tensor(np.asarray(y_train_tab_res), dtype=torch.long)

X_val_tab_tensor = torch.tensor(X_val_tab_scaled, dtype=torch.float32)
y_val_tab_tensor = torch.tensor(y_val_tab.to_numpy(), dtype=torch.long)

X_test_tensor = torch.tensor(X_test_tab_scaled, dtype=torch.float32).to(device)

train_dataset = TensorDataset(X_train_tab_tensor, y_train_tab_tensor)
val_dataset = TensorDataset(X_val_tab_tensor, y_val_tab_tensor)

train_loader = DataLoader(train_dataset, batch_size=256, shuffle=True)
val_loader = DataLoader(val_dataset, batch_size=256, shuffle=False)

tab_train_start = time.perf_counter()

tab_model = TabTransformer(
    num_features=num_features,
    num_classes=num_classes,
    d_token=32,
    n_heads=4,
    n_layers=2,
    dim_feedforward=128,
    dropout=0.2,
).to(device)

criterion = nn.CrossEntropyLoss()
optimizer = optim.Adam(tab_model.parameters(), lr=1e-3, weight_decay=1e-5)

EPOCHS = 50
PATIENCE = 10
MIN_DELTA = 1e-4

best_val_loss = float("inf")
best_model_state = None
patience_counter = 0

for epoch in range(EPOCHS):
    tab_model.train()
    train_loss = 0.0

    for batch_X, batch_y in train_loader:
        batch_X = batch_X.to(device)
        batch_y = batch_y.to(device).long()

        optimizer.zero_grad()
        output = tab_model(batch_X)
        loss = criterion(output, batch_y)
        loss.backward()
        optimizer.step()

        train_loss += loss.item() * batch_X.size(0)

    train_loss /= len(train_loader.dataset)

    tab_model.eval()
    val_loss = 0.0
    with torch.no_grad():
        for batch_X, batch_y in val_loader:
            batch_X = batch_X.to(device)
            batch_y = batch_y.to(device).long()

            output = tab_model(batch_X)
            loss = criterion(output, batch_y)
            val_loss += loss.item() * batch_X.size(0)

    val_loss /= len(val_loader.dataset)

    if epoch % 5 == 0 or epoch == EPOCHS - 1:
        print(f"Epoch {epoch:03d} | Train Loss: {train_loss:.4f} | Val Loss: {val_loss:.4f}")

    if val_loss < best_val_loss - MIN_DELTA:
        best_val_loss = val_loss
        best_model_state = copy.deepcopy(tab_model.state_dict())
        patience_counter = 0
    else:
        patience_counter += 1

    if patience_counter >= PATIENCE:
        print(f"Early stopping at epoch {epoch}")
        break

if best_model_state is not None:
    tab_model.load_state_dict(best_model_state)

tab_train_time = time.perf_counter() - tab_train_start

tab_pred_start = time.perf_counter()
tab_model.eval()
with torch.no_grad():
    tab_logits = tab_model(X_test_tensor)
    tab_probs = torch.softmax(tab_logits, dim=1).cpu().numpy()
    tab_preds = np.argmax(tab_probs, axis=1)
tab_pred_time = time.perf_counter() - tab_pred_start
tab_total_time = tab_train_time + tab_pred_time

print(f"TabTransformer train time  : {tab_train_time:.2f} sec")
print(f"TabTransformer predict time: {tab_pred_time:.2f} sec")
print(f"TabTransformer total time  : {tab_total_time:.2f} sec")

tab_metrics = evaluate_model(y_test, tab_preds, tab_probs, "TabTransformer")

# =========================
# 11. Save models + feature schema
# =========================
joblib.dump(log_model, OUTPUT_DIR / "logistic_model.joblib")
joblib.dump(feature_names, OUTPUT_DIR / "feature_names.joblib")
xgb_model.save_model(str(OUTPUT_DIR / "xgboost_model.json"))
torch.save(tab_model.state_dict(), OUTPUT_DIR / "tabtransformer_model.pt")

tab_meta = {
    "num_features": num_features,
    "num_classes": num_classes,
    "d_token": 32,
    "n_heads": 4,
    "n_layers": 2,
    "dim_feedforward": 128,
    "dropout": 0.2,
}

meta = {
    "target_col": TARGET_COL,
    "num_features": num_features,
    "num_classes": int(num_classes),
    "device_used_for_tabtransformer": str(device),
    "xgboost_device_used": xgb_device_used,
    "feature_names_file": "feature_names.joblib",
    "logistic_model_file": "logistic_model.joblib",
    "xgboost_model_file": "xgboost_model.json",
    "tabtransformer_model_file": "tabtransformer_model.pt",
    "tabtransformer_config": tab_meta,
}

export_json(OUTPUT_DIR / "model_meta.json", meta)

# =========================
# 12. Summary for web
# =========================
summary = pd.DataFrame([
    {
        "Model": "Logistic Regression",
        "Train_time_sec": round(log_train_time, 4),
        "Predict_time_sec": round(log_pred_time, 4),
        "Total_time_sec": round(log_total_time, 4),
        **log_metrics,
    },
    {
        "Model": "XGBoost",
        "Train_time_sec": round(xgb_train_time, 4),
        "Predict_time_sec": round(xgb_pred_time, 4),
        "Total_time_sec": round(xgb_total_time, 4),
        **xgb_metrics,
        "Device": xgb_device_used,
    },
    {
        "Model": "TabTransformer",
        "Train_time_sec": round(tab_train_time, 4),
        "Predict_time_sec": round(tab_pred_time, 4),
        "Total_time_sec": round(tab_total_time, 4),
        **tab_metrics,
        "Device": str(device),
    },
])

summary = summary.drop(columns=["Model"], errors="ignore").assign(
    Model=["Logistic Regression", "XGBoost", "TabTransformer"]
)

summary = summary[
    ["Model", "Train_time_sec", "Predict_time_sec", "Total_time_sec",
     "Accuracy", "Precision_weighted", "Recall_weighted", "F1_weighted", "F1_macro", "ROC_AUC"]
]

summary = summary.sort_values(by="F1_macro", ascending=False)
summary.to_csv(OUTPUT_DIR / "model_summary.csv", index=False)
export_json(OUTPUT_DIR / "model_summary.json", summary.to_dict(orient="records"))

print("\n================ Model Comparison ================\n")
print(summary.to_string(index=False))

# =========================
# 13. Batch predictions for web
# =========================
pred_df = pd.DataFrame({
    "y_true": y_test.reset_index(drop=True),
    "logistic_pred": log_preds,
    "logistic_prob_1": log_probs[:, 1],
    "xgboost_pred": xgb_preds,
    "xgboost_prob_1": xgb_probs[:, 1],
    "tabtransformer_pred": tab_preds,
    "tabtransformer_prob_1": tab_probs[:, 1],
})

pred_df.to_csv(OUTPUT_DIR / "model_predictions.csv", index=False)

# =========================
# 14. Explanations
# =========================
n_samples = min(EXPLAIN_SAMPLE_SIZE, len(X_test))

# Logistic explanations
log_explanations = []
for i in range(n_samples):
    x_row = X_test.iloc[i]
    exp_df = logistic_instance_explanation(
        model=log_model,
        x_row=x_row,
        feature_names=feature_names,
        top_k=TOP_K_EXPLANATION,
    )
    log_explanations.append({
        "row_id": int(i),
        "y_true": int(y_test.iloc[i]),
        "prediction": int(log_preds[i]),
        "probability_default": float(log_probs[i, 1]),
        "top_features": exp_df.to_dict(orient="records"),
    })

export_json(OUTPUT_DIR / "logistic_explanations.json", log_explanations)

# XGBoost explanations
print("\nBuilding SHAP explainer for XGBoost...")
xgb_explainer = shap.TreeExplainer(xgb_model)

xgb_explanations = []
for i in range(n_samples):
    x_row_df = X_test.iloc[[i]]
    exp_df = xgb_instance_explanation(
        explainer=xgb_explainer,
        x_row_df=x_row_df,
        top_k=TOP_K_EXPLANATION,
    )
    xgb_explanations.append({
        "row_id": int(i),
        "y_true": int(y_test.iloc[i]),
        "prediction": int(xgb_preds[i]),
        "probability_default": float(xgb_probs[i, 1]),
        "top_features": exp_df.to_dict(orient="records"),
    })

export_json(OUTPUT_DIR / "xgboost_explanations.json", xgb_explanations)

# TabTransformer explanations
print("\nBuilding gradient-based explanations for TabTransformer...")
tab_explanations = []
for i in range(n_samples):
    x_row_df = X_test.iloc[[i]]
    exp_df = tabtransformer_instance_explanation(
        model=tab_model,
        x_row_df=x_row_df,
        top_k=TOP_K_EXPLANATION,
        device=device,
    )
    tab_explanations.append({
        "row_id": int(i),
        "y_true": int(y_test.iloc[i]),
        "prediction": int(tab_preds[i]),
        "probability_default": float(tab_probs[i, 1]),
        "top_features": exp_df.to_dict(orient="records"),
    })

export_json(OUTPUT_DIR / "tabtransformer_explanations.json", tab_explanations)

# =========================
# 15. One merged web payload
# =========================
web_payload = {
    "meta": meta,
    "summary": summary.to_dict(orient="records"),
    "prediction_preview": pred_df.head(100).to_dict(orient="records"),
    "logistic_explanations_preview": log_explanations[:20],
    "xgboost_explanations_preview": xgb_explanations[:20],
    "tabtransformer_explanations_preview": tab_explanations[:20],
}

export_json(OUTPUT_DIR / "web_payload.json", web_payload)

print("\nĐã xuất file phục vụ nhúng web vào thư mục:", OUTPUT_DIR.resolve())
print("- logistic_model.joblib")
print("- xgboost_model.json")
print("- tabtransformer_model.pt")
print("- feature_names.joblib")
print("- model_meta.json")
print("- model_summary.csv")
print("- model_summary.json")
print("- model_predictions.csv")
print("- logistic_explanations.json")
print("- xgboost_explanations.json")
print("- tabtransformer_explanations.json")
print("- web_payload.json")
