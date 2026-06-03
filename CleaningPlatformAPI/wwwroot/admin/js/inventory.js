// inventory.js
let items = [];
let editingItemId = null;

async function loadItems() {
    try {
        const res = await apiFetch('/inventory');
        if (res.success && res.data) {
            items = res.data;
            renderItems();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

function renderItems() {
    if (!items.length) {
        document.getElementById('inventory-list').innerHTML = '<div class="alert alert-info">' + __('empty_no_inventory') + '</div>';
        return;
    }
    let html = `<table class="admin-table"><thead><tr><th>${__('th_id')}</th><th>${__('th_name')}</th><th>${__('th_item_type')}</th><th>${__('th_qty_available')}</th><th>${__('th_unit')}</th></tr></thead><tbody>`;
    items.forEach(i => {
        html += `<tr class="item-row" data-item-id="${i.id}" style="cursor:pointer;">
            <td>${i.id}</td><td>${i.name}</td>
            <td><span class="badge ${i.type === 'Equipment' ? 'badge-active' : 'badge-info'}">${i.type}</span></td>
            <td>${i.quantity}</td>
            <td>${i.unit || '-'}</td>
        </tr>`;
    });
    html += '</tbody></table>';
    document.getElementById('inventory-list').innerHTML = html;

    document.querySelectorAll('.item-row').forEach(row => {
        row.addEventListener('click', () => openEditItem(row.dataset));
    });
}

function openEditItem(data) {
    editingItemId = parseInt(data.itemId);
    const item = items.find(i => i.id === editingItemId);
    if (!item) return;
    document.getElementById('item-modal-title').innerHTML = `${__('btn_edit')}: ${item.name}`;
    document.getElementById('item-id').value = item.id;
    document.getElementById('item-name').value = item.name;
    document.getElementById('item-type').value = item.type;
    document.getElementById('item-quantity').value = item.quantity;
    document.getElementById('item-unit').value = item.unit || '';
    document.getElementById('item-delete-form').style.display = 'block';
    document.getElementById('item-adjust-form').style.display = 'block';
    document.getElementById('adjust-amount').value = '';
    document.getElementById('item-modal').style.display = 'flex';
}

function openCreateItem() {
    editingItemId = null;
    document.getElementById('item-modal-title').innerHTML = __('btn_create_item');
    document.getElementById('item-id').value = '';
    document.getElementById('item-name').value = '';
    document.getElementById('item-type').value = 'Consumable';
    document.getElementById('item-quantity').value = '';
    document.getElementById('item-unit').value = '';
    document.getElementById('item-delete-form').style.display = 'none';
    document.getElementById('item-adjust-form').style.display = 'none';
    document.getElementById('item-modal').style.display = 'flex';
}

function closeItemModal() { document.getElementById('item-modal').style.display = 'none'; }

async function saveItem() {
    const payload = {
        name: document.getElementById('item-name').value,
        type: document.getElementById('item-type').value,
        quantity: parseFloat(document.getElementById('item-quantity').value) || 0,
        unit: document.getElementById('item-unit').value || null
    };
    try {
        let res;
        if (editingItemId) {
            res = await apiFetch(`/inventory/${editingItemId}`, { method: 'PUT', body: JSON.stringify(payload) });
        } else {
            res = await apiFetch('/inventory', { method: 'POST', body: JSON.stringify(payload) });
        }
        if (res.success) {
            showSuccess(editingItemId ? __('msg_inventory_updated') : __('msg_inventory_created'));
            closeItemModal();
            loadItems();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function deleteItem() {
    if (!confirm(__('msg_confirm_delete_service'))) return;
    try {
        const res = await apiFetch(`/inventory/${editingItemId}`, { method: 'DELETE' });
        if (res.success) {
            showSuccess(__('msg_inventory_deleted'));
            closeItemModal();
            loadItems();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

async function adjustStock() {
    const amount = parseFloat(document.getElementById('adjust-amount').value);
    if (isNaN(amount) || amount === 0) { showError('Please enter a non-zero amount.'); return; }
    try {
        const res = await apiFetch(`/inventory/${editingItemId}/adjust-stock`, {
            method: 'POST',
            body: JSON.stringify({ adjustmentAmount: amount })
        });
        if (res.success) {
            showSuccess(__('msg_stock_adjusted'));
            closeItemModal();
            loadItems();
        } else showError(res.message);
    } catch(e) { showError(e.message); }
}

document.getElementById('new-item-btn').addEventListener('click', openCreateItem);
document.getElementById('item-form').addEventListener('submit', (e) => { e.preventDefault(); saveItem(); });
document.getElementById('delete-item-btn').addEventListener('click', deleteItem);
document.getElementById('adjust-stock-btn').addEventListener('click', adjustStock);

loadItems();
window.addEventListener('i18nReady', function () { loadItems(); });
