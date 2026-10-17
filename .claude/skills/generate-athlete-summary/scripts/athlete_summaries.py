#!/usr/bin/env python3
"""Helper for the athlete-summary skill. Standard library only.

  athlete_summaries.py list     --base URL [--class-year 2026] [--include-all]  athletes who competed their senior year + summary state (JSON);
                                                                                others are named under "excluded" unless --include-all
  athlete_summaries.py fetch    --base URL --out DIR ATHLETEID...              save each athlete's fact sheet as DIR/<id>.json
  athlete_summaries.py validate --facts DIR --summaries FILE.json              check drafts against their fact sheets
  athlete_summaries.py import   --base URL --summaries FILE.json [--model L]   save drafts (Status = Draft) via /Import
  athlete_summaries.py audit    --base URL --facts DIR --class-year N          check every athlete/meet/event name is linked

The admin area has no authentication, so no credentials are involved. BASE is the locally running web app.
"""
import argparse, html, json, os, re, sys, urllib.error, urllib.parse, urllib.request

MARK = re.compile(r"\d+:\d\d(?:\.\d\d)?|\d+\.\d+|\d+'\s?\d*\.?\d*\"?")
NAME = re.compile(r"\b([A-Z][a-z]+(?:-[A-Z][a-z]+)? [A-Z][a-z]+(?:-[A-Z][a-z]+)?)\b")
ORD = {"first": 1, "second": 2, "third": 3, "fourth": 4, "fifth": 5, "sixth": 6, "seventh": 7, "eighth": 8, "ninth": 9, "tenth": 10}
NUM = {"one": 1, "two": 2, "three": 3, "four": 4, "five": 5, "six": 6, "seven": 7, "eight": 8, "nine": 9, "ten": 10}


def get(url):
    try:
        with urllib.request.urlopen(url, timeout=180) as r:
            return r.read().decode()
    except urllib.error.URLError as e:
        if isinstance(e, urllib.error.HTTPError):
            raise
        sys.exit(f"Cannot reach {url}\n({e.reason}). Is the web app running? Start it, e.g.: dotnet run --project CloverleafTrack.Web --urls http://localhost:5101 --no-launch-profile")


def clause_window(text, start, end):
    before = text[max(0, start - 70): start]
    cut = max(before.rfind(","), before.rfind(";"), before.rfind(". "), before.rfind(" and "))
    before = before[cut + 1:] if cut >= 0 else before
    after = text[end: end + 50]
    stops = [i for i in (after.find(","), after.find(";"), after.find(". ")) if i >= 0]
    return before + text[start:end] + (after[: min(stops)] if stops else after)


def cmd_list(a):
    q = {"graduationYear": a.class_year} if a.class_year else {}
    if a.include_all:
        q["includeAll"] = "true"
    print(json.dumps(json.loads(get(f"{a.base}/Admin/AthleteSummaries/List?" + urllib.parse.urlencode(q))), indent=1))


def cmd_fetch(a):
    os.makedirs(a.out, exist_ok=True)
    for aid in a.athlete_ids:
        try:
            body = get(f"{a.base}/Admin/AthleteSummaries/FactSheet?athleteId={aid}")
        except urllib.error.HTTPError as e:
            print(f"{aid}: HTTP {e.code} (no performances to summarize?)"); continue
        open(os.path.join(a.out, f"{aid}.json"), "w").write(body)
        fs = json.loads(body)
        warn = []
        if not fs.get("personalRecords"):
            warn.append("no personalRecords")
        if fs["totals"]["schoolRecords"] != len({(h["event"], h["environment"]) for h in fs.get("highlights", []) if h.get("sr")}):
            warn.append("totals.schoolRecords does not match the sr highlights (check before writing)")
        print(f"{aid}: {fs['athlete']} ({fs['team']}, {len(fs['seasons'])} seasons, {len(body)} bytes)" + "".join(f"\n   WARNING: {w}" for w in warn))


def sheet_facts(fs):
    """Everything a draft is allowed to say, extracted from the fact sheet."""
    entries = list(fs.get("personalRecords", [])) + list(fs.get("highlights", []))
    names = {fs["athlete"]} | {n for e in entries for n in e.get("relay", [])}
    meets = {e["meet"] for e in entries}
    ranks = {e["allTimeRank"] for e in entries if "allTimeRank" in e}
    by_mark = {}
    for e in entries:
        m = by_mark.setdefault(e["mark"], {"sr": False, "ranks": set()})
        m["sr"] = m["sr"] or bool(e.get("sr"))
        if "allTimeRank" in e:
            m["ranks"].add(e["allTimeRank"])
    return names, meets, ranks, by_mark


