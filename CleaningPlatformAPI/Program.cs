using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;
using CleaningPlatformAPI.Authorization;
using CleaningPlatformAPI.Middleware;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Managers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AvailabilityManager>();
builder.Services.AddScoped<BookingManager>();
builder.Services.AddScoped<ScheduleManager>();
builder.Services.AddScoped<DateOverrideManager>();
builder.Services.AddScoped<AuthManager>();
builder.Services.AddScoped<TokenManager>();
builder.Services.AddScoped<EmployeeManager>();
builder.Services.AddScoped<RoleManager>();
builder.Services.AddScoped<ServiceCatalogManager>();
builder.Services.AddScoped<ClientManager>();
builder.Services.AddScoped<InvoiceManager>();
builder.Services.AddScoped<ReportingManager>();
builder.Services.AddScoped<KanbanManager>();
builder.Services.AddScoped<SopManager>();

builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MultiScheme";
    options.DefaultAuthenticateScheme = "MultiScheme";
    options.DefaultChallengeScheme = "MultiScheme";
})
.AddPolicyScheme("MultiScheme", "MultiScheme", options =>
{
    options.ForwardDefaultSelector = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
            return JwtBearerDefaults.AuthenticationScheme;
        return CookieAuthenticationDefaults.AuthenticationScheme;
    };
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Admin/Login";
    options.AccessDeniedPath = "/Admin/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    foreach (var key in PermissionKeys.All)
        options.AddPolicy(key, policy => policy.AddRequirements(new PermissionRequirement(key)));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        string message = "An unexpected error occurred.";
        if (error?.Error is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            message = "A record with this value already exists.";
        }
        else if (error?.Error is SqlException fkEx && fkEx.Number == 547)
        {
            message = "This record cannot be deleted because it is referenced by other data. Remove the related records first.";
        }
        else if (error?.Error is Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            message = app.Environment.IsDevelopment()
                ? $"Unexpected error ({ex.GetType().Name}): {ex.Message}"
                : "An unexpected error occurred.";
        }

        await context.Response.WriteAsJsonAsync(OperationResult<string>.Fail(message));
    });
});
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<SecurityStampValidator>();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapRazorPages();
app.MapGet("/admin", () => Results.Redirect("/Admin/Index"));
app.MapFallbackToFile("index.html");

// Ensure DB created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (app.Environment.IsDevelopment())
    {
        db.Database.EnsureCreated();
    }
}

app.Run();
