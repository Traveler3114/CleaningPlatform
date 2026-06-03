namespace CleaningPlatformAPI.Common;

public static class PermissionKeys
{
    // ── Pages ────────────────────────────────────────────────
    public const string PagesDaily = "pages.daily";
    public const string PagesBookings = "pages.bookings";
    public const string PagesSchedule = "pages.schedule";
    public const string PagesUsers = "pages.users";
    public const string PagesRoles = "pages.roles";
    public const string PagesClients = "pages.clients";
    public const string PagesKanban = "pages.kanban";
    public const string PagesSop = "pages.sop";
    public const string PagesReports = "pages.reports";
    public const string PagesInventory = "pages.inventory";

    // ── Bookings ─────────────────────────────────────────────
    public const string BookingsView = "bookings.view";
    public const string BookingsCreate = "bookings.create";
    public const string BookingsEdit = "bookings.edit";
    public const string BookingsDelete = "bookings.delete";
    public const string BookingsProgress = "bookings.progress";

    // ── Clients ──────────────────────────────────────────────
    public const string ClientsView = "clients.view";
    public const string ClientsCreate = "clients.create";
    public const string ClientsEdit = "clients.edit";
    public const string ClientsDelete = "clients.delete";

    // ── Invoices ─────────────────────────────────────────────
    public const string InvoicesView = "invoices.view";
    public const string InvoicesCreate = "invoices.create";
    public const string InvoicesEdit = "invoices.edit";

    // ── SOPs ─────────────────────────────────────────────────
    public const string SopsView = "sops.view";
    public const string SopsManage = "sops.manage";

    // ── Services ─────────────────────────────────────────────
    public const string ServicesView = "services.view";
    public const string ServicesManage = "services.manage";

    // ── Schedule ─────────────────────────────────────────────
    public const string ScheduleView = "schedule.view";
    public const string ScheduleEdit = "schedule.edit";

    // ── Users ────────────────────────────────────────────────
    public const string UsersView = "users.view";
    public const string UsersCreate = "users.create";
    public const string UsersEdit = "users.edit";

    // ── Roles ────────────────────────────────────────────────
    public const string RolesView = "roles.view";
    public const string RolesManage = "roles.manage";

    // ── Reports ──────────────────────────────────────────────
    public const string ReportsView = "reports.view";
    public const string ReportsExport = "reports.export";

    // ── Inventory ────────────────────────────────────────────
    public const string InventoryView = "inventory.view";
    public const string InventoryManage = "inventory.manage";

    // ── Master list (must stay in sync with Meta below) ──────
    public static readonly string[] All =
    [
        // Pages
        PagesDaily,
        PagesBookings,
        PagesSchedule,
        PagesUsers,
        PagesRoles,
        PagesClients,
        PagesKanban,
        PagesSop,
        PagesReports,
        PagesInventory,
        // Bookings
        BookingsView,
        BookingsCreate,
        BookingsEdit,
        BookingsDelete,
        BookingsProgress,
        // Clients
        ClientsView,
        ClientsCreate,
        ClientsEdit,
        ClientsDelete,
        // Invoices
        InvoicesView,
        InvoicesCreate,
        InvoicesEdit,
        // SOPs
        SopsView,
        SopsManage,
        // Services
        ServicesView,
        ServicesManage,
        // Schedule
        ScheduleView,
        ScheduleEdit,
        // Users
        UsersView,
        UsersCreate,
        UsersEdit,
        // Roles
        RolesView,
        RolesManage,
        // Reports
        ReportsView,
        ReportsExport,
        // Inventory
        InventoryView,
        InventoryManage,
    ];

    public static readonly Dictionary<string, (string DisplayName, string Description, string Category)> Meta = new()
    {
        // Pages
        [PagesDaily] = ("Daily View", "Access the daily schedule view", "Pages"),
        [PagesBookings] = ("Bookings Page", "Access the bookings list page", "Pages"),
        [PagesSchedule] = ("Schedule Page", "Access the weekly schedule editor", "Pages"),
        [PagesUsers] = ("Users Page", "Access the user management page", "Pages"),
        [PagesRoles] = ("Roles Page", "Access the roles management page", "Pages"),
        [PagesClients] = ("Clients Page", "Access the client management page", "Pages"),
        [PagesKanban] = ("Kanban / Calendar", "Access the kanban and calendar board", "Pages"),
        [PagesSop] = ("SOP Library Page", "Access the SOP library", "Pages"),
        [PagesReports] = ("Reports Page", "Access the finance and reports page", "Pages"),
        [PagesInventory] = ("Inventory Page", "Access the inventory management page", "Pages"),

        // Bookings
        [BookingsView] = ("View Bookings", "Read bookings list and detail", "Bookings"),
        [BookingsCreate] = ("Create Bookings", "Create new bookings from the admin panel", "Bookings"),
        [BookingsEdit] = ("Edit Bookings", "Update status, assign employees, edit services", "Bookings"),
        [BookingsDelete] = ("Cancel Bookings", "Cancel or delete bookings", "Bookings"),
        [BookingsProgress] = ("Progress Bookings", "Progress a booking through execution stages (field use)", "Bookings"),

        // Clients
        [ClientsView] = ("View Clients", "Read client list and detail", "Clients"),
        [ClientsCreate] = ("Create Clients", "Create new clients", "Clients"),
        [ClientsEdit] = ("Edit Clients", "Edit client profile, contacts and sites", "Clients"),
        [ClientsDelete] = ("Delete Clients", "Deactivate clients and sites", "Clients"),

        // Invoices
        [InvoicesView] = ("View Invoices", "Read invoices and payment history", "Invoices"),
        [InvoicesCreate] = ("Create Invoices", "Generate invoices from completed bookings", "Invoices"),
        [InvoicesEdit] = ("Edit Invoices", "Record payments and update invoice status", "Invoices"),

        // SOPs
        [SopsView] = ("View SOPs", "Read the SOP library and checklists", "SOPs"),
        [SopsManage] = ("Manage SOPs", "Create, edit and delete SOP templates and checklist items", "SOPs"),

        // Services
        [ServicesView] = ("View Services", "Read the service catalog", "Services"),
        [ServicesManage] = ("Manage Services", "Create, edit and delete services", "Services"),

        // Schedule
        [ScheduleView] = ("View Schedule", "Read the weekly schedule and date overrides", "Schedule"),
        [ScheduleEdit] = ("Edit Schedule", "Edit the weekly schedule and manage date overrides", "Schedule"),

        // Users
        [UsersView] = ("View Users", "Read the user list", "Users"),
        [UsersCreate] = ("Create Users", "Create new user accounts", "Users"),
        [UsersEdit] = ("Edit Users", "Reset passwords and toggle user active status", "Users"),

        // Roles
        [RolesView] = ("View Roles", "Read the roles list", "Roles"),
        [RolesManage] = ("Manage Roles", "Create, edit and delete roles", "Roles"),

        // Reports
        [ReportsView] = ("View Reports", "Read financial reports and dashboards", "Reports"),
        [ReportsExport] = ("Export Reports", "Export financial data to Excel", "Reports"),

        // Inventory
        [InventoryView] = ("View Inventory", "Read the inventory list", "Inventory"),
        [InventoryManage] = ("Manage Inventory", "Create, edit and delete inventory items", "Inventory"),
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