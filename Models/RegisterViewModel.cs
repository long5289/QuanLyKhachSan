using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public class RegisterViewModel
{
    [Required, StringLength(120), Display(Name = "Họ và tên")] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, Display(Name = "Email")] public string Email { get; set; } = string.Empty;
    [Required, StringLength(30), Display(Name = "Số điện thoại")] public string Phone { get; set; } = string.Empty;
    [Required, MinLength(6), DataType(DataType.Password), Display(Name = "Mật khẩu")] public string Password { get; set; } = string.Empty;
    [Required, Compare(nameof(Password)), DataType(DataType.Password), Display(Name = "Nhập lại mật khẩu")] public string ConfirmPassword { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
}