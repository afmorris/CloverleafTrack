#!/usr/bin/env python3
"""Builds dual-meet placings + team scores from score sheets. usage: sheets_build.py <out.sql> (run with the scratch venv: needs openpyxl).
Matches the sheets' Cloverleaf finishers to DB performances of the meet (by event + last name + mark), prints a REVIEW report (mismatches / unmatched) and writes
Id-independent SQL: MeetParticipants (opponent schools), MeetType/template, MeetPlacings (per opponent, sheet points) and MeetTeamResults (our/opponent score)."""
import os, re, sys, collections
H = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, H); sys.path.insert(0, os.path.join(H, '../places'))
from sheets_parse import load
from places_lib import q, esc, norm
D = '/Users/tony/Downloads/'
OPPNAME = {'COV': 'Coventry', 'FLD': 'Field', 'WOO': 'Woodridge', 'NOR': 'Norton', 'SPR': 'Springfield', 'NLO': 'New London'}
MEETNAME = {'2021-04-13': '2021 Woodridge Dual', '2025-04-22': '2025 Norton-Springfield Double Dual', '2024-04-23': '2024 Field-Coventry Double Dual', '2023-04-11': '2023 Woodridge Dual', '2025-04-15': '2025 Woodridge Dual'}
rows, totals = load(D + 'Cloverleaf Track Meet Score Sheet - 2024 Double Dual Coventry Field.xlsx',
                    {'CLO COV Girls': ('2024-04-23', 'COV'), 'CLO COV Boys': ('2024-04-23', 'COV'), 'CLO FLD Girls': ('2024-04-23', 'FLD'), 'CLO FLD Boys': ('2024-04-23', 'FLD')})
r2, t2 = load(D + 'Cloverleaf Track Meet Score Sheet - Woodridge.xlsx', {'Girls Meet': ('2023-04-11', 'WOO'), 'Boys Meet': ('2023-04-11', 'WOO')})
rows += r2; totals.update(t2)
# 2025-04-15 Woodridge girls (text PDF 'CLO WOO Girls'): CLO 63.33 - WOO 73.67
W25 = """Pole Vault|1|Shetterly|CLO|8'|5
Pole Vault|2|Taylor|CLO||1.33
High Jump|1|Peterson|WOO|4' 10"|5
High Jump|2|Howell|WOO|4' 8"|3
High Jump|3|Bayer|CLO|4' 6"|1
Long Jump|1|Bayer|CLO|15' 9"|5
Long Jump|2|Howell|WOO|15' 7"|3
Long Jump|3|Reinhardt|CLO|14' 9.75"|1
Shot Put|1|McConnell|CLO|36' 11"|5
Shot Put|2|JO|WOO|30' 2"|3
Shot Put|3|Ryncarz|CLO|29' 7.5"|1
Discus|1|Mayer|WOO|99' 1"|5
Discus|2|McConnell|CLO|95' 4"|3
Discus|3|Ryncarz|CLO|75' 10"|1
4x800M Relay|1|Woodridge|WOO|11:10.5|5
HURDLE_SHORT|1|Howell|WOO|17.34|5
HURDLE_SHORT|2|Berry|CLO|17.76|3
HURDLE_SHORT|3|Schlupe|WOO|19.08|1
100M|1|Kelly|CLO|12.82|5
100M|2|Shook|CLO|13.38|3
100M|3|Munchick|CLO|13.39|1
4x200M Relay|1|Cloverleaf|CLO|1:55.8|5
1600M|1|Boone|WOO|5:20.6|5
1600M|2|Mullen|WOO|5:36.5|3
1600M|3|Hibler|CLO|5:42|1
4x100M Relay|1|Cloverleaf|CLO|52.75|5
400M|1|Perry|WOO|1:03.95|5
400M|2|Wershing|CLO|1:05.92|3
400M|3|Cavey|CLO|1:07.9|1
300M Hurdles|1|Howell|WOO|50.82|5
300M Hurdles|2|Berry|CLO|54.4|3
300M Hurdles|3|McFarland|WOO|55.57|1
800M|1|Boone|WOO|2:21.1|5
800M|2|Stanford|WOO|2:35.2|3
800M|3|Hibler|CLO|2:44.6|1
200M|1|Kelly|CLO|27.7|5
200M|2|Perry|WOO|28.39|3
200M|3|Munchick|CLO|28.62|1
3200M|1|Mullen|WOO|12:04.03|5
3200M|2|Hibler|CLO|12:04.34|3
3200M|3|Singer|WOO|12:13.5|1
4x400M Relay|1|Woodridge|WOO||5"""
for l in W25.split('\n'):
    ev, pl, nm, tm_, mk, pt = l.split('|')
    rows.append(dict(meet='2025-04-15', g=2, opp='WOO', event=ev, place=int(pl), name=nm, team=tm_, mark=mk, pts=float(pt), relay='Relay' in ev))
