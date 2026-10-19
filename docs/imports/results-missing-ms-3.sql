-- Cloverleaf results present on MileSplit but missing from meets the DB already has (BRAIN.md [C74]). Inserts only; never touches an existing row.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @p INT;
IF (SELECT COUNT(*) FROM Athletes WHERE (Id = 34 AND FirstName = 'Andrew' AND LastName = 'Hibler') OR (Id = 37 AND FirstName = 'Michael' AND LastName = 'Williams') OR (Id = 198 AND FirstName = 'Austin' AND LastName = 'Ebaugh') OR (Id = 317 AND FirstName = 'Cade' AND LastName = 'Cass') OR (Id = 359 AND FirstName = 'Brett' AND LastName = 'Gucker') OR (Id = 395 AND FirstName = 'Andrew' AND LastName = 'Vasel') OR (Id = 424 AND FirstName = 'Reese' AND LastName = 'Barnett') OR (Id = 456 AND FirstName = 'Brooklyn' AND LastName = 'Cavey') OR (Id = 505 AND FirstName = 'Frank' AND LastName = 'Stepp') OR (Id = 532 AND FirstName = 'Nick' AND LastName = 'Ziegler') OR (Id = 558 AND FirstName = 'Asa' AND LastName = 'Gynn') OR (Id = 572 AND FirstName = 'Blake' AND LastName = 'Wilson') OR (Id = 581 AND FirstName = 'Alyssa' AND LastName = 'Stoner') OR (Id = 631 AND FirstName = 'John' AND LastName = 'Manzuk') OR (Id = 658 AND FirstName = 'Julia' AND LastName = 'Armogida') OR (Id = 689 AND FirstName = 'Max' AND LastName = 'Gucker') OR (Id = 691 AND FirstName = 'Patrick' AND LastName = 'McConnell')) <> 17 THROW 50010, 'Athlete Ids do not match this database', 1;
IF (SELECT COUNT(*) FROM Meets WHERE (Name = '2019 Oberlin College High School Meet #3' AND Date = '2019-02-02') OR (Name = '2022 Division I District Track Meet' AND Date = '2022-05-20') OR (Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16')) <> 3 THROW 50011, 'Meet Ids do not match this database', 1;
IF (SELECT COUNT(*) FROM Events WHERE (Id = 99 AND Name = '4x100M Relay' AND Gender = 1) OR (Id = 108 AND Name = '4x400M Relay' AND Gender = 1) OR (Id = 114 AND Name = '4x200M Relay' AND Gender = 1) OR (Id = 115 AND Name = '4x400M Relay' AND Gender = 2) OR (Id = 116 AND Name = '4x400M Relay' AND Gender = 1)) <> 5 THROW 50012, 'Event Ids do not match this database', 1;
-- 2019 Ravenna-Streetsboro Double Dual: 4x100M Relay John Manzuk, Michael Williams, Austin Ebaugh, Blake Wilson 45.98
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16') AND EventId = 99 AND AthleteId IS NULL AND ABS(ISNULL(TimeSeconds, 0) + ISNULL(DistanceInches, 0) - 45.98) < 0.06)   -- a school can enter several relay teams in one event
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16'), 99, NULL, 45.98, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 631);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 37);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 198);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 572);
END
-- 2019 Ravenna-Streetsboro Double Dual: 4x200M Relay Austin Ebaugh, Frank Stepp, Cade Cass, Blake Wilson 97.7
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16') AND EventId = 114 AND AthleteId IS NULL AND ABS(ISNULL(TimeSeconds, 0) + ISNULL(DistanceInches, 0) - 97.7) < 0.06)   -- a school can enter several relay teams in one event
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16'), 114, NULL, 97.7, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 198);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 505);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 317);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 572);
END
-- 2019 Ravenna-Streetsboro Double Dual: 4x400M Relay Nick Ziegler, Andrew Hibler, Max Gucker, Asa Gynn 235.5
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16') AND EventId = 108 AND AthleteId IS NULL AND ABS(ISNULL(TimeSeconds, 0) + ISNULL(DistanceInches, 0) - 235.5) < 0.06)   -- a school can enter several relay teams in one event
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16'), 108, NULL, 235.5, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 532);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 34);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 689);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 558);
END
-- 2019 Oberlin College High School Meet #3: 4x400M Relay Max Gucker, Brett Gucker, Andrew Vasel, Patrick McConnell 280.94
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 Oberlin College High School Meet #3' AND Date = '2019-02-02') AND EventId = 116 AND AthleteId IS NULL AND ABS(ISNULL(TimeSeconds, 0) + ISNULL(DistanceInches, 0) - 280.94) < 0.06)   -- a school can enter several relay teams in one event
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2019 Oberlin College High School Meet #3' AND Date = '2019-02-02'), 116, NULL, 280.94, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 689);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 359);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 395);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 691);
END
-- 2022 Division I District Track Meet: 4x400M Relay Reese Barnett, Julia Armogida, Brooklyn Cavey, Alyssa Stoner 258.77
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2022 Division I District Track Meet' AND Date = '2022-05-20') AND EventId = 115 AND AthleteId IS NULL AND ABS(ISNULL(TimeSeconds, 0) + ISNULL(DistanceInches, 0) - 258.77) < 0.06)   -- a school can enter several relay teams in one event
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2022 Division I District Track Meet' AND Date = '2022-05-20'), 115, NULL, 258.77, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 424);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 658);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 456);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 581);
END
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
