"""docs/imports/marks-from-milesplit.sql: take MileSplit's (official) mark where the DB differs by a small amount (<= 3.5% of a time or <= 6 inches),
except when the DB row has rounds (it already knows the prelim/final split), or when MileSplit only has a prelim row for the athlete (its final is missing).
Each UPDATE is guarded by the old value; the old values are logged in BRAIN.md [C76]."""
import json, subprocess, sys
def q(sql):
    out = subprocess.run(['docker','exec','-i','ctf-local-sql','/opt/mssql-tools18/bin/sqlcmd','-C','-I','-S','localhost','-d','ctf','-U','sa','-P','Local#Dev1234','-W','-s','|','-h','-1'],input='SET NOCOUNT ON;'+sql,capture_output=True,text=True).stdout
    return [l.split('|') for l in out.strip().split('\n') if l.strip()]
esc = lambda s: s.replace("'", "''")
ath = {int(i): (f, l) for i, f, l in q("SELECT Id, FirstName, LastName FROM Athletes")}
ev = {int(i): (n, int(g)) for i, n, g in q("SELECT Id, Name, Gender FROM Events")}
mt = {int(i): (n, d) for i, n, d in q("SELECT Id, Name, CONVERT(varchar(10), Date, 120) FROM Meets")}
rounds = {int(p) for (p,) in q("SELECT DISTINCT PerformanceId FROM PerformanceRounds")}
ups = json.load(open('results-mismatch-ms.json'))
chosen, skipped = [], []
for db, e, a, v, dv, pid, fld, rds in ups:
    small = abs(v - dv) / max(v, dv) <= 0.035 if not fld else abs(v - dv) <= 6
    if not small: continue
    if pid in rounds: skipped.append((db, e, a, v, dv, 'DB row has rounds')); continue
    if rds == ['prelim']: skipped.append((db, e, a, v, dv, 'MileSplit has only a prelim row')); continue
    chosen.append((db, e, a, v, dv, pid, fld))
used_a = sorted({a for _, _, a, *_ in chosen})
out = ["-- Official (MileSplit) marks replace slightly different DB marks (BRAIN.md [C76]); each UPDATE is guarded by the old value and skipped if the row has rounds.\nSET XACT_ABORT ON;\nBEGIN TRANSACTION;"]
out.append("IF (SELECT COUNT(*) FROM Athletes WHERE " + ' OR '.join(f"(Id = {i} AND FirstName = '{esc(ath[i][0])}' AND LastName = '{esc(ath[i][1])}')" for i in used_a) + f") <> {len(used_a)} THROW 50010, 'Athlete Ids do not match this database', 1;")
for db, e, a, v, dv, pid, fld in chosen:
    col = 'DistanceInches' if fld else 'TimeSeconds'
    m = f"(SELECT Id FROM Meets WHERE Name = '{esc(mt[db][0])}' AND Date = '{mt[db][1]}')"
    out.append(f"-- {mt[db][1]} {mt[db][0]} | {ev[e][0]} | {ath[a][0]} {ath[a][1]}: {dv:.2f} -> {v:.2f}")
    out.append(f"UPDATE Performances SET {col} = {repr(round(v, 4))} WHERE MeetId = {m} AND EventId = {e} AND AthleteId = {a} AND ABS({col} - {repr(round(dv, 4))}) < 0.001\n    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);")
out += ['EXEC sp_RebuildLeaderboards;', 'COMMIT TRANSACTION;', '']
open('../../docs/imports/marks-from-milesplit.sql', 'w').write('\n'.join(out))
print(len(chosen), 'updates;', len(skipped), 'skipped')
for s in skipped: print('  skipped', mt[s[0]][1], ev[s[1]][0], ath[s[2]], round(s[3], 2), round(s[4], 2), s[5])
print('LOG')
for db, e, a, v, dv, pid, fld in chosen: print(f"  {mt[db][1]} {mt[db][0][5:35]} | {ev[e][0]} | {ath[a][0]} {ath[a][1]}: {dv:.2f} -> {v:.2f}")