totals[('2025-04-15', 2, 'WOO')] = (63.33, 73.67)
cur = None
for l in open(os.path.join(H, 'data/textsheets.txt')):
    l = l.rstrip('\n')
    if not l.strip() or l.startswith('#'): continue
    if l.startswith('@'):
        d, g, opp, a, b = l[1:].split('|'); cur = (d, int(g), opp); totals[(d, int(g), opp)] = (float(a), float(b)); continue
    ev, pl, nm, tm_, mk, pt = l.split('|')
    rows.append(dict(meet=cur[0], g=cur[1], opp=cur[2], event=ev, place=int(pl), name=nm, team=tm_, mark=mk, pts=float(pt), relay='Relay' in ev))
def val(ev, s):
    s = s.replace(';', ':').strip()
    m = re.match(r"^(\d+)'\s*(\d+(?:\.\d+)?)?(?:\s*(\d)/(\d))?", s)
    if m and ("'" in s):
        return int(m.group(1)) * 12 + (float(m.group(2)) if m.group(2) else 0) + (int(m.group(3)) / int(m.group(4)) if m.group(3) else 0)
    m = re.match(r'^(?:(\d+):)?(\d+(?:\.\d+)?)$', s)
    return (int(m.group(1)) * 60 if m.group(1) else 0) + float(m.group(2)) if m else None
info = {n: (int(i), d) for i, n, d in q("SELECT Id, Name, CONVERT(varchar(10), Date, 120) FROM Meets WHERE Name LIKE '%Dual%'")}
perfs = collections.defaultdict(list)
mids = {k: info[v][0] for k, v in MEETNAME.items()}
for pid, mid, g, en, fn, ln, t, d in q("SELECT p.Id, p.MeetId, e.Gender, e.Name, ISNULL(a.FirstName,''), ISNULL(a.LastName,''), ISNULL(CAST(p.TimeSeconds AS varchar(30)),''), ISNULL(CAST(p.DistanceInches AS varchar(30)),'') FROM Performances p JOIN Events e ON e.Id=p.EventId LEFT JOIN Athletes a ON a.Id=p.AthleteId WHERE p.ResultStatus=0 AND p.MeetId IN (%s)" % ','.join(str(v) for v in mids.values())):
    perfs[(int(mid), int(g), en)].append((int(pid), fn, ln, float(t) if t else None, float(d) if d else None))
review = []; out = []; used = set()
for r in rows:
    if r['team'] != 'CLO': continue
    mid, md = mids[r['meet']], info[MEETNAME[r['meet']]][1]
    ev = r['event']
    if ev == 'HURDLE_SHORT': ev = '110M Hurdles' if r['g'] == 1 else '100M Hurdles'
    c = perfs.get((mid, r['g'], ev), [])
    sv_ = val(ev, r['mark']) if r['mark'] else None
    if r['relay']:
        v = val(ev, r['mark']) if r['mark'] else None
        hit = [x for x in c if x[1] == '' and x[2] == '' and x[3] is not None and v is not None and abs(x[3] - v) < 0.6]
        who = 'relay'
    else:
        parts = {'Abby R': 'Abigail Reinhardt', 'Burnett': 'Reese Barnett', 'Begucki': 'Tristin Bogucki'}.get(r['name'], r['name']).replace('.', '').split()
        last = parts[-1]; first = parts[0] if len(parts) > 1 else ''
        if len(parts) == 2 and len(parts[1]) == 1: last, first = parts[0], ''   # 'Abby R'
        hit = [x for x in c if norm(x[2]) == norm(last) or norm(x[2]).startswith(norm(last)[:4]) and len(norm(last)) >= 4 and norm(x[2])[:4] == norm(last)[:4]]
        if first: hit = [x for x in hit if norm(x[1]).startswith(norm(first)[:1])] or hit
        if len(parts) == 2 and len(parts[1]) == 1: hit = [x for x in c if norm(x[1]).startswith(norm(parts[0])[:3]) and norm(x[2]).startswith(parts[1].lower())]
        who = r['name']
    if len(hit) > 1 and not r['relay'] and sv_ is not None:
        hit = sorted(hit, key=lambda x: abs((x[3] if x[3] is not None else x[4]) - sv_))[:1]
    if len(hit) != 1: review.append(f"{'AMBIG' if hit else 'NO DB ROW'} {r['meet']} g{r['g']} {r['event']} place {r['place']} {who} {r['mark']} (opp {r['opp']})"); continue
    pid, fn, ln, t, d = hit[0]
    dv = t if t is not None else d; sv = val(ev, r['mark']) if r['mark'] else None
    if sv is not None and dv is not None and abs(dv - sv) > (0.6 if ev in ('Shot Put', 'Discus', 'High Jump', 'Long Jump', 'Pole Vault') else 0.3 if t is not None and t > 120 else 0.15):
        review.append(f"MARK DIFFERS {r['meet']} g{r['g']} {ev} place {r['place']} {fn} {ln}: sheet {r['mark']} ({sv}) vs DB {dv}")
    out.append((r, pid, fn, ln, ev, md))
