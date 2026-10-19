"""Turns ms-no-marks-raw.txt / an-no-marks-raw.txt (rows: meetId|event header|raw line; MileSplit and athletic.net ids) into docs/imports/no-marks-2005-2025.sql.
Needs the local SQL container (ctf-local-sql) for read-only lookups. Anything it cannot place with certainty is printed
as REVIEW and left out of the SQL. Run: python3 -I build_no_mark_sql.py > review.txt"""
import csv, re, subprocess, sys, collections

def q(sql):
    out = subprocess.run(['docker','exec','-i','ctf-local-sql','/opt/mssql-tools18/bin/sqlcmd','-C','-S','localhost','-d','ctf','-U','sa','-P','Local#Dev1234','-W','-s','|','-h','-1'],
                         input='SET NOCOUNT ON;'+sql, capture_output=True, text=True).stdout
    return [l.split('|') for l in out.strip().split('\n') if l.strip()]

ms2db = {}
for r in csv.DictReader(open('matches.tsv'), delimiter='\t'):
    if r['dbMeetId']: ms2db[('ms' if r['source'] == 'milesplit' else 'an') + ':' + r['meetId']] = int(r['dbMeetId'])
meets = {int(i): (int(env), d) for i, env, d in q("SELECT Id, Environment, CONVERT(varchar(10),Date,120) FROM Meets")}
events = collections.defaultdict(list)  # (gender, name, env) -> id
for i, n, g, e in q("SELECT Id, Name, Gender, Environment FROM Events WHERE EventType IN (0,1)"):
    events[(int(g), n, int(e))].append(int(i))
athletes = collections.defaultdict(list)
for i, f, l, g in q("SELECT Id, FirstName, LastName, Gender FROM Athletes"):
    athletes[(f.lower(), l.lower(), int(g))].append(int(i))
existing = {(int(m), int(e), int(a)) for m, e, a in q("SELECT MeetId, EventId, AthleteId FROM Performances WHERE AthleteId IS NOT NULL")}
ALIAS = {'addison': 'addi', 'will': 'william'}
EVMAP = [('high jump', 'High Jump'), ('pole vault', 'Pole Vault'), ('discus', 'Discus'), ('shot put', 'Shot Put'), ('1600', '1600M'), ('100 meters', '100M')]
STATUS = {'ND': 1, 'NH': 2, 'DNF': 3, 'DQ': 4}
rows, review = [], []
lines = [('ms:' + l) for l in open('ms-no-marks-raw.txt')] + [('an:' + l) for l in open('an-no-marks-raw.txt')]
for line in lines:
    mid, header, raw = line.rstrip('\n').split('|', 2)
    if mid not in ms2db: review.append(f'{mid}: no DB meet match: {raw}'); continue
    dbm = ms2db[mid]; env = meets[dbm][0]
    h = header.lower()
    if 'relay' in h or 'wheelchair' in h or 'seated' in h: review.append(f'{dbm}: skipped relay/seated: {header} / {raw}'); continue
    gender = 2 if 'girls' in h else 1 if 'boys' in h else None
    evname = next((v for k, v in EVMAP if k in h), None)
    tok = re.search(r'\b(ND|NH|DNF|DQ)\b', raw); status = STATUS[tok.group(1)]
    nm = re.sub(r'^(--+|scr)\s+', '', raw.split(' Cloverleaf')[0].strip()); nm = re.sub(r'^#\s*\d+\s+', '', nm)
    nm = re.sub(r'\s+\d{1,2}$', '', nm)
    if ',' in nm: last, first = [s.strip() for s in nm.split(',', 1)]
    else: first, _, last = nm.partition(' ')
    first_l, last_l = first.lower(), last.lower().title().lower()
    if gender is None or evname is None: review.append(f'{dbm}: cannot parse event: {header} / {raw}'); continue
    ev = events.get((gender, evname, env)) or []
    if len(ev) != 1: review.append(f'{dbm}: no unique event for {header} env={env}: {raw}'); continue
    cands = athletes.get((first_l, last_l, gender)) or athletes.get((ALIAS.get(first_l, first_l), last_l, gender)) or []
    if len(cands) != 1: review.append(f'{dbm}: athlete not unique/not found ({len(cands)}): {first} {last} g={gender} / {header}'); continue
    if (dbm, ev[0], cands[0]) in existing: continue
    rows.append((dbm, ev[0], evname, gender, cands[0], first, last, status, mid))
seen, out = set(), []
for r in sorted(rows):
    if r[:5] in seen: continue
    seen.add(r[:5]); out.append(r)
sql = ["""-- No-mark results (NM / NH / DNF) from MileSplit raw results (see BRAIN.md [C67], [C68]). Rows are the ones whose athlete has NO
-- Performance for that event at that meet; a stored mark is never touched. NH/ND lines with a seed mark still have no result.
-- Requires docs/migration_performance_result_status.sql. Athlete / event ids are asserted per block.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
"""]
for dbm, e, en, g, a, f, l, s, mid in out:
    # name is asserted loosely (first-name aliases like Addison/Addi exist), so assert on Id + LastName
    sql.append(f"""-- {f} {l}: status {s} in {en} at meet {dbm} ({mid})
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = {a} AND LastName = '{l.title().replace("'", "''")}') THROW 50001, 'Athlete {a} is not {l}', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = {e} AND Name = '{en}' AND Gender = {g}) THROW 50002, 'Event {e} is not {en}', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = {dbm} AND EventId = {e} AND AthleteId = {a})
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES ({a}, {dbm}, {e}, NULL, NULL, 0, 0, 0, {s});
""")
sql.append("EXEC sp_RebuildLeaderboards;\nCOMMIT TRANSACTION;\n")
open('../../docs/imports/no-marks-2005-2025.sql', 'w').write('\n'.join(sql))
print(f'{len(out)} rows to import; {len(review)} REVIEW:')
print('\n'.join('REVIEW ' + r for r in review))
