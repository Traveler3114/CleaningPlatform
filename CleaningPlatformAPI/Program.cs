using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Globalization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Common;
using CleaningPlatformAPI.Middleware;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Managers;
using CleaningPlatformAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// Razor Pages removed - using static HTML instead
// builder.Services.AddRazorPages();

builder.Services.AddLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = new[] { new CultureInfo("en") };
    options.SupportedUICultures = new[] { new CultureInfo("en") };
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register all managers
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
builder.Services.AddScoped<RecurringScheduleManager>();
builder.Services.AddScoped<PortalDataManager>();
builder.Services.AddScoped<BookingRequestManager>();
builder.Services.AddScoped<InventoryManager>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddHostedService<RecurringJobService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// JWT only authentication - no cookies
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnForbidden = context =>
            {
                var localizer = context.Request.HttpContext.RequestServices
                    .GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizer<SharedResources>>();
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/problem+json";
                var pd = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/403",
                    Title = "Forbidden",
                    Status = 403,
                    Detail = localizer["error_access_denied"],
                    Extensions = { ["code"] = "ACCESS_DENIED" }
                };
                return context.Response.WriteAsJsonAsync(pd, pd.GetType());
            },
            OnChallenge = context =>
            {
                var localizer = context.Request.HttpContext.RequestServices
                    .GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizer<SharedResources>>();
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/problem+json";
                var pd = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/401",
                    Title = "Unauthorized",
                    Status = 401,
                    Detail = localizer["error_authentication_required"],
                    Extensions = { ["code"] = "AUTHENTICATION_REQUIRED" }
                };
                return context.Response.WriteAsJsonAsync(pd, pd.GetType());
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var key in PermissionKeys.All)
        options.AddPolicy(key, policy => policy.RequireAssertion(ctx =>
            ctx.User.FindFirst(ClaimTypes.Role)?.Value == RoleNames.Owner ||
            ctx.User.HasClaim("permission", key)));

    options.AddPolicy("PortalOnly", policy =>
        policy.RequireAssertion(ctx => ctx.User.HasClaim("auth_type", "portal")));
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
app.UseRequestLocalization();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var localizer = context.RequestServices.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizer<SharedResources>>();
        var error = context.Features.Get<IExceptionHandlerFeature>();
        var isDev = app.Environment.IsDevelopment();

        if (error is not null)
        {
            logger.LogError(error.Error, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = 500;

        string message;
        string code;

        if (error?.Error is AppException appEx)
        {
            context.Response.StatusCode = appEx.StatusCode;
            message = appEx.Message;
            code = appEx.Code;
        }
        else if (error?.Error is SqlException sqlEx)
        {
            message = SqlHelper.GetUserFriendlyMessage(sqlEx);
            code = "DB_ERROR";
        }
        else if (error?.Error is DbUpdateException dbEx)
        {
            message = dbEx.InnerException is SqlException innerSql
                ? SqlHelper.GetUserFriendlyMessage(innerSql)
                : isDev
                    ? $"Database update failed: {dbEx.InnerException?.Message ?? dbEx.Message}"
                    : localizer["error_db_error"];
            code = "DB_UPDATE_ERROR";
        }
        else if (error?.Error is InvalidOperationException invEx)
        {
            message = isDev
                ? $"Invalid operation: {invEx.Message}"
                : localizer["error_invalid_operation"];
            code = "INVALID_OPERATION";
        }
        else if (error?.Error is UnauthorizedAccessException)
        {
            context.Response.StatusCode = 403;
            message = localizer["error_no_permission"];
            code = "ACCESS_DENIED";
        }
        else
        {
            message = isDev && error?.Error is not null
                ? $"Unexpected error ({error.Error.GetType().Name}): {error.Error.Message}"
                : localizer["error_unexpected"];
            code = "UNEXPECTED_ERROR";
        }

        context.Response.ContentType = "application/problem+json";
        var pd = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{context.Response.StatusCode}",
            Title = context.Response.StatusCode switch
            {
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                409 => "Conflict",
                422 => "Unprocessable Entity",
                _ => "Internal Server Error"
            },
            Status = context.Response.StatusCode,
            Detail = message,
            Extensions = { ["code"] = code }
        };
        await context.Response.WriteAsJsonAsync(pd, pd.GetType());
    });
});

app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<SecurityStampValidationMiddleware>();
app.UseAuthorization();

// Static files configuration for Option 1 (customer/ and admin/ folders)
app.UseStaticFiles(); // Serves from wwwroot root

// Root URL redirects
app.MapGet("/", () => Results.Redirect("/public/index.html"));
app.MapGet("/admin", () => Results.Redirect("/admin/index.html"));

// API controllers
app.MapControllers();

// Fallback - API routes return 404 JSON, other routes redirect to public index
app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        var localizer = context.RequestServices.GetRequiredService<Microsoft.Extensions.Localization.IStringLocalizer<SharedResources>>();
        context.Response.StatusCode = 404;
        context.Response.ContentType = "application/problem+json";
        var pd = new ProblemDetails
        {
            Type = "https://httpstatuses.com/404",
            Title = "Not Found",
            Status = 404,
            Detail = localizer["error_endpoint_not_found"],
            Extensions = { ["code"] = "ENDPOINT_NOT_FOUND" }
        };
        await context.Response.WriteAsJsonAsync(pd, pd.GetType());
        return;
    }
    context.Response.Redirect("/public/index.html");
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (app.Environment.IsDevelopment())
    {
        db.Database.EnsureCreated();
    }
}

app.Run();
