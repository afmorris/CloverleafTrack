/**
 * Client-side search backed by /search-index.json.
 * Index is loaded lazily on first focus of #site-search.
 */
(function () {
    'use strict';

    var input = document.getElementById('site-search');
    var resultsContainer = document.getElementById('search-results');
    if (!input || !resultsContainer) return;

    var index = null;
    var loading = false;
    var focusedIdx = -1;

    function loadIndex() {
        if (index !== null || loading) return Promise.resolve();
        loading = true;
        return fetch('/search-index.json')
            .then(function (r) { return r.json(); })
            .then(function (data) { index = data; })
            .catch(function () { index = []; })
            .finally(function () { loading = false; });
    }

    function matches(record, query) {
        if (!query) return false;
        var q = query.toLowerCase().trim();
        var haystack = (record.label + ' ' + record.subLabel).toLowerCase();
        return haystack.includes(q);
    }

    function escapeHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function renderResults(query) {
        if (!index || !query.trim()) { hideResults(); return; }

        var MAX = 5;
        var groups = { athlete: [], meet: [], event: [] };
        var groupLabels = { athlete: 'Athletes', meet: 'Meets', event: 'Events' };

        index.forEach(function (record) {
            var g = groups[record.type];
            if (g && g.length < MAX && matches(record, query)) g.push(record);
        });

        var total = groups.athlete.length + groups.meet.length + groups.event.length;
        if (total === 0) {
            resultsContainer.innerHTML = '<div class="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">No results found</div>';
            resultsContainer.classList.remove('hidden');
            return;
        }

        var html = '';
        var idx = 0;
        ['athlete', 'meet', 'event'].forEach(function (type) {
            if (!groups[type].length) return;
            html += '<div class="px-3 py-1 text-xs font-semibold text-gray-400 uppercase tracking-wide bg-gray-50 dark:bg-gray-700/50">' + groupLabels[type] + '</div>';
            groups[type].forEach(function (r) {
                html += '<a href="' + escapeHtml(r.url) + '" role="option" data-idx="' + (idx++) + '" ' +
                    'class="search-result flex flex-col px-4 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 border-b border-gray-100 dark:border-gray-700/50 last:border-0 focus:outline-none focus:bg-gray-100 dark:focus:bg-gray-700" ' +
                    'tabindex="-1">' +
                    '<span class="font-medium text-gray-900 dark:text-white text-sm">' + escapeHtml(r.label) + '</span>' +
                    '<span class="text-xs text-gray-500 dark:text-gray-400">' + escapeHtml(r.subLabel) + '</span>' +
                    '</a>';
            });
        });

        resultsContainer.innerHTML = html;
        resultsContainer.classList.remove('hidden');
    }

    function hideResults() {
        resultsContainer.classList.add('hidden');
        resultsContainer.innerHTML = '';
        focusedIdx = -1;
    }

    function getItems() {
        return Array.from(resultsContainer.querySelectorAll('.search-result'));
    }

    input.addEventListener('focus', function () {
        focusedIdx = -1;
        loadIndex().then(function () { renderResults(input.value); });
    });

    input.addEventListener('input', function () {
        focusedIdx = -1;
        renderResults(input.value);
    });

    input.addEventListener('keydown', function (e) {
        var items = getItems();
        if (e.key === 'Escape') { hideResults(); input.blur(); return; }
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            focusedIdx = Math.min(focusedIdx + 1, items.length - 1);
            if (items[focusedIdx]) items[focusedIdx].focus();
            return;
        }
        if (e.key === 'ArrowUp') {
            e.preventDefault();
            focusedIdx = Math.max(focusedIdx - 1, -1);
            if (focusedIdx === -1) input.focus();
            else if (items[focusedIdx]) items[focusedIdx].focus();
            return;
        }
        if (e.key === 'Enter' && items.length > 0 && focusedIdx >= 0) {
            e.preventDefault();
            items[focusedIdx].click();
        }
    });

    resultsContainer.addEventListener('keydown', function (e) {
        var items = getItems();
        if (e.key === 'Escape') { hideResults(); input.focus(); return; }
        if (e.key === 'ArrowDown') { e.preventDefault(); focusedIdx = Math.min(focusedIdx + 1, items.length - 1); if (items[focusedIdx]) items[focusedIdx].focus(); }
        if (e.key === 'ArrowUp') { e.preventDefault(); focusedIdx = Math.max(focusedIdx - 1, -1); if (focusedIdx === -1) input.focus(); else if (items[focusedIdx]) items[focusedIdx].focus(); }
    });

    document.addEventListener('click', function (e) {
        if (!e.target.closest('#search-container')) hideResults();
    });

    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            input.focus();
            input.select();
        }
    });
}());
