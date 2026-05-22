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
    const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    if (!schedule.length) {
        document.getElementById('schedule-list').innerHTML = '<div class="alert alert-info">No schedule entries found.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>Day</th><th>Start</th><th>End</th><th>Capacity</th></tr></thead><tbody>';
    schedule.forEach(s => {
        html += `<tr class="schedule-row" data-day="${s.dayOfWeek}" data-start="${s.startHour}" data-end="${s.endHour}" data-capacity="${s.capacity}" style="cursor:pointer;">
            <td>${dayNames[s.dayOfWeek]}</td><td>${s.startHour}</td><td>${s.endHour}</td><td>${s.capacity}</td>
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
        document.getElementById('overrides-list').innerHTML = '<div class="alert alert-info">No date overrides defined.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>Date</th><th>Start</th><th>End</th><th>Capacity</th><th>Closed</th><th>Actions</th></tr></thead><tbody>';
    overrides.forEach(o => {
        html += `<tr>
            <td>${o.date.split('T')[0]}</td><td>${o.startHour ?? '—'}</td><td>${o.endHour ?? '—'}</td><td>${o.capacity ?? '—'}</td>
            <td>${o.isFullyClosed ? 'Yes' : 'No'}</td>
            <td><button onclick="editOverride(${o.id})" class="btn btn-sm">Edit</button>
                <button onclick="deleteOverride(${o.id})" class="btn btn-sm">Delete</button></td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('overrides-list').innerHTML = html;
}

function openEditSchedule(data) {
    editingDay = parseInt(data.day);
    document.getElementById('schedule-modal-title').textContent = `Edit ${['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'][editingDay]}`;
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
    document.getElementById('schedule-modal-title').textContent = 'Add Day';
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
            showSuccess(editingDay ? 'Schedule updated' : 'Schedule added');
            closeScheduleModal();
            loadSchedule();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function deleteSchedule() {
    if (!confirm('Delete this schedule entry?')) return;
    try {
        const res = await apiFetch(`/schedule/${editingDay}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess('Schedule deleted');
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
            showSuccess('Override saved');
            closeOverrideModal();
            loadOverrides();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function deleteOverride(id) {
    if (!confirm('Delete this override?')) return;
    try {
        const res = await apiFetch(`/overrides/${id}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess('Override deleted');
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