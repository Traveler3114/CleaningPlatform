// booking-detail.js
const urlParams = new URLSearchParams(window.location.search);
const bookingId = urlParams.get('id');
let booking = null;

async function loadBookingDetail() {
    if (!bookingId) {
        document.getElementById('booking-detail').innerHTML = '<div class="alert alert-info">No booking ID provided.</div>';
        return;
    }
    try {
        const res = await apiFetch(`/bookings/${bookingId}`);
        if (res.success && res.data) {
            booking = res.data;
            renderBookingDetail();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function renderBookingDetail() {
    const container = document.getElementById('booking-detail');
    const assignedEmployees = booking.assignedEmployees || [];
    const services = booking.services || [];
    container.innerHTML = `
        <section class="detail-section">
            <div class="page-header">
                <div><h1>Booking #${booking.id}</h1><p>${booking.date.split('T')[0]} at ${booking.hour}:00</p></div>
                <span class="badge badge-${booking.status.toLowerCase()}">${booking.status}</span>
            </div>
            <div class="inline-form" style="margin-top:0.7rem;">
                <select id="status-select" class="status-select">
                    <option ${booking.status === 'Pending' ? 'selected' : ''}>Pending</option>
                    <option ${booking.status === 'Confirmed' ? 'selected' : ''}>Confirmed</option>
                    <option ${booking.status === 'InProgress' ? 'selected' : ''}>InProgress</option>
                    <option ${booking.status === 'Completed' ? 'selected' : ''}>Completed</option>
                    <option ${booking.status === 'Cancelled' ? 'selected' : ''}>Cancelled</option>
                </select>
                <button id="update-status-btn" class="btn btn-sm">Save</button>
                ${booking.status === 'Completed' ? '<button id="generate-invoice-btn" class="btn btn-sm">Generate Invoice</button>' : ''}
            </div>
        </section>
        <section class="detail-section">
            <h2 class="section-title">Client Info</h2>
            <p><strong>Name:</strong> <a href="client-detail.html?id=${booking.clientId}">${booking.clientName}</a></p>
            <p><strong>Phone:</strong> ${booking.clientPhone || '-'}</p>
            <p><strong>Email:</strong> ${booking.clientEmail || '-'}</p>
        </section>
        <section class="detail-section">
            <h2 class="section-title">Employee Assignments</h2>
            <div id="assignments-list"></div>
            <div class="inline-form" style="margin-top:1rem;">
                <select id="employee-select" class="status-select"><option value="">Select employee</option></select>
                <button id="add-assignment-btn" class="btn btn-sm">Add</button>
            </div>
        </section>
        <section class="detail-section">
            <h2 class="section-title">Services</h2>
            <div id="services-list"></div>
            <div class="card-lite" style="margin-top:1rem;">
                <h3 class="section-title">+ Add Service</h3>
                <div class="inline-form">
                    <select id="service-catalog-id" class="status-select"><option value="">Select service</option></select>
                    <input type="number" id="service-quantity" value="1" step="0.01" class="small-input" placeholder="Qty" />
                    <input type="number" id="service-estimated" step="0.01" class="small-input" placeholder="Estimated" />
                    <input type="text" id="service-notes" class="text-input" placeholder="Notes" />
                    <button id="add-service-btn" class="btn btn-sm">Add Service</button>
                </div>
            </div>
        </section>
        <section class="detail-section">
            <h2 class="section-title">SOPs</h2>
            <div id="sops-list"></div>
        </section>
    `;
    renderAssignments(assignedEmployees);
    renderServices(services);
    loadEmployeesForSelect();
    loadServicesForSelect();
    loadBookingSops();
    
    document.getElementById('update-status-btn').addEventListener('click', updateStatus);
    if (document.getElementById('generate-invoice-btn')) {
        document.getElementById('generate-invoice-btn').addEventListener('click', generateInvoice);
    }
    document.getElementById('add-assignment-btn').addEventListener('click', addAssignment);
    document.getElementById('add-service-btn').addEventListener('click', addService);
}

function renderAssignments(assignments) {
    const container = document.getElementById('assignments-list');
    if (!assignments.length) {
        container.innerHTML = '<p><strong>Current:</strong> Unassigned</p>';
        return;
    }
    let html = '<ul>';
    assignments.forEach(a => {
        html += `<li style="margin-bottom:0.4rem;">${a.fullName} (${a.role}) 
            <button onclick="removeAssignment(${a.assignmentId})" class="btn btn-sm">Remove</button></li>`;
    });
    html += '</ul>';
    container.innerHTML = html;
}

function renderServices(services) {
    const container = document.getElementById('services-list');
    if (!services.length) {
        container.innerHTML = '<div class="alert alert-info">No services assigned.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>Name</th><th>Quantity</th><th>Estimated</th><th>Final Price</th><th>Notes</th><th>Actions</th></tr></thead><tbody>';
    services.forEach(s => {
        html += `<tr>
            <td>${s.serviceName}</td>
            <td>${s.quantity}</td>
            <td>${s.estimatedPrice ? s.estimatedPrice.toFixed(2) : '-'}</td>
            <td><input type="number" id="price-${s.id}" step="0.01" class="small-input" value="${s.finalPrice || ''}" placeholder="Final price" />
                <button onclick="updateServicePrice(${s.id})" class="btn btn-sm">Save</button></td>
            <td>${s.notes || '-'}</td>
            <td><button onclick="removeService(${s.id})" class="btn btn-sm">Remove</button></td>
        </tr>`;
    });
    html += '</tbody></table>';
    container.innerHTML = html;
}

async function updateStatus() {
    const newStatus = document.getElementById('status-select').value;
    try {
        const res = await apiFetch(`/bookings/${bookingId}/status`, {
            method: 'PUT',
            body: JSON.stringify({ status: newStatus })
        });
        if (res.success) {
            showSuccess('Status updated');
            loadBookingDetail();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function generateInvoice() {
    try {
        const res = await apiFetch(`/invoices/from-booking/${bookingId}`, { method: 'POST' });
        if (res.success && res.data) {
            window.location.href = `invoice-detail.html?id=${res.data.id}`;
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function addAssignment() {
    const employeeId = document.getElementById('employee-select').value;
    if (!employeeId) { showError('Select an employee'); return; }
    try {
        const res = await apiFetch(`/bookings/${bookingId}/assignments`, {
            method: 'POST',
            body: JSON.stringify({ employeeId: parseInt(employeeId) })
        });
        if (res.success) {
            showSuccess('Assignment added');
            loadBookingDetail();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function removeAssignment(assignmentId) {
    if (!confirm('Remove this assignment?')) return;
    try {
        const res = await apiFetch(`/bookings/${bookingId}/assignments/${assignmentId}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess('Assignment removed');
            loadBookingDetail();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function addService() {
    const serviceCatalogId = document.getElementById('service-catalog-id').value;
    if (!serviceCatalogId) { showError('Select a service'); return; }
    const quantity = parseFloat(document.getElementById('service-quantity').value) || 1;
    const estimatedPrice = parseFloat(document.getElementById('service-estimated').value) || null;
    const notes = document.getElementById('service-notes').value;
    try {
        const res = await apiFetch(`/bookings/${bookingId}/services`, {
            method: 'POST',
            body: JSON.stringify({ serviceCatalogId: parseInt(serviceCatalogId), quantity, estimatedPrice, notes })
        });
        if (res.success) {
            showSuccess('Service added');
            loadBookingDetail();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function removeService(serviceId) {
    if (!confirm('Remove this service?')) return;
    try {
        const res = await apiFetch(`/bookings/${bookingId}/services/${serviceId}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess('Service removed');
            loadBookingDetail();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function updateServicePrice(serviceId) {
    const finalPrice = parseFloat(document.getElementById(`price-${serviceId}`).value) || null;
    try {
        const res = await apiFetch(`/bookings/${bookingId}/services/${serviceId}`, {
            method: 'PUT',
            body: JSON.stringify({ finalPrice })
        });
        if (res.success) {
            showSuccess('Price updated');
            loadBookingDetail();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function loadEmployeesForSelect() {
    try {
        const res = await apiFetch('/employees');
        if (res.success && res.data) {
            const select = document.getElementById('employee-select');
            select.innerHTML = '<option value="">Select employee</option>';
            res.data.forEach(e => {
                select.innerHTML += `<option value="${e.id}">${e.fullName} (${e.role})</option>`;
            });
        }
    } catch(e) { console.error(e); }
}

async function loadServicesForSelect() {
    try {
        const res = await apiFetch('/services');
        if (res.success && res.data) {
            const select = document.getElementById('service-catalog-id');
            select.innerHTML = '<option value="">Select service</option>';
            res.data.forEach(s => {
                if (s.isActive) select.innerHTML += `<option value="${s.id}">${s.name}</option>`;
            });
        }
    } catch(e) { console.error(e); }
}

async function loadBookingSops() {
    try {
        const res = await apiFetch(`/bookings/${bookingId}/sops`);
        if (res.success && res.data) {
            renderSops(res.data);
        }
    } catch(e) { console.error(e); }
}

function renderSops(sops) {
    const container = document.getElementById('sops-list');
    if (!sops.length) {
        container.innerHTML = '<div class="alert alert-info">No SOPs assigned.</div>';
        return;
    }
    let html = '';
    sops.forEach(sop => {
        html += `<div class="card-lite" style="margin-bottom:1rem;">
            <div style="display:flex; justify-content:space-between;"><strong>${sop.sopName}</strong><span>${sop.completedItems} / ${sop.totalItems} complete</span></div>
            ${sop.customInstructions ? `<p><em>${sop.customInstructions}</em></p>` : ''}
            <ul style="margin-top:0.75rem; list-style:none;">`;
        (sop.checklistItems || []).forEach(item => {
            html += `<li style="display:flex; gap:0.5rem; padding:0.35rem 0;">
                <input type="checkbox" ${item.isCompleted ? 'checked' : ''} onchange="completeChecklistItem(${sop.id}, ${item.id}, this.checked)" />
                <span style="${item.isCompleted ? 'text-decoration:line-through;' : ''}">${item.itemText} ${item.isRequired ? '<span style="color:#c62828;">*</span>' : ''}</span>
            </li>`;
        });
        html += `</ul></div>`;
    });
    container.innerHTML = html;
}

async function completeChecklistItem(assignmentId, itemId, isCompleted) {
    try {
        const res = await apiFetch(`/assignments/${assignmentId}/checklist/${itemId}`, {
            method: 'POST',
            body: JSON.stringify({ isCompleted })
        });
        if (!res.success) showError(res.message);
        else loadBookingSops();
    } catch(e) { showError(e.message); }
}

loadBookingDetail();