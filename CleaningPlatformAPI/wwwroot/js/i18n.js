(function () {
  'use strict';

  var translations = {};
  var currentLang = localStorage.getItem('lang') || 'en';
  var ready = false;
  var _saved = {};

  var fallback = {
    'nav_daily_view': 'Daily View',
    'nav_calendar': 'Calendar',
    'nav_bookings': 'Bookings',
    'nav_requests': 'Requests',
    'nav_recurring': 'Recurring',
    'nav_invoices': 'Invoices',
    'nav_clients': 'Clients',
    'nav_schedule': 'Schedule',
    'nav_services': 'Services',
    'nav_sops': 'SOPs',
    'nav_users': 'Users',
    'nav_roles': 'Roles',
    'nav_reports': 'Reports',
    'nav_dashboard': 'Dashboard',
    'nav_profile': 'Profile',
    'nav_sign_in': 'Sign In',
    'nav_sign_out': 'Sign Out',
    'btn_save': 'Save',
    'btn_cancel': 'Cancel',
    'btn_edit': 'Edit',
    'btn_delete': 'Delete',
    'btn_add': 'Add',
    'btn_create': 'Create',
    'btn_filter': 'Filter',
    'btn_search': 'Search',
    'btn_generate': 'Generate',
    'th_id': 'ID',
    'th_status': 'Status',
    'th_date': 'Date',
    'th_name': 'Name',
    'th_client': 'Client',
    'th_total': 'Total',
    'th_actions': 'Actions',
    'status_pending': 'Pending',
    'status_confirmed': 'Confirmed',
    'status_in_progress': 'In Progress',
    'status_completed': 'Completed',
    'status_cancelled': 'Cancelled',
    'status_paid': 'Paid',
    'status_unpaid': 'Unpaid',
    'status_active': 'Active',
    'status_inactive': 'Inactive',
    'ui_loading': 'Loading...',
    'ui_yes': 'Yes',
    'ui_no': 'No',
    'ui_all': 'All',
    'ui_previous': 'Previous',
    'ui_next': 'Next',
    'ui_page': 'Page',
    'ui_of': 'of',
    'ui_total': 'Total',
    'lang_en': 'EN',
    'lang_hr': 'HR',
  };

  // Synchronous init — always has something to show immediately
  var cached = localStorage.getItem('i18n_' + currentLang);
  if (cached) {
    try { translations = JSON.parse(cached); ready = true; } catch (e) {}
  }
  if (!ready) {
    translations = fallback;
    ready = true;
  }
  document.documentElement.lang = currentLang;

  function loadFromApi(lang) {
    fetch('/api/localization?culture=' + lang)
      .then(function (r) { return r.json(); })
      .then(function (data) {
        if (data && Object.keys(data).length > 0) {
          translations = data;
          localStorage.setItem('i18n_' + lang, JSON.stringify(data));
        }
        ready = true;
        document.documentElement.lang = lang;
        processDOM();
        window.dispatchEvent(new CustomEvent('i18nReady'));
      })
      .catch(function () {
        ready = true;
        processDOM();
        window.dispatchEvent(new CustomEvent('i18nReady'));
      });
  }

  function setLanguage(lang) {
    currentLang = lang;
    localStorage.setItem('lang', lang);
    document.cookie = 'lang=' + lang + ';path=/';
    var c = localStorage.getItem('i18n_' + lang);
    if (c) { try { translations = JSON.parse(c); ready = true; } catch (e) {} }
    else { translations = fallback; ready = true; }
    document.documentElement.lang = lang;
    processDOM();
    window.dispatchEvent(new CustomEvent('i18nReady'));
    loadFromApi(lang);
  }

  function __(key) {
    if (!key) return '';
    if (!ready) return null;
    return translations[key] || key;
  }

  function __status(s) {
    if (!s) return '';
    var key = 'status_' + s.replace(/([A-Z])/g, '_$1').toLowerCase().replace(/^_/, '');
    return __(key) || s;
  }

  function processDOM() {
    var els = document.querySelectorAll('[data-i18n]');
    for (var i = 0; i < els.length; i++) {
      var el = els[i];
      var key = el.getAttribute('data-i18n');
      var attr = el.getAttribute('data-i18n-attr');
      if (attr) {
        var orig = el.getAttribute(attr);
        if (!_saved[key + '@' + attr]) _saved[key + '@' + attr] = orig;
        el.setAttribute(attr, __(key));
      } else {
        if (!_saved[key]) _saved[key] = el.innerHTML;
        el.innerHTML = __(key);
      }
    }
  }

  function restoreDOM() {
    var els = document.querySelectorAll('[data-i18n]');
    for (var i = 0; i < els.length; i++) {
      var el = els[i];
      var key = el.getAttribute('data-i18n');
      var attr = el.getAttribute('data-i18n-attr');
      if (attr) {
        var saved = _saved[key + '@' + attr];
        if (saved !== undefined) el.setAttribute(attr, saved);
      } else {
        if (_saved[key] !== undefined) el.innerHTML = _saved[key];
      }
    }
  }

  function initLanguageSwitcher() {
    var switcher = document.getElementById('lang-switcher');
    if (!switcher) return;
    var btns = switcher.querySelectorAll('.lang-btn');
    for (var i = 0; i < btns.length; i++) {
      btns[i].addEventListener('click', function () {
        var lang = this.getAttribute('data-lang');
        for (var j = 0; j < btns.length; j++) btns[j].classList.remove('active');
        this.classList.add('active');
        setLanguage(lang);
      });
    }
    for (var i = 0; i < btns.length; i++) {
      if (btns[i].getAttribute('data-lang') === currentLang) btns[i].classList.add('active');
    }
  }

  var browserLang = (navigator.language || navigator.userLanguage || '').substring(0, 2);
  if (['hr'].indexOf(browserLang) >= 0) {
    if (!localStorage.getItem('lang')) currentLang = browserLang;
  }

  document.addEventListener('DOMContentLoaded', function () {
    initLanguageSwitcher();
    processDOM();
    loadFromApi(currentLang);
  });

  window.__ = __;
  window.__status = __status;
  window.setLanguage = setLanguage;
  window.i18nReady = false;
  window.addEventListener('i18nReady', function () { window.i18nReady = true; });
})();
