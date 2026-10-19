// Paste into the Browser pane's javascript tool on any athletic.net page (pane VISIBLE; see scan_no_marks.js for the caveats).
// window.anRows(urls) scans event pages one at a time in a same-origin iframe, scrolling to render virtualized rows, and returns per page
// {u, marked, nomark, rows:[{k:'ind'|'relay', place, name, mark, info|legs, sec}]} for Cloverleaf rows. marked/nomark count rows with a
// distance/height vs ND/NH/NM for EVERY team (used by the "top-8-only event" rule). window.evLinks2(meetId) lists a meet's event slugs.
// Save the result as scripts/crawl/an-rows/an-<meetId>.json and feed it to build_hytek_sql.py (parse_an.py turns it into rows).
// Individual rows: [place][initials][Name][Cloverleaf][mark][info]; relay rows: [place][Cloverleaf][mark][legs] (legs = "Relay Team" when unnamed).
window.anRows = async function (urls) {
  const res = [];
  const INIT = /^[A-Z]{2,3}$/;
  async function one(u) {
    const f = document.createElement('iframe'); f.style.cssText = 'position:fixed;left:0;top:0;width:1000px;height:800px;background:#fff;z-index:99999'; f.src = u; document.body.appendChild(f);
    const rows = new Map(); let ok = false, title = '', marked = 0, nomark = 0; const seenAll = new Set();
    const scan = (txt) => {
      const L = txt.split('\n').map(s => s.trim()).filter(s => s && s !== '\t'); let sec = '';
      L.forEach((l, i) => {
        if (/^(Finals|Prelims|Prelim|Semi-?Finals|Preliminaries|Heat \d+.*|Section \d+.*)$/i.test(l)) sec = l;
        if (/^(ND|NH|NM)$/.test(l) && /^--$/.test(L[i - 4] || '')) { const k = 'nm' + i + L[i - 2]; if (!seenAll.has(k)) { seenAll.add(k); nomark++; } }
        if (/^\d+-\d+(\.\d+)?$/.test(l) && /^\d+$/.test(L[i - 4] || '') && INIT.test(L[i - 3] || '')) { const k = 'mk' + i + L[i - 2]; if (!seenAll.has(k)) { seenAll.add(k); marked++; } }
        if (l === 'Cloverleaf') {
          let r;
          if (INIT.test(L[i - 2] || '')) r = { k: 'ind', place: L[i - 3], name: L[i - 1], mark: L[i + 1], info: L[i + 2] || '', sec };
          else r = { k: 'relay', place: L[i - 1], mark: L[i + 1], legs: L[i + 2] || '', sec };
          rows.set(JSON.stringify(r), r);
        }
      });
    };
    for (let i = 0; i < 16; i++) { await new Promise(r => setTimeout(r, 500)); try { const t = f.contentDocument.body.innerText; if (/results\/[mf]\//.test(f.contentWindow.location.pathname) && t.length > 3000) { ok = true; title = f.contentDocument.title; break; } } catch (e) { } }
    if (ok) { try { const w = f.contentWindow, d = f.contentDocument; let y = 0, stall = 0;
      for (let s = 0; s < 40; s++) { scan(d.body.innerText); const docH = d.documentElement.scrollHeight; if (y >= docH - 800) { stall++; if (stall >= 2) break; } else stall = 0; y = Math.min(y + 500, docH); w.scrollTo(0, y); await new Promise(r => setTimeout(r, 450)); } } catch (e) { } }
    f.remove(); res.push({ u: u.split('/results/')[1], ok, title: title.slice(0, 60), marked, nomark, rows: [...rows.values()] });
  }
  for (const u of urls) await one(u);
  return res;
};
window.evLinks2 = async function (meet) { for (let a = 0; a < 3; a++) { const f = document.createElement('iframe'); f.style.cssText = 'position:fixed;left:0;top:0;width:1000px;height:800px;background:#fff;z-index:99999'; f.src = 'https://www.athletic.net/TrackAndField/meet/' + meet + '/results'; document.body.appendChild(f); let l = []; for (let i = 0; i < 24; i++) { await new Promise(r => setTimeout(r, 500)); try { l = [...new Set([...f.contentDocument.querySelectorAll('a[href*="/results/"]')].map(x => x.getAttribute('href')).filter(h => /results\/[mf]\/\d/.test(h)).map(h => h.split('/results/')[1].split('?')[0]))]; if (l.length > 5) break; } catch (e) { } } f.remove(); if (l.length > 3) return l; } return []; };
