namespace HotelBooking.Web.Models;

public class BookingDetailsViewModel
{
    public Booking Booking { get; set; } = null!;
    public IReadOnlyList<Service> AvailableServices { get; set; } = [];
    public decimal PaidAmount { get; set; }
    public decimal AmountDue { get; set; }
    public bool CanAddServices { get; set; }
}