// portal-common.js — shared portal UI helpers, nav, real JWT auth

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

document.addEventListener('DOMContentLoaded', () => {
    // Handle ?session=... in URL (fallback from auth flow)
    const params = new URLSearchParams(window.location.search);
    const sessionFromUrl = params.get('session');
    if (sessionFromUrl) {
        localStorage.setItem(SESSION_KEY, sessionFromUrl);
        window.location.replace(window.location.pathname);
        return;
    }

    if (!isLoggedIn() && !window.location.pathname.endsWith('login.html')) {
        window.location.href = 'login.html';
        return;
    }

    const payload = decodeToken(getSessionToken());
    if (!payload) return;

    const pillName = document.querySelector('.user-pill-name');
    const pillEmail = document.querySelector('.user-pill-email');
    if (pillName) pillName.textContent = payload.name || 'Client';
    if (pillEmail) pillEmail.textContent = payload.email || '';

    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) logoutBtn.addEventListener('click', logout);

    const currentPage = window.location.pathname.split('/').pop();
    document.querySelectorAll('.nav-tab').forEach(tab => {
        const href = tab.getAttribute('href');
        if (href && href === currentPage) tab.classList.add('active');
    });
});

function showError(message, containerId) {
    const container = document.getElementById(containerId || 'error-container');
    if (!container) return;
    container.innerHTML = `<div class="alert alert-danger">${message}</div>`;
    setTimeout(() => container.innerHTML = '', 5000);
}

function showSuccess(message, containerId) {
    const container = document.getElementById(containerId || 'success-container');
    if (!container) return;
    container.innerHTML = `<div class="alert alert-success">${message}</div>`;
    setTimeout(() => container.innerHTML = '', 3000);
}
