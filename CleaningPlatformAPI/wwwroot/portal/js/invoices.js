// portal/invoices.js
var _cacheInvoices = [];

function renderInvoices(invoices) {
    var container = document.getElementById('invoices-list');
    if (invoices.length === 0) {
        container.innerHTML = '<div class="empty-state"><p>' + __('empty_no_invoices_yet') + '</p></div>';
        return;
    }
    var html = '<table class="portal-table"><thead><tr><th>' + __('nav_invoices') + '</th><th>' + __('th_date') + '</th><th>' + __('th_due') + '</th><th>' + __('th_status') + '</th><th>' + __('th_amount') + '</th></tr></thead><tbody>';
    invoices.forEach(function (i) {
        html += '<tr onclick="window.location.href=\'invoice-detail.html?id=' + i.id + '\'">' +
            '<td><a href="invoice-detail.html?id=' + i.id + '" class="link">' + i.invoiceNumber + '</a></td>' +
            '<td>' + formatDate(i.issueDate) + '</td>' +
            '<td>' + formatDate(i.dueDate) + '</td>' +
            '<td>' + statusBadge(i.status) + '</td>' +
            '<td>' + formatCurrency(i.totalAmount) + '</td>' +
            '</tr>';
    });
    html += '</tbody></table>';
    container.innerHTML = html;
}

function loadPortalInvoices() {
    apiFetch('/api/portal/invoices').then(function (data) {
        _cacheInvoices = data;
        renderInvoices(data);
    }).catch(function (err) {
        showError(err.message || 'Failed to load invoices');
        document.getElementById('invoices-list').innerHTML = '<div class="empty-state"><p>' + __('msg_failed_invoices_load') + '</p></div>';
    });
}

loadPortalInvoices();
window.addEventListener('i18nReady', function () {
    if (_cacheInvoices.length) {
        renderInvoices(_cacheInvoices);
    } else {
        loadPortalInvoices();
    }
});
