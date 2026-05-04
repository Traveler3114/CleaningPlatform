// ===== Vehicle Cleaning Customer Booking App =====
// Config: update API_BASE to match your deployed API URL
const API_BASE = 'http://localhost:5098';

// State
const state = {
    step: 1,
    selectedDate: null,
    selectedHour: null,
    slots: [],
    customerName: '',
    phone: '',
    booking: null
};

// DOM refs
const stepsEl = document.querySelectorAll('.step');
const stepContents = document.querySelectorAll('.step-panel');

function formatDate(date) {
    // Returns YYYY-MM-DD
    return date.toISOString().split('T')[0];
}

function formatHour(h) {
    const ampm = h >= 12 ? 'PM' : 'AM';
    const hour = h % 12 === 0 ? 12 : h % 12;
    return `${hour}:00 ${ampm}`;
}

function updateStepUI() {
    stepsEl.forEach((el, i) => {
        el.classList.remove('active', 'done');
        if (i + 1 === state.step) el.classList.add('active');
        if (i + 1 < state.step) el.classList.add('done');
    });
    stepContents.forEach((el, i) => {
        el.style.display = (i + 1 === state.step) ? 'block' : 'none';
    });
}

// ===== STEP 1: Date Selection =====
function initStep1() {
    const dateInput = document.getElementById('date-input');
    const nextBtn = document.getElementById('step1-next');

    // Default to today
    const today = new Date();
    dateInput.value = formatDate(today);
    dateInput.min = formatDate(today);

    dateInput.addEventListener('change', () => {
        nextBtn.disabled = !dateInput.value;
    });

    nextBtn.addEventListener('click', () => {
        if (!dateInput.value) return;
        state.selectedDate = new Date(dateInput.value + 'T00:00:00');
        goToStep2();
    });
}

function goToStep2() {
    state.step = 2;
    updateStepUI();
    loadSlots();
}

// ===== STEP 2: Time Slot Selection =====
async function loadSlots() {
    const slotsContainer = document.getElementById('slots-container');
    const nextBtn = document.getElementById('step2-next');
    slotsContainer.innerHTML = '<div class="loading"><span class="spinner"></span> Loading available times...</div>';
    nextBtn.disabled = true;

    const dateStr = formatDate(state.selectedDate);
    try {
        const res = await fetch(`${API_BASE}/api/availability?date=${dateStr}`);
        const result = await res.json();
        if (!result.success || !result.data || result.data.length === 0) {
            slotsContainer.innerHTML = '<p class="no-slots">No available slots for this date. Please choose another day.</p>';
            return;
        }
        state.slots = result.data;
        renderSlots();
    } catch (e) {
        slotsContainer.innerHTML = '<p class="no-slots error-msg">Error loading slots. Please try again.</p>';
    }
}

function renderSlots() {
    const slotsContainer = document.getElementById('slots-container');
    const nextBtn = document.getElementById('step2-next');
    const openSlots = state.slots.filter(s => !s.isClosed);

    if (openSlots.length === 0) {
        slotsContainer.innerHTML = '<p class="no-slots">No open slots for this date.</p>';
        return;
    }

    slotsContainer.innerHTML = '<div class="slots-grid"></div>';
    const grid = slotsContainer.querySelector('.slots-grid');

    state.slots.forEach(slot => {
        const btn = document.createElement('button');
        btn.className = 'slot-btn';
        btn.disabled = slot.isClosed || slot.available <= 0;
        if (state.selectedHour === slot.hour) btn.classList.add('selected');

        let statusHtml = '';
        if (slot.isClosed) {
            statusHtml = `<div class="slot-closed">Closed</div>`;
        } else if (slot.available <= 0) {
            statusHtml = `<div class="slot-closed">Full</div>`;
        } else {
            statusHtml = `<div class="slot-avail">${slot.available} spot${slot.available !== 1 ? 's' : ''} left</div>`;
        }

        btn.innerHTML = `<div class="slot-time">${formatHour(slot.hour)}</div>${statusHtml}`;

        btn.addEventListener('click', () => {
            state.selectedHour = slot.hour;
            // Update selection UI
            grid.querySelectorAll('.slot-btn').forEach(b => b.classList.remove('selected'));
            btn.classList.add('selected');
            nextBtn.disabled = false;
        });
        grid.appendChild(btn);
    });
}

function initStep2() {
    const nextBtn = document.getElementById('step2-next');
    const backBtn = document.getElementById('step2-back');

    nextBtn.addEventListener('click', () => {
        if (state.selectedHour === null) return;
        goToStep3();
    });

    backBtn.addEventListener('click', () => {
        state.step = 1;
        state.selectedHour = null;
        updateStepUI();
    });
}

// ===== STEP 3: Customer Details =====
function goToStep3() {
    state.step = 3;
    updateStepUI();
    renderBookingSummary();
}

function renderBookingSummary() {
    const summaryEl = document.getElementById('booking-summary');
    const dateStr = state.selectedDate.toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
    summaryEl.innerHTML = `<strong>Date:</strong> ${dateStr} &nbsp;|&nbsp; <strong>Time:</strong> ${formatHour(state.selectedHour)}`;
}

function initStep3() {
    const form = document.getElementById('details-form');
    const backBtn = document.getElementById('step3-back');
    const errorEl = document.getElementById('step3-error');

    backBtn.addEventListener('click', () => {
        state.step = 2;
        updateStepUI();
    });

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        errorEl.textContent = '';
        const name = document.getElementById('customer-name').value.trim();
        const phone = document.getElementById('customer-phone').value.trim();
        if (!name || !phone) {
            errorEl.textContent = 'Please fill in all fields.';
            return;
        }
        state.customerName = name;
        state.phone = phone;
        await submitBooking();
    });
}

async function submitBooking() {
    const errorEl = document.getElementById('step3-error');
    const submitBtn = document.getElementById('step3-submit');
    submitBtn.disabled = true;
    submitBtn.textContent = 'Booking...';

    const payload = {
        customerName: state.customerName,
        phone: state.phone,
        date: formatDate(state.selectedDate),
        hour: state.selectedHour
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
            goToStep4();
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

// ===== STEP 4: Confirmation =====
function goToStep4() {
    state.step = 4;
    updateStepUI();
    renderConfirmation();
}

function renderConfirmation() {
    const detailsEl = document.getElementById('confirmation-details');
    const dateStr = state.selectedDate.toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
    detailsEl.innerHTML = `
        <p><strong>Name:</strong> ${state.customerName}</p>
        <p><strong>Phone:</strong> ${state.phone}</p>
        <p><strong>Date:</strong> ${dateStr}</p>
        <p><strong>Time:</strong> ${formatHour(state.selectedHour)}</p>
        <p><strong>Booking ID:</strong> #${state.booking?.id || 'N/A'}</p>
        <p><strong>Status:</strong> Reserved</p>
    `;
}

function initStep4() {
    const restartBtn = document.getElementById('restart-btn');
    restartBtn.addEventListener('click', () => {
        state.step = 1;
        state.selectedDate = null;
        state.selectedHour = null;
        state.slots = [];
        state.customerName = '';
        state.phone = '';
        state.booking = null;
        document.getElementById('customer-name').value = '';
        document.getElementById('customer-phone').value = '';
        updateStepUI();
    });
}

// ===== Init =====
document.addEventListener('DOMContentLoaded', () => {
    initStep1();
    initStep2();
    initStep3();
    initStep4();
    updateStepUI();
});
