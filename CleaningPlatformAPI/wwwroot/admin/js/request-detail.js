// request-detail.js
const urlParams = new URLSearchParams(window.location.search);
const requestId = urlParams.get('id');
let requestData = null;

const statusMap = {
    New: { label: 'New', cls: 'new' },
    AdminReviewed: { label: 'Admin Reviewed', cls: 'adminreviewed' },
    SentToCustomer: { label: 'Sent to Customer', cls: 'senttocustomer' },
    CustomerConfirmed: { label: 'Customer Confirmed', cls: 'customerconfirmed' },
    Cancelled: { label: 'Cancelled', cls: 'cancelled' },
    Converted: { label: 'Converted', cls: 'converted' }
};

function fmtStatus(status) {
    const s = statusMap[status] || { label: status, cls: status.toLowerCase() };
    return `<span class="badge badge-${s.cls}">${window.__status(status)}</span>`;
}

async function loadRequestDetail() {
    if (!requestId) {
        document.getElementById('request-detail').innerHTML = `<div class="alert alert-info">${__('label_no_request_id')}</div>`;
        return;
    }
    try {
        const res = await apiFetch(`/booking-requests/${requestId}`);
        if (res.success && res.data) {
            requestData = res.data;
            renderRequestDetail();
        } else showError(res.message);
    } catch (e) { showError(e.message); }
}

async function loadServicesForSelect() {
    try {
        const res = await apiFetch('/services');
        if (res.success && res.data) {
            const select = document.getElementById('edit-services');
            select.innerHTML = '';
            res.data.forEach(s => {
                if (s.isActive) {
                    const selected = (requestData.services || []).some(rs => rs.serviceCatalogId === s.id);
                    select.innerHTML += `<option value="${s.id}" ${selected ? 'selected' : ''}>${s.name}</option>`;
                }
            });
        }
    } catch (e) { console.error(e); }
}

function renderRequestDetail() {
    const container = document.getElementById('request-detail');
    const r = requestData;
    const services = (r.services || []).map(s => s.serviceName).join(', ') || '-';
    const price = r.estimatedPrice ? r.estimatedPrice.toFixed(2) : '-';
    const canSend = r.status === 'New' || r.status === 'AdminReviewed';
    const canConvert = r.status === 'CustomerConfirmed';
    const canCancel = r.status !== 'Cancelled' && r.status !== 'Converted';

    container.innerHTML = `
        <section class="detail-section">
            <div class="page-header">
                <div><strong>Request #${r.id}</strong></div>
                ${fmtStatus(r.status)}
            </div>
            <p><strong>${__('th_created')}:</strong> ${formatDateTime(r.createdAt)}</p>
            <p><strong>Updated:</strong> ${formatDateTime(r.updatedAt)}</p>
        </section>
        <section class="detail-section">
            <h2 class="section-title">Contact Info</h2>
            <p><strong>${__('th_name')}:</strong> ${escHtml(r.contactName)}</p>
            <p><strong>${__('th_phone')}:</strong> ${escHtml(r.phone)}</p>
            <p><strong>${__('th_email')}:</strong> ${escHtml(r.email)}</p>
            <p><strong>${__('th_notes')}:</strong> ${escHtml(r.notes) || '-'}</p>
        </section>
        <section class="detail-section">
            <h2 class="section-title">${__('section_services')}</h2>
            <p><strong>Selected:</strong> ${services}</p>
            <p><strong>${__('label_est_price')}:</strong> ${price}</p>
        </section>
        <section class="detail-section">
            <h2 class="section-title">${__('nav_section_admin')}</h2>
            <div class="form-grid two-col">
                <label>${__('label_est_price')}
                    <input type="number" id="edit-estimated" step="0.01" class="text-input" value="${r.estimatedPrice || ''}" placeholder="0.00" />
                </label>
                <label>${__('label_admin_notes')}
                    <textarea id="edit-admin-notes" class="text-input" rows="3">${r.adminNotes || ''}</textarea>
                </label>
                <label class="full-span">${__('th_services')}
                    <select id="edit-services" class="text-input" multiple style="min-height:120px;"></select>
                    <small>Ctrl+click to select multiple</small>
                </label>
                <div class="full-span" style="display:flex; gap:0.5rem; flex-wrap:wrap;">
                    <button id="save-btn" class="btn btn-sm">${__('btn_save_changes')}</button>
                    ${canSend ? `<button id="send-btn" class="btn btn-sm" style="background:#1565c0;color:#fff;">${__('btn_send_to_customer')}</button>` : ''}
                    ${canConvert ? `<button id="convert-btn" class="btn btn-sm" style="background:#2e7d32;color:#fff;">${__('btn_confirm_create_booking')}</button>` : ''}
                    ${canCancel ? `<button id="cancel-btn" class="btn btn-sm" style="background:#c62828;color:#fff;">${__('btn_cancel_request')}</button>` : ''}
                </div>
            </div>
            <div id="action-message" style="margin-top:1rem;"></div>
        </section>
    `;

    loadServicesForSelect();

    document.getElementById('save-btn').addEventListener('click', saveChanges);
    if (document.getElementById('send-btn')) {
        document.getElementById('send-btn').addEventListener('click', sendToCustomer);
    }
    if (document.getElementById('convert-btn')) {
        document.getElementById('convert-btn').addEventListener('click', adminConfirm);
    }
    if (document.getElementById('cancel-btn')) {
        document.getElementById('cancel-btn').addEventListener('click', adminCancel);
    }
}

