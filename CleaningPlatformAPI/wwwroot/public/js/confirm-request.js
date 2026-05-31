// confirm-request.js
const token = new URLSearchParams(window.location.search).get('token');
let requestData = null;

async function loadRequest() {
    if (!token) {
        showMessage('No confirmation link provided. Please check your email for the correct link.', 'error');
        return;
    }
    try {
        const res = await fetch(`/api/booking-requests/customer-preview?token=${encodeURIComponent(token)}`);
        const result = await res.json();
        if (result.success && result.data) {
            requestData = result.data;
            renderDetails();
        } else {
            showMessage(result.message || 'Could not load request details. The link may be expired.', 'error');
        }
    } catch (e) {
        showMessage('Network error. Please try again.', 'error');
    }
}

function renderDetails() {
    document.getElementById('loading-msg').style.display = 'none';
    const container = document.getElementById('request-details');
    container.style.display = 'block';
    const r = requestData;
    const services = (r.services || []).map(s => s.serviceName).join(', ') || '-';
    const price = r.estimatedPrice ? r.estimatedPrice.toFixed(2) + ' €' : 'To be determined';

    let actionsHtml = '';
    if (r.status === 'SentToCustomer') {
        actionsHtml = `
            <div style="display:flex;gap:1rem;margin-top:1.5rem;">
                <button id="confirm-btn" class="btn-next" style="flex:1;">Confirm ✓</button>
                <button id="cancel-btn" class="btn-back" style="flex:1;text-align:center;">Cancel</button>
            </div>
        `;
    } else if (r.status === 'CustomerConfirmed') {
        actionsHtml = `<div class="alert alert-success" style="margin-top:1rem;">This request has already been confirmed.</div>`;
    } else if (r.status === 'Cancelled') {
        actionsHtml = `<div class="alert alert-danger" style="margin-top:1rem;">This request has been cancelled.</div>`;
    } else {
        actionsHtml = `<div class="alert alert-info" style="margin-top:1rem;">Status: ${r.status}</div>`;
    }

    container.innerHTML = `
        <div class="booking-summary">
            <div class="summary-row"><span>Request ID</span><span>#${r.id}</span></div>
            <div class="summary-row"><span>Name</span><span>${escHtml(r.contactName)}</span></div>
            <div class="summary-row"><span>Phone</span><span>${escHtml(r.phone)}</span></div>
            <div class="summary-row"><span>Email</span><span>${escHtml(r.email)}</span></div>
            <div class="summary-row"><span>Services</span><span>${services}</span></div>
            <div class="summary-row"><span>Estimated Price</span><span>${price}</span></div>
            ${r.adminNotes ? `<div class="summary-row"><span>Notes from us</span><span>${escHtml(r.adminNotes)}</span></div>` : ''}
            ${r.notes ? `<div class="summary-row"><span>Your Notes</span><span>${escHtml(r.notes)}</span></div>` : ''}
            <div class="summary-row"><span>Status</span><span>${r.status}</span></div>
        </div>
        <div id="action-area">${actionsHtml}</div>
    `;

    if (document.getElementById('confirm-btn')) {
        document.getElementById('confirm-btn').addEventListener('click', confirmRequest);
    }
    if (document.getElementById('cancel-btn')) {
        document.getElementById('cancel-btn').addEventListener('click', cancelRequest);
    }
}

async function confirmRequest() {
    const btn = document.getElementById('confirm-btn');
    btn.disabled = true;
    btn.textContent = 'Processing…';
    try {
        const res = await fetch('/api/booking-requests/customer-confirm', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ token })
        });
        const result = await res.json();
        if (result.success) {
            showResult('success', result.message || 'Your request has been confirmed!');
        } else {
            showResult('error', result.message || 'Could not confirm request.');
        }
    } catch (e) {
        showResult('error', 'Network error. Please try again.');
    }
}

async function cancelRequest() {
    if (!confirm('Are you sure you want to cancel this request?')) return;
    const btn = document.getElementById('cancel-btn');
    btn.disabled = true;
    btn.textContent = 'Processing…';
    try {
        const res = await fetch('/api/booking-requests/customer-cancel', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ token })
        });
        const result = await res.json();
        if (result.success) {
            showResult('info', result.message || 'Your request has been cancelled.');
        } else {
            showResult('error', result.message || 'Could not cancel request.');
        }
    } catch (e) {
        showResult('error', 'Network error. Please try again.');
    }
}

function showMessage(msg, type) {
    document.getElementById('loading-msg').style.display = 'none';
    const container = document.getElementById('request-details');
    container.style.display = 'block';
    container.innerHTML = `<div class="alert alert-${type === 'error' ? 'danger' : 'info'}">${escHtml(msg)}</div>`;
}

function showResult(type, msg) {
    document.getElementById('request-details').style.display = 'none';
    document.getElementById('result-msg').style.display = 'block';
    document.getElementById('result-msg').innerHTML = `
        <div class="confirmation">
            <div class="confirmation-icon">${type === 'success' ? '✅' : 'ℹ️'}</div>
            <h2 class="serif">${type === 'success' ? 'Confirmed!' : 'Updated'}</h2>
            <p>${msg}</p>
            <a href="/public/index.html" class="btn-next" style="display:inline-block;margin-top:1rem;text-decoration:none;">Return Home</a>
        </div>
    `;
}

function escHtml(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

loadRequest();
