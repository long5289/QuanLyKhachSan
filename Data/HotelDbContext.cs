using HotelBooking.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Web.Data;

public class HotelDbContext(DbContextOptions<HotelDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<BookingService> BookingServices => Set<BookingService>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>().Property(room => room.PricePerNight).HasPrecision(18, 2);
        modelBuilder.Entity<Service>().Property(service => service.Price).HasPrecision(18, 2);
        modelBuilder.Entity<Booking>().Property(booking => booking.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<Payment>().Property(payment => payment.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<BookingService>().Property(item => item.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<BookingService>().HasKey(item => new { item.BookingId, item.ServiceId });
        modelBuilder.Entity<Payment>().HasOne(payment => payment.Booking).WithOne(booking => booking.Payment).HasForeignKey<Payment>(payment => payment.BookingId);
        modelBuilder.Entity<BookingService>().HasOne(item => item.Booking).WithMany(booking => booking.BookingServices).HasForeignKey(item => item.BookingId);
        modelBuilder.Entity<BookingService>().HasOne(item => item.Service).WithMany(service => service.BookingServices).HasForeignKey(item => item.ServiceId);
        modelBuilder.Entity<Review>().HasOne(review => review.Booking).WithMany(booking => booking.Reviews).HasForeignKey(review => review.BookingId);
        modelBuilder.Entity<Review>().HasOne(review => review.Customer).WithMany(customer => customer.Reviews).HasForeignKey(review => review.CustomerId);
        modelBuilder.Entity<Booking>().HasOne(booking => booking.Room).WithMany(room => room.Bookings).HasForeignKey(booking => booking.RoomId);
        modelBuilder.Entity<Booking>().HasOne(booking => booking.Customer).WithMany(customer => customer.Bookings).HasForeignKey(booking => booking.CustomerId);
    }
}