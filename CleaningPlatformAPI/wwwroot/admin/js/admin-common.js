// admin-common.js – shared UI helpers, navigation, logout, permission-based menu
document.addEventListener('DOMContentLoaded', async () => {
    // Check authentication
    if (!isAuthenticated()) {
        window.location.href = 'login.html';
        return;
    }
    
    // Load user permissions and store globally
    const token = getToken();
    if (token) {
        const payload = JSON.parse(atob(token.split('.')[1]));
        const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        const perms = payload['permission'] || [];
        window._userPermissions = perms;
        window._userRole = role;
        // Owner has all permissions
        if (role === 'Owner') window._userPermissions = ['*'];
    }
    
    // Populate user pill
    const userPillName = document.querySelector('.user-pill-name');
    if (userPillName) {
        try {
            const user = await loadCurrentUser();
            userPillName.textContent = user.username;
            document.querySelector('.user-pill-role').textContent = user.role;
        } catch(e) { console.error(e); }
    }
    
    // Setup logout button
    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) logoutBtn.addEventListener('click', logout);
    
    // Highlight active page based on current file name
    const currentPage = window.location.pathname.split('/').pop();
    document.querySelectorAll('.nav-group__item').forEach(link => {
        const href = link.getAttribute('href');
        if (href && href === currentPage) link.classList.add('active');
    });
    
    // Show/hide navigation groups based on permissions (optional)
    // We'll keep all nav items but rely on server-side authorization for API calls.
    // You may want to hide based on permissions – for now we show all.
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