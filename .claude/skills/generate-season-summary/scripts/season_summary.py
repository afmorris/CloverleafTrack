#!/usr/bin/env python3
"""Helper for the generate-season-summary skill. Standard library only.

  season_summary.py list     --base URL                               seasons + summary state (JSON)
  season_summary.py fetch    --base URL --out DIR SEASONID             save the fact sheet as DIR/<id>.json
  season_summary.py validate --facts DIR --summary FILE.txt --season-id N   check a draft against its fact sheet
  season_summary.py import   --base URL --season-id N --summary FILE.txt [--model LABEL] [--overwrite]   save as Draft

The admin area has no authentication. BASE is the locally running web app. import refuses to replace an Approved summary
unless --overwrite is given (only after the owner has said yes).
"""
import argparse, json, os, re, sys, urllib.error, urllib.parse, urllib.request

MARK = re.compile(r"\d+:\d\d(?:\.\d\d)?|\d+\.\d+|\d+'\s?\d*\.?\d*\"?")
NUMBER = re.compile(r"\d[\d,]*(?:\.\d+)?")
NAME = re.compile(r"\b([A-Z][a-z]+(?:-[A-Z][a-z]+)? [A-Z][a-z]+(?:-[A-Z][a-z]+)?)\b")


def get(url):
    try:
        with urllib.request.urlopen(url, timeout=120) as r:
            return r.read().decode()
    except urllib.error.HTTPError:
        raise
    except urllib.error.URLError as e:
        sys.exit(f"Cannot reach {url}\n({e.reason}). Start the app, e.g.: dotnet run --project CloverleafTrack.Web --urls http://localhost:5101 --no-launch-profile")


def names_in(node, out):
    """Every string in the sheet that could be a person: values of athlete/holders/etc. keys."""
    if isinstance(node, dict):
        for k, v in node.items():
            if k in ("athlete", "holders") or k.endswith("Name"):
                for s in (v if isinstance(v, list) else [v]):
                    if isinstance(s, str):
                        out.add(s)
            names_in(v, out)
    elif isinstance(node, list):
        for v in node:
            names_in(v, out)


def cmd_list(a):
    print(json.dumps(json.loads(get(f"{a.base}/Admin/SeasonSummaries/List")), indent=1))


def cmd_fetch(a):
    os.makedirs(a.out, exist_ok=True)
    try:
        body = get(f"{a.base}/Admin/SeasonSummaries/FactSheet?seasonId={a.season_id}")
    except urllib.error.HTTPError as e:
        sys.exit(f"HTTP {e.code}: season has no results to summarize")
    open(os.path.join(a.out, f"{a.season_id}.json"), "w").write(body)
    print(f"saved {len(body)} bytes to {a.out}/{a.season_id}.json")


def cmd_validate(a):
    fs = json.load(open(os.path.join(a.facts, f"{a.season_id}.json")))
    raw = json.dumps(fs, ensure_ascii=False)
    text = open(a.summary).read().strip()
    issues = []
    words = len(text.split())
    if not (100 <= words <= 240):
        issues.append(f"word count {words} (target 120-220)")
    people = set()
    names_in(fs, people)
    people |= {n for p in list(people) for n in p.split(", ")}
    firsts = {n.split()[0] for n in people}
    for n in set(NAME.findall(text)):
        if n in people or n in raw:
            continue
        if n.split()[0] in firsts:
            issues.append(f"name not in fact sheet: {n}")
        else:
            issues.append(f"capitalized phrase not in fact sheet (a person? check): {n}")
    for m in {m.rstrip(".") for m in MARK.findall(text)}:
        if m not in raw and m.replace('"', '\\"') not in raw:
            issues.append(f"mark not in fact sheet: {m}")
    marks = {m.rstrip(".") for m in MARK.findall(text)}
    sheet_numbers = set(NUMBER.findall(raw))
    for n in set(NUMBER.findall(text)):
        n = n.rstrip(",.")
        if n in marks or any(n in m for m in marks) or re.fullmatch(r"20\d\d", n):
            continue
        if n.replace(",", "") not in {s.replace(",", "") for s in sheet_numbers}:
            issues.append(f"number not in fact sheet: {n}")
    if re.search(r"\bmeters?\b|\b(?:the|in|won|ran) (?:100|200|400|800|1600|3200)\b(?!M)", text, re.I):
        issues.append("event worded without the site's name (use '800M', '100M Hurdles')")
    if "dualRecord" not in fs and re.search(r"\bdual (?:meet )?record\b|\bundefeated\b|\bwon \w+ duals\b", text, re.I):
        issues.append("dual record mentioned but the fact sheet has none")
    if "scoringLeaders" not in fs and re.search(r"\bpoints\b|\bscoring leader", text, re.I):
        issues.append("points / scoring leaders mentioned but the fact sheet has none")
    if re.search(r"\bhistor(?:y|ic)\b|\bbest season\b|\bfirst time ever\b|\bcompared to (?:last|previous)", text, re.I):
        issues.append("comparison to other seasons / history (not in the fact sheet)")
    for r in fs.get("schoolRecords", []):
        if r.get("stillRecord") is False and r["mark"] in text and re.search(r"current (?:school )?record", text, re.I):
            issues.append(f"{r['mark']} is no longer the record but the text may call it current")
    print(f"{words} words — " + ("ok" if not issues else ""))
    for i in issues:
        print("   -", i)
    print("\nMechanical check only: read the draft against the fact sheet yourself (word-form numbers, 'most', 'first', 'all', events, genders).")
    sys.exit(1 if issues else 0)


def cmd_import(a):
    text = open(a.summary).read().strip()
    q = {"seasonId": a.season_id, "text": text, "model": a.model}
    if a.overwrite:
        q["overwrite"] = "true"
    req = urllib.request.Request(f"{a.base}/Admin/SeasonSummaries/Import", data=urllib.parse.urlencode(q).encode())
    try:
        with urllib.request.urlopen(req, timeout=120) as r:
            print(f"HTTP {r.status}: saved as Draft")
    except urllib.error.HTTPError as e:
        sys.exit(f"HTTP {e.code}: {e.read().decode()}")


def main():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = p.add_subparsers(dest="cmd", required=True)
    s = sub.add_parser("list"); s.add_argument("--base", required=True); s.set_defaults(f=cmd_list)
    s = sub.add_parser("fetch"); s.add_argument("--base", required=True); s.add_argument("--out", required=True); s.add_argument("season_id", type=int); s.set_defaults(f=cmd_fetch)
    s = sub.add_parser("validate"); s.add_argument("--facts", required=True); s.add_argument("--summary", required=True); s.add_argument("--season-id", type=int, required=True); s.set_defaults(f=cmd_validate)
    s = sub.add_parser("import"); s.add_argument("--base", required=True); s.add_argument("--season-id", type=int, required=True); s.add_argument("--summary", required=True)
    s.add_argument("--model", default="claude-opus (written by subagent in Claude Code)"); s.add_argument("--overwrite", action="store_true"); s.set_defaults(f=cmd_import)
    a = p.parse_args(); a.f(a)


if __name__ == "__main__":
    main()
