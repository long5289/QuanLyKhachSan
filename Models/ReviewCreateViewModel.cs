using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public class ReviewCreateViewModel
{
    public int BookingId { get; set; }
    [Range(1, 5, ErrorMessage = "Điểm đánh giá từ 1 đến 5.")] public int Rating { get; set; } = 5;
    [Required, StringLength(1000), Display(Name = "Nội dung")] public string Comment { get; set; } = string.Empty;
}