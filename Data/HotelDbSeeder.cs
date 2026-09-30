using HotelBooking.Web.Models;
using HotelBooking.Web.Security;

namespace HotelBooking.Web.Data;

public static class HotelDbSeeder
{
    public static void Seed(HotelDbContext db)
    {
        if (db.Rooms.Any())
        {
            return;
        }

        db.Accounts.Add(new Account { FullName = "Quản trị viên", Email = "admin@moc-hotel.vn", PasswordHash = PasswordHasher.Hash("Admin@123"), Role = AccountRole.Admin });

        db.Rooms.AddRange(
            new Room { Code = "M101", Name = "Phòng Ban Mai", Type = "Deluxe", Capacity = 2, PricePerNight = 1250000, Description = "Không gian yên tĩnh, cửa sổ lớn nhìn ra vườn.", Status = RoomStatus.Available },
            new Room { Code = "M201", Name = "Phòng Mây Trắng", Type = "Suite", Capacity = 3, PricePerNight = 1850000, Description = "Phòng rộng với khu vực tiếp khách và ban công.", Status = RoomStatus.Available },
            new Room { Code = "M301", Name = "Phòng Gió Biển", Type = "Family", Capacity = 4, PricePerNight = 2400000, Description = "Hai giường lớn, phù hợp cho gia đình và nhóm bạn.", Status = RoomStatus.Available },
            new Room { Code = "M102", Name = "Phòng An Nhiên", Type = "Standard", Capacity = 2, PricePerNight = 850000, Description = "Thiết kế tối giản, đầy đủ tiện nghi cần thiết.", Status = RoomStatus.Available });

        db.Services.AddRange(
            new Service { Name = "Bữa sáng", Price = 150000, Description = "Bữa sáng địa phương cho một người." },
            new Service { Name = "Đón sân bay", Price = 350000, Description = "Xe riêng đón khách tại sân bay." },
            new Service { Name = "Giặt ủi", Price = 100000, Description = "Dịch vụ giặt ủi trong ngày." });

        db.SaveChanges();
    }
}