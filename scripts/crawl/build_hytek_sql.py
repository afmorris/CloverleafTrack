"""Builds an import .sql for single-day meets from baumspage Hy-Tek result text (parse_hytek2.py). Same rules as build_district_sql.py
(best mark = Performance, rounds -> PerformanceRounds, ND/NH in top-8-only field events are skipped, relays need 4 named legs).
Unmatched athletes are created only when the page gives a grade (graduation year = season end year + 12 - grade); otherwise REVIEW.
Usage: python3 -I build_hytek_sql.py <out.sql>   (edit MEETS)"""
import re, subprocess, sys, collections
sys.path.insert(0, '.')
from parse_hytek2 import parse as parse_txt
import parse_an
from parse_hytek import parse_mark

BAUMS = [  # name, date, season end year, location id, season id, text file, source url, meet type, meet level
    ('2007 Husky Invitational', '2007-04-14', 2007, 37, 10, 'baumspage/northwestern-res07.txt', 'https://www.baumspage.com/track/northwestern/res07.htm', 1, 1),
    ('2008 Wolverine Invitational', '2008-04-04', 2008, 56, 3, 'baumspage/westgeauga-res08.txt', 'https://www.baumspage.com/track/westgeauga/res08.htm', 1, 1),
    ('2011 Optimist Meet', '2011-05-07', 2011, 4, 45, 'baumspage/optimist-res11.txt', 'https://www.baumspage.com/track/optimist/res11.htm', 1, 1),
]
NORWAYNE = ("Norwayne High School", "Creston")
JACKSON = ("Jackson High School", "Massillon")
MS = [  # location is an id or a (name, city) tuple created by the SQL
    ('2017 Dwight Schar Invitational', '2017-05-05', 2017, NORWAYNE, 40, 'milesplit-raw/ms-280388.txt', 'https://oh.milesplit.com/meets/280388', 3, 1),
    ('2017 Jackson Twilight Invitational', '2017-05-05', 2017, JACKSON, 40, 'milesplit-raw/ms-280389.txt', 'https://oh.milesplit.com/meets/280389', 3, 1),
    ('2018 New London Relays', '2018-04-07', 2018, 35, 1, 'milesplit-raw/ms-311971.txt', 'https://oh.milesplit.com/meets/311971', 3, 1),
    ('2019 New London Relays', '2019-04-06', 2019, 35, 15, 'milesplit-raw/ms-352490.txt', 'https://oh.milesplit.com/meets/352490', 3, 1),
    ('2019 Ravenna-Streetsboro Double Dual', '2019-04-16', 2019, 45, 15, 'milesplit-raw/ms-354958.txt', 'https://oh.milesplit.com/meets/354958', 2, 1),
    ('2019 Triway Invitational', '2019-04-20', 2019, 52, 15, 'milesplit-raw/ms-357355.txt', 'https://oh.milesplit.com/meets/357355', 3, 1),
    ('2019 Smithie Frosh-Soph Invitational', '2019-05-06', 2019, 48, 15, 'milesplit-raw/ms-359347.txt', 'https://oh.milesplit.com/meets/359347', 3, 2),
]
AN = [
    ('2009 Mansfield Mehock Relays', '2009-04-17', 2009, 32, 20, 'an-rows/an-444526.json', 'https://www.athletic.net/TrackAndField/meet/444526', 3, 1),
    ('2012 Optimist Meet', '2012-05-05', 2012, 4, 17, 'an-rows/an-157758.json', 'https://www.athletic.net/TrackAndField/meet/157758', 3, 1),
    ('2016 Optimist Meet', '2016-05-07', 2016, 4, 5, 'an-rows/an-275382.json', 'https://www.athletic.net/TrackAndField/meet/275382', 3, 1),
    ('2024 Optimist Meet', '2024-05-04', 2024, 4, 4, 'milesplit-raw/ms-582440.txt', 'https://oh.milesplit.com/meets/582440', 3, 1),
]
INDOOR = [  # environment 2
    ('2025 Kent State HS Meet #5', '2025-02-23', 2025, 28, 27, 'an-rows/an-587896.json', 'https://www.athletic.net/TrackAndField/meet/587896', 3, 1, 2),
    ('2025 Akron Rust Buster HS Open', '2025-12-13', 2026, 1, 47, 'an-rows/an-626478.json', 'https://www.athletic.net/TrackAndField/meet/626478', 3, 1, 2),
    ('2026 Akron Rubber City HS Open', '2026-01-17', 2026, 1, 47, 'an-rows/an-631604.json', 'https://www.athletic.net/TrackAndField/meet/631604', 3, 1, 2),
    ('2026 Kent State HS Meet #1', '2026-01-24', 2026, 28, 47, 'an-rows/an-632988.json', 'https://www.athletic.net/TrackAndField/meet/632988', 3, 1, 2),
    ('2026 Ashland University HS Light Giver Open', '2026-02-08', 2026, 3, 47, 'an-rows/an-636245.json', 'https://www.athletic.net/TrackAndField/meet/636245', 3, 1, 2),
]
MEETS = {'baums': BAUMS, 'ms': MS, 'an': AN, 'indoor': INDOOR}[sys.argv[2] if len(sys.argv) > 2 else 'baums']
ALIAS = {'Cassy Maxwell': 'Cassandra Maxwell', 'Neil Berentz': 'Neal Berentz', 'Rick Morrison': 'Richard Morrison', 'Ricky Morrison': 'Richard Morrison', 'Stacy Porter': 'Stacey Porter',
         'Brittany Pekar': 'Brittani Pekar', 'William Simpson': 'Billy Simpson', 'Garrett Livengood': 'Garrett Livingood', 'Garrett Livingood': 'Garrett Livingood', 'Jeff Sharrat': 'Jeff Sharratt',
         'Joe Bruening': 'Joey Bruening', 'Mackenna Huff': 'Mckenna Huff', 'R.J. Bechtel': 'RJ Bechtel', 'RJ Becthel': 'RJ Bechtel', 'Vinnie Kacinari': 'Vincent Kacinari',
         'Katelyn Donnelly': 'Katlyn Donnelly', 'Taylor Sachleban': 'Taylor Sachleben', 'T.J. Corbett': 'TJ Corbett', 'Bryce Neil': 'Bryce Neill', 'Asa Gyn': 'Asa Gynn', 'Isaiah Curt': 'Isaiah Kurtz', 'Josh Bohemian': 'Josh Bowman', 'Jacob Knau': 'Jacob Nau', 'Meagan Van Meter': 'Megan Van Meter'}
