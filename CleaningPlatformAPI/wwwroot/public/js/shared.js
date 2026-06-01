// shared.js – common functions for all pages

const publicNav = [
    { labelKey: 'nav_services', label: 'Services', href: '/public/services.html' },
    { labelKey: 'nav_get_quote', label: 'Get a Quote', href: '/public/book.html', cta: true },
    { labelKey: 'nav_sign_in', label: 'Sign In', href: '/portal/login.html' },
];

function renderPublicNav() {
    const nav = document.getElementById('site-nav');
    if (!nav) return;
    let html = '';
    publicNav.forEach(item => {
        const cls = item.cta ? 'nav-link nav-cta' : 'nav-link';
        html += `<a href="${item.href}" class="${cls}">${window.__(item.labelKey) || item.label}</a>`;
    });
    nav.innerHTML = html;
}

// ===== Mobile nav toggle =====
document.addEventListener('DOMContentLoaded', () => {
    renderPublicNav();
    const toggle = document.getElementById('menu-toggle');
    const nav = document.getElementById('site-nav');
    if (toggle && nav) {
        toggle.addEventListener('click', () => nav.classList.toggle('open'));
        nav.querySelectorAll('a').forEach(a => a.addEventListener('click', () => nav.classList.remove('open')));
    }
});

window.addEventListener('i18nReady', function () {
    var nav = document.getElementById('site-nav');
    if (nav) renderPublicNav();
});

// ===== Date & time formatting (used by booking.js) =====
function formatDate(date) {
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
}

function formatHour(h) {
    const ampm = h >= 12 ? 'PM' : 'AM';
    const hour = h % 12 === 0 ? 12 : h % 12;
    return `${hour}:00 ${ampm}`;
}

function formatDateLong(date) {
    return date.toLocaleDateString('en-US', {
        weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
    });
}