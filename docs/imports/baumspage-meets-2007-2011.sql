-- Single-day meets from baumspage Hy-Tek results (BRAIN.md [C72]); Cloverleaf rows only. Each meet is skipped if a meet with that name and date exists.
-- Rules as in ne-district-2005-2010.sql. Athlete Ids are asserted before anything is inserted.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT, @p INT;
IF (SELECT COUNT(*) FROM Athletes WHERE (Id = 4 AND FirstName = 'Jenny' AND LastName = 'Rupp') OR (Id = 10 AND FirstName = 'Joey' AND LastName = 'Bruening') OR (Id = 13 AND FirstName = 'Megan' AND LastName = 'Rybacki') OR (Id = 27 AND FirstName = 'Katlyn' AND LastName = 'Donnelly') OR (Id = 61 AND FirstName = 'Janelle' AND LastName = 'Shaffer') OR (Id = 71 AND FirstName = 'Taylor' AND LastName = 'Riggs') OR (Id = 92 AND FirstName = 'Alisha' AND LastName = 'Stahnke') OR (Id = 107 AND FirstName = 'Paige' AND LastName = 'Lloyd') OR (Id = 127 AND FirstName = 'Cody' AND LastName = 'Roberts') OR (Id = 138 AND FirstName = 'Richard' AND LastName = 'Morrison') OR (Id = 145 AND FirstName = 'Will' AND LastName = 'Luppino') OR (Id = 146 AND FirstName = 'Jimmy' AND LastName = 'Leahy') OR (Id = 229 AND FirstName = 'Josh' AND LastName = 'Newcomer') OR (Id = 244 AND FirstName = 'Ellen' AND LastName = 'Weeks') OR (Id = 265 AND FirstName = 'Steve' AND LastName = 'Dembowski') OR (Id = 269 AND FirstName = 'Jake' AND LastName = 'Lile') OR (Id = 287 AND FirstName = 'Zane' AND LastName = 'Hemming') OR (Id = 301 AND FirstName = 'Jeff' AND LastName = 'Sharratt') OR (Id = 316 AND FirstName = 'Vincent' AND LastName = 'Kacinari') OR (Id = 334 AND FirstName = 'Cody' AND LastName = 'Furr') OR (Id = 344 AND FirstName = 'Olivia' AND LastName = 'Arnold') OR (Id = 367 AND FirstName = 'Barbara' AND LastName = 'Mooneyham') OR (Id = 369 AND FirstName = 'Chris' AND LastName = 'Lamphier') OR (Id = 370 AND FirstName = 'Dylan' AND LastName = 'Varga') OR (Id = 392 AND FirstName = 'Jerry' AND LastName = 'Chidsey') OR (Id = 417 AND FirstName = 'Kelsey' AND LastName = 'Williams') OR (Id = 419 AND FirstName = 'Brian' AND LastName = 'Livingstone') OR (Id = 432 AND FirstName = 'Lee' AND LastName = 'Heckler') OR (Id = 447 AND FirstName = 'Kelli' AND LastName = 'Slavens') OR (Id = 450 AND FirstName = 'Heidi' AND LastName = 'Santelmann') OR (Id = 459 AND FirstName = 'Brittani' AND LastName = 'Pekar') OR (Id = 471 AND FirstName = 'Jennifer' AND LastName = 'Foster') OR (Id = 478 AND FirstName = 'Bret' AND LastName = 'Nehrenz') OR (Id = 483 AND FirstName = 'Shannon' AND LastName = 'Kilkenny') OR (Id = 493 AND FirstName = 'Ashlee' AND LastName = 'Burgess') OR (Id = 494 AND FirstName = 'Jenna' AND LastName = 'Elswick') OR (Id = 495 AND FirstName = 'Rachel' AND LastName = 'Gokey') OR (Id = 500 AND FirstName = 'Shawn' AND LastName = 'Tamba') OR (Id = 516 AND FirstName = 'Andrew' AND LastName = 'Bertram') OR (Id = 545 AND FirstName = 'Keith' AND LastName = 'Batesole') OR (Id = 554 AND FirstName = 'Leah' AND LastName = 'Geissman') OR (Id = 622 AND FirstName = 'Kyle' AND LastName = 'Nehrenz') OR (Id = 637 AND FirstName = 'Billy' AND LastName = 'Simpson') OR (Id = 638 AND FirstName = 'Corrin' AND LastName = 'Early') OR (Id = 682 AND FirstName = 'Caitlyn' AND LastName = 'Kilkenny') OR (Id = 693 AND FirstName = 'Stephanie' AND LastName = 'Foster') OR (Id = 694 AND FirstName = 'Mike' AND LastName = 'Fuentes') OR (Id = 704 AND FirstName = 'Kyle' AND LastName = 'Sirl') OR (Id = 726 AND FirstName = 'Cassandra' AND LastName = 'Maxwell') OR (Id = 735 AND FirstName = 'Amanda' AND LastName = 'Woodhall') OR (Id = 773 AND FirstName = 'Francesca' AND LastName = 'Luppino') OR (Id = 784 AND FirstName = 'Kalvin' AND LastName = 'Harrington') OR (Id = 786 AND FirstName = 'Taylor' AND LastName = 'Sachleben') OR (Id = 798 AND FirstName = 'Greg' AND LastName = 'Hill') OR (Id = 821 AND FirstName = 'Dennis' AND LastName = 'Berry') OR (Id = 837 AND FirstName = 'Mandy' AND LastName = 'Heidelman') OR (Id = 845 AND FirstName = 'Dan' AND LastName = 'Troike')) <> 57 THROW 50010, 'Athlete Ids do not match this database', 1;

