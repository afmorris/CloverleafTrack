/**
 * Client-side search backed by /search-index.json.
 * Desktop: inline input + dropdown panel.
 * Mobile:  icon button opens a full-screen overlay.
 */
(function () {
    'use strict';

    // ── shared state ─────────────────────────────────────────────────────────
    var index   = null;
    var loading = false;

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
        return (record.label + ' ' + record.subLabel).toLowerCase().includes(q);
    }

    function escapeHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;').replace(/</g, '&lt;')
            .replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    function buildResultsHtml(query) {
        if (!index || !query.trim()) return null;
        var MAX    = 5;
        var groups = { athlete: [], meet: [], event: [] };
        var labels = { athlete: 'Athletes', meet: 'Meets', event: 'Events' };
        index.forEach(function (r) {
            var g = groups[r.type];
            if (g && g.length < MAX && matches(r, query)) g.push(r);
        });
        var total = groups.athlete.length + groups.meet.length + groups.event.length;
        if (total === 0) {
            return '<div class="px-4 py-3 text-sm text-gray-500 dark:text-gray-400">No results found</div>';
        }
        var html = '';
        var idx  = 0;
        ['athlete', 'meet', 'event'].forEach(function (type) {
            if (!groups[type].length) return;
            html += '<div class="px-3 py-1 text-xs font-semibold text-gray-400 uppercase tracking-wide bg-gray-50 dark:bg-gray-700/50">' + labels[type] + '</div>';
            groups[type].forEach(function (r) {
                html += '<a href="' + escapeHtml(r.url) + '" role="option" data-idx="' + (idx++) + '" ' +
                    'class="search-result flex flex-col px-4 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 border-b border-gray-100 dark:border-gray-700/50 last:border-0 focus:outline-none focus:bg-gray-100 dark:focus:bg-gray-700" ' +
                    'tabindex="-1">' +
                    '<span class="font-medium text-gray-900 dark:text-white text-sm">' + escapeHtml(r.label) + '</span>' +
                    '<span class="text-xs text-gray-500 dark:text-gray-400">' + escapeHtml(r.subLabel) + '</span>' +
                    '</a>';
            });
        });
        return html;
    }

    // ── Desktop search ────────────────────────────────────────────────────────
    var desktopInput  = document.getElementById('site-search');
    var desktopPanel  = document.getElementById('search-results');

    if (desktopInput && desktopPanel) {
        var focusedIdx = -1;

        function showDesktopResults(query) {
            var html = buildResultsHtml(query);
            if (html === null) { hideDesktop(); return; }
            desktopPanel.innerHTML = html;
            desktopPanel.classList.remove('hidden');
            focusedIdx = -1;
        }

        function hideDesktop() {
            desktopPanel.classList.add('hidden');
            desktopPanel.innerHTML = '';
            focusedIdx = -1;
        }

        function getDesktopItems() {
            return Array.from(desktopPanel.querySelectorAll('.search-result'));
        }

        desktopInput.addEventListener('focus', function () {
            loadIndex().then(function () { showDesktopResults(desktopInput.value); });
        });
        desktopInput.addEventListener('input', function () {
            showDesktopResults(desktopInput.value);
        });
        desktopInput.addEventListener('keydown', function (e) {
            var items = getDesktopItems();
            if (e.key === 'Escape') { hideDesktop(); desktopInput.blur(); return; }
            if (e.key === 'ArrowDown') { e.preventDefault(); focusedIdx = Math.min(focusedIdx + 1, items.length - 1); if (items[focusedIdx]) items[focusedIdx].focus(); return; }
            if (e.key === 'ArrowUp')   { e.preventDefault(); focusedIdx = Math.max(focusedIdx - 1, -1); focusedIdx === -1 ? desktopInput.focus() : items[focusedIdx] && items[focusedIdx].focus(); return; }
            if (e.key === 'Enter' && items.length > 0 && focusedIdx >= 0) { e.preventDefault(); items[focusedIdx].click(); }
        });
        desktopPanel.addEventListener('keydown', function (e) {
            var items = getDesktopItems();
            if (e.key === 'Escape') { hideDesktop(); desktopInput.focus(); return; }
            if (e.key === 'ArrowDown') { e.preventDefault(); focusedIdx = Math.min(focusedIdx + 1, items.length - 1); items[focusedIdx] && items[focusedIdx].focus(); }
            if (e.key === 'ArrowUp')   { e.preventDefault(); focusedIdx = Math.max(focusedIdx - 1, -1); focusedIdx === -1 ? desktopInput.focus() : items[focusedIdx] && items[focusedIdx].focus(); }
        });
        document.addEventListener('click', function (e) {
            if (!e.target.closest('#search-container')) hideDesktop();
        });
    }

    // ── Mobile overlay ────────────────────────────────────────────────────────
    var mobileBtn     = document.getElementById('search-mobile-btn');
    var overlay       = document.getElementById('search-overlay');
    var overlayInput  = document.getElementById('search-overlay-input');
    var overlayPanel  = document.getElementById('search-overlay-results');
    var overlayClose  = document.getElementById('search-overlay-close');

    if (mobileBtn && overlay && overlayInput && overlayPanel && overlayClose) {
        var ovFocused = -1;

        function openOverlay() {
            overlay.classList.remove('hidden');
            overlay.classList.add('flex');
            document.body.style.overflow = 'hidden';
            overlayInput.value = '';
            overlayPanel.innerHTML = '';
            overlayInput.focus();
            loadIndex();
        }

        function closeOverlay() {
            overlay.classList.add('hidden');
            overlay.classList.remove('flex');
            document.body.style.overflow = '';
            mobileBtn.focus();
        }

        function showOverlayResults(query) {
            var html = buildResultsHtml(query);
            overlayPanel.innerHTML = html ?? '';
            ovFocused = -1;
        }

        function getOverlayItems() {
            return Array.from(overlayPanel.querySelectorAll('.search-result'));
        }

        mobileBtn.addEventListener('click', openOverlay);
        overlayClose.addEventListener('click', closeOverlay);

        overlayInput.addEventListener('input', function () {
            showOverlayResults(overlayInput.value);
        });

        overlayInput.addEventListener('keydown', function (e) {
            var items = getOverlayItems();
            if (e.key === 'Escape') { closeOverlay(); return; }
            if (e.key === 'ArrowDown') { e.preventDefault(); ovFocused = Math.min(ovFocused + 1, items.length - 1); items[ovFocused] && items[ovFocused].focus(); return; }
            if (e.key === 'ArrowUp')   { e.preventDefault(); ovFocused = Math.max(ovFocused - 1, -1); ovFocused === -1 ? overlayInput.focus() : items[ovFocused] && items[ovFocused].focus(); return; }
            if (e.key === 'Enter' && items.length > 0 && ovFocused >= 0) { e.preventDefault(); items[ovFocused].click(); }
        });

        overlayPanel.addEventListener('keydown', function (e) {
            var items = getOverlayItems();
            if (e.key === 'Escape') { closeOverlay(); return; }
            if (e.key === 'ArrowDown') { e.preventDefault(); ovFocused = Math.min(ovFocused + 1, items.length - 1); items[ovFocused] && items[ovFocused].focus(); }
            if (e.key === 'ArrowUp')   { e.preventDefault(); ovFocused = Math.max(ovFocused - 1, -1); ovFocused === -1 ? overlayInput.focus() : items[ovFocused] && items[ovFocused].focus(); }
        });
    }

    // ── Global ⌘K shortcut ────────────────────────────────────────────────────
    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            // Prefer overlay on mobile viewport, desktop input otherwise
            if (mobileBtn && overlay && window.innerWidth < 640) {
                openOverlay && openOverlay();
            } else if (desktopInput) {
                desktopInput.focus();
                desktopInput.select();
            }
        }
    });

    function openOverlay() {
        if (!overlay) return;
        overlay.classList.remove('hidden');
        overlay.classList.add('flex');
        document.body.style.overflow = 'hidden';
        overlayInput && (overlayInput.value = '');
        overlayPanel && (overlayPanel.innerHTML = '');
        overlayInput && overlayInput.focus();
        loadIndex();
    }
}());
