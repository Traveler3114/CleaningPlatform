// invoice-detail.js
function render(invoice) {
    var container = document.getElementById('invoice-detail');
    var totalPaid = invoice.paidAmount;
    var balance = invoice.balanceDue;
    var isPayable = invoice.status === 'Sent' || invoice.status === 'PartiallyPaid';

    container.innerHTML =
        '<div class="card">' +
        '<div class="invoice-header">' +
        '<div>' +
        '<h2 class="card-title" style="margin-bottom:0.15rem;">' + invoice.invoiceNumber + '</h2>' +
        '<p style="color:var(--text-muted);font-size:0.85rem;">Issued ' + formatDate(invoice.issueDate) + ' &middot; Due ' + formatDate(invoice.dueDate) + '</p>' +
        '</div>' +
        '<div style="text-align:right;">' +
        statusBadge(invoice.status) +
        '<div class="invoice-total">' + formatCurrency(invoice.totalAmount) + '</div>' +
        '</div>' +
        '</div>' +
        '</div>' +

        '<div class="card">' +
        '<h2 class="card-title">Line Items</h2>' +
        '<table class="portal-table">' +
        '<thead><tr><th>Description</th><th>Qty</th><th>Unit Price</th><th>Total</th></tr></thead>' +
        '<tbody>' +
        invoice.lines.map(function (item) {
            return '<tr><td>' + item.description + '</td><td>' + item.quantity + '</td><td>' + formatCurrency(item.unitPrice) + '</td><td>' + formatCurrency(item.quantity * item.unitPrice) + '</td></tr>';
        }).join('') +
        '</tbody>' +
        '</table>' +
        '<div style="max-width:300px;margin-left:auto;">' +
        '<div class="detail-row"><span>Subtotal</span><span>' + formatCurrency(invoice.subTotal) + '</span></div>' +
        (invoice.discountAmount > 0 ? '<div class="detail-row"><span>Discount</span><span>-' + formatCurrency(invoice.discountAmount) + '</span></div>' : '') +
        '<div class="detail-row"><span>VAT (' + invoice.vatPct + '%)</span><span>' + formatCurrency(invoice.vatAmount) + '</span></div>' +
        '<div class="detail-row" style="font-weight:700;font-size:1rem;"><span>Total</span><span>' + formatCurrency(invoice.totalAmount) + '</span></div>' +
        '</div>' +
        '</div>' +

        (invoice.payments.length > 0 ?
            '<div class="card">' +
            '<h2 class="card-title">Payment History</h2>' +
            '<table class="portal-table">' +
            '<thead><tr><th>Date</th><th>Method</th><th>Amount</th></tr></thead>' +
            '<tbody>' +
            invoice.payments.map(function (p) {
                return '<tr><td>' + formatDate(p.paymentDate) + '</td><td>' + p.method + '</td><td>' + formatCurrency(p.amount) + '</td></tr>';
            }).join('') +
            '</tbody>' +
            '</table>' +
            '</div>'
        : '') +

        (isPayable ?
            '<div class="card" style="text-align:center;padding:1.5rem;">' +
            '<p style="color:var(--text-muted);font-size:0.88rem;margin-bottom:0.25rem;">' +
            (balance < invoice.totalAmount ? 'Remaining balance: ' + formatCurrency(balance) : '') +
            '</p>' +
            '<button class="btn btn-success" disabled style="margin-top:0.5rem;">Pay Now &mdash; ' + formatCurrency(balance) + '</button>' +
            '<p style="font-size:0.75rem;color:var(--text-muted);margin-top:0.5rem;">(Online payment coming soon)</p>' +
            '</div>'
        : '') +

        '<div style="text-align:center;padding:0.5rem 0 1.5rem;">' +
        '<button class="btn btn-outline btn-sm" onclick="window.print()">Print / Download PDF</button>' +
        '</div>';
}

var params = new URLSearchParams(window.location.search);
var id = parseInt(params.get('id'));

if (!id) {
    document.getElementById('invoice-detail').innerHTML = '<div class="alert alert-danger">Invalid invoice ID.</div>';
} else {
    apiFetch('/api/portal/invoices/' + id).then(function (data) {
        render(data);
    }).catch(function (err) {
        document.getElementById('invoice-detail').innerHTML = '<div class="alert alert-danger">' + err.message + '</div>';
    });
}
