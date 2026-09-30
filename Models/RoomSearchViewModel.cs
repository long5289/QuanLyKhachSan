using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Web.Models;

public class RoomSearchViewModel
{
    [Display(Name = "Nhận phòng"), DataType(DataType.Date)] public DateTime CheckIn { get; set; }
    [Display(Name = "Trả phòng"), DataType(DataType.Date)] public DateTime CheckOut { get; set; }
    [Range(1, 20)] public int Guests { get; set; }
}