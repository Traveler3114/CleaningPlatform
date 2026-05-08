// ===== Admin Auth Utility =====
// Shared across all admin pages

const AUTH_KEY = 'vc_admin_token';
const USER_KEY = 'vc_admin_user';

const Auth = {
    getToken() {
        return localStorage.getItem(AUTH_KEY);
    },

    getUser() {
        const raw = localStorage.getItem(USER_KEY);
        try { return raw ? JSON.parse(raw) : null; } catch { return null; }
    },

    setSession(token, user) {
        localStorage.setItem(AUTH_KEY, token);
        localStorage.setItem(USER_KEY, JSON.stringify(user));
    },

    clearSession() {
        localStorage.removeItem(AUTH_KEY);
        localStorage.removeItem(USER_KEY);
    },

    isLoggedIn() {
        return !!this.getToken();
    },

    getRole() {
        return this.getUser()?.role || null;
    },

    getPermissions() {
        return this.getUser()?.permissions || [];
    },

    // Returns true if user has the given permission key OR is Owner
    can(permissionKey) {
        if (this.getRole() === 'Owner') return true;
        return this.getPermissions().includes(permissionKey);
    },

    // Checks pages.{pageKey} permission
    canPage(pageKey) {
        return this.can(`pages.${pageKey}`);
    },

    // Redirect to login if not authenticated
    requireAuth() {
        if (!this.isLoggedIn()) {
            window.location.href = '/admin/login.html';
            return false;
        }
        return true;
    },

    // Redirect to login if not authenticated, or to daily view if lacking permission
    requirePermission(permissionKey) {
        if (!this.isLoggedIn()) {
            window.location.href = '/admin/login.html';
            return false;
        }
        if (!this.can(permissionKey)) {
            window.location.href = '/admin/index.html';
            return false;
        }
        return true;
    },

    authHeader() {
        const token = this.getToken();
        return token ? { 'Authorization': `Bearer ${token}` } : {};
    },

    logout() {
        this.clearSession();
        window.location.href = '/admin/login.html';
    }
};

// ===== Header Injection =====
// Call this on every admin page after DOM loads
function injectAdminHeader(activePage) {
    if (!Auth.requireAuth()) return false;

    const user = Auth.getUser();
    const role = user?.role || '';

    const header = document.querySelector('.admin-header');
    if (!header) return true;

    const inner = header.querySelector('.header-inner');
    if (!inner) return true;

    // Permission-based nav visibility
    const navLinks = [
        { href: '/admin/index.html',    label: 'Daily View', key: 'daily',    perm: 'pages.daily' },
        { href: '/admin/bookings.html', label: 'Bookings',   key: 'bookings', perm: 'pages.bookings' },
        { href: '/admin/schedule.html', label: 'Schedule',   key: 'schedule', perm: 'pages.schedule' },
        { href: '/admin/services.html', label: 'Services',   key: 'services', perms: ['actions.serviceCatalog.edit', 'actions.serviceCatalog.manage'] },
        { href: '/admin/users.html',    label: 'Users',      key: 'users',    perm: 'pages.users' },
        { href: '/admin/roles.html',    label: 'Roles',      key: 'roles',    perm: 'pages.roles' },
    ];

    const navHtml = navLinks
        .filter(link => {
            if (link.perm) return Auth.can(link.perm);
            if (link.perms) return link.perms.some(p => Auth.can(p));
            return true;
        })
        .map(link => `<a href="${link.href}" class="nav-link${link.key === activePage ? ' active' : ''}">${link.label}</a>`)
        .join('');

    const fullName = user ? `${user.firstName} ${user.lastName}` : 'Unknown';
    const roleLabel = role || 'Unknown';

    inner.innerHTML = `
        <span class="brand">🚗 Vehicle Cleaning Admin</span>
        <nav class="main-nav">${navHtml}</nav>
        <div class="user-pill">
            <div class="user-pill-info">
                <span class="user-pill-name">${fullName}</span>
                <span class="user-pill-role">${roleLabel}</span>
            </div>
            <button class="logout-btn" onclick="Auth.logout()">Sign out</button>
        </div>
    `;

    return true;
}

// ===== API fetch wrapper with auth =====
async function apiFetch(url, options = {}) {
    const headers = {
        'Content-Type': 'application/json',
        ...Auth.authHeader(),
        ...(options.headers || {})
    };
    const res = await fetch(url, { ...options, headers });

    // If 401, session expired
    if (res.status === 401) {
        Auth.logout();
        throw new Error('Session expired');
    }

    return res;
}
