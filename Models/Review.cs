using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public class Review
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    [Range(1, 5)] public int Rating { get; set; }
    [Required, StringLength(1000)] public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}