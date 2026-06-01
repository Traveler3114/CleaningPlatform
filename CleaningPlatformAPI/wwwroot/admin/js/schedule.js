// schedule.js
let schedule = [];
let overrides = [];
let editingDay = null;

async function loadSchedule() {
    try {
        const res = await apiFetch('/schedule');
        if (res.success && res.data) {
            schedule = res.data;
            renderSchedule();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function loadOverrides() {
    try {
        const res = await apiFetch('/overrides');
        if (res.success && res.data) {
            overrides = res.data;
            renderOverrides();
        }
    } catch(e) { console.error(e); }
}

function renderSchedule() {
    const dayNames = ['day_sunday', 'day_monday', 'day_tuesday', 'day_wednesday', 'day_thursday', 'day_friday', 'day_saturday'];
    if (!schedule.length) {
        document.getElementById('schedule-list').innerHTML = '<div class="alert alert-info">' + __('empty_no_schedule_entries') + '</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>' + __('th_day') + '</th><th>' + __('th_start') + '</th><th>' + __('th_end') + '</th><th>' + __('th_capacity') + '</th></tr></thead><tbody>';
    schedule.forEach(s => {
        html += `<tr class="schedule-row" data-day="${s.dayOfWeek}" data-start="${s.startHour}" data-end="${s.endHour}" data-capacity="${s.capacity}" style="cursor:pointer;">
            <td>${__(dayNames[s.dayOfWeek])}</td><td>${s.startHour}</td><td>${s.endHour}</td><td>${s.capacity}</td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('schedule-list').innerHTML = html;
    
    document.querySelectorAll('.schedule-row').forEach(row => {
        row.addEventListener('click', () => openEditSchedule(row.dataset));
    });
}

function renderOverrides() {
    if (!overrides.length) {
        document.getElementById('overrides-list').innerHTML = '<div class="alert alert-info">' + __('empty_no_overrides_found') + '</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>' + __('th_date') + '</th><th>' + __('th_start') + '</th><th>' + __('th_end') + '</th><th>' + __('th_capacity') + '</th><th>' + __('th_closed') + '</th><th>' + __('th_actions') + '</th></tr></thead><tbody>';
    overrides.forEach(o => {
        html += `<tr>
            <td>${o.date.split('T')[0]}</td><td>${o.startHour ?? '—'}</td><td>${o.endHour ?? '—'}</td><td>${o.capacity ?? '—'}</td>
            <td>${o.isFullyClosed ? __('ui_yes') : __('ui_no')}</td>
            <td><button onclick="editOverride(${o.id})" class="btn btn-sm">${__('btn_edit')}</button>
                <button onclick="deleteOverride(${o.id})" class="btn btn-sm">${__('btn_delete')}</button></td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('overrides-list').innerHTML = html;
}

function openEditSchedule(data) {
    editingDay = parseInt(data.day);
    document.getElementById('schedule-modal-title').textContent = __('btn_edit') + ' ' + __(['day_sunday','day_monday','day_tuesday','day_wednesday','day_thursday','day_friday','day_saturday'][editingDay]);
    document.getElementById('schedule-day').value = data.day;
    document.getElementById('schedule-start').value = data.start;
    document.getElementById('schedule-end').value = data.end;
    document.getElementById('schedule-capacity').value = data.capacity;
    document.getElementById('schedule-add-form').style.display = '';
    document.getElementById('schedule-delete-form').style.display = 'block';
    document.getElementById('schedule-modal').style.display = 'flex';
}

function openAddSchedule() {
    editingDay = null;
    document.getElementById('schedule-modal-title').textContent = __('btn_add') + ' ' + __('th_day');
    document.getElementById('schedule-day').value = '';
    document.getElementById('schedule-start').value = '';
    document.getElementById('schedule-end').value = '';
    document.getElementById('schedule-capacity').value = '';
    document.getElementById('schedule-add-form').style.display = '';
    document.getElementById('schedule-delete-form').style.display = 'none';
    document.getElementById('schedule-modal').style.display = 'flex';
}

async function saveSchedule() {
    const dayOfWeek = parseInt(document.getElementById('schedule-day').value);
    const startHour = parseInt(document.getElementById('schedule-start').value);
    const endHour = parseInt(document.getElementById('schedule-end').value);
    const capacity = parseInt(document.getElementById('schedule-capacity').value);
    const payload = { dayOfWeek, startHour, endHour, capacity };
    try {
        let res;
        if (editingDay !== null) {
            res = await apiFetch(`/schedule/${editingDay}`, { method: 'PUT', body: JSON.stringify(payload) });
        } else {
            res = await apiFetch('/schedule', { method: 'POST', body: JSON.stringify(payload) });
        }
        if (res.success) {
            showSuccess(editingDay ? __('msg_schedule_updated') : __('msg_schedule_added'));
            closeScheduleModal();
            loadSchedule();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function deleteSchedule() {
    if (!confirm(__('msg_confirm_delete_schedule'))) return;
    try {
        const res = await apiFetch(`/schedule/${editingDay}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess(__('msg_schedule_deleted'));
            closeScheduleModal();
            loadSchedule();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function saveOverride() {
    const id = document.getElementById('override-id').value;
    const payload = {
        date: document.getElementById('override-date').value,
        startHour: parseInt(document.getElementById('override-start').value) || null,
        endHour: parseInt(document.getElementById('override-end').value) || null,
        capacity: parseInt(document.getElementById('override-capacity').value) || null,
        isFullyClosed: document.getElementById('override-closed').checked
    };
    try {
        const res = await apiFetch('/overrides', { method: 'POST', body: JSON.stringify(payload) });
        if (res.success) {
            showSuccess(__('msg_override_saved'));
            closeOverrideModal();
            loadOverrides();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function deleteOverride(id) {
    if (!confirm(__('msg_confirm_delete_override'))) return;
    try {
        const res = await apiFetch(`/overrides/${id}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess(__('msg_override_deleted'));
            loadOverrides();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function editOverride(id) {
    const override = overrides.find(o => o.id === id);
    if (!override) return;
    document.getElementById('override-id').value = override.id;
    document.getElementById('override-date').value = override.date.split('T')[0];
    document.getElementById('override-start').value = override.startHour ?? '';
    document.getElementById('override-end').value = override.endHour ?? '';
    document.getElementById('override-capacity').value = override.capacity ?? '';
    document.getElementById('override-closed').checked = override.isFullyClosed;
    document.getElementById('override-modal').style.display = 'flex';
}

function openOverrideModal() {
    document.getElementById('override-id').value = '';
    document.getElementById('override-date').value = '';
    document.getElementById('override-start').value = '';
    document.getElementById('override-end').value = '';
    document.getElementById('override-capacity').value = '';
    document.getElementById('override-closed').checked = false;
    document.getElementById('override-modal').style.display = 'flex';
}

function closeScheduleModal() { document.getElementById('schedule-modal').style.display = 'none'; }
function closeOverrideModal() { document.getElementById('override-modal').style.display = 'none'; }

document.getElementById('add-day-btn').addEventListener('click', openAddSchedule);
document.getElementById('schedule-add-form').addEventListener('submit', (e) => { e.preventDefault(); saveSchedule(); });
document.getElementById('schedule-delete-form').addEventListener('submit', (e) => { e.preventDefault(); deleteSchedule(); });
document.getElementById('open-override-modal').addEventListener('click', openOverrideModal);
document.getElementById('override-form').addEventListener('submit', (e) => { e.preventDefault(); saveOverride(); });

loadSchedule();
loadOverrides();
window.addEventListener('i18nReady', function () { loadSchedule(); loadOverrides(); });