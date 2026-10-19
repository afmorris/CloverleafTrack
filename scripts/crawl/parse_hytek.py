"""Parses a baumspage Hy-Tek district results text file (baumspage/ne-YYYY-res.txt) into Cloverleaf rows.
Output (stdout): JSON list of {event, gender, kind(ind|relay), athletes:[name,yr], rounds:[(round,mark_str)], status}
Usage: python3 -I parse_hytek.py baumspage/ne-2008-res.txt"""
import re, sys, json

EV = re.compile(r'^(?:Event\s+\d+\s+)?(Girls|Boys)\s+((?:\d|4x).*?|High Jump|Long Jump|Pole Vault|Shot Put|Discus.*|Triple Jump)\s*$')
SEC = re.compile(r'^(Heat|Section|Flight)\s+(\d+)\s*(.*)$')
ROW = re.compile(r'^\s*(\d+|--)\s+(?P<name>.+?)\s+(?P<yr>\d{1,2})\s+(?P<school>[A-Za-z.\' &-]*?)\s{2,}(?P<mark>\S+)\s*(?P<rest>.*)$')
RELAY = re.compile(r'^\s*(\d+|--)\s+(?P<school>[A-Za-z.\' &-]*?)\s{2,}(?P<mark>\S+)\s*(?P<rest>.*)$')
MEMBER = re.compile(r'(\d)\)\s+(.+?)\s+(\d{1,2})(?=\s{2,}|\s*$|\s+\d\))')

def norm_event(name):
    n = name.lower()
    n = re.sub(r'\b\d{2}"|\(.*?\)', ' ', n)                                   # hurdle heights, parentheses
    n = re.sub(r'\b(varsity|jv|metro|footed|open|prelims?|finals?|county|div \d|division \d+(?:\s*-\s*\w+)?)\b', ' ', n)   # qualifiers
    n = re.sub(r'(\d)m\b', r'\1', n)
    n = re.sub(r'\bmeters?\b', ' ', n); n = re.sub(r'\s+', ' ', n).strip()
    m = re.fullmatch(r'(\d+)\s*(?:dash|run)?\s*(?:wheelchair|seated)', n)
    if m: return 'Seated ' + m.group(1) + 'M'
    if re.fullmatch(r'shot put\s*(wheelchair|seated)', n): return 'Seated Shot Put'
    if 'wheelchair' in n or 'seated' in n: return name.strip()        # no DB event for e.g. seated discus
    for base, canon in (('high jump', 'High Jump'), ('long jump', 'Long Jump'), ('pole vault', 'Pole Vault'), ('triple jump', 'Triple Jump'), ('shot put', 'Shot Put'), ('discus', 'Discus')):
        if n.startswith(base):
            return canon + (' Relay' if ' relay' in n else '')   # 'High Jump Ashley Sigmo' -> High Jump; 'Shot Put Relay' stays a field relay
    table = {'100 hurdles': '100M Hurdles', '110 hurdles': '110M Hurdles', '300 hurdles': '300M Hurdles', '100 dash': '100M', '200 dash': '200M',
             '400 dash': '400M', '800 run': '800M', '1600 run': '1600M', '3200 run': '3200M', 'high jump': 'High Jump', 'long jump': 'Long Jump',
             'pole vault': 'Pole Vault', 'shot put': 'Shot Put', 'discus throw': 'Discus', 'triple jump': 'Triple Jump',
             '4x100 relay': '4x100M Relay', '4x200 relay': '4x200M Relay', '4x400 relay': '4x400M Relay', '4x800 relay': '4x800M Relay'}
    if n in table: return table[n]
    m = re.fullmatch(r'(60|100|200|400|800|1600|3200)\s*(dash|run)?', n)
    if m: return m.group(1) + 'M'
    if re.fullmatch(r'(60|100|110|300|400) hurdles', n): return n.split()[0] + 'M Hurdles'
    if re.fullmatch(r'(4000m? )?distance medley( relay)?|dmr|4k distance medley', n): return '4K Distance Medley'
    if re.fullmatch(r'(800m? )?sprint medley( relay)?|smr', n): return '800M Sprint Medley'
    m = re.fullmatch(r'(4x\d+)\s*(?:shuttle hurdles?(?: relay)?)', n)
    if m: return m.group(1) + 'M Shuttle Hurdles'
    m = re.fullmatch(r'(4x\d+)\s*(relay|shuttle hurdles?)', n)
    if m: return m.group(1) + 'M ' + ('Relay' if m.group(2) == 'relay' else 'Shuttle Hurdles')
    return name.strip()

def parse_mark(tok):
    t = tok.strip().rstrip('QqxX*').lstrip('J')
    if t in ('NT', 'NH', 'DQ', 'DNF', 'ND', 'NM', 'DNS', 'FS', 'SCR'): return ('status', t)
    m = re.fullmatch(r'(\d+):(\d{2}\.\d+)', t)
    if m: return ('time', int(m.group(1)) * 60 + float(m.group(2)))
    if re.fullmatch(r'\d+\.\d+', t): return ('time', float(t))
    m = re.fullmatch(r'(\d+)-(\d+(?:\.\d+)?)', t)
    if m: return ('dist', int(m.group(1)) * 12 + float(m.group(2)))
    return ('raw', t)

def parse(path):
    lines = open(path, encoding='utf-8', errors='replace').read().split('\n')
    out, ev, rnd = [], None, 'final'
    i = 0
    while i < len(lines):
        l = lines[i]; m = EV.match(l.strip())
        if m: ev = (m.group(1), norm_event(m.group(2))); rnd = 'final'; i += 1; continue
        bare = re.fullmatch(r'(Preliminaries|Prelims|Semi-?Finals|Semis|Finals)', l.strip(), re.I)
        if bare:
            t = bare.group(1).lower(); rnd = 'prelim' if t.startswith('prelim') else 'semi' if t.startswith('semi') else 'final'; i += 1; continue
        s = SEC.match(l.strip())
        if s:
            t = s.group(3).lower()
            rnd = 'prelim' if 'prelim' in t else 'semi' if 'semi' in t else 'final'; i += 1; continue
        if ev and re.search(r'clover', l, re.I):
            r = ROW.match(l)
            if r and re.search(r'clover', r.group('school'), re.I) and re.fullmatch(r'\d{1,2}', r.group('yr')):
                out.append({'event': ev[1], 'gender': ev[0], 'kind': 'ind', 'athletes': [[re.sub(r'\s+', ' ', r.group('name')).strip(), int(r.group('yr'))]], 'round': rnd, 'mark': r.group('mark'), 'rest': r.group('rest').strip(), 'src': l.strip()})
            else:
                q = RELAY.match(l)
                if q and re.search(r'clover', q.group('school'), re.I) and ev[1].startswith('4x'):
                    mem = []
                    for j in (1, 2):   # leg lines belong to this row only when they directly follow it ("1) Name 10   2) Name 11")
                        if i + j < len(lines) and re.match(r'^\s+\d\)', lines[i + j]):
                            mem += [(a[1], int(a[2])) for a in MEMBER.findall(lines[i + j])]
                        else:
                            break
                    out.append({'event': ev[1], 'gender': ev[0], 'kind': 'relay', 'athletes': [[n, y] for n, y in mem], 'round': rnd, 'mark': q.group('mark'), 'rest': q.group('rest').strip(), 'src': l.strip()})
        i += 1
    return out

if __name__ == '__main__':
    rows = parse(sys.argv[1])
    for r in rows:
        r['value'] = parse_mark(r['mark'])
    json.dump(rows, sys.stdout, indent=0)
