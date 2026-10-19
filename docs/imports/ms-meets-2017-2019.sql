-- Single-day meets from baumspage Hy-Tek results (BRAIN.md [C72]); Cloverleaf rows only. Each meet is skipped if a meet with that name and date exists.
-- Rules as in ne-district-2005-2010.sql. Athlete Ids are asserted before anything is inserted.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT, @p INT;
IF (SELECT COUNT(*) FROM Athletes WHERE (Id = 5 AND FirstName = 'Jack' AND LastName = 'Szklarz') OR (Id = 6 AND FirstName = 'Zach' AND LastName = 'Greer') OR (Id = 12 AND FirstName = 'Tommie' AND LastName = 'Kurtz') OR (Id = 17 AND FirstName = 'Cameron' AND LastName = 'McNeal') OR (Id = 22 AND FirstName = 'Greg' AND LastName = 'Miller') OR (Id = 31 AND FirstName = 'Estevan' AND LastName = 'Ortega') OR (Id = 34 AND FirstName = 'Andrew' AND LastName = 'Hibler') OR (Id = 35 AND FirstName = 'Luke' AND LastName = 'Malena') OR (Id = 37 AND FirstName = 'Michael' AND LastName = 'Williams') OR (Id = 63 AND FirstName = 'Elise' AND LastName = 'Patton') OR (Id = 64 AND FirstName = 'Cole' AND LastName = 'Yagersz') OR (Id = 68 AND FirstName = 'Kirsten' AND LastName = 'Wilson') OR (Id = 75 AND FirstName = 'Eli' AND LastName = 'Haynes') OR (Id = 83 AND FirstName = 'Cole' AND LastName = 'Gibson') OR (Id = 86 AND FirstName = 'Alexander' AND LastName = 'Baughman') OR (Id = 87 AND FirstName = 'Jenna' AND LastName = 'Oliver') OR (Id = 88 AND FirstName = 'Tommy' AND LastName = 'Sword') OR (Id = 120 AND FirstName = 'Sara' AND LastName = 'Keener') OR (Id = 121 AND FirstName = 'Liv' AND LastName = 'Wells') OR (Id = 124 AND FirstName = 'Kyle' AND LastName = 'Chudzinski') OR (Id = 133 AND FirstName = 'Dillon' AND LastName = 'West') OR (Id = 148 AND FirstName = 'Katelyn' AND LastName = 'Malena') OR (Id = 149 AND FirstName = 'Grant' AND LastName = 'Swain') OR (Id = 164 AND FirstName = 'Thomas' AND LastName = 'Lormeau') OR (Id = 171 AND FirstName = 'Zack' AND LastName = 'Munchick') OR (Id = 182 AND FirstName = 'Cydnei' AND LastName = 'Miller') OR (Id = 183 AND FirstName = 'Jacob' AND LastName = 'Scheck') OR (Id = 194 AND FirstName = 'Ashley' AND LastName = 'Sigmon') OR (Id = 198 AND FirstName = 'Austin' AND LastName = 'Ebaugh') OR (Id = 199 AND FirstName = 'Kala' AND LastName = 'Perry') OR (Id = 202 AND FirstName = 'TJ' AND LastName = 'Corbett') OR (Id = 207 AND FirstName = 'Bailey' AND LastName = 'Freeland') OR (Id = 213 AND FirstName = 'Meagan' AND LastName = 'Scheck') OR (Id = 215 AND FirstName = 'Ethan' AND LastName = 'Sterling') OR (Id = 220 AND FirstName = 'Arabella' AND LastName = 'Polen') OR (Id = 230 AND FirstName = 'Dana' AND LastName = 'Thur') OR (Id = 232 AND FirstName = 'Matt' AND LastName = 'Stoner') OR (Id = 234 AND FirstName = 'Nolan' AND LastName = 'Freeland') OR (Id = 235 AND FirstName = 'Mariel' AND LastName = 'Leatherman') OR (Id = 236 AND FirstName = 'Arasaylie' AND LastName = 'Byczek') OR (Id = 241 AND FirstName = 'Sarasa' AND LastName = 'Sivapatham') OR (Id = 252 AND FirstName = 'Zana' AND LastName = 'Ross') OR (Id = 266 AND FirstName = 'Corina' AND LastName = 'Cass') OR (Id = 268 AND FirstName = 'Samantha' AND LastName = 'Bergman') OR (Id = 283 AND FirstName = 'Elanna' AND LastName = 'Lear') OR (Id = 292 AND FirstName = 'Maddie' AND LastName = 'Bohan') OR (Id = 293 AND FirstName = 'Adam' AND LastName = 'Oren') OR (Id = 295 AND FirstName = 'Paige' AND LastName = 'Munchick') OR (Id = 302 AND FirstName = 'Cassidy' AND LastName = 'Smith') OR (Id = 309 AND FirstName = 'Brooklynn' AND LastName = 'Crist') OR (Id = 311 AND FirstName = 'Alex' AND LastName = 'Derhammer') OR (Id = 317 AND FirstName = 'Cade' AND LastName = 'Cass') OR (Id = 345 AND FirstName = 'DeAnte' AND LastName = 'Taylor') OR (Id = 353 AND FirstName = 'Joey' AND LastName = 'Kurtz') OR (Id = 366 AND FirstName = 'Natalie' AND LastName = 'Bailey') OR (Id = 388 AND FirstName = 'Natasha' AND LastName = 'Messmore') OR (Id = 393 AND FirstName = 'Liberty' AND LastName = 'Ridenour') OR (Id = 395 AND FirstName = 'Andrew' AND LastName = 'Vasel') OR (Id = 414 AND FirstName = 'Maria' AND LastName = 'Porras') OR (Id = 416 AND FirstName = 'Bryce' AND LastName = 'Betts') OR (Id = 418 AND FirstName = 'Lexi' AND LastName = 'Licursi') OR (Id = 464 AND FirstName = 'Sara' AND LastName = 'Naples') OR (Id = 484 AND FirstName = 'Brianna' AND LastName = 'Gallagher') OR (Id = 505 AND FirstName = 'Frank' AND LastName = 'Stepp') OR (Id = 509 AND FirstName = 'Makaela' AND LastName = 'Salyer') OR (Id = 511 AND FirstName = 'Matt' AND LastName = 'Grissom') OR (Id = 518 AND FirstName = 'Raven' AND LastName = 'Piatt') OR (Id = 525 AND FirstName = 'Jake' AND LastName = 'Kemp') OR (Id = 532 AND FirstName = 'Nick' AND LastName = 'Ziegler') OR (Id = 536 AND FirstName = 'Cooper' AND LastName = 'Hartsel') OR (Id = 541 AND FirstName = 'Cory' AND LastName = 'Starman') OR (Id = 546 AND FirstName = 'Nate' AND LastName = 'Clingan') OR (Id = 555 AND FirstName = 'Joe' AND LastName = 'Villoni') OR (Id = 558 AND FirstName = 'Asa' AND LastName = 'Gynn') OR (Id = 572 AND FirstName = 'Blake' AND LastName = 'Wilson') OR (Id = 582 AND FirstName = 'Ashley' AND LastName = 'Kizzen') OR (Id = 588 AND FirstName = 'Nick' AND LastName = 'Bailey') OR (Id = 590 AND FirstName = 'Katelyn' AND LastName = 'Young') OR (Id = 596 AND FirstName = 'Brie' AND LastName = 'King') OR (Id = 603 AND FirstName = 'Jacob' AND LastName = 'Nau') OR (Id = 604 AND FirstName = 'Jake' AND LastName = 'Arnold') OR (Id = 606 AND FirstName = 'Nathan' AND LastName = 'Szymczak') OR (Id = 607 AND FirstName = 'Riley' AND LastName = 'Freeland') OR (Id = 614 AND FirstName = 'Nyah' AND LastName = 'Hansen') OR (Id = 619 AND FirstName = 'Shae' AND LastName = 'Cornett') OR (Id = 631 AND FirstName = 'John' AND LastName = 'Manzuk') OR (Id = 642 AND FirstName = 'Tristan' AND LastName = 'Vaughan') OR (Id = 645 AND FirstName = 'Kevin' AND LastName = 'Bittaker') OR (Id = 646 AND FirstName = 'Sophia' AND LastName = 'Spencer') OR (Id = 649 AND FirstName = 'Devon' AND LastName = 'Stoehr') OR (Id = 668 AND FirstName = 'Maddie' AND LastName = 'Boltz') OR (Id = 670 AND FirstName = 'Emily' AND LastName = 'Romano') OR (Id = 681 AND FirstName = 'Maddie' AND LastName = 'Marshall') OR (Id = 688 AND FirstName = 'Lisa' AND LastName = 'Wangler') OR (Id = 689 AND FirstName = 'Max' AND LastName = 'Gucker') OR (Id = 691 AND FirstName = 'Patrick' AND LastName = 'McConnell') OR (Id = 692 AND FirstName = 'Derik' AND LastName = 'Tober') OR (Id = 705 AND FirstName = 'Sara' AND LastName = 'Ayesteran') OR (Id = 709 AND FirstName = 'Emilio' AND LastName = 'Rivera') OR (Id = 711 AND FirstName = 'Ray' AND LastName = 'Farnham') OR (Id = 712 AND FirstName = 'Daniel' AND LastName = 'Sommers') OR (Id = 717 AND FirstName = 'Luke' AND LastName = 'Yuravak') OR (Id = 720 AND FirstName = 'Rick' AND LastName = 'Patton') OR (Id = 731 AND FirstName = 'Emma' AND LastName = 'Nagel') OR (Id = 745 AND FirstName = 'Ben' AND LastName = 'Kott') OR (Id = 754 AND FirstName = 'Lexie' AND LastName = 'Lupo') OR (Id = 778 AND FirstName = 'Dylan' AND LastName = 'Craig') OR (Id = 788 AND FirstName = 'Elian' AND LastName = 'Zayed') OR (Id = 828 AND FirstName = 'Steve' AND LastName = 'Manbeck') OR (Id = 833 AND FirstName = 'Nate' AND LastName = 'Brown') OR (Id = 834 AND FirstName = 'Ryan' AND LastName = 'Curtis') OR (Id = 838 AND FirstName = 'Ben' AND LastName = 'Daniels') OR (Id = 844 AND FirstName = 'Brycen' AND LastName = 'Holliday')) <> 113 THROW 50010, 'Athlete Ids do not match this database', 1;

