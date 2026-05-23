// bookings.js
let currentFilter = 'all';

function filterBookings(filter) {
    currentFilter = filter;
    document.querySelectorAll('.btn-outline').forEach(b => b.classList.remove('btn-outline'));
    document.getElementById('filter-' + filter).classList.add('btn-outline');
    renderBookings();
}

function renderBookings() {
    const container = document.getElementById('bookings-list');
    let filtered = MOCK_BOOKINGS;
    if (currentFilter === 'upcoming') filtered = MOCK_UPCOMING;
    else if (currentFilter === 'completed') filtered = MOCK_COMPLETED;

    if (filtered.length === 0) {
        container.innerHTML = '<div class="empty-state"><p>No bookings found.</p></div>';
        return;
    }

    const html = filtered.map(b => {
        const serviceNames = b.services.map(s => s.name).join(', ');
        return `
        <div class="booking-card booking-card--${b.status}" onclick="window.location.href='booking-detail.html?id=${b.id}'">
            <div class="booking-card__header">
                <div>
                    <div class="booking-card__title">${b.serviceType}</div>
                    <div class="booking-card__meta">${formatDate(b.date)} at ${b.time}</div>
                </div>
                ${statusBadge(b.status)}
            </div>
            <div class="booking-card__meta">${serviceNames}</div>
            <div class="booking-card__footer">
                <span>${b.site || 'No location specified'}</span>
                <span>${formatCurrency(b.services.reduce((s, sv) => s + sv.price, 0))}</span>
            </div>
        </div>`;
    }).join('');
    container.innerHTML = html;
}

document.getElementById('filter-all').classList.add('btn-outline');
renderBookings();
