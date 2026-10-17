---
name: generate-athlete-summary
description: Write, check and import AI-written career recaps for CloverleafTrack athletes (one athlete, several, or a whole graduating class) as Draft summaries for review in /Admin/AthleteSummaries. Use when the user asks to write, generate, redo or refresh an athlete's summary, or summaries for a class such as "the class of 2026" or "last year's seniors".
---

# Generate athlete summaries

Writes an 80-140 word career recap per athlete from that athlete's fact sheet and saves it as a **Draft** in the `AthleteSummaries` table. The owner approves drafts in `/Admin/AthleteSummaries`. **Nothing from athlete summaries is shown on public pages yet**; this skill never approves anything.

Generation happens in chat: Opus subagents write the text, a script checks it against the data, and the app's `Import` endpoint saves it. No Anthropic API key is involved. The shape matches `generate-meet-summary`; read that skill for the shared ideas (running app, scratch directory, Opus only, approved-work protection).

## Before you start

1. **A running web app** at `BASE` (default `http://localhost:5101`; start it as described in `generate-meet-summary` if `curl -s "$BASE/Admin/AthleteSummaries/List"` fails). It connects to the live `ctf` database.
2. **Helper:** `python3 -I .claude/skills/generate-athlete-summary/scripts/athlete_summaries.py` (call it `$A`; `--help` lists commands). Use the session scratchpad for fact sheets and drafts, not the repo.

## Who gets a summary

**Only athletes who competed in their senior year**, whatever their earlier history (four years, one year, anything), because a class recap is for the seniors who finished the career. "Senior year" means any individual or relay performance at a meet in the school year ending in the graduation year (August 1 to July 31, the same boundary used for class labels). Athletes who left earlier are not written, are hidden on the review page by default, and are named under `excluded` in `list`. Write one of them only if the user asks for that athlete by name (`list --include-all`).

## Steps

1. **Find the athletes.** `$A list --base $BASE --class-year 2026` returns the eligible athletes in a graduation class with `athleteId`, `team`, `hasSummary` and `status`, plus the `excluded` names. "Last year's seniors" means the most recently graduated class (the list's default). For named athletes, match names to ids and confirm if ambiguous.
2. **Protect approved work.** Importing replaces the text and resets it to **Draft**. If any requested athlete is Approved, say so and get a clear yes first. By default write only athletes with no summary.
3. **Fetch fact sheets.** `$A fetch --base $BASE --out $SCRATCH/afs <ids...>`. A 404 means no recorded performances; skip and report. Heed any `WARNING` line.
4. **Write with Opus.** Start writers with `model: "opus"` from `references/subagent-prompt.md`, about 8-9 athletes each, all in one message so they run in parallel. Do not write the recaps yourself or use another model unless the user says so. The hand-back message is only a status note.
5. **Validate.** `$A validate --facts $SCRATCH/afs --summaries $SCRATCH/out.json`. It checks marks and names against the fact sheet, gendered pronouns, off-topic content (college, future, family, injuries), event wording, personal-best counts, the school-record count, stated all-time ranks, and school-record claims by mark. Then **read every draft against its sheet yourself**: the script cannot catch a wrong season, a swapped event, a mis-stated improvement or an invented fact. Fix small things by editing the JSON; send substantive problems back to the writer.
6. **Import.** `$A import --base $BASE --summaries $SCRATCH/out.json`.
7. **Audit links.** `$A audit --base $BASE --facts $SCRATCH/afs --class-year <N>` lists any athlete, meet or event name left unlinked on the review page. Events that exist both indoors and outdoors need the word "indoor" or "outdoor" before them to link; fix the text and re-import.
8. **Report** how many drafts were written, what you changed, what you are unsure of, and the review link `$BASE/Admin/AthleteSummaries?graduationYear=<N>`.

## Data traps (why the fact sheet looks the way it does)

- **No personal-best counts.** `PersonalBest` flags exist only for recent seasons, so the athlete page's "PRs" total undercounts older careers. The fact sheet omits it and writers must not state a count.
- **School records** come from all-time rank 1, never the stale `SchoolRecord` flag; `allTimeRank` appears only for the top 10. Relays appear with their `relay` teammates and never claim a personal best.
- `personalRecords` and `careerArc` cover the whole career; use them, not the season lists, for best marks and improvement.
- Class (Freshman to Senior) for each season comes from the graduation year; it is data, so "Class of 2026" is fine to say.
- Pronouns are avoided entirely; the data records only the boys/girls team, not how anyone refers to themselves.
- **Exhibition relays are real.** Odd relay results such as a "4x400M Relay" of about 8:00 are exhibition 4x4 relays the team runs for fun, not data errors. Writers may mention one or leave it out; never treat it as a typo.
- A summary goes stale if the athlete's performances are edited afterwards. Nothing detects this; regenerate when results change.

## Rules

Writing rules: `AthleteSummaryPrompt.SystemPrompt` in `CloverleafTrack.Services/AthleteSummaryPrompt.cs`. Changing them or the fact-sheet shape means bumping `AthleteSummaryPrompt.Version` and telling the user existing drafts used the old rules. Never edit the database directly, never mark anything Approved, never call the Anthropic API.
