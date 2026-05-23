// bookings.js
var currentFilter = 'all';

function filterBookings(filter) {
    currentFilter = filter;
    document.querySelectorAll('.btn-outline').forEach(function (b) { b.classList.remove('btn-outline'); });
    document.getElementById('filter-' + filter).classList.add('btn-outline');
    loadBookings();
}

function loadBookings() {
    var url = '/api/portal/bookings';
    if (currentFilter !== 'all') url += '?status=' + currentFilter;

    apiFetch(url).then(function (bookings) {
        renderBookings(bookings);
    }).catch(function (err) {
        showError(err.message || 'Failed to load bookings');
        document.getElementById('bookings-list').innerHTML = '<div class="empty-state"><p>Failed to load bookings. Please try again.</p></div>';
    });
}

function renderBookings(bookings) {
    var container = document.getElementById('bookings-list');
    if (bookings.length === 0) {
        container.innerHTML = '<div class="empty-state"><p>No bookings found.</p></div>';
        return;
    }

    var html = bookings.map(function (b) {
        var svcNames = (b.services || []).map(function (s) { return s.serviceName; }).join(', ');
        var estTotal = (b.services || []).reduce(function (sum, s) { return sum + (s.estimatedPrice || s.finalPrice || 0); }, 0);
        return '<div class="booking-card booking-card--' + b.status.toLowerCase() + '" onclick="window.location.href=\'booking-detail.html?id=' + b.id + '\'">' +
            '<div class="booking-card__header">' +
            '<div>' +
            '<div class="booking-card__title">' + b.serviceType + '</div>' +
            '<div class="booking-card__meta">' + formatDate(b.date) + ' at ' + formatTime(b.hour) + '</div>' +
            '</div>' +
            statusBadge(b.status) +
            '</div>' +
            '<div class="booking-card__meta">' + svcNames + '</div>' +
            '<div class="booking-card__footer">' +
            '<span>' + (b.siteName || 'No location specified') + '</span>' +
            '<span>' + formatCurrency(estTotal) + '</span>' +
            '</div>' +
            '</div>';
    }).join('');
    container.innerHTML = html;
}

document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('filter-all').classList.add('btn-outline');
    loadBookings();
});
