// admin-common.js – shared UI helpers, sidebar nav, logout

const adminNav = [
    { sectionKey: 'nav_section_operations', section: 'Operations', items: [
        { labelKey: 'nav_daily_view', label: 'Daily View', href: 'index.html', perm: 'pages.daily' },
        { labelKey: 'nav_calendar', label: 'Calendar', href: 'calendar.html', perm: 'pages.kanban' },
    ]},
    { sectionKey: 'nav_section_bookings', section: 'Bookings', items: [
        { labelKey: 'nav_bookings', label: 'Bookings', href: 'bookings.html', perm: 'pages.bookings' },
        { labelKey: 'nav_requests', label: 'Requests', href: 'requests.html', perm: 'bookings.view' },
        { labelKey: 'nav_recurring', label: 'Recurring', href: 'recurring.html', perm: 'bookings.view' },
        { labelKey: 'nav_invoices', label: 'Invoices', href: 'invoices.html', perm: 'invoices.view' },
    ]},
    { sectionKey: 'nav_section_clients', section: 'Clients', items: [
        { labelKey: 'nav_client_list', label: 'Client List', href: 'clients.html', perm: 'pages.clients' },
    ]},
    { sectionKey: 'nav_section_config', section: 'Config', items: [
        { labelKey: 'nav_schedule', label: 'Schedule', href: 'schedule.html', perm: 'schedule.view' },
        { labelKey: 'nav_services', label: 'Services', href: 'services.html', perm: 'services.view' },
        { labelKey: 'nav_inventory', label: 'Inventory', href: 'inventory.html', perm: 'pages.inventory' },
        { labelKey: 'nav_sops', label: 'SOPs', href: 'sops.html', perm: 'pages.sop' },
    ]},
    { sectionKey: 'nav_section_admin', section: 'Admin', items: [
        { labelKey: 'nav_users', label: 'Users', href: 'users.html', perm: 'pages.users' },
        { labelKey: 'nav_roles', label: 'Roles', href: 'roles.html', perm: 'pages.roles' },
        { labelKey: 'nav_reports', label: 'Reports', href: 'reports.html', perm: 'pages.reports' },
    ]},
];

var _sidebarToggleSetup = false;

function renderSidebar() {
    const sidebar = document.getElementById('sidebar');
    if (!sidebar) return;

    const perms = window._userPermissions || [];
    const currentPage = window.location.pathname.split('/').pop();

    let html = '<div class="sidebar-brand">Chistify</div><nav class="sidebar-nav">';

    adminNav.forEach(section => {
        const visibleItems = section.items.filter(item =>
            !item.perm || perms.includes('*') || perms.includes(item.perm)
        );
        if (visibleItems.length === 0) return;

        html += `<div class="nav-section"><span class="nav-section-title">${window.__(section.sectionKey) || section.section}</span>`;
        visibleItems.forEach(item => {
            const active = location.pathname.endsWith(item.href) ? ' active' : '';
            html += `<a href="${item.href}" class="nav-item${active}">${window.__(item.labelKey) || item.label}</a>`;
        });
        html += '</div>';
    });

    html += '</nav>';
    sidebar.innerHTML = html;

    if (!_sidebarToggleSetup) {
        const toggle = document.getElementById('mobile-toggle');
        if (toggle) {
            toggle.addEventListener('click', function () {
                sidebar.classList.toggle('open');
            });
            sidebar.addEventListener('click', function (e) {
                if (e.target.classList.contains('nav-item')) sidebar.classList.remove('open');
            });
        }
        _sidebarToggleSetup = true;
    }
}

document.addEventListener('DOMContentLoaded', async () => {
    if (!isAuthenticated()) {
        window.location.href = 'login.html';
        return;
    }

    const token = getToken();
    if (token) {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        const perms = payload['permission'] || [];
        window._userPermissions = perms;
        window._userRole = role;
        if (role === 'Owner') window._userPermissions = ['*'];
    }

    renderSidebar();

    try {
        const user = await loadCurrentUser();
        const userNameEl = document.querySelector('.user-name');
        const roleEl = document.querySelector('.user-role');
        const avatarEl = document.querySelector('.user-avatar');
        if (userNameEl) userNameEl.textContent = user.username;
        if (roleEl) roleEl.textContent = user.role;
        if (avatarEl) avatarEl.textContent = (user.username || 'U')[0].toUpperCase();
    } catch(e) { console.error(e); }

    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) logoutBtn.addEventListener('click', logout);
});

window.addEventListener('i18nReady', function () {
    var sidebar = document.getElementById('sidebar');
    if (sidebar) renderSidebar();
});

function statusBadge(status) {
    var cls = status.toLowerCase();
    return '<span class="badge badge-' + cls + '">' + window.__status(status) + '</span>';
}

function showError(message, containerId = 'error-container') {
    let container = document.getElementById(containerId);
    if (!container) {
        container = document.createElement('div');
        container.id = containerId;
        document.querySelector('.admin-main')?.prepend(container);
    }
    container.innerHTML = `<div class="alert alert-danger">${message}</div>`;
    setTimeout(() => container.innerHTML = '', 5000);
}

function showSuccess(message, containerId = 'success-container') {
    let container = document.getElementById(containerId);
    if (!container) {
        container = document.createElement('div');
        container.id = containerId;
        document.querySelector('.admin-main')?.prepend(container);
    }
    container.innerHTML = `<div class="alert alert-success">${message}</div>`;
    setTimeout(() => container.innerHTML = '', 3000);
}

function formatDate(date) {
    const d = new Date(date);
    return d.toISOString().split('T')[0];
}

function formatDateTime(date) {
    const d = new Date(date);
    return d.toLocaleString();
}