print(len(rows), 'sheet rows;', len(out), 'Cloverleaf finishers matched;', len(review), 'issues'); [print(' ', x) for x in review]
import pickle; pickle.dump((out, totals), open('/tmp/dual_sheets.pkl', 'wb'))

# ---- SQL ----
lines = ["-- Generated by scripts/duals/sheets_build.py from the owner's score-sheet workbooks. Dual per-opponent placings (sheet points) + team scores. Id-independent; re-runnable.",
         "SET QUOTED_IDENTIFIER ON;", "SET XACT_ABORT ON;", "BEGIN TRANSACTION;", "DECLARE @m INT, @t INT, @pid INT, @mp INT;",
         "SET @t = (SELECT TOP 1 Id FROM ScoringTemplates WHERE Name LIKE N'Dual Meet%' AND Deleted=0);", ""]
done_meet = set(); n = 0
by_meet = collections.defaultdict(list)
for r, pid, fn, ln, ev, md in out: by_meet[r['meet']].append((r, pid, fn, ln, ev, md))
for meet in by_meet:
    mn = MEETNAME[meet]; md = meet
    lines += [f"-- ===== {mn} =====", f"SET @m = (SELECT TOP 1 Id FROM Meets WHERE Name=N'{esc(mn)}' AND CAST(Date AS date)='{md}');", "IF @m IS NULL THROW 50040, 'meet missing', 1;"]
    opps = sorted({r['opp'] for r, *_ in by_meet[meet]})
    lines.append(f"UPDATE Meets SET MeetType={2 if len(opps) > 1 else 1}, ScoringTemplateId=@t WHERE Id=@m;")
    for i, o in enumerate(opps):
        lines.append(f"IF NOT EXISTS (SELECT 1 FROM MeetParticipants mp JOIN Schools s ON s.Id=mp.SchoolId WHERE mp.MeetId=@m AND s.Name=N'{OPPNAME[o]}' AND mp.Deleted=0) INSERT INTO MeetParticipants (MeetId, SchoolId, Gender, SortOrder) SELECT @m, Id, NULL, {i} FROM Schools WHERE Name=N'{OPPNAME[o]}';")
    for r, pid, fn, ln, ev, _ in by_meet[meet]:
        n += 1
        lines.append(f"-- {r['opp']} {'Boys' if r['g'] == 1 else 'Girls'} {ev}: place {r['place']} {fn} {ln}".rstrip() + f" {r['mark']}")
        if r['relay']:
            t = [x for x in perfs[(mids[meet], r['g'], ev)] if x[0] == pid][0][3]
            lines.append(f"SET @pid = (SELECT TOP 1 p.Id FROM Performances p JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND p.AthleteId IS NULL AND e.Gender={r['g']} AND e.Name=N'{ev}' AND ABS(p.TimeSeconds-{t})<0.006);")
        else:
            lines.append(f"SET @pid = (SELECT TOP 1 p.Id FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND a.FirstName=N'{esc(fn)}' AND a.LastName=N'{esc(ln)}' AND e.Gender={r['g']} AND e.Name=N'{ev}');")
        lines.append(f"SET @mp = (SELECT TOP 1 mp.Id FROM MeetParticipants mp JOIN Schools s ON s.Id=mp.SchoolId WHERE mp.MeetId=@m AND s.Name=N'{OPPNAME[r['opp']]}' AND mp.Deleted=0);")
        full = float(r['pts']) if r['pts'] not in (None, '') else 0.0
        split = round(full / 4, 2) if r['relay'] else full
        lines.append(f"IF @pid IS NOT NULL AND @mp IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MeetPlacings WHERE PerformanceId=@pid AND MeetParticipantId=@mp) INSERT INTO MeetPlacings (MeetId, PerformanceId, MeetParticipantId, Place, FullPoints, SplitPoints) VALUES (@m, @pid, @mp, {r['place']}, {full}, {split});")
    for (tm_, g, o), sc in sorted(totals.items()):
        if tm_ != meet or sc is None: continue
        lines.append(f"SET @mp = (SELECT TOP 1 mp.Id FROM MeetParticipants mp JOIN Schools s ON s.Id=mp.SchoolId WHERE mp.MeetId=@m AND s.Name=N'{OPPNAME[o]}' AND mp.Deleted=0);")
        lines.append(f"IF @mp IS NOT NULL AND NOT EXISTS (SELECT 1 FROM MeetTeamResults WHERE MeetId=@m AND Gender={g} AND OpponentMeetParticipantId=@mp AND Deleted=0) INSERT INTO MeetTeamResults (MeetId, Gender, OpponentMeetParticipantId, OurScore, OpponentScore, Place, FieldSize) VALUES (@m, {g}, @mp, {sc[0]}, {sc[1]}, NULL, NULL);")
    lines.append("")
lines += ["COMMIT TRANSACTION;", "", f"-- {n} placings"] + [f"-- REVIEW: {x}" for x in review]
open(sys.argv[1], 'w').write('\n'.join(lines) + '\n'); print('wrote', sys.argv[1], n, 'placings')