-- ===== 2017 Dwight Schar Invitational =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2017 Dwight Schar Invitational' AND Date = '2017-05-05')
BEGIN
IF NOT EXISTS (SELECT 1 FROM Locations WHERE Name = 'Norwayne High School') INSERT INTO Locations (Name, City, State, Country) VALUES ('Norwayne High School', 'Creston', 'OH', 'USA');
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2017 Dwight Schar Invitational', '2017-05-05', (SELECT Id FROM Locations WHERE Name = 'Norwayne High School'), 1, 0, 40, 3, 1, 3, 'https://oh.milesplit.com/meets/280388');
SET @m = SCOPE_IDENTITY();
-- Boys 100M: Jacob Scheck  final 11.7
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (183, @m, 45, NULL, 11.7, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Zack Munchick  final 11.7
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (171, @m, 45, NULL, 11.7, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Jacob Scheck  final 23.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (183, @m, 65, NULL, 23.5, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Zack Munchick  final 24.4
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (171, @m, 65, NULL, 24.4, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Ryan Curtis  final 56.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (834, @m, 37, NULL, 56.5, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Tristan Vaughan  final 130.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (642, @m, 34, NULL, 130.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Alex Derhammer  final 139.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (311, @m, 34, NULL, 139.5, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Nick Bailey  final 297.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (588, @m, 56, NULL, 297.0, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Max Gucker  final 312.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (689, @m, 56, NULL, 312.0, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Max Gucker  final 653.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (689, @m, 62, NULL, 653.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Estevan Ortega  final 688.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (31, @m, 62, NULL, 688.0, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 110M Hurdles: Rick Patton  final 20.4
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (720, @m, 55, NULL, 20.4, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 300M Hurdles: Bryce Betts  final 51.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (416, @m, 42, NULL, 51.0, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 300M Hurdles: Rick Patton  final 53.2
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (720, @m, 42, NULL, 53.2, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 4x100M Relay: John Manzuk / Zack Munchick / Adam Oren / Jacob Scheck  final 45.1
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 99, NULL, 45.1, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 631);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 171);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 293);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 183);
-- Boys 4x200M Relay: Eli Haynes / Derik Tober / Devon Stoehr / Matt Stoner  final 107.8
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 114, NULL, 107.8, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 75);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 692);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 649);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 232);
-- Boys 4x400M Relay: Tristan Vaughan / Ryan Curtis / Matt Stoner / Adam Oren  final 227.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 108, NULL, 227.5, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 642);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 834);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 232);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 293);
-- Boys 4x800M Relay: Nick Bailey / Dillon West / Alex Derhammer / Tristan Vaughan  final 537.6
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 118, NULL, 537.6, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 588);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 133);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 311);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 642);
-- Boys High Jump: John Manzuk  final 64.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (631, @m, 14, 64.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Long Jump: John Manzuk  final 209.75
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (631, @m, 1, 209.75, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Long Jump: Adam Oren  final 187.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (293, @m, 1, 187.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Nate Clingan  final 494.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (546, @m, 22, 494.25, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Dylan Craig  final 473.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (778, @m, 22, 473.25, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Dylan Craig  final 1270.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (778, @m, 13, 1270.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Nate Clingan  final 1118.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (546, @m, 13, 1118.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2017 Jackson Twilight Invitational =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2017 Jackson Twilight Invitational' AND Date = '2017-05-05')
BEGIN
IF NOT EXISTS (SELECT 1 FROM Locations WHERE Name = 'Jackson High School') INSERT INTO Locations (Name, City, State, Country) VALUES ('Jackson High School', 'Massillon', 'OH', 'USA');
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2017 Jackson Twilight Invitational', '2017-05-05', (SELECT Id FROM Locations WHERE Name = 'Jackson High School'), 1, 0, 40, 3, 1, 3, 'https://oh.milesplit.com/meets/280389');
SET @m = SCOPE_IDENTITY();
-- Girls 100M: Kirsten Wilson  prelim 14.08
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (68, @m, 49, NULL, 14.08, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M: Nyah Hansen  prelim 14.18
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (614, @m, 49, NULL, 14.18, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Sara Keener  prelim 28.16
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (120, @m, 39, NULL, 28.16, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Nyah Hansen  prelim 30.26
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (614, @m, 39, NULL, 30.26, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Lexi Licursi  final 70.96000000000001
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (418, @m, 46, NULL, 70.96, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Maddie Marshall  final 71.57
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (681, @m, 46, NULL, 71.57, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Riley Freeland  final 319.19
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (607, @m, 41, NULL, 319.19, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Jenna Oliver  final 350.43
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (87, @m, 41, NULL, 350.43, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Zana Ross  prelim 16.41, final 16.3
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (252, @m, 52, NULL, 16.3, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 16.41);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 16.3);
-- Girls 100M Hurdles: Sarasa Sivapatham  prelim 19.42
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (241, @m, 52, NULL, 19.42, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x100M Relay: Maddie Boltz / Sara Keener / Shae Cornett / Kirsten Wilson  final 54.62
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 106, NULL, 54.62, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 668);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 120);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 619);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 68);
-- Girls 4x800M Relay: Riley Freeland / Bailey Freeland / Jenna Oliver / Tommie Kurtz  final 617.26
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 120, NULL, 617.26, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 607);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 207);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 87);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
-- Girls Long Jump: Maddie Boltz  final 170.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (668, @m, 25, 170.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Lexi Licursi  final 160.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (418, @m, 25, 160.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Lisa Wangler  final 383.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (688, @m, 2, 383.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Katelyn Young  final 382.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (590, @m, 2, 382.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Katelyn Young  final 1452.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (590, @m, 24, 1452.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Lisa Wangler  final 1097.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (688, @m, 24, 1097.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2018 New London Relays =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2018 New London Relays', '2018-04-07', 35, 1, 0, 1, 3, 1, 3, 'https://oh.milesplit.com/meets/311971');
SET @m = SCOPE_IDENTITY();
-- Girls 4x100M Relay: Maddie Boltz / Sara Keener / Nyah Hansen / Emily Romano  final 54.3
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 106, NULL, 54.3, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 668);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 120);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 614);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 670);
-- Girls 4x200M Relay: Maddie Boltz / Sara Keener / Brianna Gallagher / Sara Ayesteran  final 119.3
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 111, NULL, 119.3, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 668);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 120);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 484);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 705);
-- Girls 4x400M Relay: Jenna Oliver / Tommie Kurtz / Sara Keener / Maddie Boltz  final 270.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 115, NULL, 270.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 87);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 120);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 668);
-- Girls 4x800M Relay: Tommie Kurtz / Bailey Freeland / Maria Porras / Jenna Oliver  final 670.2
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 120, NULL, 670.2, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 207);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 414);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 87);
-- Girls 4x1600M Relay: Bailey Freeland / Katelyn Malena / Sara Naples / Maria Porras  final 1500.8
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 110, NULL, 1500.8, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 207);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 148);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 464);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 414);
-- Boys 4x100M Relay: Adam Oren / John Manzuk / Eli Haynes / Zack Munchick  final 47.6
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 99, NULL, 47.6, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 293);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 631);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 75);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 171);
-- Boys 4x200M Relay: Zack Munchick / Blake Wilson / Matt Stoner / Ryan Curtis  final 99.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 114, NULL, 99.0, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 171);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 572);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 232);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 834);
-- Boys 4x400M Relay: Brycen Holliday / Ryan Curtis / Thomas Lormeau / Matt Stoner  final 233.8
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 108, NULL, 233.8, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 844);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 834);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 164);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 232);
-- Boys 4x800M Relay: Nick Bailey / Frank Stepp / Dillon West / Tristan Vaughan  final 546.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 118, NULL, 546.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 588);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 505);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 133);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 642);
-- Boys 4x1600M Relay: Nick Bailey / Max Gucker / Frank Stepp / Tristan Vaughan  final 1223.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 105, NULL, 1223.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 588);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 689);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 505);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 642);
-- Boys 4x110M Shuttle Hurdles: Bryce Betts / DeAnte Taylor / Tommy Sword / Jacob Nau  final 79.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 109, NULL, 79.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 416);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 345);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 88);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 603);
END