def class_of_date(grad_year, date_text):
    """Class at a date like 'April 24, 2026' / 'April 2023' (August or later belongs to the next school year)."""
    m = re.search(r"([A-Z][a-z]+) (?:\d+, )?(\d{4})", date_text)
    if not m:
        return None
    month = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"].index(m.group(1)) + 1
    end = int(m.group(2)) + (1 if month >= 8 else 0)
    return {0: "Senior", 1: "Junior", 2: "Sophomore", 3: "Freshman"}.get(grad_year - end)


def marks_by_class(fs):
    """class name -> marks the fact sheet ties to that class (season bests, first marks, dated records/highlights)."""
    out = {}
    for s in fs.get("seasons", []):
        for e in s["events"]:
            out.setdefault(s["class"], set()).add(e["seasonBest"])
    for c in fs.get("careerArc", []):
        out.setdefault(c["first"].get("class"), set()).add(c["first"]["mark"])
    for e in list(fs.get("personalRecords", [])) + list(fs.get("highlights", [])):
        out.setdefault(class_of_date(fs["classOf"], e["date"]), set()).add(e["mark"])
    return out


def cmd_validate(a):
    sums = json.load(open(a.summaries))
    flagged = 0
    for aid, text in sums.items():
        fs = json.load(open(os.path.join(a.facts, f"{aid}.json")))
        raw = json.dumps(fs, ensure_ascii=False)
        names, meets, ranks, by_mark = sheet_facts(fs)
        firsts, lasts = {n.split()[0] for n in names}, {n.split()[-1] for n in names}
        issues, words = [], len(text.split())
        if not (70 <= words <= 145):
            issues.append(f"word count {words} (target 80-140)")
        for m in set(MARK.findall(text)):
            m = m.rstrip(".")
            if m not in raw and m.replace('"', '\\"') not in raw:
                issues.append(f"mark not in fact sheet: {m}")
        for m in set(NAME.findall(text)):
            if m not in names and (m.split()[0] in firsts or m.split()[-1] in lasts):
                issues.append(f"name not in fact sheet: {m}")
        for pat, why in ((r"\b(he|she|his|her|hers|him|himself|herself)\b", "gendered pronoun"),
                         (r"\b(college|scholarship|future|plans?|grades?|gpa|family|injur\w+|parents?)\b", "off-topic personal/future content")):
            hit = re.search(pat, text, re.I)
            if hit:
                issues.append(f"{why}: '{hit.group(0)}'")
        if re.search(r"\bmeters?\b|\b(?:the|in|won|ran) (?:100|200|400|800|1600|3200)\b(?!M)", text, re.I):
            issues.append("event worded without the site's name (use '800M', '100M Hurdles', '4x400M Relay')")
        if re.search(r"\b(\d+|" + "|".join(NUM) + r")\s+personal (bests|records)\b", text, re.I):
            issues.append("states a personal-best count (not recorded for older seasons)")
        c = re.search(r"\b(\d+|" + "|".join(NUM) + r")\s+school records?\b", text, re.I)
        if c and int(NUM.get(c.group(1).lower(), c.group(1)) if not c.group(1).isdigit() else c.group(1)) != fs["totals"]["schoolRecords"]:
            issues.append(f"school record count '{c.group(0)}' but totals.schoolRecords is {fs['totals']['schoolRecords']}")
        for r in re.finditer(r"\b(first|second|third|fourth|fifth|sixth|seventh|eighth|ninth|tenth|(\d+)(?:st|nd|rd|th)|No\. ?(\d+))\b[^.;]{0,45}?(all-time|school history|in school)", text, re.I):
            n = ORD.get(r.group(1).lower()) or int(r.group(2) or r.group(3))
            if n not in ranks:
                issues.append(f"rank {n} stated but no mark has allTimeRank {n}")
        for m in MARK.finditer(text):
            mk = m.group(0).rstrip(".")
            win = clause_window(text, m.start(), m.end()).lower()
            info = by_mark.get(mk)
            if ("school record" in win or "school-record" in win) and not (info and info["sr"]):
                issues.append(f"check school-record claim near {mk} (no sr row has that mark)")
        # mark -> meet attribution: a mark the sheet pins to a meet must not be attributed to a different one
        entries = list(fs.get("personalRecords", [])) + list(fs.get("highlights", []))
        meets_of = {}
        for e in entries:
            meets_of.setdefault(e["mark"], set()).add(e["meet"])
        for m in MARK.finditer(text):
            mk = m.group(0).rstrip(".")
            if mk not in meets_of:
                continue
            after = text[m.end(): m.end() + 120]
            found = sorted(((after.find(x), x) for x in meets if after.find(x) >= 0))
            if found and found[0][1] not in meets_of[mk]:
                issues.append(f"mark {mk} is attributed to '{found[0][1]}' but the fact sheet puts it at {sorted(meets_of[mk])}")
        by_class = marks_by_class(fs)
        for m in MARK.finditer(text):
            mk = m.group(0).rstrip(".")
            win = clause_window(text, m.start(), m.end())
            cw = re.findall(r"\b(Freshman|Sophomore|Junior|Senior)\b", win)
            if cw and mk in raw and not any(mk in by_class.get(c, set()) for c in cw):
                issues.append(f"check season claim: {mk} is tied to {'/'.join(cw)} in the text but the fact sheet does not tie it to that class")
        print(f"{aid}: {fs['athlete']}, {words}w — " + ("ok" if not issues else ""))
        for i in issues:
            print("   -", i)
        flagged += bool(issues)
    print(f"\n{len(sums)} checked, {flagged} with findings. Mechanical check only: read every draft against its fact sheet.")


