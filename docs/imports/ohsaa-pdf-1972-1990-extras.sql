-- Follow-up to ohsaa-pdf-1960-1991-fixes.sql: 180M Low Hurdles event (yards stored as meters), Chrislip 1972 5th at 23.0,
-- new athlete Favna Bender (class year unknown: 1991 is a guess) and the 1990 State girls 4x800 relay 9:31.10 (6th).
-- Id-independent; re-runnable.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM Events WHERE EventKey = 'BOYS_OUTDOOR_180M_LOW_HURDLES')
    INSERT INTO Events (Name, EventKey, EventType, Gender, Environment, AthleteCount, SortOrder, EventCategory, EventCategorySortOrder)
    VALUES (N'180M Low Hurdles', 'BOYS_OUTDOOR_180M_LOW_HURDLES', 1, 1, 1, 1, 105, 5, 55);
DECLARE @a INT = (SELECT Id FROM Athletes WHERE FirstName=N'Scott' AND LastName=N'Chrislip' AND Gender=1);
DECLARE @m INT = (SELECT Id FROM Meets WHERE Name=N'1972 OHSAA State Track and Field Championships');
DECLARE @e INT = (SELECT Id FROM Events WHERE EventKey='BOYS_OUTDOOR_180M_LOW_HURDLES');
IF @a IS NULL OR @m IS NULL OR @e IS NULL THROW 50011, 'Chrislip/meet/event missing', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE AthleteId=@a AND MeetId=@m AND EventId=@e)
    INSERT INTO Performances (AthleteId, MeetId, EventId, TimeSeconds, DistanceInches, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (@a,@m,@e,23.0,NULL,0,0,0,0);

IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName=N'Favna' AND LastName=N'Bender')
    INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES (N'Favna', N'Bender', 2, 1991, 0);
DECLARE @m2 INT = (SELECT Id FROM Meets WHERE Name=N'1990 OHSAA Division I State Track and Field Championships');
DECLARE @e2 INT = (SELECT Id FROM Events WHERE Name=N'4x800M Relay' AND Gender=2 AND Environment=1);
IF @m2 IS NULL OR @e2 IS NULL THROW 50012, 'meet/event missing', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE AthleteId IS NULL AND MeetId=@m2 AND EventId=@e2 AND ABS(TimeSeconds-571.10)<0.006)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, TimeSeconds, DistanceInches, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL,@m2,@e2,571.10,NULL,0,0,0,0);
    DECLARE @p INT = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId)
    SELECT @p, Id FROM Athletes WHERE Gender=2 AND ((FirstName=N'Kim' AND LastName=N'Malarkey') OR (FirstName=N'Cathy' AND LastName=N'Ocheltree') OR (FirstName=N'Favna' AND LastName=N'Bender') OR (FirstName=N'Tracey' AND LastName=N'Malarkey'));
    IF (SELECT COUNT(*) FROM PerformanceAthletes WHERE PerformanceId=@p) <> 4 THROW 50013, 'relay needs exactly 4 athletes', 1;
END
COMMIT TRANSACTION;
EXEC sp_RebuildLeaderboards;
