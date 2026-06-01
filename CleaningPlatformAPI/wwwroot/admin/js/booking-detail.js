// booking-detail.js
const urlParams = new URLSearchParams(window.location.search);
const bookingId = urlParams.get('id');
let booking = null;

async function loadBookingDetail() {
    if (!bookingId) {
        document.getElementById('booking-detail').innerHTML = '<div class="alert alert-info">' + __('label_no_booking_id') + '</div>';
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
                <div><strong>Booking #${booking.id}</strong><p style="font-size:0.85rem;color:var(--text-muted);">${booking.date.split('T')[0]} at ${booking.hour}:00</p></div>
                ${statusBadge(booking.status)}
            </div>
            <div class="inline-form" style="margin-top:0.7rem;">
                <select id="status-select" class="status-select">
                    <option ${booking.status === 'Pending' ? 'selected' : ''}>${window.__status('Pending')}</option>
                    <option ${booking.status === 'InProgress' ? 'selected' : ''}>${window.__status('InProgress')}</option>
                    <option ${booking.status === 'Completed' ? 'selected' : ''}>${window.__status('Completed')}</option>
                    <option ${booking.status === 'Cancelled' ? 'selected' : ''}>${window.__status('Cancelled')}</option>
                </select>
                <button id="update-status-btn" class="btn btn-sm">${__('btn_save')}</button>
                ${booking.status === 'Completed' ? '<button id="generate-invoice-btn" class="btn btn-sm">' + __('btn_generate_invoice') + '</button>' : ''}
                ${renderRecurringButtons()}
            </div>
        </section>
        <section class="detail-section">
            <h2 class="section-title">${__('th_client')} Info</h2>
            <p><strong>${__('th_name')}:</strong> <a href="client-detail.html?id=${booking.clientId}">${booking.clientName}</a></p>
            <p><strong>${__('th_phone')}:</strong> ${booking.clientPhone || '-'}</p>
            <p><strong>${__('th_email')}:</strong> ${booking.clientEmail || '-'}</p>
        </section>
        <section class="detail-section">
            <h2 class="section-title">${__('section_assigned_employees')}</h2>
            <div id="assignments-list"></div>
            <div class="inline-form" style="margin-top:1rem;">
                <select id="employee-select" class="status-select"><option value="">${__('label_select_employee')}</option></select>
                <button id="add-assignment-btn" class="btn btn-sm">${__('btn_add')}</button>
            </div>
        </section>
        <section class="detail-section">
            <h2 class="section-title">${__('section_services')}</h2>
            <div id="services-list"></div>
            <div class="card-lite" style="margin-top:1rem;">
                <h3 class="section-title">+ ${__('btn_add_service')}</h3>
                <div class="inline-form">
                    <select id="service-catalog-id" class="status-select"><option value="">Select service</option></select>
                    <input type="number" id="service-quantity" value="1" step="0.01" class="small-input" placeholder="Qty" />
                    <input type="number" id="service-estimated" step="0.01" class="small-input" placeholder="Estimated" />
                    <input type="text" id="service-notes" class="text-input" placeholder="Notes" />
                    <button id="add-service-btn" class="btn btn-sm">${__('btn_add_service')}</button>
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
        container.innerHTML = '<div class="alert alert-info">' + __('empty_no_services_assigned') + '</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>' + __('th_name') + '</th><th>' + __('th_items') + '</th><th>' + __('label_est_price') + '</th><th>' + __('th_price') + '</th><th>' + __('th_notes') + '</th><th>' + __('th_actions') + '</th></tr></thead><tbody>';
    services.forEach(s => {
        html += `<tr>
            <td>${s.serviceName}</td>
            <td>${s.quantity}</td>
            <td>${s.estimatedPrice ? s.estimatedPrice.toFixed(2) : '-'}</td>
            <td><input type="number" id="price-${s.id}" step="0.01" class="small-input" value="${s.finalPrice || ''}" placeholder="Final price" />
                <button onclick="updateServicePrice(${s.id})" class="btn btn-sm">${__('btn_save')}</button></td>
            <td>${s.notes || '-'}</td>
            <td><button onclick="removeService(${s.id})" class="btn btn-sm">${__('btn_remove')}</button></td>
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
            showSuccess(__('msg_status_updated'));
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
    if (!employeeId) { showError(__('msg_select_employee')); return; }
    try {
        const res = await apiFetch(`/bookings/${bookingId}/assignments`, {
            method: 'POST',
            body: JSON.stringify({ employeeId: parseInt(employeeId) })
        });
        if (res.success) {
            showSuccess(__('msg_assignment_added'));
            loadBookingDetail();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function removeAssignment(assignmentId) {
    if (!confirm(__('msg_confirm_remove_assignment'))) return;
    try {
        const res = await apiFetch(`/bookings/${bookingId}/assignments/${assignmentId}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess(__('msg_assignment_removed'));
            loadBookingDetail();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function addService() {
    const serviceCatalogId = document.getElementById('service-catalog-id').value;
    if (!serviceCatalogId) { showError(__('msg_select_service')); return; }
    const quantity = parseFloat(document.getElementById('service-quantity').value) || 1;
    const estimatedPrice = parseFloat(document.getElementById('service-estimated').value) || null;
    const notes = document.getElementById('service-notes').value;
    try {
        const res = await apiFetch(`/bookings/${bookingId}/services`, {
            method: 'POST',
            body: JSON.stringify({ serviceCatalogId: parseInt(serviceCatalogId), quantity, estimatedPrice, notes })
        });
        if (res.success) {
            showSuccess(__('msg_service_added_booking'));
            loadBookingDetail();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function removeService(serviceId) {
    if (!confirm(__('msg_confirm_remove_service'))) return;
    try {
        const res = await apiFetch(`/bookings/${bookingId}/services/${serviceId}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess(__('msg_service_removed_booking'));
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
            showSuccess(__('msg_price_updated'));
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

// ── Recurring Schedule UI ────────────────────────────────────

function renderRecurringButtons() {
    if (booking.recurringScheduleId) {
        return `<span style="margin-left:0.5rem; font-size:0.85rem; color:#666;">
            ↻ Part of <a href="recurring.html" style="text-decoration:underline;">schedule #${booking.recurringScheduleId}</a>
            <button id="end-series-here-btn" class="btn btn-sm" style="margin-left:0.5rem; background:#c62828; color:#fff;">${__('btn_end_series_from_here')}</button>
        </span>`;
    }
    if (booking.status === 'Completed') {
        return '<button id="make-recurring-btn" class="btn btn-sm" style="margin-left:0.5rem;">' + __('btn_make_recurring') + '</button>';
    }
    return '';
}

// ── Make Recurring Modal ─────────────────────────────────────

function makeRecurringModalHtml() {
    return `
    <div id="recurring-modal" class="modal-overlay" style="display:none;">
        <div class="modal-content" style="max-width:450px;">
            <h2>${__('modal_make_recurring')}</h2>
            <p>${__('modal_make_recurring_desc').replace('{id}', booking.id)}</p>
            <form id="make-recurring-form" class="form-grid two-col">
                <label>Frequency
                    <select id="rec-frequency" class="text-input">
                        <option value="Weekly">Weekly</option>
                        <option value="Biweekly">Biweekly</option>
                        <option value="Monthly">Monthly</option>
                    </select>
                </label>
                <label id="rec-dayofweek-group">Day of Week
                    <select id="rec-dayofweek" class="text-input">
                        <option value="">--</option>
                        <option value="0">Sunday</option>
                        <option value="1">Monday</option>
                        <option value="2">Tuesday</option>
                        <option value="3">Wednesday</option>
                        <option value="4">Thursday</option>
                        <option value="5">Friday</option>
                        <option value="6">Saturday</option>
                    </select>
                </label>
                <label id="rec-dayofmonth-group" style="display:none;">Day of Month (1-28)
                    <input type="number" id="rec-dayofmonth" class="text-input" min="1" max="28" value="1" />
                </label>
                <label>Weeks Ahead (1-52)
                    <input type="number" id="rec-weeksahead" class="text-input" min="1" max="52" value="4" />
                </label>
                <label class="full-span">End Date (optional)
                    <input type="date" id="rec-endson" class="text-input" />
                </label>
                <div class="full-span" style="display:flex; gap:0.5rem;">
                    <button type="submit" class="btn btn-sm">${__('btn_create_recurring_schedule')}</button>
                    <button type="button" id="rec-cancel-btn" class="btn btn-sm">${__('btn_cancel')}</button>
                </div>
            </form>
        </div>
    </div>`;
}

function toggleRecFields(frequency) {
    const dow = document.getElementById('rec-dayofweek-group');
    const dom = document.getElementById('rec-dayofmonth-group');
    if (frequency === 'Weekly' || frequency === 'Biweekly') {
        dow.style.display = 'block';
        dom.style.display = 'none';
    } else if (frequency === 'Monthly') {
        dow.style.display = 'none';
        dom.style.display = 'block';
    } else {
        dow.style.display = 'none';
        dom.style.display = 'none';
    }
}

document.addEventListener('click', function (e) {
    // Make Recurring
    if (e.target.id === 'make-recurring-btn') {
        const existing = document.getElementById('recurring-modal');
        if (existing) { existing.style.display = 'flex'; return; }
        const div = document.createElement('div');
        div.innerHTML = makeRecurringModalHtml();
        document.body.appendChild(div);
        document.getElementById('recurring-modal').style.display = 'flex';
        const freq = document.getElementById('rec-frequency');
        freq.addEventListener('change', function () { toggleRecFields(this.value); });
        toggleRecFields(freq.value);
        document.getElementById('rec-cancel-btn').addEventListener('click', function () {
            document.getElementById('recurring-modal').style.display = 'none';
        });
        document.getElementById('make-recurring-form').addEventListener('submit', async function (ev) {
            ev.preventDefault();
            const frequency = document.getElementById('rec-frequency').value;
            const dayOfWeek = (frequency === 'Weekly' || frequency === 'Biweekly')
                ? parseInt(document.getElementById('rec-dayofweek').value) || null
                : null;
            const dayOfMonth = frequency === 'Monthly'
                ? parseInt(document.getElementById('rec-dayofmonth').value) || null
                : null;
            const autoGenerateWeeksAhead = parseInt(document.getElementById('rec-weeksahead').value) || 4;
            const endsOn = document.getElementById('rec-endson').value || null;
            try {
                const res = await apiFetch(`/recurring/from-booking/${bookingId}`, {
                    method: 'POST',
                    body: JSON.stringify({ frequency, dayOfWeek, dayOfMonth, autoGenerateWeeksAhead, endsOn })
                });
                if (res.success) {
                    showSuccess(__('msg_recurring_created'));
                    document.getElementById('recurring-modal').style.display = 'none';
                    loadBookingDetail();
                } else showError(res.message);
            } catch (err) { showError(err.message); }
        });
    }

    // End series from here
    if (e.target.id === 'end-series-here-btn') {
        const today = new Date().toISOString().split('T')[0];
        if (!confirm(__('msg_confirm_end_series').replace('{date}', today))) return;
        (async function () {
            try {
                const res = await apiFetch(`/recurring/${booking.recurringScheduleId}/end`, {
                    method: 'POST',
                    body: JSON.stringify({ endsOn: today })
                });
                if (res.success) {
                    showSuccess(__('msg_series_ended'));
                    loadBookingDetail();
                } else showError(res.message);
            } catch (err) { showError(err.message); }
        })();
    }
});

loadBookingDetail();
window.addEventListener('i18nReady', function () { loadBookingDetail(); });