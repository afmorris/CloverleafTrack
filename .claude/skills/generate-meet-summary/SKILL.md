---
name: generate-meet-summary
description: Write, check and import AI-written recaps for one or more CloverleafTrack meets as Draft summaries for the owner to review in /Admin/MeetSummaries. Use when the user asks to write, generate, redo, or refresh the summary or recap of a specific meet (or a few named meets). For a whole season use generate-season-summaries.
---

# Generate meet summaries

Writes a 70-120 word recap per meet from that meet's fact sheet and saves it as a **Draft** in the `MeetSummaries` table. The owner approves drafts in `/Admin/MeetSummaries`; only **Approved** text shows on the public meet page (and becomes its meta description). This skill never approves anything.

Generation happens here, in chat: there are no generate buttons and no Anthropic API key. Opus subagents write the text; a script checks it against the data; the app's `Import` endpoint saves it.

## Before you start

1. **A running web app.** The summary endpoints live in the admin area (no authentication). Use `BASE=http://localhost:5101` unless the user says otherwise. Check with `curl -s "$BASE/Admin/MeetSummaries/List"`; if it is down, start it (`dotnet run --project CloverleafTrack.Web --urls http://localhost:5101 --no-launch-profile`, or a `preview_start` config for that command) and wait for it. The app connects to the live `ctf` database from `appsettings.json`, so you are reading and writing real data.
2. **The helper:** `python3 -I .claude/skills/generate-meet-summary/scripts/summaries.py` (standard library only; run `--help`). Below it is `$S`.
3. A scratch directory for fact sheets and drafts. Use the session scratchpad, not the repo.

## Steps

1. **Find the meets.** `$S list --base $BASE --season-name 2025-2026` returns each meet's `meetId`, `date`, `level`, and whether it has a summary and its `status` (Draft or Approved). Match the user's wording to meet ids; confirm if ambiguous.
2. **Protect approved work.** Importing replaces the text and resets the status to **Draft**, which removes a meet's summary from the public page until it is re-approved. If any requested meet is Approved, say so and get a clear yes before overwriting it.
3. **Fetch fact sheets.** `$S fetch --base $BASE --out $SCRATCH/fs <ids...>`. A 404 means the meet has no performances; skip it and say so. These JSON files are the only input the writer sees, and they already apply the data rules (school record = all-time rank 1, no relay PR flag, rank shown only if top 10).
4. **Sanity-check the sheet.** `fetch` prints a WARNING when the listed results do not match `totals.performances` or the `sr` rows do not match `totals.schoolRecords`. A warning means the meet page and fact sheet are dropping data (this is how the medley-relay bug showed up); stop and investigate before writing.
5. **Write with Opus.** Spawn a writer subagent with `model: "opus"` using `references/subagent-prompt.md` (fill in the facts dir, the meet list, and an output file). Do not write the recaps yourself, and do not use another model, unless the user says so. Its hand-back message is only a status note.
6. **Validate.** `$S validate --facts $SCRATCH/fs --summaries $SCRATCH/out.json`. It flags marks or names missing from the fact sheet, events worded other than the site's names, word count, a "strong day" without meetScore >= 75, team results claimed when the sheet has none, and PR/record claims not backed by the row's flag. Some PR-window flags are false positives; check them by looking at the row. **Then read every draft against its fact sheet yourself.** The script cannot catch a wrong placing, a swapped athlete, or an invented fact like "finished the regular season". Fix small problems by editing the JSON (for example trimming to 120 words); send anything substantive back to the writer or rewrite it with Opus.
7. **Import.** `$S import --base $BASE --summaries $SCRATCH/out.json`. Saved as Draft.
8. **Audit links.** `$S audit --base $BASE --facts $SCRATCH/fs --season-id <N>` confirms every athlete and event name on the review page is linked. Unlinked event names mean the writer used non-site wording; fix the text and re-import.
9. **Report** how many drafts were written, anything skipped or changed by you, any claims you were unsure of, and the review link `$BASE/Admin/MeetSummaries?seasonId=<N>`. Tell the user to read and approve there.

## Rules that keep the text honest

- The writing rules are `MeetSummaryPrompt.SystemPrompt` in `CloverleafTrack.Services/MeetSummaryPrompt.cs`. If you change the rules or the fact-sheet shape, bump `MeetSummaryPrompt.Version` and tell the user existing drafts were written under the old rules.
- Never edit the database directly, never mark anything Approved, never call the Anthropic API.
- A summary can go stale if performances are edited after it was written. Nothing detects this automatically; when the user says a meet's results changed, regenerate it.
- Relay members come as arrays of names; relays never carry a `pr` flag, so never claim a relay personal best.
- No team result in the fact sheet means say nothing about who won the team competition (double duals are scored per opponent and are often not entered).

## Related

Season-wide runs: `generate-season-summaries`. Design and history: `BRAIN.md` entry [C50].
