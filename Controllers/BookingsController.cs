using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Controllers;

public class BookingsController(HotelDbContext dbContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Mine()
    {
        var email = HttpContext.Session.GetString("AccountEmail");
        if (email is null) return RedirectToAction("Login", "Account", new { returnUrl = "/Bookings/Mine" });
        var bookings = await dbContext.Bookings.Include(booking => booking.Room).Include(booking => booking.Payment).Include(booking => booking.Reviews).Include(booking => booking.BookingServices).ThenInclude(item => item.Service).Where(booking => booking.Customer.Email == email).OrderByDescending(booking => booking.CreatedAt).ToListAsync();
        return View(bookings);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var email = HttpContext.Session.GetString("AccountEmail");
        if (email is null) return RedirectToAction("Login", "Account", new { returnUrl = $"/Bookings/Details/{id}" });

        var booking = await dbContext.Bookings
            .Include(item => item.Customer)
            .Include(item => item.Room)
            .Include(item => item.Payment)
            .Include(item => item.Reviews).ThenInclude(review => review.Customer)
            .Include(item => item.BookingServices).ThenInclude(item => item.Service)
            .FirstOrDefaultAsync(item => item.Id == id && item.Customer.Email == email);
        if (booking is null) return NotFound();

        var canAddServices = booking.Status is BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.CheckedIn;
        var selectedServiceIds = booking.BookingServices.Select(item => item.ServiceId).ToList();
        var services = canAddServices
            ? await dbContext.Services.OrderBy(service => service.Name).ToListAsync()
            : [];
        var paidAmount = booking.Payment?.Status == PaymentStatus.Paid ? booking.Payment.Amount : 0;

        return View(new BookingDetailsViewModel
        {
            Booking = booking,
            AvailableServices = services,
            PaidAmount = paidAmount,
            AmountDue = Math.Max(0, booking.TotalAmount - paidAmount),
            CanAddServices = canAddServices
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddServices(int id, List<int> serviceIds)
    {
        var email = HttpContext.Session.GetString("AccountEmail");
        if (email is null) return RedirectToAction("Login", "Account", new { returnUrl = $"/Bookings/Details/{id}" });

        var booking = await dbContext.Bookings
            .Include(item => item.Customer)
            .Include(item => item.Payment)
            .Include(item => item.BookingServices)
            .FirstOrDefaultAsync(item => item.Id == id && item.Customer.Email == email);
        if (booking is null) return NotFound();
        if (booking.Status is not (BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.CheckedIn)) return NotFound();
        var existingServiceIds = booking.BookingServices.Select(item => item.ServiceId).ToHashSet();
        var newServiceIds = serviceIds.Distinct().Where(serviceId => !existingServiceIds.Contains(serviceId)).ToList();
        if (newServiceIds.Count == 0)
        {
            TempData["BookingDetailsNotice"] = serviceIds.Count == 0
                ? "Hãy chọn ít nhất một dịch vụ chưa đăng ký."
                : "Các dịch vụ đã đăng ký không thể chỉnh sửa; hãy chọn dịch vụ mới.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var services = await dbContext.Services.Where(service => newServiceIds.Contains(service.Id)).ToListAsync();
        if (services.Count != newServiceIds.Count) return BadRequest("Danh sách dịch vụ không hợp lệ.");

        foreach (var service in services)
        {
            dbContext.BookingServices.Add(new BookingService { BookingId = booking.Id, ServiceId = service.Id, Quantity = 1, UnitPrice = service.Price, IsPaid = false });
            booking.TotalAmount += service.Price;
        }

        if (booking.Payment?.Status == PaymentStatus.Pending)
        {
            booking.Payment.Amount = booking.TotalAmount;
        }

        await dbContext.SaveChangesAsync();
        if (booking.Payment?.Method == "Card" && booking.Payment.Status is PaymentStatus.Pending or PaymentStatus.Paid)
        {
            HttpContext.Session.SetInt32("DemoCheckoutBookingId", booking.Id);
            HttpContext.Session.SetString("DemoCheckoutEmail", booking.Customer.Email);
            return RedirectToAction("DemoCheckout", "Payments", new { bookingId = booking.Id });
        }

        TempData["BookingDetailsNotice"] = "Đã thêm dịch vụ vào booking. Phụ thu dịch vụ thanh toán tại khách sạn.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelUnpaidService(int id, int serviceId)
    {
        var email = HttpContext.Session.GetString("AccountEmail");
        if (email is null)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = $"/Bookings/Details/{id}" });
        }

        var booking = await dbContext.Bookings
            .Include(item => item.Customer)
            .Include(item => item.Payment)
            .Include(item => item.BookingServices)
            .FirstOrDefaultAsync(item => item.Id == id && item.Customer.Email == email);
        if (booking is null) return NotFound();
        if (booking.Status is not (BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.CheckedIn)) return NotFound();

        var bookingService = booking.BookingServices.FirstOrDefault(item => item.ServiceId == serviceId && !item.IsPaid);
        if (bookingService is null)
        {
            TempData["BookingDetailsNotice"] = "Chỉ có thể hủy dịch vụ đang chờ thanh toán.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var serviceAmount = bookingService.UnitPrice * bookingService.Quantity;
        booking.TotalAmount = Math.Max(0, booking.TotalAmount - serviceAmount);
        if (booking.Payment?.Status == PaymentStatus.Pending)
        {
            booking.Payment.Amount = Math.Max(0, booking.Payment.Amount - serviceAmount);
        }
        dbContext.BookingServices.Remove(bookingService);
        await dbContext.SaveChangesAsync();

        TempData["BookingDetailsNotice"] = "Đã hủy dịch vụ chưa thanh toán và cập nhật lại số tiền booking.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var email = HttpContext.Session.GetString("AccountEmail");
        var booking = await dbContext.Bookings.Include(item => item.Customer).Include(item => item.Payment).FirstOrDefaultAsync(item => item.Id == id && item.Customer.Email == email);
        if (booking is not null && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed))
        {
            booking.Status = BookingStatus.Cancelled;
            if (booking.Payment is not null && booking.Payment.Status == PaymentStatus.Pending) booking.Payment.Status = PaymentStatus.Cancelled;
            await dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    public async Task<IActionResult> Create(int roomId, DateTime checkIn, DateTime checkOut)
    {
        var email = HttpContext.Session.GetString("AccountEmail");
        if (email is null)
        {
            var returnUrl = Url.Action(nameof(Create), new { roomId, checkIn = checkIn.ToString("O"), checkOut = checkOut.ToString("O") });
            return RedirectToAction("Register", "Account", new { returnUrl });
        }

        var account = await dbContext.Accounts.FirstOrDefaultAsync(item => item.Email == email);
        if (account is null)
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Register", "Account", new { returnUrl = Request.Path + Request.QueryString });
        }

        var room = await dbContext.Rooms.FindAsync(roomId);
        if (room is null) return NotFound();

        return View(new BookingCreateViewModel { RoomId = room.Id, RoomName = room.Name, PricePerNight = room.PricePerNight, FullName = account.FullName, Email = account.Email, Phone = account.Phone, CheckIn = checkIn, CheckOut = checkOut, Services = await dbContext.Services.OrderBy(service => service.Name).ToListAsync(), PaymentMethod = string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel model)
    {
        var email = HttpContext.Session.GetString("AccountEmail");
        if (email is null)
        {
            var returnUrl = Url.Action(nameof(Create), new { roomId = model.RoomId, checkIn = model.CheckIn.ToString("O"), checkOut = model.CheckOut.ToString("O") });
            return RedirectToAction("Register", "Account", new { returnUrl });
        }

        var account = await dbContext.Accounts.FirstOrDefaultAsync(item => item.Email == email);
        if (account is null)
        {
            HttpContext.Session.Clear();
            return Unauthorized();
        }
        model.FullName = account.FullName;
        model.Email = account.Email;
        model.Phone = account.Phone;

        var paymentMethod = model.PaymentMethod?.Trim();
        var room = await dbContext.Rooms.FindAsync(model.RoomId);
        if (room is null) return NotFound();

        if (model.CheckOut <= model.CheckIn)
        {
            ModelState.AddModelError(nameof(model.CheckOut), "Ngày trả phòng phải sau ngày nhận phòng.");
        }

        if (model.CheckIn.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.CheckIn), "Ngày nhận phòng không được ở trong quá khứ.");
        }

        if (room.Status != RoomStatus.Available)
        {
            ModelState.AddModelError(string.Empty, "Phòng hiện không sẵn sàng để đặt.");
        }

        if (paymentMethod is not ("Cash" or "Card"))
        {
            ModelState.AddModelError(nameof(model.PaymentMethod), "Phương thức thanh toán không hợp lệ.");
        }

        var isBusy = await dbContext.Bookings.AnyAsync(booking => booking.RoomId == model.RoomId && booking.Status != BookingStatus.Cancelled && booking.CheckIn < model.CheckOut && booking.CheckOut > model.CheckIn);
        if (isBusy) ModelState.AddModelError(string.Empty, "Phòng vừa được đặt trong thời gian này. Vui lòng chọn phòng khác.");
        var services = await dbContext.Services.Where(service => model.SelectedServiceIds.Contains(service.Id)).ToListAsync();
        if (!ModelState.IsValid)
        {
            model.RoomName = room.Name;
            model.PricePerNight = room.PricePerNight;
            model.Services = await dbContext.Services.OrderBy(service => service.Name).ToListAsync();
            return View(model);
        }

        var customer = await dbContext.Customers.FirstOrDefaultAsync(item => item.Email == account.Email);
        if (customer is null)
        {
            customer = new Customer { FullName = account.FullName, Email = account.Email, Phone = account.Phone };
            dbContext.Customers.Add(customer);
        }
        else
        {
            customer.FullName = account.FullName;
            customer.Phone = account.Phone;
        }
        var nights = (model.CheckOut - model.CheckIn).Days;
        var roomAmount = nights * room.PricePerNight;
        var serviceAmount = services.Sum(service => service.Price);
        var booking = new Booking { Room = room, Customer = customer, CheckIn = model.CheckIn, CheckOut = model.CheckOut, TotalAmount = roomAmount + serviceAmount, Status = BookingStatus.Pending };
        booking.Payment = new Payment { Amount = booking.TotalAmount, Method = paymentMethod!, Status = PaymentStatus.Pending };
        booking.BookingServices = services.Select(service => new BookingService { Service = service, Quantity = 1, UnitPrice = service.Price }).ToList();
        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();
        if (paymentMethod == "Card")
        {
            HttpContext.Session.SetInt32("DemoCheckoutBookingId", booking.Id);
            HttpContext.Session.SetString("DemoCheckoutEmail", customer.Email);
            return RedirectToAction("DemoCheckout", "Payments", new { bookingId = booking.Id });
        }
        return RedirectToAction(nameof(Success), new { id = booking.Id });
    }

    public async Task<IActionResult> Success(int id)
    {
        var booking = await dbContext.Bookings.Include(item => item.Customer).Include(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (booking is null) return NotFound();
        if (booking.Payment?.Method == "Card" && booking.Payment.Status == PaymentStatus.Pending)
        {
            HttpContext.Session.SetInt32("DemoCheckoutBookingId", booking.Id);
            HttpContext.Session.SetString("DemoCheckoutEmail", booking.Customer.Email);
            return RedirectToAction("DemoCheckout", "Payments", new { bookingId = booking.Id });
        }
        return View(id);
    }
}