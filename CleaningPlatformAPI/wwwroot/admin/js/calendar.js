// calendar.js - FIXED VERSION
let currentView = 'week';
let anchorDate = new Date();
let isEmployeeView = false;
let resourceGrid = null;

function getMonday(date) {
    const d = new Date(date);
    const day = d.getDay();
    const diff = (day === 0 ? 6 : day - 1);
    d.setDate(d.getDate() - diff);
    return d;
}

function formatDateForApi(date) {
    return date.toISOString().split('T')[0];
}

async function loadCalendar() {
    const token = getToken();
    if (!token) return;
    const payload = JSON.parse(atob(token.split('.')[1]));
    const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    isEmployeeView = role === 'Employee';

    const perms = payload['permission'] || [];
    const permsArr = Array.isArray(perms) ? perms : [perms];
    const hasCreateBooking = permsArr.includes('*') || permsArr.includes('bookings.create');
    const newBookingBtn = document.getElementById('new-booking-btn');
    if (newBookingBtn) newBookingBtn.style.display = hasCreateBooking ? '' : 'none';

    if (isEmployeeView) {
        await loadEmployeeWeek();
    } else {
        await loadResourceGrid();
    }
}

async function loadEmployeeWeek() {
    let weekStart = getMonday(anchorDate);
    try {
        const res = await apiFetch(`/kanban/employee-week?weekStart=${formatDateForApi(weekStart)}`);
        if (res.success && res.data) {
            renderEmployeeWeek(res.data);
        } else showError(res.message);
    } catch (e) { showError(e.message); }
}

function renderEmployeeWeek(board) {
    const container = document.getElementById('employee-week-view');
    document.getElementById('employee-view').style.display = 'block';
    document.getElementById('admin-view').style.display = 'none';
    let html = '<div class="page-header"></div>';
    board.days.forEach(day => {
        if (!day.bookings.length) return;
        html += `<h3 style="margin:1rem 0 0.5rem;">${day.dayLabel} ${day.isToday ? '— Today' : ''}</h3>`;
        day.bookings.sort((a, b) => a.hour - b.hour).forEach(card => {
            const cssStatus = card.status.toLowerCase();
            const recIcon = card.recurringScheduleId ? '<span class="badge badge-info" title="Recurring" style="margin-right:0.3rem;">↻</span>' : '';
            html += `<div class="emp-task-card emp-task-card--${cssStatus}">
                <div class="emp-task-card__header">
                    <span class="emp-task-card__title">${recIcon}${card.hour}:00 — ${card.clientName}</span>
                    ${statusBadge(card.status)}
                </div>
                <div class="emp-task-card__meta">
                    ${card.siteName ? `<div>📍 ${card.siteName} — ${card.siteAddress || ''}</div>` : ''}
                    ${card.clientPhone ? `<div>📞 ${card.clientPhone}</div>` : ''}
                    <div>${card.serviceType} · ${card.servicesCount} service(s)</div>
                    ${card.hasSop ? `<div>SOP: ${card.sopCompletionPct}% complete<div class="cal-sop-bar"><div class="cal-sop-bar__fill" style="width:${card.sopCompletionPct}%"></div></div></div>` : ''}
                </div>
                <div class="emp-task-card__actions">
                    ${card.status === 'Pending' ? `<button onclick="updateBookingStatus(${card.bookingId}, 'InProgress')" class="btn btn-sm">Start job</button>` : ''}
                    ${card.status === 'InProgress' ? `<button onclick="updateBookingStatus(${card.bookingId}, 'Completed')" class="btn btn-sm">Mark complete</button>` : ''}
                    <a href="booking-detail.html?id=${card.bookingId}" class="btn btn-sm">Details</a>
                </div>
            </div>`;
        });
    });
    container.innerHTML = html;
}

async function updateBookingStatus(id, status) {
    try {
        const res = await apiFetch(`/bookings/${id}/status`, {
            method: 'PUT',
            body: JSON.stringify({ status })
        });
        if (res.success) {
            showSuccess(__('msg_status_updated'));
loadCalendar();
window.addEventListener('i18nReady', function () { loadCalendar(); });
        } else showError(res.message);
    } catch (e) { showError(e.message); }
}

