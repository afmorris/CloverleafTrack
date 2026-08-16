/**
 * Sortable tables — shared client-side column sorting for <table data-sortable>.
 *
 * Extracted from the old inline sort script on Leaderboard/Details.cshtml so every
 * table sitewide (Roster, Leaderboard index, Meet results, Leaderboard event pages)
 * can opt in with markup only.
 *
 * Markup contract:
 *   <table data-sortable>
 *     <thead>
 *       <tr>
 *         <th data-sort-col="mark" data-sort-type="num" data-sort-dir="asc">Mark</th>
 *         ...
 *       </tr>
 *     </thead>
 *     <tbody>
 *       <tr>
 *         <td data-sort="71.10">1:11.10</td>
 *         ...
 *       </tr>
 *     </tbody>
 *   </table>
 *
 * - data-sort-col:  stable identifier for the column. Also used as the URL hash key
 *                    (see below), so keep it short and unique within the page.
 * - data-sort-type: "num" (parseFloat) or "str" (localeCompare). Use "str" for ISO
 *                    yyyy-MM-dd date strings too — they already sort correctly as text.
 * - data-sort-dir:  "asc" | "desc" — which raw-value direction counts as "best" for
 *                    this column (e.g. "asc" for times, "desc" for distances). Defaults
 *                    to "asc" when omitted.
 * - Each <th data-sort-col> is automatically wrapped in a real <button> (keyboard
 *   support for free) plus a fixed-width, aria-hidden caret — do not hand-author the
 *   button/caret markup, just the data-* attributes and the header label text.
 * - data-sort="value" on the matching <td> supplies the raw sort key by column
 *   position (index-matched against the header row). Falls back to the cell's
 *   trimmed text content when omitted. NEVER put a formatted display string (e.g.
 *   "1:11.10", "Sophomore", "Aug 3, 2026") in data-sort — use the raw comparable
 *   value instead (raw seconds, a class ordinal, an ISO date).
 * - A <tr data-sort-group-header> is a section-divider row (e.g. a category label
 *   row spanning the table). It is excluded from sorting and hidden while any sort
 *   is active, then restored together with the rest of the original order on the
 *   third click (back to "no sort").
 * - .rank-cell (optionally wrapping a nested element such as a <span>) is renumbered
 *   1..n after every sort, counting only currently visible rows (a row hidden by
 *   filters.js via the `hidden` property, or by page-specific filtering via
 *   `style.display = 'none'`, is skipped). This module never sets a data row's own
 *   hidden/display state itself — that stays owned by filters.js / page filters;
 *   only row order (and group-header visibility, which is this module's own
 *   bookkeeping, not a data row) changes here.
 *
 * Click cycle per column: best-first -> worst-first -> original DOM order -> repeat.
 *
 * Sort state is persisted in the URL hash as #sort=<col>&dir=<asc|desc>, merged into
 * the hash the same way filters.js merges its own keys (read-modify-write, other
 * keys such as #env=outdoor or #gender=boys are left untouched).
 */