def cmd_import(a):
    for aid, text in json.load(open(a.summaries)).items():
        data = urllib.parse.urlencode({"athleteId": aid, "text": text, "model": a.model}).encode()
        with urllib.request.urlopen(urllib.request.Request(f"{a.base}/Admin/AthleteSummaries/Import", data=data), timeout=120) as r:
            print(f"{aid}: HTTP {r.status}")


def cmd_audit(a):
    page = get(f"{a.base}/Admin/AthleteSummaries?graduationYear={a.class_year}")
    cards = re.findall(r'<div class="card" id="athlete-(\d+)">.*?<div class="summary[^"]*">(.*?)</div>', page, re.S)
    bad = 0
    for aid, body in cards:
        path = os.path.join(a.facts, f"{aid}.json")
        if not os.path.exists(path):
            continue
        fs = json.load(open(path))
        names, meets, _, _ = sheet_facts(fs)
        evs = {s_e["event"] for s in fs["seasons"] for s_e in s["events"]}
        plain = html.unescape(re.sub(r"<a [^>]*>.*?</a>", "", body))
        miss = [f"meet {m}" for m in meets if m in plain]
        miss += [f"athlete {n}" for n in names if n in plain]
        miss += [f"event '{plain[max(0, m.start()-14):m.end()+2]}' (add indoor/outdoor?)" for e in sorted(evs, key=len, reverse=True)
                 for m in re.finditer(r"(?<!\w)" + re.escape(e) + r"(?!\w)", plain, re.I)]
        n_links = len(re.findall(r'href="/(?:roster|events|meets)/', body))
        print(f"{aid}: {fs['athlete']}: {n_links} links" + (f"; UNLINKED {miss[:4]}" if miss else "; all linked"))
        bad += bool(miss)
    print(f"{len(cards)} cards, {bad} with unlinked mentions")


def main():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = p.add_subparsers(dest="cmd", required=True)
    s = sub.add_parser("list"); s.add_argument("--base", required=True); s.add_argument("--class-year", type=int); s.add_argument("--include-all", action="store_true"); s.set_defaults(f=cmd_list)
    s = sub.add_parser("fetch"); s.add_argument("--base", required=True); s.add_argument("--out", required=True); s.add_argument("athlete_ids", nargs="+"); s.set_defaults(f=cmd_fetch)
    s = sub.add_parser("validate"); s.add_argument("--facts", required=True); s.add_argument("--summaries", required=True); s.set_defaults(f=cmd_validate)
    s = sub.add_parser("import"); s.add_argument("--base", required=True); s.add_argument("--summaries", required=True)
    s.add_argument("--model", default="claude-opus (written by subagent in Claude Code)"); s.set_defaults(f=cmd_import)
    s = sub.add_parser("audit"); s.add_argument("--base", required=True); s.add_argument("--facts", required=True); s.add_argument("--class-year", type=int, required=True); s.set_defaults(f=cmd_audit)
    a = p.parse_args(); a.f(a)


if __name__ == "__main__":
    main()
