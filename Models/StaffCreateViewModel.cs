using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public class StaffCreateViewModel
{
    [Required, StringLength(120), Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(180), Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(30), Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password), Display(Name = "Mật khẩu tạm")]
    public string Password { get; set; } = string.Empty;
}