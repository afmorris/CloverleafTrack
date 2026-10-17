---
name: generate-season-summaries
description: Write AI recaps for every meet in a CloverleafTrack season (iterates the whole season, in parallel batches) and import them as Drafts for review in /Admin/MeetSummaries. Use when the user asks for summaries for a season, all meets, or "the rest of the meets", e.g. "do 2026-2027".
---

# Generate summaries for a whole season

This is `generate-meet-summary` run over every meet in a season. Read that skill first; it defines the rules, the helper script, the validation, and the Draft/Approved safety behavior. This one adds the iteration.

## Steps

1. **Resolve the season.** `python3 -I .claude/skills/generate-meet-summary/scripts/summaries.py list --base $BASE --season-name <YYYY-YYYY>` (or `--season-id`; with neither, it uses the current season). Check the app is running first (see the meet skill). It returns every meet in date order with `hasSummary` and `status`.
2. **Decide the scope** and say it back in one line, e.g. "21 meets: 3 have no summary, 18 Drafts". The default is **only meets with no summary**. Redoing Drafts is fine if the user asked for a redo. **Never overwrite Approved summaries** without an explicit yes naming them, because importing resets them to Draft and takes them off the public page.
3. **Fetch fact sheets** for the in-scope meets (`fetch`). Meets that return 404 have no performances; skip them and list them in the report. Watch for any `WARNING` lines from `fetch` (listed results not matching the totals) and resolve them before writing.
4. **Batch and write in parallel.** Split the meets into groups of about 10-11. Start one Opus writer per group **in a single message** so they run concurrently, each using `../generate-meet-summary/references/subagent-prompt.md` and writing its own `out_<n>.json`. Any grouping works; put each meet's format (dual, double dual, invitational, JV) in the group's meet list.
5. **Merge and validate.** Combine the `out_*.json` objects into one file, confirm every in-scope meet has a draft, and run `validate`. Then read every draft against its sheet, as in the meet skill. Look especially at: meets with no `teamResults` (nothing about who won), JV meets, tiny meets (a short draft is correct), and the state/regional meets where only a few results exist.
6. **Import** (`import`), then **audit links** (`audit --season-id <N>`).
7. **Report** as a short table (meet, words, anything you changed or are unsure about), the skipped meets, and the review link `$BASE/Admin/MeetSummaries?seasonId=<N>`. Remind the user the drafts are not public until they approve them.

## Notes

- A new season's meets only have summaries once their performances are entered; run this after the results are in.
- Cost and time scale with meets: each writer takes about two minutes for ten meets.
- Athlete summaries are a separate feature and are not covered here.
