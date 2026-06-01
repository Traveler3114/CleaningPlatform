$root = "C:\Users\matej\Documents\VS Projects\CleaningPlatform\CleaningPlatformAPI\wwwroot"
$admin = "$root\admin"
$public = "$root\public"
$portal = "$root\portal"

# ======================================================================
# 1. Add language switcher CSS to all stylesheets
# ======================================================================
$langSwitcherCss = @"

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

.lang-btn:hover {
    background: rgba(255,255,255,0.1);
    color: white;
}

.lang-btn.active {
    background: var(--accent);
    color: white;
    border-color: var(--accent);
}

.header-right #lang-switcher .lang-btn {
    border-color: var(--border);
    color: var(--text-muted);
}

.header-right #lang-switcher .lang-btn.active {
    color: white;
}
"@

Add-Content -Path "$admin\css\admin.css" -Value $langSwitcherCss -Encoding UTF8
Write-Host "Added lang-switcher CSS to admin.css"

Add-Content -Path "$public\css\customer.css" -Value $langSwitcherCss -Encoding UTF8
Write-Host "Added lang-switcher CSS to customer.css"

Add-Content -Path "$portal\css\portal.css" -Value $langSwitcherCss -Encoding UTF8
Write-Host "Added lang-switcher CSS to portal.css"

# ======================================================================
# 2. Update all ADMIN HTML files
# ======================================================================
$adminFiles = @(
    "index.html", "calendar.html", "bookings.html", "booking-detail.html",
    "clients.html", "client-detail.html", "invoices.html", "invoice-detail.html",
    "recurring.html", "reports.html", "requests.html", "request-detail.html",
    "roles.html", "schedule.html", "services.html", "sops.html", "users.html",
    "profile.html", "login.html"
)

foreach ($file in $adminFiles) {
    $path = "$admin\$file"
    if (!(Test-Path $path)) { Write-Host "  Skipping $file (not found)"; continue }
    
    $c = Get-Content $path -Raw
    $old = $c
    
    # Add i18n.js script before closing </body>
    if ($c -notmatch "js/i18n.js") {
        $c = $c -replace '(</body>)', '<script src="/js/i18n.js"></script>$1'
    }
    
    # Add lang-switcher in header-right
    if ($c -notmatch "lang-switcher" -and $c -match '<div class="header-right">') {
        $c = $c -replace '(<div class="header-right">)', '$1<div id="lang-switcher"><button class="lang-btn" data-lang="en">EN</button><button class="lang-btn" data-lang="hr">HR</button></div>'
    }
    
    # Add data-i18n to page title
    if ($c -match '<title>([^<]+)</title>') {
        $titleKey = switch -Regex ($Matches[1]) {
            'Daily View' { 'page_admin_daily' }
            'Calendar' { 'page_admin_calendar' }
            'Bookings' { 'page_admin_bookings' }
            'Booking Detail' { 'page_admin_booking_detail' }
            'Clients' { 'page_admin_clients' }
            'Client Detail' { 'page_admin_client_detail' }
            'Invoices' { 'page_admin_invoices' }
            'Invoice Detail' { 'page_admin_invoice_detail' }
            'Recurring' { 'page_admin_recurring' }
            'Reports' { 'page_admin_reports' }
            'Requests' { 'page_admin_requests' }
            'Request Detail' { 'page_admin_request_detail' }
            'Roles' { 'page_admin_roles' }
            'Schedule' { 'page_admin_schedule' }
            'Services' { 'page_admin_services' }
            'SOPs' { 'page_admin_sops' }
            'Users' { 'page_admin_users' }
            'Profile' { 'page_admin_profile' }
            'Admin Login' { 'page_admin_login' }
        }
        if ($titleKey) {
            $c = $c -replace '<title>([^<]+)</title>', "<title data-i18n=""$titleKey"">`$1</title>"
        }
    }

    if ($c -ne $old) {
        Set-Content -Path $path -Value $c -Encoding UTF8
        Write-Host "  Updated admin/$file"
    }
}

