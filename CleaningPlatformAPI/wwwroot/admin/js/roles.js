// roles.js
let roles = [];
let permissions = [];
let editingRoleId = null;
let isEditMode = false;

async function loadRoles() {
    try {
        const res = await apiFetch('/roles');
        if (res.success && res.data) {
            roles = res.data;
            renderRoles();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function loadPermissions() {
    try {
        const res = await apiFetch('/roles/permissions');
        if (res.success && res.data) {
            permissions = res.data;
            renderPermissionsGrid();
        }
    } catch(e) { console.error(e); }
}

function renderPermissionsGrid() {
    const container = document.getElementById('permissions-grid');
    if (!container) return;
    const grouped = {};
    permissions.forEach(p => {
        if (!grouped[p.category]) grouped[p.category] = [];
        grouped[p.category].push(p);
    });
    let html = '';
    for (const [category, perms] of Object.entries(grouped)) {
        html += `<div style="grid-column:1/-1;"><strong>${category}</strong></div>`;
        perms.forEach(p => {
            html += `<label class="checkbox-label"><input type="checkbox" name="permission" value="${p.key}" class="checkbox" /> ${p.displayName}</label>`;
        });
    }
    container.innerHTML = html;
}

function renderRoles() {
    if (!roles.length) {
        document.getElementById('roles-list').innerHTML = `<div class="alert alert-info">${__('empty_no_roles')}</div>`;
        return;
    }
    const permissionNames = {};
    permissions.forEach(p => { permissionNames[p.key] = p.displayName; });
    let html = `<table class="admin-table"><thead><tr><th>${__('th_id')}</th><th>${__('th_name')}</th><th>${__('th_permissions')}</th></tr></thead><tbody>`;
    roles.forEach(r => {
        const permNames = r.permissions.map(p => permissionNames[p] || p).join(', ');
        html += `<tr class="role-row ${r.isProtected ? 'role-protected' : ''}" data-role-id="${r.id}" data-role-name="${r.name}" data-role-protected="${r.isProtected}" data-role-permissions="${r.permissions.join('|')}" style="cursor:pointer;">
            <td>${r.id}</td>
            <td>${r.name}${r.isProtected ? ' <span class="badge badge-protected">Protected</span>' : ''}</td>
            <td class="permissions-cell">${permNames}</td>
        </tr>`;
    });
    html += '</tbody></td>';
    document.getElementById('roles-list').innerHTML = html;
    
    document.querySelectorAll('.role-row').forEach(row => {
        row.addEventListener('click', () => {
            if (row.dataset.roleProtected === 'true') return;
            openEditRole(row.dataset);
        });
    });
}

function openEditRole(data) {
    isEditMode = true;
    editingRoleId = parseInt(data.roleId);
    document.getElementById('role-modal-title').textContent = `${__('btn_edit') + ' ' + __('th_role')}: ${data.roleName}`;
    document.getElementById('role-name').value = data.roleName;
    const permissionsList = data.rolePermissions ? data.rolePermissions.split('|') : [];
    document.querySelectorAll('input[name="permission"]').forEach(cb => {
        cb.checked = permissionsList.includes(cb.value);
    });
    document.getElementById('role-delete-form').style.display = 'block';
    document.getElementById('role-modal').style.display = 'flex';
}

function openCreateRole() {
    isEditMode = false;
    editingRoleId = null;
    document.getElementById('role-modal-title').textContent = __('btn_create') + ' ' + __('th_role');
    document.getElementById('role-name').value = '';
    document.querySelectorAll('input[name="permission"]').forEach(cb => cb.checked = false);
    document.getElementById('role-delete-form').style.display = 'none';
    document.getElementById('role-modal').style.display = 'flex';
}

async function saveRole() {
    const name = document.getElementById('role-name').value.trim();
    if (!name) { showError(__('msg_role_name_required')); return; }
    const selectedPermissions = Array.from(document.querySelectorAll('input[name="permission"]:checked')).map(cb => cb.value);
    if (isEditMode) {
        try {
            const res = await apiFetch(`/roles/${editingRoleId}`, {
                method: 'PUT',
                body: JSON.stringify({ name, permissions: selectedPermissions })
            });
            if (res.success) {
                showSuccess(__('msg_role_updated'));
                closeRoleModal();
                loadRoles();
            } else showError(res.message);
        } catch(e) { showError(e.message); }
    } else {
        try {
            const res = await apiFetch('/roles', {
                method: 'POST',
                body: JSON.stringify({ name, permissions: selectedPermissions })
            });
            if (res.success) {
                showSuccess(__('msg_role_created'));
                closeRoleModal();
                loadRoles();
            } else showError(res.message);
        } catch(e) { showError(e.message); }
    }
}

async function deleteRole() {
    if (!confirm(__('msg_confirm_delete_role'))) return;
    try {
        const res = await apiFetch(`/roles/${editingRoleId}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess(__('msg_role_deleted'));
            closeRoleModal();
            loadRoles();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function closeRoleModal() {
    document.getElementById('role-modal').style.display = 'none';
}

document.getElementById('new-role-btn').addEventListener('click', openCreateRole);
document.getElementById('role-create-form').addEventListener('submit', (e) => { e.preventDefault(); saveRole(); });
document.getElementById('role-delete-form').addEventListener('submit', (e) => { e.preventDefault(); deleteRole(); });
document.getElementById('role-modal').addEventListener('click', (e) => { if (e.target === document.getElementById('role-modal')) closeRoleModal(); });

loadPermissions();
loadRoles();
window.addEventListener('i18nReady', function () { loadRoles(); });