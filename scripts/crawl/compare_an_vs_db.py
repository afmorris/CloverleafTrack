"""Results-level comparison: athletic.net results JSON (per meet, Cloverleaf rows only) vs the DB, per matched meet.
For every Cloverleaf row in the source (best mark per athlete-event) finds the DB Performance for the same meet/event/athlete:
OK (mark within tolerance), MISMATCH (different mark), MISSING (no DB row). Relays are matched by event + time only.
Reads /tmp/msraw/ms-<id>.txt (see the download loop in BRAIN.md [C74]) and the local ctf-local-sql. Writes results-compare-ms.tsv."""
import csv, json, re, subprocess, sys, collections, os
sys.path.insert(0, '.')
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


ANDIR = os.environ.get('ANDIR', '/tmp/andir')
mt = {}
for l in list(open('matches.tsv'))[1:]:
    p = l.rstrip('\n').split('\t')
    if p[0] == 'athleticnet' and p[4]: mt[p[1]] = int(p[4])
def an_event(name):
    n = name.strip()
    m = re.match(r'^(\d+)m( Hurdles)?$', n) or re.match(r'^(\d+) Meters?( Hurdles)?$', n)
    if m: return f"{m.group(1)}M" + (' Hurdles' if m.group(2) else '')
    m = re.match(r'^(4x\d+)m? Relay$', n) or re.match(r'^(4x\d+) Meters? Relay$', n)
    if m: return m.group(1) + 'M Relay'
    m = re.match(r'^(\d+)m Sprint Medley', n)
    if n in ('DMR 4000m',): return '4K Distance Medley'
    if n in ('SMR 800m',): return '800M Sprint Medley'
    if n == '4x110 Shuttle Hurdles' or n == '4x100 Shuttle Hurdles': return '4x100M Shuttle Hurdles'
    return {'Shot Put': 'Shot Put', 'Discus': 'Discus', 'Long Jump': 'Long Jump', 'High Jump': 'High Jump', 'Pole Vault': 'Pole Vault', 'Triple Jump': 'Triple Jump'}.get(n, n)
def an_mark(res):
    r = re.sub(r'[A-Za-z]+$', '', res.strip())
    if re.match(r'^\d+-\d+(\.\d+)?$', r): f, i = r.split('-'); return 'dist', int(f) * 12 + float(i)
    m = re.match(r'^(?:(\d+):)?(\d+(?:\.\d+)?)$', r)
    if m: return 'time', (int(m.group(1)) * 60 if m.group(1) else 0) + float(m.group(2))
    return 'other', None
rows_out, summary = [], []
def load_text():
    meets_j = {}
    cur = None; ev = None
    for fn in sorted(os.listdir(ANDIR)):
        if not fn.endswith('.txt'): continue
        for line in open(os.path.join(ANDIR, fn), encoding='utf-8'):
            line = line.rstrip('\n')
            if line.startswith('M '): cur = meets_j.setdefault(line[2:].strip(), {'meet': line[2:].strip(), 'events': []}); continue
            if line.startswith('E|'): _, name, g = line.split('|'); ev = {'ev': name, 'g': g, 'rows': []}; cur['events'].append(ev); continue
            p = line.split('|')
            if len(p) < 7: continue
            ev['rows'].append({'Place': p[0], 'Round': p[1], 'Result': p[2], 'AthleteID': p[3], 'FirstName': p[4], 'LastName': p[5], 'Exhibition': p[6] == '1'})
    return meets_j
