using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Controllers;

public class PaymentsController(HotelDbContext dbContext, IConfiguration configuration) : Controller
{
    [HttpGet]
    public async Task<IActionResult> DemoCheckout(int bookingId)
    {
        if (!configuration.GetValue<bool>("Payments:DemoMode")) return NotFound();
        if (HttpContext.Session.GetInt32("DemoCheckoutBookingId") != bookingId) return NotFound();

        var booking = await dbContext.Bookings.Include(item => item.Customer).Include(item => item.Room)
            .Include(item => item.Payment).Include(item => item.BookingServices)
            .FirstOrDefaultAsync(item => item.Id == bookingId);
        if (booking?.Payment is null || booking.Payment.Method != "Card") return NotFound();
        if (!string.Equals(HttpContext.Session.GetString("DemoCheckoutEmail"), booking.Customer.Email, StringComparison.OrdinalIgnoreCase)) return NotFound();

        var isFullBookingPayment = booking.Payment.Status == PaymentStatus.Pending;
        var unpaidServices = booking.BookingServices.Where(item => !item.IsPaid).ToList();
        var isServiceSupplement = booking.Payment.Status == PaymentStatus.Paid && unpaidServices.Count > 0;
        if (!isFullBookingPayment && !isServiceSupplement) return NotFound();
        var unpaidServiceTotal = unpaidServices.Sum(item => item.UnitPrice * item.Quantity);
        var outstandingAmount = Math.Max(0, booking.TotalAmount - booking.Payment.Amount);
        var checkoutAmount = isFullBookingPayment
            ? booking.Payment.Amount
            : Math.Min(unpaidServiceTotal, outstandingAmount);
        if (checkoutAmount <= 0) return NotFound();

        return View(new DemoCheckoutViewModel
        {
            BookingId = booking.Id,
            GuestName = booking.Customer.FullName,
            RoomName = booking.Room.Name,
            CheckIn = booking.CheckIn,
            CheckOut = booking.CheckOut,
            Amount = checkoutAmount,
            IsServiceSupplement = isServiceSupplement
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteDemo(int bookingId)
    {
        if (!configuration.GetValue<bool>("Payments:DemoMode")) return NotFound();
        if (HttpContext.Session.GetInt32("DemoCheckoutBookingId") != bookingId) return NotFound();

        var booking = await dbContext.Bookings.Include(item => item.Customer).Include(item => item.Room).Include(item => item.Payment).Include(item => item.BookingServices)
            .FirstOrDefaultAsync(item => item.Id == bookingId);
        if (booking?.Payment is null || booking.Payment.Method != "Card") return NotFound();
        if (!string.Equals(HttpContext.Session.GetString("DemoCheckoutEmail"), booking.Customer.Email, StringComparison.OrdinalIgnoreCase)) return NotFound();

        var unpaidServices = booking.BookingServices.Where(item => !item.IsPaid).ToList();
        var isSupplementalPayment = booking.Payment.Status == PaymentStatus.Paid;
        if (booking.Payment.Status == PaymentStatus.Pending)
        {
            booking.Payment.Status = PaymentStatus.Paid;
            booking.Payment.Amount = booking.TotalAmount;
        }
        else if (booking.Payment.Status == PaymentStatus.Paid && unpaidServices.Count == 0)
        {
            return NotFound();
        }
        else if (booking.Payment.Status != PaymentStatus.Paid)
        {
            return NotFound();
        }

        if (isSupplementalPayment)
        {
            var unpaidServiceTotal = unpaidServices.Sum(item => item.UnitPrice * item.Quantity);
            var outstandingAmount = Math.Max(0, booking.TotalAmount - booking.Payment.Amount);
            var supplementalPayment = Math.Min(unpaidServiceTotal, outstandingAmount);
            if (supplementalPayment <= 0) return NotFound();
            booking.Payment.Amount += supplementalPayment;
        }
        foreach (var service in unpaidServices) service.IsPaid = true;
        booking.Payment.PaidAt = DateTime.UtcNow;
        booking.Payment.TransactionCode = $"DEMO-{booking.Id}-{Guid.NewGuid():N}"[..20];
        await dbContext.SaveChangesAsync();
        HttpContext.Session.Remove("DemoCheckoutBookingId");
        return RedirectToAction(nameof(DemoResult), new { bookingId });
    }

    [HttpGet]
    public async Task<IActionResult> DemoResult(int bookingId)
    {
        var email = HttpContext.Session.GetString("AccountEmail");
        var booking = await dbContext.Bookings.Include(item => item.Customer).Include(item => item.Room).Include(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == bookingId);
        if (booking is null || booking.Payment?.Status != PaymentStatus.Paid) return NotFound();
        var sessionEmail = email ?? HttpContext.Session.GetString("DemoCheckoutEmail");
        if (!string.Equals(sessionEmail, booking.Customer.Email, StringComparison.OrdinalIgnoreCase)) return NotFound();
        HttpContext.Session.Remove("DemoCheckoutEmail");
        return View(booking);
    }
}