using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Controllers;

public class ReviewsController(HotelDbContext dbContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int bookingId)
    {
        var email = HttpContext.Session.GetString("AccountEmail");
        if (email is null) return RedirectToAction("Login", "Account", new { returnUrl = $"/Reviews/Create?bookingId={bookingId}" });

        var booking = await dbContext.Bookings.Include(item => item.Customer).Include(item => item.Reviews)
            .FirstOrDefaultAsync(item => item.Id == bookingId);
        if (booking is null || booking.Status == BookingStatus.Cancelled || !string.Equals(booking.Customer.Email, email, StringComparison.OrdinalIgnoreCase)) return NotFound();
        if (await dbContext.Reviews.AnyAsync(review => review.BookingId == bookingId)) return Conflict("Booking này đã được đánh giá.");
        return View(new ReviewCreateViewModel { BookingId = bookingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewCreateViewModel model)
    {
        var accountEmail = HttpContext.Session.GetString("AccountEmail");
        if (accountEmail is null) return RedirectToAction("Login", "Account", new { returnUrl = $"/Reviews/Create?bookingId={model.BookingId}" });
        var booking = await dbContext.Bookings.Include(item => item.Customer).Include(item => item.Reviews)
            .FirstOrDefaultAsync(item => item.Id == model.BookingId);
        if (booking is null) return NotFound();
        if (booking.Status == BookingStatus.Cancelled) ModelState.AddModelError(string.Empty, "Booking đã hủy nên không thể gửi đánh giá.");
        if (!string.Equals(booking.Customer.Email, accountEmail, StringComparison.OrdinalIgnoreCase)) return NotFound();
        if (await dbContext.Reviews.AnyAsync(review => review.BookingId == model.BookingId)) ModelState.AddModelError(string.Empty, "Booking này đã được đánh giá.");
        if (!ModelState.IsValid) return View(model);
        dbContext.Reviews.Add(new Review { BookingId = booking.Id, CustomerId = booking.CustomerId, Rating = model.Rating, Comment = model.Comment });
        await dbContext.SaveChangesAsync();
        return View("Success");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var email = HttpContext.Session.GetString("AccountEmail");
        if (email is null) return RedirectToAction("Login", "Account", new { returnUrl = $"/Reviews/Edit/{id}" });

        var review = await dbContext.Reviews.Include(item => item.Booking).ThenInclude(booking => booking.Customer)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (review is null || !string.Equals(review.Booking.Customer.Email, email, StringComparison.OrdinalIgnoreCase)) return NotFound();

        return View(new ReviewCreateViewModel { BookingId = review.BookingId, Rating = review.Rating, Comment = review.Comment });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReviewCreateViewModel model)
    {
        var email = HttpContext.Session.GetString("AccountEmail");
        if (email is null) return RedirectToAction("Login", "Account", new { returnUrl = $"/Reviews/Edit/{id}" });

        var review = await dbContext.Reviews.Include(item => item.Booking).ThenInclude(booking => booking.Customer)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (review is null || !string.Equals(review.Booking.Customer.Email, email, StringComparison.OrdinalIgnoreCase)) return NotFound();
        if (model.BookingId != review.BookingId) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        review.Rating = model.Rating;
        review.Comment = model.Comment.Trim();
        await dbContext.SaveChangesAsync();
        return RedirectToAction("Mine", "Bookings");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var email = HttpContext.Session.GetString("AccountEmail");
        if (email is null) return RedirectToAction("Login", "Account", new { returnUrl = "/Bookings/Mine" });

        var review = await dbContext.Reviews.Include(item => item.Booking).ThenInclude(booking => booking.Customer)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (review is null || !string.Equals(review.Booking.Customer.Email, email, StringComparison.OrdinalIgnoreCase)) return NotFound();

        dbContext.Reviews.Remove(review);
        await dbContext.SaveChangesAsync();
        return RedirectToAction("Mine", "Bookings");
    }
}