for mid, j in load_text().items():
    dbid = mt[mid]; env = meets[dbid][0]
    ok = mis = miss = unres = 0; best = {}; relays = collections.defaultdict(dict)
    for ev in j['events']:
        g = 2 if ev['g'] == 'F' else 1
        name = an_event(ev['ev'])
        if 'Seated' in meets[dbid][2] and not name.startswith('4x'): name = 'Seated ' + name
        isrelay = bool(re.match(r'^4x', name)) or 'Relay' in name or 'Medley' in name or 'Shuttle' in name
        for r in ev['rows']:
            if r.get('Exhibition'): continue
            k, v = an_mark(r['Result'])
            if k not in ('time', 'dist'):
                if r['Result'] in ('DQ', 'DNF', 'NM'): rows_out.append((dbid, mid, 'NOMARK', ev['g'], name, r['FirstName'], r['Result'], ''))
                continue
            if isrelay:
                d = relays[(g, name)]; d[r['AthleteID']] = min(v, d.get(r['AthleteID'], 1e9)); continue
            key = (g, name, r['AthleteID'], f"{r['FirstName']} {r['LastName']}")
            cur = best.get(key); isfield = name in FIELD
            if cur is None or (v > cur if isfield else v < cur): best[key] = v
    for (g, evname, aid_an, who), v in best.items():
        eid = events.get((env, g, evname))
        if not eid: unres += 1; rows_out.append((dbid, mid, 'NOEVENT', 'Girls' if g == 2 else 'Boys', evname, who, v, '')); continue
        tol = 0.3 if evname in FIELD else 0.06
        cand = perf.get((dbid, eid), [])
        aid, how = resolve(who, g)
        if aid is None and how == 'ambiguous':
            hit = [a for a in ath.get((norm(who), g), []) if any(c[0] == a for c in cand)]
            if hit: aid, how = hit[0], 'dup'
        if aid is None and evname.startswith('Seated'): aid, how = resolve(who, 3 - g)
        if aid is None: unres += 1; rows_out.append((dbid, mid, 'ATHLETE?', 'Girls' if g == 2 else 'Boys', evname, who, v, how)); continue
        mine = [c for c in cand if c[0] == aid]
        if not mine: miss += 1; rows_out.append((dbid, mid, 'MISSING', 'Girls' if g == 2 else 'Boys', evname, who, v, how))
        else:
            c = mine[0]; dv = c[2] if c[2] is not None else c[1]
            alt = int(v) + round((v % 1) * 10, 1) if round(v * 100) % 10 != 0 and round(v * 100) % 100 < 10 and evname not in FIELD else None
            if dv is not None and (abs(dv - v) <= tol or (alt is not None and abs(dv - alt) <= tol)): ok += 1
            else: mis += 1; rows_out.append((dbid, mid, 'MISMATCH', 'Girls' if g == 2 else 'Boys', evname, who, v, dv))
    for (g, evname), vs in relays.items():
        eid = events.get((env, g, evname))
        if not eid: continue
        left = [c for c in perf.get((dbid, eid), []) if c[0] is None]
        for v in sorted(vs.values()):
            hit = next((c for c in left if any(x is not None and abs(x - v) <= 0.06 for x in (c[1], c[2]))), None)
            if hit: left.remove(hit); ok += 1
            elif left: c = left.pop(0); mis += 1; rows_out.append((dbid, mid, 'RELAY-MISMATCH', 'Girls' if g == 2 else 'Boys', evname, '', v, c[1] or c[2]))
            else: miss += 1; rows_out.append((dbid, mid, 'RELAY-MISSING', 'Girls' if g == 2 else 'Boys', evname, '', v, ''))
    dbn = sum(len(v) for (m, e), v in perf.items() if m == dbid)
    summary.append((dbid, mid, meets[dbid][2][:40], ok, mis, miss, unres, dbn))
with open(os.environ.get('OUT', 'results-compare-an.tsv'), 'w') as f:
    w = csv.writer(f, delimiter='\t'); w.writerow(['dbMeet', 'anMeet', 'status', 'gender', 'event', 'who', 'sourceValue', 'dbValue'])
    for r in rows_out: w.writerow(r)
print('dbMeet anMeet name OK MISMATCH MISSING UNRESOLVED dbPerfs')
for s in summary: print(*s)
