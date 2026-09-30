using HotelBooking.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => options.Cookie.HttpOnly = true);
var useSqlServer = builder.Configuration.GetValue<bool>("UseSqlServer");
builder.Services.AddDbContext<HotelDbContext>(options =>
{
    if (useSqlServer)
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("HotelDatabase"));
    }
    else
    {
        options.UseInMemoryDatabase("HotelBookingDemo");
    }
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    HotelDbSeeder.Seed(scope.ServiceProvider.GetRequiredService<HotelDbContext>());
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