# ======================================================================
# 2b. SPECIAL: Add data-i18n to admin login page headings
# ======================================================================
$loginPath = "$admin\login.html"
$c = Get-Content $loginPath -Raw
if ($c -match '<h1>[^<]+</h1>') {
    $c = $c -replace '<h1>Admin Login</h1>', '<h1 data-i18n="admin_login_title">Admin Login</h1>'
}
if ($c -match '<p class="login-subtitle">[^<]+</p>') {
    $c = $c -replace '<p class="login-subtitle">[^<]+</p>', '<p class="login-subtitle" data-i18n="admin_login_subtitle">Enter your credentials to access the admin panel.</p>'
}
Set-Content -Path $loginPath -Value $c -Encoding UTF8
Write-Host "  Updated admin/login.html headings"

# ======================================================================
# 2c. SPECIAL: Add data-i18n to admin index.html breadcrumb + headings
# ======================================================================
$indexPath = "$admin\index.html"
$c = Get-Content $indexPath -Raw
$c = $c -replace '<strong>Daily View</strong>', '<strong data-i18n="nav_daily_view">Daily View</strong>'
$c = $c -replace '<h2 class="section-title">Bookings</h2>', '<h2 class="section-title" data-i18n="section_bookings">Bookings</h2>'
$c = $c -replace '<h2 class="section-title">Slots</h2>', '<h2 class="section-title" data-i18n="section_slots">Slots</h2>'
$c = $c -replace '<label for="selected-date">Date</label>', '<label for="selected-date" data-i18n="label_date">Date</label>'
$c = $c -replace '<button id="logout-btn" class="logout-btn">Sign out</button>', '<button id="logout-btn" class="logout-btn" data-i18n="nav_sign_out">Sign out</button>'
$c = $c -replace 'class="user-name">Loading...<', 'class="user-name" data-i18n="ui_loading">Loading...<'
Set-Content -Path $indexPath -Value $c -Encoding UTF8
Write-Host "  Updated admin/index.html with data-i18n"

# ======================================================================
# 3. Update all PUBLIC HTML files
# ======================================================================
$publicFiles = @("index.html", "services.html", "book.html")

foreach ($file in $publicFiles) {
    $path = "$public\$file"
    if (!(Test-Path $path)) { Write-Host "  Skipping $file (not found)"; continue }
    
    $c = Get-Content $path -Raw
    $old = $c
    
    # Add i18n.js script
    if ($c -notmatch "js/i18n.js") {
        $c = $c -replace '(</body>)', '<script src="/js/i18n.js"></script>$1'
    }
    
    # Add lang-switcher in header-inner
    if ($c -notmatch "lang-switcher" -and $c -match '<div class="header-inner">') {
        $c = $c -replace '(<div class="header-inner">)', '$1<div id="lang-switcher"><button class="lang-btn" data-lang="en">EN</button><button class="lang-btn" data-lang="hr">HR</button></div>'
    }
    
    # Add data-i18n to page title
    if ($c -match '<title>([^<]+)</title>') {
        $titleKey = switch -Regex ($Matches[1]) {
            'Professional Cleaning Services' { 'page_public_home' }
            'Our Services' { 'page_public_services' }
            'Book a Cleaning' { 'page_public_book' }
        }
        if ($titleKey) {
            $c = $c -replace '<title>([^<]+)</title>', "<title data-i18n=""$titleKey"">`$1</title>"
        }
    }
    
    if ($c -ne $old) {
        Set-Content -Path $path -Value $c -Encoding UTF8
        Write-Host "  Updated public/$file"
    }
}

