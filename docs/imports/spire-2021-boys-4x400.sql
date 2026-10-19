-- 2021 SPIRE Ohio Championship Preview boys 4x400M Relay, 4:08.95 (9th). Legs supplied by the owner from the meet video (leg 3 is a best guess: Cole Yagersz).
-- Neither athletic.net nor MileSplit lists legs for this team. BRAIN.md [C78].
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT = (SELECT Id FROM Meets WHERE Name = '2021 SPIRE Ohio Championship Preview Meet' AND CAST(Date AS date) = '2021-01-30');
DECLARE @e INT = (SELECT Id FROM Events WHERE Name = '4x400M Relay' AND Gender = 1 AND Environment = 2);
DECLARE @a1 INT = (SELECT Id FROM Athletes WHERE FirstName = 'Alexander' AND LastName = 'Baughman');
DECLARE @a2 INT = (SELECT Id FROM Athletes WHERE FirstName = 'Patrick' AND LastName = 'McConnell');
DECLARE @a3 INT = (SELECT Id FROM Athletes WHERE FirstName = 'Cole' AND LastName = 'Yagersz');
DECLARE @a4 INT = (SELECT Id FROM Athletes WHERE FirstName = 'Chase' AND LastName = 'Young');
IF @m IS NULL OR @e IS NULL OR @a1 IS NULL OR @a2 IS NULL OR @a3 IS NULL OR @a4 IS NULL THROW 50030, 'Meet, event or athlete not found', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = @m AND EventId = @e AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, @e, NULL, 248.95, 0, 0, 0, 0);
    DECLARE @p INT = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, @a1), (@p, @a2), (@p, @a3), (@p, @a4);
END
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
