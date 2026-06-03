// service-detail.js
const urlParams = new URLSearchParams(window.location.search);
const serviceId = urlParams.get('id');
let service = null;

async function loadService() {
    if (!serviceId) {
        showCreateForm();
        return;
    }
    try {
        const res = await apiFetch(`/services/${serviceId}`);
        if (res.success && res.data) {
            service = res.data;
            renderServiceDetail();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function showCreateForm() {
    document.getElementById('service-detail').innerHTML = `
        <div class="page-header"></div>
        <div class="card-lite">
            <form id="create-service-form" class="form-grid two-col">
                <label>${__('th_code')} <input type="text" id="svc-code" class="small-input" required /></label>
                <label>${__('th_name')} <input type="text" id="svc-name" class="text-input" required /></label>
                <label>${__('th_category')}
                    <select id="svc-category" class="text-input">
                        <option value="">-- Select --</option>
                        <option value="Stairs">Stairs</option>
                        <option value="Office">Office</option>
                        <option value="Private">Private</option>
                        <option value="Special">Special</option>
                        <option value="Carpet">Carpet</option>
                        <option value="Furniture">Furniture</option>
                        <option value="Exterior">Exterior</option>
                        <option value="Laundry">Laundry</option>
                        <option value="Vehicle">Vehicle</option>
                        <option value="Boat">Boat</option>
                    </select>
                </label>
                <label>${__('th_unit')} <input type="text" id="svc-unit" class="text-input" /></label>
                <label>${__('th_base_price')} <input type="number" id="svc-base-price" step="0.01" class="small-input" /></label>
                <label>${__('label_approx_time')} <input type="number" id="svc-approx-time" step="1" class="small-input" /></label>
                <label>${__('th_type')}
                    <select id="svc-service-type" class="text-input">
                        <option value="SiteBased">SiteBased</option>
                        <option value="Vehicle">Vehicle</option>
                        <option value="Boat">Boat</option>
                    </select>
                </label>
                <label class="checkbox-label"><input type="checkbox" id="svc-active" checked /> ${__('status_active')}</label>
                <div class="full-span"><button type="submit" class="btn btn-sm">${__('btn_create_service')}</button></div>
            </form>
        </div>
    `;
    document.getElementById('create-service-form').addEventListener('submit', createService);
}

async function createService(e) {
    e.preventDefault();
    const payload = {
        catalogCode: document.getElementById('svc-code').value,
        name: document.getElementById('svc-name').value,
        category: document.getElementById('svc-category').value || null,
        unit: document.getElementById('svc-unit').value || null,
        basePrice: parseFloat(document.getElementById('svc-base-price').value) || null,
        approxTime: parseInt(document.getElementById('svc-approx-time').value) || null,
        serviceType: document.getElementById('svc-service-type').value,
        isActive: document.getElementById('svc-active').checked
    };
    try {
        const res = await apiFetch('/services', { method: 'POST', body: JSON.stringify(payload) });
        if (res.success && res.data) {
            window.location.href = `service-detail.html?id=${res.data.id}`;
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function renderServiceDetail() {
    const container = document.getElementById('service-detail');
    container.innerHTML = `
        <section class="detail-section">
            <div class="page-header">
                <div>
                    <strong style="font-size:1.2rem;">${service.name}</strong>
                    <div><span class="badge badge-active">${service.catalogCode}</span>
                    <span class="badge ${service.isActive ? 'badge-active' : 'badge-inactive'}">${service.isActive ? __('status_active') : __('status_inactive')}</span></div>
                </div>
                <button id="edit-service-btn" class="btn btn-sm">${__('btn_edit')}</button>
                <button id="delete-service-btn" class="btn btn-sm" style="background:#c62828;color:#fff;">${__('btn_delete')}</button>
            </div>
        </section>
        <div id="edit-panel" style="display:none;" class="card-lite">
            <h3 class="section-title">${__('btn_edit')} ${service.name}</h3>
            <form id="edit-form" class="form-grid two-col">
                <label>${__('th_code')} <input type="text" id="edit-code" value="${service.catalogCode}" class="small-input" required /></label>
                <label>${__('th_name')} <input type="text" id="edit-name" value="${escapeHtml(service.name)}" class="text-input" required /></label>
                <label>${__('th_category')}
                    <select id="edit-category" class="text-input">
                        <option value="">-- Select --</option>
                        <option value="Stairs">Stairs</option>
                        <option value="Office">Office</option>
                        <option value="Private">Private</option>
                        <option value="Special">Special</option>
                        <option value="Carpet">Carpet</option>
                        <option value="Furniture">Furniture</option>
                        <option value="Exterior">Exterior</option>
                        <option value="Laundry">Laundry</option>
                        <option value="Vehicle">Vehicle</option>
                        <option value="Boat">Boat</option>
                    </select>
                </label>
                <label>${__('th_unit')} <input type="text" id="edit-unit" value="${service.unit || ''}" class="text-input" /></label>
                <label>${__('th_base_price')} <input type="number" id="edit-base-price" step="0.01" value="${service.basePrice || ''}" class="small-input" /></label>
                <label>${__('label_approx_time')} <input type="number" id="edit-approx-time" step="1" value="${service.approxTime || ''}" class="small-input" /></label>
                <label>${__('th_type')}
                    <select id="edit-service-type" class="text-input">
                        <option value="SiteBased" ${service.serviceType === 'SiteBased' ? 'selected' : ''}>SiteBased</option>
                        <option value="Vehicle" ${service.serviceType === 'Vehicle' ? 'selected' : ''}>Vehicle</option>
                        <option value="Boat" ${service.serviceType === 'Boat' ? 'selected' : ''}>Boat</option>
                    </select>
                </label>
                <label class="checkbox-label"><input type="checkbox" id="edit-active" ${service.isActive ? 'checked' : ''} /> ${__('status_active')}</label>
                <div class="modal-actions full-span">
                    <button type="submit" class="btn btn-sm">${__('btn_save')}</button>
                    <button type="button" id="cancel-edit" class="btn btn-sm">${__('btn_cancel')}</button>
                </div>
            </form>
        </div>
        <section class="detail-section">
            <h2 class="section-title">${__('label_requirements')}</h2>
            <div id="requirements-display"></div>
            <div class="card-lite" style="margin-top:1rem;">
                <h3 class="section-title">+ ${__('btn_add')}</h3>
                <form id="add-requirement-form" class="form-grid two-col">
                    <label>${__('th_inventory')} <select id="req-inventory-id" class="text-input"><option value="">--</option></select></label>
                    <label>${__('th_quantity_needed')} <input type="number" id="req-quantity" step="0.01" min="0.01" class="small-input" required /></label>
                    <div><button type="submit" class="btn btn-sm">${__('btn_add')}</button></div>
                </form>
            </div>
        </section>
    `;
    document.getElementById('edit-service-btn').addEventListener('click', () => {
        document.getElementById('edit-panel').style.display = 'block';
    });
    document.getElementById('cancel-edit').addEventListener('click', () => {
        document.getElementById('edit-panel').style.display = 'none';
    });
    document.getElementById('edit-form').addEventListener('submit', updateService);
    document.getElementById('delete-service-btn').addEventListener('click', deleteService);
    document.getElementById('add-requirement-form').addEventListener('submit', addRequirement);
    loadRequirements();
    loadInventorySelect();
}

async function loadRequirements() {
    try {
        const res = await apiFetch(`/services/${serviceId}/requirements`);
        if (res.success && res.data) {
            renderRequirements(res.data);
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function renderRequirements(requirements) {
    const container = document.getElementById('requirements-display');
    if (!requirements.length) {
        container.innerHTML = '<div class="alert alert-info">' + __('empty_no_requirements') + '</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>' + __('th_inventory') + '</th><th>' + __('th_unit') + '</th><th>' + __('th_quantity_needed') + '</th><th>' + __('th_actions') + '</th></tr></thead><tbody>';
    requirements.forEach(r => {
        html += `<tr>
            <td>${r.inventoryName}</td>
            <td>${r.unit || '-'}</td>
            <td>${r.quantityNeeded}</td>
            <td>
                <button onclick="editRequirement(${r.id}, ${r.quantityNeeded})" class="btn btn-sm">${__('btn_edit')}</button>
                <button onclick="removeRequirement(${r.id})" class="btn btn-sm">${__('btn_remove')}</button>
            </td>
        </tr>`;
    });
    html += '</tbody></table>';
    container.innerHTML = html;
}

async function loadInventorySelect() {
    try {
        const res = await apiFetch('/inventory');
        if (res.success && res.data) {
            const select = document.getElementById('req-inventory-id');
            select.innerHTML = '<option value="">-- Select --</option>';
            res.data.forEach(item => {
                select.innerHTML += `<option value="${item.id}">${item.name} (${item.unit || '-'})</option>`;
            });
        }
    } catch(e) { showError(e.message); }
}

async function addRequirement(e) {
    e.preventDefault();
    const inventoryId = parseInt(document.getElementById('req-inventory-id').value);
    const quantityNeeded = parseFloat(document.getElementById('req-quantity').value);
    if (!inventoryId) { showError('Please select an inventory item.'); return; }
    if (!quantityNeeded || quantityNeeded <= 0) { showError(__('err_quantity_needed_required')); return; }
    try {
        const res = await apiFetch(`/services/${serviceId}/requirements`, {
            method: 'POST',
            body: JSON.stringify({ inventoryId, quantityNeeded })
        });
        if (res.success) {
            showSuccess(__('msg_requirement_added'));
            document.getElementById('add-requirement-form').reset();
            loadRequirements();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function editRequirement(reqId, currentQty) {
    const newQty = prompt(`${__('th_quantity_needed')}:`, currentQty);
    if (!newQty || parseFloat(newQty) <= 0) return;
    try {
        const res = await apiFetch(`/services/${serviceId}/requirements/${reqId}`, {
            method: 'PUT',
            body: JSON.stringify({ inventoryId: 0, quantityNeeded: parseFloat(newQty) })
        });
        if (res.success) {
            showSuccess(__('msg_requirement_updated'));
            loadRequirements();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function removeRequirement(reqId) {
    if (!confirm(__('msg_confirm_remove_item'))) return;
    try {
        const res = await apiFetch(`/services/${serviceId}/requirements/${reqId}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess(__('msg_requirement_removed'));
            loadRequirements();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function updateService(e) {
    e.preventDefault();
    const payload = {
        catalogCode: document.getElementById('edit-code').value,
        name: document.getElementById('edit-name').value,
        category: document.getElementById('edit-category').value || null,
        unit: document.getElementById('edit-unit').value || null,
        basePrice: parseFloat(document.getElementById('edit-base-price').value) || null,
        approxTime: parseInt(document.getElementById('edit-approx-time').value) || null,
        serviceType: document.getElementById('edit-service-type').value,
        isActive: document.getElementById('edit-active').checked
    };
    try {
        const res = await apiFetch(`/services/${serviceId}`, { method: 'PUT', body: JSON.stringify(payload) });
        if (res.success) {
            showSuccess(__('msg_service_updated'));
            document.getElementById('edit-panel').style.display = 'none';
            loadService();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function deleteService() {
    if (!confirm(__('msg_confirm_delete_service'))) return;
    try {
        const res = await apiFetch(`/services/${serviceId}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess(__('msg_service_deleted'));
            window.location.href = '/admin/services.html';
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function escapeHtml(str) { return str.replace(/[&<>]/g, function(m){if(m==='&') return '&amp;'; if(m==='<') return '&lt;'; if(m==='>') return '&gt;'; return m;}); }

loadService();
window.addEventListener('i18nReady', function () { loadService(); });
