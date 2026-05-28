// portal-common.js — shared portal UI helpers, nav, real JWT auth

const portalNav = [
    { label: 'Dashboard', href: 'index.html' },
    { label: 'Bookings', href: 'bookings.html' },
    { label: 'Invoices', href: 'invoices.html' },
    { label: 'Profile', href: 'profile.html' },
];

function renderPortalNav() {
    const nav = document.getElementById('portal-nav');
    if (!nav) return;
    const currentPage = window.location.pathname.split('/').pop();
    let html = '<div class="nav-inner">';
    portalNav.forEach(item => {
        const active = location.pathname.endsWith(item.href) ? ' active' : '';
        html += `<a href="${item.href}" class="nav-tab${active}">${item.label}</a>`;
    });
    html += '</div>';
    nav.innerHTML = html;
}

const SESSION_KEY = 'portalSession';

function getSessionToken() {
    return localStorage.getItem(SESSION_KEY);
}

function decodeToken(token) {
    try {
        return JSON.parse(atob(token.split('.')[1]));
    } catch (e) {
        return null;
    }
}

function getClientId() {
    const token = getSessionToken();
    if (!token) return null;
    const payload = decodeToken(token);
    return payload ? parseInt(payload.client_id) : null;
}

function isLoggedIn() {
    const token = getSessionToken();
    if (!token) return false;
    const payload = decodeToken(token);
    if (!payload) return false;
    if (payload.auth_type !== 'portal') return false;
    const now = Math.floor(Date.now() / 1000);
    return payload.exp > now;
}

function logout() {
    localStorage.removeItem(SESSION_KEY);
    window.location.href = 'login.html';
}

function apiFetch(path, options) {
    var token = getSessionToken();
    if (!token) {
        logout();
        return Promise.reject('No token');
    }
    return fetch(path, {
        method: 'GET',
        headers: { 'Authorization': 'Bearer ' + token, 'Content-Type': 'application/json' },
        ...options
    }).then(function (r) { return r.json(); }).then(function (res) {
        if (!res.success) throw new Error(res.message || 'Request failed');
        return res.data;
    });
}

function formatCurrency(amount) {
    return '\u20AC' + Number(amount).toFixed(2);
}

function formatDate(dateStr) {
    var d = new Date(dateStr);
    return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function statusBadge(status) {
    var cls = status.toLowerCase();
    return '<span class="badge badge-' + cls + '">' + status + '</span>';
}

function formatTime(hour) {
    return String(hour).padStart(2, '0') + ':00';
}

document.addEventListener('DOMContentLoaded', function () {
    var params = new URLSearchParams(window.location.search);
    var sessionFromUrl = params.get('session');
    if (sessionFromUrl) {
        localStorage.setItem(SESSION_KEY, sessionFromUrl);
        window.location.replace(window.location.pathname);
        return;
    }

    if (!isLoggedIn() && !window.location.pathname.endsWith('login.html')) {
        window.location.href = 'login.html';
        return;
    }

    renderPortalNav();

    var payload = decodeToken(getSessionToken());
    if (!payload) return;

    var pillName = document.querySelector('.user-pill-name');
    var pillEmail = document.querySelector('.user-pill-email');
    if (pillName) pillName.textContent = payload.name || 'Client';
    if (pillEmail) pillEmail.textContent = payload.email || '';

    var logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) logoutBtn.addEventListener('click', logout);
});

function showError(message, containerId) {
    var container = document.getElementById(containerId || 'error-container');
    if (!container) return;
    container.innerHTML = '<div class="alert alert-danger">' + message + '</div>';
    setTimeout(function () { container.innerHTML = ''; }, 5000);
}

function showSuccess(message, containerId) {
    var container = document.getElementById(containerId || 'success-container');
    if (!container) return;
    container.innerHTML = '<div class="alert alert-success">' + message + '</div>';
    setTimeout(function () { container.innerHTML = ''; }, 3000);
}