-- ===== 2007 Husky Invitational =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2007 Husky Invitational' AND Date = '2007-04-14')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2007 Husky Invitational', '2007-04-14', 37, 1, 0, 10, 3, 1, 1, 'https://www.baumspage.com/track/northwestern/res07.htm');
SET @m = SCOPE_IDENTITY();
-- Girls 4x800M Relay: Heidi Santelmann / Jenny Rupp / Amanda Woodhall / Francesca Luppino  final 656.8
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 120, NULL, 656.8, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 450);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 4);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 735);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 773);
-- Boys 4x800M Relay: Kyle Nehrenz / Bret Nehrenz / Will Luppino / Dan Troike  final 561.2
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 118, NULL, 561.2, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 622);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 478);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 145);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 845);
-- Girls 100M Hurdles: Jenny Rupp  prelim 16.4, final 17.6
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (4, @m, 52, NULL, 16.4, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 16.4);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 17.6);
-- Girls 100M Hurdles: Stephanie Foster  prelim 19.8, final 18.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (693, @m, 52, NULL, 18.0, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 19.8);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 18.0);
-- Boys 110M Hurdles: Mike Fuentes  prelim 16.0, final 15.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (694, @m, 55, NULL, 15.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 16.0);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 15.9);
-- Boys 110M Hurdles: Greg Hill  prelim 16.4, final 18.3
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (798, @m, 55, NULL, 16.4, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 16.4);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 18.3);
-- Girls 100M: Morgan O'Rourke  prelim 13.31, final 13.1
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Morgan' AND LastName = 'O''Rourke' AND Gender = 2), @m, 49, NULL, 13.1, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 13.31);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 13.1);
-- Girls 100M: Amber Hansen  prelim 14.11, final 13.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Amber' AND LastName = 'Hansen' AND Gender = 2), @m, 49, NULL, 13.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 14.11);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 13.9);
-- Boys 100M: Cody Roberts  prelim 11.8, final 11.8
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (127, @m, 45, NULL, 11.8, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 11.8);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 11.8);
-- Boys 100M: Kyle Sirl  prelim 12.01, final 12.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (704, @m, 45, NULL, 12.0, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 12.01);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 12.0);
-- Girls 200M: Brittani Pekar  prelim 28.1
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (459, @m, 39, NULL, 28.1, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Caitlyn Kilkenny  prelim 28.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (682, @m, 39, NULL, 28.5, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Cody Roberts  prelim 23.8
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (127, @m, 65, NULL, 23.8, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Dylan Varga  prelim 24.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (370, @m, 65, NULL, 24.0, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Cassy Maxwell  final 1292.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (726, @m, 24, 1292.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Ellen Weeks  final 883.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (244, @m, 24, 883.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Shawn Tamba  final 1121.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (500, @m, 13, 1121.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls High Jump: Amber Hansen  final 58.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Amber' AND LastName = 'Hansen' AND Gender = 2), @m, 11, 58.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys High Jump: Ricky Morrison  final 62.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (138, @m, 14, 62.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys High Jump: Taylor Riggs  final 62.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (71, @m, 14, 62.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Caitlyn Kilkenny  final 182.75
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (682, @m, 25, 182.75, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Long Jump: Cody Roberts  final 231.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (127, @m, 1, 231.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Cassy Maxwell  final 349.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (726, @m, 2, 349.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Amber Hansen  final 333.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Amber' AND LastName = 'Hansen' AND Gender = 2), @m, 2, 333.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Mike Fuentes  final 560.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (694, @m, 22, 560.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Lee Heckler  final 372.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (432, @m, 22, 372.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Pole Vault: Stephanie Foster  final 114.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (693, @m, 17, 114.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Pole Vault: Amber Hansen  final 102.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Amber' AND LastName = 'Hansen' AND Gender = 2), @m, 17, 102.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Jake Lile  final 132.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (269, @m, 3, 132.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Josh Newcomer  final 132.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (229, @m, 3, 132.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2008 Wolverine Invitational =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2008 Wolverine Invitational' AND Date = '2008-04-04')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2008 Wolverine Invitational', '2008-04-04', 56, 1, 0, 3, 3, 1, 1, 'https://www.baumspage.com/track/westgeauga/res08.htm');
SET @m = SCOPE_IDENTITY();
-- Girls 100M: Heidi Santelmann  prelim 14.2, final 14.04
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (450, @m, 49, NULL, 14.04, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 14.2);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 14.04);
-- Girls 100M: Olivia Arnold  prelim 15.06
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (344, @m, 49, NULL, 15.06, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Rachel Gokey  prelim 29.65
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (495, @m, 39, NULL, 29.65, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Mandy Heidelman  prelim 30.6
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (837, @m, 39, NULL, 30.6, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Megan Rybacki  final 67.92
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (13, @m, 46, NULL, 67.92, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Francesca Luppino  final 160.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (773, @m, 50, NULL, 160.0, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Leah Geissman  final 365.09
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (554, @m, 41, NULL, 365.09, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Amanda Woodhall  final 368.32
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (735, @m, 41, NULL, 368.32, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 3200M: Ashlee Burgess  final 821.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (493, @m, 35, NULL, 821.0, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Stephanie Foster  prelim 16.82, final 16.87
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (693, @m, 52, NULL, 16.82, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 16.82);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 16.87);
-- Girls 100M Hurdles: Barbara Mooneyham  prelim 19.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (367, @m, 52, NULL, 19.25, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Heidi Santelmann  final 49.7
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (450, @m, 29, NULL, 49.7, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Janelle Shaffer  final 53.28
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (61, @m, 29, NULL, 53.28, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x100M Relay: Olivia Arnold / Kelli Slavens / Mandy Heidelman / Brittany Pekar  final 56.51
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 106, NULL, 56.51, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 344);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 447);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 837);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 459);
-- Girls 4x200M Relay: Mandy Heidelman / Kelli Slavens / Jenna Elswick / Rachel Gokey  final 121.7
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 111, NULL, 121.7, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 837);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 447);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 494);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 495);
-- Girls 4x400M Relay: Heidi Santelmann / Francesca Luppino / Leah Geissman / Brittany Pekar  final 269.8
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 115, NULL, 269.8, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 450);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 773);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 554);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 459);
-- Girls 4x800M Relay: Taylor Sachleban / Katlyn Donnelly / Leah Geissman / Francesca Luppino  final 681.35
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 120, NULL, 681.35, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 786);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 27);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 554);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 773);
-- Girls High Jump: Jennifer Foster  final 58.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (471, @m, 11, 58.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls High Jump: Brittany Pekar  NH
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (459, @m, 11, NULL, NULL, 0, 0, 0, 2);
SET @p = SCOPE_IDENTITY();
-- Girls Pole Vault: Stephanie Foster  final 108.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (693, @m, 17, 108.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Pole Vault: Paige Lloyd  final 102.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (107, @m, 17, 102.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Janelle Shaffer  final 157.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (61, @m, 25, 157.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Shannon Kilkenny  final 153.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (483, @m, 25, 153.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Kelsey Williams  final 334.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (417, @m, 2, 334.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Ellen Weeks  final 319.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (244, @m, 2, 319.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Corrin Early  final 1055.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (638, @m, 24, 1055.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Ellen Weeks  final 735.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (244, @m, 24, 735.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Cody Roberts  prelim 11.81
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (127, @m, 45, NULL, 11.81, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Jerry Chidsey  prelim 11.87
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (392, @m, 45, NULL, 11.87, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Jerry Chidsey  prelim 24.62
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (392, @m, 65, NULL, 24.62, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Zane Hemming  prelim 25.76
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (287, @m, 65, NULL, 25.76, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Taylor Riggs  final 53.02
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (71, @m, 37, NULL, 53.02, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Joey Bruening  final 143.2
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (10, @m, 34, NULL, 143.2, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Will Luppino  final 291.92
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (145, @m, 56, NULL, 291.92, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Andrew Bertram  final 305.69
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (516, @m, 56, NULL, 305.69, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Bret Nehrenz  final 688.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (478, @m, 62, NULL, 688.0, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 110M Hurdles: Greg Hill  prelim 15.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (798, @m, 55, NULL, 15.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 110M Hurdles: Keith Batesole  prelim 19.26
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (545, @m, 55, NULL, 19.26, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 300M Hurdles: Greg Hill  final 47.3
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (798, @m, 42, NULL, 47.3, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 300M Hurdles: Keith Batesole  final 49.15
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (545, @m, 42, NULL, 49.15, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 4x100M Relay: Jerry Chidsey / Zane Hemming / Kyle Sirl / Cody Roberts  final 48.31
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 99, NULL, 48.31, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 392);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 287);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 704);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 127);
-- Boys 4x400M Relay: Richard Morrison / Greg Hill / Jerry Chidsey / Taylor Riggs  final 229.13
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 108, NULL, 229.13, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 138);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 798);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 392);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 71);
-- Boys 4x800M Relay: Will Luppino / Dennis Berry / Andrew Bertram / Joey Bruening  final 565.46
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 118, NULL, 565.46, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 145);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 821);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 516);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 10);
-- Boys High Jump: Taylor Riggs  NH
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (71, @m, 14, NULL, NULL, 0, 0, 0, 2);
SET @p = SCOPE_IDENTITY();
-- Boys High Jump: Richard Morrison  NH
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (138, @m, 14, NULL, NULL, 0, 0, 0, 2);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Chris Lamphier  final 132.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (369, @m, 3, 132.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Vincent Kacinari  final 96.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (316, @m, 3, 96.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Long Jump: Cody Roberts  final 221.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (127, @m, 1, 221.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Long Jump: Richard Morrison  final 212.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (138, @m, 1, 212.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Kalvin Harrington  final 518.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (784, @m, 22, 518.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Cody Furr  final 473.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (334, @m, 22, 473.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Steve Dembowski  final 1130.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (265, @m, 13, 1130.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: William Simpson  ND
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (637, @m, 13, NULL, NULL, 0, 0, 0, 1);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2011 Optimist Meet =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2011 Optimist Meet' AND Date = '2011-05-07')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2011 Optimist Meet', '2011-05-07', 4, 1, 0, 45, 3, 1, 1, 'https://www.baumspage.com/track/optimist/res11.htm');
SET @m = SCOPE_IDENTITY();
-- Boys Shot Put: Brian Livingstone  final 622.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (419, @m, 22, 622.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Steve Dembowski  final 516.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (265, @m, 22, 516.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Brian Livingstone  final 1657.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (419, @m, 13, 1657.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Steve Dembowski  final 1573.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (265, @m, 13, 1573.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