(function () {
    'use strict';

    function getHashParams() {
        var params = {};
        if (!location.hash || location.hash === '#') return params;
        location.hash.slice(1).split('&').forEach(function (pair) {
            var parts = pair.split('=');
            if (parts.length === 2) params[decodeURIComponent(parts[0])] = decodeURIComponent(parts[1]);
        });
        return params;
    }

    function setHashParams(updates) {
        var params = getHashParams();
        Object.keys(updates).forEach(function (key) {
            if (updates[key] === null) delete params[key];
            else params[key] = updates[key];
        });
        var hash = Object.keys(params).map(function (key) {
            return encodeURIComponent(key) + '=' + encodeURIComponent(params[key]);
        }).join('&');
        history.replaceState(null, '', hash ? '#' + hash : location.pathname + location.search);
    }

    function isRowVisible(row) {
        return !row.hidden && row.style.display !== 'none';
    }

    function renumberRankCells(tbody) {
        var n = 0;
        Array.prototype.forEach.call(tbody.children, function (row) {
            if (row.tagName !== 'TR' || !isRowVisible(row)) return;
            var cell = row.querySelector('.rank-cell');
            if (!cell) return;
            n++;
            var target = cell.querySelector('span') || cell;
            target.textContent = n;
        });
    }

    // Wraps each sortable <th>'s existing content in a real <button>, appends a
    // fixed-width aria-hidden caret, and returns the list of sortable column entries.
    function buildHeader(table) {
        var headerRow = table.querySelector('thead tr');
        if (!headerRow) return [];

        var entries = [];

        Array.prototype.forEach.call(headerRow.children, function (th, index) {
            if (!th.hasAttribute('data-sort-col')) return;

            var label = th.textContent.trim();

            th.setAttribute('scope', th.getAttribute('scope') || 'col');
            th.setAttribute('aria-sort', 'none');

            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'sort-th-btn';
            btn.setAttribute('aria-label', 'Sort by ' + label);

            while (th.firstChild) btn.appendChild(th.firstChild);

            var caret = document.createElement('span');
            caret.className = 'sort-caret';
            caret.setAttribute('aria-hidden', 'true');
            caret.textContent = '↕'; // neutral indicator; reserves width so no layout shift on sort
            btn.appendChild(caret);

            th.appendChild(btn);

            entries.push({ th: th, btn: btn, caret: caret, index: index, label: label });
        });

        return entries;
    }

    function dataRows(tbody) {
        return Array.prototype.filter.call(tbody.children, function (row) {
            return row.tagName === 'TR' && !row.hasAttribute('data-sort-group-header');
        });
    }

    function cellValue(row, index) {
        var cell = row.children[index];
        if (!cell) return '';
        return cell.hasAttribute('data-sort') ? cell.getAttribute('data-sort') : cell.textContent.trim();
    }

    function compare(va, vb, type, ascending) {
        if (type === 'num') {
            var na = parseFloat(va);
            var nb = parseFloat(vb);
            var aMissing = va === '' || isNaN(na);
            var bMissing = vb === '' || isNaN(nb);
            if (aMissing && bMissing) return 0;
            if (aMissing) return 1;  // missing values always sort last, regardless of direction
            if (bMissing) return -1;
            return ascending ? na - nb : nb - na;
        }
        var cmp = String(va).localeCompare(String(vb));
        return ascending ? cmp : -cmp;
    }

    function initTable(table) {
        var tbody = table.querySelector('tbody');
        if (!tbody) return;

        var sortables = buildHeader(table);
        if (!sortables.length) return;

        var originalOrder = Array.prototype.slice.call(tbody.children);
        var groupHeaderRows = originalOrder.filter(function (row) {
            return row.hasAttribute && row.hasAttribute('data-sort-group-header');
        });

        var liveRegion = document.createElement('div');
        liveRegion.className = 'sr-only';
        liveRegion.setAttribute('aria-live', 'polite');
        liveRegion.setAttribute('role', 'status');
        table.insertAdjacentElement('afterend', liveRegion);

        var state = { col: null, mode: 'none' }; // mode: 'none' | 'best' | 'worst'

        function announce(message) {
            liveRegion.textContent = message;
        }

        function applyAriaSort(activeEntry, actualDir) {
            sortables.forEach(function (entry) {
                if (entry === activeEntry && actualDir) {
                    entry.th.setAttribute('aria-sort', actualDir === 'asc' ? 'ascending' : 'descending');
                    entry.caret.textContent = actualDir === 'asc' ? '↑' : '↓';
                } else {
                    entry.th.setAttribute('aria-sort', 'none');
                    entry.caret.textContent = '↕';
                }
            });
        }

        function applySort(entry, mode, writeHash) {
            state.col = entry;
            state.mode = mode;

            if (mode === 'none') {
                originalOrder.forEach(function (row) { tbody.appendChild(row); });
                groupHeaderRows.forEach(function (row) { row.hidden = false; });
                applyAriaSort(null, null);
                renumberRankCells(tbody);
                if (writeHash) setHashParams({ sort: null, dir: null });
                announce('Sort cleared, showing original order');
                return;
            }

            var bestDir = entry.th.getAttribute('data-sort-dir') === 'desc' ? 'desc' : 'asc';
            var actualDir = mode === 'best' ? bestDir : (bestDir === 'asc' ? 'desc' : 'asc');
            var ascending = actualDir === 'asc';
            var type = entry.th.getAttribute('data-sort-type') === 'num' ? 'num' : 'str';
            var colIndex = entry.index;

            // Group-header/divider rows don't participate in a flat sort — hide them
            // while sorted, restore them (and the whole original order) on reset.
            groupHeaderRows.forEach(function (row) { row.hidden = true; });

            var rows = dataRows(tbody);
            rows.sort(function (a, b) {
                return compare(cellValue(a, colIndex), cellValue(b, colIndex), type, ascending);
            });
            rows.forEach(function (row) { tbody.appendChild(row); });

            applyAriaSort(entry, actualDir);
            renumberRankCells(tbody);
            if (writeHash) setHashParams({ sort: entry.th.getAttribute('data-sort-col'), dir: actualDir });
            announce('Sorted by ' + entry.label + ', ' + (mode === 'best' ? 'best first' : 'worst first'));
        }

        sortables.forEach(function (entry) {
            entry.btn.addEventListener('click', function () {
                var nextMode;
                if (state.col !== entry) nextMode = 'best';
                else if (state.mode === 'best') nextMode = 'worst';
                else if (state.mode === 'worst') nextMode = 'none';
                else nextMode = 'best';
                applySort(entry, nextMode, true);
            });
        });

        // Applies a sort described by the URL hash (#sort=col&dir=asc|desc) without
        // going through the click cycle. No-op if this table has no matching column.
        table._sortableApplyFromHash = function (col, dir) {
            var entry = null;
            for (var i = 0; i < sortables.length; i++) {
                if (sortables[i].th.getAttribute('data-sort-col') === col) { entry = sortables[i]; break; }
            }
            if (!entry) return;
            var bestDir = entry.th.getAttribute('data-sort-dir') === 'desc' ? 'desc' : 'asc';
            var mode = dir === bestDir ? 'best' : 'worst';
            applySort(entry, mode, false);
        };
    }

    function initAll() {
        var tables = Array.prototype.slice.call(document.querySelectorAll('table[data-sortable]'));
        tables.forEach(initTable);

        var hashParams = getHashParams();
        if (hashParams.sort && hashParams.dir) {
            tables.forEach(function (table) {
                if (typeof table._sortableApplyFromHash === 'function') {
                    table._sortableApplyFromHash(hashParams.sort, hashParams.dir);
                }
            });
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initAll);
    } else {
        initAll();
    }
}());
