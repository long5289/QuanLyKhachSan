using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public class LoginViewModel
{
    [Required, EmailAddress, Display(Name = "Email")] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Display(Name = "Mật khẩu")] public string Password { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }
}