-- ===== 2019 New London Relays =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2019 New London Relays', '2019-04-06', 35, 1, 0, 15, 3, 1, 3, 'https://oh.milesplit.com/meets/352490');
SET @m = SCOPE_IDENTITY();
-- Girls 4x1600M Relay: Makaela Salyer / Tommie Kurtz / Maria Porras / Jenna Oliver  final 1509.3
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 110, NULL, 1509.3, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 509);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 414);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 87);
-- Boys 4x1600M Relay: Max Gucker / Brycen Holliday / Nick Bailey / Alex Derhammer  final 1181.4
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 105, NULL, 1181.4, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 689);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 844);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 588);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 311);
-- Girls 4x100M Shuttle Hurdles: Liberty Ridenour / Natalie Bailey / Elise Patton / Kala Perry  final 81.1
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 119, NULL, 81.1, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 393);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 366);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 63);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 199);
-- Boys 4x110M Shuttle Hurdles: Bryce Betts / Ray Farnham / Austin Ebaugh / DeAnte Taylor  final 74.8
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 109, NULL, 74.8, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 416);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 711);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 198);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 345);
-- Girls 4x100M Relay: Nyah Hansen / Paige Munchick / Elanna Lear / Maddie Boltz  final 54.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 106, NULL, 54.0, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 614);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 295);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 283);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 668);
-- Girls 4x200M Relay: Nyah Hansen / Maddie Boltz / Ashley Kizzen / Dana Thur  final 116.7
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 111, NULL, 116.7, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 614);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 668);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 582);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 230);
-- Girls 4x800M Relay: Tommie Kurtz / Paige Munchick / Sophia Spencer / Maria Porras  final 660.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 120, NULL, 660.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 295);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 646);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 414);
-- Boys 4x800M Relay: Nick Bailey / Frank Stepp / Brycen Holliday / Alex Derhammer  final 530.8
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 118, NULL, 530.8, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 588);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 505);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 844);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 311);
-- Girls 4x400M Relay: Tommie Kurtz / Maddie Boltz / Dana Thur / Paige Munchick  final 278.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 115, NULL, 278.0, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 668);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 230);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 295);
-- Boys 4x400M Relay: Frank Stepp / Nick Bailey / DeAnte Taylor / Brycen Holliday  final 219.2
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 108, NULL, 219.2, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 505);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 588);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 345);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 844);
END

