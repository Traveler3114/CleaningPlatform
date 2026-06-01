import fs from 'node:fs';
import path from 'node:path';

const root = 'C:/Users/matej/Documents/VS Projects/CleaningPlatform/CleaningPlatformAPI/wwwroot';
const admin = path.join(root, 'admin');
const pub = path.join(root, 'public');
const portal = path.join(root, 'portal');

const langSwitcherCSS = `
/* ── Language Switcher ──────────────────────────── */
#lang-switcher {
    display: flex;
    align-items: center;
    gap: 2px;
    margin-left: 0.5rem;
}
.lang-btn {
    background: transparent;
    border: 1px solid rgba(255,255,255,0.2);
    color: rgba(255,255,255,0.6);
    padding: 0.15rem 0.45rem;
    font-size: 0.7rem;
    font-weight: 600;
    cursor: pointer;
    border-radius: 3px;
    transition: 0.15s;
    font-family: inherit;
    line-height: 1.4;
}
.lang-btn:hover { background: rgba(255,255,255,0.1); color: white; }
.lang-btn.active { background: var(--accent); color: white; border-color: var(--accent); }
.header-right #lang-switcher .lang-btn { border-color: var(--border); color: var(--text-muted); }
.header-right #lang-switcher .lang-btn.active { color: white; }
`;

// 1. Add CSS to all 3 stylesheets
fs.appendFileSync(path.join(admin, 'css/admin.css'), langSwitcherCSS, 'utf8');
fs.appendFileSync(path.join(pub, 'css/customer.css'), langSwitcherCSS, 'utf8');
fs.appendFileSync(path.join(portal, 'css/portal.css'), langSwitcherCSS, 'utf8');
console.log('CSS updated');

// 2. Helper: add i18n.js + lang-switcher + data-i18n title
function updateHtml(filePath, { titleKey, headerSelector, addLangSwitcher = true }) {
  let c = fs.readFileSync(filePath, 'utf8');
  const orig = c;

  // Add i18n.js before </body>
  if (!c.includes('js/i18n.js')) {
    c = c.replace('</body>', '<script src="/js/i18n.js"></script>\n</body>');
  }

  // Add lang-switcher in header
  if (addLangSwitcher && !c.includes('lang-switcher')) {
    const ls = '<div id="lang-switcher"><button class="lang-btn" data-lang="en">EN</button><button class="lang-btn" data-lang="hr">HR</button></div>';
    if (headerSelector) {
      c = c.replace(headerSelector, (m) => m + ls);
    }
  }

  // Add data-i18n to <title>
  if (titleKey) {
    c = c.replace(/<title>([^<]+)<\/title>/, `<title data-i18n="${titleKey}">$1</title>`);
  }

  if (c !== orig) {
    fs.writeFileSync(filePath, c, 'utf8');
    return true;
  }
  return false;
}

const langSwitcherHTML = '<div id="lang-switcher"><button class="lang-btn" data-lang="en">EN</button><button class="lang-btn" data-lang="hr">HR</button></div>';

