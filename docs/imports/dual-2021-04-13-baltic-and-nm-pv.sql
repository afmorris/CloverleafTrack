-- 2021 Woodridge Dual (owner correction): Isaac Baltic cleared 7'6" (O) and missed 8' three times; Grace Goldsberry and Avery Kott no-marked at 5' (XXX) in the pole vault. Id-independent; re-runnable.
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT = (SELECT TOP 1 Id FROM Meets WHERE Name=N'2021 Woodridge Dual' AND CAST(Date AS date)='2021-04-13');
DECLARE @pid INT, @a INT, @e INT;
IF @m IS NULL THROW 50110, 'meet missing', 1;
-- Baltic: back to 7'6" (90) with the corrected ladder
UPDATE p SET p.DistanceInches=90 FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND a.FirstName+N' '+a.LastName=N'Isaac Baltic' AND e.Gender=1 AND e.Name=N'Pole Vault';
SET @pid = (SELECT TOP 1 p.Id FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND a.FirstName+N' '+a.LastName=N'Isaac Baltic' AND e.Gender=1 AND e.Name=N'Pole Vault');
IF @pid IS NULL THROW 50111, 'Baltic PV missing', 1;
DELETE FROM PerformanceBars WHERE PerformanceId=@pid;
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (@pid, 84, 'O'), (@pid, 90, 'O'), (@pid, 96, 'XXX');
-- Goldsberry and Kott: no-mark rows with their 5' ladder
SET @e = (SELECT TOP 1 Id FROM Events WHERE Name=N'Pole Vault' AND Gender=2 AND Environment=1);
SET @a = (SELECT TOP 1 Id FROM Athletes WHERE FirstName=N'Grace' AND LastName=N'Goldsberry' AND Gender=2);
IF @a IS NULL OR @e IS NULL THROW 50112, 'Goldsberry/event missing', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE AthleteId=@a AND MeetId=@m AND EventId=@e) INSERT INTO Performances (AthleteId, MeetId, EventId, TimeSeconds, DistanceInches, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (@a, @m, @e, NULL, NULL, 0, 0, 0, 1);
SET @pid = (SELECT Id FROM Performances WHERE AthleteId=@a AND MeetId=@m AND EventId=@e);
IF NOT EXISTS (SELECT 1 FROM PerformanceBars WHERE PerformanceId=@pid) INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (@pid, 60, 'XXX');
SET @a = (SELECT TOP 1 Id FROM Athletes WHERE FirstName=N'Avery' AND LastName=N'Kott' AND Gender=2);
IF @a IS NULL THROW 50113, 'Kott missing', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE AthleteId=@a AND MeetId=@m AND EventId=@e) INSERT INTO Performances (AthleteId, MeetId, EventId, TimeSeconds, DistanceInches, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (@a, @m, @e, NULL, NULL, 0, 0, 0, 1);
SET @pid = (SELECT Id FROM Performances WHERE AthleteId=@a AND MeetId=@m AND EventId=@e);
IF NOT EXISTS (SELECT 1 FROM PerformanceBars WHERE PerformanceId=@pid) INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (@pid, 60, 'XXX');
COMMIT TRANSACTION;
EXEC sp_RebuildLeaderboards;