-- ===== 2019 Ravenna-Streetsboro Double Dual =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16')
BEGIN
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Ben' AND LastName = 'Nagy' AND Gender = 1) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Ben', 'Nagy', 1, 2020, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Bridgette' AND LastName = 'Donahue' AND Gender = 2) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Bridgette', 'Donahue', 2, 2021, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Caleb' AND LastName = 'Hill' AND Gender = 1) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Caleb', 'Hill', 1, 2021, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Evan' AND LastName = 'Allen' AND Gender = 1) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Evan', 'Allen', 1, 2021, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Joey' AND LastName = 'Weills' AND Gender = 2) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Joey', 'Weills', 2, 2022, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Jordan' AND LastName = 'Smith' AND Gender = 1) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Jordan', 'Smith', 1, 2020, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Megan' AND LastName = 'Van Meter' AND Gender = 2) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Megan', 'Van Meter', 2, 2020, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Savanna' AND LastName = 'Brooks' AND Gender = 2) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Savanna', 'Brooks', 2, 2022, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Scott' AND LastName = 'Stoner' AND Gender = 1) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Scott', 'Stoner', 1, 2020, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Sean' AND LastName = 'Barker' AND Gender = 1) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Sean', 'Barker', 1, 2022, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Zane' AND LastName = 'Evans' AND Gender = 1) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Zane', 'Evans', 1, 2022, 0);
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2019 Ravenna-Streetsboro Double Dual', '2019-04-16', 45, 1, 0, 15, 3, 1, 2, 'https://oh.milesplit.com/meets/354958');
SET @m = SCOPE_IDENTITY();
-- Girls 100M: Maddie Boltz  final 12.93
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (668, @m, 49, NULL, 12.93, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M: Nyah Hansen  final 13.15
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (614, @m, 49, NULL, 13.15, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M: Elanna Lear  final 14.28
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (283, @m, 49, NULL, 14.28, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M: Bridgette Donahue  final 65.4
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Bridgette' AND LastName = 'Donahue' AND Gender = 2), @m, 49, NULL, 65.4, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Maddie Boltz  final 28.06
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (668, @m, 39, NULL, 28.06, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Nyah Hansen  final 28.91
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (614, @m, 39, NULL, 28.91, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Elanna Lear  final 30.36
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (283, @m, 39, NULL, 30.36, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Tommie Kurtz  final 62.09
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (12, @m, 46, NULL, 62.09, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Emma Nagel  final 70.03
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (731, @m, 46, NULL, 70.03, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Arabella Polen  final 73.37
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (220, @m, 46, NULL, 73.37, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Cydnei Miller  final 74.33
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (182, @m, 46, NULL, 74.33, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Liv Wells  final 75.03
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (121, @m, 46, NULL, 75.03, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Bailey Freeland  final 158.12
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (207, @m, 50, NULL, 158.12, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Sophia Spencer  final 160.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (646, @m, 50, NULL, 160.25, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Tommie Kurtz  final 161.65
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (12, @m, 50, NULL, 161.65, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Bailey Freeland  final 356.89
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (207, @m, 41, NULL, 356.89, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Maria Porras  final 359.61
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (414, @m, 41, NULL, 359.61, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Jenna Oliver  final 362.7
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (87, @m, 41, NULL, 362.7, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Makaela Salyer  final 366.58
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (509, @m, 41, NULL, 366.58, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Maddie Bohan  final 414.40999999999997
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (292, @m, 41, NULL, 414.41, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Brooklynn Crist  final 438.33
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (309, @m, 41, NULL, 438.33, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Paige Munchick  final 18.88
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (295, @m, 52, NULL, 18.88, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Natalie Bailey  final 20.17
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (366, @m, 52, NULL, 20.17, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Elise Patton  final 21.22
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (63, @m, 52, NULL, 21.22, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Paige Munchick  final 52.48
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (295, @m, 29, NULL, 52.48, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Natalie Bailey  final 56.57
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (366, @m, 29, NULL, 56.57, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x100M Relay: Nyah Hansen / Paige Munchick / Ashley Kizzen / Dana Thur  final 53.55
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 106, NULL, 53.55, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 614);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 295);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 582);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 230);
-- Girls 4x200M Relay: Elanna Lear / Liberty Ridenour / Natalie Bailey / Cydnei Miller  final 125.62
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 111, NULL, 125.62, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 283);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 393);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 366);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 182);
-- Girls 4x400M Relay: Emma Nagel / Jenna Oliver / Liv Wells / Arabella Polen  final 287.48
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 115, NULL, 287.48, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 731);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 87);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 121);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 220);
-- Girls High Jump: Ashley Sigmon  final 59.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (194, @m, 11, 59.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls High Jump: Maddie Boltz  final 54.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (668, @m, 11, 54.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls High Jump: Cydnei Miller  final 50.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (182, @m, 11, 50.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Maddie Boltz  final 192.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (668, @m, 25, 192.25, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Dana Thur  final 175.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (230, @m, 25, 175.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Arabella Polen  final 156.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (220, @m, 25, 156.25, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Meagan Scheck  final 346.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (213, @m, 2, 346.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Megan Van Meter  final 291.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Megan' AND LastName = 'Van Meter' AND Gender = 2), @m, 2, 291.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Maddie Marshall  final 280.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (681, @m, 2, 280.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Meagan Scheck  final 1091.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (213, @m, 24, 1091.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Maddie Marshall  final 871.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (681, @m, 24, 871.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Blake Wilson  final 11.77
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (572, @m, 45, NULL, 11.77, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Michael Williams  final 11.91
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (37, @m, 45, NULL, 11.91, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: John Manzuk  final 12.14
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (631, @m, 45, NULL, 12.14, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Cade Cass  final 12.86
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (317, @m, 45, NULL, 12.86, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Blake Wilson  final 23.76
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (572, @m, 65, NULL, 23.76, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Frank Stepp  final 24.2
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (505, @m, 65, NULL, 24.2, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Luke Yuravak  final 25.03
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (717, @m, 65, NULL, 25.03, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Nick Ziegler  final 25.48
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (532, @m, 65, NULL, 25.48, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Cade Cass  final 25.84
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (317, @m, 65, NULL, 25.84, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Cole Yagersz  final 25.85
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (64, @m, 65, NULL, 25.85, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Kevin Bittaker  final 26.12
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (645, @m, 65, NULL, 26.12, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Emilio Rivera  final 26.35
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (709, @m, 65, NULL, 26.35, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Asa Gynn  final 26.61
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (558, @m, 65, NULL, 26.61, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Ben Kott  final 26.62
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (745, @m, 65, NULL, 26.62, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Grant Swain  final 26.68
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (149, @m, 65, NULL, 26.68, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Caleb Hill  final 28.62
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Caleb' AND LastName = 'Hill' AND Gender = 1), @m, 65, NULL, 28.62, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Joe Villoni  final 28.96
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (555, @m, 65, NULL, 28.96, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Thomas Lormeau  final 53.89
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (164, @m, 37, NULL, 53.89, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Frank Stepp  final 54.18
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (505, @m, 37, NULL, 54.18, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Nick Ziegler  final 56.48
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (532, @m, 37, NULL, 56.48, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Cade Cass  final 56.91
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (317, @m, 37, NULL, 56.91, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Nick Bailey  final 129.24
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (588, @m, 34, NULL, 129.24, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Brycen Holliday  final 129.87
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (844, @m, 34, NULL, 129.87, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Andrew Hibler  final 144.15
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (34, @m, 34, NULL, 144.15, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Thomas Lormeau  final 144.21
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (164, @m, 34, NULL, 144.21, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Matt Grissom  final 147.82999999999998
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (511, @m, 34, NULL, 147.83, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Ben Nagy  final 154.04
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Ben' AND LastName = 'Nagy' AND Gender = 1), @m, 34, NULL, 154.04, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Nolan Freeland  final 154.93
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (234, @m, 34, NULL, 154.93, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Scott Stoner  final 159.37
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Scott' AND LastName = 'Stoner' AND Gender = 1), @m, 34, NULL, 159.37, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Jack Szklarz  final 162.03
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (5, @m, 34, NULL, 162.03, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Zane Evans  final 179.09
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Zane' AND LastName = 'Evans' AND Gender = 1), @m, 34, NULL, 179.09, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Max Gucker  final 290.86
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (689, @m, 56, NULL, 290.86, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Alex Derhammer  final 299.82
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (311, @m, 56, NULL, 299.82, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Andrew Hibler  final 305.61
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (34, @m, 56, NULL, 305.61, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Matt Grissom  final 315.92
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (511, @m, 56, NULL, 315.92, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Ben Nagy  final 337.57
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Ben' AND LastName = 'Nagy' AND Gender = 1), @m, 56, NULL, 337.57, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Nolan Freeland  final 338.36
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (234, @m, 56, NULL, 338.36, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Joey Kurtz  final 340.15999999999997
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (353, @m, 56, NULL, 340.16, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 110M Hurdles: DeAnte Taylor  final 16.95
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (345, @m, 55, NULL, 16.95, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 110M Hurdles: Bryce Betts  final 18.48
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (416, @m, 55, NULL, 18.48, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 110M Hurdles: Ray Farnham  final 19.96
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (711, @m, 55, NULL, 19.96, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 300M Hurdles: DeAnte Taylor  final 45.96
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (345, @m, 42, NULL, 45.96, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 300M Hurdles: Austin Ebaugh  final 46.81
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (198, @m, 42, NULL, 46.81, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 4x100M Relay: John Manzuk / Michael Williams / Austin Ebaugh / Blake Wilson  final 50.06
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 99, NULL, 50.06, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 631);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 37);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 198);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 572);
-- Boys 4x200M Relay: Austin Ebaugh / Frank Stepp / Cade Cass / Blake Wilson  final 104.66
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 114, NULL, 104.66, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 198);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 505);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 317);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 572);
-- Boys 4x400M Relay: Nick Ziegler / Andrew Hibler / Max Gucker / Asa Gynn  final 255.81
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 108, NULL, 255.81, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 532);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 34);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 689);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 558);
-- Boys High Jump: Nick Bailey  final 62.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (588, @m, 14, 62.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys High Jump: Ben Kott  final 60.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (745, @m, 14, 60.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Long Jump: Michael Williams  final 227.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (37, @m, 1, 227.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Long Jump: Jacob Nau  final 217.75
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (603, @m, 1, 217.75, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Jake Kemp  final 503.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (525, @m, 22, 503.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Ethan Sterling  final 488.75
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (215, @m, 22, 488.75, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Jake Kemp  final 1344.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (525, @m, 13, 1344.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Max Gucker  final 647.3
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (689, @m, 62, NULL, 647.3, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Kyle Chudzinski  final 656.56
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (124, @m, 62, NULL, 656.56, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Andrew Vasel  final 700.04
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (395, @m, 62, NULL, 700.04, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Joey Kurtz  final 714.69
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (353, @m, 62, NULL, 714.69, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 3200M: Makaela Salyer  final 798.8
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (509, @m, 35, NULL, 798.8, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 3200M: Maria Porras  final 818.63
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (414, @m, 35, NULL, 818.63, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x800M Relay: Maria Porras / Sophia Spencer / Liv Wells / Tommie Kurtz  final 661.7
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 120, NULL, 661.7, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 414);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 646);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 121);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
-- Boys 4x800M Relay: Nick Bailey / Frank Stepp / Brycen Holliday / Alex Derhammer  final 533.79
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 118, NULL, 533.79, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 588);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 505);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 844);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 311);
END

-- ===== 2019 Triway Invitational =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2019 Triway Invitational' AND Date = '2019-04-20')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2019 Triway Invitational', '2019-04-20', 52, 1, 0, 15, 3, 1, 3, 'https://oh.milesplit.com/meets/357355');
SET @m = SCOPE_IDENTITY();
-- Girls 100M: Nyah Hansen  prelim 13.86
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (614, @m, 49, NULL, 13.86, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M: Corina Cass  prelim 15.08
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (266, @m, 49, NULL, 15.08, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Nyah Hansen  prelim 29.23
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (614, @m, 39, NULL, 29.23, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Elanna Lear  prelim 30.93
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (283, @m, 39, NULL, 30.93, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Emma Nagel  final 69.3
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (731, @m, 46, NULL, 69.3, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Arabella Polen  final 73.32
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (220, @m, 46, NULL, 73.32, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Tommie Kurtz  final 147.27
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (12, @m, 50, NULL, 147.27, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Sophia Spencer  final 176.41
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (646, @m, 50, NULL, 176.41, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Jenna Oliver  final 352.19
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (87, @m, 41, NULL, 352.19, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Natalie Bailey  prelim 20.44
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (366, @m, 52, NULL, 20.44, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Liberty Ridenour  prelim 21.44
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (393, @m, 52, NULL, 21.44, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Paige Munchick  final 52.94
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (295, @m, 29, NULL, 52.94, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Natalie Bailey  final 56.62
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (366, @m, 29, NULL, 56.62, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x100M Relay: Nyah Hansen / Natalie Bailey / Corina Cass / Liberty Ridenour  final 57.23
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 106, NULL, 57.23, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 614);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 366);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 266);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 393);
-- Girls 4x200M Relay: Nyah Hansen / Liberty Ridenour / Elanna Lear / Cydnei Miller  final 122.34
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 111, NULL, 122.34, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 614);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 393);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 283);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 182);
-- Girls 4x400M Relay: Tommie Kurtz / Maddie Boltz / Emma Nagel / Bailey Freeland  final 265.88
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 115, NULL, 265.88, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 668);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 731);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 207);
-- Girls 4x800M Relay: Paige Munchick / Tommie Kurtz / Sophia Spencer / Bailey Freeland  final 637.49
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 120, NULL, 637.49, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 295);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 646);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 207);
-- Girls Long Jump: Maddie Boltz  final 190.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (668, @m, 25, 190.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Michael Williams  prelim 12.06, final 11.98
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (37, @m, 45, NULL, 11.98, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 12.06);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 11.98);
-- Boys 100M: Austin Ebaugh  prelim 12.61
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (198, @m, 45, NULL, 12.61, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Blake Wilson  prelim 24.11, final 23.68
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (572, @m, 65, NULL, 23.68, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 1, 24.11);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (@p, 3, 23.68);
-- Boys 200M: Luke Yuravak  prelim 25.14
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (717, @m, 65, NULL, 25.14, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Thomas Lormeau  final 53.46
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (164, @m, 37, NULL, 53.46, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Nick Ziegler  final 55.49
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (532, @m, 37, NULL, 55.49, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Nick Bailey  final 124.51
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (588, @m, 34, NULL, 124.51, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Brycen Holliday  final 126.27
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (844, @m, 34, NULL, 126.27, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Alex Derhammer  final 297.69
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (311, @m, 56, NULL, 297.69, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Andrew Hibler  final 300.27
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (34, @m, 56, NULL, 300.27, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Max Gucker  final 593.91
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (689, @m, 62, NULL, 593.91, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Kyle Chudzinski  final 660.45
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (124, @m, 62, NULL, 660.45, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 110M Hurdles: Bryce Betts  prelim 20.15
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (416, @m, 55, NULL, 20.15, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 110M Hurdles: Ray Farnham  prelim 20.98
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (711, @m, 55, NULL, 20.98, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 300M Hurdles: Austin Ebaugh  final 46.67
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (198, @m, 42, NULL, 46.67, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 300M Hurdles: Bryce Betts  final 51.05
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (416, @m, 42, NULL, 51.05, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 4x100M Relay: John Manzuk / Michael Williams / Frank Stepp / Blake Wilson  final 46.11
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 99, NULL, 46.11, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 631);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 37);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 505);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 572);
-- Boys 4x200M Relay: Austin Ebaugh / Jake Arnold / Cade Cass / Luke Yuravak  final 100.86
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 114, NULL, 100.86, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 198);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 604);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 317);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 717);
-- Boys 4x400M Relay: Blake Wilson / Frank Stepp / Thomas Lormeau / Nick Bailey  final 214.7
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 108, NULL, 214.7, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 572);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 505);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 164);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 588);
-- Boys 4x800M Relay: Nick Bailey / Frank Stepp / Alex Derhammer / Brycen Holliday  final 510.85
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 118, NULL, 510.85, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 588);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 505);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 311);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 844);
END

-- ===== 2019 Smithie Frosh-Soph Invitational =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2019 Smithie Frosh-Soph Invitational' AND Date = '2019-05-06')
BEGIN
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Caleb' AND LastName = 'Hill' AND Gender = 1) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Caleb', 'Hill', 1, 2021, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Evan' AND LastName = 'Allen' AND Gender = 1) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Evan', 'Allen', 1, 2021, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Joey' AND LastName = 'Weills' AND Gender = 2) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Joey', 'Weills', 2, 2022, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Savanna' AND LastName = 'Brooks' AND Gender = 2) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Savanna', 'Brooks', 2, 2022, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Sean' AND LastName = 'Barker' AND Gender = 1) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Sean', 'Barker', 1, 2022, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Trevor' AND LastName = 'Millbrandt' AND Gender = 1) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Trevor', 'Millbrandt', 1, 2022, 0);
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE FirstName = 'Zane' AND LastName = 'Evans' AND Gender = 1) INSERT INTO Athletes (FirstName, LastName, Gender, GraduationYear, IsActive) VALUES ('Zane', 'Evans', 1, 2022, 0);
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2019 Smithie Frosh-Soph Invitational', '2019-05-06', 48, 1, 0, 15, 3, 2, 3, 'https://oh.milesplit.com/meets/359347');
SET @m = SCOPE_IDENTITY();
-- Boys 100M: Patrick McConnell  final 12.98
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (691, @m, 45, NULL, 12.98, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Kevin Bittaker  final 13.02
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (645, @m, 45, NULL, 13.02, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Alexander Baughman  final 13.04
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (86, @m, 45, NULL, 13.04, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Grant Swain  final 13.05
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (149, @m, 45, NULL, 13.05, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Sean Barker  final 13.66
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Sean' AND LastName = 'Barker' AND Gender = 1), @m, 45, NULL, 13.66, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Caleb Hill  final 14.03
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Caleb' AND LastName = 'Hill' AND Gender = 1), @m, 45, NULL, 14.03, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 100M: Zane Evans  final 15.08
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Zane' AND LastName = 'Evans' AND Gender = 1), @m, 45, NULL, 15.08, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Kevin Bittaker  final 26.63
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (645, @m, 65, NULL, 26.63, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Patrick McConnell  final 27.05
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (691, @m, 65, NULL, 27.05, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Cooper Hartsel  final 27.8
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (536, @m, 65, NULL, 27.8, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Alexander Baughman  final 28.56
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (86, @m, 65, NULL, 28.56, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: T.J. Corbett  final 29.32
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (202, @m, 65, NULL, 29.32, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 200M: Caleb Hill  final 30.05
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Caleb' AND LastName = 'Hill' AND Gender = 1), @m, 65, NULL, 30.05, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Cooper Hartsel  final 62.86
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (536, @m, 37, NULL, 62.86, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Patrick McConnell  final 64.08
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (691, @m, 37, NULL, 64.08, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Trevor Millbrandt  final 64.59
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Trevor' AND LastName = 'Millbrandt' AND Gender = 1), @m, 37, NULL, 64.59, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Sean Barker  final 65.44
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Sean' AND LastName = 'Barker' AND Gender = 1), @m, 37, NULL, 65.44, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Steve Manbeck  final 66.55
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (828, @m, 37, NULL, 66.55, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: T.J. Corbett  final 67.78
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (202, @m, 37, NULL, 67.78, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Alexander Baughman  final 69.87
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (86, @m, 37, NULL, 69.87, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 400M: Joe Villoni  final 71.41
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (555, @m, 37, NULL, 71.41, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Cade Cass  final 138.02
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (317, @m, 34, NULL, 138.02, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Cole Yagersz  final 142.47
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (64, @m, 34, NULL, 142.47, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Nolan Freeland  final 162.12
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (234, @m, 34, NULL, 162.12, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Zane Evans  final 168.7
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Zane' AND LastName = 'Evans' AND Gender = 1), @m, 34, NULL, 168.7, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 800M: Jack Szklarz  final 172.23
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (5, @m, 34, NULL, 172.23, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Andrew Hibler  final 314.62
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (34, @m, 56, NULL, 314.62, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Jack Szklarz  final 341.88
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (5, @m, 56, NULL, 341.88, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 1600M: Nolan Freeland  final 361.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (234, @m, 56, NULL, 361.25, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 3200M: Luke Malena  final 681.49
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (35, @m, 62, NULL, 681.49, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 110M Hurdles: Ray Farnham  final 20.07
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (711, @m, 55, NULL, 20.07, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 300M Hurdles: Ray Farnham  final 49.32
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (711, @m, 42, NULL, 49.32, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys 4x100M Relay: Emilio Rivera / Grant Swain / Ben Kott / Luke Yuravak  final 48.69
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 99, NULL, 48.69, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 709);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 149);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 745);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 717);
-- Boys 4x200M Relay: Emilio Rivera / Cade Cass / Ben Kott / Luke Yuravak  final 102.67
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 114, NULL, 102.67, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 709);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 317);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 745);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 717);
-- Boys 4x400M Relay: Cade Cass / Andrew Hibler / Cole Yagersz / Steve Manbeck  final 233.48
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 108, NULL, 233.48, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 317);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 34);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 64);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 828);
-- Boys 4x800M Relay: Andrew Hibler / Steve Manbeck / Nolan Freeland / Cole Yagersz  final 592.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 118, NULL, 592.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 34);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 828);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 234);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 64);
-- Boys High Jump: Ben Kott  NH
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (745, @m, 14, NULL, NULL, 0, 0, 0, 2);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Nate Brown  final 120.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (833, @m, 3, 120.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Daniel Sommers  final 96.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (712, @m, 3, 96.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Luke Malena  NH
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (35, @m, 3, NULL, NULL, 0, 0, 0, 2);
SET @p = SCOPE_IDENTITY();
-- Boys Pole Vault: Sean Barker  NH
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Sean' AND LastName = 'Barker' AND Gender = 1), @m, 3, NULL, NULL, 0, 0, 0, 2);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Tommy Sword  final 437.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (88, @m, 22, 437.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Zach Greer  final 398.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (6, @m, 22, 398.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Elian Zayed  final 302.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (788, @m, 22, 302.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Greg Miller  final 275.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (22, @m, 22, 275.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Shot Put: Evan Allen  final 0.25
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Evan' AND LastName = 'Allen' AND Gender = 1), @m, 22, 0.25, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Ben Kott  final 1326.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (745, @m, 13, 1326.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Luke Yuravak  final 1183.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (717, @m, 13, 1183.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Tommy Sword  final 1086.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (88, @m, 13, 1086.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Greg Miller  final 1061.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (22, @m, 13, 1061.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Zach Greer  final 1060.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (6, @m, 13, 1060.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Evan Allen  final 901.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Evan' AND LastName = 'Allen' AND Gender = 1), @m, 13, 901.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Boys Discus: Elian Zayed  final 687.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (788, @m, 13, 687.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M: Elanna Lear  final 14.84
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (283, @m, 49, NULL, 14.84, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M: Brie King  final 16.33
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (596, @m, 49, NULL, 16.33, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Elanna Lear  final 30.82
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (283, @m, 39, NULL, 30.82, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Samantha Bergman  final 33.75
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (268, @m, 39, NULL, 33.75, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 200M: Natalie Bailey  final 35.88
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (366, @m, 39, NULL, 35.88, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Sophia Spencer  final 73.47
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (646, @m, 46, NULL, 73.47, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 400M: Arabella Polen  final 77.95
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (220, @m, 46, NULL, 77.95, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 800M: Maria Porras  final 165.02
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (414, @m, 50, NULL, 165.02, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 1600M: Makaela Salyer  final 373.8
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (509, @m, 41, NULL, 373.8, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Liberty Ridenour  final 20.73
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (393, @m, 52, NULL, 20.73, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Elise Patton  final 21.32
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (63, @m, 52, NULL, 21.32, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Kala Perry  final 21.54
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (199, @m, 52, NULL, 21.54, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 100M Hurdles: Joey Weills  final 22.81
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Joey' AND LastName = 'Weills' AND Gender = 2), @m, 52, NULL, 22.81, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Liberty Ridenour  final 57.9
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (393, @m, 29, NULL, 57.9, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Kala Perry  final 62.64
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (199, @m, 29, NULL, 62.64, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Joey Weills  final 64.89
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Joey' AND LastName = 'Weills' AND Gender = 2), @m, 29, NULL, 64.89, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 300M Hurdles: Elise Patton  final 66.8
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (63, @m, 29, NULL, 66.8, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls 4x100M Relay: Elanna Lear / Liberty Ridenour / Natalie Bailey / Cassidy Smith  final 59.12
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 106, NULL, 59.12, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 283);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 393);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 366);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 302);
-- Girls 4x200M Relay: Elanna Lear / Cydnei Miller / Natalie Bailey / Liberty Ridenour  final 124.23
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 111, NULL, 124.23, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 283);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 182);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 366);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 393);
-- Girls 4x800M Relay: Maria Porras / Makaela Salyer / Arabella Polen / Sara Naples  final 711.2
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, @m, 120, NULL, 711.2, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 414);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 509);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 220);
INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 464);
-- Girls Pole Vault: Cassidy Smith  final 84.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (302, @m, 17, 84.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Long Jump: Arabella Polen  final 145.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (220, @m, 25, 145.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Raven Piatt  final 277.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (518, @m, 2, 277.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Natasha Messmore  final 274.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (388, @m, 2, 274.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Savanna Brooks  final 258.5
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Savanna' AND LastName = 'Brooks' AND Gender = 2), @m, 2, 258.5, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Shot Put: Arasaylie Byczek  final 203.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (236, @m, 2, 203.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Cydnei Miller  final 883.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (182, @m, 24, 883.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Savanna Brooks  final 734.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Savanna' AND LastName = 'Brooks' AND Gender = 2), @m, 24, 734.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Raven Piatt  final 674.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (518, @m, 24, 674.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Natasha Messmore  final 650.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (388, @m, 24, 650.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
-- Girls Discus: Arasaylie Byczek  final 585.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (236, @m, 24, 585.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
