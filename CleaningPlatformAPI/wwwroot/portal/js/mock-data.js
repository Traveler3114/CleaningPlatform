// mock-data.js — hardcoded data for the client portal prototype

const MOCK_CLIENT = {
    id: 1,
    name: 'Ana Horvat',
    email: 'ana.horvat@email.com',
    phone: '+385 91 234 5678',
    company: '',
    since: '2024-03-15',
    avatarInitial: 'AH'
};

const MOCK_SITES = [
    { id: 1, name: 'Dom — Kneza Branimira 12', address: 'Kneza Branimira 12, Zagreb' },
    { id: 2, name: 'Ured — Ilica 25', address: 'Ilica 25, 3. kat, Zagreb' }
];

const MOCK_PAYMENT_METHODS = [
    { id: 1, type: 'visa', last4: '4242', expiry: '06/27' }
];

const MOCK_BOOKINGS = [
    {
        id: 101,
        date: '2026-05-25',
        time: '09:00',
        serviceType: 'Vehicle',
        services: [{ name: 'Exterior Premium Wash', quantity: 1, price: 25.00 }],
        status: 'confirmed',
        site: null,
        employees: ['Marko'],
        notes: 'Bijeli VW Golf, reg. ZG 1234-AB',
        createdAt: '2026-05-20T10:30:00'
    },
    {
        id: 102,
        date: '2026-05-27',
        time: '14:00',
        serviceType: 'SiteBased',
        services: [{ name: 'Office Cleaning', quantity: 1, price: 120.00 }],
        status: 'pending',
        site: 'Ured — Ilica 25',
        employees: ['Ivana', 'Tomislav'],
        notes: '',
        createdAt: '2026-05-22T08:15:00'
    },
    {
        id: 103,
        date: '2026-05-28',
        time: '11:00',
        serviceType: 'Boat',
        services: [{ name: 'Boat Basic Wash', quantity: 1, price: 85.00 }],
        status: 'confirmed',
        site: null,
        employees: ['Petar'],
        notes: 'Gumenjak 5m, ACI marina',
        createdAt: '2026-05-22T16:45:00'
    },
    {
        id: 99,
        date: '2026-05-20',
        time: '10:00',
        serviceType: 'Vehicle',
        services: [{ name: 'Interior & Exterior Detail', quantity: 1, price: 150.00 }],
        status: 'completed',
        site: null,
        employees: ['Marko', 'Ivana'],
        notes: 'Crni Audi A4',
        createdAt: '2026-05-18T09:00:00'
    },
    {
        id: 97,
        date: '2026-05-15',
        time: '08:00',
        serviceType: 'SiteBased',
        services: [{ name: 'Stairwell Cleaning', quantity: 1, price: 90.00 }],
        status: 'completed',
        site: 'Dom — Kneza Branimira 12',
        employees: ['Tomislav'],
        notes: 'Zgrada A, ulaz 1-4',
        createdAt: '2026-05-10T11:20:00'
    },
    {
        id: 94,
        date: '2026-05-08',
        time: '13:00',
        serviceType: 'Vehicle',
        services: [{ name: 'Exterior Premium Wash', quantity: 1, price: 25.00 }],
        status: 'cancelled',
        site: null,
        employees: [],
        notes: '',
        createdAt: '2026-05-05T14:30:00'
    }
];

const MOCK_INVOICES = [
    {
        id: 501,
        number: 'INV-2026-0042',
        issueDate: '2026-05-21',
        dueDate: '2026-06-05',
        status: 'sent',
        items: [
            { description: 'Interior & Exterior Detail — Booking #99', quantity: 1, unitPrice: 150.00 }
        ],
        subTotal: 150.00,
        vatAmount: 37.50,
        discountAmount: 0,
        totalAmount: 187.50,
        payments: [],
        bookingId: 99
    },
    {
        id: 502,
        number: 'INV-2026-0043',
        issueDate: '2026-05-16',
        dueDate: '2026-05-31',
        status: 'paid',
        items: [
            { description: 'Stairwell Cleaning — Booking #97', quantity: 1, unitPrice: 90.00 }
        ],
        subTotal: 90.00,
        vatAmount: 22.50,
        discountAmount: 0,
        totalAmount: 112.50,
        payments: [
            { date: '2026-05-18', method: 'BankTransfer', amount: 112.50 }
        ],
        bookingId: 97
    }
];

const MOCK_UPCOMING = MOCK_BOOKINGS.filter(b => b.status !== 'completed' && b.status !== 'cancelled');
const MOCK_COMPLETED = MOCK_BOOKINGS.filter(b => b.status === 'completed');

function findBooking(id) {
    return MOCK_BOOKINGS.find(b => b.id === id);
}

function findInvoice(id) {
    return MOCK_INVOICES.find(i => i.id === id);
}

function formatCurrency(amount) {
    return '\u20AC' + amount.toFixed(2);
}

function formatDate(dateStr) {
    const d = new Date(dateStr);
    return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function statusBadge(status) {
    const cls = status.toLowerCase();
    return `<span class="badge badge-${cls}">${status}</span>`;
}
