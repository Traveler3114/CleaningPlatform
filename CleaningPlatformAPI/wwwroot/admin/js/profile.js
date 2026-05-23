// profile.js
let currentUser = null;

async function loadProfile() {
    try {
        const res = await apiFetch('/users/me');
        if (res.success && res.data) {
            currentUser = res.data;
            renderProfile();
            loadAssignedBookings();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function loadAssignedBookings() {
    try {
        const res = await apiFetch('/bookings/employee/assigned');
        if (res.success && res.data) {
            renderBookings(res.data);
        }
    } catch(e) { console.error(e); }
}

function renderProfile() {
    const container = document.getElementById('profile-content');
    container.innerHTML = `
        <section class="detail-section">
            <h2 class="section-title">Account Info</h2>
            <div class="form-grid two-col">
                <label>Username <input class="text-input" value="${currentUser.username}" readonly /></label>
                <label>Full Name <input class="text-input" value="${currentUser.firstName} ${currentUser.lastName}" readonly /></label>
                <label>Role <input class="text-input" value="${currentUser.role}" readonly /></label>
            </div>
        </section>
        <section class="detail-section" style="margin-top:1rem;">
            <h2 class="section-title">Change Password</h2>
            <form id="change-password-form" class="form-grid">
                <label>Current Password <input type="password" id="current-password" class="text-input" required /></label>
                <label>New Password <input type="password" id="new-password" class="text-input" required minlength="8" /></label>
                <small>At least 8 characters, including uppercase, lowercase, and a number.</small>
                <div><button type="submit" class="btn btn-sm">Change Password</button></div>
            </form>
        </section>
        <section class="detail-section" style="margin-top:1rem;">
            <h2 class="section-title">My Assigned Bookings</h2>
            <div id="assigned-bookings"></div>
        </section>
    `;
    document.getElementById('change-password-form').addEventListener('submit', changePassword);
}

function renderBookings(bookings) {
    const container = document.getElementById('assigned-bookings');
    if (!bookings.length) {
        container.innerHTML = '<p>No bookings assigned.</p>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>ID</th><th>Client</th><th>Date</th><th>Hour</th><th>Status</th><th></th></tr></thead><tbody>';
    bookings.forEach(b => {
        html += `<tr>
            <td>${b.id}</td>
            <td>${b.clientName}</td>
            <td>${b.date.split('T')[0]}</td>
            <td>${b.hour}:00</div></div></td>
            <td><span class="badge badge-${b.status.toLowerCase()}">${b.status}</span></td>
            <td><a href="booking-detail.html?id=${b.id}" class="btn btn-sm">Open</a></td>
        </tr>`;
    });
    html += '</tbody></table>';
    container.innerHTML = html;
}

async function changePassword(e) {
    e.preventDefault();
    const currentPassword = document.getElementById('current-password').value;
    const newPassword = document.getElementById('new-password').value;
    try {
        const res = await apiFetch('/auth/change-password', {
            method: 'POST',
            body: JSON.stringify({ currentPassword, newPassword })
        });
        if (res.success) {
            showSuccess('Password changed. Please login again.');
            setTimeout(() => logout(), 2000);
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

loadProfile();