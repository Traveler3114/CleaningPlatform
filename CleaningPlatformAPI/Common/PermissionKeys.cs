namespace CleaningPlatformAPI.Common;

public static class PermissionKeys
{
    public const string PagesDaily = "pages.daily";
    public const string PagesBookings = "pages.bookings";
    public const string PagesSchedule = "pages.schedule";
    public const string PagesUsers = "pages.users";
    public const string PagesRoles = "pages.roles";

    public const string ActionsBookingUpdateStatus = "actions.booking.updateStatus";
    public const string ActionsScheduleEdit = "actions.schedule.edit";
    public const string ActionsOverrideManage = "actions.override.manage";
    public const string ActionsUserCreate = "actions.user.create";
    public const string ActionsUserToggleActive = "actions.user.toggleActive";
    public const string ActionsRoleManage = "actions.role.manage";

    public static readonly string[] All =
    [
        PagesDaily,
        PagesBookings,
        PagesSchedule,
        PagesUsers,
        PagesRoles,
        ActionsBookingUpdateStatus,
        ActionsScheduleEdit,
        ActionsOverrideManage,
        ActionsUserCreate,
        ActionsUserToggleActive,
        ActionsRoleManage
    ];

    public static readonly Dictionary<string, (string DisplayName, string Description, string Category)> Meta = new()
    {
        [PagesDaily]                  = ("Daily View Page",           "Can access the daily view page",        "Pages"),
        [PagesBookings]               = ("Bookings Page",             "Can access the bookings page",          "Pages"),
        [PagesSchedule]               = ("Schedule Page",             "Can access the weekly schedule editor", "Pages"),
        [PagesUsers]                  = ("Users Page",                "Can access the users management page",  "Pages"),
        [PagesRoles]                  = ("Roles Page",                "Can access the roles management page",  "Pages"),
        [ActionsBookingUpdateStatus]  = ("Update Booking Status",     "Can update the status of bookings",     "Bookings"),
        [ActionsScheduleEdit]         = ("Edit Schedule",             "Can edit the weekly schedule",          "Schedule"),
        [ActionsOverrideManage]       = ("Manage Date Overrides",     "Can manage date-specific overrides",    "Schedule"),
        [ActionsUserCreate]           = ("Create User Accounts",      "Can create new user accounts",          "Users"),
        [ActionsUserToggleActive]     = ("Activate/Deactivate Users", "Can toggle user active status",         "Users"),
        [ActionsRoleManage]           = ("Manage Roles",              "Can create, edit and delete roles",     "Roles"),
    };
}
