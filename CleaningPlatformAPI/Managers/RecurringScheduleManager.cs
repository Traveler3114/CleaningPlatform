using Microsoft.EntityFrameworkCore;
using CleaningPlatformAPI.Data;
using CleaningPlatformAPI.Contracts;
using CleaningPlatformAPI.Enums;
using CleaningPlatformAPI.Entities;
using Microsoft.Extensions.Localization;
using CleaningPlatformAPI;
using CleaningPlatformAPI.Mapping;
using CleaningPlatformAPI.Common;

namespace CleaningPlatformAPI.Managers;

public class RecurringScheduleManager
{
    private readonly AppDbContext _db;
    private readonly SopManager _sopManager;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly ILogger<RecurringScheduleManager> _logger;

    public RecurringScheduleManager(AppDbContext db, SopManager sopManager, ILogger<RecurringScheduleManager> logger, IStringLocalizer<SharedResources> localizer) { _db = db; _sopManager = sopManager; _logger = logger; 
            _localizer = localizer;}

    public async Task<List<RecurringScheduleResponse>> GetAllAsync(int? clientId = null, CancellationToken ct = default)
    {
        var query = _db.RecurringSchedules
            .Include(rs => rs.SourceBooking).ThenInclude(b => b.Client)
            .Include(rs => rs.SourceBooking).ThenInclude(b => b.Site)
            .AsQueryable();

        if (clientId.HasValue)
            query = query.Where(rs => rs.SourceBooking.ClientId == clientId.Value);

        var schedules = await query.OrderByDescending(rs => rs.CreatedAt).ToListAsync(ct);

        List<RecurringScheduleResponse> result = [];
        foreach (var s in schedules)
        {
            var upcomingCount = await _db.Bookings
                .CountAsync(b => b.RecurringScheduleId == s.Id
                    && b.Status == BookingStatus.Pending
                    && b.ScheduledDate >= DateTime.UtcNow.Date, ct);

            result.Add(new RecurringScheduleResponse
            {
                Id = s.Id,
                SourceBookingId = s.SourceBookingId,
                Frequency = s.Frequency,
                DayOfWeek = s.DayOfWeek,
                DayOfMonth = s.DayOfMonth,
                AutoGenerateWeeksAhead = s.AutoGenerateWeeksAhead,
                IsActive = s.IsActive,
                EndsOn = s.EndsOn,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                UpcomingCount = upcomingCount,
                ClientId = s.SourceBooking.ClientId,
                ClientName = s.SourceBooking.Client?.ClientName ?? string.Empty,
                SiteName = s.SourceBooking.Site?.SiteName
            });
        }

        return result;
    }

    public async Task<RecurringScheduleResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var s = await _db.RecurringSchedules
            .Include(rs => rs.SourceBooking).ThenInclude(b => b.Client)
            .Include(rs => rs.SourceBooking).ThenInclude(b => b.Site)
            .FirstOrDefaultAsync(rs => rs.Id == id, ct);

        if (s is null)
            throw new AppException("RECURRING_NOT_FOUND", $"Recurring schedule #{id} was not found.", 404);

        var upcomingCount = await _db.Bookings
            .CountAsync(b => b.RecurringScheduleId == s.Id
                && b.Status == BookingStatus.Pending
                && b.ScheduledDate >= DateTime.UtcNow.Date, ct);

