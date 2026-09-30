using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public class BookingCreateViewModel
{
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    [Required, StringLength(120), Display(Name = "Họ và tên")] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, Display(Name = "Email")] public string Email { get; set; } = string.Empty;
    [Required, StringLength(30), Display(Name = "Số điện thoại")] public string Phone { get; set; } = string.Empty;
    [DataType(DataType.Date), Display(Name = "Nhận phòng")] public DateTime CheckIn { get; set; }
    [DataType(DataType.Date), Display(Name = "Trả phòng")] public DateTime CheckOut { get; set; }
    public List<int> SelectedServiceIds { get; set; } = [];
    public IReadOnlyList<Service> Services { get; set; } = [];
    [Required, StringLength(40)] public string PaymentMethod { get; set; } = string.Empty;
}