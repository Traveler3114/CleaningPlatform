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
                <div class="stat-card"><span>Revenue MTD</span><strong>${data.monthlyRevenue?.totalRevenue?.toFixed(2) ?? '—'}</strong></div>
                <div class="stat-card"><span>Overdue</span><strong>${data.overdueInvoices?.totalOverdueAmount?.toFixed(2) ?? '0'}</strong><small>${data.overdueInvoices?.overdueInvoiceCount ?? 0} invoices</small></div>
                <div class="stat-card"><span>Completion rate</span><strong>${data.completionRate?.completionRatePct?.toFixed(1) ?? '0'}%</strong></div>
                <div class="stat-card"><span>Top client</span><strong>${data.topClient?.clientName ?? '—'}</strong></div>
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
        document.getElementById('revenue-table').innerHTML = '<div class="alert alert-info">No data available.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>Month</th><th>Invoices</th><th>Revenue</th><th>VAT</th><th>Discount</th></tr></thead><tbody>';
    revenue.forEach(r => {
        html += `<tr>
            <td>${r.year}-${String(r.month).padStart(2,'0')}</td>
            <td>${r.invoiceCount}</td>
            <td>${r.totalRevenue?.toFixed(2)}</td>
            <td>${r.totalVat?.toFixed(2)}</td>
            <td>${r.totalDiscount?.toFixed(2)}</td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('revenue-table').innerHTML = html;
}

function renderTopClients() {
    if (!topClients.length) {
        document.getElementById('top-clients-table').innerHTML = '<div class="alert alert-info">No data available.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>Client</th><th>Invoices</th><th>Billed</th><th>Paid</th></tr></thead><tbody>';
    topClients.forEach(c => {
        html += `<tr>
            <td>${c.clientName}</td>
            <td>${c.invoiceCount}</td>
            <td>${c.totalBilled?.toFixed(2)}</td>
            <td>${c.totalPaid?.toFixed(2)}</td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('top-clients-table').innerHTML = html;
}

function renderUtilization() {
    if (!utilization.length) {
        document.getElementById('utilization-table').innerHTML = '<div class="alert alert-info">No data available.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>Employee</th><th>Assigned</th><th>Completed</th><th>Rate</th><th>Days Active</th></tr></thead><tbody>';
    utilization.forEach(u => {
        html += `<tr>
            <td>${u.employeeName}</td>
            <td>${u.jobsAssigned}</td>
            <td>${u.jobsCompleted}</td>
            <td>${u.completionRatePct?.toFixed(1)}%</td>
            <td>${u.daysActive}</td>
        </tr>`;
    });
    html += '</tbody></tr>';
    document.getElementById('utilization-table').innerHTML = html;
}

function renderCompletionRates() {
    if (!completionRates.length) {
        document.getElementById('completion-table').innerHTML = '<div class="alert alert-info">No data available.</div>';
        return;
    }
    let html = '<table class="admin-table"><thead><tr><th>Month</th><th>Total</th><th>Completed</th><th>Rate</th></tr></thead><tbody>';
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
        document.getElementById('overdue-stats').innerHTML = '<div class="alert alert-info">No data available.</div>';
        return;
    }
    const html = `
        <div class="kpi-card"><span>Total overdue value</span><strong>${overdue.totalOverdueAmount?.toFixed(2)}</strong></div>
        <div class="kpi-card"><span>Overdue invoice count</span><strong>${overdue.overdueInvoiceCount}</strong></div>
        <div class="kpi-card"><span>Average overdue amount</span><strong>${overdue.avgOverdueAmount?.toFixed(2)}</strong></div>
        <div class="kpi-card"><span>Max days overdue</span><strong>${overdue.maxDaysOverdue} days</strong></div>
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
        showSuccess('Export started');
    } catch(e) { showError(e.message); }
}

document.getElementById('export-btn').addEventListener('click', exportInvoices);
loadReports();