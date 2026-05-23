// users.js
let users = [];

async function loadUsers() {
    try {
        const res = await apiFetch('/employees');
        if (res.success && res.data) {
            users = res.data;
            renderUsers();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function loadRolesForSelect() {
    try {
        const res = await apiFetch('/roles');
        if (res.success && res.data) {
            const select = document.getElementById('user-role');
            select.innerHTML = '';
            res.data.forEach(r => {
                select.innerHTML += `<option value="${r.name}">${r.name}</option>`;
            });
        }
    } catch(e) { console.error(e); }
}

function renderUsers() {
    if (!users.length) {
        document.getElementById('users-list').innerHTML = '<div class="alert alert-info">No users found.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>Name</th><th>Username</th><th>Role</th><th>Active</th></tr></thead><tbody>';
    users.forEach(u => {
        html += `<tr class="${!u.isActive ? 'row-inactive' : ''} user-row" data-user-id="${u.id}" data-username="${u.username}" data-fullname="${u.firstName} ${u.lastName}" data-role="${u.role}" data-active="${u.isActive}" style="cursor:pointer;">
            <td>${u.firstName} ${u.lastName}</td>
            <td>${u.username}</td>
            <td><span class="badge badge-${u.role.toLowerCase()}">${u.role}</span></td>
            <td>
                <label class="checkbox-label">
                    <input type="checkbox" class="checkbox" ${u.isActive ? 'checked' : ''} onchange="toggleUserStatus(${u.id}, this.checked)" />
                    Active
                </label>
            </td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('users-list').innerHTML = html;
    
    document.querySelectorAll('.user-row').forEach(row => {
        row.addEventListener('click', (event) => {
            if (event.target.closest('.checkbox-label')) return;
            openUserDetail(row.dataset);
        });
    });
}

async function toggleUserStatus(id, isActive) {
    try {
        const res = await apiFetch(`/employees/${id}/toggle`, { method: 'PUT' });
        if (res.success) {
            showSuccess('User status updated');
            loadUsers();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function createUser(e) {
    e.preventDefault();
    const payload = {
        firstName: document.getElementById('user-firstname').value,
        lastName: document.getElementById('user-lastname').value,
        password: document.getElementById('user-password').value,
        role: document.getElementById('user-role').value
    };
    try {
        const res = await apiFetch('/auth/register', { method: 'POST', body: JSON.stringify(payload) });
        if (res.success) {
            showSuccess('User created');
            closeUserModal();
            loadUsers();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function resetPassword(userId, newPassword) {
    try {
        const res = await apiFetch('/auth/reset-password', {
            method: 'POST',
            body: JSON.stringify({ userId, newPassword })
        });
        if (res.success) {
            showSuccess('Password reset');
            closeDetailUserModal();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function openUserDetail(data) {
    document.getElementById('modal-username').value = data.username;
    document.getElementById('modal-fullname').value = data.fullname;
    document.getElementById('modal-role').value = data.role;
    document.getElementById('modal-active').value = data.active === 'true' ? 'Yes' : 'No';
    document.getElementById('reset-user-id').value = data.userId;
    document.getElementById('user-modal').style.display = 'flex';
}

function closeUserModal() {
    document.getElementById('new-user-modal').style.display = 'none';
}

function closeDetailUserModal() {
    document.getElementById('user-modal').style.display = 'none';
}

document.getElementById('new-user-btn').addEventListener('click', () => {
    document.getElementById('new-user-modal').style.display = 'flex';
});
document.getElementById('create-user-form').addEventListener('submit', createUser);
document.getElementById('reset-password-form').addEventListener('submit', (e) => {
    e.preventDefault();
    const userId = parseInt(document.getElementById('reset-user-id').value);
    const newPassword = document.getElementById('reset-password').value;
    resetPassword(userId, newPassword);
});
document.querySelectorAll('.modal-backdrop').forEach(modal => {
    modal.addEventListener('click', (e) => {
        if (e.target === modal) modal.style.display = 'none';
    });
});

loadRolesForSelect();
loadUsers();