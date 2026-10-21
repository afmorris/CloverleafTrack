-- Dual-meet relays score 5-0 (winner 5, everyone else 0); the first Ravenna-Streetsboro import gave 2nd/3rd relays 3/1. Only per-opponent placings (MeetParticipantId NOT NULL) of relay performances (AthleteId NULL) with Place > 1 are touched. Re-runnable.
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
UPDATE mp SET mp.FullPoints = 0, mp.SplitPoints = 0
FROM MeetPlacings mp
JOIN Performances p ON p.Id = mp.PerformanceId
WHERE p.AthleteId IS NULL AND mp.MeetParticipantId IS NOT NULL AND mp.Place > 1 AND (mp.FullPoints <> 0 OR mp.SplitPoints <> 0);
COMMIT TRANSACTION;
