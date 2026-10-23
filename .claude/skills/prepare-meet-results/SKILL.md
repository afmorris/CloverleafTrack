---
name: prepare-meet-results
description: Turn a source of meet results (Coach Gucker's scanned dual sheets, an athletic.net meet, a Hy-Tek export, a partial away-dual list) into a meet-results JSON file for the admin Import screen, with the review done in chat first. Use when the user asks to prepare, convert, or import results or pre-meet entries for a meet from a scan, link, or pasted text.
---

# Prepare a meet-results file

The site has no AI. Your job is the AI half: read the source, produce a file in the format of `docs/meet-results-format.md`, and settle every doubt with the owner **before** the file is uploaded at `/Admin/MeetImport?meetId=N` (that screen then does its own matching and shows a review grid). Never write to the database from this skill.

## Steps
1. **Identify the meet** (name, date, id). Get its context: `GET /Admin/MeetImport/Context?meetId=N` on a running local site, or query the `ctf` database read-only (meet type, opponents from `MeetParticipants`/`Schools`, events for the meet's environment, athletes, existing entries and performances). Note what is already stored so the file only carries what is new or different.
2. **Read the source carefully.** Scans: read every page, orientation can change between passes, so identify a page by its content (event heading). athletic.net: use the scripts in `scripts/athleticnet` (read the rendered DOM; the API 403s). Hy-Tek / MileSplit text: see `scripts/crawl`.
3. **Write only Cloverleaf's results.** Away duals often report only the top three: list just what the source shows, with the places it gives. Do not invent places, marks, or athletes.
4. **Use the site's names.** Event names exactly as in the Context (or the loose forms the format allows); athletes as `First Last` matching the roster. If a name is uncertain, check the roster by grade and gender, and say so in `notes`.
5. **Marks.**
   - Field events: the sheet's **Best** column is the mark, not your sum of the attempts (BRAIN.md [C116]). The form writes feet | inches as "15 | 3/4" = 15' 0.75".
   - Hand-timed tenths are sometimes printed as hundredths in Hy-Tek (27.6 shows as 27.06): the tenths are right.
   - Pole vault / high jump with no cleared height is `status: "NH"`; all fouls is `NM`.
   - Seated (wheelchair) events never score for the team; the importer handles points, you only give the mark and place.
6. **Places.** Invitational: `place` = overall place. Dual / double dual: one entry in `placings` per opponent (`{ "opponent": "<school as on the meet>", "place": n }`). A dual relay only places for the winning team (5-0), but give the places the sheet shows.
7. **Ask about every doubt.** Illegible digits, struck-through marks, a name that fits two athletes, a page you cannot read: list them for the owner and wait. Put the resolved doubts in `notes` ("Smith 400 read as 52.4 by owner").
8. **Save** the file to `docs/imports/<yyyy-mm-dd>-<meet-slug>.json` (the repo keeps these like the SQL imports), tell the owner the Import URL, and what to expect on the review screen (counts of new / differs / needs attention).
9. After the owner applies it, offer the follow-ups that are still manual: round chains, attempt series, bar ladders, and a meet recap (`generate-meet-summary`).

## Do not
- Do not guess a mark or a name to make the file look complete.
- Do not edit stored results here; a "Differs" row on the review screen is the owner's decision.
