/**
 * Filter chip system for Roster, Leaderboard, and Meets pages.
 *
 * Mark filterable items with data-filterable and per-key data attributes:
 *   <div data-filterable data-gender="boys" data-categories="sprints,distance">
 *
 * Group sections that should hide when empty:
 *   <div data-filterable-section>
 *
 * URL hash persistence: #gender=boys&categories=sprints
 */
(function () {
    'use strict';

    function getHashFilters() {
        var filters = {};
        if (!location.hash || location.hash === '#') return filters;
        location.hash.slice(1).split('&').forEach(function (pair) {
            var parts = pair.split('=');
            if (parts.length === 2) filters[decodeURIComponent(parts[0])] = decodeURIComponent(parts[1]);
        });
        return filters;
    }

    function setHashFilter(key, value) {
        var filters = getHashFilters();
        if (value === 'all') delete filters[key];
        else filters[key] = value;
        var hash = Object.keys(filters).map(function (k) {
            return encodeURIComponent(k) + '=' + encodeURIComponent(filters[k]);
        }).join('&');
        history.replaceState(null, '', hash ? '#' + hash : location.pathname + location.search);
    }

    function applyFilters() {
        var filters = getHashFilters();

        document.querySelectorAll('.filter-group').forEach(function (group) {
            var key = group.dataset.filterKey;
            var activeValue = filters[key] || 'all';
            group.querySelectorAll('button[data-value]').forEach(function (btn) {
                var isActive = btn.dataset.value === activeValue;
                btn.setAttribute('aria-pressed', isActive ? 'true' : 'false');
                if (isActive) {
                    btn.classList.remove('filter-chip-inactive');
                    btn.classList.add('filter-chip-active');
                } else {
                    btn.classList.remove('filter-chip-active');
                    btn.classList.add('filter-chip-inactive');
                }
            });
        });

        document.querySelectorAll('[data-filterable]').forEach(function (item) {
            var visible = true;
            Object.keys(filters).forEach(function (key) {
                var value = filters[key];
                if (value === 'all') return;
                var itemVal = (item.dataset[key] || '').toLowerCase();
                if (!itemVal) return; // no data attribute for this key = matches everything
                var vals = itemVal.split(',').map(function (v) { return v.trim(); });
                if (!vals.includes(value.toLowerCase())) visible = false;
            });
            item.hidden = !visible;
        });

        document.querySelectorAll('[data-filterable-section]').forEach(function (section) {
            var hasVisible = section.querySelectorAll('[data-filterable]:not([hidden])').length > 0;
            section.hidden = !hasVisible;
        });
    }

    function initFilterChips() {
        document.querySelectorAll('.filter-group button[data-value]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var group = this.closest('.filter-group');
                var key = group.dataset.filterKey;
                setHashFilter(key, this.dataset.value);
                applyFilters();
            });
        });

        window.addEventListener('hashchange', applyFilters);
        applyFilters();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initFilterChips);
    } else {
        initFilterChips();
    }
}());
