/* Royal Hotel - SQL Server database setup */
USE master;
GO

IF DB_ID(N'HotelBookingDb') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE [HotelBookingDb]');
END;
GO

USE HotelBookingDb;
GO

IF OBJECT_ID(N'dbo.Accounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Accounts
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Accounts PRIMARY KEY,
        FullName nvarchar(120) NOT NULL,
        Email nvarchar(180) NOT NULL,
        PasswordHash nvarchar(128) NOT NULL,
        Phone nvarchar(30) NOT NULL CONSTRAINT DF_Accounts_Phone DEFAULT N'',
        Role int NOT NULL CONSTRAINT DF_Accounts_Role DEFAULT 0,
        CONSTRAINT UQ_Accounts_Email UNIQUE (Email)
    );
END;
GO

IF OBJECT_ID(N'dbo.Rooms', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Rooms
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Rooms PRIMARY KEY,
        Code nvarchar(20) NOT NULL,
        Name nvarchar(120) NOT NULL,
        Type nvarchar(50) NOT NULL,
        Capacity int NOT NULL,
        PricePerNight decimal(18,2) NOT NULL,
        Description nvarchar(1000) NOT NULL CONSTRAINT DF_Rooms_Description DEFAULT N'',
        Status int NOT NULL CONSTRAINT DF_Rooms_Status DEFAULT 0,
        CONSTRAINT UQ_Rooms_Code UNIQUE (Code)
    );
END;
GO

IF OBJECT_ID(N'dbo.Customers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customers
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
        FullName nvarchar(120) NOT NULL,
        Email nvarchar(180) NOT NULL,
        Phone nvarchar(30) NOT NULL CONSTRAINT DF_Customers_Phone DEFAULT N'',
        Address nvarchar(250) NOT NULL CONSTRAINT DF_Customers_Address DEFAULT N''
    );
END;
GO

IF OBJECT_ID(N'dbo.Services', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Services
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Services PRIMARY KEY,
        Name nvarchar(120) NOT NULL,
        Price decimal(18,2) NOT NULL,
        Description nvarchar(500) NOT NULL CONSTRAINT DF_Services_Description DEFAULT N''
    );
END;
GO

IF OBJECT_ID(N'dbo.Bookings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bookings
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Bookings PRIMARY KEY,
        RoomId int NOT NULL,
        CustomerId int NOT NULL,
        CheckIn datetime2 NOT NULL,
        CheckOut datetime2 NOT NULL,
        TotalAmount decimal(18,2) NOT NULL,
        Status int NOT NULL CONSTRAINT DF_Bookings_Status DEFAULT 0,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_Bookings_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Bookings_Rooms_RoomId FOREIGN KEY (RoomId) REFERENCES dbo.Rooms (Id),
        CONSTRAINT FK_Bookings_Customers_CustomerId FOREIGN KEY (CustomerId) REFERENCES dbo.Customers (Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.Payments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Payments PRIMARY KEY,
        BookingId int NOT NULL,
        Amount decimal(18,2) NOT NULL,
        Method nvarchar(40) NOT NULL CONSTRAINT DF_Payments_Method DEFAULT N'Cash',
        Status int NOT NULL CONSTRAINT DF_Payments_Status DEFAULT 0,
        TransactionCode nvarchar(100) NOT NULL CONSTRAINT DF_Payments_TransactionCode DEFAULT N'',
        PaidAt datetime2 NULL,
        CONSTRAINT UQ_Payments_BookingId UNIQUE (BookingId),
        CONSTRAINT FK_Payments_Bookings_BookingId FOREIGN KEY (BookingId) REFERENCES dbo.Bookings (Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.BookingServices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BookingServices
    (
        BookingId int NOT NULL,
        ServiceId int NOT NULL,
        Quantity int NOT NULL CONSTRAINT DF_BookingServices_Quantity DEFAULT 1,
        UnitPrice decimal(18,2) NOT NULL,
        IsPaid bit NOT NULL CONSTRAINT DF_BookingServices_IsPaid DEFAULT 0,
        CONSTRAINT PK_BookingServices PRIMARY KEY (BookingId, ServiceId),
        CONSTRAINT FK_BookingServices_Bookings_BookingId FOREIGN KEY (BookingId) REFERENCES dbo.Bookings (Id),
        CONSTRAINT FK_BookingServices_Services_ServiceId FOREIGN KEY (ServiceId) REFERENCES dbo.Services (Id)
    );
END;
GO

IF COL_LENGTH(N'dbo.BookingServices', N'IsPaid') IS NULL
BEGIN
    ALTER TABLE dbo.BookingServices
        ADD IsPaid bit NOT NULL CONSTRAINT DF_BookingServices_IsPaid DEFAULT 0;

    UPDATE bookingService
    SET IsPaid = 1
    FROM dbo.BookingServices AS bookingService
    INNER JOIN dbo.Bookings AS booking ON booking.Id = bookingService.BookingId
    INNER JOIN dbo.Payments AS payment ON payment.BookingId = booking.Id
    WHERE payment.Status = 1 AND payment.Amount >= booking.TotalAmount;
END;
GO

IF OBJECT_ID(N'dbo.Reviews', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reviews
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Reviews PRIMARY KEY,
        BookingId int NOT NULL,
        CustomerId int NOT NULL,
        Rating int NOT NULL,
        Comment nvarchar(1000) NOT NULL,
        CreatedAt datetime2 NOT NULL CONSTRAINT DF_Reviews_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Reviews_BookingId UNIQUE (BookingId),
        CONSTRAINT FK_Reviews_Bookings_BookingId FOREIGN KEY (BookingId) REFERENCES dbo.Bookings (Id),
        CONSTRAINT FK_Reviews_Customers_CustomerId FOREIGN KEY (CustomerId) REFERENCES dbo.Customers (Id)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Accounts WHERE Email = N'adminlong@gmail.com')
BEGIN
    INSERT INTO dbo.Accounts (FullName, Email, PasswordHash, Phone, Role)
    VALUES (N'Quản trị viên', N'adminlong@gmail.com', CONVERT(varchar(128), HASHBYTES('SHA2_256', N'hoanglong'), 2), N'', 2);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Rooms)
BEGIN
    INSERT INTO dbo.Rooms (Code, Name, Type, Capacity, PricePerNight, Description, Status)
    VALUES
        (N'M101', N'Phòng Ban Mai', N'Deluxe', 2, 1250000, N'Không gian yên tĩnh, cửa sổ lớn nhìn ra vườn.', 0),
        (N'M201', N'Phòng Mây Trắng', N'Suite', 3, 1850000, N'Phòng rộng với khu vực tiếp khách và ban công.', 0),
        (N'M301', N'Phòng Gió Biển', N'Family', 4, 2400000, N'Hai giường lớn, phù hợp cho gia đình và nhóm bạn.', 0),
        (N'M102', N'Phòng An Nhiên', N'Standard', 2, 850000, N'Thiết kế tối giản, đầy đủ tiện nghi cần thiết.', 0);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Services)
BEGIN
    INSERT INTO dbo.Services (Name, Price, Description)
    VALUES
        (N'Bữa sáng', 150000, N'Bữa sáng địa phương cho một người.'),
        (N'Đón sân bay', 350000, N'Xe riêng đón khách tại sân bay.'),
        (N'Giặt ủi', 100000, N'Dịch vụ giặt ủi trong ngày.');
END;
GO

SELECT name AS TableName FROM sys.tables WHERE name IN (N'Accounts', N'Rooms', N'Customers', N'Services', N'Bookings', N'Payments', N'BookingServices', N'Reviews') ORDER BY name;
GO