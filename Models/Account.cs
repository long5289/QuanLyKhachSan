using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public enum AccountRole { Customer, Staff, Admin }

public class Account
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(180)] public string Email { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;
    [StringLength(30)] public string Phone { get; set; } = string.Empty;
    public AccountRole Role { get; set; } = AccountRole.Customer;
}