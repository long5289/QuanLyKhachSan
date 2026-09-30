using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Controllers;

public class StaffController(HotelDbContext dbContext) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var role = HttpContext.Session.GetString("AccountRole");
        if (role is not (nameof(AccountRole.Staff) or nameof(AccountRole.Admin)))
        {
            context.Result = RedirectToAction("Login", "Account", new { returnUrl = HttpContext.Request.Path });
        }
        base.OnActionExecuting(context);
    }

    public async Task<IActionResult> Index()
    {
        var bookings = await dbContext.Bookings.Include(booking => booking.Room)
            .Include(booking => booking.Customer).Include(booking => booking.Payment)
            .Where(booking => booking.Status == BookingStatus.Pending
                || booking.Status == BookingStatus.Confirmed
                || booking.Status == BookingStatus.CheckedIn
                || (booking.Status == BookingStatus.Cancelled && booking.Payment != null && booking.Payment.Status == PaymentStatus.Paid))
            .OrderBy(booking => booking.CheckIn).ToListAsync();
        return View(bookings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, BookingStatus status)
    {
        var booking = await dbContext.Bookings.Include(item => item.Payment).FirstOrDefaultAsync(item => item.Id == id);
        var allowed = booking is not null && (booking.Status, status) switch
        {
            (BookingStatus.Pending, BookingStatus.Confirmed) => true,
            (BookingStatus.Pending, BookingStatus.Cancelled) => true,
            (BookingStatus.Confirmed, BookingStatus.CheckedIn) => true,
            (BookingStatus.Confirmed, BookingStatus.Cancelled) => true,
            (BookingStatus.CheckedIn, BookingStatus.Completed) => true,
            _ => false
        };
        if (booking is not null && allowed)
        {
            booking.Status = status;
            if (status == BookingStatus.Cancelled && booking.Payment?.Status == PaymentStatus.Pending)
            {
                booking.Payment.Status = PaymentStatus.Cancelled;
            }
            await dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRefunded(int id)
    {
        var booking = await dbContext.Bookings.Include(item => item.Payment).FirstOrDefaultAsync(item => item.Id == id);
        if (booking is not null
            && booking.Status == BookingStatus.Cancelled
            && booking.Payment?.Status == PaymentStatus.Paid)
        {
            booking.Payment.Status = PaymentStatus.Refunded;
            await dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}