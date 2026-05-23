// invoice-detail.js
const urlParams = new URLSearchParams(window.location.search);
const invoiceId = urlParams.get('id');

async function loadInvoice() {
    if (!invoiceId) {
        document.getElementById('invoice-detail').innerHTML = '<div class="alert alert-info">No invoice ID provided.</div>';
        return;
    }
    try {
        const res = await apiFetch(`/invoices/${invoiceId}`);
        if (res.success && res.data) {
            renderInvoice(res.data);
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function renderInvoice(invoice) {
    const lines = invoice.lines || [];
    const payments = invoice.payments || [];
    document.getElementById('invoice-detail').innerHTML = `
        <div class="page-header">
            <div><h1>${invoice.invoiceNumber}</h1><span class="badge badge-${invoice.status.toLowerCase()}">${invoice.status}</span></div>
        </div>
        <section class="stats-grid">
            <div class="kpi-card"><span>Total</span><strong>${invoice.totalAmount.toFixed(2)}</strong></div>
            <div class="kpi-card"><span>Balance due</span><strong>${invoice.balanceDue.toFixed(2)}</strong></div>
        </section>
        <section class="card-lite">
            <h2 class="section-title">Update status</h2>
            <div class="inline-form wrap">
                <select id="status-select" class="status-select">
                    <option ${invoice.status === 'Draft' ? 'selected' : ''}>Draft</option>
                    <option ${invoice.status === 'Sent' ? 'selected' : ''}>Sent</option>
                    <option ${invoice.status === 'PartiallyPaid' ? 'selected' : ''}>PartiallyPaid</option>
                    <option ${invoice.status === 'Paid' ? 'selected' : ''}>Paid</option>
                    <option ${invoice.status === 'Overdue' ? 'selected' : ''}>Overdue</option>
                    <option ${invoice.status === 'WrittenOff' ? 'selected' : ''}>WrittenOff</option>
                </select>
                <button id="update-status-btn" class="btn btn-sm">Update Status</button>
            </div>
        </section>
        <section class="card-lite">
            <h2 class="section-title">Record payment</h2>
            <div class="form-grid two-col">
                <label>Payment date <input type="date" id="payment-date" class="text-input" /></label>
                <label>Amount <input type="number" id="payment-amount" step="0.01" class="text-input" /></label>
                <label>Method <input type="text" id="payment-method" value="BankTransfer" class="text-input" /></label>
                <label>Reference <input type="text" id="payment-reference" class="text-input" /></label>
                <label class="full-span">Notes <input type="text" id="payment-notes" class="text-input" /></label>
                <div><button id="record-payment-btn" class="btn btn-sm">Record Payment</button></div>
            </div>
        </section>
        <section class="detail-section">
            <h2 class="section-title">Invoice Lines</h2>
            <table class="admin-table"><thead><tr><th>Description</th><th>Quantity</th><th>Unit Price</th><th>Total</th></tr></thead><tbody>
                ${lines.map(l => `<tr><td>${l.description}</td><td>${l.quantity}</td><td>${l.unitPrice.toFixed(2)}</td><td>${l.lineTotalAmount.toFixed(2)}</td></tr>`).join('')}
            </tbody>}</table>
        </section>
        <section class="detail-section">
            <h2 class="section-title">Payments</h2>
            ${payments.length === 0 ? '<div class="alert alert-info">No payments recorded.</div>' : `
                <table class="admin-table"><thead><tr><th>Date</th><th>Amount</th><th>Method</th><th>Reference</th><th>Notes</th></tr></thead><tbody>
                ${payments.map(p => `<tr><td>${p.paymentDate.split('T')[0]}</td><td>${p.amount.toFixed(2)}</td><td>${p.method}</td><td>${p.reference || '-'}</td><td>${p.notes || '-'}</td></tr>`).join('')}
                </tbody>}</table>
            `}
        </section>
    `;
    document.getElementById('update-status-btn').addEventListener('click', updateStatus);
    document.getElementById('record-payment-btn').addEventListener('click', recordPayment);
}

async function updateStatus() {
    const newStatus = document.getElementById('status-select').value;
    try {
        const res = await apiFetch(`/invoices/${invoiceId}/status`, {
            method: 'PUT',
            body: JSON.stringify({ status: newStatus })
        });
        if (res.success) {
            showSuccess('Status updated');
            loadInvoice();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function recordPayment() {
    const amount = parseFloat(document.getElementById('payment-amount').value);
    if (!amount || amount <= 0) {
        showError('Please enter a valid amount');
        return;
    }
    const payload = {
        paymentDate: document.getElementById('payment-date').value || new Date().toISOString().split('T')[0],
        amount: amount,
        method: document.getElementById('payment-method').value,
        reference: document.getElementById('payment-reference').value || null,
        notes: document.getElementById('payment-notes').value || null
    };
    try {
        const res = await apiFetch(`/invoices/${invoiceId}/payments`, {
            method: 'POST',
            body: JSON.stringify(payload)
        });
        if (res.success) {
            showSuccess('Payment recorded');
            document.getElementById('payment-amount').value = '';
            document.getElementById('payment-reference').value = '';
            document.getElementById('payment-notes').value = '';
            loadInvoice();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

loadInvoice();