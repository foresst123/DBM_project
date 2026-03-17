namespace SWD302_Project_HostelManagement.VNPay;

/// <summary>
/// VNPay configuration loaded from appsettings.json
/// </summary>
public static class VNPayConfig
{
    private static string? _tmnCode;
    private static string? _hashSecret;
    private static string? _baseUrl;
    private static string? _returnUrl;

    /// <summary>
    /// Initialize VNPay configuration from IConfiguration
    /// </summary>
    public static void Initialize(IConfiguration configuration)
    {
        var vnpaySettings = configuration.GetSection("VNPaySettings");

        _tmnCode = vnpaySettings.GetValue<string>("TmnCode") 
            ?? throw new InvalidOperationException("VNPaySettings:TmnCode is not configured");

        _hashSecret = vnpaySettings.GetValue<string>("HashSecret")
            ?? throw new InvalidOperationException("VNPaySettings:HashSecret is not configured");

        _baseUrl = vnpaySettings.GetValue<string>("BaseUrl")
            ?? throw new InvalidOperationException("VNPaySettings:BaseUrl is not configured");

        _returnUrl = vnpaySettings.GetValue<string>("ReturnUrl")
            ?? throw new InvalidOperationException("VNPaySettings:ReturnUrl is not configured");
    }

    public static string TmnCode => _tmnCode ?? throw new InvalidOperationException("VNPayConfig not initialized");
    public static string HashSecret => _hashSecret ?? throw new InvalidOperationException("VNPayConfig not initialized");
    public static string BaseUrl => _baseUrl ?? throw new InvalidOperationException("VNPayConfig not initialized");
    public static string ReturnUrl => _returnUrl ?? throw new InvalidOperationException("VNPayConfig not initialized");
}
