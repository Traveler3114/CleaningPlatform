// ===== Customer Website App =====
const API_BASE = '';

// ── State ──────────────────────────────────────────────────
const state = {
    step: 1,
    selectedService: null,
    selectedDate: null,
    selectedHour: null,
    slots: [],
    customerName: '',
    phone: '',
    email: '',
    booking: null,
    services: [],
    activeCategory: 'all'
};

// ── Helpers ─────────────────────────────────────────────────
function formatDate(date) {
    const yyyy = date.getFullYear();
    const mm   = String(date.getMonth() + 1).padStart(2, '0');
    const dd   = String(date.getDate()).padStart(2, '0');
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

// ── Mobile nav toggle ────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    const toggle = document.getElementById('menu-toggle');
    const nav    = document.getElementById('site-nav');
    if (toggle && nav) {
        toggle.addEventListener('click', () => nav.classList.toggle('open'));
        nav.querySelectorAll('a').forEach(a => a.addEventListener('click', () => nav.classList.remove('open')));
    }
});

// ── Services ─────────────────────────────────────────────────
async function loadServices() {
    const grid = document.getElementById('services-grid');
    if (!grid) return;

    grid.innerHTML = '<p class="services-loading"><span class="spinner"></span> Loading services…</p>';

    try {
        const res    = await fetch(`${API_BASE}/api/services`);
        const result = await res.json();

        if (!result.success || !result.data || result.data.length === 0) {
            grid.innerHTML = '<p class="services-loading">No services available at the moment.</p>';
            return;
        }

        state.services = result.data.filter(s => s.isActive);
        renderCategoryFilters();
        renderServiceCards('all');
    } catch (e) {
        grid.innerHTML = '<p class="services-loading">Could not load services. Please refresh.</p>';
    }
}

function renderCategoryFilters() {
    const bar = document.getElementById('category-filter');
    if (!bar) return;

    const cats = ['all', ...new Set(state.services.map(s => s.category).filter(Boolean))];

    bar.innerHTML = cats.map(cat => `
        <button class="cat-btn ${cat === 'all' ? 'active' : ''}"
                data-cat="${cat}"
                onclick="filterCategory('${cat}')">
            ${cat === 'all' ? 'All Services' : cat}
        </button>
    `).join('');
}

function filterCategory(cat) {
    state.activeCategory = cat;
    document.querySelectorAll('.cat-btn').forEach(b => {
        b.classList.toggle('active', b.dataset.cat === cat);
    });
    renderServiceCards(cat);
}

function renderServiceCards(cat) {
    const grid = document.getElementById('services-grid');
    if (!grid) return;

    const filtered = cat === 'all'
        ? state.services
        : state.services.filter(s => s.category === cat);

    if (filtered.length === 0) {
        grid.innerHTML = '<p class="services-loading">No services in this category.</p>';
        return;
    }

    grid.innerHTML = filtered.map(s => {
        const priceLabel = s.priceMin && s.priceMax
            ? `<strong>${s.priceMin}–${s.priceMax} €</strong> / ${s.unit || 'per service'}`
            : s.priceAvg
            ? `<strong>from ${s.priceAvg} €</strong> / ${s.unit || 'per service'}`
            : '';

        return `
            <div class="service-card" data-id="${s.id}" onclick="selectServiceFromCard(${s.id})">
                ${s.category ? `<div class="service-category-badge">${s.category}</div>` : ''}
                <h3>${s.name}</h3>
                ${priceLabel ? `<p class="service-price">${priceLabel}</p>` : ''}
            </div>
        `;
    }).join('');
}

function selectServiceFromCard(id) {
    const service = state.services.find(s => s.id === id);
    if (!service) return;

    state.selectedService = service;

    // Highlight card
    document.querySelectorAll('.service-card').forEach(c => {
        c.classList.toggle('selected', parseInt(c.dataset.id) === id);
    });

    // Scroll to booking
    document.getElementById('booking').scrollIntoView({ behavior: 'smooth', block: 'start' });

    // Pre-populate service in booking step 1 and advance to step 2
    setTimeout(() => {
        goToStep(2);
    }, 400);
}

// ── Booking Steps ────────────────────────────────────────────
function updateStepUI() {
    const panels = document.querySelectorAll('.step-panel');
    const dots   = document.querySelectorAll('.progress-step');

    panels.forEach((p, i) => {
        p.classList.toggle('active', i + 1 === state.step);
    });

    dots.forEach((d, i) => {
        d.classList.remove('active', 'done');
        if (i + 1 === state.step) d.classList.add('active');
        if (i + 1 < state.step)  d.classList.add('done');
    });

    // Sync step 1 service display
    if (state.step === 1) renderBookingServiceList();
    if (state.step === 2) renderDateStep();
    if (state.step === 3) renderDetailsStep();
    if (state.step === 4) renderConfirmation();
}

