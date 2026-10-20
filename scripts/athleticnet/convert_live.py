#!/usr/bin/env python3 -I
"""Converts MileSplit Live field-event rows (scripts/athleticnet/live-raw/<msMeetId>.txt (written by scan_live.py), see README) into the inputs of build_scan_sql.py /
build_bars_sql.py: data-live/<dbMeetId>/{sheets.txt,bars.txt,db.txt}. Cloverleaf rows only; performances that already have attempts or bars are skipped.
usage: convert_live.py <live-dir> [msMeetId ...]   (needs the local ctf-local-sql container and ../crawl/matches.tsv)
Live row cells: Place, (blank), Athlete, Year, Team, Mark, then Attempt 1..6 (horizontal) or one cell per bar height (vertical: PPP/P = pass,
O clear, X miss; '--' = not taken)."""
import collections, os, re, subprocess, sys
here = os.path.dirname(os.path.abspath(__file__))
def q(sql):
    out = subprocess.run(['docker', 'exec', '-i', 'ctf-local-sql', '/opt/mssql-tools18/bin/sqlcmd', '-C', '-I', '-S', 'localhost', '-d', 'ctf', '-U', 'sa', '-P', 'Local#Dev1234', '-W', '-s', '|', '-h', '-1'],
                         input='SET NOCOUNT ON;' + sql, capture_output=True, text=True).stdout
    return [l.split('|') for l in out.strip().split('\n') if l.strip()]
ALIAS = {'kierston campbell': 'kk campbell', 'isabella messmore': 'izzy messmore', 'jacob schaefer': 'jake schaefer', 'jamere johnson': 'jj johnson'}
def norm(n):
    n = re.sub(r'[^a-z ]', '', re.sub(r'\s+', ' ', re.sub(r'\b\d+\b', ' ', n)).lower().strip())
    return ALIAS.get(n, n)
EV = {'Long Jump': 'Long Jump', 'Triple Jump': 'Triple Jump', 'Shot Put': 'Shot Put', 'Discus Throw': 'Discus', 'Discus': 'Discus', 'Javelin Throw': 'Javelin', 'Pole Vault': 'Pole Vault', 'High Jump': 'High Jump', 'Weight Throw': 'Weight Throw'}
VERT = ('Pole Vault', 'High Jump')
TRACK = {'60 meter Dash': '60M', '100 meter Dash': '100M', '200 meter Dash': '200M', '400 meter Dash': '400M', '800 meter Run': '800M', '1600 meter Run': '1600M', '3200 meter Run': '3200M', '100 meter Hurdles': '100M Hurdles', '110 meter Hurdles': '110M Hurdles', '300 meter Hurdles': '300M Hurdles', '60 meter Hurdles': '60M Hurdles'}
def lev(a, b):
    prev = list(range(len(b) + 1))
    for i, ca in enumerate(a, 1):
        cur = [i]
        for j, cb in enumerate(b, 1): cur.append(min(prev[j] + 1, cur[-1] + 1, prev[j - 1] + (ca != cb)))
        prev = cur
    return prev[-1]
def inches(s):
    m = re.match(r'^(\d+)-(\d+(?:\.\d+)?)$', s)
    return int(m.group(1)) * 12 + float(m.group(2)) if m else None
