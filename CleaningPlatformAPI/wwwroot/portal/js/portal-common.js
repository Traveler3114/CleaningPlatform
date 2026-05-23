// portal-common.js — shared portal UI helpers, nav, mock auth

const PORTAL_STORAGE_KEY = 'portal_client';

function getStoredClient() {
    const data = localStorage.getItem(PORTAL_STORAGE_KEY);
    return data ? JSON.parse(data) : null;
}

function setStoredClient(client) {
    localStorage.setItem(PORTAL_STORAGE_KEY, JSON.stringify(client));
}

function clearStoredClient() {
    localStorage.removeItem(PORTAL_STORAGE_KEY);
}

function isLoggedIn() {
    return !!getStoredClient();
}

function logout() {
    clearStoredClient();
    window.location.href = 'login.html';
}

document.addEventListener('DOMContentLoaded', () => {
    if (!isLoggedIn() && !window.location.pathname.endsWith('login.html')) {
        window.location.href = 'login.html';
        return;
    }

    const client = getStoredClient();
    if (!client) return;

    const pillName = document.querySelector('.user-pill-name');
    const pillEmail = document.querySelector('.user-pill-email');
    if (pillName) pillName.textContent = client.name;
    if (pillEmail) pillEmail.textContent = client.email;

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
