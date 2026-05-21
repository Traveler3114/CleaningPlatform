// services.js
const API_BASE = '';

let services = [];

async function loadServices() {
    const grid = document.getElementById('services-grid');
    if (!grid) return;
    grid.innerHTML = '<p class="services-loading"><span class="spinner"></span> Loading services…</p>';
    try {
        const res = await fetch(`${API_BASE}/api/services`);
        const result = await res.json();
        if (!result.success || !result.data || result.data.length === 0) {
            grid.innerHTML = '<p class="services-loading">No services available at the moment.</p>';
            return;
        }
        services = result.data.filter(s => s.isActive);
        renderCategoryFilters();
        renderServiceCards('all');
    } catch (e) {
        grid.innerHTML = '<p class="services-loading">Could not load services. Please refresh.</p>';
    }
}

function renderCategoryFilters() {
    const bar = document.getElementById('category-filter');
    if (!bar) return;
    const cats = ['all', ...new Set(services.map(s => s.category).filter(Boolean))];
    bar.innerHTML = cats.map(cat => `<button class="cat-btn ${cat === 'all' ? 'active' : ''}" data-cat="${cat}" onclick="filterCategory('${cat}')">${cat === 'all' ? 'All Services' : cat}</button>`).join('');
}

window.filterCategory = function(cat) {
    document.querySelectorAll('.cat-btn').forEach(b => b.classList.toggle('active', b.dataset.cat === cat));
    renderServiceCards(cat);
};

function renderServiceCards(cat) {
    const grid = document.getElementById('services-grid');
    if (!grid) return;
    const filtered = cat === 'all' ? services : services.filter(s => s.category === cat);
    if (filtered.length === 0) {
        grid.innerHTML = '<p class="services-loading">No services in this category.</p>';
        return;
    }
    grid.innerHTML = filtered.map(s => {
        let priceHtml = '';
        if (s.priceMin && s.priceMax) priceHtml = `<strong>${s.priceMin}–${s.priceMax} €</strong> / ${s.unit || 'service'}`;
        else if (s.priceAvg) priceHtml = `<strong>from ${s.priceAvg} €</strong> / ${s.unit || 'service'}`;
        return `
            <div class="service-card" data-id="${s.id}" onclick="selectService(${s.id})">
                ${s.category ? `<div class="service-category-badge">${s.category}</div>` : ''}
                <h3>${s.name}</h3>
                ${priceHtml ? `<p class="service-price">${priceHtml}</p>` : ''}
                ${s.description ? `<p class="service-description" style="font-size:0.8rem; margin-top:0.5rem;">${s.description.substring(0,100)}</p>` : ''}
            </div>
        `;
    }).join('');
}

window.selectService = function(id) {
    window.location.href = `book.html?serviceId=${id}`;
};

loadServices();