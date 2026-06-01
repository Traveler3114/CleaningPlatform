// profile.js
function render(client) {
    var container = document.getElementById('profile-content');

    container.innerHTML =
        '<div class="card">' +
        '<div class="profile-avatar">' + client.avatarInitial + '</div>' +
        '<h2 class="card-title" style="margin-bottom:0.25rem;">' + client.name + '</h2>' +
        '<p style="color:var(--text-muted);font-size:0.85rem;margin-bottom:1rem;">Client since ' + formatDate(client.since) + '</p>' +
        '<div class="detail-grid two-col">' +
        '<div>' +
        '<div class="detail-row"><span>Email</span><span>' + (client.email || '—') + '</span></div>' +
        '<div class="detail-row"><span>Phone</span><span>' + (client.phone || '—') + '</span></div>' +
        (client.company ? '<div class="detail-row"><span>Company</span><span>' + client.company + '</span></div>' : '') +
        '</div>' +
        '</div>' +
        '</div>' +

        '<div class="card">' +
        '<h2 class="card-title">My Sites</h2>' +
        (client.sites.length === 0 ? '<div class="empty-state"><p>No sites saved.</p></div>' :
            '<table class="portal-table">' +
            '<thead><tr><th>Name</th><th>Address</th><th>City</th></tr></thead>' +
            '<tbody>' +
            client.sites.map(function (s) {
                return '<tr><td>' + s.name + '</td><td>' + s.address + '</td><td>' + (s.city || '') + '</td></tr>';
            }).join('') +
            '</tbody>' +
            '</table>'
        ) +
        '</div>' +

        '<div class="card">' +
        '<h2 class="card-title">Payment Methods</h2>' +
        '<table class="portal-table">' +
        '<thead><tr><th>Card</th><th>Expiry</th></tr></thead>' +
        '<tbody>' +
        '<tr><td>&bull;&bull;&bull;&bull; 4242</td><td>06/27</td></tr>' +
        '</tbody>' +
        '</table>' +
        '</div>' +

        '<div class="card" style="text-align:center;padding:1.5rem;">' +
        '<p style="color:var(--text-muted);font-size:0.85rem;">Want to make a new booking?</p>' +
        '<a href="/public/book.html" class="btn" style="margin-top:0.5rem;">Book a Slot</a>' +
        '</div>';
}

apiFetch('/api/portal/profile').then(function (data) {
    render(data);
}).catch(function (err) {
    showError(err.message || __('msg_failed_profile'));
});
