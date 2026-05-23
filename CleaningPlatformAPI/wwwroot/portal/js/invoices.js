// portal/invoices.js
function renderInvoices() {
    const container = document.getElementById('invoices-list');
    if (MOCK_INVOICES.length === 0) {
        container.innerHTML = '<div class="empty-state"><p>No invoices yet.</p></div>';
        return;
    }
    let html = '<table class="portal-table"><thead><tr><th>Invoice</th><th>Issue Date</th><th>Due Date</th><th>Status</th><th>Amount</th></tr></thead><tbody>';
    MOCK_INVOICES.forEach(i => {
        html += `<tr onclick="window.location.href='invoice-detail.html?id=${i.id}'">
            <td><a href="invoice-detail.html?id=${i.id}" class="link">${i.number}</a></td>
            <td>${formatDate(i.issueDate)}</td>
            <td>${formatDate(i.dueDate)}</td>
            <td>${statusBadge(i.status)}</td>
            <td>${formatCurrency(i.totalAmount)}</td>
        </tr>`;
    });
    html += '</tbody></table>';
    container.innerHTML = html;
}

renderInvoices();
