using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using HotelBooking.Web.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Controllers;

public class AdminController(HotelDbContext dbContext) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (HttpContext.Session.GetString("AccountRole") != AccountRole.Admin.ToString())
        {
            context.Result = RedirectToAction("Login", "Account", new { returnUrl = HttpContext.Request.Path });
        }
        base.OnActionExecuting(context);
    }

    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            TotalRooms = await dbContext.Rooms.CountAsync(),
            AvailableRooms = await dbContext.Rooms.CountAsync(room => room.Status == RoomStatus.Available),
            PendingBookings = await dbContext.Bookings.CountAsync(booking => booking.Status == BookingStatus.Pending),
            Revenue = await dbContext.Payments.Where(payment => payment.Status == PaymentStatus.Paid).SumAsync(payment => (decimal?)payment.Amount) ?? 0,
            RecentBookings = await dbContext.Bookings.Include(booking => booking.Room).Include(booking => booking.Customer).Include(booking => booking.Payment).OrderByDescending(booking => booking.CreatedAt).Take(8).ToListAsync()
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Rooms() => View(new AdminRoomsViewModel { Rooms = await dbContext.Rooms.OrderBy(room => room.Code).ToListAsync() });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRoom(Room room)
    {
        var existing = await dbContext.Rooms.FindAsync(room.Id);
        if (existing is null) return NotFound();
        if (!ModelState.IsValid) return RedirectToAction(nameof(Rooms));
        existing.Code = room.Code;
        existing.Name = room.Name;
        existing.Type = room.Type;
        existing.Capacity = room.Capacity;
        existing.PricePerNight = room.PricePerNight;
        existing.Description = room.Description;
        existing.Status = room.Status;
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Rooms));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await dbContext.Rooms.Include(item => item.Bookings).FirstOrDefaultAsync(item => item.Id == id);
        if (room is null) return NotFound();
        if (room.Bookings.Count == 0)
        {
            dbContext.Rooms.Remove(room);
            await dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Rooms));
    }

    [HttpGet]
    public async Task<IActionResult> Services() => View(await dbContext.Services.OrderBy(service => service.Name).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> Staff() => View(new AdminStaffViewModel { Staff = await dbContext.Accounts.Where(account => account.Role == AccountRole.Staff).OrderBy(account => account.FullName).ToListAsync() });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStaff(AdminStaffViewModel model)
    {
        var email = model.NewStaff.Email.Trim().ToLowerInvariant();
        if (await dbContext.Accounts.AnyAsync(account => account.Email.ToLower() == email))
        {
            ModelState.AddModelError("NewStaff.Email", "Email này đã được sử dụng.");
        }
        if (!ModelState.IsValid)
        {
            model.Staff = await dbContext.Accounts.Where(account => account.Role == AccountRole.Staff).OrderBy(account => account.FullName).ToListAsync();
            return View(nameof(Staff), model);
        }

        dbContext.Accounts.Add(new Account
        {
            FullName = model.NewStaff.FullName.Trim(),
            Email = email,
            Phone = model.NewStaff.Phone.Trim(),
            PasswordHash = PasswordHasher.Hash(model.NewStaff.Password),
            Role = AccountRole.Staff
        });
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Staff));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStaff(int id)
    {
        var staff = await dbContext.Accounts.FirstOrDefaultAsync(account => account.Id == id && account.Role == AccountRole.Staff);
        if (staff is not null)
        {
            dbContext.Accounts.Remove(staff);
            await dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Staff));
    }

    [HttpGet]
    public async Task<IActionResult> Payments()
    {
        var payments = await dbContext.Payments.Include(payment => payment.Booking).ThenInclude(booking => booking.Customer)
            .Include(payment => payment.Booking).ThenInclude(booking => booking.Room)
            .OrderByDescending(payment => payment.Booking.CreatedAt).ToListAsync();
        return View(payments);
    }

    [HttpGet]
    public async Task<IActionResult> Reviews()
    {
        var reviews = await dbContext.Reviews.Include(review => review.Customer).Include(review => review.Booking)
            .ThenInclude(booking => booking.Room).OrderByDescending(review => review.CreatedAt).ToListAsync();
        return View(reviews);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await dbContext.Reviews.FindAsync(id);
        if (review is not null)
        {
            dbContext.Reviews.Remove(review);
            await dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Reviews));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddService(Service service)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Services));
        dbContext.Services.Add(service);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Services));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteService(int id)
    {
        var service = await dbContext.Services.FindAsync(id);
        if (service is null) return NotFound();
        var isUsed = await dbContext.BookingServices.AnyAsync(item => item.ServiceId == id);
        if (!isUsed)
        {
            dbContext.Services.Remove(service);
            await dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Services));
    }

    public async Task<IActionResult> Customers() => View(await dbContext.Customers.OrderBy(customer => customer.FullName).ToListAsync());

    public async Task<IActionResult> Reports()
    {
        var model = new AdminReportViewModel
        {
            ConfirmedRevenue = await dbContext.Payments.Where(payment => payment.Status == PaymentStatus.Paid).SumAsync(payment => (decimal?)payment.Amount) ?? 0,
            CompletedBookings = await dbContext.Bookings.CountAsync(booking => booking.Status == BookingStatus.Completed),
            TotalCustomers = await dbContext.Customers.CountAsync(),
            RoomOccupancy = await dbContext.Rooms.Select(room => new RoomOccupancyItem { RoomName = room.Name, BookingCount = room.Bookings.Count(booking => booking.Status != BookingStatus.Cancelled) }).OrderByDescending(item => item.BookingCount).ToListAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRoom(AdminRoomsViewModel model)
    {
        if (!ModelState.IsValid) return View(nameof(Rooms), new AdminRoomsViewModel { NewRoom = model.NewRoom, Rooms = await dbContext.Rooms.ToListAsync() });
        dbContext.Rooms.Add(model.NewRoom);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Rooms));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetBookingStatus(int id, BookingStatus status)
    {
        var booking = await dbContext.Bookings.Include(item => item.Payment).FirstOrDefaultAsync(item => item.Id == id);
        if (booking is not null && IsAllowedTransition(booking.Status, status))
        {
            booking.Status = status;
            if (status == BookingStatus.Cancelled && booking.Payment?.Status == PaymentStatus.Pending)
            {
                booking.Payment.Status = PaymentStatus.Cancelled;
            }
            else if (status == BookingStatus.Cancelled && booking.Payment?.Status == PaymentStatus.Paid)
            {
                TempData["BookingCancellationNotice"] = $"Booking #{id} đã hủy nhưng thanh toán đã thu; cần xử lý hoàn tiền riêng.";
            }
            await dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private static bool IsAllowedTransition(BookingStatus current, BookingStatus next) =>
        (current, next) switch
        {
            (BookingStatus.Pending, BookingStatus.Confirmed) => true,
            (BookingStatus.Pending, BookingStatus.Cancelled) => true,
            (BookingStatus.Confirmed, BookingStatus.CheckedIn) => true,
            (BookingStatus.Confirmed, BookingStatus.Cancelled) => true,
            (BookingStatus.CheckedIn, BookingStatus.Completed) => true,
            _ => false
        };

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPaymentStatus(int bookingId, PaymentStatus status, string? transactionCode)
    {
        var payment = await dbContext.Payments.Include(item => item.Booking).FirstOrDefaultAsync(item => item.BookingId == bookingId);
        var hasUnpaidBalance = payment is not null && payment.Booking.TotalAmount > payment.Amount;
        var validTransition = payment is not null && ((payment.Status, status) switch
        {
            (PaymentStatus.Pending, PaymentStatus.Paid) => true,
            (PaymentStatus.Pending, PaymentStatus.Failed) => true,
            (PaymentStatus.Failed, PaymentStatus.Pending) => true,
            (PaymentStatus.Paid, PaymentStatus.Refunded) => true,
            (PaymentStatus.Paid, PaymentStatus.Paid) => hasUnpaidBalance,
            _ => false
        });
        if (payment is not null && validTransition)
        {
            payment.Status = status;
            payment.PaidAt = status == PaymentStatus.Paid ? DateTime.UtcNow : null;
            if (status == PaymentStatus.Paid)
            {
                payment.Amount = payment.Booking.TotalAmount;
                var services = await dbContext.BookingServices.Where(item => item.BookingId == bookingId && !item.IsPaid).ToListAsync();
                foreach (var service in services) service.IsPaid = true;
            }
            if (!string.IsNullOrWhiteSpace(transactionCode)) payment.TransactionCode = transactionCode.Trim();
            await dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Payments));
    }
}