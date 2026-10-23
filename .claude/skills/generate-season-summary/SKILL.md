---
name: generate-season-summary
description: Write, check and import the AI-written "season in words" for a CloverleafTrack season as a Draft for the owner to review in /Admin/SeasonSummaries. Use when the user asks to write, generate, redo or refresh the season summary / season wrap text, e.g. "write the 2025-2026 season summary". For single meets use generate-meet-summary.
---

# Generate a season summary

Writes a 120-220 word recap of one season from its fact sheet (built from the season wrap's numbers: totals, school records, standouts, breakouts, biggest jumps, seniors, dual record) and saves it as a **Draft** in `SeasonSummaries`. The owner approves it in `/Admin/SeasonSummaries`; only **Approved** text shows on `/seasons/{name}/wrap` under "The season in words". This skill never approves anything.

AI never runs inside the site: an Opus subagent writes the text here in chat, a script checks it against the data, the app's `Import` endpoint saves it.

## Before you start

1. **A running web app** (`BASE=http://localhost:5101` unless told otherwise). Check `curl -s "$BASE/Admin/SeasonSummaries/List"`; if down, start `dotnet run --project CloverleafTrack.Web --urls http://localhost:5101 --no-launch-profile` and wait. It connects to the live `ctf` database, so you read and write real data.
2. The table must exist: `docs/migration_season_summaries.sql` applied to `ctf`. `List` returning a 500 means it is not; tell the user, do not run the migration on the live database without asking.
3. Helper: `python3 -I .claude/skills/generate-season-summary/scripts/season_summary.py` (below: `$S`; `--help`). A scratch directory for the fact sheet and draft (the session scratchpad, not the repo).

## Steps

1. **Pick the season.** `$S list --base $BASE` gives each season's `seasonId`, `status` (Draft/Approved/none) and whether it is current. Match the user's wording ("2025-2026").
2. **Protect approved work.** If the season's summary is Approved, say that importing replaces it and returns it to Draft (pulling it off the public wrap until re-approved), and get a clear yes before going on. `import` refuses without `--overwrite`; pass that flag only after the yes.
3. **Fetch.** `$S fetch --base $BASE --out $SCRATCH/fs <seasonId>`. A failure means the season has no results. Skim the sheet: `inProgress: true` means the numbers are "so far" and the text must say so.
4. **Write with Opus.** Spawn a subagent with `model: "opus"` using `references/subagent-prompt.md`. Do not write it yourself or use another model unless the user says so.
5. **Validate.** `$S validate --facts $SCRATCH/fs --summary $SCRATCH/draft.txt --season-id <id>`. It flags any person name, mark or number not in the fact sheet, word count, non-site event wording, and claims about dual records, points or history the sheet does not carry. **Then read the draft against the fact sheet yourself**; the script cannot catch a swapped athlete, a wrong "most", or a record called current when `stillRecord` is false. Fix small things by editing the file; send substantive problems back to Opus.
6. **Import.** `$S import --base $BASE --season-id <id> --summary $SCRATCH/draft.txt` (add `--overwrite` only after step 2's yes). Saved as Draft.
7. **Report**: the word count, anything you changed, claims you are unsure of, and the review link `$BASE/Admin/SeasonSummaries`. Tell the user to read and approve there.

## Rules that keep the text honest

- The writing rules are `SeasonSummaryPrompt.SystemPrompt` in `CloverleafTrack.Services/SeasonSummaryPrompt.cs`. If you change the rules or the fact-sheet shape, bump `SeasonSummaryPrompt.Version` and tell the user existing drafts were written under the old rules.
- Data rules the writer must respect (they are also in the fact sheet's `notes`): the headline PR count counts a first-ever mark as a PR (generous); only "breakouts" / "biggest jumps" are real improvements; a school record whose `stillRecord` is false has since been broken; seated events never score for the team.
- Never edit the database directly, never mark anything Approved, never call the Anthropic API.
- The text can go stale if results change after it was written (the season is in progress, or marks are edited). Nothing detects this; regenerate when asked.

## Related

Meet recaps: `generate-meet-summary`. Design and history: `BRAIN.md` entry [C124].
