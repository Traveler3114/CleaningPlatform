// ===== Permission Key Definitions =====
// Mirror of backend PermissionKeys.cs — single source of truth on the frontend.
// Only edit this file when adding/removing a permission key.

const PERMISSIONS = {
    'pages.daily':                  { label: 'Daily View Page',            category: 'Pages' },
    'pages.bookings':               { label: 'Bookings Page',              category: 'Pages' },
    'pages.schedule':               { label: 'Schedule Page',              category: 'Pages' },
    'pages.users':                  { label: 'Users Page',                 category: 'Pages' },
    'pages.roles':                  { label: 'Roles Page',                 category: 'Pages' },
    'actions.booking.updateStatus': { label: 'Update Booking Status',      category: 'Bookings' },
    'actions.schedule.edit':        { label: 'Edit Schedule',              category: 'Schedule' },
    'actions.override.manage':      { label: 'Manage Date Overrides',      category: 'Schedule' },
    'actions.user.create':          { label: 'Create User Accounts',       category: 'Users' },
    'actions.user.toggleActive':    { label: 'Activate/Deactivate Users',  category: 'Users' },
    'actions.role.manage':          { label: 'Manage Roles',               category: 'Roles' },
};
