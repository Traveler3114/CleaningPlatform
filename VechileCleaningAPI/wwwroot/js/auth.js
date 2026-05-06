// ===== Admin Auth Guard =====
// Include this script on every admin page BEFORE any API calls are made.
// It checks for a valid token and redirects to login if missing.

(function () {
    const token = localStorage.getItem('auth_token');
    if (!token) {
        window.location.href = '/admin/login.html';
    }
})();

// Authenticated fetch wrapper — automatically attaches Bearer token to all requests
async function authFetch(url, options = {}) {
    const token = localStorage.getItem('auth_token');
    const headers = {
        'Content-Type': 'application/json',
        ...(options.headers || {}),
        ...(token ? { 'Authorization': `Bearer ${token}` } : {})
    };
    const res = await fetch(url, { ...options, headers });

    // If 401, token expired or invalid — send to login
    if (res.status === 401) {
        localStorage.removeItem('auth_token');
        window.location.href = '/admin/login.html';
        return;
    }
    return res;
}

// Logout — clears token and redirects to login
function logout() {
    localStorage.removeItem('auth_token');
    window.location.href = '/admin/login.html';
}
