"""Builds docs/imports/ne-district-2005-2010.sql from baumspage Hy-Tek district results (parse_hytek.py) for the
years the DB had no district meet. Read-only lookups against the local container. Run: python3 -I build_district_sql.py > review.txt"""
import json, re, subprocess, collections, sys
from parse_hytek import parse, parse_mark, norm_event as norm

def q(sql):
    out = subprocess.run(['docker','exec','-i','ctf-local-sql','/opt/mssql-tools18/bin/sqlcmd','-C','-S','localhost','-d','ctf','-U','sa','-P','Local#Dev1234','-W','-s','|','-h','-1'],
                         input='SET NOCOUNT ON;'+sql, capture_output=True, text=True).stdout
    return [l.split('|') for l in out.strip().split('\n') if l.strip()]

YEARS = {  # year: (end date, season id, url)
    2005: ('2005-05-21', 9,  'https://www.athletic.net/TrackAndField/meet/670398'),
    2006: ('2006-05-19', 30, 'https://www.athletic.net/TrackAndField/meet/670405'),
    2008: ('2008-05-23', 3,  'https://www.baumspage.com/tf/ne/2008/ncanton/res08.htm'),
    2009: ('2009-05-22', 20, 'https://www.athletic.net/TrackAndField/meet/84587'),
    2010: ('2010-05-21', 13, 'https://www.athletic.net/TrackAndField/meet/112489'),
}
ALIAS = {'Cassy Maxwell': 'Cassandra Maxwell', 'Neil Berentz': 'Neal Berentz', 'Rick Morrison': 'Richard Morrison', 'Stacy Porter': 'Stacey Porter',
         'Brittany Pekar': 'Brittani Pekar', 'William Simpson': 'Billy Simpson', 'Garrett Livengood': 'Garrett Livingood', 'Jeff Sharrat': 'Jeff Sharratt',
         'Joe Bruening': 'Joey Bruening', 'Mackenna Huff': 'Mckenna Huff', 'R.J. Bechtel': 'RJ Bechtel', 'RJ Becthel': 'RJ Bechtel', 'Vinnie Kacinari': 'Vincent Kacinari'}
NEW = {  # athletes not in the DB: (first, last, gender, grad year)
    'Amber Hansen': ('Amber', 'Hansen', 2, 2007), "Morgan O'Rourke": ('Morgan', "O'Rourke", 2, 2007)}
events = {(int(g), n): int(i) for i, n, g in q("SELECT Id, Name, Gender FROM Events WHERE Environment = 1 AND Gender IN (1,2)")}
ath = collections.defaultdict(list)
for i, f, l, g in q("SELECT Id, FirstName, LastName, Gender FROM Athletes"): ath[(f.lower() + ' ' + l.lower(), int(g))].append(int(i))
sql_esc = lambda s: s.replace("'", "''")
review, out = [], ["""-- Northeast Ohio Division I District (North Canton) 2005, 2006, 2008, 2009, 2010: meets missing from the DB (BRAIN.md [C71]).
-- Source: baumspage.com Hy-Tek results (scripts/crawl/baumspage/ne-YYYY-res.txt); Cloverleaf rows only. The meet date is the LAST day of the
-- meet, matching the 2003 / 2004 / 2007 / 2011 district rows. Each year is skipped if a meet with that name and date already exists.
-- Best mark per athlete-event is the Performance; prelim/semi/final times become PerformanceRounds (2+ rounds); athletes with no legal mark
-- (ND / NH / DQ / DNF) become ResultStatus rows. NT (no time) rows are ignored.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT, @p INT;
"""]
def aid(name, g):
    nm = ALIAS.get(name, name)
    if name in NEW: return None, name   # created by the SQL itself: always referenced by name, never by a baked-in id
    c = ath.get((nm.lower(), g))
    if c and len(c) == 1: return c[0], nm
    if name in NEW: return None, name
    return 'MISSING', name

FIELD = ('High Jump', 'Long Jump', 'Pole Vault', 'Shot Put', 'Discus', 'Triple Jump')
EVH = re.compile(r'^(?:Event\s+\d+\s+)?(Girls|Boys)\s+((?:\d|4x).*?|High Jump|Long Jump|Pole Vault|Shot Put|Discus.*|Triple Jump)\s*$')
ROWR = re.compile(r'^\s*(\d+|--)\s+(.+?)\s+(\d{1,2})\s+([A-Za-z.\' &-]*?)\s{2,}(\S+)')
def unrecorded_events(path):
    """Field events where the meet only recorded roughly the top 8 (<= 10 rows with a mark) and everyone else is ND/NH: those
    no-mark rows mean 'not recorded', not 'no mark', so they must NOT become NM/NH results."""
    ev, cnt = None, collections.defaultdict(lambda: [0, 0])
    for l in open(path, encoding='utf-8', errors='replace'):
        m = EVH.match(l.strip())
        if m: ev = (m.group(1), norm(m.group(2))); continue
        r = ROWR.match(l)
        if ev and r and ev[1] in FIELD: cnt[ev][1 if r.group(5).lstrip('J') in ('ND', 'NH', 'NM', 'DNF', 'NT', 'FS') else 0] += 1
    return {k for k, (marked, nomark) in cnt.items() if marked <= 10 and nomark >= 2}
