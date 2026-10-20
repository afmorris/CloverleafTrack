-- From the official OHSAA state results 2001-2008 (https://www.ohsaa.org/sports/tf/<year>/resultslnk.htm -> bd1/gd1reslt<yy>.htm) (BRAIN.md [C95]). Owner-approved.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
-- Sean Hutson, 2005 OHSAA Division I State boys 1600M: 4:27.50 (267.5) -> 4:27.05 (267.05) (official results and athletic.net)
UPDATE p SET TimeSeconds = 267.05 FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = N'Sean' AND a.LastName = N'Hutson' AND m.Name = N'2005 OHSAA Division I State Track and Field Championships' AND e.Name = N'1600M' AND e.Gender = 1 AND ABS(p.TimeSeconds - 267.5) < 0.001;
-- Cory Beebe, 2005 OHSAA Division I State boys 300M hurdles: prelim 38.92 (Q) -> final 38.68 (mark stays 38.68)
DECLARE @pid INT = (SELECT p.Id FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = N'Cory' AND a.LastName = N'Beebe' AND m.Name = N'2005 OHSAA Division I State Track and Field Championships' AND e.Name = N'300M Hurdles' AND e.Gender = 1 AND ABS(p.TimeSeconds - 38.68) < 0.001);
IF @pid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PerformanceRounds WHERE PerformanceId = @pid)
    INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@pid, 1, 38.92), (@pid, 3, 38.68);
-- 2001 OHSAA Division I State (June 1-2, 2001; modelled on the 2002 State record: Dayton Welcome Stadium, season 2000-2001): Andy Bittner, grade 11, boys 3200M, 9:51.18 (11th)
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = N'2001 OHSAA Division I State Track and Field Championships')
    INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType)
    VALUES (N'2001 OHSAA Division I State Track and Field Championships', '2001-06-01', (SELECT Id FROM Locations WHERE Name = N'Dayton Welcome Stadium'), 1, 0, (SELECT Id FROM Seasons WHERE Name = N'2000-2001'), 3, 1, 1);
DECLARE @m INT = (SELECT Id FROM Meets WHERE Name = N'2001 OHSAA Division I State Track and Field Championships');
DECLARE @a INT = (SELECT Id FROM Athletes WHERE FirstName = N'Andy' AND LastName = N'Bittner' AND Gender = 1);
DECLARE @e INT = (SELECT Id FROM Events WHERE Name = N'3200M' AND Gender = 1 AND Environment = 1);
IF @m IS NULL OR @a IS NULL OR @e IS NULL THROW 50097, '2001 meet, Andy Bittner or the 3200M event not found', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = @m AND EventId = @e AND AthleteId = @a)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (@a, @m, @e, NULL, 591.18, 0, 0, 0, 0);
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
