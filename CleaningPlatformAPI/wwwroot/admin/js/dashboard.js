// dashboard.js
let currentDate = new Date().toISOString().split('T')[0];
let dashboardData = null;
let _allBookings = [];

function getUserRole() {
    const token = getToken();
    if (!token) return null;
    try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        return payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    } catch { return null; }
}

async function loadDashboard() {
    try {
        const summaryRes = await apiFetch('/reports/dashboard');
        if (summaryRes.success) dashboardData = summaryRes.data;
        else console.error(summaryRes.message);
    } catch(e) { console.error(e); }
    
    await loadBookings();
    await loadSlots();
    renderKPIs();
}

async function loadBookings() {
    try {
        const isEmployee = getUserRole() === 'Employee';
        const endpoint = isEmployee
            ? `/bookings/employee/assigned?date=${currentDate}`
            : `/bookings?date=${currentDate}`;
        const res = await apiFetch(endpoint);
        if (res.success) {
            const bookings = res.data || [];
            _allBookings = bookings;
            renderBookings(bookings);
        } else {
            document.getElementById('bookings-table').innerHTML = `<div class="alert alert-info">${res.message}</div>`;
        }
    } catch(e) {
        document.getElementById('bookings-table').innerHTML = `<div class="alert alert-danger">${__('msg_failed_load_bookings')}</div>`;
    }
}

async function loadSlots() {
    try {
        const res = await apiFetch(`/availability?date=${currentDate}`);
        if (res.success) {
            const slots = res.data || [];
            renderSlots(slots);
        } else {
            document.getElementById('slots-table').innerHTML = `<div class="alert alert-info">${res.message}</div>`;
        }
    } catch(e) {
        document.getElementById('slots-table').innerHTML = `<div class="alert alert-danger">${__('msg_failed_load_slots')}</div>`;
    }
}

function renderBookings(bookings) {
    if (!bookings.length) {
        document.getElementById('bookings-table').innerHTML = `<div class="alert alert-info">${__('empty_no_bookings_date')}</div>`;
        return;
    }
    let html = `<table class="admin-table"><thead><tr><th>${__('th_id')}</th><th>${__('th_client')}</th><th>${__('th_time')}</th><th>${__('th_status')}</th><th>${__('label_assigned_employees')}</th></tr></thead><tbody>`;
    bookings.forEach(b => {
        html += `<tr>
            <td><a href="booking-detail.html?id=${b.id}" class="link">#${b.id}</a></td>
            <td>${b.clientName}</td>
            <td>${b.hour}:00</td>
            <td>${statusBadge(b.status)}</td>
            <td>${(b.assignedEmployees || []).map(e => e.fullName).join(', ') || '-'}</td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('bookings-table').innerHTML = html;
}

function renderSlots(slots) {
    if (!slots.length) {
        document.getElementById('slots-table').innerHTML = `<div class="alert alert-info">${__('empty_no_slots_date')}</div>`;
        return;
    }
    let html = `<table class="admin-table"><thead><tr><th>${__('label_hour')}</th><th>${__('label_capacity')}</th><th>${__('label_booked')}</th><th>${__('label_available')}</th><th>${__('label_state')}</th></tr></thead><tbody>`;
    slots.forEach(s => {
        const isFull = !s.isClosed && s.available <= 0;
        const state = s.isClosed ? 'Closed' : isFull ? 'Full' : 'Open';
        const stateClass = s.isClosed ? 'closed' : isFull ? 'full' : 'open';
        html += `<tr class="${s.isClosed ? 'row-closed' : isFull ? 'row-full' : ''}">
            <td>${s.hour}:00</td><td>${s.capacity}</td><td>${s.booked}</td><td>${s.available}</td>
            <td><span class="badge badge-${stateClass}">${__status(state)}</span></td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('slots-table').innerHTML = html;
}

function renderKPIs() {
    if (!dashboardData) return;
    const mtd = dashboardData.monthlyRevenue;
    const overdue = dashboardData.overdueInvoices;
    const completion = dashboardData.completionRate;
    const topClient = dashboardData.topClient;
    let pending = _allBookings.filter(function (b) { return b.status === 'Pending'; }).length;
    const kpiHtml = `
        <div class="kpi-card"><span>${__('kpi_revenue_mtd')}</span><strong>${mtd ? mtd.totalRevenue.toFixed(2) : '—'}</strong></div>
        <div class="kpi-card"><span>${__('kpi_overdue_invoices')}</span><strong>${overdue ? overdue.totalOverdueAmount.toFixed(2) : '0'}</strong><small>${overdue ? overdue.overdueInvoiceCount : 0} ${__('nav_invoices')}</small></div>
        <div class="kpi-card"><span>${__('kpi_completion_rate')}</span><strong>${completion ? completion.completionRatePct.toFixed(1) : '0'}%</strong></div>
        <div class="kpi-card"><span>${__('kpi_top_client')}</span><strong>${topClient ? topClient.clientName : '—'}</strong></div>
        <div class="kpi-card"><span>${__('kpi_pending_today')}</span><strong>${pending}</strong></div>
    `;
    document.getElementById('kpi-grid').innerHTML = kpiHtml;
}

document.getElementById('selected-date').addEventListener('change', (e) => {
    currentDate = e.target.value;
    loadDashboard();
});

document.getElementById('selected-date').value = currentDate;
loadDashboard();
window.addEventListener('i18nReady', function () { loadDashboard(); });