// ===== ADMIN HTML =====
const adminFiles = [
  { file: 'index.html', title: 'page_admin_daily', hdr: /(<div class="header-right">)/ },
  { file: 'calendar.html', title: 'page_admin_calendar', hdr: /(<div class="header-right">)/ },
  { file: 'bookings.html', title: 'page_admin_bookings', hdr: /(<div class="header-right">)/ },
  { file: 'booking-detail.html', title: 'page_admin_booking_detail', hdr: /(<div class="header-right">)/ },
  { file: 'clients.html', title: 'page_admin_clients', hdr: /(<div class="header-right">)/ },
  { file: 'client-detail.html', title: 'page_admin_client_detail', hdr: /(<div class="header-right">)/ },
  { file: 'invoices.html', title: 'page_admin_invoices', hdr: /(<div class="header-right">)/ },
  { file: 'invoice-detail.html', title: 'page_admin_invoice_detail', hdr: /(<div class="header-right">)/ },
  { file: 'recurring.html', title: 'page_admin_recurring', hdr: /(<div class="header-right">)/ },
  { file: 'reports.html', title: 'page_admin_reports', hdr: /(<div class="header-right">)/ },
  { file: 'requests.html', title: 'page_admin_requests', hdr: /(<div class="header-right">)/ },
  { file: 'request-detail.html', title: 'page_admin_request_detail', hdr: /(<div class="header-right">)/ },
  { file: 'roles.html', title: 'page_admin_roles', hdr: /(<div class="header-right">)/ },
  { file: 'schedule.html', title: 'page_admin_schedule', hdr: /(<div class="header-right">)/ },
  { file: 'services.html', title: 'page_admin_services', hdr: /(<div class="header-right">)/ },
  { file: 'sops.html', title: 'page_admin_sops', hdr: /(<div class="header-right">)/ },
  { file: 'users.html', title: 'page_admin_users', hdr: /(<div class="header-right">)/ },
  { file: 'profile.html', title: 'page_admin_profile', hdr: /(<div class="header-right">)/ },
  { file: 'login.html', title: 'page_admin_login', hdr: null, addLangSwitcher: false },
];

for (const f of adminFiles) {
  const fp = path.join(admin, f.file);
  if (!fs.existsSync(fp)) continue;
  let changed = updateHtml(fp, { titleKey: f.title, headerSelector: f.hdr, addLangSwitcher: f.addLangSwitcher !== false });
  if (changed) console.log(`  admin/${f.file}`);
}

// Admin login page - add data-i18n to heading
let loginC = fs.readFileSync(path.join(admin, 'login.html'), 'utf8');
loginC = loginC.replace('<h1>Admin Login</h1>', '<h1 data-i18n="admin_login_title">Admin Login</h1>');
loginC = loginC.replace(/<p class="login-subtitle">[^<]+<\/p>/, '<p class="login-subtitle" data-i18n="admin_login_subtitle">Enter your credentials to access the admin panel.</p>');
fs.writeFileSync(path.join(admin, 'login.html'), loginC, 'utf8');
console.log('  admin/login.html - headings');

// Admin index.html - extra data-i18n
let indexC = fs.readFileSync(path.join(admin, 'index.html'), 'utf8');
indexC = indexC.replace('<strong>Daily View</strong>', '<strong data-i18n="nav_daily_view">Daily View</strong>');
indexC = indexC.replace('<h2 class="section-title">Bookings</h2>', '<h2 class="section-title" data-i18n="section_bookings">Bookings</h2>');
indexC = indexC.replace('<h2 class="section-title">Slots</h2>', '<h2 class="section-title" data-i18n="section_slots">Slots</h2>');
indexC = indexC.replace('<label for="selected-date">Date</label>', '<label for="selected-date" data-i18n="label_date">Date</label>');
indexC = indexC.replace('<button id="logout-btn" class="logout-btn">Sign out</button>', '<button id="logout-btn" class="logout-btn" data-i18n="nav_sign_out">Sign out</button>');
fs.writeFileSync(path.join(admin, 'index.html'), indexC, 'utf8');
console.log('  admin/index.html - extra data-i18n');

// ===== PUBLIC HTML =====
const publicFiles = [
  { file: 'index.html', title: 'page_public_home', hdr: null },
  { file: 'services.html', title: 'page_public_services', hdr: null },
  { file: 'book.html', title: 'page_public_book', hdr: null },
];

for (const f of publicFiles) {
  const fp = path.join(pub, f.file);
  if (!fs.existsSync(fp)) continue;
  let c = fs.readFileSync(fp, 'utf8');
  const orig = c;
  if (!c.includes('js/i18n.js')) c = c.replace('</body>', '<script src="/js/i18n.js"></script>\n</body>');
  if (!c.includes('lang-switcher') && c.includes('class="header-inner"')) {
    c = c.replace('class="header-inner"', 'class="header-inner">' + langSwitcherHTML);
  }
  if (f.title && c.includes('<title>')) {
    c = c.replace(/<title>([^<]+)<\/title>/, `<title data-i18n="${f.title}">$1</title>`);
  }
  if (c !== orig) {
    fs.writeFileSync(fp, c, 'utf8');
    console.log(`  public/${f.file}`);
  }
}

