// booking-detail.js
function render(booking) {
    var container = document.getElementById('booking-detail');

    container.innerHTML =
        '<div class="card">' +
        '<div class="invoice-header">' +
        '<div>' +
        '<h2 class="card-title" style="margin-bottom:0.25rem;">' + booking.serviceType + ' Booking</h2>' +
        '<p style="color:var(--text-muted);font-size:0.85rem;">Booking #' + booking.id + '</p>' +
        '</div>' +
        statusBadge(booking.status) +
        '</div>' +
        '<div class="detail-grid two-col">' +
        '<div>' +
        '<div class="detail-row"><span>Date</span><span>' + formatDate(booking.date) + '</span></div>' +
        '<div class="detail-row"><span>Time</span><span>' + formatTime(booking.hour) + '</span></div>' +
        '<div class="detail-row"><span>Location</span><span>' + (booking.siteName || 'On-site (vehicle/boat)') + '</span></div>' +
        '</div>' +
        '<div>' +
        '<div class="detail-row"><span>Status</span><span>' + booking.status + '</span></div>' +
        '<div class="detail-row"><span>Assigned to</span><span>' + (booking.assignedEmployees.length ? booking.assignedEmployees.map(function (e) { return e.fullName; }).join(', ') : 'Not yet assigned') + '</span></div>' +
        '<div class="detail-row"><span>Booked on</span><span>' + formatDate(booking.createdAt) + '</span></div>' +
        '</div>' +
        '</div>' +
        '</div>' +

        '<div class="card">' +
        '<h2 class="card-title">Services</h2>' +
        '<table class="portal-table">' +
        '<thead><tr><th>Service</th><th>Qty</th><th>Price</th></tr></thead>' +
        '<tbody>' +
        booking.services.map(function (s) {
            var price = s.finalPrice || s.estimatedPrice || 0;
            return '<tr><td>' + s.serviceName + '</td><td>' + s.quantity + '</td><td>' + formatCurrency(price) + '</td></tr>';
        }).join('') +
        '</tbody>' +
        '</table>' +
        '</div>' +

        (booking.notes ? '' +
            '<div class="card">' +
            '<h2 class="card-title">Notes</h2>' +
            '<p style="color:var(--text-muted);font-size:0.88rem;">' + booking.notes + '</p>' +
            '</div>'
        : '') +

        '<div class="card" style="text-align:center;padding:1.5rem;">' +
        '<p style="color:var(--text-muted);font-size:0.85rem;margin-bottom:0.75rem;">Need to make changes?</p>' +
        '<button class="btn btn-outline" disabled>Cancel Booking</button>' +
        '<span style="display:inline-block;width:0.5rem;"></span>' +
        '<button class="btn btn-outline" disabled>Reschedule</button>' +
        '<p style="font-size:0.75rem;color:var(--text-muted);margin-top:0.5rem;">(Contact us to make changes)</p>' +
        '</div>';
}

var params = new URLSearchParams(window.location.search);
var id = parseInt(params.get('id'));

if (!id) {
    document.getElementById('booking-detail').innerHTML = '<div class="alert alert-danger">Invalid booking ID.</div>';
} else {
    apiFetch('/api/portal/bookings/' + id).then(function (data) {
        render(data);
    }).catch(function (err) {
        document.getElementById('booking-detail').innerHTML = '<div class="alert alert-danger">' + err.message + '</div>';
    });
}