        return new RecurringScheduleResponse
        {
            Id = s.Id,
            SourceBookingId = s.SourceBookingId,
            Frequency = s.Frequency,
            DayOfWeek = s.DayOfWeek,
            DayOfMonth = s.DayOfMonth,
            AutoGenerateWeeksAhead = s.AutoGenerateWeeksAhead,
            IsActive = s.IsActive,
            EndsOn = s.EndsOn,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            UpcomingCount = upcomingCount,
            ClientId = s.SourceBooking.ClientId,
            ClientName = s.SourceBooking.Client?.ClientName ?? string.Empty,
            SiteName = s.SourceBooking.Site?.SiteName
        };
    }

    public async Task<RecurringScheduleResponse> CreateFromBookingAsync(int bookingId, CreateRecurringScheduleRequest dto, CancellationToken ct = default)
    {
        var validation = ValidateFrequency(dto.Frequency, dto.DayOfWeek, dto.DayOfMonth, dto.AutoGenerateWeeksAhead);
        if (validation is not null)
            throw new AppException("RECURRING_VALIDATION", validation, 422);

        var booking = await _db.Bookings
            .Include(b => b.BookingServices)
            .FirstOrDefaultAsync(b => b.Id == bookingId, ct);

        if (booking is null)
            throw new AppException("BOOKING_NOT_FOUND", $"Booking #{bookingId} was not found.", 404);

        if (booking.Status != BookingStatus.Completed)
            throw new AppException("BOOKING_NOT_COMPLETED_RECURRING",
                $"Booking #{bookingId} has status '{booking.Status}'. Only Completed bookings can be used as a recurring source.", 422);

        var now = DateTime.UtcNow;
        var schedule = new RecurringSchedule
        {
            SourceBookingId = bookingId,
            Frequency = dto.Frequency,
            DayOfWeek = dto.DayOfWeek,
            DayOfMonth = dto.DayOfMonth,
            AutoGenerateWeeksAhead = dto.AutoGenerateWeeksAhead,
            EndsOn = dto.EndsOn,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.RecurringSchedules.Add(schedule);
        await _db.SaveChangesAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var lookaheadEnd = today.AddDays(dto.AutoGenerateWeeksAhead * 7);
        await GenerateForScheduleAsync(schedule.Id, today, lookaheadEnd, ct);

        return await GetByIdAsync(schedule.Id, ct);
    }

    public async Task<RecurringScheduleResponse> UpdateAsync(int id, UpdateRecurringScheduleRequest dto, CancellationToken ct = default)
    {
        var validation = ValidateFrequency(dto.Frequency, dto.DayOfWeek, dto.DayOfMonth, dto.AutoGenerateWeeksAhead);
        if (validation is not null)
            throw new AppException("RECURRING_VALIDATION", validation, 422);

        var schedule = await _db.RecurringSchedules
            .Include(rs => rs.SourceBooking).ThenInclude(b => b.Client)
            .Include(rs => rs.SourceBooking).ThenInclude(b => b.Site)
            .FirstOrDefaultAsync(rs => rs.Id == id, ct);

        if (schedule is null)
            throw new AppException("RECURRING_NOT_FOUND", $"Recurring schedule #{id} was not found.", 404);

        schedule.Frequency = dto.Frequency;
        schedule.DayOfWeek = dto.DayOfWeek;
        schedule.DayOfMonth = dto.DayOfMonth;
        schedule.AutoGenerateWeeksAhead = dto.AutoGenerateWeeksAhead;
        schedule.EndsOn = dto.EndsOn;
        schedule.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var now = DateTime.UtcNow;
        var futurePending = await _db.Bookings
            .Where(b => b.RecurringScheduleId == id
                && b.Status == BookingStatus.Pending
                && b.ScheduledDate > now.Date)
            .ToListAsync(ct);

        foreach (var booking in futurePending)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(ct);

        var today = DateOnly.FromDateTime(now);
        var lookaheadEnd = today.AddDays(dto.AutoGenerateWeeksAhead * 7);
        await GenerateForScheduleAsync(id, today, lookaheadEnd, ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<RecurringScheduleResponse> EndSeriesAsync(int id, EndSeriesRequest dto, CancellationToken ct = default)
    {
        var schedule = await _db.RecurringSchedules.FindAsync([id], ct);
        if (schedule is null)
            throw new AppException("RECURRING_NOT_FOUND", $"Recurring schedule #{id} was not found.", 404);

        schedule.IsActive = false;
        schedule.EndsOn = dto.EndsOn;
        schedule.UpdatedAt = DateTime.UtcNow;

        var endDate = dto.EndsOn.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var futurePending = await _db.Bookings
            .Where(b => b.RecurringScheduleId == id
                && b.ScheduledDate >= endDate
                && b.Status == BookingStatus.Pending)
            .ToListAsync(ct);

        foreach (var booking in futurePending)
            booking.Status = BookingStatus.Cancelled;

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<List<GenerateResult>> RunAutoGenerateAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var schedules = await _db.RecurringSchedules
            .Where(rs => rs.IsActive && (rs.EndsOn == null || rs.EndsOn > today))
            .ToListAsync(ct);

        List<GenerateResult> results = [];
        foreach (var schedule in schedules)
        {
            try
            {
                var lookaheadEnd = today.AddDays(schedule.AutoGenerateWeeksAhead * 7);
                var result = await GenerateForScheduleAsync(schedule.Id, today, lookaheadEnd, ct);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RunAutoGenerate failed for schedule {ScheduleId}", schedule.Id);
                results.Add(new GenerateResult());
            }
        }

        return results;
    }

    private async Task<GenerateResult> GenerateForScheduleAsync(int scheduleId, DateOnly rangeStart, DateOnly rangeEnd, CancellationToken ct = default)
    {
        var schedule = await _db.RecurringSchedules
            .Include(rs => rs.SourceBooking).ThenInclude(b => b.BookingServices)
            .FirstOrDefaultAsync(rs => rs.Id == scheduleId, ct);

        if (schedule is null || !schedule.IsActive)
            return new GenerateResult();

        var source = schedule.SourceBooking;
        var anchorDate = DateOnly.FromDateTime(source.ScheduledDate);
        var occurrenceDates = GetOccurrenceDates(schedule, rangeStart, rangeEnd, anchorDate);

        var effectiveEnd = schedule.EndsOn;
        if (effectiveEnd.HasValue)
            occurrenceDates = occurrenceDates.Where(d => d < effectiveEnd.Value).ToList();

        if (occurrenceDates.Count == 0)
            return new GenerateResult();

        var existingDates = await _db.Bookings
            .Where(b => b.RecurringScheduleId == scheduleId
                && b.ScheduledDate >= rangeStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                && b.ScheduledDate <= rangeEnd.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                && b.Status != BookingStatus.Cancelled)
            .Select(b => DateOnly.FromDateTime(b.ScheduledDate))
            .ToListAsync(ct);

        var existingSet = new HashSet<DateOnly>(existingDates);

        var overrides = await _db.DateOverrides
            .Where(o => o.Date >= rangeStart && o.Date <= rangeEnd && o.IsFullyClosed)
            .Select(o => o.Date)
            .ToListAsync(ct);

        var closedSet = new HashSet<DateOnly>(overrides);

        List<BookingResponse> generated = [];
        List<SkippedOccurrence> skipped = [];
        var now = DateTime.UtcNow;
        List<Booking> newBookings = [];

        foreach (var date in occurrenceDates)
        {
            if (existingSet.Contains(date))
            {
                skipped.Add(new SkippedOccurrence { Date = date, Reason = "Already exists" });
                continue;
            }

            if (closedSet.Contains(date))
            {
                skipped.Add(new SkippedOccurrence { Date = date, Reason = "Closed day" });
                continue;
            }

            var newBooking = new Booking
            {
                ClientId = source.ClientId,
                SiteId = source.SiteId,
                ServiceType = source.ServiceType,
                ScheduledDate = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                ScheduledTimeSlot = source.ScheduledTimeSlot,
                Status = BookingStatus.Pending,
                Notes = source.Notes,
                RecurringScheduleId = scheduleId,
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var bs in source.BookingServices)
            {
                newBooking.BookingServices.Add(new BookingService
                {
                    ServiceCatalogId = bs.ServiceCatalogId,
                    EstimatedPrice = bs.EstimatedPrice,
                    Quantity = bs.Quantity,
                    Notes = bs.Notes
                });
            }

            _db.Bookings.Add(newBooking);
            newBookings.Add(newBooking);
        }

        await _db.SaveChangesAsync(ct);

        foreach (var b in newBookings)
        {
            generated.Add(BookingMapper.ToResponse(b));
            await _sopManager.EnsureServiceSopsAssignedAsync(b.Id, ct);
        }

        return new GenerateResult { Generated = generated, Skipped = skipped };
    }

    private static List<DateOnly> GetOccurrenceDates(RecurringSchedule schedule, DateOnly rangeStart, DateOnly rangeEnd, DateOnly anchorDate)
    {
        List<DateOnly> dates = [];
        var current = rangeStart;

        while (current <= rangeEnd)
        {
            var matches = schedule.Frequency switch
            {
                "Weekly" => schedule.DayOfWeek.HasValue && (int)current.DayOfWeek == schedule.DayOfWeek.Value,
                "Biweekly" => schedule.DayOfWeek.HasValue
                    && (int)current.DayOfWeek == schedule.DayOfWeek.Value
                    && IsBiweeklyMatch(current, anchorDate),
                "Monthly" => schedule.DayOfMonth.HasValue && current.Day == schedule.DayOfMonth.Value,
                _ => false
            };

            if (matches)
                dates.Add(current);

            current = current.AddDays(1);
        }

        return dates;
    }

    private static bool IsBiweeklyMatch(DateOnly current, DateOnly anchor)
    {
        var diff = Math.Abs(current.DayNumber - anchor.DayNumber);
        return (diff / 7) % 2 == 0;
    }

    private static string? ValidateFrequency(string frequency, int? dayOfWeek, int? dayOfMonth, int autoGenerateWeeksAhead)
    {
        if (frequency != "Weekly" && frequency != "Biweekly" && frequency != "Monthly")
            return "Frequency must be 'Weekly', 'Biweekly', or 'Monthly'.";

        if ((frequency == "Weekly" || frequency == "Biweekly") && !dayOfWeek.HasValue)
            return "DayOfWeek is required for Weekly and Biweekly frequencies.";

        if (dayOfWeek.HasValue && (dayOfWeek.Value < 0 || dayOfWeek.Value > 6))
            return "DayOfWeek must be between 0 (Sunday) and 6 (Saturday).";

        if (frequency == "Monthly" && !dayOfMonth.HasValue)
            return "DayOfMonth is required for Monthly frequency.";

        if (dayOfMonth.HasValue && (dayOfMonth.Value < 1 || dayOfMonth.Value > 28))
            return "DayOfMonth must be between 1 and 28. Values above 28 are not used because not all months have more than 28 days.";

        if (autoGenerateWeeksAhead < 1 || autoGenerateWeeksAhead > 52)
            return "AutoGenerateWeeksAhead must be between 1 and 52.";

        return null;
    }
}
