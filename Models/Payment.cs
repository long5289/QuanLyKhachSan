using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public enum PaymentStatus { Pending, Paid, Refunded, Failed, Cancelled }

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public decimal Amount { get; set; }
    [Required, StringLength(40)] public string Method { get; set; } = "Cash";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    [StringLength(100)] public string TransactionCode { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
}