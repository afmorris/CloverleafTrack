"""Turns the JSON captured from athletic.net event pages (see scripts/athleticnet/scan_an_rows.js) into the row format used by
build_hytek_sql.py: [{event, gender, kind, athletes:[[name, grade]], round, mark, src}] plus per-event marked / no-mark counts."""
import json, re
from parse_hytek import parse_mark

SLUG = {'100m': '100M', '200m': '200M', '400m': '400M', '800m': '800M', '1600m': '1600M', '3200m': '3200M', '60m': '60M', '60mh': '60M Hurdles',
        '110mh': '110M Hurdles', '100mh': '100M Hurdles', '300mh': '300M Hurdles', '4x100m': '4x100M Relay', '4x200m': '4x200M Relay',
        '4x400m': '4x400M Relay', '4x800m': '4x800M Relay', '4x1600m': '4x1600M Relay', 'shot': 'Shot Put', 'discus': 'Discus', 'hj': 'High Jump',
        'pv': 'Pole Vault', 'lj': 'Long Jump', 'tj': 'Triple Jump', 'jt': 'Javelin', 'hammer': 'Hammer Throw'}

def clean_mark(m):
    return re.sub(r'[a-zA-Z]$', '', m.strip()) if re.match(r'^[\d:.]+[ah]$', m.strip()) else m.strip()

def parse(path):
    rows = []
    for page in json.load(open(path)):
        u = page['u']                     # e.g. m/1/100m
        parts = u.split('/')
        if len(parts) < 3 or parts[2] not in SLUG: continue
        gender = 'Boys' if parts[0] == 'm' else 'Girls'
        ev = SLUG[parts[2]]
        for r in page['rows']:
            sec = (r.get('sec') or '').lower()
            rnd = 'prelim' if 'prelim' in sec else 'semi' if 'semi' in sec else 'final'
            if r['k'] == 'ind':
                m = re.search(r'Yr:\s*(\d+)', r.get('info', ''))
                rows.append({'event': ev, 'gender': gender, 'kind': 'ind', 'athletes': [[r['name'], int(m.group(1)) if m else None]], 'round': rnd, 'mark': clean_mark(r['mark']), 'src': u})
            else:
                legs = re.sub(r'^(?:PB|SB)?\s*(?:•\s*)?(?:\+[\d.]+pts)?', '', r.get('legs', '')).strip()
                names = [x.strip() for x in legs.split(',') if x.strip()] if legs and 'Relay Team' not in legs else []
                rows.append({'event': ev, 'gender': gender, 'kind': 'relay', 'athletes': [[n, None] for n in names], 'round': rnd, 'mark': clean_mark(r['mark']), 'src': u})
    return rows

def unrecorded(path):
    out = set()
    for page in json.load(open(path)):
        parts = page['u'].split('/')
        if len(parts) >= 3 and parts[2] in SLUG and ((8 <= page.get('marked', 0) <= 10 and page.get('nomark', 0) >= 2) or (6 <= page.get('marked', 0) <= 10 and page.get('nomark', 0) >= 3 * page.get('marked', 0))):
            out.add(('Boys' if parts[0] == 'm' else 'Girls', SLUG[parts[2]]))
    return out
