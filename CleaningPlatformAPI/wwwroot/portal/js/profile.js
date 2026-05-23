// profile.js
function render() {
    const client = MOCK_CLIENT;
    const container = document.getElementById('profile-content');

    container.innerHTML = `
        <div class="card">
            <div class="profile-avatar">${client.avatarInitial}</div>
            <h2 class="card-title" style="margin-bottom:0.25rem;">${client.name}</h2>
            <p style="color:var(--text-muted);font-size:0.85rem;margin-bottom:1rem;">Client since ${formatDate(client.since)}</p>

            <div class="detail-grid two-col">
                <div>
                    <div class="detail-row"><span>Email</span><span>${client.email}</span></div>
                    <div class="detail-row"><span>Phone</span><span>${client.phone}</span></div>
                    ${client.company ? `<div class="detail-row"><span>Company</span><span>${client.company}</span></div>` : ''}
                </div>
            </div>
        </div>

        <div class="card">
            <h2 class="card-title">My Sites</h2>
            ${MOCK_SITES.length === 0 ? '<div class="empty-state"><p>No sites saved.</p></div>' : `
                <table class="portal-table">
                    <thead><tr><th>Name</th><th>Address</th></tr></thead>
                    <tbody>
                        ${MOCK_SITES.map(s => `
                            <tr><td>${s.name}</td><td>${s.address}</td></tr>
                        `).join('')}
                    </tbody>
                </table>
            `}
        </div>

        <div class="card">
            <h2 class="card-title">Payment Methods</h2>
            ${MOCK_PAYMENT_METHODS.length === 0 ? '<div class="empty-state"><p>No payment methods saved.</p></div>' : `
                <table class="portal-table">
                    <thead><tr><th>Card</th><th>Expiry</th></tr></thead>
                    <tbody>
                        ${MOCK_PAYMENT_METHODS.map(p => `
                            <tr><td>&bull;&bull;&bull;&bull; ${p.last4}</td><td>${p.expiry}</td></tr>
                        `).join('')}
                    </tbody>
                </table>
            `}
        </div>

        <div class="card" style="text-align:center;padding:1.5rem;">
            <p style="color:var(--text-muted);font-size:0.85rem;">Want to make a new booking?</p>
            <a href="/public/book.html" class="btn" style="margin-top:0.5rem;">Book a Slot</a>
        </div>
    `;
}

render();
