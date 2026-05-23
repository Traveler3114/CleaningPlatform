// dashboard.js
function renderKpis(data) {
    document.getElementById('kpi-grid').innerHTML =
        '<div class="kpi-card"><span>Upcoming Bookings</span><strong>' + data.upcomingBookings + '</strong></div>' +
        '<div class="kpi-card"><span>Completed</span><strong>' + data.completedBookings + '</strong></div>' +
        '<div class="kpi-card"><span>Total Spent</span><strong>' + formatCurrency(data.totalSpent) + '</strong></div>' +
        '<div class="kpi-card"><span>Outstanding</span><strong>' + formatCurrency(data.outstandingAmount) + '</strong></div>';
}

function renderUpcomingBookings(bookings) {
    var container = document.getElementById('upcoming-bookings');
    if (bookings.length === 0) {
        container.innerHTML = '<div class="empty-state"><p>No upcoming bookings.</p></div>';
        return;
    }
    var html = bookings.map(function (b) {
        return '<div class="booking-card booking-card--' + b.status.toLowerCase() + '" onclick="window.location.href=\'booking-detail.html?id=' + b.id + '\'">' +
            '<div class="booking-card__header">' +
            '<div class="booking-card__title">' + b.serviceType + ' &mdash; ' + formatDate(b.date) + '</div>' +
            statusBadge(b.status) +
            '</div>' +
            '<div class="booking-card__meta">' + formatTime(b.hour) + ' &middot; ' + b.services + '</div>' +
            '<div class="booking-card__footer">' +
            '<span>' + (b.siteName || 'No site') + '</span>' +
            '<span>' + formatCurrency(b.estimatedTotal) + '</span>' +
            '</div>' +
            '</div>';
    }).join('');
    container.innerHTML = html;
}

function renderRecentInvoices(invoices) {
    var container = document.getElementById('recent-invoices');
    if (invoices.length === 0) {
        container.innerHTML = '<div class="empty-state"><p>No invoices yet.</p></div>';
        return;
    }
    var html = '<table class="portal-table"><thead><tr><th>Invoice</th><th>Date</th><th>Status</th><th>Amount</th></tr></thead><tbody>';
    invoices.slice(0, 5).forEach(function (i) {
        html += '<tr onclick="window.location.href=\'invoice-detail.html?id=' + i.id + '\'">' +
            '<td><a href="invoice-detail.html?id=' + i.id + '" class="link">' + i.number + '</a></td>' +
            '<td>' + formatDate(i.issueDate) + '</td>' +
            '<td>' + statusBadge(i.status) + '</td>' +
            '<td>' + formatCurrency(i.totalAmount) + '</td>' +
            '</tr>';
    });
    html += '</tbody></table>';
    container.innerHTML = html;
}

apiFetch('/api/portal/dashboard').then(function (data) {
    renderKpis(data);
    renderUpcomingBookings(data.upcomingBookingList);
    renderRecentInvoices(data.recentInvoices);
}).catch(function (err) {
    showError(err.message || 'Failed to load dashboard');
});
