namespace CleaningPlatformAPI.Common;

public static class PermissionKeys
{
    // ── Pages ────────────────────────────────────────────────
    public const string PagesDaily    = "pages.daily";
    public const string PagesBookings = "pages.bookings";
    public const string PagesSchedule = "pages.schedule";
    public const string PagesUsers    = "pages.users";
    public const string PagesRoles    = "pages.roles";
    public const string PagesClients  = "pages.clients";
    public const string PagesKanban   = "pages.kanban";
    public const string PagesSop      = "pages.sop";
    public const string PagesReports  = "pages.reports";

    // ── Booking actions ──────────────────────────────────────
    public const string ActionsBookingAssign       = "actions.booking.assign";
    public const string ActionsBookingUpdateStatus = "actions.booking.updateStatus";
    public const string ActionsBookingCreate       = "actions.booking.create";

    // ── Schedule actions ─────────────────────────────────────
    public const string ActionsScheduleEdit   = "actions.schedule.edit";
    public const string ActionsOverrideManage = "actions.override.manage";

    // ── User actions ─────────────────────────────────────────
    public const string ActionsUserCreate       = "actions.user.create";
    public const string ActionsUserToggleActive = "actions.user.toggleActive";

    // ── Role actions ─────────────────────────────────────────
    public const string ActionsRoleManage = "actions.role.manage";

    // ── Service catalog actions ──────────────────────────────
    public const string ActionsServiceCatalogEdit   = "actions.serviceCatalog.edit";
    public const string ActionsServiceCatalogManage = "actions.serviceCatalog.manage";

    // ── SOP actions ───────────────────────────────────────────
    public const string ActionsSopManage = "actions.sop.manage";

    // ── Reporting actions ────────────────────────────────────
    public const string ActionsReportsExport = "actions.reports.export";

    // ── Master list (must stay in sync with Meta below) ──────
    public static readonly string[] All =
    [
        PagesDaily,
        PagesBookings,
        PagesSchedule,
        PagesUsers,
        PagesRoles,
        PagesClients,
        PagesKanban,
        PagesSop,
        PagesReports,
        ActionsBookingAssign,
        ActionsBookingUpdateStatus,
        ActionsBookingCreate,
        ActionsScheduleEdit,
        ActionsOverrideManage,
        ActionsUserCreate,
        ActionsUserToggleActive,
        ActionsRoleManage,
        ActionsServiceCatalogEdit,
        ActionsServiceCatalogManage,
        ActionsSopManage,
        ActionsReportsExport,
    ];

    public static readonly Dictionary<string, (string DisplayName, string Description, string Category)> Meta = new()
    {
        // Pages
        [PagesDaily]    = ("Daily View Page",    "Can access the daily view page",            "Pages"),
        [PagesBookings] = ("Bookings Page",       "Can access the bookings page",              "Pages"),
        [PagesSchedule] = ("Schedule Page",       "Can access the weekly schedule editor",     "Pages"),
        [PagesUsers]    = ("Users Page",          "Can access the users management page",      "Pages"),
        [PagesRoles]    = ("Roles Page",          "Can access the roles management page",      "Pages"),
        [PagesClients]  = ("Clients Page",        "Can access the clients management page",    "Pages"),
        [PagesKanban]   = ("Kanban Board",        "Can access the Kanban board",               "Pages"),
        [PagesSop]      = ("SOP Library Page",    "Can access the SOP library",                "Pages"),
        [PagesReports]  = ("Reports Page",        "Can access the finance and reports page",   "Pages"),

        // Booking actions
        [ActionsBookingAssign]       = ("Assign Employees to Bookings", "Can assign and remove employees from bookings", "Bookings"),
        [ActionsBookingUpdateStatus] = ("Update Booking Status",        "Can update the status of bookings",            "Bookings"),
        [ActionsBookingCreate]       = ("Create Bookings",              "Can create new bookings from the admin panel", "Bookings"),

        // Schedule actions
        [ActionsScheduleEdit]   = ("Edit Schedule",           "Can edit the weekly schedule",          "Schedule"),
        [ActionsOverrideManage] = ("Manage Date Overrides",   "Can manage date-specific overrides",    "Schedule"),

        // User actions
        [ActionsUserCreate]       = ("Create User Accounts",      "Can create new user accounts",          "Users"),
        [ActionsUserToggleActive] = ("Activate/Deactivate Users", "Can toggle user active status",         "Users"),

        // Role actions
        [ActionsRoleManage] = ("Manage Roles", "Can create, edit and delete roles", "Roles"),

        // Service catalog actions
        [ActionsServiceCatalogEdit]   = ("Edit Service Catalog", "Can update service pricing and details", "Services"),
        [ActionsServiceCatalogManage] = ("Manage Services",      "Can add or remove services",             "Services"),

        // SOP actions
        [ActionsSopManage] = ("Manage SOPs", "Can create, edit and delete SOP templates and checklist items", "SOPs"),

        // Reporting actions
        [ActionsReportsExport] = ("Export Reports", "Can export financial reports to Excel", "Reports"),
    };

    static PermissionKeys()
    {
        var missingMeta = All.Where(k => !Meta.ContainsKey(k)).ToList();
        if (missingMeta.Count > 0)
            throw new InvalidOperationException(
                $"PermissionKeys.Meta is missing entries for: {string.Join(", ", missingMeta)}");

        var extraMeta = Meta.Keys.Where(k => !All.Contains(k)).ToList();
        if (extraMeta.Count > 0)
            throw new InvalidOperationException(
                $"PermissionKeys.Meta has entries not in All: {string.Join(", ", extraMeta)}");
    }
}
