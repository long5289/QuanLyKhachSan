# Royal Hotel

Website quản lý đặt phòng khách sạn online bằng ASP.NET Core MVC, Entity Framework Core và SQL Server. Hiện ứng dụng mặc định chạy bằng InMemory database để hoàn thiện giao diện và nghiệp vụ trước.

## Chạy dự án

```powershell
dotnet restore
dotnet run
```

Project dùng `UseAppHost=false` để tránh lỗi file `HotelBooking.Web.exe` bị khóa khi còn một phiên chạy cũ. Nếu VS Code vẫn giữ phiên cũ, dừng phiên đó bằng `Ctrl+C` rồi chạy lại.

Chuỗi kết nối mặc định nằm trong `appsettings.json`:

```text
Server=localhost;Database=HotelBookingDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Nếu SQL Server dùng instance khác, sửa giá trị `HotelDatabase` trước khi chạy.

Khi muốn chuyển sang SQL Server, đổi `UseSqlServer` thành `true` trong `appsettings.json`. Khi đó ứng dụng sẽ dùng chuỗi `HotelDatabase` thay vì database demo.

Tài khoản quản trị demo: `admin@moc-hotel.vn` / `Admin@123`.

## Tạo database

Bạn có thể mở file `Database/CreateHotelBookingDb.sql` bằng SSMS 21, chọn đúng SQL Server instance rồi bấm **Execute**. Script sẽ tạo database, bảng, khóa ngoại và dữ liệu mẫu an toàn nếu chạy lại.

Nếu muốn dùng EF Core migration thay cho script SSMS, cài công cụ EF Core:

```powershell
dotnet tool install --global dotnet-ef
```

Sau đó chạy tại thư mục dự án:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Chỉ chọn **một** cách tạo database: script SSMS hoặc EF migration. Sau khi tạo database thành công, đổi `UseSqlServer` thành `true` trong `appsettings.json` rồi chạy lại ứng dụng.

## Phạm vi hiện tại

- Trang chủ với form tìm phòng theo ngày và số khách.
- Mô hình `Room`, `Customer`, `Booking` và `Service`.
- Tìm các phòng còn trống, đúng sức chứa và không trùng lịch đặt.
- Trạng thái phòng và trạng thái đặt phòng làm nền tảng cho phần quản trị tiếp theo.
- Form đặt phòng, kiểm tra ngày hợp lệ và chống trùng lịch.
- Khu vực quản trị với thống kê, xác nhận đặt phòng và thêm phòng.