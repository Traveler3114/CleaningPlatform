// bookings.js
let currentDateFilter = null;
let currentPage = 1;
let currentSearch = '';
let currentStatus = '';

async function loadBookings() {
    try {
        let url = `/bookings?page=${currentPage}&pageSize=50`;
        if (currentDateFilter) {
            url = `/bookings?date=${currentDateFilter}`;
            const res = await apiFetch(url);
            if (res.success) renderBookingsFlat(res.data || []);
            else showError(res.message);
            return;
        }
        if (currentSearch) url += `&search=${encodeURIComponent(currentSearch)}`;
        if (currentStatus) url += `&status=${encodeURIComponent(currentStatus)}`;
        const res = await apiFetch(url);
        if (res.success && res.data) {
            renderBookingsPaginated(res.data);
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function renderBookingsFlat(bookings) {
    if (!bookings.length) {
        document.getElementById('bookings-list').innerHTML = '<div class="alert alert-info">No bookings found.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>ID</th><th>Client</th><th>Date</th><th>Hour</th><th>Status</th><th>Update Status</th></tr></thead><tbody>';
    bookings.forEach(b => {
        const recurringBadge = b.recurringScheduleId ? '<span class="badge badge-info" title="Part of recurring schedule">↻</span> ' : '';
        html += `<tr class="booking-row" data-id="${b.id}" style="cursor:pointer;">
            <td><a href="booking-detail.html?id=${b.id}" class="link">${b.id}</a></td>
            <td><a href="client-detail.html?id=${b.clientId}" class="link">${recurringBadge}${b.clientName}</a></td>
            <td>${b.date.split('T')[0]}</td>
            <td>${b.hour}:00</td>
            <td><span class="badge badge-${b.status.toLowerCase()}">${b.status}</span></td>
            <td>
                <select class="status-select" data-id="${b.id}" onchange="updateBookingStatus(${b.id}, this.value)">
                    <option ${b.status === 'Pending' ? 'selected' : ''}>Pending</option>
                    <option ${b.status === 'Confirmed' ? 'selected' : ''}>Confirmed</option>
                    <option ${b.status === 'InProgress' ? 'selected' : ''}>InProgress</option>
                    <option ${b.status === 'Completed' ? 'selected' : ''}>Completed</option>
                    <option ${b.status === 'Cancelled' ? 'selected' : ''}>Cancelled</option>
                </select>
                <button class="btn btn-sm" onclick="updateBookingStatus(${b.id}, this.previousElementSibling.value)">Save</button>
            </td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('bookings-list').innerHTML = html;
}

function renderBookingsPaginated(pagedResult) {
    const bookings = pagedResult.items || [];
    if (!bookings.length) {
        document.getElementById('bookings-list').innerHTML = '<div class="alert alert-info">No bookings found.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>ID</th><th>Client</th><th>Date</th><th>Hour</th><th>Status</th><th>Update Status</th></tr></thead><tbody>';
    bookings.forEach(b => {
        const recurringBadge = b.recurringScheduleId ? '<span class="badge badge-info" title="Part of recurring schedule">↻</span> ' : '';
        html += `<tr class="booking-row" data-id="${b.id}" style="cursor:pointer;">
            <td><a href="booking-detail.html?id=${b.id}" class="link">${b.id}</a></td>
            <td><a href="client-detail.html?id=${b.clientId}" class="link">${recurringBadge}${b.clientName}</a></td>
            <td>${b.date.split('T')[0]}</td>
            <td>${b.hour}:00</td>
            <td><span class="badge badge-${b.status.toLowerCase()}">${b.status}</span></td>
            <td>
                <select class="status-select" data-id="${b.id}" onchange="updateBookingStatus(${b.id}, this.value)">
                    <option ${b.status === 'Pending' ? 'selected' : ''}>Pending</option>
                    <option ${b.status === 'Confirmed' ? 'selected' : ''}>Confirmed</option>
                    <option ${b.status === 'InProgress' ? 'selected' : ''}>InProgress</option>
                    <option ${b.status === 'Completed' ? 'selected' : ''}>Completed</option>
                    <option ${b.status === 'Cancelled' ? 'selected' : ''}>Cancelled</option>
                </select>
                <button class="btn btn-sm" onclick="updateBookingStatus(${b.id}, this.previousElementSibling.value)">Save</button>
            </td>
        </tr>`;
    });
    html += '</tbody></table>';
    if (pagedResult.totalPages > 1) {
        html += '<div class="pagination">';
        if (pagedResult.hasPreviousPage) html += `<button onclick="goToPage(${pagedResult.page - 1})" class="btn btn-sm">Previous</button>`;
        html += `<span>Page ${pagedResult.page} of ${pagedResult.totalPages}</span>`;
        if (pagedResult.hasNextPage) html += `<button onclick="goToPage(${pagedResult.page + 1})" class="btn btn-sm">Next</button>`;
        html += '</div>';
    }
    document.getElementById('bookings-list').innerHTML = html;
}

function goToPage(page) {
    currentPage = page;
    loadBookings();
}

async function updateBookingStatus(id, status) {
    try {
        const res = await apiFetch(`/bookings/${id}/status`, {
            method: 'PUT',
            body: JSON.stringify({ status })
        });
        if (res.success) {
            showSuccess('Status updated');
            loadBookings();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function createAdminBooking(e) {
    e.preventDefault();
    const clientId = parseInt(document.getElementById('client-id').value);
    if (!clientId) { showError('Please select a client'); return; }
    const payload = {
        clientId,
        siteId: parseInt(document.getElementById('site-id').value) || null,
        serviceType: document.getElementById('service-type').value,
        date: document.getElementById('booking-date').value,
        hour: parseInt(document.getElementById('booking-hour').value),
        notes: document.getElementById('booking-notes').value,
        services: []
    };
    const initialService = document.getElementById('initial-service').value;
    if (initialService) {
        payload.services.push({ serviceCatalogId: parseInt(initialService), quantity: 1 });
    }
    try {
        const res = await apiFetch('/bookings/admin', { method: 'POST', body: JSON.stringify(payload) });
        if (res.success) {
            showSuccess('Booking created');
            document.getElementById('new-booking-panel').style.display = 'none';
            loadBookings();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function loadClientsForSelect() {
    try {
        const res = await apiFetch('/clients?page=1&pageSize=200');
        if (res.success && res.data.items) {
            const select = document.getElementById('client-id');
            select.innerHTML = '<option value="">Select client</option>';
            res.data.items.forEach(c => {
                select.innerHTML += `<option value="${c.id}">${c.clientName}</option>`;
            });
        }
    } catch(e) { console.error(e); }
}

async function loadServicesForSelect() {
    try {
        const res = await apiFetch('/services');
        if (res.success && res.data) {
            const select = document.getElementById('initial-service');
            select.innerHTML = '<option value="">No initial service</option>';
            res.data.forEach(s => {
                if (s.isActive) select.innerHTML += `<option value="${s.id}">${s.name}</option>`;
            });
        }
    } catch(e) { console.error(e); }
}

async function loadSitesForClient(clientId) {
    const siteSelect = document.getElementById('site-id');
    siteSelect.disabled = true;
    siteSelect.innerHTML = '<option value="">No site</option>';
    if (!clientId) { siteSelect.disabled = false; return; }
    try {
        const res = await apiFetch(`/clients/${clientId}/sites`);
        if (res.success && res.data) {
            const activeSites = res.data.filter(s => s.isActive);
            activeSites.forEach(s => {
                siteSelect.innerHTML += `<option value="${s.id}">${s.siteName} (${s.siteType || 'Other'})</option>`;
            });
        }
        siteSelect.disabled = false;
    } catch(e) { siteSelect.disabled = false; }
}

document.getElementById('client-id').addEventListener('change', (e) => {
    loadSitesForClient(e.target.value);
});
document.getElementById('new-booking-btn').addEventListener('click', () => {
    const panel = document.getElementById('new-booking-panel');
    panel.style.display = panel.style.display === 'none' ? 'block' : 'none';
});
document.getElementById('create-booking-form').addEventListener('submit', createAdminBooking);
document.getElementById('filter-form').addEventListener('submit', (e) => {
    e.preventDefault();
    currentDateFilter = document.getElementById('date-filter').value;
    currentPage = 1;
    loadBookings();
});
document.getElementById('show-all-btn').addEventListener('click', () => {
    currentDateFilter = null;
    document.getElementById('date-filter').value = '';
    currentPage = 1;
    loadBookings();
});

loadClientsForSelect();
loadServicesForSelect();
loadBookings();