// Public index.html - hero + features + footer data-i18n
let pubIdx = fs.readFileSync(path.join(pub, 'index.html'), 'utf8');
pubIdx = pubIdx.replace('<div class="hero-badge">Professional Cleaning Services</div>', '<div class="hero-badge" data-i18n="hero_badge">Professional Cleaning Services</div>');
pubIdx = pubIdx.replace('<h1 class="serif">Spotless Results,<br>Every Time</h1>', '<h1 class="serif" data-i18n="hero_title">Spotless Results,<br>Every Time</h1>');
pubIdx = pubIdx.replace('<div class="hero-stat"><strong>500+</strong><span>Happy Clients</span></div>', '<div class="hero-stat"><strong>500+</strong><span data-i18n="hero_stat_clients">Happy Clients</span></div>');
pubIdx = pubIdx.replace('<div class="hero-stat"><strong>5★</strong><span>Rated Service</span></div>', '<div class="hero-stat"><strong>5★</strong><span data-i18n="hero_stat_rating">Rated Service</span></div>');
pubIdx = pubIdx.replace('<div class="hero-stat"><strong>Same Day</strong><span>Availability</span></div>', '<div class="hero-stat"><strong data-i18n="hero_stat_sameday">Same Day</strong><span data-i18n="hero_stat_availability">Availability</span></div>');
pubIdx = pubIdx.replace('<div class="hero-stat"><strong>10+ yrs</strong><span>Experience</span></div>', '<div class="hero-stat"><strong>10+ yrs</strong><span data-i18n="hero_stat_experience">Experience</span></div>');
pubIdx = pubIdx.replace('<div class="section-label">Why Us</div>', '<div class="section-label" data-i18n="section_why_us">Why Us</div>');
pubIdx = pubIdx.replace('<h2 class="serif">Cleaning Done Right</h2>', '<h2 class="serif" data-i18n="section_why_title">Cleaning Done Right</h2>');
pubIdx = pubIdx.replace('<span class="feature-icon">🧹</span><h3>Professional Equipment</h3><p>We use industry-grade tools and eco-friendly products for a deep, lasting clean on every job.</p>', '<span class="feature-icon">🧹</span><h3 data-i18n="feature_equipment_title">Professional Equipment</h3><p data-i18n="feature_equipment_desc">We use industry-grade tools and eco-friendly products for a deep, lasting clean on every job.</p>');
pubIdx = pubIdx.replace('<span class="feature-icon">⏱️</span><h3>On Time, Every Time</h3><p>Your time is valuable. We show up when scheduled and complete jobs efficiently without cutting corners.</p>', '<span class="feature-icon">⏱️</span><h3 data-i18n="feature_ontime_title">On Time, Every Time</h3><p data-i18n="feature_ontime_desc">Your time is valuable. We show up when scheduled and complete jobs efficiently without cutting corners.</p>');
pubIdx = pubIdx.replace('<span class="feature-icon">📱</span><h3>Easy Online Booking</h3><p>Book your slot in minutes, pick your service, choose a time — no phone calls required.</p>', '<span class="feature-icon">📱</span><h3 data-i18n="feature_easybook_title">Easy Online Booking</h3><p data-i18n="feature_easybook_desc">Book your slot in minutes, pick your service, choose a time — no phone calls required.</p>');
pubIdx = pubIdx.replace('<span class="feature-icon">✅</span><h3>Satisfaction Guaranteed</h3><p>Not happy with the result? We\'ll come back and make it right. Your satisfaction is our priority.</p>', '<span class="feature-icon">✅</span><h3 data-i18n="feature_guarantee_title">Satisfaction Guaranteed</h3><p data-i18n="feature_guarantee_desc">Not happy with the result? We\'ll come back and make it right. Your satisfaction is our priority.</p>');
pubIdx = pubIdx.replace('<h4>Services</h4>', '<h4 data-i18n="footer_services">Services</h4>');
pubIdx = pubIdx.replace('<h4>Contact</h4>', '<h4 data-i18n="footer_contact">Contact</h4>');
pubIdx = pubIdx.replace('<p>&copy; 2025 Chistify. All rights reserved.</p>', '<p data-i18n="footer_copyright">&copy; 2025 Chistify. All rights reserved.</p>');
pubIdx = pubIdx.replace('Staff Login', '<span data-i18n="nav_staff_login">Staff Login</span>');
fs.writeFileSync(path.join(pub, 'index.html'), pubIdx, 'utf8');
console.log('  public/index.html - hero/features/footer');

