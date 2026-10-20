#!/usr/bin/env python3 -I
"""Finishing places from the MileSplit Hy-Tek text results downloaded to /tmp/msraw (see BRAIN.md [C74]; ids.txt = '<msMeetId> <dbMeetId>').
usage: ms_places.py <out.sql>"""
import os, re, sys
H = os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0, H); sys.path.insert(0, os.path.join(H, '../crawl'))
from places_lib import *
from parse_hytek import parse
FIELD = {'Long Jump', 'Triple Jump', 'Shot Put', 'Discus', 'Javelin', 'High Jump', 'Pole Vault'}
def times(s): return [v for v in (re.sub(r'[^0-9:.]', '', x) for x in re.findall(r'\d+:\d+\.\d+|\d+\.\d+', s)) if v]
def tsec(x):
    m = re.match(r'^(?:(\d+):)?(\d+\.\d+)$', x); return (int(m.group(1)) * 60 if m.group(1) else 0) + float(m.group(2)) if m else None
rows = []
for l in open('/tmp/msraw/ids.txt'):
    p = l.split()
    if len(p) < 2: continue
    path = f'/tmp/msraw/ms-{p[0]}.txt'
    if not os.path.exists(path): continue
    for r in parse(path):
        if r['round'] != 'final': continue
        pl = re.match(r'^\s*(\d+)\s', r.get('src', ''))
        if not pl: continue
        g = 2 if r['gender'] == 'Girls' else 1
        if r['kind'] == 'ind':
            rows.append(dict(meet=int(p[1]), g=g, ev=r['event'], name=r['athletes'][0][0], time=None, place=int(pl.group(1)), src='ms ' + p[0]))
        else:
            ts = [t for t in (tsec(x) for x in times(r['src'])) if t is not None]
            if ts: rows.append(dict(meet=int(p[1]), g=g, ev=r['event'], name=None, time=ts, place=int(pl.group(1)), src='ms ' + p[0]))
build(rows, sys.argv[1], 'MileSplit Hy-Tek texts')