function escHtml(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

async function saveChanges() {
    const estimatedPrice = parseFloat(document.getElementById('edit-estimated').value) || null;
    const adminNotes = document.getElementById('edit-admin-notes').value;
    const select = document.getElementById('edit-services');
    const serviceCatalogIds = Array.from(select.selectedOptions).map(o => parseInt(o.value));

    if (!serviceCatalogIds.length) {
        showError('At least one service must be selected.', 'action-message');
        return;
    }

    try {
        const res = await apiFetch(`/booking-requests/${requestId}`, {
            method: 'PUT',
            body: JSON.stringify({ estimatedPrice, adminNotes, serviceCatalogIds })
        });
        if (res.success) {
            showSuccess(__('msg_changes_saved'), 'action-message');
            loadRequestDetail();
        } else showError(res.message, 'action-message');
    } catch (e) { showError(e.message, 'action-message'); }
}

async function sendToCustomer() {
    try {
        const res = await apiFetch(`/booking-requests/${requestId}/send`, { method: 'POST' });
        if (res.success) {
            showSuccess(__('msg_email_sent'), 'action-message');
            loadRequestDetail();
        } else showError(res.message, 'action-message');
    } catch (e) { showError(e.message, 'action-message'); }
}

async function adminConfirm() {
    if (!confirm(__('msg_confirm_create_booking'))) return;
    try {
        const res = await apiFetch(`/booking-requests/${requestId}/confirm`, { method: 'POST' });
        if (res.success && res.data) {
            const booking = res.data;
            showSuccess(`${__('msg_booking_created')} <a href="booking-detail.html?id=${booking.id}" style="color:#fff;text-decoration:underline;">${__('btn_view')}</a>`, 'action-message');
            loadRequestDetail();
        } else showError(res.message, 'action-message');
    } catch (e) { showError(e.message, 'action-message'); }
}

async function adminCancel() {
    if (!confirm(__('msg_confirm_cancel_request_title'))) return;
    try {
        const res = await apiFetch(`/booking-requests/${requestId}/status`, {
            method: 'PUT',
            body: JSON.stringify({ status: 'Cancelled' })
        });
        // Fallback: the API might not have a direct cancel endpoint; use generic update
        if (res.success) {
            showSuccess(__('msg_request_cancelled'), 'action-message');
            loadRequestDetail();
        } else {
            // Try direct status update via update endpoint
            const res2 = await apiFetch(`/booking-requests/${requestId}`, {
                method: 'PUT',
                body: JSON.stringify({
                    estimatedPrice: requestData.estimatedPrice,
                    adminNotes: requestData.adminNotes,
                    serviceCatalogIds: (requestData.services || []).map(s => s.serviceCatalogId)
                })
            });
            if (res2.success) {
                showSuccess(__('msg_request_cancelled'), 'action-message');
loadRequestDetail();
window.addEventListener('i18nReady', function () { loadRequestDetail(); });
            } else showError(res2.message, 'action-message');
        }
    } catch (e) { showError(e.message, 'action-message'); }
}

loadRequestDetail();
