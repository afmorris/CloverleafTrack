-- 1986 OHSAA Class AAA State: Lodi Cloverleaf girls 4x800 relay, 6th, 9:32.73 (Sue Wagner, Beth Kaniecki, Laura Foradori, Linda Melendez).
-- New meet (date is a June 1 placeholder; the PDF prints none) and three new athletes (class years unknown: 1987 is a guess). Id-independent; re-runnable.
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @loc INT = (SELECT Id FROM Locations WHERE Name = N'Jesse Owens Track - Ohio Stadium');
DECLARE @season INT = (SELECT Id FROM Seasons WHERE Name = N'1985-1986');
IF @loc IS NULL OR @season IS NULL THROW 50020, 'location/season missing', 1;
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = N'1986 OHSAA Class AAA State Championships')
    INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType) VALUES (N'1986 OHSAA Class AAA State Championships', '1986-06-01', @loc, 1, 0, @season, 3, 1, 1);
INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive)
SELECT v.f, v.l, 2, 1987, 0 FROM (VALUES (N'Sue', N'Wagner'), (N'Beth', N'Kaniecki'), (N'Linda', N'Melendez')) v(f, l)
WHERE NOT EXISTS (SELECT 1 FROM Athletes a WHERE a.FirstName = v.f AND a.LastName = v.l AND a.Gender = 2);
DECLARE @m INT = (SELECT Id FROM Meets WHERE Name = N'1986 OHSAA Class AAA State Championships');
DECLARE @e INT = (SELECT Id FROM Events WHERE Name = N'4x800M Relay' AND Gender = 2 AND Environment = 1);
IF NOT EXISTS (SELECT 1 FROM Performances WHERE AthleteId IS NULL AND MeetId = @m AND EventId = @e AND ABS(TimeSeconds - 572.73) < 0.006)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, TimeSeconds, DistanceInches, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, @e, 572.73, NULL, 0, 0, 0, 0);
    DECLARE @p INT = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId)
    SELECT @p, MIN(Id) FROM Athletes WHERE Gender = 2 AND ((FirstName = N'Sue' AND LastName = N'Wagner') OR (FirstName = N'Beth' AND LastName = N'Kaniecki') OR (FirstName = N'Laura' AND LastName = N'Foradori') OR (FirstName = N'Linda' AND LastName = N'Melendez'))
    GROUP BY FirstName, LastName;
    IF (SELECT COUNT(*) FROM PerformanceAthletes WHERE PerformanceId = @p) <> 4 THROW 50021, 'relay needs exactly 4 athletes', 1;
    INSERT INTO MeetPlacings (MeetId, PerformanceId, MeetParticipantId, Place, FullPoints, SplitPoints) VALUES (@m, @p, NULL, 6, 0, 0);
END
COMMIT TRANSACTION;
EXEC sp_RebuildLeaderboards;
