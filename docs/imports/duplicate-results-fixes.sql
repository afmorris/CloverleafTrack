-- Five athlete/meet/event pairs that had two results rows (BRAIN.md [C86]). Rows are found by athlete name + meet name/date + event + exact mark.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
CREATE TABLE #del (PerfId INT);
-- Bryce Neill, 2025 Ravenna-Streetsboro 200M: keep 27.14 (MileSplit), delete 27.00
INSERT INTO #del SELECT p.Id FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = N'Bryce' AND a.LastName = N'Neill' AND m.Name = N'2025 Ravenna-Streetsboro Double Dual' AND e.Name = N'200M' AND ABS(p.TimeSeconds - 27.0) < 0.001;
-- Jackie Baker, 1996 Medina County 100M: keep 14.6, delete 14.7
INSERT INTO #del SELECT p.Id FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = N'Jackie' AND a.LastName = N'Baker' AND m.Name = N'1996 Medina County Championships' AND e.Name = N'100M' AND ABS(p.TimeSeconds - 14.7) < 0.001;
-- Nolan McNeal, 2021 Black River Dual 300M hurdles: keep 46.1, delete 46.2
INSERT INTO #del SELECT p.Id FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = N'Nolan' AND a.LastName = N'McNeal' AND m.Name = N'2021 Black River Dual' AND e.Name = N'300M Hurdles' AND ABS(p.TimeSeconds - 46.2) < 0.001;
-- Weston Miller, 2026 New London Dual long jump: identical 156.5 twice; keep the row that has the meet entry, delete the other
INSERT INTO #del SELECT p.Id FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = N'Weston' AND a.LastName = N'Miller' AND m.Name = N'2026 New London Dual' AND e.Name = N'Long Jump' AND ABS(p.DistanceInches - 156.5) < 0.001
   AND NOT EXISTS (SELECT 1 FROM MeetEntries me WHERE me.PerformanceId = p.Id);
IF (SELECT COUNT(*) FROM #del) <> 4 THROW 50080, 'Expected exactly 4 duplicate rows to delete', 1;
IF EXISTS (SELECT 1 FROM MeetPlacings WHERE PerformanceId IN (SELECT PerfId FROM #del)) OR EXISTS (SELECT 1 FROM PerformanceAthletes WHERE PerformanceId IN (SELECT PerfId FROM #del))
    THROW 50081, 'A duplicate row has placings or relay legs attached', 1;
DELETE FROM MeetEntries WHERE PerformanceId IN (SELECT PerfId FROM #del);   -- Baker's second pre-meet entry
DELETE FROM PersonalBestHistory WHERE PerformanceId IN (SELECT PerfId FROM #del);
DELETE FROM Leaderboards WHERE PerformanceId IN (SELECT PerfId FROM #del);
DELETE FROM PerformancePercentiles WHERE PerformanceId IN (SELECT PerfId FROM #del);
DELETE FROM Performances WHERE Id IN (SELECT PerfId FROM #del);
DROP TABLE #del;
-- Gordy Surrarrer's second Ravenna discus row (82' 6") is Gunnar's throw: Gunnar has no discus entry there and throws 79'-83' elsewhere, Gordy 65'-75'
DECLARE @gordy INT = (SELECT Id FROM Athletes WHERE FirstName = N'Gordy' AND LastName = N'Surrarrer' AND GraduationYear = 2029);
DECLARE @gunnar INT = (SELECT Id FROM Athletes WHERE FirstName = N'Gunnar' AND LastName = N'Surrarrer' AND GraduationYear = 2028);
DECLARE @m INT = (SELECT Id FROM Meets WHERE Name = N'2026 Ravenna-Streetsboro Double Dual' AND CAST(Date AS date) = '2026-04-08');
DECLARE @e INT = (SELECT Id FROM Events WHERE Name = N'Discus' AND Gender = 1 AND Environment = 1);
IF @gordy IS NULL OR @gunnar IS NULL OR @m IS NULL OR @e IS NULL THROW 50082, 'Athlete, meet or event not found', 1;
IF EXISTS (SELECT 1 FROM Performances WHERE MeetId = @m AND EventId = @e AND AthleteId = @gunnar) THROW 50083, 'Gunnar already has a discus result at this meet', 1;
DECLARE @pid INT = (SELECT Id FROM Performances WHERE MeetId = @m AND EventId = @e AND AthleteId = @gordy AND ABS(DistanceInches - 990) < 0.001);
IF @pid IS NOT NULL
BEGIN
    UPDATE Performances SET AthleteId = @gunnar WHERE Id = @pid;
    UPDATE MeetEntries SET AthleteId = @gunnar WHERE PerformanceId = @pid;
END
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
