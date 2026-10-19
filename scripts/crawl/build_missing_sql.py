"""docs/imports/results-missing-ms.sql from results-missing-ms.json (compare_ms_vs_db.py): Cloverleaf results that MileSplit has for a
meet the DB already holds, but the DB lacks (same meet+event+athlete). Individuals need an exact/unique athlete match; relays need four
named, resolvable legs and no relay row in that event. Ids are asserted; every insert is guarded by NOT EXISTS."""
import json, subprocess
def q(sql):
    out = subprocess.run(['docker','exec','-i','ctf-local-sql','/opt/mssql-tools18/bin/sqlcmd','-C','-I','-S','localhost','-d','ctf','-U','sa','-P','Local#Dev1234','-W','-s','|','-h','-1'],input='SET NOCOUNT ON;'+sql,capture_output=True,text=True).stdout
    return [l.split('|') for l in out.strip().split('\n') if l.strip()]
esc = lambda s: s.replace("'", "''")
# athletes created by earlier import SQL files get different Ids in different databases: reference those by name
BYNAME = {('Amber', 'Hansen'), ('Morgan', "O'Rourke"), ('Ben', 'Nagy'), ('Bridgette', 'Donahue'), ('Caleb', 'Hill'), ('Evan', 'Allen'), ('Joey', 'Weills'), ('Jordan', 'Smith'),
          ('Megan', 'Van Meter'), ('Savanna', 'Brooks'), ('Scott', 'Stoner'), ('Sean', 'Barker'), ('Zane', 'Evans'), ('Trevor', 'Millbrandt')}
ath = {int(i): (f, l) for i, f, l in q("SELECT Id, FirstName, LastName FROM Athletes")}
gen = {int(i): int(g) for i, g in q("SELECT Id, Gender FROM Athletes")}
def ref(i): return f"(SELECT Id FROM Athletes WHERE FirstName = '{esc(ath[i][0])}' AND LastName = '{esc(ath[i][1])}' AND Gender = {gen[i]})" if ath[i] in BYNAME else str(i)
ev = {int(i): (n, int(g)) for i, n, g in q("SELECT Id, Name, Gender FROM Events")}
mt = {int(i): n for i, n in q("SELECT Id, Name FROM Meets")}
ins = json.load(open('results-missing-ms.json'))
used_a = sorted({a for k, db, e, ids, v, f in ins for a in ids if ath[a] not in BYNAME}); used_m = sorted({db for k, db, e, ids, v, f in ins}); used_e = sorted({e for k, db, e, ids, v, f in ins})
out = ["-- Cloverleaf results present on MileSplit but missing from meets the DB already has (BRAIN.md [C74]). Inserts only; never touches an existing row.\nSET XACT_ABORT ON;\nBEGIN TRANSACTION;\nDECLARE @p INT;"]
out.append("IF (SELECT COUNT(*) FROM Athletes WHERE " + ' OR '.join(f"(Id = {i} AND FirstName = '{esc(ath[i][0])}' AND LastName = '{esc(ath[i][1])}')" for i in used_a) + f") <> {len(used_a)} THROW 50010, 'Athlete Ids do not match this database', 1;")
out.append("IF (SELECT COUNT(*) FROM Meets WHERE " + ' OR '.join(f"(Id = {i} AND Name = '{esc(mt[i])}')" for i in used_m) + f") <> {len(used_m)} THROW 50011, 'Meet Ids do not match this database', 1;")
out.append("IF (SELECT COUNT(*) FROM Events WHERE " + ' OR '.join(f"(Id = {i} AND Name = '{esc(ev[i][0])}' AND Gender = {ev[i][1]})" for i in used_e) + f") <> {len(used_e)} THROW 50012, 'Event Ids do not match this database', 1;")
for kind, db, e, ids, v, fld in ins:
    col_t, col_d = ('NULL', repr(round(v, 4))) if fld else (repr(round(v, 4)), 'NULL')
    who = ', '.join(f'{ath[i][0]} {ath[i][1]}' for i in ids)
    out.append(f"-- {mt[db]}: {ev[e][0]} {who} {v}")
    if kind == 'ind':
        out.append(f"IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = {db} AND EventId = {e} AND AthleteId = {ref(ids[0])})\n    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ({ref(ids[0])}, {db}, {e}, {col_d}, {col_t}, 0, 0, 0, 0);")
    else:
        out.append(f"IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = {db} AND EventId = {e} AND AthleteId IS NULL)\nBEGIN\n    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, {db}, {e}, {col_d}, {col_t}, 0, 0, 0, 0);\n    SET @p = SCOPE_IDENTITY();")
        for i in ids: out.append(f"    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, {ref(i)});")
        out.append("END")
out += ['EXEC sp_RebuildLeaderboards;', 'COMMIT TRANSACTION;', '']
open('../../docs/imports/results-missing-ms.sql', 'w').write('\n'.join(out))
print(len(ins), 'rows')
