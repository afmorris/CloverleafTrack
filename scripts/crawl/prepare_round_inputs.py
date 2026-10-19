#!/usr/bin/env python3 -I
"""Turn an-rounds.txt (athletic.net multi-round lines, '@@<anMeetId> title' blocks) into per-DB-meet input dirs
(../athleticnet/data/<dbMeetId>/{an.txt,db.txt}) using matches.tsv. db.txt is a read-only dump of that meet's
performances from the DB given by the SQLCMD_* environment (local copy by default)."""
import csv, os, subprocess, sys
here = os.path.dirname(os.path.abspath(__file__))
out_root = os.path.join(here, '..', 'athleticnet', 'data')
m = {r['meetId']: r['dbMeetId'] for r in csv.DictReader(open(os.path.join(here, 'matches.tsv')), delimiter='\t') if r['source'] == 'athleticnet' and r['dbMeetId']}
blocks = {}; cur = None
for l in open(os.path.join(here, 'an-rounds.txt')):
    l = l.rstrip('\n')
    if l.startswith('@@'):
        cur = l[2:].split(' ', 1); blocks[cur[0]] = [cur[1], []]
    elif cur and l.strip():
        blocks[cur[0]][1].append(l)
ids = []
for an, (title, lines) in blocks.items():
    db = m.get(an)
    if not db: print('no DB match for', an, title); continue
    d = os.path.join(out_root, db); os.makedirs(d, exist_ok=True)
    p = os.path.join(d, 'an.txt')
    if os.path.exists(p):  # multiple AN meets for one DB meet: append lines
        old = open(p).read().split('#SERIES')[0].rstrip('\n'); lines = old.split('\n')[1:] + lines
    open(p, 'w').write(title + '\n' + '\n'.join(lines) + '\n#SERIES {}\n')
    ids.append(db)
q = "SET NOCOUNT ON; SELECT p.Id, e.Gender, e.Name, ISNULL(a.FirstName+' '+a.LastName, (SELECT STRING_AGG(a2.FirstName+' '+a2.LastName,'/') FROM PerformanceAthletes pa JOIN Athletes a2 ON a2.Id=pa.AthleteId WHERE pa.PerformanceId=p.Id)), p.TimeSeconds, p.DistanceInches, 0 FROM Performances p JOIN Events e ON e.Id=p.EventId LEFT JOIN Athletes a ON a.Id=p.AthleteId WHERE p.MeetId=%d"
for db in sorted(set(ids), key=int):
    r = subprocess.run(['sqlcmd', '-S', 'localhost,14330', '-U', 'sa', '-P', 'Local#Dev1234', '-C', '-d', 'ctf', '-h', '-1', '-W', '-s', '|', '-Q', q % int(db)], capture_output=True, text=True)
    open(os.path.join(out_root, db, 'db.txt'), 'w').write(r.stdout)
print(len(set(ids)), 'DB meets prepared:', ' '.join(sorted(set(ids), key=int)))
