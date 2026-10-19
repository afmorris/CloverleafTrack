-- Owner-reviewed marks from the 12 "large differences" (docs/results-level-gap-report.md, BRAIN.md [C77]). Only the four rows where the owner chose the
-- official (MileSplit) mark are changed; each UPDATE is guarded by the old value.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
-- Clay Combs, 2025 Nikki Reynolds discus: 157' 10" -> 157' 0"
UPDATE p SET DistanceInches = 1884 FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = 'Clay' AND a.LastName = 'Combs' AND m.Name = '2025 Nikki Reynolds Invitational' AND CAST(m.Date AS date) = '2025-05-03' AND e.Name = 'Discus' AND ABS(p.DistanceInches - 1894) < 0.001;
-- Dylan Wittman, 2024 Stars discus: 103' 3" -> 100' 8"
UPDATE p SET DistanceInches = 1208 FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = 'Dylan' AND a.LastName = 'Wittman' AND m.Name = '2024 Stars of Track and Field' AND CAST(m.Date AS date) = '2024-05-06' AND e.Name = 'Discus' AND ABS(p.DistanceInches - 1239) < 0.001;
-- Timmy Shores, 2024 Regional discus: 128' 6" -> 129' 7"
UPDATE p SET DistanceInches = 1555 FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = 'Timmy' AND a.LastName = 'Shores' AND m.Name = '2024 Division I Regional Track Meet' AND CAST(m.Date AS date) = '2024-05-24' AND e.Name = 'Discus' AND ABS(p.DistanceInches - 1542) < 0.001;
-- Nick Bailey, 2017 Division II District 800M: mark 2:18.46 -> 2:12.94 (prelim); chain prelim 2:12.94 -> final 2:18.46
DECLARE @pid INT = (SELECT p.Id FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = 'Nick' AND a.LastName = 'Bailey' AND m.Name = '2017 Division II District Track Meet' AND CAST(m.Date AS date) = '2017-05-20' AND e.Name = '800M' AND ABS(p.TimeSeconds - 138.46) < 0.001);
IF @pid IS NOT NULL
BEGIN
    UPDATE Performances SET TimeSeconds = 132.94 WHERE Id = @pid;
    IF NOT EXISTS (SELECT 1 FROM PerformanceRounds WHERE PerformanceId = @pid)
    BEGIN
        INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@pid, 1, 132.94);
        INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@pid, 3, 138.46);
    END
END
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
