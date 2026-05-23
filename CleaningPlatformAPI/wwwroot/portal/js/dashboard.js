// dashboard.js
function renderKpis() {
    const upcoming = MOCK_UPCOMING.length;
    const completed = MOCK_COMPLETED.length;
    const totalSpent = MOCK_INVOICES.reduce((sum, i) => sum + i.totalAmount, 0);
    const dueAmount = MOCK_INVOICES
        .filter(i => i.status === 'sent' || i.status === 'partiallypaid')
        .reduce((sum, i) => sum + i.totalAmount - i.payments.reduce((p, pay) => p + pay.amount, 0), 0);

    document.getElementById('kpi-grid').innerHTML = `
        <div class="kpi-card"><span>Upcoming Bookings</span><strong>${upcoming}</strong></div>
        <div class="kpi-card"><span>Completed</span><strong>${completed}</strong></div>
        <div class="kpi-card"><span>Total Spent</span><strong>${formatCurrency(totalSpent)}</strong></div>
        <div class="kpi-card"><span>Outstanding</span><strong>${formatCurrency(dueAmount)}</strong></div>
    `;
}

function renderUpcomingBookings() {
    const container = document.getElementById('upcoming-bookings');
    if (MOCK_UPCOMING.length === 0) {
        container.innerHTML = '<div class="empty-state"><p>No upcoming bookings.</p></div>';
        return;
    }
    const html = MOCK_UPCOMING.map(b => `
        <div class="booking-card booking-card--${b.status}" onclick="window.location.href='booking-detail.html?id=${b.id}'">
            <div class="booking-card__header">
                <div class="booking-card__title">${b.serviceType} — ${formatDate(b.date)}</div>
                ${statusBadge(b.status)}
            </div>
            <div class="booking-card__meta">${b.time} &middot; ${b.services.map(s => s.name).join(', ')}</div>
            <div class="booking-card__footer">
                <span>${b.site || 'No site'}</span>
                <span>${formatCurrency(b.services.reduce((s, sv) => s + sv.price, 0))}</span>
            </div>
        </div>
    `).join('');
    container.innerHTML = html;
}

function renderRecentInvoices() {
    const container = document.getElementById('recent-invoices');
    if (MOCK_INVOICES.length === 0) {
        container.innerHTML = '<div class="empty-state"><p>No invoices yet.</p></div>';
        return;
    }
    let html = '<table class="portal-table"><thead><tr><th>Invoice</th><th>Date</th><th>Status</th><th>Amount</th></tr></thead><tbody>';
    MOCK_INVOICES.slice(0, 5).forEach(i => {
        html += `<tr onclick="window.location.href='invoice-detail.html?id=${i.id}'">
            <td><a href="invoice-detail.html?id=${i.id}" class="link">${i.number}</a></td>
            <td>${formatDate(i.issueDate)}</td>
            <td>${statusBadge(i.status)}</td>
            <td>${formatCurrency(i.totalAmount)}</td>
        </tr>`;
    });
    html += '</tbody></table>';
    container.innerHTML = html;
}

renderKpis();
renderUpcomingBookings();
renderRecentInvoices();
