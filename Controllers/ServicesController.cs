using HotelBooking.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Controllers;

public class ServicesController(HotelDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index() => View(await dbContext.Services.OrderBy(service => service.Name).ToListAsync());
}