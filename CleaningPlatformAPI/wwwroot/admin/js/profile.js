// profile.js
let currentUser = null;

async function loadProfile() {
    try {
        const res = await apiFetch('/employees/me');
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
            <h2 class="section-title">${__('label_account_info')}</h2>
            <div class="form-grid two-col">
                <label>${__('label_username')} <input class="text-input" value="${currentUser.username}" readonly /></label>
                <label>${__('label_full_name')} <input class="text-input" value="${currentUser.firstName} ${currentUser.lastName}" readonly /></label>
                <label>${__('label_role')} <input class="text-input" value="${currentUser.role}" readonly /></label>
            </div>
        </section>
        <section class="detail-section" style="margin-top:1rem;">
            <h2 class="section-title">${__('label_change_password')}</h2>
            <form id="change-password-form" class="form-grid">
                <label>${__('label_current_password')} <input type="password" id="current-password" class="text-input" required /></label>
                <label>${__('label_new_password')} <input type="password" id="new-password" class="text-input" required minlength="8" /></label>
                <small>At least 8 characters, including uppercase, lowercase, and a number.</small>
                <div><button type="submit" class="btn btn-sm">${__('btn_change_password')}</button></div>
            </form>
        </section>
        <section class="detail-section" style="margin-top:1rem;">
            <h2 class="section-title">${__('label_my_assigned_bookings')}</h2>
            <div id="assigned-bookings"></div>
        </section>
    `;
    document.getElementById('change-password-form').addEventListener('submit', changePassword);
}

function renderBookings(bookings) {
    const container = document.getElementById('assigned-bookings');
    if (!bookings.length) {
        container.innerHTML = '<p>' + __('empty_no_bookings_assigned') + '</p>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>' + __('th_id') + '</th><th>' + __('th_client') + '</th><th>' + __('th_date') + '</th><th>' + __('label_hour') + '</th><th>' + __('th_status') + '</th><th></th></tr></thead><tbody>';
    bookings.forEach(b => {
        html += `<tr>
            <td>${b.id}</td>
            <td>${b.clientName}</td>
            <td>${b.date.split('T')[0]}</td>
            <td>${b.hour}:00</div></div></td>
            <td>${statusBadge(b.status)}</td>
            <td><a href="booking-detail.html?id=${b.id}" class="btn btn-sm">${__('btn_open')}</a></td>
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
            showSuccess(__('msg_password_changed'));
            setTimeout(() => logout(), 2000);
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

loadProfile();
window.addEventListener('i18nReady', function () { loadProfile(); });