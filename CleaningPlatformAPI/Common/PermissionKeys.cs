namespace CleaningPlatformAPI.Common;

public static class PermissionKeys
{
    // ── Pages (unchanged) ───────────────────────────────────
    public const string PagesDaily    = "pages.daily";
    public const string PagesBookings = "pages.bookings";
    public const string PagesSchedule = "pages.schedule";
    public const string PagesUsers    = "pages.users";
    public const string PagesRoles    = "pages.roles";
    public const string PagesClients  = "pages.clients";
    public const string PagesKanban   = "pages.kanban";
    public const string PagesSop      = "pages.sop";
    public const string PagesReports  = "pages.reports";

    // ── Domain permissions ──────────────────────────────────
    public const string BookingsView   = "bookings.view";
    public const string BookingsCreate = "bookings.create";
    public const string BookingsEdit   = "bookings.edit";
    public const string BookingsDelete = "bookings.delete";

    public const string ClientsView   = "clients.view";
    public const string ClientsCreate = "clients.create";
    public const string ClientsEdit   = "clients.edit";
    public const string ClientsDelete = "clients.delete";

    public const string InvoicesView   = "invoices.view";
    public const string InvoicesCreate = "invoices.create";
    public const string InvoicesEdit   = "invoices.edit";

    public const string SopsView   = "sops.view";
    public const string SopsManage = "sops.manage";

    public const string ServicesView   = "services.view";
    public const string ServicesManage = "services.manage";

    public const string ScheduleView = "schedule.view";
    public const string ScheduleEdit = "schedule.edit";

    public const string UsersView   = "users.view";
    public const string UsersCreate = "users.create";
    public const string UsersEdit   = "users.edit";

    public const string RolesView   = "roles.view";
    public const string RolesManage = "roles.manage";

    public const string ReportsView   = "reports.view";
    public const string ReportsExport = "reports.export";

    public static readonly string[] All =
    [
        PagesDaily, PagesBookings, PagesSchedule, PagesUsers, PagesRoles, PagesClients, PagesKanban, PagesSop, PagesReports,
        BookingsView, BookingsCreate, BookingsEdit, BookingsDelete,
        ClientsView, ClientsCreate, ClientsEdit, ClientsDelete,
        InvoicesView, InvoicesCreate, InvoicesEdit,
        SopsView, SopsManage,
        ServicesView, ServicesManage,
        ScheduleView, ScheduleEdit,
        UsersView, UsersCreate, UsersEdit,
        RolesView, RolesManage,
        ReportsView, ReportsExport
    ];

    public static readonly Dictionary<string, (string DisplayName, string Description, string Category)> Meta = new()
    {
        [PagesDaily] = ("Daily View Page", "Can access the daily view page", "Pages"),
        [PagesBookings] = ("Bookings Page", "Can access the bookings page", "Pages"),
        [PagesSchedule] = ("Schedule Page", "Can access the weekly schedule editor", "Pages"),
        [PagesUsers] = ("Users Page", "Can access the users management page", "Pages"),
        [PagesRoles] = ("Roles Page", "Can access the roles management page", "Pages"),
        [PagesClients] = ("Clients Page", "Can access the clients management page", "Pages"),
        [PagesKanban] = ("Kanban Board", "Can access the Kanban board", "Pages"),
        [PagesSop] = ("SOP Library Page", "Can access the SOP library", "Pages"),
        [PagesReports] = ("Reports Page", "Can access the finance and reports page", "Pages"),
        [BookingsView] = ("View Bookings", "Read bookings list and detail", "Bookings"),
        [BookingsCreate] = ("Create Bookings", "Create new bookings", "Bookings"),
        [BookingsEdit] = ("Edit Bookings", "Update status, assignments and prices", "Bookings"),
        [BookingsDelete] = ("Delete Bookings", "Cancel or delete bookings", "Bookings"),
        [ClientsView] = ("View Clients", "Read client list and detail", "Clients"),
        [ClientsCreate] = ("Create Clients", "Create new clients", "Clients"),
        [ClientsEdit] = ("Edit Clients", "Edit profiles, contacts, and sites", "Clients"),
        [ClientsDelete] = ("Delete Clients", "Deactivate clients and sites", "Clients"),
        [InvoicesView] = ("View Invoices", "Read invoices", "Invoices"),
        [InvoicesCreate] = ("Create Invoices", "Generate invoice from booking", "Invoices"),
        [InvoicesEdit] = ("Edit Invoices", "Record payments and update status", "Invoices"),
        [SopsView] = ("View SOPs", "Read SOP library", "SOPs"),
        [SopsManage] = ("Manage SOPs", "Create, edit, delete SOP templates and checklist items", "SOPs"),
        [ServicesView] = ("View Services", "Read service catalog", "Services"),
        [ServicesManage] = ("Manage Services", "Create, edit, delete services", "Services"),
        [ScheduleView] = ("View Schedule", "Read schedule and overrides", "Schedule"),
        [ScheduleEdit] = ("Edit Schedule", "Edit schedule and date overrides", "Schedule"),
        [UsersView] = ("View Users", "Read user list", "Users"),
        [UsersCreate] = ("Create Users", "Create new users", "Users"),
        [UsersEdit] = ("Edit Users", "Reset passwords and toggle active", "Users"),
        [RolesView] = ("View Roles", "Read role list", "Roles"),
        [RolesManage] = ("Manage Roles", "Create, edit, delete roles", "Roles"),
        [ReportsView] = ("View Reports", "Read financial reports", "Reports"),
        [ReportsExport] = ("Export Reports", "Export reports to Excel", "Reports"),
    };

    static PermissionKeys()
    {
        var missingMeta = All.Where(k => !Meta.ContainsKey(k)).ToList();
        if (missingMeta.Count > 0) throw new InvalidOperationException($"PermissionKeys.Meta is missing entries for: {string.Join(", ", missingMeta)}");
        var extraMeta = Meta.Keys.Where(k => !All.Contains(k)).ToList();
        if (extraMeta.Count > 0) throw new InvalidOperationException($"PermissionKeys.Meta has entries not in All: {string.Join(", ", extraMeta)}");
    }
}
