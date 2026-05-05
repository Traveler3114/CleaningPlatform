using Microsoft.EntityFrameworkCore;
using VechileCleaningAPI.Data;
using VechileCleaningAPI.Managers;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:5098", "https://*:7124");

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AvailabilityManager>();
builder.Services.AddScoped<BookingManager>();
builder.Services.AddScoped<ScheduleManager>();
builder.Services.AddScoped<OverrideManager>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRouting();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapGet("/admin", () => Results.File("admin/index.html", "text/html"));
app.MapFallbackToFile("index.html");

// Ensure DB created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();