"""Results-level comparison: MileSplit raw Hy-Tek text vs the DB, per matched meet.
For every Cloverleaf row in the source (best mark per athlete-event) finds the DB Performance for the same meet/event/athlete:
OK (mark within tolerance), MISMATCH (different mark), MISSING (no DB row). Relays are matched by event + time only.
Reads /tmp/msraw/ms-<id>.txt (see the download loop in BRAIN.md [C74]) and the local ctf-local-sql. Writes results-compare-ms.tsv."""
import csv, re, subprocess, sys, collections, os
sys.path.insert(0, '.')
from parse_hytek2 import parse
from parse_hytek import parse_mark

def q(sql):
    out = subprocess.run(['docker', 'exec', '-i', 'ctf-local-sql', '/opt/mssql-tools18/bin/sqlcmd', '-C', '-I', '-S', 'localhost', '-d', 'ctf', '-U', 'sa', '-P', 'Local#Dev1234', '-W', '-s', '|', '-h', '-1'],
                         input='SET NOCOUNT ON;' + sql, capture_output=True, text=True).stdout
    return [l.split('|') for l in out.strip().split('\n') if l.strip()]

ALIAS = {'hilar y hewit': 'hillary hewit', 'cassy maxwell': 'cassandra maxwell', 'neil berentz': 'neal berentz', 'rick morrison': 'richard morrison', 'ricky morrison': 'richard morrison', 'stacy porter': 'stacey porter',
         'brittany pekar': 'brittani pekar', 'william simpson': 'billy simpson', 'garrett livengood': 'garrett livingood', 'jeff sharrat': 'jeff sharratt', 'joe bruening': 'joey bruening',
         'mackenna huff': 'mckenna huff', 'r.j. bechtel': 'rj bechtel', 'rj becthel': 'rj bechtel', 'vinnie kacinari': 'vincent kacinari', 'katelyn donnelly': 'katlyn donnelly', 't.j. corbett': 'tj corbett', 'bryce neil': 'bryce neill', 'asa gyn': 'asa gynn', 'isaiah curt': 'isaiah kurtz', 'josh bohemian': 'josh bowman', 'jacob knau': 'jacob nau', 'meagan van meter': 'megan van meter'}
def norm(n):
    n = re.sub(r'\s+', ' ', re.sub(r'\b\d+\b', ' ', n)).strip()   # Hy-Tek bib numbers glued into names ('Vinnie 570 Kacinari')
    return re.sub(r'[^a-z ]', '', ALIAS.get(n.lower().strip(), n.lower().strip())).strip()

events = {(int(e), int(g), n): int(i) for i, n, g, e in q("SELECT Id, Name, Gender, Environment FROM Events")}
ev_name = {i: (e, g, n) for (e, g, n), i in events.items()}
ath = collections.defaultdict(list)
for i, f, l, g in q("SELECT Id, FirstName, LastName, Gender FROM Athletes"): ath[(norm(f + ' ' + l), int(g))].append(int(i))
meets = {int(i): (int(e), d, n) for i, e, d, n in q("SELECT Id, Environment, CONVERT(varchar(10),Date,120), Name FROM Meets")}
perf = collections.defaultdict(list)   # (meet, event) -> [(athleteId|None, time, dist, status, perfId)]
for pid, m, e, a, t, d, s in q("SELECT Id, MeetId, EventId, ISNULL(CAST(AthleteId AS varchar(12)),''), ISNULL(CAST(TimeSeconds AS varchar(40)),''), ISNULL(CAST(DistanceInches AS varchar(40)),''), ResultStatus FROM Performances"):
    perf[(int(m), int(e))].append((int(a) if a else None, float(t) if t else None, float(d) if d else None, int(s), int(pid)))
def lev(a, b):
    prev = list(range(len(b) + 1))
    for i, ca in enumerate(a, 1):
        cur = [i]
        for j, cb in enumerate(b, 1): cur.append(min(prev[j] + 1, cur[-1] + 1, prev[j - 1] + (ca != cb)))
        prev = cur
    return prev[-1]
