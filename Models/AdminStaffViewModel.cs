namespace HotelBooking.Web.Models;

public class AdminStaffViewModel
{
    public StaffCreateViewModel NewStaff { get; set; } = new();
    public IReadOnlyList<Account> Staff { get; set; } = [];
}