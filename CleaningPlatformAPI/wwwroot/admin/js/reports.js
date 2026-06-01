// reports.js
let revenue = [];
let topClients = [];
let utilization = [];
let completionRates = [];
let overdue = null;

async function loadReports() {
    await Promise.all([
        loadRevenue(),
        loadTopClients(),
        loadUtilization(),
        loadCompletionRates(),
        loadOverdue(),
        loadDashboardSummary()
    ]);
}

async function loadDashboardSummary() {
    try {
        const res = await apiFetch('/reports/dashboard');
        if (res.success && res.data) {
            const data = res.data;
            const statsHtml = `
                <div class="stat-card"><span>${__('kpi_revenue_mtd')}</span><strong>${data.monthlyRevenue?.totalRevenue?.toFixed(2) ?? '—'}</strong></div>
                <div class="stat-card"><span>${__('kpi_overdue_invoices')}</span><strong>${data.overdueInvoices?.totalOverdueAmount?.toFixed(2) ?? '0'}</strong><small>${data.overdueInvoices?.overdueInvoiceCount ?? 0} ${__('invoices')}</small></div>
                <div class="stat-card"><span>${__('kpi_completion_rate')}</span><strong>${data.completionRate?.completionRatePct?.toFixed(1) ?? '0'}%</strong></div>
                <div class="stat-card"><span>${__('kpi_top_client')}</span><strong>${data.topClient?.clientName ?? '—'}</strong></div>
            `;
            document.getElementById('dashboard-stats').innerHTML = statsHtml;
        }
    } catch(e) { console.error(e); }
}

async function loadRevenue() {
    try {
        const res = await apiFetch('/reports/revenue');
        if (res.success && res.data) {
            revenue = res.data;
            renderRevenue();
        }
    } catch(e) { console.error(e); }
}

async function loadTopClients() {
    try {
        const res = await apiFetch('/reports/top-clients');
        if (res.success && res.data) {
            topClients = res.data;
            renderTopClients();
        }
    } catch(e) { console.error(e); }
}

async function loadUtilization() {
    try {
        const res = await apiFetch('/reports/utilization');
        if (res.success && res.data) {
            utilization = res.data;
            renderUtilization();
        }
    } catch(e) { console.error(e); }
}

async function loadCompletionRates() {
    try {
        const res = await apiFetch('/reports/completion');
        if (res.success && res.data) {
            completionRates = res.data;
            renderCompletionRates();
        }
    } catch(e) { console.error(e); }
}

async function loadOverdue() {
    try {
        const res = await apiFetch('/reports/overdue');
        if (res.success && res.data) {
            overdue = res.data;
            renderOverdue();
        }
    } catch(e) { console.error(e); }
}

function renderRevenue() {
    if (!revenue.length) {
        document.getElementById('revenue-table').innerHTML = `<div class="alert alert-info">${__('empty_no_data_available')}</div>`;
        return;
    }
    let html = `<table class="admin-table"><thead><tr><th>${__('th_month')}</th><th>${__('th_invoices')}</th><th>${__('th_revenue')}</th><th>${__('th_vat')}</th><th>${__('th_discount')}</th></tr></thead><tbody>`;
    revenue.forEach(r => {
        html += `<tr>
            <td>${r.year}-${String(r.month).padStart(2,'0')}</td>
            <td>${r.invoiceCount}</td>
            <td>${r.totalRevenue?.toFixed(2)}</td>
            <td>${r.totalVat?.toFixed(2)}</td>
            <td>${r.totalDiscount?.toFixed(2)}</td>
        </tr>`;
    });
    html += `</tbody></table>`;
    document.getElementById('revenue-table').innerHTML = html;
}

function renderTopClients() {
    if (!topClients.length) {
        document.getElementById('top-clients-table').innerHTML = `<div class="alert alert-info">${__('empty_no_data_available')}</div>`;
        return;
    }
    let html = `<table class="admin-table"><thead><tr><th>${__('th_client')}</th><th>${__('th_invoices')}</th><th>${__('th_billed')}</th><th>${__('th_paid')}</th></tr></thead><tbody>`;
    topClients.forEach(c => {
        html += `<tr>
            <td>${c.clientName}</td>
            <td>${c.invoiceCount}</td>
            <td>${c.totalBilled?.toFixed(2)}</td>
            <td>${c.totalPaid?.toFixed(2)}</td>
        </tr>`;
    });
    html += `</tbody></table>`;
    document.getElementById('top-clients-table').innerHTML = html;
}

function renderUtilization() {
    if (!utilization.length) {
        document.getElementById('utilization-table').innerHTML = `<div class="alert alert-info">${__('empty_no_data_available')}</div>`;
        return;
    }
    let html = `<table class="admin-table"><thead><tr><th>${__('th_employee')}</th><th>${__('th_assigned')}</th><th>${__('th_completed')}</th><th>${__('th_rate')}</th><th>${__('th_days_active')}</th></tr></thead><tbody>`;
    utilization.forEach(u => {
        html += `<tr>
            <td>${u.employeeName}</td>
            <td>${u.jobsAssigned}</td>
            <td>${u.jobsCompleted}</td>
            <td>${u.completionRatePct?.toFixed(1)}%</td>
            <td>${u.daysActive}</td>
        </tr>`;
    });
    html += `</tbody></table>`;
    document.getElementById('utilization-table').innerHTML = html;
}

function renderCompletionRates() {
    if (!completionRates.length) {
        document.getElementById('completion-table').innerHTML = `<div class="alert alert-info">${__('empty_no_data_available')}</div>`;
        return;
    }
    let html = `<table class="admin-table"><thead><tr><th>${__('th_month')}</th><th>${__('th_total')}</th><th>${__('th_completed')}</th><th>${__('th_rate')}</th></tr></thead><tbody>`;
    completionRates.forEach(c => {
        html += `<tr>
            <td>${c.year}-${String(c.month).padStart(2,'0')}</td>
            <td>${c.totalJobs}</td>
            <td>${c.completedJobs}</td>
            <td>${c.completionRatePct?.toFixed(1)}%</td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('completion-table').innerHTML = html;
}

function renderOverdue() {
    if (!overdue) {
        document.getElementById('overdue-stats').innerHTML = `<div class="alert alert-info">${__('empty_no_data_available')}</div>`;
        return;
    }
    const html = `
        <div class="kpi-card"><span>${__('Total overdue value')}</span><strong>${overdue.totalOverdueAmount?.toFixed(2)}</strong></div>
        <div class="kpi-card"><span>${__('Overdue invoice count')}</span><strong>${overdue.overdueInvoiceCount}</strong></div>
        <div class="kpi-card"><span>${__('Average overdue amount')}</span><strong>${overdue.avgOverdueAmount?.toFixed(2)}</strong></div>
        <div class="kpi-card"><span>${__('Max days overdue')}</span><strong>${overdue.maxDaysOverdue} ${__('days')}</strong></div>
    `;
    document.getElementById('overdue-stats').innerHTML = html;
}

async function exportInvoices() {
    try {
        const response = await apiFetch('/reports/export', { download: true });
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `invoice-export-${new Date().toISOString().slice(0,19).replace(/:/g, '-')}.xlsx`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        window.URL.revokeObjectURL(url);
        showSuccess(__('msg_export_started'));
    } catch(e) { showError(e.message); }
}

document.getElementById('export-btn').addEventListener('click', exportInvoices);
loadReports();
window.addEventListener('i18nReady', function () { loadReports(); });