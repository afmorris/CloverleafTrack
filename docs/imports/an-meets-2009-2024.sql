-- Single-day meets from baumspage Hy-Tek results (BRAIN.md [C72]); Cloverleaf rows only. Each meet is skipped if a meet with that name and date exists.
-- Rules as in ne-district-2005-2010.sql. Athlete Ids are asserted before anything is inserted.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT, @p INT;
IF (SELECT COUNT(*) FROM Athletes WHERE (Id = 12 AND FirstName = 'Tommie' AND LastName = 'Kurtz') OR (Id = 59 AND FirstName = 'Lauren' AND LastName = 'Frank') OR (Id = 68 AND FirstName = 'Kirsten' AND LastName = 'Wilson') OR (Id = 87 AND FirstName = 'Jenna' AND LastName = 'Oliver') OR (Id = 117 AND FirstName = 'Zoe' AND LastName = 'Riddile') OR (Id = 120 AND FirstName = 'Sara' AND LastName = 'Keener') OR (Id = 125 AND FirstName = 'Vanessa' AND LastName = 'Rivera') OR (Id = 147 AND FirstName = 'Alayna' AND LastName = 'Richmond') OR (Id = 207 AND FirstName = 'Bailey' AND LastName = 'Freeland') OR (Id = 252 AND FirstName = 'Zana' AND LastName = 'Ross') OR (Id = 307 AND FirstName = 'Vickie' AND LastName = 'Andrews') OR (Id = 383 AND FirstName = 'Isaac' AND LastName = 'Baltic') OR (Id = 419 AND FirstName = 'Brian' AND LastName = 'Livingstone') OR (Id = 439 AND FirstName = 'Kyle' AND LastName = 'Juszczyk') OR (Id = 590 AND FirstName = 'Katelyn' AND LastName = 'Young') OR (Id = 595 AND FirstName = 'Abby' AND LastName = 'Andrews') OR (Id = 607 AND FirstName = 'Riley' AND LastName = 'Freeland') OR (Id = 616 AND FirstName = 'Anna' AND LastName = 'Winnicki') OR (Id = 665 AND FirstName = 'Taylor' AND LastName = 'Napholz') OR (Id = 668 AND FirstName = 'Maddie' AND LastName = 'Boltz') OR (Id = 688 AND FirstName = 'Lisa' AND LastName = 'Wangler') OR (Id = 755 AND FirstName = 'Erian' AND LastName = 'Hamilton') OR (Id = 761 AND FirstName = 'Austin' AND LastName = 'Roberts')) <> 23 THROW 50010, 'Athlete Ids do not match this database', 1;

-- ===== 2009 Mansfield Mehock Relays =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2009 Mansfield Mehock Relays' AND Date = '2009-04-17')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2009 Mansfield Mehock Relays', '2009-04-17', 32, 1, 0, 20, 3, 1, 3, 'https://www.athletic.net/TrackAndField/meet/444526');
SET @m = SCOPE_IDENTITY();
-- Boys Shot Put: Kyle Juszczyk  final 621.75
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (439, @m, 22, 621.75, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2012 Optimist Meet =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2012 Optimist Meet' AND Date = '2012-05-05')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2012 Optimist Meet', '2012-05-05', 4, 1, 0, 17, 3, 1, 3, 'https://www.athletic.net/TrackAndField/meet/157758');
SET @m = SCOPE_IDENTITY();
-- Boys Shot Put: Brian Livingstone  final 584.75
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (419, @m, 22, 584.75, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Austin Roberts  final 552.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (761, @m, 22, 552.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Brian Livingstone  final 1604.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (419, @m, 13, 1604.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Austin Roberts  final 1468.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (761, @m, 13, 1468.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2016 Optimist Meet =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2016 Optimist Meet' AND Date = '2016-05-07')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2016 Optimist Meet', '2016-05-07', 4, 1, 0, 5, 3, 1, 3, 'https://www.athletic.net/TrackAndField/meet/275382');
SET @m = SCOPE_IDENTITY();
-- Girls 100M: Lauren Frank  prelim 13.62
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (59, @m, 49, NULL, 13.62, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M: Kirsten Wilson  prelim 13.69
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (68, @m, 49, NULL, 13.69, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Lauren Frank  prelim 27.72
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (59, @m, 39, NULL, 27.72, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Sara Keener  prelim 28.01
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (120, @m, 39, NULL, 28.01, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Erian Hamilton  prelim 63.2
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (755, @m, 46, NULL, 63.2, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Anna Winnicki  prelim 65.73
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (616, @m, 46, NULL, 65.73, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Jenna Oliver  final 149.43
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (87, @m, 50, NULL, 149.43, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Vanessa Rivera  final 151.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (125, @m, 50, NULL, 151.25, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Riley Freeland  final 333.01
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (607, @m, 41, NULL, 333.01, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Tommie Kurtz  final 343.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (12, @m, 41, NULL, 343.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 3200M: Vanessa Rivera  final 702.72
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (125, @m, 35, NULL, 702.72, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 3200M: Bailey Freeland  final 762.75
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (207, @m, 35, NULL, 762.75, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Zana Ross  prelim 16.41
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (252, @m, 52, NULL, 16.41, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Alayna Richmond  prelim 19.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (147, @m, 52, NULL, 19.25, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Zana Ross  prelim 48.84
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (252, @m, 29, NULL, 48.84, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Alayna Richmond  prelim 54.67
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (147, @m, 29, NULL, 54.67, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x100M Relay: Zoe Riddile / Sara Keener / Taylor Napholz / Kirsten Wilson  final 53.21
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 106, NULL, 53.21, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 117);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 120);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 665);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 68);
-- Girls 4x200M Relay: Zoe Riddile / Sara Keener / Zana Ross / Lauren Frank  final 115.06
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 111, NULL, 115.06, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 117);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 120);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 252);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 59);
-- Girls 4x400M Relay: Erian Hamilton / Lauren Frank / Tommie Kurtz / Riley Freeland  final 258.56
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 115, NULL, 258.56, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 755);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 59);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 607);
-- Girls 4x800M Relay: Jenna Oliver / Tommie Kurtz / Erian Hamilton / Riley Freeland  final 592.52
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 120, NULL, 592.52, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 87);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 755);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 607);
-- Girls Shot Put: Katelyn Young  final 438.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (590, @m, 2, 438.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Lisa Wangler  final 393.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (688, @m, 2, 393.25, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Katelyn Young  final 1470.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (590, @m, 24, 1470.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Lisa Wangler  final 1142.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (688, @m, 24, 1142.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls High Jump: Vickie Andrews  final 60.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (307, @m, 11, 60.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Abby Andrews  final 171.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (595, @m, 25, 171.25, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Maddie Boltz  final 156.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (668, @m, 25, 156.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2024 Optimist Meet =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2024 Optimist Meet' AND Date = '2024-05-04')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2024 Optimist Meet', '2024-05-04', 4, 1, 0, 4, 3, 1, 3, 'https://oh.milesplit.com/meets/582440');
SET @m = SCOPE_IDENTITY();
-- Boys Pole Vault: Isaac Baltic  final 168.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (383, @m, 3, 168.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
