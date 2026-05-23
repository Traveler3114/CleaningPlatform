// booking.js
const API_BASE = '';

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
    services: []
};

function getQueryParam(param) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(param);
}

async function loadServices() {
    const container = document.getElementById('booking-service-list');
    if (!container) return;
    container.innerHTML = '<p class="info-msg"><span class="spinner"></span> Loading services…</p>';
    try {
        const res = await fetch(`${API_BASE}/api/services`);
        const result = await res.json();
        if (!result.success || !result.data || result.data.length === 0) {
            container.innerHTML = '<p class="info-msg">No services available.</p>';
            return;
        }
        state.services = result.data.filter(s => s.isActive);
        renderBookingServiceList();

        // Pre-select service from URL param
        const serviceIdParam = getQueryParam('serviceId');
        if (serviceIdParam) {
            const preselected = state.services.find(s => s.id == serviceIdParam);
            if (preselected) {
                state.selectedService = preselected;
                renderBookingServiceList(); // re-render with selected highlight
                updateStep1Next();
                // optionally auto-advance to step 2
                setTimeout(() => goToStep(2), 400);
            }
        }
    } catch (e) {
        container.innerHTML = '<p class="error-msg">Could not load services. Please refresh.</p>';
    }
}

function renderBookingServiceList() {
    const container = document.getElementById('booking-service-list');
    if (!container) return;
    container.innerHTML = state.services.map(s => {
        const selected = state.selectedService && state.selectedService.id === s.id;
        const priceLabel = s.priceMin && s.priceMax ? `${s.priceMin}–${s.priceMax} €` : (s.priceAvg ? `from ${s.priceAvg} €` : '');
        return `
            <div class="service-card ${selected ? 'selected' : ''}" data-booking-service-id="${s.id}" onclick="selectBookingService(${s.id})" style="margin-bottom:0.6rem;">
                ${s.category ? `<div class="service-category-badge">${s.category}</div>` : ''}
                <h3>${s.name}</h3>
                ${priceLabel ? `<p class="service-price"><strong>${priceLabel}</strong>${s.unit ? ' / ' + s.unit : ''}</p>` : ''}
            </div>
        `;
    }).join('');
}

window.selectBookingService = function(id) {
    const service = state.services.find(s => s.id === id);
    if (!service) return;
    state.selectedService = service;
    renderBookingServiceList();
    updateStep1Next();
};

function updateStep1Next() {
    const btn = document.getElementById('step1-next');
    if (btn) btn.disabled = !state.selectedService;
}

// === Step 2: Date & Time ===
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
        const res = await fetch(`${API_BASE}/api/availability?date=${dateStr}`);
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
    const now = new Date();
    const isToday = formatDate(date) === formatDate(now);
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
        btn.innerHTML = `<div class="slot-time">${formatHour(slot.hour)}</div><div class="slot-avail">${slot.available} spot${slot.available !== 1 ? 's' : ''} left</div>`;
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

// === Step 3: Customer Details ===
function renderDetailsStep() {
    const summary = document.getElementById('step3-summary');
    if (summary && state.selectedService && state.selectedDate && state.selectedHour !== null) {
        summary.innerHTML = `
            <div class="summary-row"><span>Service</span><span>${state.selectedService.name}</span></div>
            <div class="summary-row"><span>Date</span><span>${formatDateLong(state.selectedDate)}</span></div>
            <div class="summary-row"><span>Time</span><span>${formatHour(state.selectedHour)}</span></div>
        `;
    }
    document.getElementById('customer-name').value = state.customerName;
    document.getElementById('customer-phone').value = state.phone;
    document.getElementById('customer-email').value = state.email;
}

// === Step 4: Confirmation ===
function renderConfirmation() {
    const el = document.getElementById('confirmation-details');
    if (!el) return;
    el.innerHTML = `
        <div class="summary-row"><span>Booking ID</span><span>#${state.booking?.id || 'N/A'}</span></div>
        <div class="summary-row"><span>Service</span><span>${state.selectedService?.name || '—'}</span></div>
        <div class="summary-row"><span>Date</span><span>${state.selectedDate ? formatDateLong(state.selectedDate) : '—'}</span></div>
        <div class="summary-row"><span>Time</span><span>${state.selectedHour !== null ? formatHour(state.selectedHour) : '—'}</span></div>
        <div class="summary-row"><span>Name</span><span>${state.customerName}</span></div>
        <div class="summary-row"><span>Phone</span><span>${state.phone}</span></div>
        ${state.email ? `<div class="summary-row"><span>Email</span><span>${state.email}</span></div>` : ''}
        <div class="summary-row"><span>Status</span><span>${state.booking?.status || 'Pending'}</span></div>
    `;
}

async function submitBooking() {
    const errorEl = document.getElementById('step3-error');
    const submitBtn = document.getElementById('step3-submit');
    errorEl.textContent = '';
    const name = document.getElementById('customer-name').value.trim();
    const phone = document.getElementById('customer-phone').value.trim();
    const email = document.getElementById('customer-email').value.trim();
    if (!name) { errorEl.textContent = 'Please enter your full name.'; return; }
    if (!phone) { errorEl.textContent = 'Please enter your phone number.'; return; }
    state.customerName = name;
    state.phone = phone;
    state.email = email;
    submitBtn.disabled = true;
    submitBtn.textContent = 'Booking…';
    const payload = {
        customerName: state.customerName,
        phone: state.phone,
        date: formatDate(state.selectedDate),
        hour: state.selectedHour,
        serviceCatalogId: state.selectedService ? state.selectedService.id : 0
    };
    try {
        const res = await fetch(`${API_BASE}/api/bookings`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
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
        submitBtn.disabled = false;
        submitBtn.textContent = 'Confirm Booking';
    }
}

function resetBooking() {
    state.step = 1;
    state.selectedService = null;
    state.selectedDate = null;
    state.selectedHour = null;
    state.slots = [];
    state.customerName = '';
    state.phone = '';
    state.email = '';
    state.booking = null;
    ['customer-name', 'customer-phone', 'customer-email'].forEach(id => { const el = document.getElementById(id); if (el) el.value = ''; });
    updateStepUI();
    // clear URL param
    window.history.replaceState({}, document.title, window.location.pathname);
}

// === Step navigation & UI ===
function updateStepUI() {
    const panels = document.querySelectorAll('.step-panel');
    const dots = document.querySelectorAll('.progress-step');
    panels.forEach((p, i) => p.classList.toggle('active', i + 1 === state.step));
    dots.forEach((d, i) => {
        d.classList.remove('active', 'done');
        if (i + 1 === state.step) d.classList.add('active');
        if (i + 1 < state.step) d.classList.add('done');
    });
    if (state.step === 1) renderBookingServiceList();
    if (state.step === 2) renderDateStep();
    if (state.step === 3) renderDetailsStep();
    if (state.step === 4) renderConfirmation();
}

function goToStep(step) {
    state.step = step;
    updateStepUI();
    document.getElementById('booking').scrollIntoView({ behavior: 'smooth', block: 'start' });
}

// === Event wiring ===
document.addEventListener('DOMContentLoaded', () => {
    loadServices();
    updateStepUI();
    document.getElementById('step1-next').addEventListener('click', () => goToStep(2));
    document.getElementById('step2-back').addEventListener('click', () => goToStep(1));
    document.getElementById('step2-next').addEventListener('click', () => goToStep(3));
    document.getElementById('step3-back').addEventListener('click', () => goToStep(2));
    document.getElementById('step3-submit').addEventListener('click', submitBooking);
    document.getElementById('restart-btn').addEventListener('click', resetBooking);
});