-- Two relay marks corrected against the official results (BRAIN.md [C78]). Each UPDATE is guarded by the old value.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
-- 2025 Green Twilight Relays boys 4x1600M: 25:42.98 (the girls' time) -> 19:56.86
UPDATE p SET TimeSeconds = 1196.86 FROM Performances p JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE m.Name LIKE '%Green Twilight%' AND YEAR(m.Date) = 2025 AND e.Name = '4x1600M Relay' AND e.Gender = 1 AND p.AthleteId IS NULL AND ABS(p.TimeSeconds - 1542.98) < 0.001;
-- 2024 Division I District girls 4x400M: 4:15.93 (prelim) -> 4:10.33 (final); chain prelim/final
DECLARE @pid INT = (SELECT p.Id FROM Performances p JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE m.Name LIKE '%2024 Division I District%' AND YEAR(m.Date) = 2024 AND e.Name = '4x400M Relay' AND e.Gender = 2 AND p.AthleteId IS NULL AND ABS(p.TimeSeconds - 255.93) < 0.001);
IF @pid IS NOT NULL
BEGIN
    UPDATE Performances SET TimeSeconds = 250.33 WHERE Id = @pid;
    IF NOT EXISTS (SELECT 1 FROM PerformanceRounds WHERE PerformanceId = @pid)
    BEGIN
        INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@pid, 1, 255.93);
        INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@pid, 3, 250.33);
    END
END
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
