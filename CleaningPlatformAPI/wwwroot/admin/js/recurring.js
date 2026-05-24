// recurring.js
let schedules = [];
let editingId = null;

const DAY_NAMES = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

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
        container.innerHTML = '<div class="alert alert-info">No recurring schedules found.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr>' +
        '<th>ID</th><th>Client</th><th>Site</th><th>Frequency</th><th>Day</th>' +
        '<th>Weeks Ahead</th><th>Upcoming</th><th>Status</th><th>Ends On</th><th>Actions</th>' +
        '</tr></thead><tbody>';
    schedules.forEach(s => {
        const dayText = s.frequency === 'Monthly'
            ? `Day ${s.dayOfMonth}`
            : s.dayOfWeek !== null ? DAY_NAMES[s.dayOfWeek] : '-';
        const statusText = s.isActive ? 'Active' : 'Ended';
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
                <button onclick="openEditModal(${s.id})" class="btn btn-sm" ${!s.isActive ? 'disabled' : ''}>Edit</button>
                <button onclick="openEndModal(${s.id})" class="btn btn-sm" style="background:#c62828;color:#fff;" ${!s.isActive ? 'disabled' : ''}>End Series</button>
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
            showSuccess('Schedule updated');
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
    if (!endsOn) { showError('Please select a date.'); return; }
    if (!confirm(`End series #${editingId} from ${endsOn}? All future pending bookings will be cancelled.`)) return;
    try {
        const res = await apiFetch(`/recurring/${editingId}/end`, {
            method: 'POST',
            body: JSON.stringify({ endsOn })
        });
        if (res.success) {
            showSuccess('Series ended');
            closeEndModal();
            loadSchedules();
        } else showError(res.message);
    } catch (e) { showError(e.message); }
});

document.getElementById('end-cancel-btn').addEventListener('click', closeEndModal);

document.getElementById('run-auto-btn').addEventListener('click', async function () {
    this.disabled = true;
    this.textContent = 'Running...';
    try {
        const res = await apiFetch('/recurring/run-auto', { method: 'POST' });
        if (res.success && res.data) {
            let totalGenerated = 0;
            res.data.forEach(r => { totalGenerated += (r.generated || []).length; });
            showSuccess(`Auto-generate complete: ${totalGenerated} booking(s) created across ${res.data.length} schedule(s).`);
            loadSchedules();
        } else showError(res.message);
    } catch (e) { showError(e.message); }
    finally {
        this.disabled = false;
        this.textContent = 'Run Auto-Generate';
    }
});

loadSchedules();
