(function () {
  'use strict';

  var translations = {};
  var availableLangs = typeof window.__I18N_LANGS !== 'undefined' ? window.__I18N_LANGS : ['en'];
  var currentLang = localStorage.getItem('lang') || availableLangs[0];
  var ready = false;
  var _saved = {};

  // Synchronous init — use generated data if available
  if (typeof window.__I18N_DATA !== 'undefined' && currentLang === 'en') {
    translations = window.__I18N_DATA;
    ready = true;
  }
  var cached = localStorage.getItem('i18n_' + currentLang);
  if (cached) {
    try { translations = JSON.parse(cached); ready = true; } catch (e) {}
  }
  document.documentElement.lang = currentLang;

  function loadFromJson(lang) {
    fetch('/i18n/' + lang + '.json')
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
    else if (typeof window.__I18N_DATA !== 'undefined' && lang === 'en') { translations = window.__I18N_DATA; ready = true; }
    else { translations = {}; ready = true; }
    document.documentElement.lang = lang;
    processDOM();
    window.dispatchEvent(new CustomEvent('i18nReady'));
    loadFromJson(lang);
  }

  function __(key) {
    if (!key) return '';
    if (!ready) return null;
    return translations[key] || key;
  }

  function __status(s) {
    if (!s) return '';
    var key = typeof window.__STATUS_MAP !== 'undefined' ? window.__STATUS_MAP[s] : null;
    if (key) return __(key);
    return s;
  }

  function __error(code, fallback) {
    if (code && typeof window.ERROR_CODE_MAP !== 'undefined' && window.ERROR_CODE_MAP[code]) {
      var key = window.ERROR_CODE_MAP[code];
      var t = __(key);
      if (t !== key) return t;
    }
    return fallback || 'Request failed.';
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
    if (btns.length === 0 && typeof window.__I18N_LANGS !== 'undefined') {
      for (var i = 0; i < window.__I18N_LANGS.length; i++) {
        var code = window.__I18N_LANGS[i];
        var btn = document.createElement('button');
        btn.className = 'lang-btn';
        btn.setAttribute('data-lang', code);
        btn.textContent = __('lang_' + code) || code.toUpperCase();
        btn.addEventListener('click', function () {
          var lang = this.getAttribute('data-lang');
          var all = switcher.querySelectorAll('.lang-btn');
          for (var j = 0; j < all.length; j++) all[j].classList.remove('active');
          this.classList.add('active');
          setLanguage(lang);
        });
        switcher.appendChild(btn);
      }
      btns = switcher.querySelectorAll('.lang-btn');
    }
    for (var i = 0; i < btns.length; i++) {
      if (btns[i].getAttribute('data-lang') === currentLang) btns[i].classList.add('active');
    }
  }

  var browserLang = (navigator.language || navigator.userLanguage || '').substring(0, 2);
  if (availableLangs.indexOf(browserLang) >= 0) {
    if (!localStorage.getItem('lang')) currentLang = browserLang;
  }

  document.addEventListener('DOMContentLoaded', function () {
    initLanguageSwitcher();
    processDOM();
    loadFromJson(currentLang);
  });

  window.__ = __;
  window.__status = __status;
  window.__error = __error;
  window.setLanguage = setLanguage;
  window.i18nReady = false;
  window.addEventListener('i18nReady', function () { window.i18nReady = true; });
})();