function goToStep(step) {
    state.step = step;
    updateStepUI();

    const bookingEl = document.getElementById('booking');
    if (bookingEl) bookingEl.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

// ── Step 1: Service Selection ────────────────────────────────
function renderBookingServiceList() {
    const container = document.getElementById('booking-service-list');
    if (!container) return;

    if (state.services.length === 0) {
        container.innerHTML = '<p class="info-msg"><span class="spinner"></span> Loading services…</p>';
        return;
    }

    container.innerHTML = state.services.map(s => {
        const selected = state.selectedService && state.selectedService.id === s.id;
        const priceLabel = s.priceMin && s.priceMax
            ? `${s.priceMin}–${s.priceMax} €`
            : s.priceAvg ? `from ${s.priceAvg} €` : '';

        return `
            <div class="service-card ${selected ? 'selected' : ''}"
                 data-booking-service-id="${s.id}"
                 onclick="selectBookingService(${s.id})"
                 style="margin-bottom:0.6rem;">
                ${s.category ? `<div class="service-category-badge">${s.category}</div>` : ''}
                <h3>${s.name}</h3>
                ${priceLabel ? `<p class="service-price"><strong>${priceLabel}</strong>${s.unit ? ' / ' + s.unit : ''}</p>` : ''}
            </div>
        `;
    }).join('');

    updateStep1Next();
}

function selectBookingService(id) {
    const service = state.services.find(s => s.id === id);
    if (!service) return;
    state.selectedService = service;

    document.querySelectorAll('[data-booking-service-id]').forEach(c => {
        c.classList.toggle('selected', parseInt(c.dataset.bookingServiceId) === id);
    });

    updateStep1Next();
}

function updateStep1Next() {
    const btn = document.getElementById('step1-next');
    if (btn) btn.disabled = !state.selectedService;
}

// ── Step 2: Date & Time ──────────────────────────────────────
function renderDateStep() {
    initDatePicker();
    if (state.selectedDate) loadSlots(state.selectedDate);
}

function initDatePicker() {
    const input = document.getElementById('date-input');
    if (!input) return;

    const today = new Date();
    const minDate = formatDate(today);
    input.min = minDate;

    if (!state.selectedDate) {
        input.value = minDate;
        state.selectedDate = new Date(minDate + 'T00:00:00');
    } else {
        input.value = formatDate(state.selectedDate);
    }

    input.onchange = () => {
        if (!input.value) return;
        state.selectedDate = new Date(input.value + 'T00:00:00');
        state.selectedHour = null;
        updateStep2Next();
        loadSlots(state.selectedDate);
    };
}

async function loadSlots(date) {
    const container = document.getElementById('slots-container');
    if (!container) return;

    container.innerHTML = '<p class="info-msg"><span class="spinner"></span> Loading available times…</p>';
    updateStep2Next();

    const dateStr = formatDate(date);
    try {
        const res    = await fetch(`${API_BASE}/api/availability?date=${dateStr}`);
        const result = await res.json();

        if (!result.success || !result.data || result.data.length === 0) {
            container.innerHTML = '<p class="info-msg">No available slots for this date. Please choose another day.</p>';
            return;
        }

        state.slots = result.data;
        renderSlots(date);
    } catch (e) {
        container.innerHTML = '<p class="error-msg">Error loading available times. Please try again.</p>';
    }
}

function renderSlots(date) {
    const container = document.getElementById('slots-container');
    if (!container) return;

    const now      = new Date();
    const isToday  = formatDate(date) === formatDate(now);
    const currHour = now.getHours();

    const available = state.slots.filter(s => {
        const open = !s.isClosed && s.available > 0;
        return isToday ? open && s.hour > currHour : open;
    });

    if (available.length === 0) {
        container.innerHTML = '<p class="info-msg">No open slots for this date. Please choose another day.</p>';
        return;
    }

    container.innerHTML = '<div class="slots-grid"></div>';
    const grid = container.querySelector('.slots-grid');

    available.forEach(slot => {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = `slot-btn ${state.selectedHour === slot.hour ? 'selected' : ''}`;
        btn.innerHTML = `
            <div class="slot-time">${formatHour(slot.hour)}</div>
            <div class="slot-avail">${slot.available} spot${slot.available !== 1 ? 's' : ''} left</div>
        `;
        btn.addEventListener('click', () => {
            state.selectedHour = slot.hour;
            grid.querySelectorAll('.slot-btn').forEach(b => b.classList.remove('selected'));
            btn.classList.add('selected');
            updateStep2Next();
        });
        grid.appendChild(btn);
    });
}

function updateStep2Next() {
    const btn = document.getElementById('step2-next');
    if (btn) btn.disabled = state.selectedHour === null;
}

// ── Step 3: Customer Details ─────────────────────────────────
function renderDetailsStep() {
    // Show summary of what's been selected
    const summary = document.getElementById('step3-summary');
    if (summary && state.selectedService && state.selectedDate && state.selectedHour !== null) {
        summary.innerHTML = `
            <div class="summary-row">
                <span>Service</span>
                <span>${state.selectedService.name}</span>
            </div>
            <div class="summary-row">
                <span>Date</span>
                <span>${formatDateLong(state.selectedDate)}</span>
            </div>
            <div class="summary-row">
                <span>Time</span>
                <span>${formatHour(state.selectedHour)}</span>
            </div>
        `;
    }

    // Restore values if coming back
    const nameInput  = document.getElementById('customer-name');
    const phoneInput = document.getElementById('customer-phone');
    const emailInput = document.getElementById('customer-email');
    if (nameInput  && state.customerName) nameInput.value  = state.customerName;
    if (phoneInput && state.phone)        phoneInput.value = state.phone;
    if (emailInput && state.email)        emailInput.value = state.email;
}

// ── Step 4: Confirmation ─────────────────────────────────────
function renderConfirmation() {
    const el = document.getElementById('confirmation-details');
    if (!el) return;

    el.innerHTML = `
        <div class="summary-row">
            <span>Booking ID</span>
            <span>#${state.booking?.id || 'N/A'}</span>
        </div>
        <div class="summary-row">
            <span>Service</span>
            <span>${state.selectedService?.name || '—'}</span>
        </div>
        <div class="summary-row">
            <span>Date</span>
            <span>${state.selectedDate ? formatDateLong(state.selectedDate) : '—'}</span>
        </div>
        <div class="summary-row">
            <span>Time</span>
            <span>${state.selectedHour !== null ? formatHour(state.selectedHour) : '—'}</span>
        </div>
        <div class="summary-row">
            <span>Name</span>
            <span>${state.customerName}</span>
        </div>
        <div class="summary-row">
            <span>Phone</span>
            <span>${state.phone}</span>
        </div>
        ${state.email ? `
        <div class="summary-row">
            <span>Email</span>
            <span>${state.email}</span>
        </div>` : ''}
        <div class="summary-row">
            <span>Status</span>
            <span>${state.booking?.status || 'Pending'}</span>
        </div>
    `;
}

// ── Submit Booking ───────────────────────────────────────────
async function submitBooking() {
    const errorEl  = document.getElementById('step3-error');
    const submitBtn = document.getElementById('step3-submit');

    errorEl.textContent = '';

    const name  = document.getElementById('customer-name').value.trim();
    const phone = document.getElementById('customer-phone').value.trim();
    const email = document.getElementById('customer-email').value.trim();

    if (!name)  { errorEl.textContent = 'Please enter your full name.'; return; }
    if (!phone) { errorEl.textContent = 'Please enter your phone number.'; return; }

    state.customerName = name;
    state.phone        = phone;
    state.email        = email;

    submitBtn.disabled    = true;
    submitBtn.textContent = 'Booking…';

    const payload = {
        customerName: state.customerName,
        phone:        state.phone,
        date:         formatDate(state.selectedDate),
        hour:         state.selectedHour
    };

    try {
        const res    = await fetch(`${API_BASE}/api/bookings`, {
            method:  'POST',
            headers: { 'Content-Type': 'application/json' },
            body:    JSON.stringify(payload)
        });
        const result = await res.json();

        if (result.success) {
            state.booking = result.data;
            goToStep(4);
        } else {
            errorEl.textContent = result.message || 'Booking failed. Please try again.';
        }
    } catch (e) {
        errorEl.textContent = 'Network error. Please try again.';
    } finally {
        submitBtn.disabled    = false;
        submitBtn.textContent = 'Confirm Booking';
    }
}

// ── Reset ─────────────────────────────────────────────────────
function resetBooking() {
    state.step            = 1;
    state.selectedService = null;
    state.selectedDate    = null;
    state.selectedHour    = null;
    state.slots           = [];
    state.customerName    = '';
    state.phone           = '';
    state.email           = '';
    state.booking         = null;

    const fields = ['customer-name', 'customer-phone', 'customer-email'];
    fields.forEach(id => { const el = document.getElementById(id); if (el) el.value = ''; });

    updateStepUI();
}

// ── Wire up DOM events ────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    // Load services
    loadServices();
    updateStepUI();

    // Step 1 next
    const step1Next = document.getElementById('step1-next');
    if (step1Next) step1Next.addEventListener('click', () => goToStep(2));

    // Step 2 nav
    const step2Back = document.getElementById('step2-back');
    const step2Next = document.getElementById('step2-next');
    if (step2Back) step2Back.addEventListener('click', () => goToStep(1));
    if (step2Next) step2Next.addEventListener('click', () => goToStep(3));

    // Step 3 submit
    const step3Back   = document.getElementById('step3-back');
    const step3Submit = document.getElementById('step3-submit');
    if (step3Back)   step3Back.addEventListener('click', () => goToStep(2));
    if (step3Submit) step3Submit.addEventListener('click', submitBooking);

    // Restart
    const restartBtn = document.getElementById('restart-btn');
    if (restartBtn) restartBtn.addEventListener('click', resetBooking);
});
