// booking.js — dropdown auto-add + chips with x
let services = [];
let selectedIds = [];

async function loadServices() {
    const select = document.getElementById('service-select');
    if (!select) return;
    try {
        const res = await fetch('/api/services');
        const result = await res.json();
        if (!result.success || !result.data) return;
        services = result.data.filter(s => s.isActive);
        select.innerHTML = '<option value="">Select a service…</option>' +
            services.map(s => `<option value="${s.id}">${s.name}${s.priceAvg ? ` (from ${s.priceAvg} €)` : ''}</option>`).join('');
    } catch (e) {}
}

function renderChips() {
    const container = document.getElementById('selected-services');
    const hint = document.getElementById('service-hint');
    container.innerHTML = selectedIds.map(id => {
        const s = services.find(sv => sv.id === id);
        return s ? `<span style="display:inline-flex;align-items:center;gap:0.4rem;background:var(--primary);color:#fff;padding:0.45rem 0.9rem;border-radius:20px;font-size:0.9rem;font-weight:500;">
            ${s.name}
            <button type="button" onclick="removeService(${id})" style="background:none;border:none;color:#fff;font-size:1.1rem;cursor:pointer;padding:0;line-height:1;opacity:0.8;">&times;</button>
        </span>` : '';
    }).join('');
    hint.textContent = selectedIds.length === 0 ? 'Choose a service from the dropdown above' : '';
}

function removeService(id) {
    selectedIds = selectedIds.filter(sid => sid !== id);
    renderChips();
}

document.getElementById('service-select').addEventListener('change', () => {
    const select = document.getElementById('service-select');
    const id = parseInt(select.value);
    if (!id) return;
    if (selectedIds.includes(id)) return;
    selectedIds.push(id);
    select.value = '';
    renderChips();
});

document.getElementById('request-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const errorEl = document.getElementById('form-error');
    const submitBtn = e.target.querySelector('button[type="submit"]');
    errorEl.textContent = '';

    const name = document.getElementById('contact-name').value.trim();
    const phone = document.getElementById('contact-phone').value.trim();
    const email = document.getElementById('contact-email').value.trim();
    const notes = document.getElementById('contact-notes').value.trim();

    if (selectedIds.length === 0) { errorEl.textContent = 'Please add at least one service.'; return; }
    if (!name) { errorEl.textContent = 'Please enter your full name.'; return; }
    if (!phone) { errorEl.textContent = 'Please enter your phone number.'; return; }
    if (!email) { errorEl.textContent = 'Please enter your email address.'; return; }

    submitBtn.disabled = true;
    submitBtn.textContent = 'Sending…';

    try {
        const res = await fetch('/api/booking-requests', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                contactName: name,
                phone,
                email,
                notes: notes || null,
                serviceCatalogIds: selectedIds
            })
        });
        const result = await res.json();
        if (result.success) {
            const names = selectedIds.map(id => {
                const s = services.find(sv => sv.id === id);
                return s ? s.name : '';
            }).filter(Boolean).join(', ');
            document.getElementById('success-details').innerHTML = `
                <div style="display:grid;grid-template-columns:1fr 2fr;gap:0.3rem 1rem;">
                    <span style="color:var(--text-muted);">Request ID</span><span>#${result.data.id}</span>
                    <span style="color:var(--text-muted);">Services</span><span>${names}</span>
                    <span style="color:var(--text-muted);">Name</span><span>${name}</span>
                    <span style="color:var(--text-muted);">Email</span><span>${email}</span>
                    <span style="color:var(--text-muted);">Phone</span><span>${phone}</span>
                    <span style="color:var(--text-muted);">Status</span><span>${result.data.status}</span>
                </div>
            `;
            document.getElementById('request-form').style.display = 'none';
            document.getElementById('success-message').style.display = 'block';
        } else {
            errorEl.textContent = result.message || 'Request failed. Please try again.';
        }
    } catch (e) {
        errorEl.textContent = 'Network error. Please try again.';
    } finally {
        submitBtn.disabled = false;
        submitBtn.textContent = 'Send Request';
    }
});

document.getElementById('restart-btn').addEventListener('click', () => {
    selectedIds = [];
    renderChips();
    document.getElementById('request-form').reset();
    document.getElementById('request-form').style.display = 'block';
    document.getElementById('success-message').style.display = 'none';
    window.scrollTo({ top: 0, behavior: 'smooth' });
});

loadServices();
