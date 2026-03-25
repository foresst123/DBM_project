import pandas as pd

# 1. Đọc dữ liệu
df = pd.read_csv("accepted_2007_to_2018Q4.csv")

# 2. Lọc loan_status (chỉ giữ Fully Paid & Charged Off)
df = df[df["loan_status"].isin(["Fully Paid", "Charged Off"])]

# 3. Tạo label
df["label"] = df["loan_status"].map({
    "Fully Paid": 0,
    "Charged Off": 1
})

# 4. Chọn các cột theo 5C
selected_columns = [
    # Character
    "pub_rec",
    "num_tl_90g_dpd_24m",

    # Capacity
    "annual_inc",
    "dti",
    "loan_amnt",
    "term",

    # Capital
    "tot_cur_bal",

    # Collateral
    "home_ownership",

    # Conditions
    "purpose",
    "verification_status",

    # Label
    "label"
]

df_final = df[selected_columns]

# 5. Xử lý missing (có thể tùy chỉnh thêm)
df_final = df_final.dropna()

# 6. Reset index
df_final = df_final.reset_index(drop=True)
missing_df = pd.DataFrame({
    "missing_count": df_final.isnull().sum(),
    "missing_percent": df_final.isnull().mean() * 100
})

print(missing_df)
# 7. Lưu file mới
df_final.to_csv("credit_risk_dataset_5c.csv", index=False)

print("✅ Đã tạo file credit_risk_dataset_5c.csv")
print("Shape:", df_final.shape)
print(df_final["label"].value_counts())