// ===== PORTAL HTML =====
const portalFiles = [
  { file: 'index.html', title: 'page_portal_dashboard', hdr: /(<div class="header-inner">)/ },
  { file: 'bookings.html', title: 'page_portal_bookings', hdr: /(<div class="header-inner">)/ },
  { file: 'booking-detail.html', title: 'page_portal_booking_detail', hdr: /(<div class="header-inner">)/ },
  { file: 'invoices.html', title: 'page_portal_invoices', hdr: /(<div class="header-inner">)/ },
  { file: 'invoice-detail.html', title: 'page_portal_invoice_detail', hdr: /(<div class="header-inner">)/ },
  { file: 'profile.html', title: 'page_portal_profile', hdr: /(<div class="header-inner">)/ },
  { file: 'login.html', title: 'page_portal_login', hdr: null, addLangSwitcher: false },
];

for (const f of portalFiles) {
  const fp = path.join(portal, f.file);
  if (!fs.existsSync(fp)) continue;
  let changed = updateHtml(fp, { titleKey: f.title, headerSelector: f.hdr, addLangSwitcher: f.addLangSwitcher !== false });
  if (changed) console.log(`  portal/${f.file}`);
}

// Portal login - add data-i18n
let pl = fs.readFileSync(path.join(portal, 'login.html'), 'utf8');
pl = pl.replace('<label for="email">Email address</label>', '<label for="email" data-i18n="label_email_address">Email address</label>');
pl = pl.replace('<button id="send-link-btn" class="btn" style="width:100%;justify-content:center;padding:0.7rem;">Send Magic Link</button>', '<button id="send-link-btn" class="btn" style="width:100%;justify-content:center;padding:0.7rem;" data-i18n="btn_send_magic_link">Send Magic Link</button>');
fs.writeFileSync(path.join(portal, 'login.html'), pl, 'utf8');
console.log('  portal/login.html - data-i18n');

// Portal index - headings
let pi = fs.readFileSync(path.join(portal, 'index.html'), 'utf8');
pi = pi.replace('<h1>Dashboard</h1>', '<h1 data-i18n="nav_dashboard">Dashboard</h1>');
pi = pi.replace('<h2 class="card-title">Upcoming Bookings</h2>', '<h2 class="card-title" data-i18n="section_upcoming_bookings">Upcoming Bookings</h2>');
pi = pi.replace('<h2 class="card-title">Recent Invoices</h2>', '<h2 class="card-title" data-i18n="section_recent_invoices">Recent Invoices</h2>');
pi = pi.replace('<button id="logout-btn" class="logout-btn">Sign out</button>', '<button id="logout-btn" class="logout-btn" data-i18n="nav_sign_out">Sign out</button>');
fs.writeFileSync(path.join(portal, 'index.html'), pi, 'utf8');
console.log('  portal/index.html - headings');