def fmt(x):  # feet-inches string the builders parse (F-I); measured marks are quarter inches, so Live typos like 45-6.26 snap to 45-6.25
    x = round(x * 4) / 4
    f = int(x // 12); i = round(x - f * 12, 2); return f"{f}-{i:g}"
meets = {}
for l in list(open(os.path.join(here, '../crawl/matches.tsv')))[1:]:
    p = l.rstrip('\n').split('\t')
    if p[0] == 'milesplit' and p[4]: meets[p[1]] = int(p[4])
live_dir = sys.argv[1]
ids = sys.argv[2:] or [f[:-4] for f in sorted(os.listdir(live_dir)) if f.endswith('.txt')]
ath = collections.defaultdict(list)
for i, f, l, g in q("SELECT Id, FirstName, LastName, Gender FROM Athletes"): ath[(norm(f + ' ' + l), int(g))].append((int(i), f + ' ' + l))
notes = []
OVERRIDE_FIRST4 = {('530482', 'Jayce Kreider', 'Shot Put')}
# Ladders whose Live columns do not line up with the heights; transcribed by the owner (watching the meet / the Live page). 'XXP' = two misses then a pass;
# a lone 'X' = the last miss that ended the competition (needs docs/migration_performance_bars_x.sql).
MANUAL_BARS = {
    ('530482', 'Isaac Baltic', 'Pole Vault'): '10-0:P;10-6:P;11-0:O;11-6:XO;12-0:XO;12-6:XXX',
    ('530482', 'Jace Peacock', 'Pole Vault'): '7-0:P;7-6:P;8-0:P;8-6:P;9-0:XO;9-6:XXX',
    ('537703', 'Isaac Baltic', 'Pole Vault'): '11-8:P;12-0:P;12-4:P;12-8:P;13-0:P;13-4:O;13-8:XO;14-0:O;14-4:XXP;14-8:X',
}
for msid in ids:
    if msid not in meets: notes.append(f'{msid}: no DB meet match'); continue
    dbid = meets[msid]
    perfs = q(f"SELECT p.Id, e.Gender, e.Name, a.FirstName+' '+a.LastName, ISNULL(CAST(p.TimeSeconds AS varchar(30)),'NULL'), ISNULL(CAST(p.DistanceInches AS varchar(30)),'NULL'), (SELECT COUNT(*) FROM PerformanceAttempts x WHERE x.PerformanceId=p.Id)+(SELECT COUNT(*) FROM PerformanceBars b WHERE b.PerformanceId=p.Id), p.ResultStatus FROM Performances p JOIN Events e ON e.Id=p.EventId JOIN Athletes a ON a.Id=p.AthleteId WHERE p.MeetId={dbid}")
    have = {(int(g), ev, n): (int(i), d, int(c)) for i, g, ev, n, t, d, c, rs in perfs}
    os.makedirs(os.path.join(here, 'data-live', str(dbid)), exist_ok=True)
    sheets, bars = [], []
    rounds = collections.defaultdict(dict)   # (gender, event, name) -> {round: seconds}
    evname = None; gender = None; cols = []; rnd = 'F'; tmatch = None
    for line in open(os.path.join(live_dir, msid + '.txt'), encoding='utf-8'):
        line = line.rstrip('\n')
        if line.startswith('E|'):
            p = line.split('|', 4); title = p[2]; rnd = p[3] if len(p) > 3 else 'F'; header = p[4] if len(p) > 4 else ''
            m = re.match(r'^(Boys|Girls)\s+(.*?)\s*$', title)
            evname = EV.get(re.sub(r'\s+(JV|Varsity|Open)$', '', m.group(2))) if m else None; gender = (1 if m.group(1) == 'Boys' else 2) if m else None
            cols = header.split('\u00a6') if header != 'EMPTY' else []
            tmatch = m.group(2) if m else None
            continue
        if evname is None and gender is not None and cols and 'Time' in cols and 'Athlete' in cols and rnd in ('P', 'S', 'F'):
            c = line.split('|')
            if len(c) > cols.index('Time') and len(c) > cols.index('Athlete'):
                tev = TRACK.get(re.sub(r'\s+(JV|Varsity|Open)$', '', re.sub(r'\s+\d+"$', '', tmatch)), None) if tmatch else None
                if tev:
                    t = re.sub(r'[^0-9:.]', '', c[cols.index('Time')].split(' ')[0])
                    m2 = re.match(r'^(?:(\d+):)?(\d+(?:\.\d+)?)$', t)
                    if m2:
                        sec = (int(m2.group(1)) * 60 if m2.group(1) else 0) + float(m2.group(2))
                        cand = ath.get((norm(c[cols.index('Athlete')]), gender), [])
                        if len(cand) == 1: rounds[(gender, tev, cand[0][1])][rnd] = sec
            continue
        if not evname or rnd != 'F' or not cols: continue
        c = line.split('|')
        if len(c) < 6 or 'Mark' not in cols: continue
        who = c[cols.index('Athlete')] if 'Athlete' in cols and len(c) > cols.index('Athlete') else ''
        mark = c[cols.index('Mark')]
        cand = ath.get((norm(who), gender), [])
        if not cand:   # same last name, first name prefix / small edit distance, unique
            wf, _, wl = norm(who).partition(' ')
            cand = [(i, nm) for (k, g), lst in ath.items() if g == gender for i, nm in lst
                    if k.partition(' ')[2] == wl.replace(' ', '') or k.partition(' ')[2] == wl or (len(wl) >= 5 and lev(k.partition(' ')[2], wl) <= 1)
                    if (k.partition(' ')[0].startswith(wf) or wf.startswith(k.partition(' ')[0]) or lev(k.partition(' ')[0], wf) <= 1)]
            if len({i for i, _ in cand}) != 1: cand = []
        row = next((have[(gender, evname, nm)] + (nm,) for i, nm in cand if (gender, evname, nm) in have), None)
        if row is None:
            if mark not in ('--', '') : notes.append(f'{msid}/{dbid}: no DB result for {who} {evname} ({"Boys" if gender == 1 else "Girls"}) mark {mark}')
            continue
        pid, dist, haveser, nm = row
        if haveser and not os.environ.get('LIVE_IGNORE_EXISTING'): continue
        if evname in VERT and (msid, nm, evname) in MANUAL_BARS:
            bars.append(f'{gender}|{evname}|{nm}|{MANUAL_BARS[(msid, nm, evname)]}'); continue
        if evname in VERT:
            idx =[(i, h) for i, h in enumerate(cols) if re.match(r'^\d+-[\d.]+$', h)]
            if not idx: continue   # this meet's Live page has marks only (no bar heights)
            if len(c) <= idx[-1][0]: notes.append(f'{msid}: ladder width mismatch for {who} {evname}'); continue
            lad = []; ok = True
            for i, h in idx:
                cell = c[i].strip()
                if cell in ('--', ''): continue
                o = 'P' if re.fullmatch(r'P+', cell) else cell
                if o not in ('O', 'XO', 'XXO', 'XXX', 'P', 'XP', 'XXP'):
                    notes.append(f'{msid}: unsupported ladder cell {cell!r} for {who} {evname} at {h}'); ok = False; break
                lad.append(f'{fmt(inches(h))}:{o}')
            if ok and lad: bars.append(f'{gender}|{evname}|{nm}|{";".join(lad)}')
        else:
            idx = [i for i, h in enumerate(cols) if h.startswith('Attempt')]
            if not idx: continue
            att = []; ok = True
            for i in idx:
                cell = re.sub(r'\s*\(.*\)\s*$', '', c[i].strip()) if len(c) > i else ''   # wind reading '14-8.50 (-2.1)'
                if cell in ('--', ''): att.append(''); continue
                if cell in ('F', 'P'): att.append(cell); continue
                x = inches(cell)
                if x is None: notes.append(f'{msid}: odd attempt {cell!r} for {who} {evname}'); ok = False; break
                att.append(fmt(x))
            if (msid, nm, evname) in OVERRIDE_FIRST4: att = att[:4]   # owner decision: row contaminated after attempt 4
            if ok and any(a for a in att): sheets.append(f'{gender}|{evname}|{nm}|{";".join(att)}')
    d = os.path.join(here, 'data-live', str(dbid))
    with open(os.path.join(d, 'sheets.txt'), 'w') as f: f.write('\n'.join(sheets) + ('\n' if sheets else ''))
    with open(os.path.join(d, 'bars.txt'), 'w') as f: f.write('\n'.join(bars) + ('\n' if bars else ''))
    mrow = q(f"SELECT Name, CONVERT(varchar(10), Date, 120) FROM Meets WHERE Id={dbid}")[0]
    with open(os.path.join(d, 'meta.txt'), 'w') as f: f.write(f'{mrow[0]}|{mrow[1]}\n')
    with open(os.path.join(d, 'rounds.txt'), 'w') as f:
        for (g, tev, nm), rr in sorted(rounds.items()):
            if len(rr) >= 2: f.write(f"{g}|{tev}|{nm}|" + ';'.join(f'{k}:{v}' for k, v in sorted(rr.items())) + '\n')
    with open(os.path.join(d, 'db.txt'), 'w') as f:
        for i, g, evn, n, t, dd, cnt, rs in perfs: f.write(f'{i}|{g}|{evn}|{n}|{t}|{dd}|{cnt}|{rs}\n')
    print(f'{msid} -> db {dbid}: {len(sheets)} series, {len(bars)} ladders')
for n in notes: print('NOTE', n)
