#!/usr/bin/env python3
"""Helper for the meet-summary skills. Standard library only.

  summaries.py list     --base URL (--season-name 2025-2026 | --season-id N)   meets in a season + summary state (JSON)
  summaries.py fetch    --base URL --out DIR MEETID...                        save each meet's fact sheet as DIR/<id>.json
  summaries.py validate --facts DIR --summaries FILE.json                     check drafts against their fact sheets
  summaries.py import   --base URL --summaries FILE.json [--model LABEL]      save drafts (Status = Draft) via /Import
  summaries.py audit    --base URL --facts DIR --season-id N                  check every name/event is linked on the review page

The admin area has no authentication, so no credentials are involved. BASE is the locally running web app.
"""
import argparse, html, json, os, re, sys, urllib.error, urllib.parse, urllib.request

MARK = re.compile(r"\d+:\d\d(?:\.\d\d)?|\d+\.\d+|\d+'\s?\d*\.?\d*\"?")
NAME = re.compile(r"\b([A-Z][a-z]+(?:-[A-Z][a-z]+)? [A-Z][a-z]+(?:-[A-Z][a-z]+)?)\b")


def get(url):
    try:
        with urllib.request.urlopen(url, timeout=120) as r:
            return r.read().decode()
    except urllib.error.URLError as e:
        if isinstance(e, urllib.error.HTTPError):
            raise
        sys.exit(f"Cannot reach {url}\n({e.reason}). Is the web app running? Start it, e.g.: dotnet run --project CloverleafTrack.Web --urls http://localhost:5101 --no-launch-profile")


def rows_of(fs):
    """(gender, event, result) for every result row in a fact sheet."""
    for g in ("boys", "girls", "mixed"):
        for e in fs.get(g) or []:
            for r in e["results"]:
                yield g, e["event"], r


def clause_window(text, start, end):
    """Text around a mark, cut at the nearest clause boundary on each side so neighbouring results don't bleed in."""
    before = text[max(0, start - 70): start]
    cut = max(before.rfind(","), before.rfind(";"), before.rfind(". "), before.rfind(" and "))
    before = before[cut + 1:] if cut >= 0 else before
    after = text[end: end + 50]
    stops = [i for i in (after.find(","), after.find(";"), after.find(". ")) if i >= 0]
    return before + text[start:end] + (after[: min(stops)] if stops else after)


def cmd_list(a):
    q = {"seasonName": a.season_name} if a.season_name else {"seasonId": a.season_id} if a.season_id else {}
    print(json.dumps(json.loads(get(f"{a.base}/Admin/MeetSummaries/List?" + urllib.parse.urlencode(q))), indent=1))


def cmd_fetch(a):
    os.makedirs(a.out, exist_ok=True)
    for mid in a.meet_ids:
        try:
            body = get(f"{a.base}/Admin/MeetSummaries/FactSheet?meetId={mid}")
        except urllib.error.HTTPError as e:
            print(f"{mid}: HTTP {e.code} (no performances to summarize?)"); continue
        open(os.path.join(a.out, f"{mid}.json"), "w").write(body)
        fs = json.loads(body)
        rows = list(rows_of(fs))
        t = fs.get("totals", {})
        warn = []
        if len(rows) != t.get("performances"):
            warn.append(f"{len(rows)} results listed but totals say {t.get('performances')} performances (rows dropped?)")
        if sum(1 for _, _, r in rows if r.get("sr")) != t.get("schoolRecords"):
            warn.append(f"{sum(1 for _, _, r in rows if r.get('sr'))} sr rows but totals say {t.get('schoolRecords')} school records")
        print(f"{mid}: saved ({len(body)} bytes)" + ("".join(f"\n   WARNING: {w}" for w in warn)))


