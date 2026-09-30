using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Controllers;

public class RoomsController(HotelDbContext dbContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int? year, int? month)
    {
        var today = DateTime.Today;
        var selectedYear = year is >= 1 and <= 9998 ? year.Value : today.Year;
        var selectedMonth = month is >= 1 and <= 12 ? month.Value : today.Month;
        var monthStart = new DateTime(selectedYear, selectedMonth, 1);
        var monthEnd = monthStart.AddMonths(1);
        var rooms = await dbContext.Rooms.AsNoTracking().OrderBy(room => room.Code).ToListAsync();
        var roomIds = rooms.Select(room => room.Id).ToList();
        var bookings = roomIds.Count == 0
            ? []
            : await dbContext.Bookings.AsNoTracking()
                .Where(booking => roomIds.Contains(booking.RoomId)
                    && booking.Status != BookingStatus.Cancelled
                    && booking.CheckIn < monthEnd
                    && booking.CheckOut > monthStart)
                .Select(booking => new { booking.RoomId, booking.CheckIn, booking.CheckOut })
                .ToListAsync();
        var reviews = roomIds.Count == 0
            ? []
            : await dbContext.Reviews.AsNoTracking()
                .Include(review => review.Customer)
                .Include(review => review.Booking)
                .Where(review => roomIds.Contains(review.Booking.RoomId))
                .OrderByDescending(review => review.CreatedAt)
                .ToListAsync();
        var days = Enumerable.Range(0, DateTime.DaysInMonth(selectedYear, selectedMonth))
            .Select(offset => monthStart.AddDays(offset)).ToList();
        var leadingBlankDays = ((int)monthStart.DayOfWeek + 6) % 7;

        var model = new RoomCalendarViewModel
        {
            Month = monthStart,
            Days = days,
            Rooms = rooms.Select(room =>
            {
                var roomDays = days.Select(day =>
                {
                    var bookingExists = bookings.Any(booking => booking.RoomId == room.Id
                        && booking.CheckIn.Date < day.AddDays(1)
                        && booking.CheckOut.Date > day);
                    var isMaintenance = room.Status == RoomStatus.Maintenance;
                    var isOccupied = room.Status == RoomStatus.Occupied;
                    var isBooked = !isMaintenance && (isOccupied || bookingExists);
                    return new RoomCalendarDayViewModel
                    {
                        Date = day,
                        Status = isMaintenance ? "Bảo trì" : isBooked ? "Đã đặt" : "Trống",
                        CssClass = isMaintenance ? "calendar-maintenance" : isBooked ? "calendar-booked" : "calendar-available",
                        Description = isMaintenance ? "Phòng đang bảo trì" : isBooked ? "Đã có lịch đặt" : "Chưa có lịch đặt"
                    };
                }).ToList();
                var calendarSlots = Enumerable.Repeat<RoomCalendarDayViewModel?>(null, leadingBlankDays)
                    .Concat(roomDays)
                    .ToList();
                while (calendarSlots.Count % 7 != 0) calendarSlots.Add(null);

                return new RoomCalendarEntryViewModel
                {
                    Room = room,
                    RecentReview = reviews.FirstOrDefault(review => review.Booking.RoomId == room.Id),
                    Days = roomDays,
                    CalendarSlots = calendarSlots
                };
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Search(RoomSearchViewModel search)
    {
        if (search.CheckOut <= search.CheckIn)
        {
            ModelState.AddModelError(nameof(search.CheckOut), "Ngày trả phòng phải sau ngày nhận phòng.");
            return View(new RoomSearchResultViewModel { Search = search });
        }

        var rooms = await dbContext.Rooms.AsNoTracking()
            .Where(room => room.Status == RoomStatus.Available && room.Capacity >= search.Guests)
            .Where(room => !room.Bookings.Any(booking =>
                booking.Status != BookingStatus.Cancelled &&
                booking.CheckIn < search.CheckOut && booking.CheckOut > search.CheckIn))
            .OrderBy(room => room.PricePerNight)
            .ToListAsync();

        var roomIds = rooms.Select(room => room.Id).ToList();
        var reviews = roomIds.Count == 0
            ? []
            : await dbContext.Reviews.AsNoTracking()
                .Include(review => review.Customer)
                .Include(review => review.Booking)
                .Where(review => roomIds.Contains(review.Booking.RoomId))
                .OrderByDescending(review => review.CreatedAt)
                .ToListAsync();

        return View(new RoomSearchResultViewModel
        {
            Search = search,
            Rooms = rooms.Select(room => new RoomSearchItemViewModel
            {
                Room = room,
                RecentReview = reviews.FirstOrDefault(review => review.Booking.RoomId == room.Id)
            }).ToList()
        });
    }

}