#!/usr/bin/env python3 -I
"""Finishing places from athletic.net result dumps (/tmp/andir/part*.txt, see compare_an_vs_db.py: 'M <anMeetId>', 'E|<event>|<F|M>', 'place|round|result|athleteId|first|last|exh|').
usage: an_places.py <out.sql>"""
import glob, os, re, sys
H = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, H)
from places_lib import *
meets = {}
for l in list(open(os.path.join(H, '../crawl/matches.tsv')))[1:]:
    p = l.rstrip('\n').split('\t')
    if p[0] == 'athleticnet' and p[4]: meets[p[1]] = int(p[4])
def tsec(x):
    x = re.sub(r'[^0-9:.]', '', x); m = re.match(r'^(?:(\d+):)?(\d+(?:\.\d+)?)$', x)
    return (int(m.group(1)) * 60 if m.group(1) else 0) + float(m.group(2)) if m else None
rows = []; cur = None; ev = None; g = None
for f in sorted(glob.glob('/tmp/andir/*.txt')):
    for line in open(f, encoding='utf-8'):
        line = line.rstrip('\n')
        if line.startswith('M '): cur = meets.get(line[2:].strip()); continue
        if line.startswith('E|'):
            _, name, gg = line.split('|'); g = 2 if gg == 'F' else 1; ev = dbev(re.sub(r'^(\d+)m\b', r'\1 meter', name.replace('4x100 Relay', '4x100 Meter Relay').replace('4x200 Relay', '4x200 Meter Relay').replace('4x400 Relay', '4x400 Meter Relay').replace('4x800 Relay', '4x800 Meter Relay'))); continue
        p = line.split('|')
        if cur is None or not ev or len(p) < 7 or p[1] != 'F' or not re.match(r'^\d+$', p[0]): continue
        if '<BR>' in p[4] or p[4] == 'Relay Team':
            t = tsec(p[2])
            if t is not None: rows.append(dict(meet=cur, g=g, ev=ev, name=None, time=t, place=int(p[0]), src='an'))
        else: rows.append(dict(meet=cur, g=g, ev=ev, name=f'{p[4]} {p[5]}', time=None, place=int(p[0]), src='an'))
build(rows, sys.argv[1], 'athletic.net dumps')
