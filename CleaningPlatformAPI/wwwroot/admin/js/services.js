// services.js
let services = [];

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
        document.getElementById('services-list').innerHTML = '<div class="alert alert-info">' + __('empty_no_services') + '</div>';
        return;
    }
    let html = `<table class="admin-table"><thead><tr><th>${__('th_id')}</th><th>${__('th_code')}</th><th>${__('th_name')}</th><th>${__('th_category')}</th><th>${__('th_unit')}</th><th>${__('th_base_price')}</th><th>${__('th_approx_time')}</th><th>${__('th_active')}</th></tr></thead><tbody>`;
    services.forEach(s => {
        const approxStr = s.approxTime ? `${s.approxTime} min` : '-';
        html += `<tr class="service-row ${!s.isActive ? 'row-inactive' : ''}" data-service-id="${s.id}" style="cursor:pointer;">
            <td>${s.id}</td><td>${s.catalogCode}</td><td>${s.name}</td>
            <td>${s.category || '-'}</td><td>${s.unit || '-'}</td>
            <td>${s.basePrice ? s.basePrice.toFixed(2) : '-'}</td>
            <td>${approxStr}</td>
            <td><span class="badge ${s.isActive ? 'badge-active' : 'badge-inactive'}">${s.isActive ? __('status_active') : __('status_inactive')}</span></td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('services-list').innerHTML = html;

    document.querySelectorAll('.service-row').forEach(row => {
        row.addEventListener('click', () => {
            const id = row.dataset.serviceId;
            window.location.href = `service-detail.html?id=${id}`;
        });
    });
}

loadServices();
window.addEventListener('i18nReady', function () { loadServices(); });
