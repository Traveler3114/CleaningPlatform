// portal/invoices.js
function renderInvoices(invoices) {
    var container = document.getElementById('invoices-list');
    if (invoices.length === 0) {
        container.innerHTML = '<div class="empty-state"><p>No invoices yet.</p></div>';
        return;
    }
    var html = '<table class="portal-table"><thead><tr><th>Invoice</th><th>Issue Date</th><th>Due Date</th><th>Status</th><th>Amount</th></tr></thead><tbody>';
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

apiFetch('/api/portal/invoices').then(function (data) {
    renderInvoices(data);
}).catch(function (err) {
    showError(err.message || 'Failed to load invoices');
    document.getElementById('invoices-list').innerHTML = '<div class="empty-state"><p>Failed to load invoices.</p></div>';
});
