---
name: create-season
description: Create the next CloverleafTrack season (a row in the ctf database's Seasons table), optionally making it the current season. Use when the user asks to create, add, or start a new season.
disable-model-invocation: true
---

# Create a CloverleafTrack season

Adds one row to `Seasons` in the `ctf` SQL Server database, the same thing `/Admin/Seasons/Create` does. This writes to the live database, so always preview and get a clear yes before running the INSERT.

## Conventions (from the existing data)

- `Name` is `YYYY-YYYY`, e.g. `2025-2026`. Seasons are looked up by this name in URLs (`/seasons/{name}`), so it must be unique.
- `StartDate` is Dec 1 of the first year; `EndDate` is Jun 30 of the second year (indoor starts in winter, outdoor ends at state).
- Exactly one season has `IsCurrentSeason = 1`. The admin controller unsets all other current seasons when one is made current; do the same.
- `Status` is `SeasonStatus`: 1 Draft, 2 Importing, 3 Partial, 4 RecordOnly, 5 Complete. A new season is **Draft (1)**.
- `ScoringEnabled` gates the public `/seasons/{name}/scoring` page (404 when 0). Default to 0 unless the user wants team scoring for the season.
- `Notes` is optional free text, usually NULL.

## Steps

1. **Connect.** The connection string is `DefaultConnection` in `CloverleafTrack.Web/appsettings.json`. Export the password to `SQLCMDPASSWORD` without printing it, and use `sqlcmd -S <server> -d ctf -U <user> -C -W -s'|'` (install with `brew install sqlcmd` if missing):
   ```bash
   export SQLCMDPASSWORD="$(grep -o 'Password=[^;"]*' CloverleafTrack.Web/appsettings.json | cut -d= -f2)"
   ```
   Take the server and user from that same connection string. Never echo the password.
2. **Look at what exists.** `SELECT TOP 5 Id, Name, StartDate, EndDate, IsCurrentSeason, Status, ScoringEnabled FROM Seasons ORDER BY StartDate DESC`. The new name is the one after the latest `Name`, unless the user gave one. Stop if that name already exists.
3. **Ask only what you can't infer:** make it the current season now (usually yes when the season is about to start, no when staging ahead), and whether to enable scoring. Everything else follows the conventions above.
4. **Preview.** Show the exact row you will insert and whether another season will lose its current flag. Wait for a clear yes.
5. **Insert in one transaction**, clearing the old current flag first only when the new season becomes current:
   ```sql
   BEGIN TRANSACTION;
   UPDATE Seasons SET IsCurrentSeason = 0 WHERE IsCurrentSeason = 1;   -- only if making it current
   INSERT INTO Seasons (Name, StartDate, EndDate, IsCurrentSeason, Notes, Status, ScoringEnabled)
   OUTPUT INSERTED.Id
   VALUES (@Name, @Start, @End, @IsCurrent, NULL, 1, @Scoring);
   COMMIT;
   ```
   Use sqlcmd variables (`-v`) or literal values you have already validated; the name is `YYYY-YYYY` digits only.
6. **Verify** by re-selecting the top rows, confirm exactly one `IsCurrentSeason = 1`, and report the new Id and `/Admin/Seasons/Edit/{id}`.

## What this does not do

- **No meets.** Create them in `/Admin/Meets` (each needs a Season, MeetType, MeetLevel). `Meets.SeasonId` is required.
- **No roster changes.** Athlete class is derived from `GraduationYear`, so there is nothing to roll over. Athletes who graduated may still have `IsActive = 1`; mention it if the user is rolling over, but do not change it unasked.
- **No leaderboard rebuild.** `sp_RebuildLeaderboards` runs automatically after performance writes, not season writes.
- Does not edit the code, schema, or BRAIN.md; a season is data, not a code change.