# ======================================================================
# 3b. SPECIAL: Public index.html hero + features + footer
# ======================================================================
$pubIndex = "$public\index.html"
$c = Get-Content $pubIndex -Raw
$c = $c -replace '<div class="hero-badge">Professional Cleaning Services</div>', '<div class="hero-badge" data-i18n="hero_badge">Professional Cleaning Services</div>'
$c = $c -replace '<h1 class="serif">Spotless Results,<br>Every Time</h1>', '<h1 class="serif" data-i18n="hero_title">Spotless Results,<br>Every Time</h1>'
$c = $c -replace '<p>Book your cleaning appointment online in minutes\.([^<]*)</p>', '<p data-i18n="hero_subtitle">Book your cleaning appointment online in minutes. We handle vehicles, offices, stairwells, boats, and more — done right, done professionally.</p>'
$c = $c -replace '<div class="hero-stat"><strong>500\+</strong><span>Happy Clients</span></div>', '<div class="hero-stat"><strong>500+</strong><span data-i18n="hero_stat_clients">Happy Clients</span></div>'
$c = $c -replace '<div class="hero-stat"><strong>5★</strong><span>Rated Service</span></div>', '<div class="hero-stat"><strong>5★</strong><span data-i18n="hero_stat_rating">Rated Service</span></div>'
$c = $c -replace '<div class="hero-stat"><strong>Same Day</strong><span>Availability</span></div>', '<div class="hero-stat"><strong data-i18n="hero_stat_sameday">Same Day</strong><span data-i18n="hero_stat_availability">Availability</span></div>'
$c = $c -replace '<div class="hero-stat"><strong>10\+ yrs</strong><span>Experience</span></div>', '<div class="hero-stat"><strong>10+ yrs</strong><span data-i18n="hero_stat_experience">Experience</span></div>'
$c = $c -replace '<div class="section-label">Why Us</div>', '<div class="section-label" data-i18n="section_why_us">Why Us</div>'
$c = $c -replace '<h2 class="serif">Cleaning Done Right</h2>', '<h2 class="serif" data-i18n="section_why_title">Cleaning Done Right</h2>'
$c = $c -replace '<p>We combine professional-grade equipment, trained staff([^<]*)</p>', '<p data-i18n="section_why_subtitle">We combine professional-grade equipment, trained staff, and a streamlined booking system to deliver results you can count on.</p>'

# Feature cards
$c = $c -replace '<span class="feature-icon">🧹</span><h3>Professional Equipment</h3><p>We use industry-grade tools and eco-friendly products for a deep, lasting clean on every job.</p>', '<span class="feature-icon">🧹</span><h3 data-i18n="feature_equipment_title">Professional Equipment</h3><p data-i18n="feature_equipment_desc">We use industry-grade tools and eco-friendly products for a deep, lasting clean on every job.</p>'
$c = $c -replace '<span class="feature-icon">⏱️</span><h3>On Time, Every Time</h3><p>Your time is valuable. We show up when scheduled and complete jobs efficiently without cutting corners.</p>', '<span class="feature-icon">⏱️</span><h3 data-i18n="feature_ontime_title">On Time, Every Time</h3><p data-i18n="feature_ontime_desc">Your time is valuable. We show up when scheduled and complete jobs efficiently without cutting corners.</p>'
$c = $c -replace '<span class="feature-icon">📱</span><h3>Easy Online Booking</h3><p>Book your slot in minutes, pick your service, choose a time — no phone calls required.</p>', '<span class="feature-icon">📱</span><h3 data-i18n="feature_easybook_title">Easy Online Booking</h3><p data-i18n="feature_easybook_desc">Book your slot in minutes, pick your service, choose a time — no phone calls required.</p>'
$c = $c -replace '<span class="feature-icon">✅</span><h3>Satisfaction Guaranteed</h3><p>Not happy with the result.*?</p>', '<span class="feature-icon">✅</span><h3 data-i18n="feature_guarantee_title">Satisfaction Guaranteed</h3><p data-i18n="feature_guarantee_desc">Not happy with the result? We\'ll come back and make it right. Your satisfaction is our priority.</p>'

