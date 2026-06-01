// services.js
let services = [];
let editingServiceId = null;

async function loadServices() {
    try {
        const res = await apiFetch('/services');
        if (res.success && res.data) {
            services = res.data;
            renderServices();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function renderServices() {
    if (!services.length) {
        document.getElementById('services-list').innerHTML = '<div class="alert alert-info">No services found.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>ID</th><th>Code</th><th>Name</th><th>Category</th><th>Unit</th><th>Base Price</th><th>Active</th></tr></thead><tbody>';
    services.forEach(s => {
        html += `<tr class="service-row ${!s.isActive ? 'row-inactive' : ''}" data-service-id="${s.id}" data-service-code="${s.catalogCode}" data-service-name="${s.name}" data-service-category="${s.category || ''}" data-service-unit="${s.unit || ''}" data-service-base-price="${s.basePrice || ''}" data-service-service-type="${s.serviceType}" data-service-active="${s.isActive}" style="cursor:pointer;">
            <td>${s.id}</td><td>${s.catalogCode}</td><td>${s.name}</td>
            <td>${s.category || '-'}</td><td>${s.unit || '-'}</td>
            <td>${s.basePrice ? s.basePrice.toFixed(2) : '-'}</td>
            <td><span class="badge ${s.isActive ? 'badge-active' : 'badge-inactive'}">${s.isActive ? 'Active' : 'Inactive'}</span></td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('services-list').innerHTML = html;
    
    document.querySelectorAll('.service-row').forEach(row => {
        row.addEventListener('click', () => openEditService(row.dataset));
    });
}

function openEditService(data) {
    editingServiceId = parseInt(data.serviceId);
    document.getElementById('service-modal-title').textContent = `Edit Service: ${data.serviceName}`;
    document.getElementById('service-id').value = data.serviceId;
    document.getElementById('service-code').value = data.serviceCode;
    document.getElementById('service-name').value = data.serviceName;
    document.getElementById('service-category').value = data.serviceCategory;
    document.getElementById('service-unit').value = data.serviceUnit;
    document.getElementById('service-base-price').value = data.serviceBasePrice;
    document.getElementById('service-service-type').value = data.serviceServiceType || 'Vehicle';
    document.getElementById('service-active').checked = data.serviceActive === 'true';
    document.getElementById('service-delete-form').style.display = 'block';
    document.getElementById('service-modal').style.display = 'flex';
}

function openCreateService() {
    editingServiceId = null;
    document.getElementById('service-modal-title').textContent = 'Create Service';
    document.getElementById('service-id').value = '';
    document.getElementById('service-code').value = '';
    document.getElementById('service-name').value = '';
    document.getElementById('service-category').value = '';
    document.getElementById('service-unit').value = '';
    document.getElementById('service-base-price').value = '';
    document.getElementById('service-service-type').value = 'SiteBased';
    document.getElementById('service-active').checked = true;
    document.getElementById('service-delete-form').style.display = 'none';
    document.getElementById('service-modal').style.display = 'flex';
}

async function saveService() {
    const payload = {
        catalogCode: document.getElementById('service-code').value,
        name: document.getElementById('service-name').value,
        category: document.getElementById('service-category').value || null,
        unit: document.getElementById('service-unit').value || null,
        basePrice: parseFloat(document.getElementById('service-base-price').value) || null,
        serviceType: document.getElementById('service-service-type').value,
        isActive: document.getElementById('service-active').checked
    };
    try {
        let res;
        if (editingServiceId) {
            res = await apiFetch(`/services/${editingServiceId}`, { method: 'PUT', body: JSON.stringify(payload) });
        } else {
            res = await apiFetch('/services', { method: 'POST', body: JSON.stringify(payload) });
        }
        if (res.success) {
            showSuccess(editingServiceId ? 'Service updated' : 'Service created');
            closeServiceModal();
            loadServices();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function deleteService() {
    if (!confirm('Delete this service?')) return;
    try {
        const res = await apiFetch(`/services/${editingServiceId}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess('Service deleted');
            closeServiceModal();
            loadServices();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function closeServiceModal() { document.getElementById('service-modal').style.display = 'none'; }

document.getElementById('new-service-btn').addEventListener('click', openCreateService);
document.getElementById('service-form').addEventListener('submit', (e) => { e.preventDefault(); saveService(); });
document.getElementById('service-delete-form').addEventListener('submit', (e) => { e.preventDefault(); deleteService(); });

loadServices();