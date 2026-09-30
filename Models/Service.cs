using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public class Service
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Name { get; set; } = string.Empty;
    [Range(0, double.MaxValue)] public decimal Price { get; set; }
    [StringLength(500)] public string Description { get; set; } = string.Empty;
    public ICollection<BookingService> BookingServices { get; set; } = [];
}