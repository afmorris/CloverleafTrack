"""Parses Cloverleaf dual-meet score-sheet workbooks/PDF text into rows: dict(meet, g, opp, event, place, name, team, mark, pts, relay)
plus team totals. Sources: the owner's xlsx score sheets (one sheet per team pair) and the 2025 Woodridge girls text PDF."""
import re, sys, os
import openpyxl
EVMAP = {'4x800 Meter Relay': '4x800M Relay', '4x200 Meter Relay': '4x200M Relay', '4x100 Meter Relay': '4x100M Relay', '4x400 Meter Relay': '4x400M Relay',
         '100/110 Meter Hurdles': 'HURDLE_SHORT', '100 Meter Dash': '100M', '1600 Meter Run': '1600M', '400 Meter Dash': '400M', '300 Meter Hurdles': '300M Hurdles',
         '800 Meter Run': '800M', '200 Meter Dash': '200M', '3200 Meter Run': '3200M', 'High Jump': 'High Jump', 'Long Jump': 'Long Jump', 'Pole Vault': 'Pole Vault',
         'Shot Put': 'Shot Put', 'Discus': 'Discus'}
def tm(t): return {'CLO': 'CLO', 'C': 'CLO', 'COV': 'COV', 'FLD': 'FLD', 'F': 'FLD', 'WOO': 'WOO', 'W': 'WOO'}.get(t, t)
def split_entry(s):
    """'Abby R (CLO)  4' 4' / 'Woodridge - 10:38' / 'Cloverleaf (CLO)  10:31' -> (name, team, mark)"""
    if s is None: return None
    s = str(s).strip()
    if not s or s.lower() == 'none': return None
    m = re.match(r'^(.*?)\s*\((CLO|COV|FLD|WOO|C|F|W)\)\s*[-]?\s*(.*)$', s)
    if m: return m.group(1).strip(), tm(m.group(2)), m.group(3).strip()
    m = re.match(r'^(.*?)\s+-\s+(.*)$', s)
    if m: return m.group(1).strip(), None, m.group(2).strip()
    return s, None, ''
def sheet_rows(ws, meet, opp_of):
    out = []; tot = {}
    rows = [[c for c in r] for r in ws.iter_rows(values_only=True)]
    head = str(rows[0][1]) if rows[0][1] else ''
    gender = 2 if 'Girls' in head else 1
    for r in rows[2:]:
        ev = str(r[0]).strip() if r[0] else ''
        if ev not in EVMAP: continue
        cells = r[1:]
        # layout: entry, team, pts, entry, team, pts, entry, team, pts, ... (relay rows: entry, team, pts, pts, total ...)
        trip = [(cells[i], cells[i + 1], cells[i + 2]) for i in (0, 3, 6) if i + 2 < len(cells)]
        for place, (e, t, p) in enumerate(trip, 1):
            x = split_entry(e)
            if not x: continue
            name, team, mark = x
            team = team or tm(str(t).strip()) if t not in (None, '') else team
            if team is None and t: team = tm(str(t).strip())
            out.append(dict(meet=meet, g=gender, event=EVMAP[ev], place=place, name=name, team=team, mark=mark, pts=p))
    return gender, out

TEAMNAMES = {'cloverleaf': 'CLO', 'cloverleaf b': 'CLO', 'woodridge': 'WOO', 'coventry': 'COV', 'field': 'FLD'}
def load(path, pairs):
    """pairs: {sheetTitle: (meetKey, oppCode)}; returns (rows, totals) where totals[(meet, g, opp)] = (CLO, OPP)"""
    wb = openpyxl.load_workbook(path, data_only=True); rows = []; totals = {}
    for title, (meet, opp) in pairs.items():
        ws = wb[title]; allr = [list(r) for r in ws.iter_rows(values_only=True)]
        head = allr[0]; g = 2 if 'Girls' in str(head[1]) else 1
        nums = [v for v in head if isinstance(v, (int, float))]
        totals[(meet, g, opp)] = (nums[-2], nums[-1]) if len(nums) >= 2 else None
        for r in allr[2:]:
            ev = str(r[0]).strip() if r[0] else ''
            if ev not in EVMAP: continue
            cells = r[1:]
            for place, i in enumerate((0, 3, 6), 1):
                if i + 2 >= len(cells): continue
                e, t, p = cells[i], cells[i + 1], cells[i + 2]
                x = split_entry(e)
                if not x: continue
                name, team, mark = x
                if team is None:
                    team = TEAMNAMES.get(name.lower()) or (tm(str(t).strip()) if t not in (None, '') else None)
                is_relay = ev.endswith('Relay') or '4x' in ev
                rows.append(dict(meet=meet, g=g, opp=opp, event=EVMAP[ev], place=place, name=name, team=team, mark=mark, pts=p, relay=is_relay))
    return rows, totals
if __name__ == '__main__':
    D = '/Users/tony/Downloads/'
    rows, tot = load(D + 'Cloverleaf Track Meet Score Sheet - 2024 Double Dual Coventry Field.xlsx',
                     {'CLO COV Girls': ('2024-04-23', 'COV'), 'CLO COV Boys': ('2024-04-23', 'COV'), 'CLO FLD Girls': ('2024-04-23', 'FLD'), 'CLO FLD Boys': ('2024-04-23', 'FLD')})
    r2, t2 = load(D + 'Cloverleaf Track Meet Score Sheet - Woodridge.xlsx', {'Girls Meet': ('2023-04-11', 'WOO'), 'Boys Meet': ('2023-04-11', 'WOO')})
    for r in rows + r2: print(r['meet'], r['g'], r['opp'], r['event'], r['place'], r['name'], r['team'], r['mark'], r['pts'])
    print(tot, t2)