# Footer
$c = $c -replace '<p>Professional cleaning services for individuals([^<]*)</p>', '<p data-i18n="footer_description">Professional cleaning services for individuals. Vehicle washing, office and stairwell cleaning, boats, and more — done right, every time.</p>'
$c = $c -replace '<h4>Services</h4>', '<h4 data-i18n="footer_services">Services</h4>'
$c = $c -replace '<h4>Contact</h4>', '<h4 data-i18n="footer_contact">Contact</h4>'
$c = $c -replace '<a href="/public/services.html">All Services</a>', '<a href="/public/services.html" data-i18n="footer_all_services">All Services</a>'
$c = $c -replace '<p>&copy; 2025 Chistify\. All rights reserved\.</p>', '<p data-i18n="footer_copyright">&copy; 2025 Chistify. All rights reserved.</p>'
$c = $c -replace 'Staff Login', '<span data-i18n="nav_staff_login">Staff Login</span>'
Set-Content -Path $pubIndex -Value $c -Encoding UTF8
Write-Host "  Updated public/index.html with data-i18n"

# ======================================================================
# 4. Update all PORTAL HTML files
# ======================================================================
$portalFiles = @("index.html", "bookings.html", "booking-detail.html", "invoices.html", "invoice-detail.html", "profile.html", "login.html")

foreach ($file in $portalFiles) {
    $path = "$portal\$file"
    if (!(Test-Path $path)) { Write-Host "  Skipping $file (not found)"; continue }
    
    $c = Get-Content $path -Raw
    $old = $c
    
    # Add i18n.js script
    if ($c -notmatch "js/i18n.js") {
        $c = $c -replace '(</body>)', '<script src="/js/i18n.js"></script>$1'
    }
    
    # Add lang-switcher in portal header-inner
    if ($c -notmatch "lang-switcher" -and $c -match '<div class="header-inner">') {
        $c = $c -replace '(<div class="header-inner">)', '$1<div id="lang-switcher"><button class="lang-btn" data-lang="en">EN</button><button class="lang-btn" data-lang="hr">HR</button></div>'
    }
    
    # Add data-i18n to page title
    if ($c -match '<title>([^<]+)</title>') {
        $titleKey = switch -Regex ($Matches[1]) {
            'Dashboard.*Portal' { 'page_portal_dashboard' }
            'My Bookings' { 'page_portal_bookings' }
            'Booking Details' { 'page_portal_booking_detail' }
            'My Invoices' { 'page_portal_invoices' }
            'Invoice Details' { 'page_portal_invoice_detail' }
            'My Profile' { 'page_portal_profile' }
            'Client Login' { 'page_portal_login' }
        }
        if ($titleKey) {
            $c = $c -replace '<title>([^<]+)</title>', "<title data-i18n=""$titleKey"">`$1</title>"
        }
    }
    
    # Add data-i18n to logout button
    $c = $c -replace '<button id="logout-btn" class="logout-btn">Sign out</button>', '<button id="logout-btn" class="logout-btn" data-i18n="nav_sign_out">Sign out</button>'
    
    # Add data-i18n to h1
    $c = $c -replace '<h1>(?!<)\s*(Dashboard)\s*</h1>', '<h1 data-i18n="nav_dashboard">Dashboard</h1>'
    
    if ($c -ne $old) {
        Set-Content -Path $path -Value $c -Encoding UTF8
        Write-Host "  Updated portal/$file"
    }
}

# ======================================================================
# 4b. SPECIAL: Portal index.html headings
# ======================================================================
$portalIdx = "$portal\index.html"
$c = Get-Content $portalIdx -Raw
$c = $c -replace '<h1>Dashboard</h1>', '<h1 data-i18n="nav_dashboard">Dashboard</h1>'
$c = $c -replace '<h2 class="card-title">Upcoming Bookings</h2>', '<h2 class="card-title" data-i18n="section_upcoming_bookings">Upcoming Bookings</h2>'
$c = $c -replace '<h2 class="card-title">Recent Invoices</h2>', '<h2 class="card-title" data-i18n="section_recent_invoices">Recent Invoices</h2>'
$c = $c -replace 'class="user-pill-name">Loading...<', 'class="user-pill-name" data-i18n="ui_loading">Loading...<'
Set-Content -Path $portalIdx -Value $c -Encoding UTF8
Write-Host "  Updated portal/index.html with data-i18n"

