#!/usr/bin/env python3 -I
"""Finishing places from the OHSAA state results pages: baumspage mirror (/tmp/ohsaa/pages/<year>-<b|g><div>.txt, 2007+) and ohsaa.org 2001-2008 pages
(/tmp/ohsaa_old/p<year>-<bd|gd><div>reslt<yy>.txt). usage: state_places.py <out.sql>"""
import glob, os, re, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from places_lib import *
names = {n: int(i) for i, n in q("SELECT Id, Name FROM Meets WHERE Name LIKE '%OHSAA%State%'")}
def meet(year, div):
    want = f'{year} OHSAA Seated Division State' if div == 'Seated' else f'{year} OHSAA Division {div} State'
    c = [i for n, i in names.items() if n.startswith(want)]
    return c[0] if len(c) == 1 else None
def title(t):
    t = t.strip(); g = 2 if t.startswith('Girls') else 1 if t.startswith('Boys') else None
    t = re.sub(r'^(Girls|Boys)\s+', '', t); seated = ' Seated' in t; t = t.replace(' Seated', '')
    m = re.match(r'^(\d+) Meter( Hurdles| Dash| Run)?', t)
    if m: ev = m.group(1) + 'M' + (' Hurdles' if 'Hurdles' in t else '')
    elif re.match(r'^4x(\d+) Meter Relay', t): ev = '4x' + re.match(r'^4x(\d+)', t).group(1) + 'M Relay'
    else: ev = {'Shot Put': 'Shot Put', 'Discus Throw': 'Discus', 'Long Jump': 'Long Jump', 'High Jump': 'High Jump', 'Pole Vault': 'Pole Vault', 'Triple Jump': 'Triple Jump', 'Javelin Throw': 'Javelin'}.get(t, t)
    return g, ('Seated ' + ev if seated else ev)
def tsec(tok):
    tok = re.sub(r'[^0-9:.]', '', tok)
    m = re.match(r'^(?:(\d+):)?(\d+\.\d+)$', tok)
    return (int(m.group(1)) * 60 if m.group(1) else 0) + float(m.group(2)) if m else None
rows = []
def add(year, div, g, ev, name, rel, place):
    mid = meet(year, div)
    if mid and place.isdigit(): rows.append(dict(meet=mid, g=g, ev=ev, name=name, time=rel, place=int(place), src=f'{year} {div}'))
for f in sorted(glob.glob('/tmp/ohsaa/pages/*.txt')):
    base = os.path.basename(f)[:-4]
    if re.search(r'MeetProg|heat|part', base): continue
    year = base[:4]; div = {'1': 'I', '2': 'II', '3': 'III', '4': 'Seated'}.get(re.search(r'(\d)$', base).group(1))
    ev = ''; hdr = ''; g = None
    for l in open(f).read().split('\n'):
        m = re.match(r'^Event \d+\s+(.*)$', l)
        if m: g, ev = title(m.group(1)); continue
        if re.search(r'Name\s+Year School|School\s+Seed', l): hdr = l
        if not re.search('cloverleaf', l, re.I) or not ev: continue
        m1 = re.match(r'^\s*(\S+)\s+(?:\d+\s+)?(.+?)\s+(\d{1,2})\s+Cloverleaf\s+(.*)$', l); rel = re.match(r'^\s*(\S+)\s+Cloverleaf\s+(.*)$', l) if 'Relay' in ev else None
        if rel: place, rest, name = rel.group(1), rel.group(2), None
        elif m1: place, name, rest = m1.group(1), m1.group(2).strip(), m1.group(4)
        else: continue
        toks = re.findall(r'\d+:\d+\.\d+|\d+\.\d+', rest)
        t = None
        if name is None:
            t = tsec(toks[1] if 'Seed' in hdr and 'Finals' in hdr and len(toks) >= 2 else (toks[0] if toks else ''))
            if t is None: continue
        add(year, div, g, ev, name, t, place)
for f in sorted(glob.glob('/tmp/ohsaa_old/p20*-*.txt')):
    m = re.match(r'.*/p(\d{4})-(b|g)d(\d)reslt', f)
    if not m: continue
    year, gc, d = m.group(1), m.group(2), m.group(3); div = {'1': 'I', '2': 'II', '3': 'III'}[d]
    ev = ''; g = None
    for l in open(f).read().split('\n'):
        mm = re.match(r'^Event \d+\s+(.*)$', l)
        if mm: g, ev = title(mm.group(1)); continue
        if not re.search('cloverleaf', l, re.I) or not ev: continue
        if 'Relay' in ev:
            r = re.match(r'^\s*(\d+)\s+(?:Lodi )?Cloverleaf\s+(\S+)', l)
            if r and tsec(r.group(2)) is not None: add(year, div, g, ev, None, tsec(r.group(2)), r.group(1))
        else:
            r = re.match(r'^\s*(\d+)\s+(.+?)\s+(\d{1,2})\s+(?:Lodi )?Cloverleaf\b', l)
            if r: add(year, div, g, ev, r.group(2).strip(), None, r.group(1))
build(rows, sys.argv[1], 'OHSAA state pages')
