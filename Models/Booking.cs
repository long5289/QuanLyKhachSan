namespace HotelBooking.Web.Models;

public enum BookingStatus { Pending, Confirmed, CheckedIn, Completed, Cancelled }

public class Booking
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Payment? Payment { get; set; }
    public ICollection<BookingService> BookingServices { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}