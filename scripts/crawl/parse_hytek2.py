"""Header-driven parser for Hy-Tek text results (baumspage.com meet pages). Handles "First Last  YY School", "Last, First  School"
(no grade) and relay rows, and Seed / Prelims / Semis / Finals / Wind / H# / Points columns. Returns Cloverleaf rows only.
Each row: {event, gender, kind, athletes:[[name, grade|None]], round, mark, src}. A Finals row that also carries a Prelims column
yields BOTH rounds."""
import re
from parse_hytek import norm_event, parse_mark

EVH = re.compile(r'^(?:Event\s+\d+\s+)?(Girls|Boys|Women|Men)\s+((?:\d|4x).*?|(?:High Jump|Long Jump|Pole Vault|Shot Put|Discus|Triple Jump|Javelin|Hammer)(?:\s.*)?)\s*$')
COLH = re.compile(r'^\s*(Name\s+Year\s+School|School)\s+(?P<cols>.*\S)\s*$')
SCHOOL = r"(?:Cloverleaf|Lodi Cloverl\w*|Lodi Clover\w*)(?:\s+'[A-Z]')?"
ROW = re.compile(r'^\s*(?P<pl>\d+|--)\s+(?P<name>.+?)\s+(?:(?P<yr>\d{1,2})\s+)?(?P<school>' + SCHOOL + r')\s+(?P<cols>\S.*?)\s*$')
RELAY = re.compile(r'^\s*(?P<pl>\d+|--)\s+(?P<school>' + SCHOOL + r')\s+(?P<cols>\S.*?)\s*$')
MEMBER = re.compile(r'(\d)\)\s+(.+?)(?:\s+(\d{1,2}))?(?=\s{2,}|\s*$|\s+\d\))')
ROUND_OF = {'prelims': 'prelim', 'preliminaries': 'prelim', 'semis': 'semi', 'semi-finals': 'semi', 'finals': 'final'}

def flip(name):
    name = re.sub(r'#\s*\d+', ' ', name)
    name = re.sub(r'\s+(FR|SO|JR|SR)\s+', ' ', name + ' ').strip()
    if ',' in name:
        last, first = [x.strip() for x in name.split(',', 1)]
        return f'{first} {last}'
    return re.sub(r'\s+', ' ', name).strip()

def parse(path):
    lines = open(path, encoding='utf-8', errors='replace').read().split('\n')
    out, ev, cols, section = [], None, [], 'final'
    for i, l in enumerate(lines):
        s = l.strip()
        m = EVH.match(s)
        if m: ev = ({'Women': 'Girls', 'Men': 'Boys'}.get(m.group(1), m.group(1)), norm_event(m.group(2))); cols = []; section = 'final'; continue
        c = COLH.match(l)
        if c: cols = c.group('cols').split(); continue
        b = re.fullmatch(r'(?:Heat\s+\d+\s+|Section\s+\d+\s+)?(Preliminaries|Prelims|Semi-?Finals|Semis|Finals)', s, re.I)
        if b: t = b.group(1).lower(); section = 'prelim' if t.startswith('prelim') else 'semi' if t.startswith('semi') else 'final'; continue
        if not ev or not re.search(r'clover', l, re.I): continue
        relay = ev[1].startswith('4x')
        r = (RELAY if relay else ROW).match(l)
        if not r: continue
        toks = r.group('cols').split()
        marks = {}
        for name, tok in zip(cols[: len(toks)] if cols else [], toks):
            n = name.lower()
            if n in ROUND_OF: marks[ROUND_OF[n]] = tok
        if not marks and toks: marks[section] = toks[-1] if len(toks) == 1 else toks[0]
        if relay:
            mem = []
            for j in (1, 2):
                if i + j < len(lines) and re.match(r'^\s+\d\)', lines[i + j]): mem += [(flip(a[1]), int(a[2]) if a[2] else None) for a in MEMBER.findall(lines[i + j])]
                else: break
            athletes = [[n, y] for n, y in mem]
        else:
            athletes = [[flip(r.group('name')), int(r.group('yr')) if r.group('yr') else None]]
        for rnd, tok in marks.items():
            out.append({'event': ev[1], 'gender': ev[0], 'kind': 'relay' if relay else 'ind', 'athletes': athletes, 'round': rnd, 'mark': tok, 'src': l.strip()})
    return out

if __name__ == '__main__':
    import sys, json
    rows = parse(sys.argv[1])
    for r in rows: r['value'] = parse_mark(r['mark'])
    for r in rows: print(r['gender'], r['event'], r['round'], r['mark'], [a[0] for a in r['athletes']])