# ======================================================================
# 4c. SPECIAL: Portal login page
# ======================================================================
$portalLogin = "$portal\login.html"
$c = Get-Content $portalLogin -Raw
$c = $c -replace '<h1 style="text-align:center;">Welcome back</h1>', '<h1 style="text-align:center;" data-i18n="section_welcome_back">Welcome back</h1>'
$c = $c -replace '<label for="email">Email address</label>', '<label for="email" data-i18n="label_email_address">Email address</label>'
$c = $c -replace '<button id="send-link-btn" class="btn" style="width:100%;justify-content:center;padding:0.7rem;">Send Magic Link</button>', '<button id="send-link-btn" class="btn" style="width:100%;justify-content:center;padding:0.7rem;" data-i18n="btn_send_magic_link">Send Magic Link</button>'
Set-Content -Path $portalLogin -Value $c -Encoding UTF8
Write-Host "  Updated portal/login.html with data-i18n"

# ======================================================================
# 5. Update nav JS files (admin-common.js, shared.js, portal-common.js)
# ======================================================================
function Update-NavFile {
    param($path, $navVarName)
    $c = Get-Content $path -Raw
    $old = $c
    
    # Replace nav items - add labelKey and use __()
    $c = $c -replace "(const $navVarName = \[)", 'window.i18nLabels = window.i18nLabels || {}; $1'
    
    # Add labelKey to each nav item: { label: 'X' -> { labelKey: 'nav_x', label: 'X'
    $c = $c -replace "(label:\s*')Daily View(')", "labelKey: 'nav_daily_view', label: `$1Daily View`$2"
    $c = $c -replace "(label:\s*')Calendar(')", "labelKey: 'nav_calendar', label: `$1Calendar`$2"
    $c = $c -replace "(label:\s*')Bookings(')", "labelKey: 'nav_bookings', label: `$1Bookings`$2"
    $c = $c -replace "(label:\s*')Requests(')", "labelKey: 'nav_requests', label: `$1Requests`$2"
    $c = $c -replace "(label:\s*')Recurring(')", "labelKey: 'nav_recurring', label: `$1Recurring`$2"
    $c = $c -replace "(label:\s*')Invoices(')", "labelKey: 'nav_invoices', label: `$1Invoices`$2"
    $c = $c -replace "(label:\s*')Client List(')", "labelKey: 'nav_client_list', label: `$1Client List`$2"
    $c = $c -replace "(label:\s*')Schedule(')", "labelKey: 'nav_schedule', label: `$1Schedule`$2"
    $c = $c -replace "(label:\s*')Services(')", "labelKey: 'nav_services', label: `$1Services`$2"
    $c = $c -replace "(label:\s*')SOPs(')", "labelKey: 'nav_sops', label: `$1SOPs`$2"
    $c = $c -replace "(label:\s*')Users(')", "labelKey: 'nav_users', label: `$1Users`$2"
    $c = $c -replace "(label:\s*')Roles(')", "labelKey: 'nav_roles', label: `$1Roles`$2"
    $c = $c -replace "(label:\s*')Reports(')", "labelKey: 'nav_reports', label: `$1Reports`$2"
    $c = $c -replace "(label:\s*')Dashboard(')", "labelKey: 'nav_dashboard', label: `$1Dashboard`$2"
    $c = $c -replace "(label:\s*')Profile(')", "labelKey: 'nav_profile', label: `$1Profile`$2"
    $c = $c -replace "(label:\s*')Services(')(?=\s*,|\s*])", "labelKey: 'nav_services', label: `$1Services`$2"
    $c = $c -replace "(label:\s*')Get a Quote(')", "labelKey: 'nav_get_quote', label: `$1Get a Quote`$2"
    $c = $c -replace "(label:\s*')Sign In(')", "labelKey: 'nav_sign_in', label: `$1Sign In`$2"
    
    # Update nav sections for admin
    $c = $c -replace "section:\s*'Operations'", "sectionKey: 'nav_section_operations', section: 'Operations'"
    $c = $c -replace "section:\s*'Bookings'", "sectionKey: 'nav_section_bookings', section: 'Bookings'"
    $c = $c -replace "section:\s*'Clients'", "sectionKey: 'nav_section_clients', section: 'Clients'"
    $c = $c -replace "section:\s*'Config'", "sectionKey: 'nav_section_config', section: 'Config'"
    $c = $c -replace "section:\s*'Admin'", "sectionKey: 'nav_section_admin', section: 'Admin'"
    
    if ($c -ne $old) {
        Set-Content -Path $path -Value $c -Encoding UTF8
        Write-Host "  Updated nav: $path"
    }
}