by_last = collections.defaultdict(list)   # (gender, last) -> [(first, id)]
for (nm, g), ids in ath.items():
    first, _, last = nm.partition(' ')
    for i in ids: by_last[(g, last)].append((first, i))
def resolve(name, g):
    n = norm(name)
    ids = ath.get((n, g), [])
    if len(ids) == 1: return ids[0], 'exact'
    if len(ids) > 1: return None, 'ambiguous'
    first, _, last = n.partition(' ')
    cands = []
    for (gg, ll), lst in by_last.items():
        if gg != g or (ll != last and not (len(last) >= 5 and lev(ll, last) <= 1)): continue
        for f, i in lst:
            if f == first or f.startswith(first) or first.startswith(f) or lev(f, first) <= 2 or (f[:1] == first[:1] and ll == last): cands.append((f, ll, i))
    cands = list({c[2]: c for c in cands}.values())
    if len(cands) == 1: return cands[0][2], 'fuzzy'
    return None, 'none' if not cands else 'ambiguous'
FIELD = ('High Jump', 'Long Jump', 'Pole Vault', 'Shot Put', 'Discus', 'Triple Jump', 'Hammer Throw', 'Javelin')

MSDIR = os.environ.get('MSDIR', '/tmp/msraw')
pairs = [l.split() for l in open(MSDIR + '/ids.txt')]
rows_out, summary, inserts, updates = [], [], [], []
for msid, dbid in pairs:
    path = f'{MSDIR}/ms-{msid}.txt'
    if not os.path.exists(path): summary.append((int(dbid), msid, 'no raw', 0, 0, 0, 0, 0)); continue
    dbid = int(dbid); env = meets[dbid][0]
    src = parse(path)
    for r in src:   # throws/jumps 'relays' list each athlete's own mark: store them as the plain individual event
        if r['kind'] == 'ind' and r['event'].endswith(' Relay') and r['event'][:-6] in FIELD: r['event'] = r['event'][:-6]
    mentions = len(re.findall(r'(?i)clover', open(path, encoding='utf-8', errors='replace').read()))
    best = {}
    seen_rounds = {}
    for r in src:
        k, v = parse_mark(r['mark'])
        if k not in ('time', 'dist'): continue
        if r['kind'] == 'relay': continue
        key = (r['gender'], r['event'], r['kind'], tuple(a[0] for a in r['athletes']))
        cur = best.get(key)
        isfield = r['event'] in FIELD
        seen_rounds.setdefault(key, set()).add(r['round'])
        if cur is None or (v > cur[0] if isfield else v < cur[0]): best[key] = (v, r)
    ok = mis = miss = unres = 0
    for (gender, evname, kind, who), (v, r) in best.items():
        g = 2 if gender == 'Girls' else 1
        eid = events.get((env, g, evname))
        if not eid: unres += 1; rows_out.append((dbid, msid, 'NOEVENT', gender, evname, who, v, '')); continue
        tol = 0.3 if evname in FIELD else 0.06
        cand = perf.get((dbid, eid), [])
        aid, how = resolve(who[0], g)
        if aid is None and evname.startswith('Seated'): aid, how = resolve(who[0], 3 - g)   # mixed-gender wheelchair fields are labelled Boys
        if aid is None and how == 'ambiguous':   # duplicate athlete records (same normalised name): use the one that has a result in this meet/event
            dup = ath.get((norm(who[0]), g), [])
            hit = [a for a in dup if any(c[0] == a for c in cand)]
            if len(hit) >= 1: aid, how = hit[0], 'dup'
        if aid is None: unres += 1; rows_out.append((dbid, msid, 'ATHLETE?', gender, evname, who[0], v, how)); continue
        mine = [c for c in cand if c[0] == aid]
        if not mine:
            miss += 1; rows_out.append((dbid, msid, 'MISSING', gender, evname, who[0], v, how))
            inserts.append(('ind', dbid, eid, [aid], v, evname in FIELD))
        else:
            c = mine[0]; dv = c[2] if c[2] is not None else c[1]
            alt = int(v) + round((v % 1) * 10, 1) if round(v * 100) % 10 != 0 and round(v * 100) % 100 < 10 and evname not in FIELD else None   # Hy-Tek prints hand-timed 27.6 as 27.06
            if dv is not None and (abs(dv - v) <= tol or (alt is not None and abs(dv - alt) <= tol)): ok += 1
            else:
                mis += 1; rows_out.append((dbid, msid, 'MISMATCH', gender, evname, who[0], v, dv))
                updates.append((dbid, eid, aid, v, dv, c[4], evname in FIELD, sorted(seen_rounds.get((gender, evname, kind, who), []))))
    # relays: one entry per Cloverleaf TEAM (grouped by the four leg names), matched one-to-one to the DB's relay rows by time
    teams = collections.defaultdict(dict)       # (gender, event) -> {legs|None: (best, athletes)}
    for r in src:
        if r['kind'] != 'relay': continue
        k, v = parse_mark(r['mark'])
        if k not in ('time', 'dist'): continue
        legs = frozenset(a[0] for a in r['athletes']) if len(r['athletes']) == 4 else None
        d = teams[(r['gender'], r['event'])]
        cur = d.get(legs)
        if cur is None or v < cur[0]: d[legs] = (v, r['athletes'])
    for (gender, evname), d in teams.items():
        g = 2 if gender == 'Girls' else 1
        eid = events.get((env, g, evname))
        if not eid: continue
        named = {k: x for k, x in d.items() if k is not None}
        entries = list(named.values()) if named else [x for x in d.values()]       # nameless rounds only count when no named team exists
        dbrel = [c for c in perf.get((dbid, eid), []) if c[0] is None]
        tol = 0.06
        left = list(dbrel)
        pending = []
        for v, aths in sorted(entries, key=lambda x: x[0]):
            alt = int(v) + round((v % 1) * 10, 1) if round(v * 100) % 10 != 0 and round(v * 100) % 100 < 10 else None
            hit = next((c for c in left if any(x is not None and (abs(x - v) <= tol or (alt is not None and abs(x - alt) <= tol)) for x in (c[1], c[2]))), None)
            if hit: left.remove(hit); ok += 1
            else: pending.append((v, aths))
        for v, aths in pending:
            if left:
                c = left.pop(0); mis += 1; rows_out.append((dbid, msid, 'RELAY-MISMATCH', gender, evname, [a[0] for a in aths], v, c[1] or c[2]))
            else:
                miss += 1; rows_out.append((dbid, msid, 'RELAY-MISSING', gender, evname, [a[0] for a in aths], v, ''))
                legs = [resolve(a[0], g) for a in aths]
                if len(legs) == 4 and all(l[0] for l in legs) and len({l[0] for l in legs}) == 4: inserts.append(('relay', dbid, eid, [l[0] for l in legs], v, False))
    dbn = sum(len(v) for (m, e), v in perf.items() if m == dbid)
    summary.append((dbid, msid, meets[dbid][2][:40], mentions, ok, mis, miss, unres, dbn))
with open(os.environ.get('OUT', 'results-compare-ms.tsv'), 'w') as f:
    w = csv.writer(f, delimiter='\t'); w.writerow(['dbMeet', 'msMeet', 'status', 'gender', 'event', 'who', 'sourceValue', 'dbValue'])
    for r in rows_out: w.writerow(r)
print('dbMeet msMeet name clover-mentions OK MISMATCH MISSING UNRESOLVED dbPerfs')
for s in sorted(summary, key=lambda s: -(s[6] + s[5]) if isinstance(s[6], int) else 0)[:40]: print(*s)
tot = collections.Counter(r[2] for r in rows_out)
print(dict(tot), 'OK total', sum(s[4] for s in summary))

import json
json.dump(updates, open(os.environ.get('OUTUPD', 'results-mismatch-ms.json'), 'w'))
json.dump(inserts, open(os.environ.get('OUTJSON', 'results-missing-ms.json'), 'w'))
print(len(inserts), 'insertable missing rows ->', 'results-missing-ms.json')
