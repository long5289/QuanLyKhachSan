namespace HotelBooking.Web.Models;

public class BookingService
{
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public bool IsPaid { get; set; }
}