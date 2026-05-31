// admin-common.js – shared UI helpers, sidebar nav, logout

const adminNav = [
    { section: 'Operations', items: [
        { label: 'Daily View', href: 'index.html', perm: 'pages.daily' },
        { label: 'Calendar', href: 'calendar.html', perm: 'pages.kanban' },
    ]},
    { section: 'Bookings', items: [
        { label: 'Bookings', href: 'bookings.html', perm: 'pages.bookings' },
        { label: 'Requests', href: 'requests.html', perm: 'bookings.view' },
        { label: 'Recurring', href: 'recurring.html', perm: 'bookings.view' },
        { label: 'Invoices', href: 'invoices.html', perm: 'invoices.view' },
    ]},
    { section: 'Clients', items: [
        { label: 'Client List', href: 'clients.html', perm: 'pages.clients' },
    ]},
    { section: 'Config', items: [
        { label: 'Schedule', href: 'schedule.html', perm: 'schedule.view' },
        { label: 'Services', href: 'services.html', perm: 'services.view' },
        { label: 'SOPs', href: 'sops.html', perm: 'pages.sop' },
    ]},
    { section: 'Admin', items: [
        { label: 'Users', href: 'users.html', perm: 'pages.users' },
        { label: 'Roles', href: 'roles.html', perm: 'pages.roles' },
        { label: 'Reports', href: 'reports.html', perm: 'pages.reports' },
    ]},
];

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

        html += `<div class="nav-section"><span class="nav-section-title">${section.section}</span>`;
        visibleItems.forEach(item => {
            const active = location.pathname.endsWith(item.href) ? ' active' : '';
            html += `<a href="${item.href}" class="nav-item${active}">${item.label}</a>`;
        });
        html += '</div>';
    });

    html += '</nav>';
    sidebar.innerHTML = html;

    const toggle = document.getElementById('mobile-toggle');
    if (toggle && sidebar) {
        toggle.addEventListener('click', () => sidebar.classList.toggle('open'));
        sidebar.querySelectorAll('.nav-item').forEach(item => {
            item.addEventListener('click', () => sidebar.classList.remove('open'));
        });
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

