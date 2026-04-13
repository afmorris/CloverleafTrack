-- =============================================================
-- Backfill MeetEntry rows for existing performances in the
-- 2025-2026 season that were entered before the Entries system
-- was in place.  Safe to re-run (all inserts are guarded with
-- NOT EXISTS).
-- =============================================================

BEGIN TRANSACTION;

DECLARE @SeasonId INT;
SELECT @SeasonId = Id FROM Seasons WHERE Name = '2025-2026';

IF @SeasonId IS NULL
BEGIN
    RAISERROR('Season "2025-2026" not found. Verify the season name and try again.', 16, 1);
    ROLLBACK;
    RETURN;
END

-- ── 1. Individual performances ────────────────────────────────
INSERT INTO MeetEntries (MeetId, EventId, AthleteId, PerformanceId)
SELECT
    p.MeetId,
    p.EventId,
    p.AthleteId,
    p.Id
FROM Performances p
INNER JOIN Meets m ON m.Id = p.MeetId
WHERE m.SeasonId = @SeasonId
  AND p.AthleteId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM MeetEntries me
      WHERE me.PerformanceId = p.Id AND me.Deleted = 0
  );

-- ── 2. Relay performances ─────────────────────────────────────
INSERT INTO MeetEntries (MeetId, EventId, AthleteId, PerformanceId)
SELECT
    p.MeetId,
    p.EventId,
    NULL,
    p.Id
FROM Performances p
INNER JOIN Meets m ON m.Id = p.MeetId
WHERE m.SeasonId = @SeasonId
  AND p.AthleteId IS NULL
  AND NOT EXISTS (
      SELECT 1 FROM MeetEntries me
      WHERE me.PerformanceId = p.Id AND me.Deleted = 0
  );

-- ── 3. Relay team members (MeetEntryAthletes) ─────────────────
INSERT INTO MeetEntryAthletes (MeetEntryId, AthleteId)
SELECT
    me.Id,
    pa.AthleteId
FROM MeetEntries me
INNER JOIN Performances p   ON p.Id  = me.PerformanceId
INNER JOIN PerformanceAthletes pa ON pa.PerformanceId = p.Id
INNER JOIN Meets m          ON m.Id  = me.MeetId
WHERE m.SeasonId    = @SeasonId
  AND p.AthleteId   IS NULL
  AND me.Deleted    = 0
  AND NOT EXISTS (
      SELECT 1 FROM MeetEntryAthletes mea WHERE mea.MeetEntryId = me.Id
  );

COMMIT;

-- ── Verification ──────────────────────────────────────────────
SELECT
    COUNT(*)                                                      AS TotalEntries,
    SUM(CASE WHEN me.AthleteId IS NOT NULL THEN 1 ELSE 0 END)    AS Individual,
    SUM(CASE WHEN me.AthleteId IS NULL     THEN 1 ELSE 0 END)    AS Relay,
    SUM(CASE WHEN me.PerformanceId IS NOT NULL THEN 1 ELSE 0 END) AS WithResult
FROM MeetEntries me
INNER JOIN Meets m ON m.Id = me.MeetId
WHERE m.SeasonId = @SeasonId
  AND me.Deleted = 0;