Update-NavFile "$admin\js\admin-common.js" "adminNav"
Update-NavFile "$public\js\shared.js" "publicNav"
Update-NavFile "$portal\js\portal-common.js" "portalNav"

# ======================================================================
# 5b. Update render functions to use __() for nav labels
# ======================================================================
$adminCommon = "$admin\js\admin-common.js"
$c = Get-Content $adminCommon -Raw
# Update section title rendering
$c = $c -replace "html \+= `<div class=\"nav-section\"><span class=\"nav-section-title\">\$\{section\.section\}</span>`;", "html += `<div class=\"nav-section\"><span class=\"nav-section-title\">\$\{(window.__(section.sectionKey) || section.section)\}</span>`;"
# Update nav item rendering
$c = $c -replace "html \+= `<a href=\"\$\{item\.href\}\" class=\"nav-item\$\{active\}\">\$\{item\.label\}</a>`;", "html += `<a href=\"\$\{item.href\}\" class=\"nav-item\$\{active\}\">\$\{(window.__(item.labelKey) || item.label)\}</a>`;"
Set-Content -Path $adminCommon -Value $c -Encoding UTF8
Write-Host "  Updated admin-common.js render with __()"

$sharedJs = "$public\js\shared.js"
$c = Get-Content $sharedJs -Raw
$c = $c -replace "html \+= `<a href=\"\$\{item\.href\}\" class=\"\$\{cls\}\">\$\{item\.label\}</a>`;", "html += `<a href=\"\$\{item.href\}\" class=\"\$\{cls\}\">\$\{(window.__(item.labelKey) || item.label)\}</a>`;"
Set-Content -Path $sharedJs -Value $c -Encoding UTF8
Write-Host "  Updated shared.js render with __()"

$portalCommon = "$portal\js\portal-common.js"
$c = Get-Content $portalCommon -Raw
$c = $c -replace "html \+= `<a href=\"\$\{item\.href\}\" class=\"nav-tab\$\{active\}\">\$\{item\.label\}</a>`;", "html += `<a href=\"\$\{item.href\}\" class=\"nav-tab\$\{active\}\">\$\{(window.__(item.labelKey) || item.label)\}</a>`;"
Set-Content -Path $portalCommon -Value $c -Encoding UTF8
Write-Host "  Updated portal-common.js render with __()"

# ======================================================================
# 6. Update admin JS files with translation calls
# ======================================================================
$adminJsPath = "$admin\js"
$adminJsFiles = Get-ChildItem "$adminJsPath\*.js" -Exclude "admin-common.js","admin-api.js","dashboard.js"

