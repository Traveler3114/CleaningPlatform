// dashboard.js
let currentDate = new Date().toISOString().split('T')[0];
let dashboardData = null;

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
            renderBookings(bookings);
        } else {
            document.getElementById('bookings-table').innerHTML = `<div class="alert alert-info">${res.message}</div>`;
        }
    } catch(e) {
        document.getElementById('bookings-table').innerHTML = `<div class="alert alert-danger">Failed to load bookings.</div>`;
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
        document.getElementById('slots-table').innerHTML = `<div class="alert alert-danger">Failed to load slots.</div>`;
    }
}

function renderBookings(bookings) {
    if (!bookings.length) {
        document.getElementById('bookings-table').innerHTML = '<div class="alert alert-info">No bookings found for selected date.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>ID</th><th>Client</th><th>Time</th><th>Status</th><th>Assigned Employees</th></tr></thead><tbody>';
    bookings.forEach(b => {
        html += `<tr>
            <td><a href="booking-detail.html?id=${b.id}" class="link">#${b.id}</a></td>
            <td>${b.clientName}</td>
            <td>${b.hour}:00</td>
            <td><span class="badge badge-${b.status.toLowerCase()}">${b.status}</span></td>
            <td>${(b.assignedEmployees || []).map(e => e.fullName).join(', ') || '-'}</td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('bookings-table').innerHTML = html;
}

function renderSlots(slots) {
    if (!slots.length) {
        document.getElementById('slots-table').innerHTML = '<div class="alert alert-info">No slots available for this date.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>Hour</th><th>Capacity</th><th>Booked</th><th>Available</th><th>State</th></tr></thead><tbody>';
    slots.forEach(s => {
        const isFull = !s.isClosed && s.available <= 0;
        const state = s.isClosed ? 'Closed' : isFull ? 'Full' : 'Open';
        const stateClass = s.isClosed ? 'closed' : isFull ? 'full' : 'open';
        html += `<tr class="${s.isClosed ? 'row-closed' : isFull ? 'row-full' : ''}">
            <td>${s.hour}:00</td><td>${s.capacity}</td><td>${s.booked}</td><td>${s.available}</td>
            <td><span class="badge badge-${stateClass}">${state}</span></td>
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
    // Also we need pending/confirmed counts from current bookings? We can compute from loaded bookings.
    // For simplicity, we'll compute from bookings table.
    const bookingsTable = document.querySelector('#bookings-table table');
    let pending = 0, confirmed = 0;
    if (bookingsTable) {
        const rows = bookingsTable.querySelectorAll('tbody tr');
        rows.forEach(row => {
            const statusCell = row.cells[3];
            const status = statusCell.innerText.trim();
            if (status === 'Pending') pending++;
            if (status === 'Confirmed') confirmed++;
        });
    }
    const kpiHtml = `
        <div class="kpi-card"><span>Revenue MTD</span><strong>${mtd ? mtd.totalRevenue.toFixed(2) : '—'}</strong></div>
        <div class="kpi-card"><span>Overdue invoices</span><strong>${overdue ? overdue.totalOverdueAmount.toFixed(2) : '0'}</strong><small>${overdue ? overdue.overdueInvoiceCount : 0} invoices</small></div>
        <div class="kpi-card"><span>Completion rate</span><strong>${completion ? completion.completionRatePct.toFixed(1) : '0'}%</strong></div>
        <div class="kpi-card"><span>Top client</span><strong>${topClient ? topClient.clientName : '—'}</strong></div>
        <div class="kpi-card"><span>Pending today</span><strong>${pending}</strong></div>
        <div class="kpi-card"><span>Confirmed today</span><strong>${confirmed}</strong></div>
    `;
    document.getElementById('kpi-grid').innerHTML = kpiHtml;
}

document.getElementById('selected-date').addEventListener('change', (e) => {
    currentDate = e.target.value;
    loadDashboard();
});

// Initial load
document.getElementById('selected-date').value = currentDate;
loadDashboard();