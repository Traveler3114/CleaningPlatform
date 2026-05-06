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

    // Redirect to login if not authenticated
    requireAuth() {
        if (!this.isLoggedIn()) {
            window.location.href = '/admin/login.html';
            return false;
        }
        return true;
    },

    // Redirect to login if not authenticated, or to daily view if insufficient role
    requireRole(...roles) {
        if (!this.isLoggedIn()) {
            window.location.href = '/admin/login.html';
            return false;
        }
        const role = this.getRole();
        if (!roles.map(r => r.toLowerCase()).includes(role?.toLowerCase())) {
            window.location.href = '/admin/index.html';
            return false;
        }
        return true;
    },

    authHeader() {
        const token = this.getToken();
        return token ? { 'Authorization': `Bearer ${token}` } : {};
    },

    async fetchMe() {
        // Decode JWT payload to get user info
        const token = this.getToken();
        if (!token) return null;
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return payload;
        } catch { return null; }
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

    // Role-based nav visibility
    const canSeeUsers = role.toLowerCase() === 'owner';

    const header = document.querySelector('.admin-header');
    if (!header) return true;

    const inner = header.querySelector('.header-inner');
    if (!inner) return true;

    // Rebuild nav with role-aware links
    const navLinks = [
        { href: '/admin/index.html', label: 'Daily View', key: 'daily', roles: null },
        { href: '/admin/bookings.html', label: 'Bookings', key: 'bookings', roles: null },
        { href: '/admin/schedule.html', label: 'Schedule', key: 'schedule', roles: ['owner', 'dispatcher'] },
        { href: '/admin/users.html', label: 'Users', key: 'users', roles: ['owner'] },
    ];

    const navHtml = navLinks
        .filter(link => !link.roles || link.roles.includes(role.toLowerCase()))
        .map(link => `<a href="${link.href}" class="nav-link${link.key === activePage ? ' active' : ''}">${link.label}</a>`)
        .join('');

    // User pill
    const fullName = user ? `${user.name} ${user.surname}` : 'Unknown';
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
