using System;
using SWD302_Project_HostelManagement.VNPay;

namespace SWD302_Project_HostelManagement.VNPay;

public static class VNPayHelper
{
    /// <summary>
    /// Creates a VNPay payment URL for booking payment
    /// </summary>
    /// <param name="amount">Amount in VND</param>
    /// <param name="bookingId">Booking ID</param>
    /// <param name="description">Payment description</param>
    /// <returns>VNPay payment URL</returns>
    public static string CreatePaymentUrl(decimal amount, int bookingId, string description)
    {
        var pay = new VnPayLibrary();

        pay.AddRequestData("vnp_Version", "2.1.0");
        pay.AddRequestData("vnp_Command", "pay");
        pay.AddRequestData("vnp_TmnCode", VNPayConfig.TmnCode);
        pay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
        pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        pay.AddRequestData("vnp_CurrCode", "VND");
        pay.AddRequestData("vnp_IpAddr", "127.0.0.1");
        pay.AddRequestData("vnp_Locale", "vn");
        pay.AddRequestData("vnp_OrderInfo", description);
        pay.AddRequestData("vnp_OrderType", "other");
        pay.AddRequestData("vnp_ReturnUrl", VNPayConfig.ReturnUrl);
        pay.AddRequestData("vnp_TxnRef", $"BOOKING_{bookingId}_{DateTime.Now.Ticks}");

        return pay.CreateRequestUrl(VNPayConfig.BaseUrl, VNPayConfig.HashSecret);
    }
}
