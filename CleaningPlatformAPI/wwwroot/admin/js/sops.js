// sops.js
let templates = [];
let services = [];
let editingTemplateId = null;

async function loadServicesForSelect() {
    try {
        const res = await apiFetch('/services');
        if (res.success && res.data) {
            services = res.data.filter(s => s.isActive);
            renderServiceSelects();
        }
    } catch(e) { console.error(e); }
}

function renderServiceSelects() {
    const selects = ['template-service-catalog', 'edit-template-service-catalog'];
    selects.forEach(id => {
        const select = document.getElementById(id);
        if (!select) return;
        select.innerHTML = '<option value="">Optional</option>';
        services.forEach(s => {
            select.innerHTML += `<option value="${s.id}">${s.name}</option>`;
        });
    });
}

async function loadTemplates() {
    try {
        const res = await apiFetch('/sops');
        if (res.success && res.data) {
            templates = res.data;
            renderTemplates();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function renderTemplates() {
    if (!templates.length) {
        document.getElementById('sops-list').innerHTML = '<div class="alert alert-info">No SOP templates found.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>Name</th><th>Service Type</th><th>Linked Service</th><th>Active</th><th>Checklist</th><th>Actions</th></tr></thead><tbody>';
    templates.forEach(t => {
        html += `<tr class="sop-row" data-id="${t.id}" data-name="${t.name}" data-description="${t.description || ''}" data-active="${t.isActive}" data-service-type="${t.serviceType}" data-service-catalog-id="${t.serviceCatalogId || ''}" style="cursor:pointer;">
            <td>${t.name}</td>
            <td>${t.serviceType}</td>
            <td>${t.serviceCatalogName || '-'}</td>
            <td>${t.isActive ? 'Yes' : 'No'}</td>
            <td>${t.checklistItems?.length || 0}</td>
            <td><button onclick="deleteTemplate(${t.id})" class="btn btn-sm">Deactivate</button></td>
        </tr>`;
        // Checklist items row
        html += `<tr><td colspan="6"><strong>Checklist items</strong>`;
        if (t.checklistItems && t.checklistItems.length) {
            html += `<div style="display:flex; flex-wrap:wrap; gap:0.5rem; margin:0.5rem 0;">`;
            t.checklistItems.forEach(item => {
                html += `<div style="background:#f5f5f5; padding:0.2rem 0.6rem; border-radius:16px;">
                    ${item.sortOrder}. ${item.itemText} ${item.isRequired ? '<span style="color:#c62828;">*</span>' : ''}
                    <button onclick="deleteChecklistItem(${item.id})" class="btn btn-sm" style="margin-left:0.3rem;">✖</button>
                </div>`;
            });
            html += `</div>`;
        }
        html += `<form class="inline-form" onsubmit="addChecklistItem(${t.id}, this); return false;">
                    <input type="text" name="itemText" class="text-input" placeholder="New checklist item" />
                    <input type="number" name="sortOrder" class="small-input" placeholder="Order" />
                    <label><input type="checkbox" name="isRequired" /> Required</label>
                    <button type="submit" class="btn btn-sm">Add item</button>
                </form>`;
        html += `</td></tr>`;
    });
    html += '</tbody></td>';
    document.getElementById('sops-list').innerHTML = html;
    
    document.querySelectorAll('.sop-row').forEach(row => {
        row.addEventListener('click', (event) => {
            if (event.target.closest('button, form')) return;
            openEditTemplate(row.dataset);
        });
    });
}

async function createTemplate(e) {
    e.preventDefault();
    const payload = {
        name: document.getElementById('template-name').value,
        serviceType: document.getElementById('template-service-type').value,
        serviceCatalogId: parseInt(document.getElementById('template-service-catalog').value) || null,
        description: document.getElementById('template-description').value || null,
        isActive: document.getElementById('template-active').checked
    };
    try {
        const res = await apiFetch('/sops', { method: 'POST', body: JSON.stringify(payload) });
        if (res.success) {
            showSuccess('Template created');
            document.getElementById('create-template-form').reset();
            loadTemplates();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function openEditTemplate(data) {
    editingTemplateId = parseInt(data.id);
    document.getElementById('edit-template-id').value = data.id;
    document.getElementById('edit-template-name').value = data.name;
    document.getElementById('edit-template-service-type').value = data.serviceType;
    document.getElementById('edit-template-service-catalog').value = data.serviceCatalogId;
    document.getElementById('edit-template-description').value = data.description;
    document.getElementById('edit-template-active').checked = data.active === 'true';
    document.getElementById('sop-modal').style.display = 'flex';
}

async function updateTemplate(e) {
    e.preventDefault();
    const payload = {
        name: document.getElementById('edit-template-name').value,
        serviceType: document.getElementById('edit-template-service-type').value,
        serviceCatalogId: parseInt(document.getElementById('edit-template-service-catalog').value) || null,
        description: document.getElementById('edit-template-description').value || null,
        isActive: document.getElementById('edit-template-active').checked
    };
    try {
        const res = await apiFetch(`/sops/${editingTemplateId}`, { method: 'PUT', body: JSON.stringify(payload) });
        if (res.success) {
            showSuccess('Template updated');
            closeSopModal();
            loadTemplates();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function deleteTemplate(id) {
    if (!confirm('Deactivate this SOP template? It will no longer be usable for new bookings.')) return;
    try {
        const res = await apiFetch(`/sops/${id}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess('Template deactivated');
            loadTemplates();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function addChecklistItem(templateId, form) {
    const itemText = form.itemText.value;
    if (!itemText) { showError('Item text required'); return; }
    const payload = {
        itemText: itemText,
        sortOrder: parseInt(form.sortOrder.value) || 0,
        isRequired: form.isRequired.checked
    };
    try {
        const res = await apiFetch(`/sops/${templateId}/items`, { method: 'POST', body: JSON.stringify(payload) });
        if (res.success) {
            showSuccess('Item added');
            loadTemplates();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function deleteChecklistItem(itemId) {
    if (!confirm('Remove this checklist item?')) return;
    try {
        const res = await apiFetch(`/sops/items/${itemId}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess('Item removed');
            loadTemplates();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function closeSopModal() { document.getElementById('sop-modal').style.display = 'none'; }

document.getElementById('create-template-form').addEventListener('submit', createTemplate);
document.getElementById('edit-template-form').addEventListener('submit', updateTemplate);
document.getElementById('sop-delete-form').addEventListener('submit', (e) => { e.preventDefault(); if (editingTemplateId) deleteTemplate(editingTemplateId); });

loadServicesForSelect();
loadTemplates();