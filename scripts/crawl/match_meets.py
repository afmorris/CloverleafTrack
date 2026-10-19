#!/usr/bin/env python3 -I
"""Match crawled athletic.net / MileSplit meets to DB meets (by environment, date and name similarity).
usage: match_meets.py  -> writes matches.tsv and prints a coverage summary.
Inputs: dbmeets.txt (Id|Date|Environment|Name), athleticnet-meets.txt, milesplit-meets-raw.txt"""
import re, datetime as dt, sys
MON = {m: i for i, m in enumerate('Jan Feb Mar Apr May Jun Jul Aug Sep Oct Nov Dec'.split(), 1)}
STOP = {'the','of','and','vs','v','hs','high','school','annual','meet','open','track','field','invitational','invite','relays','championship','championships','state','ohsaa','d1','d2','d3','div','division','univ','university','college'}
CHAMP = re.compile(r'district|region|regional|state|championship|finale|finals|league|conference|PTC|OATCCC|OHIAA|suburban', re.I)
def toks(s):
    s = re.sub(r"[^a-z0-9 ]", " ", s.lower()); s = re.sub(r"\b(19|20)\d\d\b", " ", s)
    return {t for t in s.split() if t not in STOP and not t.isdigit()}
def sim(a, b):
    A, B = toks(a), toks(b)
    return len(A & B) / max(1, len(A | B)) if (A or B) else 0
db = []
for l in open('dbmeets.txt'):
    p = l.strip().split('|')
    if len(p) < 4: continue
    db.append(dict(id=int(p[0]), date=dt.date.fromisoformat(p[1]), env=int(p[2]), name=p[3]))
cands = []
for l in open('athleticnet-meets.txt'):
    sea, md, mid, name = l.rstrip('\n').split('|', 3)
    env = 1 if sea.startswith('outdoor') else 2; y = int(sea[-4:])
    mon, day = md.split(); m = MON[mon]
    year = y if env == 1 else (y - 1 if m >= 10 else y)
    cands.append(dict(src='athleticnet', mid=mid, name=name, date=dt.date(year, m, int(day)), env=env,
                      url=f'https://www.athletic.net/TrackAndField/meet/{mid}'))
for l in open('milesplit-meets-raw.txt'):
    sea, md, mid, name = l.rstrip('\n').split(' ', 3)
    env = 1 if sea.endswith('o') else 2; y = int(sea[:4]); m, d = map(int, md.split('/'))
    year = y + 1 if env == 1 else (y if m >= 10 else y + 1)
    cands.append(dict(src='milesplit', mid=mid, name=name, date=dt.date(year, m, d), env=env, url=f'https://oh.milesplit.com/meets/{mid}'))
EXCLUDE = re.compile(r'7th|8th|grade|team import|nationals|middle school|\bMS\b', re.I)
rows = []; used = {}
for c in cands:
    best = None
    for m in db:
        if m['env'] != c['env'] or EXCLUDE.search(c['name']): continue
        if bool(re.search('seated', c['name'], re.I)) != bool(re.search('seated', m['name'], re.I)): continue
        dd = abs((m['date'] - c['date']).days)
        if dd > 3: continue
        s = sim(c['name'], m['name']) + (1.0 if dd == 0 else 0.4 if dd <= 1 else 0.1)
        if best is None or s > best[0]: best = (s, m, dd)
    window = [m for m in db if m['env'] == c['env'] and not EXCLUDE.search(c['name']) and bool(re.search('seated', c['name'], re.I)) == bool(re.search('seated', m['name'], re.I)) and abs((m['date'] - c['date']).days) <= (0 if not CHAMP.search(c['name']) else 3)]
    ok = best and (best[0] >= 1.3 or (best[2] == 0 and best[0] >= 1.15))
    if not ok and window:
        cw = sorted(window, key=lambda m: -sim(c['name'], m['name']))
        top = cw[0]
        if (len(window) == 1 and sim(c['name'], top['name']) >= 0.15 and (top['date'] == c['date'] or (CHAMP.search(c['name']) and CHAMP.search(top['name'])))) or \
           (len(window) == 1 and CHAMP.search(c['name']) and CHAMP.search(top['name'])) or \
           (CHAMP.search(c['name']) and CHAMP.search(top['name']) and sim(c['name'], top['name']) >= 0.2):
            best = (1.0, top, abs((top['date'] - c['date']).days)); ok = True
    # exact-date single DB meet on that date is also accepted when it is the only candidate
    c['match'] = best[1] if ok else None; c['score'] = best[0] if best else 0; c['dd'] = best[2] if best else None
    rows.append(c)
with open('matches.tsv', 'w') as f:
    f.write('source\tmeetId\tdate\tcrawledName\tdbMeetId\tdbDate\tdbName\tscore\n')
    for c in rows:
        m = c['match']
        f.write('\t'.join([c['src'], c['mid'], str(c['date']), c['name'], str(m['id']) if m else '', str(m['date']) if m else '', m['name'] if m else '', f"{c['score']:.2f}"]) + '\n')
matched = {c['match']['id'] for c in rows if c['match']}
print('crawled:', len(rows), 'matched:', sum(1 for c in rows if c['match']), 'distinct DB meets covered:', len(matched), 'of', len(db))
for src in ('athleticnet', 'milesplit'):
    rs = [c for c in rows if c['src'] == src]; print(src, len(rs), 'matched', sum(1 for c in rs if c['match']))

print()
for c in rows:
    if c['match']: continue
    near = [m for m in db if m['env'] == c['env'] and abs((m['date'] - c['date']).days) <= 4]
    print(f"{c['src'][:2]} {c['date']} {c['name'][:48]:48} -> " + ('; '.join(f"{m['id']}:{m['date']:%m-%d} {m['name'][:30]}" for m in near) or 'NO DB MEET'))

# ---- one official link per DB meet: prefer athletic.net, then MileSplit; best score first
pick = {}
for c in rows:
    if not c['match']: continue
    k = c['match']['id']
    rank = (0 if c['src'] == 'athleticnet' else 1, -c['score'], c['dd'])
    if k not in pick or rank < pick[k][0]: pick[k] = (rank, c)
with open('official-results-links.sql', 'w') as f:
    f.write("-- Generated by scripts/crawl/match_meets.py: official results links from the athletic.net / oh.milesplit.com crawl.\n")
    f.write("-- Only fills meets whose OfficialResultsUrl is still NULL. Requires docs/migration_meet_official_results_url.sql.\n")
    f.write("SET XACT_ABORT ON;\nBEGIN TRANSACTION;\n")
    for k in sorted(pick):
        c = pick[k][1]
        nm = [m for m in db if m['id'] == k][0]
        f.write(f"-- {nm['date']} {nm['name']}  <=  {c['src']}: {c['name']} ({c['date']})\n")
        f.write(f"UPDATE Meets SET OfficialResultsUrl = '{c['url']}' WHERE Id = {k} AND OfficialResultsUrl IS NULL;\n")
    f.write("COMMIT TRANSACTION;\n")
print('links:', len(pick), 'athleticnet:', sum(1 for v in pick.values() if v[1]['src']=='athleticnet'), 'milesplit:', sum(1 for v in pick.values() if v[1]['src']=='milesplit'))
