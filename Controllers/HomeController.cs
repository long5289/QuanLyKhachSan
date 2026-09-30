using System.Diagnostics;
using HotelBooking.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelBooking.Web.Models;

namespace HotelBooking.Web.Controllers;

public class HomeController(HotelDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(new HomeViewModel
        {
            Search = new RoomSearchViewModel { CheckIn = DateTime.Today.AddDays(1), CheckOut = DateTime.Today.AddDays(2), Guests = 2 },
            FeaturedRooms = await dbContext.Rooms.Where(room => room.Status == RoomStatus.Available).OrderBy(room => room.PricePerNight).Take(4).ToListAsync(),
            Services = await dbContext.Services.OrderBy(service => service.Name).Take(3).ToListAsync(),
            Reviews = await dbContext.Reviews.Include(review => review.Customer).OrderByDescending(review => review.CreatedAt).Take(3).ToListAsync()
        });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
