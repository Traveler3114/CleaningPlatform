// clients.js
let currentPage = 1;
let currentSearch = '';
let currentType = '';

async function loadClients() {
    try {
        const url = `/clients?page=${currentPage}&pageSize=50&search=${encodeURIComponent(currentSearch)}&type=${encodeURIComponent(currentType)}`;
        const res = await apiFetch(url);
        if (res.success && res.data) {
            renderClients(res.data);
            renderKPIs(res.data.items);
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function renderClients(pagedResult) {
    const clients = pagedResult.items || [];
    if (!clients.length) {
        document.getElementById('clients-list').innerHTML = '<div class="alert alert-info">No clients found.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>Name</th><th>Type</th><th>Primary Contact</th><th>Phone</th><th>Total Bookings</th><th>Created</th></tr></thead><tbody>';
    clients.forEach(c => {
        html += `<tr class="client-row" data-id="${c.id}" style="cursor:pointer;">
            <td>${c.clientName}</td>
            <td><span class="badge badge-${c.type.toLowerCase()}">${c.type}</span></td>
            <td>${c.primaryContactName || '-'}</td>
            <td>${c.primaryContactPhone || '-'}</td>
            <td>${c.totalBookings}</td>
            <td>${new Date(c.createdAt).toLocaleDateString()}</td>
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
    document.getElementById('clients-list').innerHTML = html;
    
    document.querySelectorAll('.client-row').forEach(row => {
        row.addEventListener('click', () => {
            window.location.href = `client-detail.html?id=${row.dataset.id}`;
        });
    });
}

function renderKPIs(clients) {
    const total = clients.length;
    const persons = clients.filter(c => c.type === 'Person').length;
    const businesses = clients.filter(c => c.type === 'Business').length;
    document.getElementById('kpi-grid').innerHTML = `
        <div class="kpi-card"><span>Total clients</span><strong>${total}</strong></div>
        <div class="kpi-card"><span>Persons</span><strong>${persons}</strong></div>
        <div class="kpi-card"><span>Businesses</span><strong>${businesses}</strong></div>
    `;
}

function goToPage(page) { currentPage = page; loadClients(); }

document.getElementById('new-client-btn').addEventListener('click', () => {
    window.location.href = 'client-detail.html?new=1';
});
document.getElementById('apply-filter').addEventListener('click', () => {
    currentSearch = document.getElementById('search-input').value;
    currentType = document.getElementById('type-filter').value;
    currentPage = 1;
    loadClients();
});
loadClients();