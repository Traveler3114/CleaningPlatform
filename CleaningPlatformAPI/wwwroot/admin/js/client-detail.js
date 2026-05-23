// client-detail.js
const urlParams = new URLSearchParams(window.location.search);
const clientId = urlParams.get('id');
const isNew = urlParams.get('new') === '1';
let client = null;

async function loadClient() {
    if (isNew) {
        showCreateForm();
        return;
    }
    if (!clientId) {
        document.getElementById('client-detail').innerHTML = '<div class="alert alert-info">No client ID provided.</div>';
        return;
    }
    try {
        const res = await apiFetch(`/clients/${clientId}`);
        if (res.success && res.data) {
            client = res.data;
            renderClientDetail();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function showCreateForm() {
    document.getElementById('client-detail').innerHTML = `
        <div class="page-header"><h1>New Client</h1></div>
        <div class="card-lite">
            <form id="create-client-form" class="form-grid two-col">
                <label>Client Name <input type="text" id="client-name" class="text-input" required /></label>
                <label>Type
                    <select id="client-type" class="status-select">
                        <option value="Person">Person</option>
                        <option value="Business">Business</option>
                    </select>
                </label>
                <label>Primary Contact Name <input type="text" id="primary-name" class="text-input" required /></label>
                <label>Phone <input type="text" id="primary-phone" class="text-input" required /></label>
                <label>Email <input type="email" id="primary-email" class="text-input" /></label>
                <label id="oib-field">OIB <input type="text" id="oib" class="text-input" /></label>
                <label id="payment-terms-field">Payment Terms <input type="text" id="payment-terms" class="text-input" /></label>
                <div class="full-span"><button type="submit" class="btn btn-sm">Create Client</button></div>
            </form>
        </div>
    `;
    const typeSelect = document.getElementById('client-type');
    const oibField = document.getElementById('oib-field');
    const ptField = document.getElementById('payment-terms-field');
    function sync() {
        const isBusiness = typeSelect.value === 'Business';
        oibField.style.display = isBusiness ? 'block' : 'none';
        ptField.style.display = isBusiness ? 'block' : 'none';
    }
    typeSelect.addEventListener('change', sync);
    sync();
    document.getElementById('create-client-form').addEventListener('submit', createClient);
}

async function createClient(e) {
    e.preventDefault();
    const payload = {
        clientName: document.getElementById('client-name').value,
        type: document.getElementById('client-type').value,
        primaryContactName: document.getElementById('primary-name').value,
        primaryContactPhone: document.getElementById('primary-phone').value,
        primaryContactEmail: document.getElementById('primary-email').value || null,
        oib: document.getElementById('oib').value || null,
        paymentTerms: document.getElementById('payment-terms').value || null
    };
    try {
        const res = await apiFetch('/clients', { method: 'POST', body: JSON.stringify(payload) });
        if (res.success && res.data) {
            window.location.href = `client-detail.html?id=${res.data.id}`;
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function renderClientDetail() {
    const container = document.getElementById('client-detail');
    const contacts = client.contacts || [];
    const sites = client.sites || [];
    const bookings = client.bookings || [];
    container.innerHTML = `
        <section class="detail-section">
            <div class="page-header">
                <div>
                    <h1>${client.clientName}</h1>
                    <div><span class="badge badge-${client.type.toLowerCase()}">${client.type}</span>
                    <span class="badge ${client.isActive ? 'badge-active' : 'badge-inactive'}">${client.isActive ? 'Active' : 'Inactive'}</span></div>
                </div>
                <button id="edit-client-btn" class="btn btn-sm">Edit</button>
            </div>
        </section>
        <div id="edit-panel" style="display:none;" class="card-lite">
            <h3 class="section-title">Edit Client & Contacts</h3>
            <form id="edit-form">
                <div class="form-grid two-col">
                    <label>Client Name <input type="text" id="edit-client-name" value="${escapeHtml(client.clientName)}" class="text-input" /></label>
                    <label>OIB <input type="text" id="edit-oib" value="${client.oib || ''}" class="text-input" /></label>
                    <label>Payment Terms <input type="text" id="edit-payment-terms" value="${client.paymentTerms || ''}" class="text-input" /></label>
                    <label>Notes <input type="text" id="edit-notes" value="${client.notes || ''}" class="text-input" /></label>
                </div>
                <h4 class="section-title">Contacts</h4>
                <table class="admin-table" id="contacts-table"><thead><tr><th>Name</th><th>Role</th><th>Phone</th><th>Email</th><th>Primary</th><th>Active</th><th>Actions</th></tr></thead><tbody id="contacts-tbody"></tbody></table>
                <button type="button" id="add-contact-btn" class="btn btn-sm">+ Add Contact</button>
                <div class="modal-actions"><button type="submit" class="btn btn-sm">Save</button><button type="button" id="cancel-edit" class="btn btn-sm">Cancel</button></div>
            </form>
        </div>
        <section class="detail-section"><h2 class="section-title">Contacts</h2><div id="contacts-display"></div></section>
        <section class="detail-section"><h2 class="section-title">Sites</h2><div id="sites-display"></div>
            <div class="card-lite" style="margin-top:1rem;"><h3 class="section-title">+ Add Site</h3>
            <form id="add-site-form" class="form-grid two-col">
                <label>Site Name <input type="text" id="site-name" class="text-input" required /></label>
                <label>Address <input type="text" id="site-address" class="text-input" required /></label>
                <label>City <input type="text" id="site-city" class="text-input" /></label>
                <label>Postal Code <input type="text" id="site-postal" class="text-input" /></label>
                <label>Type <select id="site-type" class="status-select"><option value="">—</option><option>Office</option><option>Stairwell</option><option>Garage</option><option>Facility</option><option>Boat</option><option>Vehicle</option><option>Other</option></select></label>
                <label>Floor Area m² <input type="number" id="site-area" step="0.01" class="text-input" /></label>
                <label class="full-span">Access Notes <input type="text" id="site-notes" class="text-input" /></label>
                <div><button type="submit" class="btn btn-sm">Add Site</button></div>
            </form></div>
        </section>
        <section class="detail-section"><h2 class="section-title">Booking History</h2><div id="bookings-display"></div></section>
    `;
    renderContactsDisplay(contacts);
    renderSitesDisplay(sites);
    renderBookingsDisplay(bookings);
    renderContactsTable(contacts);
    
    document.getElementById('edit-client-btn').addEventListener('click', () => {
        document.getElementById('edit-panel').style.display = 'block';
    });
    document.getElementById('cancel-edit').addEventListener('click', () => {
        document.getElementById('edit-panel').style.display = 'none';
    });
    document.getElementById('add-contact-btn').addEventListener('click', addContactRow);
    document.getElementById('edit-form').addEventListener('submit', updateClient);
    document.getElementById('add-site-form').addEventListener('submit', addSite);
}

function renderContactsDisplay(contacts) {
    if (!contacts.length) {
        document.getElementById('contacts-display').innerHTML = '<div class="alert alert-info">No contacts found.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>Name</th><th>Role</th><th>Phone</th><th>Email</th><th>Primary</th><th>Active</th></tr></thead><tbody>';
    contacts.forEach(c => {
        html += `<tr class="${!c.isActive ? 'row-inactive' : ''}">
            <td>${escapeHtml(c.contactName)}</td><td>${c.role || '-'}</td><td>${c.phone || '-'}</td><td>${c.email || '-'}</td>
            <td>${c.isPrimary ? '<span class="badge badge-active">Primary</span>' : '-'}</td>
            <td><span class="badge ${c.isActive ? 'badge-active' : 'badge-inactive'}">${c.isActive ? 'Active' : 'Inactive'}</span></td>
        </tr>`;
    });
    html += '</tbody></table><div class="alert alert-info">Use Edit to update client and contacts in one save action.</div>';
    document.getElementById('contacts-display').innerHTML = html;
}

function renderSitesDisplay(sites) {
    if (!sites.length) {
        document.getElementById('sites-display').innerHTML = '<div class="alert alert-info">No sites found.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>Site Name</th><th>Address</th><th>City</th><th>Type</th><th>Active</th><th>Actions</th></tr></thead><tbody>';
    sites.forEach(s => {
        html += `<tr class="${!s.isActive ? 'row-inactive' : ''}">
            <td>${escapeHtml(s.siteName)}</td><td>${escapeHtml(s.address)}</td><td>${s.city || '-'}</td><td>${s.siteType || '-'}</td>
            <td><span class="badge ${s.isActive ? 'badge-active' : 'badge-inactive'}">${s.isActive ? 'Active' : 'Inactive'}</span></td>
            <td>${s.isActive ? `<button onclick="deactivateSite(${s.id})" class="btn btn-sm">Deactivate</button>` : ''}</td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('sites-display').innerHTML = html;
}

function renderBookingsDisplay(bookings) {
    if (!bookings.length) {
        document.getElementById('bookings-display').innerHTML = '<div class="alert alert-info">No bookings found.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>Date</th><th>Time</th><th>Status</th><th>Services</th><th>Actions</th></tr></thead><tbody>';
    bookings.forEach(b => {
        html += `<tr>
            <td>${b.date.split('T')[0]}</td><td>${b.hour}:00</td>
            <td><span class="badge badge-${b.status.toLowerCase()}">${b.status}</span></td>
            <td>${b.servicesCount}</td>
            <td><a href="booking-detail.html?id=${b.id}" class="btn btn-sm">View</a></td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('bookings-display').innerHTML = html;
}

function renderContactsTable(contacts) {
    const tbody = document.getElementById('contacts-tbody');
    tbody.innerHTML = '';
    contacts.forEach(c => {
        const row = tbody.insertRow();
        row.innerHTML = `
            <td><input data-field="contactName" class="text-input" value="${escapeHtml(c.contactName)}" /><input type="hidden" data-field="id" value="${c.id}" /></td>
            <td><input data-field="role" class="small-input" value="${c.role || ''}" /></td>
            <td><input data-field="phone" class="small-input" value="${c.phone || ''}" /></td>
            <td><input data-field="email" class="text-input" value="${c.email || ''}" /></td>
            <td><input data-field="isPrimary" type="radio" name="primary-contact" ${c.isPrimary ? 'checked' : ''} /></td>
            <td><input data-field="isActive" type="checkbox" ${c.isActive ? 'checked' : ''} /></td>
            <td><button type="button" class="btn btn-sm" onclick="this.closest('tr').remove()">Remove</button></td>
        `;
    });
}

function addContactRow() {
    const tbody = document.getElementById('contacts-tbody');
    const row = tbody.insertRow();
    row.innerHTML = `
        <td><input data-field="contactName" class="text-input" value="" /><input type="hidden" data-field="id" value="0" /></td>
        <td><input data-field="role" class="small-input" value="" /></td>
        <td><input data-field="phone" class="small-input" value="" /></td>
        <td><input data-field="email" class="text-input" value="" /></td>
        <td><input data-field="isPrimary" type="radio" name="primary-contact" /></td>
        <td><input data-field="isActive" type="checkbox" checked /></td>
        <td><button type="button" class="btn btn-sm" onclick="this.closest('tr').remove()">Remove</button></td>
    `;
}

async function updateClient(e) {
    e.preventDefault();
    const contacts = [];
    const rows = document.querySelectorAll('#contacts-tbody tr');
    rows.forEach(row => {
        const id = parseInt(row.querySelector('[data-field="id"]').value) || null;
        contacts.push({
            id: id,
            contactName: row.querySelector('[data-field="contactName"]').value,
            role: row.querySelector('[data-field="role"]').value || null,
            phone: row.querySelector('[data-field="phone"]').value,
            email: row.querySelector('[data-field="email"]').value || null,
            isPrimary: row.querySelector('[data-field="isPrimary"]').checked,
            isActive: row.querySelector('[data-field="isActive"]').checked
        });
    });
    const payload = {
        clientName: document.getElementById('edit-client-name').value,
        oib: document.getElementById('edit-oib').value || null,
        paymentTerms: document.getElementById('edit-payment-terms').value || null,
        notes: document.getElementById('edit-notes').value || null,
        contacts: contacts
    };
    try {
        const res = await apiFetch(`/clients/${clientId}`, { method: 'PUT', body: JSON.stringify(payload) });
        if (res.success) {
            showSuccess('Client updated');
            document.getElementById('edit-panel').style.display = 'none';
            loadClient();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function addSite(e) {
    e.preventDefault();
    const payload = {
        siteName: document.getElementById('site-name').value,
        address: document.getElementById('site-address').value,
        city: document.getElementById('site-city').value || null,
        postalCode: document.getElementById('site-postal').value || null,
        siteType: document.getElementById('site-type').value || null,
        floorAreaM2: parseFloat(document.getElementById('site-area').value) || null,
        accessNotes: document.getElementById('site-notes').value || null,
        isActive: true
    };
    try {
        const res = await apiFetch(`/clients/${clientId}/sites`, { method: 'POST', body: JSON.stringify(payload) });
        if (res.success) {
            showSuccess('Site added');
            document.getElementById('add-site-form').reset();
            loadClient();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function deactivateSite(siteId) {
    if (!confirm('Deactivate this site?')) return;
    try {
        const res = await apiFetch(`/clients/${clientId}/sites/${siteId}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess('Site deactivated');
            loadClient();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function escapeHtml(str) { return str.replace(/[&<>]/g, function(m){if(m==='&') return '&amp;'; if(m==='<') return '&lt;'; if(m==='>') return '&gt;'; return m;}); }

loadClient();