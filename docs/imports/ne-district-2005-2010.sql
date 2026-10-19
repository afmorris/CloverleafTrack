-- Northeast Ohio Division I District (North Canton) 2005, 2006, 2008, 2009, 2010: meets missing from the DB (BRAIN.md [C71]).
-- Source: baumspage.com Hy-Tek results (scripts/crawl/baumspage/ne-YYYY-res.txt); Cloverleaf rows only. The meet date is the LAST day of the
-- meet, matching the 2003 / 2004 / 2007 / 2011 district rows. Each year is skipped if a meet with that name and date already exists.
-- Best mark per athlete-event is the Performance; prelim/semi/final times become PerformanceRounds (2+ rounds); athletes with no legal mark
-- (ND / NH / DQ / DNF) become ResultStatus rows. NT (no time) rows are ignored.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT, @p INT;

-- ===== 2005 Division I District Track Meet =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2005 Division I District Track Meet' AND Date = '2005-05-21')
BEGIN
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Amber' AND LastName = 'Hansen' AND Gender = 2) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Amber', 'Hansen', 2, 2007, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Morgan' AND LastName = 'O''Rourke' AND Gender = 2) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Morgan', 'O''Rourke', 2, 2007, 0);
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2005 Division I District Track Meet', '2005-05-21', 24, 1, 0, 9, 3, 1, 1, 'https://www.athletic.net/TrackAndField/meet/670398');
SET @m = SCOPE_IDENTITY();
-- Girls 4x800M Relay: Catie Coleman / Leah Geissman / Hillary Hewit / Heidi Santelmann  final 583.24
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 120, NULL, 583.24, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 352);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 554);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 304);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 450);
-- Boys 4x800M Relay: Sean Hutson / Dan Troike / Josh McElwaine / Jordan Bates  final 514.33
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 118, NULL, 514.33, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 275);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 845);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 405);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 734);
-- Girls 100M Hurdles: Jenny Rupp  prelim 17.22, semi 17.11
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (4, @m, 52, NULL, 17.11, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 17.22);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 17.11);
-- Boys 110M Hurdles: Cory Beebe  prelim 15.36, semi 15.33, final 15.18
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (212, @m, 55, NULL, 15.18, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 15.36);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 15.33);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 15.18);
-- Boys 110M Hurdles: Steve Drake  prelim 16.75, semi 16.88
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (237, @m, 55, NULL, 16.75, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 16.75);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 16.88);
-- Girls 100M: Morgan O'Rourke  prelim 13.65, semi 13.98
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Morgan' AND LastName = 'O''Rourke' AND Gender = 2), @m, 49, NULL, 13.65, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 13.65);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 13.98);
-- Girls 100M: Tess Goodrow  prelim 14.02, semi 14.35
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (390, @m, 49, NULL, 14.02, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 14.02);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 14.35);
-- Boys 100M: Darin Bohner  prelim 12.3
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (435, @m, 45, NULL, 12.3, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Neil Berentz  prelim 13.14
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (331, @m, 45, NULL, 13.14, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x200M Relay: Samantha Rogers / Caitlyn Kilkenny / Morgan O'Rourke / Tess Goodrow  prelim 114.18
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 111, NULL, 114.18, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 630);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 682);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, (SELECT Id FROM Athletes WHERE FirstName = 'Morgan' AND LastName = 'O''Rourke' AND Gender = 2));
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 390);
-- Boys 4x200M Relay: Adam Wanter / Taylor Harrington / Nate Miglich / Rick Morrison  prelim 99.66
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 114, NULL, 99.66, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 451);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 58);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 375);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 138);
-- Girls 1600M: Taylor Sachleben  final 365.64
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (786, @m, 41, NULL, 365.64, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Katrin Bates  final 379.02
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (271, @m, 41, NULL, 379.02, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Sean Hutson  prelim 281.47, final 267.76
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (275, @m, 56, NULL, 267.76, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 281.47);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 267.76);
-- Boys 1600M: Kyle Nehrenz  prelim 309.43
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (622, @m, 56, NULL, 309.43, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x100M Relay: Samantha Rogers / Morgan O'Rourke / Tess Goodrow / Amber Hansen  prelim 53.11
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 106, NULL, 53.11, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 630);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, (SELECT Id FROM Athletes WHERE FirstName = 'Morgan' AND LastName = 'O''Rourke' AND Gender = 2));
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 390);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, (SELECT Id FROM Athletes WHERE FirstName = 'Amber' AND LastName = 'Hansen' AND Gender = 2));
-- Boys 4x100M Relay: Darin Bohner / Dave Whelan / Steve Drake / Luke Anderson  prelim 46.16
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 99, NULL, 46.16, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 435);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 170);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 237);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 278);
-- Girls 400M: Heidi Santelmann  prelim 62.77
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (450, @m, 46, NULL, 62.77, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Leah Geissman  prelim 69.22
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (554, @m, 46, NULL, 69.22, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Taylor Riggs  prelim 57.11
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (71, @m, 37, NULL, 57.11, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Adam Wanter  prelim 57.77
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (451, @m, 37, NULL, 57.77, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Jenny Rupp  prelim 49.33, final 48.97
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (4, @m, 29, NULL, 48.97, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 49.33);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 48.97);
-- Boys 300M Hurdles: Cory Beebe  prelim 40.88, final 39.08
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (212, @m, 42, NULL, 39.08, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 40.88);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 39.08);
-- Boys 300M Hurdles: Steve Drake  prelim 42.6
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (237, @m, 42, NULL, 42.6, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Hillary Hewit  prelim 153.31, final 139.82
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (304, @m, 50, NULL, 139.82, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 153.31);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 139.82);
-- Girls 800M: Betsy Reedy  prelim 156.92000000000002, final 153.81
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (458, @m, 50, NULL, 153.81, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 156.92);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 153.81);
-- Girls 200M: Morgan O'Rourke  prelim 28.84
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Morgan' AND LastName = 'O''Rourke' AND Gender = 2), @m, 39, NULL, 28.84, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Tess Goodrow  prelim 29.43
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (390, @m, 39, NULL, 29.43, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Rick Morrison  prelim 24.88
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (138, @m, 65, NULL, 24.88, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Taylor Harrington  prelim 26.07
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (58, @m, 65, NULL, 26.07, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 3200M: Catie Coleman  final 688.22
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (352, @m, 35, NULL, 688.22, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Sean Hutson  final 610.99
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (275, @m, 62, NULL, 610.99, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x400M Relay: Hillary Hewit / Caitlyn Kilkenny / Jenny Rupp / Heidi Santelmann  prelim 254.24, final 257.73
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 115, NULL, 254.24, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 304);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 682);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 4);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 450);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 254.24);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 257.73);
-- Boys 4x400M Relay: Steve Drake / Luke Anderson / Dave Whelan / Cory Beebe  prelim 223.19
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 108, NULL, 223.19, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 237);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 278);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 170);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 212);
-- Girls Discus: Allison Early  final 1365.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (356, @m, 24, 1365.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Desserae Bailey  final 1296.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (246, @m, 24, 1296.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls High Jump: Caitlyn Kilkenny  final 60.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (682, @m, 11, 60.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Jenny Rupp  final 191.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (4, @m, 25, 191.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Pole Vault: Amanda Ewing  final 96.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (690, @m, 17, 96.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Luke Anderson  final 156.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (278, @m, 3, 156.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Josh Newcomer  final 144.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (229, @m, 3, 144.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2006 Division I District Track Meet =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2006 Division I District Track Meet' AND Date = '2006-05-19')
BEGIN
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Amber' AND LastName = 'Hansen' AND Gender = 2) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Amber', 'Hansen', 2, 2007, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Morgan' AND LastName = 'O''Rourke' AND Gender = 2) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Morgan', 'O''Rourke', 2, 2007, 0);
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2006 Division I District Track Meet', '2006-05-19', 24, 1, 0, 30, 3, 1, 1, 'https://www.athletic.net/TrackAndField/meet/670405');
SET @m = SCOPE_IDENTITY();
-- Girls 4x800M Relay: Heidi Santelmann / Francesca Luppino / Leah Geissman / Hillary Hewit  final 635.12
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 120, NULL, 635.12, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 450);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 773);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 554);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 304);
-- Boys 4x800M Relay: Jon Walkup / Greg Hill / Dan Troike / Jordan Bates  final 516.33
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 118, NULL, 516.33, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 678);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 798);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 845);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 734);
-- Girls 100M Hurdles: Marie Bruening  prelim 17.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (409, @m, 52, NULL, 17.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Jenny Rupp  prelim 16.4, semi 16.42, final 15.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (4, @m, 52, NULL, 15.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 16.4);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 16.42);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 15.9);
-- Boys 110M Hurdles: Steve Drake  prelim 16.4, semi 16.15
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (237, @m, 55, NULL, 16.15, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 16.4);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 16.15);
-- Boys 110M Hurdles: Mike Fuentes  prelim 17.1
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (694, @m, 55, NULL, 17.1, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M: Morgan O'Rourke  prelim 13.93
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Morgan' AND LastName = 'O''Rourke' AND Gender = 2), @m, 49, NULL, 13.93, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M: Amber Hansen  prelim 13.94
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Amber' AND LastName = 'Hansen' AND Gender = 2), @m, 49, NULL, 13.94, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Mitch McConahay  prelim 11.87
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (177, @m, 45, NULL, 11.87, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: James Rogers  prelim 12.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (9, @m, 45, NULL, 12.5, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x200M Relay: Morgan O'Rourke / Katlyn Henderson / Stacey Porter / Paige Lloyd  prelim 117.3
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 111, NULL, 117.3, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, (SELECT Id FROM Athletes WHERE FirstName = 'Morgan' AND LastName = 'O''Rourke' AND Gender = 2));
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 740);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 303);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 107);
-- Boys 4x200M Relay: Tyler Goins / Taylor Harrington / Kurt Abfall / Chris Pekar  prelim 100.81
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 114, NULL, 100.81, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 347);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 58);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 157);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 743);
-- Girls 1600M: Leah Geissman  final 335.43
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (554, @m, 41, NULL, 335.43, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Amanda Woodhall  final 351.43
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (735, @m, 41, NULL, 351.43, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Jon Walkup  final 269.82
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (678, @m, 56, NULL, 269.82, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Dan Troike  final 291.72
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (845, @m, 56, NULL, 291.72, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x100M Relay: Morgan O'Rourke / Katlyn Henderson / Amber Hansen / Caitlyn Kilkenny  prelim 52.76, final 52.88
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 106, NULL, 52.76, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, (SELECT Id FROM Athletes WHERE FirstName = 'Morgan' AND LastName = 'O''Rourke' AND Gender = 2));
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 740);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, (SELECT Id FROM Athletes WHERE FirstName = 'Amber' AND LastName = 'Hansen' AND Gender = 2));
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 682);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 52.76);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 52.88);
-- Boys 4x100M Relay: Tyler Goins / James Rogers / Mitch McConahay / Devon Neal  prelim 47.08
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 99, NULL, 47.08, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 347);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 9);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 177);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 153);
-- Girls 400M: Heidi Santelmann  prelim 63.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (450, @m, 46, NULL, 63.5, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Stacey Porter  prelim 72.4
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (303, @m, 46, NULL, 72.4, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Chris Pekar  prelim 58.34
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (743, @m, 37, NULL, 58.34, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Taylor Riggs  prelim 56.01
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (71, @m, 37, NULL, 56.01, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Jenny Rupp  prelim 48.74, final 47.47
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (4, @m, 29, NULL, 47.47, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 48.74);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 47.47);
-- Girls 300M Hurdles: Stephanie Foster  prelim 51.66
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (693, @m, 29, NULL, 51.66, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 300M Hurdles: Steve Drake  prelim 44.48
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (237, @m, 42, NULL, 44.48, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 300M Hurdles: Greg Hill  prelim 45.75
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (798, @m, 42, NULL, 45.75, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Betsy Reedy  prelim 155.1, final 155.88
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (458, @m, 50, NULL, 155.1, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 155.1);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 155.88);
-- Girls 800M: Hillary Hewit  prelim 153.59, final 140.35
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (304, @m, 50, NULL, 140.35, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 153.59);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 140.35);
-- Boys 800M: Jon Walkup  prelim 121.38, final 129.91
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (678, @m, 34, NULL, 121.38, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 121.38);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 129.91);
-- Girls 200M: Katlyn Henderson  prelim 31.56
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (740, @m, 39, NULL, 31.56, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Caitlyn Kilkenny  prelim 28.01, semi 28.1
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (682, @m, 39, NULL, 28.01, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 28.01);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 28.1);
-- Boys 200M: Mitch McConahay  prelim 24.54, semi 24.1
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (177, @m, 65, NULL, 24.1, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 24.54);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 24.1);
-- Boys 200M: Devon Neal  prelim 24.37, semi 24.81
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (153, @m, 65, NULL, 24.37, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 24.37);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 24.81);
-- Girls 3200M: Katrin Bates  final 790.85
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (271, @m, 35, NULL, 790.85, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 3200M: Laura Service  final 817.95
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (32, @m, 35, NULL, 817.95, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x400M Relay: Heidi Santelmann / Jenny Rupp / Stephanie Foster / Hillary Hewit  final 256.99
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 115, NULL, 256.99, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 450);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 4);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 693);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 304);
-- Girls Discus: Allison Early  final 1218.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (356, @m, 24, 1218.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Cassandra Maxwell  final 1350.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (726, @m, 24, 1350.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Pole Vault: Amber Hansen  final 102.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Amber' AND LastName = 'Hansen' AND Gender = 2), @m, 17, 102.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Pole Vault: Stephanie Foster  final 102.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (693, @m, 17, 102.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Josh Newcomer  final 150.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (229, @m, 3, 150.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Allison Early  final 443.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (356, @m, 2, 443.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2008 Division I District Track Meet =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2008 Division I District Track Meet' AND Date = '2008-05-23')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2008 Division I District Track Meet', '2008-05-23', 24, 1, 0, 3, 3, 1, 1, 'https://www.baumspage.com/tf/ne/2008/ncanton/res08.htm');
SET @m = SCOPE_IDENTITY();
-- Girls 4x800M Relay: Francesca Luppino / Amanda Woodhall / Janelle Shaffer / Leah Geissman  final 633.7
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 120, NULL, 633.7, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 773);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 735);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 61);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 554);
-- Boys 4x800M Relay: Bret Nehrenz / Joey Bruening / Dennis Berry / Andrew Bertram  final 554.95
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 118, NULL, 554.95, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 478);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 10);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 821);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 516);
-- Girls 100M Hurdles: Heidi Santelmann  prelim 16.71, semi 16.67, final 15.68
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (450, @m, 52, NULL, 15.68, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 16.71);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 16.67);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 15.68);
-- Girls 100M Hurdles: Megan Rybacki  prelim 17.57, semi 17.31
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (13, @m, 52, NULL, 17.31, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 17.57);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 17.31);
-- Boys 110M Hurdles: Keith Batesole  prelim 18.21, semi 18.56
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (545, @m, 55, NULL, 18.21, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 18.21);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 18.56);
-- Boys 110M Hurdles: Greg Hill  prelim 16.19, semi 15.92, final 15.68
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (798, @m, 55, NULL, 15.68, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 16.19);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 15.92);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 15.68);
-- Girls 100M: Olivia Arnold  prelim 14.02
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (344, @m, 49, NULL, 14.02, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M: Lauren McDermott  prelim 14.67
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (242, @m, 49, NULL, 14.67, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Jerry Chidsey  prelim 11.91
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (392, @m, 45, NULL, 11.91, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Kyle Sirl  prelim 12.3
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (704, @m, 45, NULL, 12.3, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x200M Relay: Olivia Arnold / Rachel Gokey / Megan McCarthy / Megan Rybacki  prelim 112.78, final 112.67
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 111, NULL, 112.67, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 344);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 495);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 219);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 13);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 112.78);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 112.67);
-- Boys 4x200M Relay: Garrett Livingood / Bobby Ellis / Ryan Weir / Brandon Boltz  prelim 103.58
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 114, NULL, 103.58, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 386);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 783);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 742);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 840);
-- Girls 1600M: Amanda Woodhall  final 343.96
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (735, @m, 41, NULL, 343.96, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Ashlee Burgess  final 373.47
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (493, @m, 41, NULL, 373.47, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Will Luppino  final 282.52
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (145, @m, 56, NULL, 282.52, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Andrew Bertram  final 297.15999999999997
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (516, @m, 56, NULL, 297.16, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x100M Relay: Olivia Arnold / Kelli Slavens / Mandy Heidelman / Taylor Webster  prelim 55.62
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 106, NULL, 55.62, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 344);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 447);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 837);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 400);
-- Boys 4x100M Relay: Jerry Chidsey / Zane Hemming / Richard Morrison / Adam Wanter  prelim 45.75
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 99, NULL, 45.75, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 392);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 287);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 138);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 451);
-- Girls 400M: Brittany Pekar  prelim 64.11, final 64.55
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (459, @m, 46, NULL, 64.11, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 64.11);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 64.55);
-- Girls 400M: Jenna Elswick  prelim 72.96000000000001
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (494, @m, 46, NULL, 72.96, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Taylor Riggs  prelim 52.21, final 51.92
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (71, @m, 37, NULL, 51.92, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 52.21);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 51.92);
-- Girls 300M Hurdles: Jennifer Foster  prelim 50.85
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (471, @m, 29, NULL, 50.85, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Heidi Santelmann  prelim 48.13, final 47.87
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (450, @m, 29, NULL, 47.87, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 48.13);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 47.87);
-- Boys 300M Hurdles: Greg Hill  prelim 45.11
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (798, @m, 42, NULL, 45.11, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 300M Hurdles: Keith Batesole  prelim 46.86
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (545, @m, 42, NULL, 46.86, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Francesca Luppino  prelim 151.03, final 152.63
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (773, @m, 50, NULL, 151.03, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 151.03);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 152.63);
-- Girls 800M: Leah Geissman  prelim 153.07, final 149.7
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (554, @m, 50, NULL, 149.7, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 153.07);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 149.7);
-- Boys 800M: Andrew Bertram  prelim 142.32999999999998
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (516, @m, 34, NULL, 142.33, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Joey Bruening  prelim 143.32999999999998
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (10, @m, 34, NULL, 143.33, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Megan McCarthy  prelim 29.25, semi 29.23
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (219, @m, 39, NULL, 29.23, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 29.25);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 29.23);
-- Girls 200M: Rachel Gokey  prelim 29.81
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (495, @m, 39, NULL, 29.81, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Jerry Chidsey  prelim 24.36, semi 24.36
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (392, @m, 65, NULL, 24.36, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 24.36);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 24.36);
-- Boys 200M: Adam Wanter  prelim 24.25, semi 25.32
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (451, @m, 65, NULL, 24.25, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 24.25);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 2, 25.32);
-- Girls 3200M: Ashlee Burgess  final 802.91
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (493, @m, 35, NULL, 802.91, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 3200M: Alisha Stahnke  final 804.43
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (92, @m, 35, NULL, 804.43, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Bret Nehrenz  final 662.34
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (478, @m, 62, NULL, 662.34, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Jeff Sharratt  final 699.27
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (301, @m, 62, NULL, 699.27, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x400M Relay: Heidi Santelmann / Francesca Luppino / Leah Geissman / Brittany Pekar  prelim 267.23
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 115, NULL, 267.23, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 450);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 773);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 554);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 459);
-- Boys 4x400M Relay: Adam Wanter / Richard Morrison / Will Luppino / Taylor Riggs  prelim 214.75, final 214.41
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 108, NULL, 214.41, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 451);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 138);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 145);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 71);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 214.75);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 214.41);
-- Girls Discus: Corrin Early  final 1038.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (638, @m, 24, 1038.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Kelsey Williams  final 1022.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (417, @m, 24, 1022.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: William Simpson  final 1361.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (637, @m, 13, 1361.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Kyle Juszczyk  ND
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (439, @m, 13, NULL, NULL, 0, 0, 0, 1);
SET @p = SCOPE_IDENTITY();
-- Girls High Jump: Jennifer Foster  final 58.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (471, @m, 11, 58.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Shannon Kilkenny  final 174.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (483, @m, 25, 174.25, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Janelle Shaffer  final 185.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (61, @m, 25, 185.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Long Jump: Adam Wanter  final 216.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (451, @m, 1, 216.25, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Long Jump: Richard Morrison  ND
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (138, @m, 1, NULL, NULL, 0, 0, 0, 1);
SET @p = SCOPE_IDENTITY();
-- Girls Pole Vault: Stephanie Foster  final 132.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (693, @m, 17, 132.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Pole Vault: Paige Lloyd  final 120.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (107, @m, 17, 120.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Chris Lamphier  final 132.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (369, @m, 3, 132.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Tony Boscia  NH
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (780, @m, 3, NULL, NULL, 0, 0, 0, 2);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Ellen Weeks  final 350.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (244, @m, 2, 350.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Kelsey Williams  final 356.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (417, @m, 2, 356.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Kyle Juszczyk  final 599.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (439, @m, 22, 599.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Kalvin Harrington  final 546.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (784, @m, 22, 546.25, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2009 Division I District Track Meet =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2009 Division I District Track Meet' AND Date = '2009-05-22')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2009 Division I District Track Meet', '2009-05-22', 24, 1, 0, 20, 3, 1, 1, 'https://www.athletic.net/TrackAndField/meet/84587');
SET @m = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Stephanie Foster  prelim 16.06, final 15.6
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (693, @m, 52, NULL, 15.6, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 16.06);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 15.6);
-- Girls 100M Hurdles: Megan Rybacki  prelim 16.51, final 15.96
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (13, @m, 52, NULL, 15.96, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 16.51);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 15.96);
-- Boys 110M Hurdles: Keith Batesole  prelim 17.16
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (545, @m, 55, NULL, 17.16, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 110M Hurdles: Jake Sideris  prelim 17.83
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (333, @m, 55, NULL, 17.83, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M: Taylor Detwiler  prelim 13.52
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (404, @m, 49, NULL, 13.52, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M: Olivia Arnold  prelim 14.21
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (344, @m, 49, NULL, 14.21, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Jerry Chidsey  prelim 12.3
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (392, @m, 45, NULL, 12.3, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x800M Relay: Lauren McGraw / Katlyn Donnelly / Aly Rice / Mackenna Huff  final 673.82
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 120, NULL, 673.82, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 165);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 27);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 660);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 797);
-- Boys 4x800M Relay: Chris Matejka / Will Luppino / Ryan Weir / Bret Nehrenz  final 525.16
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 118, NULL, 525.16, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 843);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 145);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 742);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 478);
-- Boys 4x200M Relay: Zane Hemming / Bobby Ellis / Joe Hemperly / Garrett Livengood  prelim 98.84
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 114, NULL, 98.84, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 287);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 783);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 373);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 386);
-- Girls 1600M: Katlyn Donnelly  final 375.92
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (27, @m, 41, NULL, 375.92, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Mackenna Huff  final 384.88
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (797, @m, 41, NULL, 384.88, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Will Luppino  final 284.65999999999997
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (145, @m, 56, NULL, 284.66, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Joe Bruening  final 299.88
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (10, @m, 56, NULL, 299.88, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x100M Relay: Olivia Arnold / Jenna Elswick / Taylor Webster / Taylor Detwiler  prelim 52.36, final 52.67
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 106, NULL, 52.36, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 344);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 494);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 400);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 404);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 52.36);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 52.67);
-- Boys 4x100M Relay: Zach Miglich / Zane Hemming / Brandon Boltz / Cody Roberts  prelim 45.88
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 99, NULL, 45.88, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 796);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 287);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 840);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 127);
-- Girls 400M: Brittani Pekar  prelim 63.32, final 64.01
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (459, @m, 46, NULL, 63.32, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 63.32);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 64.01);
-- Girls 400M: Taylor Webster  prelim 67.94
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (400, @m, 46, NULL, 67.94, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Matt Illig  prelim 54.75
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (36, @m, 37, NULL, 54.75, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Joe Hemperly  prelim 54.85
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (373, @m, 37, NULL, 54.85, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Kelani Nuckols  prelim 48.15, final 48.18
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (66, @m, 29, NULL, 48.15, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 48.15);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 48.18);
-- Girls 300M Hurdles: Jennifer Foster  prelim 48.27, final 48.21
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (471, @m, 29, NULL, 48.21, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 48.27);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 48.21);
-- Boys 300M Hurdles: Keith Batesole  prelim 43.91
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (545, @m, 42, NULL, 43.91, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 300M Hurdles: Clay Vondriska  prelim 46.98
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (800, @m, 42, NULL, 46.98, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Candace Andel  prelim 148.53, final 152.51
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (650, @m, 50, NULL, 148.53, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 148.53);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 152.51);
-- Girls 800M: Lauren McGraw  prelim 164.64
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (165, @m, 50, NULL, 164.64, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Chris Matejka  prelim 125.55, final 120.98
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (843, @m, 34, NULL, 120.98, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 125.55);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 120.98);
-- Boys 800M: Will Luppino  prelim 126.17, final 135.98
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (145, @m, 34, NULL, 126.17, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 126.17);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 135.98);
-- Girls 200M: Taylor Detwiler  prelim 28.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (404, @m, 39, NULL, 28.25, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Janelle Shaffer  prelim 29.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (61, @m, 39, NULL, 29.25, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Brandon Boltz  prelim 25.37
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (840, @m, 65, NULL, 25.37, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Zane Hemming  prelim 25.56
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (287, @m, 65, NULL, 25.56, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Bret Nehrenz  final 619.65
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (478, @m, 62, NULL, 619.65, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: R.J. Bechtel  final 682.92
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (440, @m, 62, NULL, 682.92, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x400M Relay: Candace Andel / Megan Rybacki / Kelani Nuckols / Brittani Pekar  prelim 259.44, final 259.31
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 115, NULL, 259.31, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 650);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 13);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 66);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 459);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 259.44);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 259.31);
-- Boys 4x400M Relay: Ryan Weir / Joe Hemperly / Keith Batesole / Matt Illig  prelim 226.67000000000002
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 108, NULL, 226.67, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 742);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 373);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 545);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 36);
-- Boys Discus: Kyle Juszczyk  final 1781.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (439, @m, 13, 1781.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Cody Furr  final 1756.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (334, @m, 13, 1756.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls High Jump: Jennifer Foster  final 62.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (471, @m, 11, 62.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Kelani Nuckols  final 206.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (66, @m, 25, 206.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Long Jump: Cody Roberts  final 238.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (127, @m, 1, 238.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Long Jump: Zach Miglich  final 235.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (796, @m, 1, 235.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Pole Vault: Stephanie Foster  final 138.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (693, @m, 17, 138.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Pole Vault: Jennifer Foster  final 126.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (471, @m, 17, 126.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Chris Lamphier  final 144.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (369, @m, 3, 144.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Vinnie Kacinari  NH
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (316, @m, 3, NULL, NULL, 0, 0, 0, 2);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Kyle Juszczyk  final 636.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (439, @m, 22, 636.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2010 Division I District Track Meet =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2010 Division I District Track Meet' AND Date = '2010-05-21')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2010 Division I District Track Meet', '2010-05-21', 24, 1, 0, 13, 3, 1, 1, 'https://www.athletic.net/TrackAndField/meet/112489');
SET @m = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Jennifer Foster  prelim 15.74, final 15.95
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (471, @m, 52, NULL, 15.74, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 15.74);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 15.95);
-- Girls 100M Hurdles: Megan Rybacki  prelim 16.02, final 16.1
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (13, @m, 52, NULL, 16.02, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 16.02);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 16.1);
-- Boys 110M Hurdles: Derek Daubenmire  prelim 16.49
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (162, @m, 55, NULL, 16.49, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 110M Hurdles: Jake Sideris  prelim 17.46
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (333, @m, 55, NULL, 17.46, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M: Janelle Shaffer  prelim 13.18, final 13.56
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (61, @m, 49, NULL, 13.18, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 13.18);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 13.56);
-- Boys 100M: Brandon Boltz  prelim 12.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (840, @m, 45, NULL, 12.0, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Garrett Livingood  prelim 12.05
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (386, @m, 45, NULL, 12.05, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x200M Relay: Kelani Nuckols / Jenna Elswick / Janelle Shaffer / Taylor Detwiler  prelim 107.71000000000001, final 107.74000000000001
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 111, NULL, 107.71, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 66);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 494);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 61);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 404);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 107.71);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 107.74);
-- Boys 4x200M Relay: Brandon Boltz / Bobby Ellis / Garrett Livingood / Ryan Weir  prelim 96.4
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 114, NULL, 96.4, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 840);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 783);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 386);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 742);
-- Girls 1600M: Ariel Talabac  final 369.7
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (261, @m, 41, NULL, 369.7, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Amanda Kalain  final 384.52
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (489, @m, 41, NULL, 384.52, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Will Luppino  final 287.81
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (145, @m, 56, NULL, 287.81, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x100M Relay: Jenna Elswick / Katy Dean / Morgan Siegler / Taylor Detwiler  prelim 53.0, final 52.61
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 106, NULL, 52.61, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 494);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 226);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 142);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 404);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 53.0);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 52.61);
-- Boys 4x100M Relay: Garrett Livingood / Joe Hemperly / Bobby Ellis / Brandon Boltz  prelim 45.86
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 99, NULL, 45.86, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 386);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 373);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 783);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 840);
-- Boys 400M: Bobby Ellis  prelim 55.79
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (783, @m, 37, NULL, 55.79, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Ryan Weir  prelim 53.12
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (742, @m, 37, NULL, 53.12, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Kelani Nuckols  prelim 48.64, final 48.92
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (66, @m, 29, NULL, 48.64, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 48.64);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 48.92);
-- Girls 300M Hurdles: Megan Rybacki  prelim 48.48, final 47.32
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (13, @m, 29, NULL, 47.32, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 48.48);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 47.32);
-- Boys 300M Hurdles: Derek Daubenmire  prelim 43.53
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (162, @m, 42, NULL, 43.53, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 300M Hurdles: Clay Vondriska  prelim 44.38
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (800, @m, 42, NULL, 44.38, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Lauren McGraw  prelim 162.36
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (165, @m, 50, NULL, 162.36, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Mckenna Huff  prelim 163.68
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (797, @m, 50, NULL, 163.68, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Travis Connor  prelim 130.85
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (600, @m, 34, NULL, 130.85, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Paul Hissong  prelim 136.81
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (346, @m, 34, NULL, 136.81, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Katy Dean  prelim 29.03
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (226, @m, 39, NULL, 29.03, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Taylor Detwiler  prelim 27.51
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (404, @m, 39, NULL, 27.51, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Brandon Boltz  prelim 24.66
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (840, @m, 65, NULL, 24.66, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Joe Hemperly  prelim 23.47
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (373, @m, 65, NULL, 23.47, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: RJ Becthel  final 649.03
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (440, @m, 62, NULL, 649.03, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Joey Bruening  final 672.17
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (10, @m, 62, NULL, 672.17, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x400M Relay: Kelani Nuckols / Jenna Elswick / Morgan Eyring / Janelle Shaffer  prelim 261.12
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 115, NULL, 261.12, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 66);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 494);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 527);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 61);
-- Boys 4x400M Relay: Ryan Weir / Bobby Ellis / Joe Hemperly / Will Luppino  prelim 217.67000000000002
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 108, NULL, 217.67, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 742);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 783);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 373);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 145);
-- Girls Discus: Corrin Early  final 1450.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (638, @m, 24, 1450.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Cody Furr  final 1770.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (334, @m, 13, 1770.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls High Jump: Jennifer Foster  final 64.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (471, @m, 11, 64.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Kelani Nuckols  final 192.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (66, @m, 25, 192.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Pole Vault: Jennifer Foster  final 144.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (471, @m, 17, 144.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Brian Livingstone  final 578.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (419, @m, 22, 578.25, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