// ===== NAV JS FILES =====
// admin-common.js
let ac = fs.readFileSync(path.join(admin, 'js/admin-common.js'), 'utf8');
ac = ac.replace(/{ label: 'Daily View'/g, "{ labelKey: 'nav_daily_view', label: 'Daily View'");
ac = ac.replace(/{ label: 'Calendar'/g, "{ labelKey: 'nav_calendar', label: 'Calendar'");
ac = ac.replace(/{ label: 'Bookings'/g, "{ labelKey: 'nav_bookings', label: 'Bookings'");
ac = ac.replace(/{ label: 'Requests'/g, "{ labelKey: 'nav_requests', label: 'Requests'");
ac = ac.replace(/{ label: 'Recurring'/g, "{ labelKey: 'nav_recurring', label: 'Recurring'");
ac = ac.replace(/{ label: 'Invoices'/g, "{ labelKey: 'nav_invoices', label: 'Invoices'");
ac = ac.replace(/{ label: 'Client List'/g, "{ labelKey: 'nav_client_list', label: 'Client List'");
ac = ac.replace(/{ label: 'Schedule'/g, "{ labelKey: 'nav_schedule', label: 'Schedule'");
ac = ac.replace(/{ label: 'Services'/g, "{ labelKey: 'nav_services', label: 'Services'");
ac = ac.replace(/{ label: 'SOPs'/g, "{ labelKey: 'nav_sops', label: 'SOPs'");
ac = ac.replace(/{ label: 'Users'/g, "{ labelKey: 'nav_users', label: 'Users'");
ac = ac.replace(/{ label: 'Roles'/g, "{ labelKey: 'nav_roles', label: 'Roles'");
ac = ac.replace(/{ label: 'Reports'/g, "{ labelKey: 'nav_reports', label: 'Reports'");

ac = ac.replace("section: 'Operations'", "sectionKey: 'nav_section_operations', section: 'Operations'");
ac = ac.replace("section: 'Bookings'", "sectionKey: 'nav_section_bookings', section: 'Bookings'");
ac = ac.replace("section: 'Clients'", "sectionKey: 'nav_section_clients', section: 'Clients'");
ac = ac.replace("section: 'Config'", "sectionKey: 'nav_section_config', section: 'Config'");
ac = ac.replace("section: 'Admin'", "sectionKey: 'nav_section_admin', section: 'Admin'");

ac = ac.replace('<span class="nav-section-title">${section.section}</span>', '<span class="nav-section-title">${window.__(section.sectionKey) || section.section}</span>');
ac = ac.replace('class="nav-item${active}">${item.label}</a>', 'class="nav-item${active}">${window.__(item.labelKey) || item.label}</a>');
fs.writeFileSync(path.join(admin, 'js/admin-common.js'), ac, 'utf8');
console.log('  admin-common.js - nav with __()');

// shared.js
let sh = fs.readFileSync(path.join(pub, 'js/shared.js'), 'utf8');
sh = sh.replace(/{ label: 'Services'/g, "{ labelKey: 'nav_services', label: 'Services'");
sh = sh.replace(/{ label: 'Get a Quote'/g, "{ labelKey: 'nav_get_quote', label: 'Get a Quote'");
sh = sh.replace(/{ label: 'Sign In'/g, "{ labelKey: 'nav_sign_in', label: 'Sign In'");
sh = sh.replace('${item.label}</a>', '${window.__(item.labelKey) || item.label}</a>');
fs.writeFileSync(path.join(pub, 'js/shared.js'), sh, 'utf8');
console.log('  shared.js - nav with __()');

// portal-common.js
let pc = fs.readFileSync(path.join(portal, 'js/portal-common.js'), 'utf8');
pc = pc.replace(/{ label: 'Dashboard'/g, "{ labelKey: 'nav_dashboard', label: 'Dashboard'");
pc = pc.replace(/{ label: 'Bookings'/g, "{ labelKey: 'nav_bookings', label: 'Bookings'");
pc = pc.replace(/{ label: 'Invoices'/g, "{ labelKey: 'nav_invoices', label: 'Invoices'");
pc = pc.replace(/{ label: 'Profile'/g, "{ labelKey: 'nav_profile', label: 'Profile'");
pc = pc.replace('${item.label}</a>', '${window.__(item.labelKey) || item.label}</a>');
fs.writeFileSync(path.join(portal, 'js/portal-common.js'), pc, 'utf8');
console.log('  portal-common.js - nav with __()');

// ===== ADMIN JS FILES - Replace hardcoded strings =====
const adminReplacements = {
  "'No bookings found.'": "__('empty_no_bookings_all')",
  "'No services found.'": "__('empty_no_services')",
  "'No clients found.'": "__('empty_no_clients')",
  "'No requests found.'": "__('empty_no_requests')",
  "'No invoices found.'": "__('empty_no_invoices')",
  "'No schedule entries found.'": "__('empty_no_schedule')",
  "'No date overrides defined.'": "__('empty_no_overrides')",
  "'No roles found.'": "__('empty_no_roles')",
  "'No users found.'": "__('empty_no_users')",
  "'No services assigned.'": "__('empty_no_services_assigned')",
  "'No SOPs assigned.'": "__('empty_no_sops_assigned')",
  "'No data available.'": "__('empty_no_data')",
  "'No payments recorded.'": "__('empty_no_payments')",
  "'No recurring schedules found.'": "__('empty_no_recurring')",
  "'No templates found.'": "__('empty_no_templates')",
  "'No bookings assigned.'": "__('empty_no_bookings_assigned')",
  "'No contacts found.'": "__('empty_no_contacts')",
  "'No sites found.'": "__('empty_no_sites')",
  "'Failed to load bookings.'": "__('msg_failed_bookings')",
  "'Failed to load slots.'": "__('msg_failed_slots')",
  "'Status updated'": "__('msg_status_updated')",
  "'Booking created'": "__('msg_booking_created')",
  "'Changes saved.'": "__('msg_changes_saved')",
  "'Service updated'": "__('msg_service_updated')",
  "'Service created'": "__('msg_service_created')",
  "'Service deleted'": "__('msg_service_deleted')",
  "'Client updated'": "__('msg_client_updated')",
  "'Site added'": "__('msg_site_added')",
  "'Site deactivated'": "__('msg_site_deactivated')",
  "'User created'": "__('msg_user_created')",
  "'Password reset'": "__('msg_password_reset')",
  "'Template created'": "__('msg_template_created')",
  "'Template updated'": "__('msg_template_updated')",
  "'Template deactivated'": "__('msg_template_deactivated')",
  "'Item added'": "__('msg_item_added')",
  "'Item removed'": "__('msg_item_removed')",
  "'Schedule updated'": "__('msg_schedule_updated')",
  "'Schedule added'": "__('msg_schedule_added')",
  "'Schedule deleted'": "__('msg_schedule_deleted')",
  "'Override saved'": "__('msg_override_saved')",
  "'Override deleted'": "__('msg_override_deleted')",
  "'Role updated'": "__('msg_role_updated')",
  "'Role created'": "__('msg_role_created')",
  "'Role deleted'": "__('msg_role_deleted')",
  "'Assignment added'": "__('msg_assignment_added')",
  "'Assignment removed'": "__('msg_assignment_removed')",
  "'Service added'": "__('msg_service_added_booking')",
  "'Service removed'": "__('msg_service_removed_booking')",
  "'Price updated'": "__('msg_price_updated')",
  "'Recurring schedule created'": "__('msg_recurring_created')",
  "'Series ended'": "__('msg_series_ended')",
  "'Export started'": "__('msg_export_started')",
  "'Email sent to customer.'": "__('msg_email_sent')",
  "'Request cancelled.'": "__('msg_request_cancelled')",
  "'User status updated'": "__('msg_status_updated_user')",
  "'Please select a client'": "__('msg_select_client')",
  "'Select an employee'": "__('msg_select_employee')",
  "'Select a service'": "__('msg_select_service')",
  "'Please enter a booking ID'": "__('msg_enter_booking_id')",
  "'Please enter a valid amount'": "__('msg_enter_valid_amount')",
  "'Role name is required'": "__('msg_role_name_required')",
  "'Item text required'": "__('msg_item_text_required')",
  "'Delete this service?'": "__('msg_confirm_delete_service')",
  "'Deactivate this SOP?'": "__('msg_confirm_deactivate_sop')",
  "'Delete this day?'": "__('msg_confirm_delete_schedule')",
  "'Delete this override?'": "__('msg_confirm_delete_override')",
  "'Deactivate this site?'": "__('msg_confirm_deactivate_site')",
  "'Remove this assignment?'": "__('msg_confirm_remove_assignment')",
  "'Remove this service?'": "__('msg_confirm_remove_service')",
  "'Remove this checklist item?'": "__('msg_confirm_remove_item')",
  "'Delete this role?'": "__('msg_confirm_delete_role')",
  "'Cancel this request?'": "__('msg_confirm_cancel_request_title')",
};

const adminJsDir = path.join(admin, 'js');
const adminJsSkip = ['admin-common.js', 'admin-api.js', 'dashboard.js'];
const adminJsFiles = fs.readdirSync(adminJsDir).filter(f => f.endsWith('.js') && !adminJsSkip.includes(f));

for (const file of adminJsFiles) {
  const fp = path.join(adminJsDir, file);
  let c = fs.readFileSync(fp, 'utf8');
  const orig = c;
  for (const [k, v] of Object.entries(adminReplacements)) {
    // Escape special regex chars in key
    const escaped = k.replace(/[.*+?^${}()|[\]\\']/g, '\\$&');
    // Match the string as-is (with single quotes), replace with function call
    const regex = new RegExp(escaped, 'g');
    c = c.replace(regex, v);
  }
  if (c !== orig) {
    fs.writeFileSync(fp, c, 'utf8');
    console.log(`  admin/js/${file}`);
  }
}

// ===== PORTAL JS FILES =====
const portalReplacements = {
  "'No upcoming bookings.'": "__('empty_no_upcoming')",
  "'No invoices yet.'": "__('empty_no_invoices_yet')",
  "'Failed to load dashboard'": "__('msg_failed_dashboard')",
  "'Failed to load bookings. Please try again.'": "__('msg_failed_bookings_load')",
  "'Failed to load invoices.'": "__('msg_failed_invoices_load')",
  "'Failed to load profile'": "__('msg_failed_profile')",
  "'Please enter your email address.'": "__('msg_enter_email')",
  "'Something went wrong.'": "__('msg_something_wrong')",
  "'Could not connect to server. Please try again.'": "__('msg_could_not_connect')",
  "'Invalid booking ID.'": "__('msg_invalid_booking_id')",
  "'Invalid invoice ID.'": "__('msg_invalid_invoice_id')",
  "'Network error. Please try again.'": "__('msg_network_error')",
  "'Please add at least one service.'": "__('msg_add_service_first')",
  "'Please enter your full name.'": "__('msg_enter_name')",
  "'Please enter your phone number.'": "__('msg_enter_phone')",
  "'Request failed. Please try again.'": "__('msg_request_failed')",
  "'Sending...'": "__('msg_sending')",
  "'Send Magic Link'": "__('btn_send_magic_link')",
};

const portalJsDir = path.join(portal, 'js');
const portalJsFiles = fs.readdirSync(portalJsDir).filter(f => f.endsWith('.js') && f !== 'portal-common.js');
for (const file of portalJsFiles) {
  const fp = path.join(portalJsDir, file);
  let c = fs.readFileSync(fp, 'utf8');
  const orig = c;
  for (const [k, v] of Object.entries(portalReplacements)) {
    const escaped = k.replace(/[.*+?^${}()|[\]\\']/g, '\\$&');
    c = c.replace(new RegExp(escaped, 'g'), v);
  }
  if (c !== orig) {
    fs.writeFileSync(fp, c, 'utf8');
    console.log(`  portal/js/${file}`);
  }
}

// ===== PUBLIC JS FILES =====
const publicJsDir = path.join(pub, 'js');
const publicJsFiles = fs.readdirSync(publicJsDir).filter(f => f.endsWith('.js') && f !== 'shared.js');
for (const file of publicJsFiles) {
  const fp = path.join(publicJsDir, file);
  let c = fs.readFileSync(fp, 'utf8');
  const orig = c;
  for (const [k, v] of Object.entries(portalReplacements)) {
    const escaped = k.replace(/[.*+?^${}()|[\]\\']/g, '\\$&');
    c = c.replace(new RegExp(escaped, 'g'), v);
  }
  if (c !== orig) {
    fs.writeFileSync(fp, c, 'utf8');
    console.log(`  public/js/${file}`);
  }
}

console.log('\nAll complete!');
