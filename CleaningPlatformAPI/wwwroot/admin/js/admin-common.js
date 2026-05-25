// admin-common.js – shared UI helpers, sidebar nav, logout
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

    // Load user info
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

    // Highlight active nav item
    const currentPage = window.location.pathname.split('/').pop();
    document.querySelectorAll('.nav-item').forEach(link => {
        const href = link.getAttribute('href');
        if (href && href === currentPage) link.classList.add('active');
    });

    // Mobile sidebar toggle
    const toggle = document.getElementById('mobile-toggle');
    const sidebar = document.getElementById('sidebar');
    if (toggle && sidebar) {
        toggle.addEventListener('click', () => sidebar.classList.toggle('open'));
        // Close sidebar on nav click (mobile)
        sidebar.querySelectorAll('.nav-item').forEach(item => {
            item.addEventListener('click', () => sidebar.classList.remove('open'));
        });
    }
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
