// Paste into the Browser pane's javascript tool on any athletic.net page (the pane must be VISIBLE; a hidden pane never
// paints the SPA and every page comes back empty). Scans each varsity event page of a meet in a same-origin iframe, one at a
// time, and returns Cloverleaf rows carrying ND / NM / NH / DNF / DQ / DNS (field events: ?tab=field-series). Takes ~2-3 min per
// meet; the tool call limit is 45 s, so start it without awaiting and poll window.__prog:
//   eval(<this file>); window.__prog = []; window.__job = (async () => { for (const m of [637822, ...]) window.__prog.push(await window.scanMeet(m)); window.__done = true; })();
// FALSE POSITIVE TO FILTER IN POST-PROCESSING: the line before the name is the athlete's INITIALS (e.g. "NM" for Noah Mion),
// which matches the NM token. A real status sits in the place column (field series) or right after the team (running results).
// Only division 1 pages are scanned (m/1, f/1); pages with no Cloverleaf row come back in `bad`, as do pages that failed to render.
window.scanPages = async function (urls) {
  const T = /^(ND|NM|NH|DNF|DQ|DNS|FS|NT)$/; const res = [];
  async function one(u) {
    const f = document.createElement('iframe'); f.style.cssText = 'position:fixed;left:0;top:0;width:1000px;height:800px;background:#fff;z-index:99999'; f.src = u; document.body.appendChild(f);
    let txt = '', ok = false;
    for (let i = 0; i < 16; i++) { await new Promise(r => setTimeout(r, 500)); try { txt = f.contentDocument.body.innerText; if (/results\/[mf]\//.test(f.contentWindow.location.pathname) && txt.length > 3000 && (txt.includes('Cloverleaf') || i >= 8)) { ok = txt.includes('Cloverleaf'); break; } } catch (e) { } }
    const L = txt.split('\n').map(s => s.trim()).filter(s => s && s !== '\t'); const hits = [];
    L.forEach((l, i) => { if (l === 'Cloverleaf') { const pre = L.slice(Math.max(0, i - 4), i); const post = (L[i + 1] || '').split(' ')[0]; if (pre.some(x => T.test(x)) || T.test(post)) hits.push(L.slice(Math.max(0, i - 4), i + 11).join('|')); } });
    f.remove(); res.push({ u: u.split('/meet/')[1], ok, len: txt.length, hits });
  }
  for (const u of urls) await one(u);
  return res;
};
window.eventLinks = async function (meet) {
  const f = document.createElement('iframe'); f.style.cssText = 'position:fixed;left:0;top:0;width:1000px;height:800px;background:#fff;z-index:99999'; f.src = 'https://www.athletic.net/TrackAndField/meet/' + meet + '/results'; document.body.appendChild(f);
  let l = []; for (let i = 0; i < 24; i++) { await new Promise(r => setTimeout(r, 500)); try { l = [...new Set([...f.contentDocument.querySelectorAll('a[href*="/results/"]')].map(x => x.getAttribute('href')).filter(h => /results\/[mf]\/1\//.test(h)).map(h => h.split('/results/')[1].split('?')[0]))]; if (l.length > 5) break; } catch (e) { } }
  f.remove(); return l;
};
window.scanMeet = async function (meet) {
  const ev = await window.eventLinks(meet); const fld = /^[mf]\/\d\/(s-)?(shot|discus|hj|pv|lj|tj|jt)/;
  const urls = ev.map(e => 'https://www.athletic.net/TrackAndField/meet/' + meet + '/results/' + e + (fld.test(e) ? '?tab=field-series' : ''));
  const r = await window.scanPages(urls); return { meet, events: ev.length, bad: r.filter(x => !x.ok).map(x => x.u), hits: r.filter(x => x.hits.length).map(x => ({ u: x.u, h: x.hits })) };
};