async function loadResourceGrid() {
    let apiDate = anchorDate;
    if (currentView === 'week') apiDate = getMonday(anchorDate);
    if (currentView === 'month') apiDate = new Date(anchorDate.getFullYear(), anchorDate.getMonth(), 1);
    try {
        const [gridRes, warnRes] = await Promise.all([
            apiFetch(`/kanban/resourcegrid?anchorDate=${formatDateForApi(apiDate)}&view=${currentView}`),
            currentView === 'day' ? apiFetch(`/kanban/equipment-warnings?date=${formatDateForApi(anchorDate)}`) : Promise.resolve(null)
        ]);
        if (gridRes.success && gridRes.data) {
            resourceGrid = gridRes.data;
            resourceGrid.anchorDate = new Date(resourceGrid.anchorDate);
            renderResourceGrid();
            renderEquipmentWarnings(warnRes);
        } else showError(gridRes.message);
    } catch (e) { showError(e.message); }
}

function renderEquipmentWarnings(warnRes) {
    const container = document.getElementById('equipment-warnings');
    if (!container) return;
    if (!warnRes || !warnRes.success || !warnRes.data || !warnRes.data.length) {
        container.innerHTML = '';
        return;
    }
    let html = '<div class="alert alert-warning" style="margin-bottom:1rem;"><strong>' + __('label_equipment_warnings') + ':</strong><ul style="margin:0.5rem 0 0 1rem;">';
    warnRes.data.forEach(w => {
        html += `<li>${__('label_insufficient_equipment').replace('{0}', w.inventoryName).replace('{1}', w.hour).replace('{2}', w.required).replace('{3}', w.available)}</li>`;
    });
    html += '</ul></div>';
    container.innerHTML = html;
}

