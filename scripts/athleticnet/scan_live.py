#!/usr/bin/env python3
"""Headless scan of MileSplit Live (https://milesplit.live) for Cloverleaf rows: every event of a meet, all rounds (finals, prelims, semis).
Writes scripts/athleticnet/live/<msMeetId>.txt in the format convert_live.py reads:
    E|<eventId>|<title>|<round>|<header columns joined by ¦>      then one line per Cloverleaf row (cells joined by |)
usage: scan_live.py [--out DIR] [--force] [--parallel N] msMeetId [...]
Needs playwright + its chromium (scratch venv); runs without any visible window. A meet whose file already exists is skipped (resume).
Live is a single-page app: an event's results appear a few seconds after the page loads, and an unknown event id shows only the news widget."""
import argparse, asyncio, os, re, sys, time
from playwright.async_api import async_playwright

POLL = """() => {
  const txt = document.body.innerText; const k = txt.indexOf('News\\n'); if (k < 0) return {s: 'wait'};
  const tail = txt.slice(k + 5); const title = tail.split('\\n')[0].trim();
  if (!title || /Loading/.test(title)) return {s: 'wait'};
  if (/^Don.t Miss Your/.test(title)) return {s: 'dm'};
  const clean = c => c.innerText.replace(/\\s+/g, ' ').trim();
  const trs = [...document.querySelectorAll('tr')];
  const head = trs[0] ? [...trs[0].children].map(clean).join('\\u00a6') : '';
  const tabs = tail.slice(0, 260).replace(/\\n/g, '|');
  if (trs.length > 3 && /^Place/.test(head)) return {s: 'table', title, tabs, header: head, rows: trs.slice(1).map(tr => [...tr.children].map(clean))};
  if (/NO RESULTS TO DISPLAY/.test(txt)) return {s: 'empty', title, tabs};
  return {s: 'title', title, tabs};
}"""

DEFAULT_MEETS = [500910, 520947, 530482, 536722, 537703, 544353, 557755, 565228, 568366, 582665, 584821, 599694, 600237, 602433, 607578, 610551, 612249,
                 613912, 613915, 642052, 648529, 650781, 652304, 655975, 679505, 679882, 679901, 680244, 686244, 698212, 709968, 712377]


async def get_ev(browser, sem, meet, eid, rnd, lt, quick=False):
    """Returns None (no such event), or dict(title, tabs, header, rows) / dict(title, tabs, empty=True)."""
    async with sem:
        page = await browser.new_page()
        try:
            for attempt in range(3):   # a page load occasionally times out; do not mistake that for a missing event
                try:
                    await page.goto(f'https://milesplit.live/meets/{meet}/events/{eid}/results/{rnd}/{lt}', wait_until='domcontentloaded', timeout=45000); break
                except Exception:
                    if attempt == 2: raise
            t0 = time.time(); dm_since = None
            while time.time() - t0 < (14 if quick else 32):
                await asyncio.sleep(0.4)
                try: r = await page.evaluate(POLL)
                except Exception: continue
                s = r['s']
                if s == 'wait': continue
                if s == 'dm':
                    dm_since = dm_since or time.time()
                    if time.time() - dm_since > (7 if quick else 9): return None
                    continue
                if quick: return {'title': r['title']}
                if s == 'table':
                    await asyncio.sleep(0.8)
                    try: r = await page.evaluate(POLL)
                    except Exception: pass
                    return r if r['s'] == 'table' else {'title': r['title'], 'tabs': r.get('tabs', ''), 'empty': True}
                if s == 'empty' or (s == 'title' and time.time() - t0 > 15): return {'title': r['title'], 'tabs': r.get('tabs', ''), 'empty': True}
            return None
        except Exception as e:
            print('   error', meet, eid, rnd, e, file=sys.stderr); return None
        finally:
            await page.close()


async def exists(browser, sem, meet, eid):
    return (await get_ev(browser, sem, meet, eid, 'F', 'M', quick=True)) is not None


async def scan_id(browser, sem, meet, eid):
    r = await get_ev(browser, sem, meet, eid, 'F', 'M')
    if r is None: return None
    girls = r['title'].startswith('Girls')
    if girls: r = await get_ev(browser, sem, meet, eid, 'F', 'F')
    if r is None: return None
    lt = 'F' if girls else 'M'
    blocks = [('F', r)]
    tabs = r.get('tabs', '')
    if 'Prelims' in tabs: blocks.append(('P', await get_ev(browser, sem, meet, eid, 'P', lt)))
    if re.search(r'Semifinals|Semis', tabs): blocks.append(('S', await get_ev(browser, sem, meet, eid, 'S', lt)))
    out = []
    for rnd, x in blocks:
        if not x: continue
        if x.get('empty'): out.append(f"E|{eid}|{x['title']}|{rnd}|EMPTY"); continue
        out.append(f"E|{eid}|{x['title']}|{rnd}|{x['header']}")
        for row in x['rows']:
            if re.search('cloverleaf', ' '.join(row), re.I): out.append('|'.join(row))
    return out


async def scan_meet(browser, sem, meet):
    hit = None
    for p in [1, 2, 3, 4, 5, 6, 7, 8, 10, 12, 15, 20, 30, 50, 80, 100, 130, 160, 200, 260, 320]:
        if await exists(browser, sem, meet, p): hit = p; break
    if hit is None: return None, 'none'
    async def sweep(start, step, limit):
        e, miss, last = start, 0, None
        while miss < limit and 1 <= e <= start + 500 * (1 if step > 0 else 0) + 500:
            ids = [e + step * i for i in range(6) if 1 <= e + step * i]
            if not ids: break
            res = await asyncio.gather(*(exists(browser, sem, meet, i) for i in ids))
            for i, ok in zip(ids, res):
                if ok: last, miss = i, 0
                else: miss += 1
                if miss >= limit: break
            e = ids[-1] + step
        return last
    hi = await sweep(hit + 1, 1, 12) or hit
    lo = await sweep(hit - 1, -1, 6) or hit
    ids = list(range(lo, hi + 1))
    res = await asyncio.gather(*(scan_id(browser, sem, meet, i) for i in ids))
    lines = [l for r in res if r for l in r]
    return '\n'.join(lines) + '\n', f'{lo}-{hi}'


async def main():
    ap = argparse.ArgumentParser(); ap.add_argument('meets', nargs='*', type=int); ap.add_argument('--out', default=os.path.join(os.path.dirname(os.path.abspath(__file__)), 'live'))
    ap.add_argument('--force', action='store_true'); ap.add_argument('--parallel', type=int, default=4)
    a = ap.parse_args(); os.makedirs(a.out, exist_ok=True)
    meets = a.meets or DEFAULT_MEETS
    async with async_playwright() as pw:
        browser = await pw.chromium.launch(headless=True)
        sem = asyncio.Semaphore(a.parallel)
        for i, m in enumerate(meets, 1):
            path = os.path.join(a.out, f'{m}.txt')
            if os.path.exists(path) and not a.force: print(f'[{i}/{len(meets)}] {m}: exists, skipped', flush=True); continue
            t0 = time.time()
            text, rng = await scan_meet(browser, sem, m)
            if text is None: print(f'[{i}/{len(meets)}] {m}: no Live events found', flush=True); open(path, 'w').write('# none\n'); continue
            open(path + '.tmp', 'w').write(text); os.replace(path + '.tmp', path)
            print(f'[{i}/{len(meets)}] {m}: events {rng}, {text.count(chr(10))} lines, {time.time() - t0:.0f}s', flush=True)
        await browser.close()

asyncio.run(main())
