namespace HotelBooking.Web.Models;

public class DemoCheckoutViewModel
{
    public int BookingId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public decimal Amount { get; set; }
    public bool IsServiceSupplement { get; set; }
}