$replacements = @{
    "'No bookings found.'" = "__('empty_no_bookings_all')"
    "'No services found.'" = "__('empty_no_services')"
    "'No clients found.'" = "__('empty_no_clients')"
    "'No requests found.'" = "__('empty_no_requests')"
    "'No invoices found.'" = "__('empty_no_invoices')"
    "'No schedule entries found.'" = "__('empty_no_schedule')"
    "'No date overrides defined.'" = "__('empty_no_overrides')"
    "'No roles found.'" = "__('empty_no_roles')"
    "'No users found.'" = "__('empty_no_users')"
    "'No services assigned.'" = "__('empty_no_services_assigned')"
    "'No SOPs assigned.'" = "__('empty_no_sops_assigned')"
    "'No data available.'" = "__('empty_no_data')"
    "'No payments recorded.'" = "__('empty_no_payments')"
    "'No recurring schedules found.'" = "__('empty_no_recurring')"
    "'No templates found.'" = "__('empty_no_templates')"
    "'No bookings assigned.'" = "__('empty_no_bookings_assigned')"
    "'No contacts found.'" = "__('empty_no_contacts')"
    "'No sites found.'" = "__('empty_no_sites')"
    "'Failed to load bookings.'" = "__('msg_failed_bookings')"
    "'Failed to load slots.'" = "__('msg_failed_slots')"
    "'Status updated'" = "__('msg_status_updated')"
    "'Booking created'" = "__('msg_booking_created')"
    "'Changes saved.'" = "__('msg_changes_saved')"
    "'Service updated'" = "__('msg_service_updated')"
    "'Service created'" = "__('msg_service_created')"
    "'Service deleted'" = "__('msg_service_deleted')"
    "'Client updated'" = "__('msg_client_updated')"
    "'Site added'" = "__('msg_site_added')"
    "'Site deactivated'" = "__('msg_site_deactivated')"
    "'User created'" = "__('msg_user_created')"
    "'Password reset'" = "__('msg_password_reset')"
    "'Template created'" = "__('msg_template_created')"
    "'Template updated'" = "__('msg_template_updated')"
    "'Template deactivated'" = "__('msg_template_deactivated')"
    "'Item added'" = "__('msg_item_added')"
    "'Item removed'" = "__('msg_item_removed')"
    "'Schedule updated'" = "__('msg_schedule_updated')"
    "'Schedule added'" = "__('msg_schedule_added')"
    "'Schedule deleted'" = "__('msg_schedule_deleted')"
    "'Override saved'" = "__('msg_override_saved')"
    "'Override deleted'" = "__('msg_override_deleted')"
    "'Role updated'" = "__('msg_role_updated')"
    "'Role created'" = "__('msg_role_created')"
    "'Role deleted'" = "__('msg_role_deleted')"
    "'Assignment added'" = "__('msg_assignment_added')"
    "'Assignment removed'" = "__('msg_assignment_removed')"
    "'Service added'" = "__('msg_service_added_booking')"
    "'Service removed'" = "__('msg_service_removed_booking')"
    "'Price updated'" = "__('msg_price_updated')"
    "'Recurring schedule created'" = "__('msg_recurring_created')"
    "'Series ended'" = "__('msg_series_ended')"
    "'Export started'" = "__('msg_export_started')"
    "'Email sent to customer.'" = "__('msg_email_sent')"
    "'Request cancelled.'" = "__('msg_request_cancelled')"
    "'User status updated'" = "__('msg_status_updated_user')"
    "'Please select a client'" = "__('msg_select_client')"
    "'Select an employee'" = "__('msg_select_employee')"
    "'Select a service'" = "__('msg_select_service')"
    "'Please enter a booking ID'" = "__('msg_enter_booking_id')"
    "'Please enter a valid amount'" = "__('msg_enter_valid_amount')"
    "'Role name is required'" = "__('msg_role_name_required')"
    "'Item text required'" = "__('msg_item_text_required')"
    "'Delete this service?'" = "__('msg_confirm_delete_service')"
    "'Deactivate this SOP?'" = "__('msg_confirm_deactivate_sop')"
    "'Delete this day?'" = "__('msg_confirm_delete_schedule')"
    "'Delete this override?'" = "__('msg_confirm_delete_override')"
    "'Deactivate this site?'" = "__('msg_confirm_deactivate_site')"
    "'Remove this assignment?'" = "__('msg_confirm_remove_assignment')"
    "'Remove this service?'" = "__('msg_confirm_remove_service')"
    "'Remove this checklist item?'" = "__('msg_confirm_remove_item')"
    "'Delete this role?'" = "__('msg_confirm_delete_role')"
    "'Cancel this request?'" = "__('msg_confirm_cancel_request_title')"
}