BYNAME = {'Amber Hansen', "Morgan O'Rourke"}   # created by an earlier import SQL, so their Ids differ between databases: always reference by name
FIELD = ('High Jump', 'Long Jump', 'Pole Vault', 'Shot Put', 'Discus', 'Triple Jump')

def parse(path):
    return parse_an.parse(path) if path.endswith('.json') else parse_txt(path)

def q(sql):
    out = subprocess.run(['docker', 'exec', '-i', 'ctf-local-sql', '/opt/mssql-tools18/bin/sqlcmd', '-C', '-I', '-S', 'localhost', '-d', 'ctf', '-U', 'sa', '-P', 'Local#Dev1234', '-W', '-s', '|', '-h', '-1'],
                         input='SET NOCOUNT ON;' + sql, capture_output=True, text=True).stdout
    return [l.split('|') for l in out.strip().split('\n') if l.strip()]

events = {(int(e), int(g), n): int(i) for i, n, g, e in q("SELECT Id, Name, Gender, Environment FROM Events WHERE Gender IN (1,2)")}
ath = collections.defaultdict(list)
for i, f, l, g in q("SELECT Id, FirstName, LastName, Gender FROM Athletes"): ath[(f.lower() + ' ' + l.lower(), int(g))].append(int(i))
esc = lambda s: s.replace("'", "''")
EVH = re.compile(r'^(?:Event\s+\d+\s+)?(Girls|Boys|Women|Men)\s+(.+?)\s*$')
from parse_hytek import norm_event

