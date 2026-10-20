-- Owner-reviewed mark differences from the baumspage comparison (BRAIN.md [C82], [C84]). Every UPDATE is guarded by the old value; old values are in the comments.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
-- Brian Livingstone, 2011 Suburban League Championships boys shot put: 51' 1" (613) -> 52' 1" (625)
UPDATE p SET DistanceInches = 625 FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = 'Brian' AND a.LastName = 'Livingstone' AND m.Name = '2011 Suburban League Championships' AND CAST(m.Date AS date) = '2011-05-14' AND e.Name = 'Shot Put' AND ABS(p.DistanceInches - 613) < 0.001;
-- Mandy Fugate, 2003 Division I District girls 200M: 27.38 -> 29.35
UPDATE p SET TimeSeconds = 29.35 FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = 'Mandy' AND a.LastName = 'Fugate' AND m.Name = '2003 Division I District Track Meet' AND CAST(m.Date AS date) = '2003-05-22' AND e.Name = '200M' AND e.Gender = 2 AND ABS(p.TimeSeconds - 27.38) < 0.001;
-- Sarah Schleich, 2008 Amherst Comet Relays long jump: 12' 2" (146) -> 13' 6.75" (162.75)
UPDATE p SET DistanceInches = 162.75 FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = 'Sarah' AND a.LastName = 'Schleich' AND m.Name = '2008 Amherst Comet Relays' AND CAST(m.Date AS date) = '2008-04-26' AND e.Name = 'Long Jump' AND ABS(p.DistanceInches - 146) < 0.001;
-- 2004 Division I District hurdles: DB held the final; use the faster semi and chain semi (round 2) -> final (round 3)
DECLARE @pid INT;
SET @pid = (SELECT p.Id FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = 'Cory' AND a.LastName = 'Beebe' AND m.Name = '2004 Division I District Track Meet' AND CAST(m.Date AS date) = '2004-05-20' AND e.Name = '110M Hurdles' AND ABS(p.TimeSeconds - 16.65) < 0.001);   -- 16.65 -> 15.81
IF @pid IS NOT NULL BEGIN UPDATE Performances SET TimeSeconds = 15.81 WHERE Id = @pid;
  IF NOT EXISTS (SELECT 1 FROM PerformanceRounds WHERE PerformanceId = @pid) BEGIN INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@pid, 2, 15.81), (@pid, 3, 16.65); END END
SET @pid = (SELECT p.Id FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = 'Cory' AND a.LastName = 'Beebe' AND m.Name = '2004 Division I District Track Meet' AND CAST(m.Date AS date) = '2004-05-20' AND e.Name = '300M Hurdles' AND ABS(p.TimeSeconds - 40.25) < 0.001);   -- 40.25 -> 39.94
IF @pid IS NOT NULL BEGIN UPDATE Performances SET TimeSeconds = 39.94 WHERE Id = @pid;
  IF NOT EXISTS (SELECT 1 FROM PerformanceRounds WHERE PerformanceId = @pid) BEGIN INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@pid, 2, 39.94), (@pid, 3, 40.25); END END
SET @pid = (SELECT p.Id FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = 'Kalyn' AND a.LastName = 'OShea' AND m.Name = '2004 Division I District Track Meet' AND CAST(m.Date AS date) = '2004-05-20' AND e.Name = '300M Hurdles' AND ABS(p.TimeSeconds - 49.1) < 0.001);   -- 49.1 -> 49.04
IF @pid IS NOT NULL BEGIN UPDATE Performances SET TimeSeconds = 49.04 WHERE Id = @pid;
  IF NOT EXISTS (SELECT 1 FROM PerformanceRounds WHERE PerformanceId = @pid) BEGIN INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@pid, 2, 49.04), (@pid, 3, 49.10); END END
-- Girls 4x800M relay, 2011 Suburban League: 654.27 -> 654.57
UPDATE p SET TimeSeconds = 654.57 FROM Performances p JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE m.Name = '2011 Suburban League Championships' AND CAST(m.Date AS date) = '2011-05-14' AND e.Name = '4x800M Relay' AND e.Gender = 2 AND p.AthleteId IS NULL AND ABS(p.TimeSeconds - 654.27) < 0.001;
-- Girls 4x400M relay, 2010 Suburban League: 270.18 -> 270.81
UPDATE p SET TimeSeconds = 270.81 FROM Performances p JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE m.Name = '2010 Suburban League Championships' AND CAST(m.Date AS date) = '2010-05-15' AND e.Name = '4x400M Relay' AND e.Gender = 2 AND p.AthleteId IS NULL AND ABS(p.TimeSeconds - 270.18) < 0.001;
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
