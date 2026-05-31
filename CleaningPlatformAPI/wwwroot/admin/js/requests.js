// requests.js
let currentPage = 1;
let currentStatus = '';

const statusMap = {
    New: { label: 'New', cls: 'new' },
    AdminReviewed: { label: 'Admin Reviewed', cls: 'adminreviewed' },
    SentToCustomer: { label: 'Sent to Customer', cls: 'senttocustomer' },
    CustomerConfirmed: { label: 'Customer Confirmed', cls: 'customerconfirmed' },
    Cancelled: { label: 'Cancelled', cls: 'cancelled' },
    Converted: { label: 'Converted', cls: 'converted' }
};

function fmtStatus(status) {
    const s = statusMap[status] || { label: status, cls: 'info' };
    return `<span class="badge badge-${s.cls}">${s.label}</span>`;
}

async function loadRequests() {
    try {
        let url = `/booking-requests?page=${currentPage}&pageSize=50`;
        if (currentStatus) url += `&status=${encodeURIComponent(currentStatus)}`;
        const res = await apiFetch(url);
        if (res.success && res.data) {
            renderRequests(res.data);
        } else showError(res.message);
    } catch (e) { showError(e.message); }
}

function renderRequests(pagedResult) {
    const items = pagedResult.items || [];
    if (!items.length) {
        document.getElementById('requests-list').innerHTML = '<div class="alert alert-info">No requests found.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>ID</th><th>Name</th><th>Phone</th><th>Email</th><th>Services</th><th>Est. Price</th><th>Status</th><th>Created</th></tr></thead><tbody>';
    items.forEach(r => {
        const services = (r.services || []).map(s => s.serviceName).join(', ') || '-';
        const price = r.estimatedPrice ? r.estimatedPrice.toFixed(2) : '-';
        const created = r.createdAt ? r.createdAt.split('T')[0] : '-';
        html += `<tr style="cursor:pointer;" onclick="window.location='request-detail.html?id=${r.id}'">
            <td><a href="request-detail.html?id=${r.id}" class="link">${r.id}</a></td>
            <td>${escHtml(r.contactName)}</td>
            <td>${escHtml(r.phone)}</td>
            <td>${escHtml(r.email)}</td>
            <td>${services}</td>
            <td>${price}</td>
            <td>${fmtStatus(r.status)}</td>
            <td>${created}</td>
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
    document.getElementById('requests-list').innerHTML = html;
}

function goToPage(page) {
    currentPage = page;
    loadRequests();
}

function escHtml(str) {
    if (!str) return '';
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

document.getElementById('filter-btn').addEventListener('click', () => {
    currentStatus = document.getElementById('status-filter').value;
    currentPage = 1;
    loadRequests();
});

loadRequests();
