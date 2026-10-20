-- OHSAA state-meet PDFs (1960-1991): results the DB was missing. Id-independent (lookups by name); safe to re-run (NOT EXISTS guards).
-- Placeholder meet dates (June 1) where the PDF prints none. Yards are stored as meters, as for the existing 1962 results.
-- Skipped on purpose: Chrislip 1972 180-yd low hurdles 23.0 (no such event), 1990 girls 4x800 (Favna Bender not in DB).
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @loc INT = (SELECT Id FROM Locations WHERE Name = N'Jesse Owens Track - Ohio Stadium');

-- seasons
INSERT INTO Seasons (Name, StartDate, EndDate, IsCurrentSeason, Status, ScoringEnabled)
SELECT v.n, v.s, v.e, 0, 4, 0 FROM (VALUES (N'1959-1960','1959-12-01','1960-06-30'),(N'1971-1972','1971-12-01','1972-06-30'),(N'1972-1973','1972-12-01','1973-06-30')) v(n,s,e)
WHERE NOT EXISTS (SELECT 1 FROM Seasons WHERE Name = v.n);

-- meets (Environment 1 outdoor, MeetType 1, MeetLevel 1, EntryStatus 3)
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType)
SELECT v.n, v.d, @loc, 1, 0, (SELECT Id FROM Seasons WHERE Name = v.s), 3, 1, 1
FROM (VALUES (N'1960 Class A State Championships','1960-06-01',N'1959-1960'),
             (N'1972 OHSAA State Track and Field Championships','1972-06-01',N'1971-1972'),
             (N'1973 OHSAA State Track and Field Championships','1973-06-01',N'1972-1973'),
             (N'1982 OHSAA Division I State Track and Field Championships','1982-06-01',N'1981-1982')) v(n,d,s)
WHERE NOT EXISTS (SELECT 1 FROM Meets WHERE Name = v.n);

-- athlete
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = N'Scott' AND LastName = N'Chrislip')
    INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES (N'Scott', N'Chrislip', 1, 1973, 0);

-- results: (athlete, gender, meet, event, time, inches)
DECLARE @r TABLE (fn NVARCHAR(50), ln NVARCHAR(50), g INT, meet NVARCHAR(200), ev NVARCHAR(100), t FLOAT, d FLOAT);
INSERT INTO @r VALUES
 (N'Charles', N'Massie', 1, N'1994 OHSAA Division I State Track and Field Championships', N'800M', 114.60, NULL),
 (N'Scott', N'Murphy', 1, N'1982 OHSAA Division I State Track and Field Championships', N'800M', 115.62, NULL),
 (N'Scott', N'Chrislip', 1, N'1972 OHSAA State Track and Field Championships', N'High Jump', NULL, 76),
 (N'Scott', N'Chrislip', 1, N'1973 OHSAA State Track and Field Championships', N'High Jump', NULL, 74),
 (N'Bill', N'Heffelfinger', 1, N'1960 Class A State Championships', N'200M', 22.4, NULL),
 (N'Kim', N'Malarkey', 2, N'1990 OHSAA Division I State Track and Field Championships', N'1600M', 318.10, NULL),
 (N'Kim', N'Malarkey', 2, N'1991 OHSAA Division I State Track and Field Championships', N'1600M', 304.92, NULL);

DECLARE @fn NVARCHAR(50), @ln NVARCHAR(50), @g INT, @meet NVARCHAR(200), @ev NVARCHAR(100), @t FLOAT, @d FLOAT, @a INT, @m INT, @e INT;
DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT fn,ln,g,meet,ev,t,d FROM @r;
OPEN c; FETCH NEXT FROM c INTO @fn,@ln,@g,@meet,@ev,@t,@d;
WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @a = (SELECT TOP 1 Id FROM Athletes WHERE FirstName=@fn AND LastName=@ln AND Gender=@g);
    SELECT @m = (SELECT TOP 1 Id FROM Meets WHERE Name=@meet);
    SELECT @e = (SELECT TOP 1 Id FROM Events WHERE Name=@ev AND Gender=@g AND Environment=1);
    IF @a IS NULL OR @m IS NULL OR @e IS NULL THROW 50010, 'athlete/meet/event not found', 1;
    IF NOT EXISTS (SELECT 1 FROM Performances WHERE AthleteId=@a AND MeetId=@m AND EventId=@e)
        INSERT INTO Performances (AthleteId, MeetId, EventId, TimeSeconds, DistanceInches, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
        VALUES (@a, @m, @e, @t, @d, 0, 0, 0, 0);
    FETCH NEXT FROM c INTO @fn,@ln,@g,@meet,@ev,@t,@d;
END
CLOSE c; DEALLOCATE c;
COMMIT TRANSACTION;
EXEC sp_RebuildLeaderboards;