def unrecorded(path):
    ev, cnt = None, collections.defaultdict(lambda: [0, 0])
    for l in open(path, encoding='utf-8', errors='replace'):
        m = EVH.match(l.strip())
        if m and (norm_event(m.group(2)) in FIELD or re.match(r'\d|4x', m.group(2))): ev = ({'Women': 'Girls', 'Men': 'Boys'}.get(m.group(1), m.group(1)), norm_event(m.group(2))); continue
        if ev and ev[1] in FIELD and re.match(r'^\s*(\d+|--)\s+\S', l):
            last = l.split()[-1] if l.split() else ''
            toks = ' '.join(l.split()[-4:])
            cnt[ev][1 if re.search(r'\b(ND|NH|NM|FOUL|FS)\b', toks) else 0] += 1
    return {k for k, (marked, nomark) in cnt.items() if 8 <= marked <= 10 and nomark >= 2}

gy_global = {}   # (name) -> graduation year, from any meet in this run that gives the athlete's grade
for _n, _d, _y, _l, _s, _p, _u, _t, _lv, *_ in MEETS:
    for _r in parse(_p):
        for _nm, _gr in _r['athletes']:
            if _gr and _nm not in gy_global: gy_global[_nm] = _y + 12 - _gr
review, out, stats = [], ["-- Single-day meets from baumspage Hy-Tek results (BRAIN.md [C72]); Cloverleaf rows only. Each meet is skipped if a meet with that name and date exists.\n-- Rules as in ne-district-2005-2010.sql. Athlete Ids are asserted before anything is inserted.\nSET XACT_ABORT ON;\nBEGIN TRANSACTION;\nDECLARE @m INT, @p INT;\n"], {}
used_ids, newathletes = {}, {}
body_all = []
for name, date, yend, loc, season, path, url, mtype, mlevel, *rest in MEETS:
    env = rest[0] if rest else 1
    rows = parse(path)
    unrec = parse_an.unrecorded(path) if path.endswith('.json') else unrecorded(path)
    grades = {}   # a grade given for an athlete in any row of the meet is used for all of their rows
    for r in rows:
        for nm_, gr_ in r['athletes']:
            if gr_: grades[nm_] = gr_
    groups = collections.OrderedDict()
    teams = collections.defaultdict(set)     # (gender, event) -> {frozenset(legs)} : a school can enter several relay teams in one event
    for r in rows:
        if r['kind'] == 'relay' and len(r['athletes']) == 4: teams[(r['gender'], r['event'])].add(frozenset(a[0] for a in r['athletes']))
    for r in rows:
        if r['kind'] == 'relay':
            legs = frozenset(a[0] for a in r['athletes']) if len(r['athletes']) == 4 else None
            t = teams.get((r['gender'], r['event']), set())
            if legs is None and len(t) == 1: legs = next(iter(t))        # a round without leg names belongs to the event's only known team
            key = (r['gender'], r['event'], 'RELAY' + ('' if legs is None else '|' + '|'.join(sorted(legs))), 'relay')
        else: key = (r['gender'], r['event'], r['athletes'][0][0], 'ind')
        groups.setdefault(key, []).append(r)
    body, n_marks, n_nm, n_rounds, n_skip = [], 0, 0, 0, 0
    newhere = {}
    for (gender, evname, who, kind), rs in groups.items():
        g = 2 if gender == 'Girls' else 1
        eid = events.get((env, g, evname))
        if not eid: review.append(f'{name}: no event for {gender} {evname}'); continue
        isfield = evname in FIELD
        vals, status = {}, None
        for r in rs:
            k, v = parse_mark(r['mark'])
            if k in ('time', 'dist'): vals[r['round']] = v
            elif k == 'status' and v in ('ND', 'NH', 'DQ', 'DNF', 'NM'): status = v
            elif r['mark'].upper() in ('FOUL', 'FS') and isfield: status = 'ND'
        if kind == 'relay':
            full = [x['athletes'] for x in rs if len(x['athletes']) == 4]
            people = full[0] if full else []
        else: people = rs[0]['athletes']
        if kind == 'relay' and len(people) != 4: review.append(f'{name}: relay without 4 named legs: {gender} {evname} {sorted(vals.items())}'); n_skip += 1; continue
        refs, bad = [], False
        for nm, grade in people:
            grade = grade or grades.get(nm)
            full_name = ALIAS.get(nm, nm)
            c = ath.get((full_name.lower(), g), [])
            if len(c) == 1 and full_name in BYNAME:
                f_, _, l_ = full_name.partition(' ')
                refs.append(f"(SELECT Id FROM Athletes WHERE FirstName = '{esc(f_)}' AND LastName = '{esc(l_)}' AND Gender = {g})"); continue
            if len(c) == 1: refs.append(str(c[0])); used_ids[c[0]] = full_name; continue
            if grade or nm in gy_global:
                gy = yend + 12 - grade if grade else gy_global[nm]
                first, _, last = nm.partition(' ')
                newhere[(first, last, g)] = gy
                refs.append(f"(SELECT Id FROM Athletes WHERE FirstName = '{esc(first)}' AND LastName = '{esc(last)}' AND Gender = {g})")
            else:
                review.append(f'{name}: athlete not in DB and no grade given: {nm} ({gender} {evname})'); bad = True
        if bad: n_skip += 1; continue
        if not vals and status is None: continue
        if not vals and (gender, evname) in unrec:
            review.append(f'{name}: skipped (top-8-only event, mark not recorded): {gender} {evname} {who} {status}'); n_skip += 1; continue
        if vals:
            best = max(vals.values()) if isfield else min(vals.values()); rs_status = 0
            col_t, col_d = ('NULL', repr(round(best, 4))) if isfield else (repr(round(best, 4)), 'NULL')
        else:
            col_t = col_d = 'NULL'; rs_status = {'ND': 1, 'NM': 1, 'NH': 2, 'DNF': 3, 'DQ': 4}[status]
        body.append(f"-- {gender} {evname}: {who if kind != 'relay' else ' / '.join(p[0] for p in people)}  " + (', '.join(f'{k} {v}' for k, v in vals.items()) if vals else status))
        body.append(f"INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ({'NULL' if kind == 'relay' else refs[0]}, @m, {eid}, {col_d}, {col_t}, 0, 0, 0, {rs_status});")
        body.append("SET @p = SCOPE_IDENTITY();")
        if kind == 'relay':
            for r_ in refs: body.append(f"INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, {r_});")
        if vals: n_marks += 1
        else: n_nm += 1
        if not isfield and len(vals) >= 2:
            for rn, rname in ((1, 'prelim'), (2, 'semi'), (3, 'final')):
                if rname in vals: body.append(f"INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, {rn}, {repr(round(vals[rname], 4))});"); n_rounds += 1
    head = [f"-- ===== {name} =====\nIF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '{name}' AND Date = '{date}')\nBEGIN"]
    for (first, last, g), gy in sorted(newhere.items()):
        head.append(f"IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = '{esc(first)}' AND LastName = '{esc(last)}' AND Gender = {g}) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('{esc(first)}', '{esc(last)}', {g}, {gy}, 0);")
        newathletes[(first, last, g)] = gy
    if isinstance(loc, tuple):
        head.append(f"IF NOT EXISTS (SELECT 1 FROM Locations WHERE Name = '{esc(loc[0])}') INSERT INTO Locations (Name, City, State, Country) VALUES ('{esc(loc[0])}', '{esc(loc[1])}', 'OH', 'USA');")
        locsql = f"(SELECT Id FROM Locations WHERE Name = '{esc(loc[0])}')"
    else: locsql = str(loc)
    head.append(f"INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('{name}', '{date}', {locsql}, {env}, 0, {season}, 3, {mlevel}, {mtype}, '{url}');\nSET @m = SCOPE_IDENTITY();")
    body_all += head + body + ['END', '']
    stats[name] = (n_marks, n_nm, n_rounds, n_skip)
chk = ' OR '.join(f"(Id = {i} AND FirstName = '{esc(n.split(' ', 1)[0])}' AND LastName = '{esc(n.split(' ', 1)[1])}')" for i, n in sorted(used_ids.items()))
out[0] += f"IF (SELECT COUNT(*) FROM Athletes WHERE {chk}) <> {len(used_ids)} THROW 50010, 'Athlete Ids do not match this database', 1;\n"
out += body_all + ['EXEC sp_RebuildLeaderboards;', 'COMMIT TRANSACTION;', '']
open(sys.argv[1], 'w').write('\n'.join(out))
for k, v in stats.items(): print(k, 'marks %d, no-mark %d, rounds %d, skipped %d' % v)
print('new athletes:', newathletes)
print('\n'.join(review) or 'no review items')