function renderResourceGrid() {
    document.getElementById('employee-view').style.display = 'none';
    document.getElementById('admin-view').style.display = 'block';
    if (!resourceGrid) return;

    const anchorDateObj = resourceGrid.anchorDate;
    let rangeLabel = '';
    if (currentView === 'week') {
        const start = new Date(anchorDateObj);
        const end = new Date(start);
        end.setDate(start.getDate() + 6);
        rangeLabel = `${start.toLocaleDateString()} – ${end.toLocaleDateString()}`;
    } else if (currentView === 'day') {
        rangeLabel = anchorDateObj.toLocaleDateString(undefined, { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
    } else {
        rangeLabel = anchorDateObj.toLocaleDateString(undefined, { year: 'numeric', month: 'long' });
    }
    document.getElementById('cal-range-label').innerText = rangeLabel;

    const employees = resourceGrid.employees || [];
    const unassigned = resourceGrid.unassigned || [];

    if (currentView === 'day') {
        renderDayView(employees, unassigned);
    } else if (currentView === 'week') {
        renderWeekView(employees, unassigned);
    } else {
        renderMonthView(employees, unassigned);
    }
}

function renderDayView(employees, unassigned) {
    const hours = Array.from({ length: 12 }, (_, i) => i + 7);
    let html = `<div class="resource-grid-wrapper"><table class="resource-grid"><thead><tr>
        <th class="resource-col-header resource-col-time-header">Time</th>
        <th class="resource-col-header resource-col-unassigned-header">Unassigned ${unassigned.length ? `<span class="resource-unassigned-badge">${unassigned.length}</span>` : ''}</th>`;
    employees.forEach(emp => {
        html += `<th class="resource-col-header ${emp.isFree ? 'resource-col-header--free' : ''}" data-emp-name="${emp.fullName.toLowerCase()}">
            <div class="resource-emp-name">${emp.fullName}</div><div class="resource-emp-role">${emp.role}</div></th>`;
    });
    html += `</tr></thead><tbody>`;
    hours.forEach(hour => {
        html += `<tr><td class="resource-cell resource-cell--time">${hour}:00</td>`;
        html += `<td class="resource-cell resource-cell--unassigned">`;
        unassigned.filter(c => c.hour === hour).forEach(card => {
            const recIcon = card.recurringScheduleId ? '↻ ' : '';
            html += `<a href="booking-detail.html?id=${card.bookingId}" class="resource-card resource-card--${card.status.toLowerCase()}">
                <div class="resource-card__time">${card.hour}:00</div><div class="resource-card__client">${recIcon}${card.clientName}</div>
                <div class="resource-card__meta">${card.serviceType}${card.siteName ? ' · ' + card.siteName : ''}</div>
            </a>`;
        });
        html += `</td>`;
        employees.forEach(emp => {
            const cellBookings = emp.bookings.filter(c => c.hour === hour);
            html += `<td class="resource-cell ${!cellBookings.length ? 'resource-cell--free' : ''}" data-emp-name="${emp.fullName.toLowerCase()}">`;
            cellBookings.forEach(card => {
                const recIcon = card.recurringScheduleId ? '↻ ' : '';
                html += `<a href="booking-detail.html?id=${card.bookingId}" class="resource-card resource-card--${card.status.toLowerCase()}">
                    <div class="resource-card__time">${card.hour}:00</div><div class="resource-card__client">${recIcon}${card.clientName}</div>
                    <div class="resource-card__meta">${card.serviceType}${card.siteName ? ' · ' + card.siteName : ''}</div>
                </a>`;
            });
            html += `</td>`;
        });
        html += `</tr>`;
    });
    html += `</tbody></table></div>`;
    document.getElementById('resource-grid').innerHTML = html;
    setupFilter();
}

function renderWeekView(employees, unassigned) {
    const start = new Date(resourceGrid.anchorDate);
    const days = [];
    for (let i = 0; i < 7; i++) {
        const d = new Date(start);
        d.setDate(start.getDate() + i);
        days.push(d);
    }
    let html = `<div class="resource-grid-wrapper"><table class="resource-grid"><thead><tr>
        <th class="resource-col-header resource-col-time-header">Date</th>
        <th class="resource-col-header resource-col-unassigned-header">Unassigned ${unassigned.length ? `<span class="resource-unassigned-badge">${unassigned.length}</span>` : ''}</th>`;
    employees.forEach(emp => {
        html += `<th class="resource-col-header ${emp.isFree ? 'resource-col-header--free' : ''}" data-emp-name="${emp.fullName.toLowerCase()}">
            <div class="resource-emp-name">${emp.fullName}</div><div class="resource-emp-role">${emp.role}</div></th>`;
    });
    html += `</tr></thead><tbody>`;
    days.forEach(day => {
        const isToday = formatDateForApi(day) === formatDateForApi(new Date());
        html += `<tr class="${isToday ? 'resource-row--today' : ''}">
            <td class="resource-cell resource-cell--time"><div>${day.toLocaleDateString(undefined, { weekday: 'short' })}</div><div>${day.toLocaleDateString()}</div></td>
            <td class="resource-cell resource-cell--unassigned">`;
        unassigned.filter(c => {
            const cardDate = new Date(c.date);
            return formatDateForApi(cardDate) === formatDateForApi(day);
        }).forEach(card => {
            const recIcon = card.recurringScheduleId ? '↻ ' : '';
            html += `<a href="booking-detail.html?id=${card.bookingId}" class="resource-chip resource-chip--${card.status.toLowerCase()}">${recIcon}${card.hour}:00 ${card.clientName}</a>`;
        });
        html += `</td>`;
        employees.forEach(emp => {
            const dayBookings = emp.bookings.filter(c => {
                const cardDate = new Date(c.date);
                return formatDateForApi(cardDate) === formatDateForApi(day);
            });
            html += `<td class="resource-cell ${!dayBookings.length ? 'resource-cell--free' : ''}" data-emp-name="${emp.fullName.toLowerCase()}">`;
            dayBookings.forEach(card => {
                const recIcon = card.recurringScheduleId ? '↻ ' : '';
                html += `<a href="booking-detail.html?id=${card.bookingId}" class="resource-chip resource-chip--${card.status.toLowerCase()}">${recIcon}${card.hour}:00 ${card.clientName}</a>`;
            });
            html += `</td>`;
        });
        html += `</tr>`;
    });
    html += `</tbody></table></div>`;
    document.getElementById('resource-grid').innerHTML = html;
    setupFilter();
}

function renderMonthView(employees, unassigned) {
    const anchorDateObj = resourceGrid.anchorDate;
    const year = anchorDateObj.getFullYear();
    const month = anchorDateObj.getMonth();
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const days = [];
    for (let d = new Date(firstDay); d <= lastDay; d.setDate(d.getDate() + 1)) {
        days.push(new Date(d));
    }
    let html = `<div class="resource-grid-wrapper"><table class="resource-grid"><thead><tr>
        <th class="resource-col-header resource-col-time-header">Date</th>
        <th class="resource-col-header resource-col-unassigned-header">Unassigned ${unassigned.length ? `<span class="resource-unassigned-badge">${unassigned.length}</span>` : ''}</th>`;
    employees.forEach(emp => {
        html += `<th class="resource-col-header ${emp.isFree ? 'resource-col-header--free' : ''}" data-emp-name="${emp.fullName.toLowerCase()}">
            <div class="resource-emp-name">${emp.fullName}</div><div class="resource-emp-role">${emp.role}</div></th>`;
    });
    html += `</tr></thead><tbody>`;
    days.forEach(day => {
        const isToday = formatDateForApi(day) === formatDateForApi(new Date());
        const unassignedCount = unassigned.filter(c => {
            const cardDate = new Date(c.date);
            return formatDateForApi(cardDate) === formatDateForApi(day);
        }).length;
        html += `<tr class="${isToday ? 'resource-row--today' : ''}">
            <td class="resource-cell resource-cell--time"><div>${day.toLocaleDateString(undefined, { weekday: 'short' })}</div><div>${day.toLocaleDateString()}</div></td>
            <td class="resource-cell resource-cell--unassigned">${unassignedCount ? `<a href="javascript:changeViewToDay('${formatDateForApi(day)}')" class="resource-count">${unassignedCount}</a>` : ''}</td>`;
        employees.forEach(emp => {
            const count = emp.bookings.filter(c => {
                const cardDate = new Date(c.date);
                return formatDateForApi(cardDate) === formatDateForApi(day);
            }).length;
            html += `<td class="resource-cell ${count === 0 ? 'resource-cell--free' : ''}" data-emp-name="${emp.fullName.toLowerCase()}">${count ? `<a href="javascript:changeViewToDay('${formatDateForApi(day)}')" class="resource-count">${count}</a>` : ''}</td>`;
        });
        html += `</tr>`;
    });
    html += `</tbody></table></div>`;
    document.getElementById('resource-grid').innerHTML = html;
    setupFilter();
}

function changeViewToDay(dateStr) {
    anchorDate = new Date(dateStr);
    currentView = 'day';
    updateViewButtons();
    loadResourceGrid();
}

function setupFilter() {
    const filterInput = document.getElementById('emp-filter');
    if (filterInput) {
        filterInput.oninput = function () {
            const q = this.value.toLowerCase().trim();
            document.querySelectorAll('[data-emp-name]').forEach(el => {
                const name = el.getAttribute('data-emp-name') || '';
                el.style.display = !q || name.includes(q) ? '' : 'none';
            });
        };
    }
}

function updateViewButtons() {
    document.getElementById('view-week').classList.toggle('active', currentView === 'week');
    document.getElementById('view-day').classList.toggle('active', currentView === 'day');
    document.getElementById('view-month').classList.toggle('active', currentView === 'month');
}

document.getElementById('view-week').addEventListener('click', () => { currentView = 'week'; updateViewButtons(); loadResourceGrid(); });
document.getElementById('view-day').addEventListener('click', () => { currentView = 'day'; updateViewButtons(); loadResourceGrid(); });
document.getElementById('view-month').addEventListener('click', () => { currentView = 'month'; updateViewButtons(); loadResourceGrid(); });
document.getElementById('nav-prev').addEventListener('click', () => {
    if (currentView === 'week') anchorDate.setDate(anchorDate.getDate() - 7);
    else if (currentView === 'day') anchorDate.setDate(anchorDate.getDate() - 1);
    else anchorDate.setMonth(anchorDate.getMonth() - 1);
    loadResourceGrid();
});
document.getElementById('nav-next').addEventListener('click', () => {
    if (currentView === 'week') anchorDate.setDate(anchorDate.getDate() + 7);
    else if (currentView === 'day') anchorDate.setDate(anchorDate.getDate() + 1);
    else anchorDate.setMonth(anchorDate.getMonth() + 1);
    loadResourceGrid();
});

loadCalendar();