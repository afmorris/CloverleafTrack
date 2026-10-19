-- Results-level comparison with athletic.net (BRAIN.md [C82]): fixes for the athletic.net-only meets. Each UPDATE is guarded by the old value.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
-- 2014 Division I District, Sean Brown pole vault: DB had 10' 6" (copied from James Reinhart's mark); athletic.net has Brown 10' 0" (12th), Reinhart 10' 6" (11th).
UPDATE p SET DistanceInches = 120 FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = 'Sean' AND a.LastName = 'Brown' AND m.Name = '2014 Division I District Track Meet' AND CAST(m.Date AS date) = '2014-05-21' AND e.Name = 'Pole Vault' AND ABS(p.DistanceInches - 126) < 0.001;
-- 2019 OHSAA Seated State, Brooklynn Crist: the DB held copies of her other 2019 marks (20.56 / 1:20.52 / 2:56.71); athletic.net has 23.85 / 1:22.15 / 2:54.49.
DECLARE @m INT = (SELECT Id FROM Meets WHERE Name = '2019 OHSAA Seated Division State Track and Field Championships' AND CAST(Date AS date) = '2019-05-31');
DECLARE @crist INT = (SELECT Id FROM Athletes WHERE FirstName = 'Brooklynn' AND LastName = 'Crist');
DECLARE @donahue INT = (SELECT Id FROM Athletes WHERE FirstName = 'Bridgette' AND LastName = 'Donahue');
DECLARE @e100 INT = (SELECT Id FROM Events WHERE Name = 'Seated 100M' AND Gender = 2 AND Environment = 1);
DECLARE @e400 INT = (SELECT Id FROM Events WHERE Name = 'Seated 400M' AND Gender = 2 AND Environment = 1);
DECLARE @e800 INT = (SELECT Id FROM Events WHERE Name = 'Seated 800M' AND Gender = 2 AND Environment = 1);
DECLARE @esp INT = (SELECT Id FROM Events WHERE Name = 'Seated Shot Put' AND Gender = 2 AND Environment = 1);
IF @m IS NULL OR @crist IS NULL OR @donahue IS NULL OR @e100 IS NULL OR @e400 IS NULL OR @e800 IS NULL OR @esp IS NULL THROW 50060, 'Meet, athlete or event not found', 1;
UPDATE Performances SET TimeSeconds = 23.85 WHERE MeetId = @m AND AthleteId = @crist AND EventId = @e100 AND ABS(TimeSeconds - 20.56) < 0.001;
UPDATE Performances SET TimeSeconds = 82.15 WHERE MeetId = @m AND AthleteId = @crist AND EventId = @e400 AND ABS(TimeSeconds - 80.52) < 0.001;
UPDATE Performances SET TimeSeconds = 174.49 WHERE MeetId = @m AND AthleteId = @crist AND EventId = @e800 AND ABS(TimeSeconds - 176.71) < 0.001;
-- Bridgette Donahue's 2019 seated state shot put (7' 4.75") was missing.
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = @m AND AthleteId = @donahue AND EventId = @esp)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (@donahue, @m, @esp, 88.75, NULL, 0, 0, 0, 0);
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
