// invoices.js
let currentPage = 1;
let currentSearch = '';

async function loadInvoices() {
    try {
        const url = `/invoices?page=${currentPage}&pageSize=50&search=${encodeURIComponent(currentSearch)}`;
        const res = await apiFetch(url);
        if (res.success && res.data) {
            renderInvoices(res.data);
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function renderInvoices(pagedResult) {
    const invoices = pagedResult.items || [];
    if (!invoices.length) {
        document.getElementById('invoices-list').innerHTML = '<div class="alert alert-info">' + __('empty_no_invoices') + '</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>' + __('th_invoice_number') + '</th><th>' + __('th_client') + '</th><th>' + __('th_status') + '</th><th>' + __('th_total') + '</th></tr></thead><tbody>';
    invoices.forEach(i => {
        html += `<tr style="cursor:pointer;" onclick="window.location.href='invoice-detail.html?id=${i.id}'">
            <td><a href="invoice-detail.html?id=${i.id}" class="link">${i.invoiceNumber}</a></td>
            <td>${i.clientName}</td>
            <td>${statusBadge(i.status)}</td>
            <td>${i.totalAmount.toFixed(2)}</td>
        </tr>`;
    });
    html += '</tbody></table>';
    if (pagedResult.totalPages > 1) {
        html += '<div class="pagination">';
        if (pagedResult.hasPreviousPage) html += `<button onclick="goToPage(${pagedResult.page - 1})" class="btn btn-sm">${__('ui_previous')}</button>`;
        html += `<span>${__('ui_page')} ${pagedResult.page} ${__('ui_of')} ${pagedResult.totalPages}</span>`;
        if (pagedResult.hasNextPage) html += `<button onclick="goToPage(${pagedResult.page + 1})" class="btn btn-sm">${__('ui_next')}</button>`;
        html += '</div>';
    }
    document.getElementById('invoices-list').innerHTML = html;
}

function goToPage(page) {
    currentPage = page;
    loadInvoices();
}

async function generateInvoice() {
    const bookingId = document.getElementById('generate-booking-id').value;
    if (!bookingId) {
        showError(__('msg_enter_booking_id'));
        return;
    }
    try {
        const res = await apiFetch(`/invoices/from-booking/${bookingId}`, { method: 'POST' });
        if (res.success && res.data) {
            window.location.href = `invoice-detail.html?id=${res.data.id}`;
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

document.getElementById('generate-invoice-btn').addEventListener('click', generateInvoice);
loadInvoices();
window.addEventListener('i18nReady', function () { loadInvoices(); });