def cmd_validate(a):
    sums = json.load(open(a.summaries))
    problems = 0
    for mid, text in sums.items():
        fs = json.load(open(os.path.join(a.facts, f"{mid}.json")))
        raw = json.dumps(fs, ensure_ascii=False)
        rows = list(rows_of(fs))
        names = {n for _, _, r in rows for n in ([r["athlete"]] if "athlete" in r else r.get("relayTeam", []))}
        firsts = {n.split()[0] for n in names}
        issues = []
        words = len(text.split())
        if not (60 <= words <= 120):
            issues.append(f"word count {words} (target 70-120; tiny meets may be shorter)")
        for m in set(MARK.findall(text)):
            m = m.rstrip(".")
            if m not in raw and m.replace('"', '\\"') not in raw:
                issues.append(f"mark not in fact sheet: {m}")
        for m in set(NAME.findall(text)):
            if m not in names and m.split()[0] in firsts:
                issues.append(f"name not in fact sheet: {m}")
        if re.search(r"\bmeters?\b|\b(?:the|in|won|ran) (?:100|200|400|800|1600|3200)\b(?!M)", text, re.I):
            issues.append("event worded without the site's name (use '800M', '100M Hurdles', '4x400M Relay')")
        score = fs.get("meetScore")
        if re.search(r"\bstrong day\b", text, re.I) and not (score and score >= 75):
            issues.append(f"'strong day' but meetScore is {score}")
        if "teamResults" not in fs and re.search(r"team (title|standing|score|result)|\bwon the meet\b|\b(boys|girls) (finished|placed|took) (first|second|third|\d)|\bof \d+ teams\b", text, re.I):
            issues.append("team result mentioned but the fact sheet has none")
        # attribution windows: a PR / record claim near a mark must be backed by that row's flag
        for m in MARK.finditer(text):
            mk = m.group(0).rstrip(".")
            cands = [r for _, _, r in rows if r["mark"] == mk]
            if not cands:
                continue
            win = clause_window(text, m.start(), m.end()).lower()
            if "personal best" in win and not any(r.get("pr") for r in cands) and not all("relayTeam" in r for r in cands):
                issues.append(f"check PR claim near {mk} (window heuristic; often a false positive)")
            if ("school record" in win or "school-record" in win) and not any(r.get("sr") for r in cands):
                issues.append(f"check school-record claim near {mk}")
        npr = sum(1 for _, _, r in rows if r.get("pr"))
        print(f"{mid}: {words}w, {npr} PR rows in sheet — " + ("ok" if not issues else ""))
        for i in issues:
            print("   -", i)
        problems += bool(issues)
    print(f"\n{len(sums)} checked, {problems} with findings. This is a mechanical check: still read each draft against its fact sheet.")


def cmd_import(a):
    sums = json.load(open(a.summaries))
    for mid, text in sums.items():
        data = urllib.parse.urlencode({"meetId": mid, "text": text, "model": a.model}).encode()
        with urllib.request.urlopen(urllib.request.Request(f"{a.base}/Admin/MeetSummaries/Import", data=data), timeout=120) as r:
            print(f"{mid}: HTTP {r.status}")


def cmd_audit(a):
    page = get(f"{a.base}/Admin/MeetSummaries?seasonId={a.season_id}")
    cards = re.findall(r'<div class="card" id="meet-(\d+)">.*?<div class="summary[^"]*">(.*?)</div>', page, re.S)
    bad = 0
    for mid, body in cards:
        path = os.path.join(a.facts, f"{mid}.json")
        if not os.path.exists(path):
            continue
        fs = json.load(open(path))
        rows = list(rows_of(fs))
        names = {n for _, _, r in rows for n in ([r["athlete"]] if "athlete" in r else r.get("relayTeam", []))}
        evs = {e for _, e, _ in rows}
        plain = html.unescape(re.sub(r"<a [^>]*>.*?</a>", "", body))
        miss = [f"athlete {n}" for n in names if n in plain]
        miss += [f"event '{plain[max(0, m.start()-20):m.end()+3]}'" for e in sorted(evs, key=len, reverse=True)
                 for m in re.finditer(r"(?<!\w)" + re.escape(e) + r"(?!\w)", plain, re.I)]
        n_links = len(re.findall(r'href="/(?:roster|events)/', body))
        print(f"{mid}: {n_links} links" + (f"; UNLINKED {miss[:4]}" if miss else "; all linked"))
        bad += bool(miss)
    print(f"{len(cards)} cards, {bad} with unlinked mentions")


def main():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = p.add_subparsers(dest="cmd", required=True)
    s = sub.add_parser("list"); s.add_argument("--base", required=True); s.add_argument("--season-name"); s.add_argument("--season-id", type=int); s.set_defaults(f=cmd_list)
    s = sub.add_parser("fetch"); s.add_argument("--base", required=True); s.add_argument("--out", required=True); s.add_argument("meet_ids", nargs="+"); s.set_defaults(f=cmd_fetch)
    s = sub.add_parser("validate"); s.add_argument("--facts", required=True); s.add_argument("--summaries", required=True); s.set_defaults(f=cmd_validate)
    s = sub.add_parser("import"); s.add_argument("--base", required=True); s.add_argument("--summaries", required=True)
    s.add_argument("--model", default="claude-opus (written by subagent in Claude Code)"); s.set_defaults(f=cmd_import)
    s = sub.add_parser("audit"); s.add_argument("--base", required=True); s.add_argument("--facts", required=True); s.add_argument("--season-id", type=int, required=True); s.set_defaults(f=cmd_audit)
    a = p.parse_args(); a.f(a)


if __name__ == "__main__":
    main()