used_ids = {}
stats = {}
for year, (date, season, url) in YEARS.items():
    rows = parse(f'baumspage/ne-{year}-res.txt')
    unrec = unrecorded_events(f'baumspage/ne-{year}-res.txt')
    meetname = f'{year} Division I District Track Meet'
    body = []
    groups = collections.OrderedDict()
    for r in rows:
        # one Cloverleaf relay per event: merge its rounds, taking the leg names from whichever round lists all four
        key = (r['gender'], r['event'], 'RELAY', 'relay') if r['kind'] == 'relay' else (r['gender'], r['event'], tuple((a[0]) for a in r['athletes']), r['kind'])
        groups.setdefault(key, []).append(r)
    n_perf = n_nm = n_rounds = n_skip = 0
    newath = set()
    for (gender, evname, names, kind), rs in groups.items():
        if kind == 'relay':
            full = [x['athletes'] for x in rs if len(x['athletes']) == 4]
            names = tuple(a[0] for a in full[0]) if full else ()
        g = 2 if gender == 'Girls' else 1
        eid = events.get((g, evname))
        if not eid: review.append(f'{year}: no event for {gender} {evname}'); continue
        isfield = evname in ('High Jump', 'Long Jump', 'Pole Vault', 'Shot Put', 'Discus', 'Triple Jump')
        vals = {}
        status = None
        for r in rs:
            k, v = parse_mark(r['mark'])
            if k in ('time', 'dist'): vals[r['round']] = v
            elif k == 'status' and v in ('ND', 'NH', 'DQ', 'DNF', 'NM'): status = v
        ids = []
        bad = False
        for nme in names:
            a, nm = aid(nme, g)
            if a == 'MISSING': review.append(f'{year}: athlete not resolved: {nme}'); bad = True
            ids.append((a, nm, nme))
        if bad: n_skip += 1; continue
        if kind == 'relay' and len(ids) != 4: review.append(f'{year}: relay without 4 legs: {evname} {names}'); n_skip += 1; continue
        if not vals and status is None: continue  # NT / scratch only
        if not vals and (gender, evname) in unrec:
            review.append(f'{year}: skipped (top-8-only event, mark not recorded): {gender} {evname} {", ".join(names)} {status}'); n_skip += 1; continue
        ref = lambda a, nm, raw: (f"(SELECT Id FROM Athletes WHERE FirstName = '{sql_esc(NEW[raw][0])}' AND LastName = '{sql_esc(NEW[raw][1])}' AND Gender = {NEW[raw][2]})" if a is None else str(a))
        for a, nm, raw in ids:
            if a is None: newath.add(raw)
        if vals:
            best = max(vals.values()) if isfield else min(vals.values())
            col_t, col_d = ('NULL', repr(round(best, 4))) if isfield else (repr(round(best, 4)), 'NULL')
            rs_status = 0
        else:
            col_t = col_d = 'NULL'
            rs_status = {'NM': 1, 'ND': 1, 'NH': 2, 'DNF': 3, 'DQ': 4}[status]
            if (status in ('ND',) and not isfield) or (status == 'NH' and evname not in ('High Jump', 'Pole Vault')): review.append(f'{year}: odd status {status} for {evname} {names}')
        for a, nm, raw in ids:
            if a is not None: used_ids[a] = nm
        athcol = 'NULL' if kind == 'relay' else ref(*ids[0])
        who = ' / '.join(i[2] for i in ids)
        body.append(f"-- {gender} {evname}: {who}  " + (', '.join(f'{k} {v}' for k, v in vals.items()) if vals else status))
        body.append(f"INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ({athcol}, @m, {eid}, {col_d}, {col_t}, 0, 0, 0, {rs_status});")
        body.append("SET @p = SCOPE_IDENTITY();")
        if kind == 'relay':
            for a, nm, raw in ids: body.append(f"INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, {ref(a, nm, raw)});")
        if vals: n_perf += 1
        else: n_nm += 1
        if not isfield and len(vals) >= 2:
            for rn, rname in ((1, 'prelim'), (2, 'semi'), (3, 'final')):
                if rname in vals:
                    body.append(f"INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, {rn}, {repr(round(vals[rname], 4))});"); n_rounds += 1
    head = [f"""-- ===== {meetname} =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '{meetname}' AND Date = '{date}')
BEGIN"""]
    for raw in sorted(newath):
        f, l, g, gy = NEW[raw]
        head.append(f"IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = '{sql_esc(f)}' AND LastName = '{sql_esc(l)}' AND Gender = {g}) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('{sql_esc(f)}', '{sql_esc(l)}', {g}, {gy}, 0);")
    head.append(f"INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('{meetname}', '{date}', 24, 1, 0, {season}, 3, 1, 1, '{url}');\nSET @m = SCOPE_IDENTITY();")
    out += head + body + ['END', '']
    stats[year] = (n_perf, n_nm, n_rounds, n_skip)
chk = ' OR '.join(f"(Id = {i} AND FirstName = '{sql_esc(n.split(' ', 1)[0])}' AND LastName = '{sql_esc(n.split(' ', 1)[1])}')" for i, n in sorted(used_ids.items()))
out[0] = out[0].replace('DECLARE @m INT, @p INT;\n', 'DECLARE @m INT, @p INT;\n' + f"-- every baked-in athlete Id must still be the same person (guards against running on a DB whose Ids differ)\nIF (SELECT COUNT(*) FROM Athletes WHERE {chk}) <> {len(used_ids)} THROW 50010, 'Athlete Ids do not match this database', 1;\n")
out += ['EXEC sp_RebuildLeaderboards;', 'COMMIT TRANSACTION;', '']
open('../../docs/imports/ne-district-2005-2010.sql', 'w').write('\n'.join(out))
for y, s in stats.items(): print(y, 'marks %d, no-mark %d, rounds %d, skipped %d' % s)
print('\n'.join(review) or 'no review items')
