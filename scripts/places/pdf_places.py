#!/usr/bin/env python3 -I
"""Finishing places read by hand from the OHSAA state-results PDFs (1960-2000; OCR'd, then checked against the page images). usage: pdf_places.py <out.sql>"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from places_lib import *
mid = {n: int(i) for i, n in q("SELECT Id, Name FROM Meets WHERE Name LIKE '%State%'")}
S = lambda y: next(i for n, i in mid.items() if n.startswith(f'{y} ') and 'State' in n and 'Seated' not in n)
R = [(1960, 1, '200M', 'Bill Heffelfinger', 1), (1962, 1, '200M', 'Bill Heffelfinger', 2), (1972, 1, 'High Jump', 'Scott Chrislip', 3), (1972, 1, '180M Low Hurdles', 'Scott Chrislip', 5),
     (1973, 1, 'High Jump', 'Scott Chrislip', 3), (1982, 1, '800M', 'Scott Murphy', 6), (1990, 2, '1600M', 'Kim Malarkey', 5), (1991, 2, '1600M', 'Kim Malarkey', 5),
     (1991, 1, 'Discus', 'Cory Carlson', 1), (1992, 1, 'Discus', 'Cory Carlson', 1), (1993, 1, 'Discus', 'Cory Carlson', 1), (1993, 1, 'Shot Put', 'Cory Carlson', 2),
     (1993, 1, '800M', 'Charles Massie', 4), (1993, 1, '1600M', 'Charles Massie', 4), (1994, 1, '800M', 'Charles Massie', 7), (1994, 1, 'Discus', 'Matt Fordenwalt', 6),
     (1995, 1, 'Discus', 'Matt Fordenwalt', 3), (2000, 2, '400M', 'Megan Linden', 7)]
rows = [dict(meet=S(y), g=g, ev=ev, name=n, time=None, place=p, src='pdf') for y, g, ev, n, p in R]
rows.append(dict(meet=S(1990), g=2, ev='4x800M Relay', name=None, time=571.10, place=6, src='pdf'))
build(rows, sys.argv[1], 'OHSAA PDFs')
