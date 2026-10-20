#!/usr/bin/env python3 -I
"""Finishing places from MileSplit Live finals (scripts/athleticnet/live-raw). usage: live_places.py <out.sql>"""
import os, re, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from places_lib import *
H = os.path.dirname(os.path.abspath(__file__)); RAW = os.path.join(H, '../athleticnet/live-raw')
meets = {}
for l in list(open(os.path.join(H, '../crawl/matches.tsv')))[1:]:
    p = l.rstrip('\n').split('\t')
    if p[0] == 'milesplit' and p[4]: meets[p[1]] = int(p[4])
def secs(t):
    t = re.sub(r'[^0-9:.]', '', t.split(' ')[0]); m = re.match(r'^(?:(\d+):)?(\d+(?:\.\d+)?)$', t)
    return (int(m.group(1)) * 60 if m.group(1) else 0) + float(m.group(2)) if m else None
rows = []
for fn in sorted(os.listdir(RAW)):
    ms = fn[:-4]
    if ms not in meets: continue
    ev = g = None; cols = []; rnd = None
    for line in open(os.path.join(RAW, fn), encoding='utf-8'):
        line = line.rstrip('\n')
        if line.startswith('E|'):
            p = line.split('|', 4); rnd = p[3]; cols = (p[4] if len(p) > 4 else '').split('¦')
            m = re.match(r'^(Boys|Girls)\s+(.*)$', p[2]); g = (1 if m.group(1) == 'Boys' else 2) if m else None; ev = dbev(m.group(2)) if m else None; continue
        if rnd != 'F' or not ev or 'Place' not in cols: continue
        c = line.split('|'); pl = re.match(r'^\d+', c[0].strip())
        if not pl: continue
        if 'Athlete' in cols and len(c) > cols.index('Athlete'):
            rows.append(dict(meet=meets[ms], g=g, ev=ev, name=c[cols.index('Athlete')].strip(), time=None, place=int(pl.group(0)), src='live ' + ms))
        elif 'Team' in cols and 'Time' in cols and len(c) > cols.index('Time') and re.match(r'^Cloverleaf', c[cols.index('Team')]):
            t = secs(c[cols.index('Time')])
            if t is not None: rows.append(dict(meet=meets[ms], g=g, ev=ev, name=None, time=t, place=int(pl.group(0)), src='live ' + ms))
build(rows, sys.argv[1], 'MileSplit Live finals')