foreach ($file in $adminJsFiles) {
    $c = Get-Content $file.FullName -Raw
    $old = $c
    foreach ($r in $replacements.Keys) {
        if ($c -match [regex]::Escape($r)) {
            $c = $c -replace [regex]::Escape($r), $replacements[$r]
        }
    }
    if ($c -ne $old) {
        Set-Content -Path $file.FullName -Value $c -Encoding UTF8
        Write-Host "  Updated admin/$($file.Name)"
    }
}

# ======================================================================
# 7. Update portal JS files
# ======================================================================
$portalJsPath = "$portal\js"
$portalJsFiles = Get-ChildItem "$portalJsPath\*.js" -Exclude "portal-common.js"

$portalReplacements = @{
    "'No upcoming bookings.'" = "__('empty_no_upcoming')"
    "'No invoices yet.'" = "__('empty_no_invoices_yet')"
    "'Failed to load dashboard'" = "__('msg_failed_dashboard')"
    "'Failed to load bookings. Please try again.'" = "__('msg_failed_bookings_load')"
    "'Failed to load invoices.'" = "__('msg_failed_invoices_load')"
    "'Failed to load profile'" = "__('msg_failed_profile')"
    "'Please enter your email address.'" = "__('msg_enter_email')"
    "'Something went wrong.'" = "__('msg_something_wrong')"
    "'Could not connect to server. Please try again.'" = "__('msg_could_not_connect')"
    "'Invalid booking ID.'" = "__('msg_invalid_booking_id')"
    "'Invalid invoice ID.'" = "__('msg_invalid_invoice_id')"
    "'Network error. Please try again.'" = "__('msg_network_error')"
    "'Please add at least one service.'" = "__('msg_add_service_first')"
    "'Please enter your full name.'" = "__('msg_enter_name')"
    "'Please enter your phone number.'" = "__('msg_enter_phone')"
    "'Request failed. Please try again.'" = "__('msg_request_failed')"
    "'Sending...'" = "__('msg_sending')"
    "'Send Magic Link'" = "__('btn_send_magic_link')"
}

foreach ($file in $portalJsFiles) {
    $c = Get-Content $file.FullName -Raw
    $old = $c
    foreach ($r in $portalReplacements.Keys) {
        if ($c -match [regex]::Escape($r)) {
            $c = $c -replace [regex]::Escape($r), $portalReplacements[$r]
        }
    }
    if ($c -ne $old) {
        Set-Content -Path $file.FullName -Value $c -Encoding UTF8
        Write-Host "  Updated portal/$($file.Name)"
    }
}

# ======================================================================
# 8. Update public JS files
# ======================================================================
$publicJsPath = "$public\js"
$publicJsFiles = Get-ChildItem "$publicJsPath\*.js" -Exclude "shared.js"

foreach ($file in $publicJsFiles) {
    $c = Get-Content $file.FullName -Raw
    $old = $c
    foreach ($r in $portalReplacements.Keys) {
        if ($c -match [regex]::Escape($r)) {
            $c = $c -replace [regex]::Escape($r), $portalReplacements[$r]
        }
    }
    if ($c -ne $old) {
        Set-Content -Path $file.FullName -Value $c -Encoding UTF8
        Write-Host "  Updated public/$($file.Name)"
    }
}

Write-Host "`nAll HTML/JS/CSS updates complete!"
