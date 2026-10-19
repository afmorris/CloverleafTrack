-- 2018 Orange Relays: the official results file these two Cloverleaf runners' 3200M times under the 1600 header (confirmed by the owner:
-- both are 3200M runs). Their real 1600M marks (Sara Naples 6:17.24, Dillon West 5:12.55) stay as they are. BRAIN.md [C77].
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT = (SELECT Id FROM Meets WHERE Name = '2018 Orange Relays' AND Date = '2018-04-27');
DECLARE @sara INT = (SELECT Id FROM Athletes WHERE FirstName = 'Sara' AND LastName = 'Naples' AND Gender = 2);
DECLARE @dillon INT = (SELECT Id FROM Athletes WHERE FirstName = 'Dillon' AND LastName = 'West' AND Gender = 1);
DECLARE @e3200g INT = (SELECT Id FROM Events WHERE Name = '3200M' AND Gender = 2 AND Environment = 1);
DECLARE @e3200b INT = (SELECT Id FROM Events WHERE Name = '3200M' AND Gender = 1 AND Environment = 1);
IF @m IS NULL OR @sara IS NULL OR @dillon IS NULL OR @e3200g IS NULL OR @e3200b IS NULL THROW 50020, 'Meet, athlete or event not found', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = @m AND EventId = @e3200g AND AthleteId = @sara)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (@sara, @m, @e3200g, NULL, 768.68, 0, 0, 0, 0);
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = @m AND EventId = @e3200b AND AthleteId = @dillon)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (@dillon, @m, @e3200b, NULL, 601.05, 0, 0, 0, 0);
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
