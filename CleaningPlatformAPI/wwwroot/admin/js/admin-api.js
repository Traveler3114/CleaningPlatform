// admin-api.js – shared API client with JWT
const API_BASE = '/api';

function getToken() {
    return localStorage.getItem('accessToken');
}

function setToken(token) {
    if (token) localStorage.setItem('accessToken', token);
    else localStorage.removeItem('accessToken');
}

function isAuthenticated() {
    return !!getToken();
}

function logout() {
    localStorage.removeItem('accessToken');
    window.location.href = 'login.html';
}

function translateApiError(code, fallback) {
    if (code && window.ERROR_CODE_MAP && window.ERROR_CODE_MAP[code]) {
        var key = window.ERROR_CODE_MAP[code];
        var t = window.__(key);
        if (t !== key) return t;
    }
    return fallback || 'Request failed.';
}

async function apiFetch(endpoint, options = {}) {
    const token = getToken();
    const headers = {
        'Content-Type': 'application/json',
        ...(options.headers || {})
    };
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    const response = await fetch(`${API_BASE}${endpoint}`, {
        ...options,
        headers
    });

    // Handle 401 Unauthorized
    if (response.status === 401) {
        logout();
        throw new Error('Session expired. Please login again.');
    }

    // Handle 403 Forbidden
    if (response.status === 403) {
        throw new Error('Access denied. You do not have permission for this action.');
    }

    // For file downloads, return raw response
    if (options.download) return response;

    let data;
    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
        data = await response.json();
    } else {
        const text = await response.text();
        if (!response.ok) throw new Error(`Request failed (HTTP ${response.status})`);
        throw new Error('Invalid server response.');
    }

    if (!response.ok) {
        const code = data && data.code || data && data.title;
        const detail = data && data.detail || `Request failed (HTTP ${response.status})`;
        const errorMsg = translateApiError(code, detail);
        var err = new Error(errorMsg);
        err.code = code;
        throw err;
    }
    return data; // Direct response (no envelope — errors use ProblemDetails)
}

// Helper to check permissions from token claims (simplified – you may need to decode JWT)
async function getUserPermissions() {
    // Decode JWT payload (assuming it contains "permission" claims)
    const token = getToken();
    if (!token) return [];
    try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        // Owner gets all permissions (backend doesn't add individual permission claims for owner)
        if (payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] === 'Owner') {
            // Return a special marker or fetch all permissions from somewhere?
            // For simplicity, we'll treat Owner as having everything.
            return ['*'];
        }
        const perms = payload['permission'] || [];
        return Array.isArray(perms) ? perms : [perms];
    } catch (e) {
        return [];
    }
}

function hasPermission(permission) {
    // This would need to be async if we decode token each time – but we can cache.
    // We'll implement a cache after login.
    return window._userPermissions ? window._userPermissions.includes('*') || window._userPermissions.includes(permission) : false;
}

async function loadCurrentUser() {
    return await apiFetch('/employees/me');
}