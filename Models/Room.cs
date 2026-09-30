using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public enum RoomStatus { Available, Occupied, Maintenance }

public class Room
{
    public int Id { get; set; }
    [Required, StringLength(20)] public string Code { get; set; } = string.Empty;
    [Required, StringLength(120)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(50)] public string Type { get; set; } = string.Empty;
    [Range(1, 100)] public int Capacity { get; set; }
    [Range(0, double.MaxValue)] public decimal PricePerNight { get; set; }
    [StringLength(1000)] public string Description { get; set; } = string.Empty;
    public RoomStatus Status { get; set; } = RoomStatus.Available;
    public ICollection<Booking> Bookings { get; set; } = [];
}