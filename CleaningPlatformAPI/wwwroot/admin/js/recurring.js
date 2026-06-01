// recurring.js
let schedules = [];
let editingId = null;

const DAY_KEYS = ['day_sunday', 'day_monday', 'day_tuesday', 'day_wednesday', 'day_thursday', 'day_friday', 'day_saturday'];

async function loadSchedules() {
    try {
        const res = await apiFetch('/recurring');
        if (res.success && res.data) {
            schedules = res.data;
            renderSchedules();
        } else showError(res.message);
    } catch (e) { showError(e.message); }
}

function renderSchedules() {
    const container = document.getElementById('recurring-list');
    if (!schedules.length) {
        container.innerHTML = '<div class="alert alert-info">' + __('empty_no_recurring') + '</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr>' +
        '<th>' + __('th_id') + '</th><th>' + __('th_client') + '</th><th>' + __('th_site') + '</th><th>' + __('th_frequency') + '</th><th>' + __('th_day') + '</th>' +
        '<th>' + __('th_weeks_ahead') + '</th><th>' + __('th_upcoming') + '</th><th>' + __('th_status') + '</th><th>' + __('th_ends_on') + '</th><th>' + __('th_actions') + '</th>' +
        '</tr></thead><tbody>';
    schedules.forEach(s => {
        const dayText = s.frequency === 'Monthly'
            ? __('label_day_of_month') + ' ' + s.dayOfMonth
            : s.dayOfWeek !== null ? __(DAY_KEYS[s.dayOfWeek]) : '-';
        const statusText = s.isActive ? __('status_active') : __('status_ended');
        const statusClass = s.isActive ? 'badge-active' : 'badge-inactive';
        const endsOn = s.endsOn || '-';
        html += `<tr>
            <td>${s.id}</td>
            <td><a href="client-detail.html?id=${s.clientId}" class="link">${s.clientName}</a></td>
            <td>${s.siteName || '-'}</td>
            <td>${s.frequency}</td>
            <td>${dayText}</td>
            <td>${s.autoGenerateWeeksAhead}</td>
            <td><span class="badge badge-pending">${s.upcomingCount}</span></td>
            <td><span class="badge ${statusClass}">${statusText}</span></td>
            <td>${endsOn}</td>
            <td style="white-space:nowrap;">
                <button onclick="openEditModal(${s.id})" class="btn btn-sm" ${!s.isActive ? 'disabled' : ''}>${__('btn_edit')}</button>
                <button onclick="openEndModal(${s.id})" class="btn btn-sm" style="background:#c62828;color:#fff;" ${!s.isActive ? 'disabled' : ''}>${__('btn_end_series')}</button>
            </td>
        </tr>`;
    });
    html += '</tbody></table>';
    container.innerHTML = html;
}

function openEditModal(id) {
    const s = schedules.find(x => x.id === id);
    if (!s) return;
    editingId = id;
    document.getElementById('edit-frequency').value = s.frequency;
    document.getElementById('edit-dayofweek').value = s.dayOfWeek !== null ? s.dayOfWeek : '';
    document.getElementById('edit-dayofmonth').value = s.dayOfMonth || '';
    document.getElementById('edit-weeksahead').value = s.autoGenerateWeeksAhead;
    document.getElementById('edit-endson').value = s.endsOn || '';
    toggleDayFields(s.frequency);
    document.getElementById('edit-modal').style.display = 'flex';
}

function closeEditModal() {
    document.getElementById('edit-modal').style.display = 'none';
    editingId = null;
}

function toggleDayFields(frequency) {
    const dowGroup = document.getElementById('edit-dayofweek-group');
    const domGroup = document.getElementById('edit-dayofmonth-group');
    if (frequency === 'Weekly' || frequency === 'Biweekly') {
        dowGroup.style.display = 'block';
        domGroup.style.display = 'none';
    } else if (frequency === 'Monthly') {
        dowGroup.style.display = 'none';
        domGroup.style.display = 'block';
    } else {
        dowGroup.style.display = 'none';
        domGroup.style.display = 'none';
    }
}

document.getElementById('edit-frequency').addEventListener('change', function () {
    toggleDayFields(this.value);
});

document.getElementById('edit-form').addEventListener('submit', async function (e) {
    e.preventDefault();
    if (editingId === null) return;
    const frequency = document.getElementById('edit-frequency').value;
    const dayOfWeek = (frequency === 'Weekly' || frequency === 'Biweekly')
        ? parseInt(document.getElementById('edit-dayofweek').value) || null
        : null;
    const dayOfMonth = frequency === 'Monthly'
        ? parseInt(document.getElementById('edit-dayofmonth').value) || null
        : null;
    const autoGenerateWeeksAhead = parseInt(document.getElementById('edit-weeksahead').value) || 4;
    const endsOn = document.getElementById('edit-endson').value || null;
    try {
        const res = await apiFetch(`/recurring/${editingId}`, {
            method: 'PUT',
            body: JSON.stringify({ frequency, dayOfWeek, dayOfMonth, autoGenerateWeeksAhead, endsOn })
        });
        if (res.success) {
            showSuccess(__('msg_schedule_updated'));
            closeEditModal();
            loadSchedules();
        } else showError(res.message);
    } catch (e) { showError(e.message); }
});

document.getElementById('edit-cancel-btn').addEventListener('click', closeEditModal);

function openEndModal(id) {
    editingId = id;
    const today = new Date().toISOString().split('T')[0];
    document.getElementById('end-date').value = today;
    document.getElementById('end-modal').style.display = 'flex';
}

function closeEndModal() {
    document.getElementById('end-modal').style.display = 'none';
    editingId = null;
}

document.getElementById('end-form').addEventListener('submit', async function (e) {
    e.preventDefault();
    if (editingId === null) return;
    const endsOn = document.getElementById('end-date').value;
    if (!endsOn) { showError(__('msg_select_date')); return; }
    if (!confirm(__('msg_confirm_end_series').replace('{id}', editingId).replace('{date}', endsOn))) return;
    try {
        const res = await apiFetch(`/recurring/${editingId}/end`, {
            method: 'POST',
            body: JSON.stringify({ endsOn })
        });
        if (res.success) {
            showSuccess(__('msg_series_ended'));
            closeEndModal();
            loadSchedules();
        } else showError(res.message);
    } catch (e) { showError(e.message); }
});

document.getElementById('end-cancel-btn').addEventListener('click', closeEndModal);

document.getElementById('run-auto-btn').addEventListener('click', async function () {
    this.disabled = true;
    this.textContent = __('msg_running');
    try {
        const res = await apiFetch('/recurring/run-auto', { method: 'POST' });
        if (res.success && res.data) {
            let totalGenerated = 0;
            res.data.forEach(r => { totalGenerated += (r.generated || []).length; });
            showSuccess(__('msg_auto_generate_complete').replace('{generated}', totalGenerated).replace('{schedules}', res.data.length));
            loadSchedules();
        } else showError(res.message);
    } catch (e) { showError(e.message); }
    finally {
        this.disabled = false;
        this.textContent = __('btn_run_auto_generate');
    }
});

loadSchedules();
window.addEventListener('i18nReady', function () { loadSchedules(); });
