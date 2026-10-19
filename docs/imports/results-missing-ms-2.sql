-- Cloverleaf results present on MileSplit but missing from meets the DB already has (BRAIN.md [C74]). Inserts only; never touches an existing row.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @p INT;
IF (SELECT COUNT(*) FROM Athletes WHERE (Id = 12 AND FirstName = 'Tommie' AND LastName = 'Kurtz') OR (Id = 34 AND FirstName = 'Andrew' AND LastName = 'Hibler') OR (Id = 37 AND FirstName = 'Michael' AND LastName = 'Williams') OR (Id = 75 AND FirstName = 'Eli' AND LastName = 'Haynes') OR (Id = 87 AND FirstName = 'Jenna' AND LastName = 'Oliver') OR (Id = 88 AND FirstName = 'Tommy' AND LastName = 'Sword') OR (Id = 121 AND FirstName = 'Liv' AND LastName = 'Wells') OR (Id = 146 AND FirstName = 'Jimmy' AND LastName = 'Leahy') OR (Id = 164 AND FirstName = 'Thomas' AND LastName = 'Lormeau') OR (Id = 171 AND FirstName = 'Zack' AND LastName = 'Munchick') OR (Id = 194 AND FirstName = 'Ashley' AND LastName = 'Sigmon') OR (Id = 198 AND FirstName = 'Austin' AND LastName = 'Ebaugh') OR (Id = 213 AND FirstName = 'Meagan' AND LastName = 'Scheck') OR (Id = 215 AND FirstName = 'Ethan' AND LastName = 'Sterling') OR (Id = 230 AND FirstName = 'Dana' AND LastName = 'Thur') OR (Id = 287 AND FirstName = 'Zane' AND LastName = 'Hemming') OR (Id = 293 AND FirstName = 'Adam' AND LastName = 'Oren') OR (Id = 309 AND FirstName = 'Brooklynn' AND LastName = 'Crist') OR (Id = 317 AND FirstName = 'Cade' AND LastName = 'Cass') OR (Id = 345 AND FirstName = 'DeAnte' AND LastName = 'Taylor') OR (Id = 359 AND FirstName = 'Brett' AND LastName = 'Gucker') OR (Id = 395 AND FirstName = 'Andrew' AND LastName = 'Vasel') OR (Id = 414 AND FirstName = 'Maria' AND LastName = 'Porras') OR (Id = 424 AND FirstName = 'Reese' AND LastName = 'Barnett') OR (Id = 456 AND FirstName = 'Brooklyn' AND LastName = 'Cavey') OR (Id = 464 AND FirstName = 'Sara' AND LastName = 'Naples') OR (Id = 484 AND FirstName = 'Brianna' AND LastName = 'Gallagher') OR (Id = 505 AND FirstName = 'Frank' AND LastName = 'Stepp') OR (Id = 509 AND FirstName = 'Makaela' AND LastName = 'Salyer') OR (Id = 525 AND FirstName = 'Jake' AND LastName = 'Kemp') OR (Id = 532 AND FirstName = 'Nick' AND LastName = 'Ziegler') OR (Id = 541 AND FirstName = 'Cory' AND LastName = 'Starman') OR (Id = 546 AND FirstName = 'Nate' AND LastName = 'Clingan') OR (Id = 558 AND FirstName = 'Asa' AND LastName = 'Gynn') OR (Id = 572 AND FirstName = 'Blake' AND LastName = 'Wilson') OR (Id = 581 AND FirstName = 'Alyssa' AND LastName = 'Stoner') OR (Id = 582 AND FirstName = 'Ashley' AND LastName = 'Kizzen') OR (Id = 588 AND FirstName = 'Nick' AND LastName = 'Bailey') OR (Id = 603 AND FirstName = 'Jacob' AND LastName = 'Nau') OR (Id = 614 AND FirstName = 'Nyah' AND LastName = 'Hansen') OR (Id = 631 AND FirstName = 'John' AND LastName = 'Manzuk') OR (Id = 635 AND FirstName = 'Logan' AND LastName = 'Stewart') OR (Id = 642 AND FirstName = 'Tristan' AND LastName = 'Vaughan') OR (Id = 658 AND FirstName = 'Julia' AND LastName = 'Armogida') OR (Id = 668 AND FirstName = 'Maddie' AND LastName = 'Boltz') OR (Id = 670 AND FirstName = 'Emily' AND LastName = 'Romano') OR (Id = 681 AND FirstName = 'Maddie' AND LastName = 'Marshall') OR (Id = 688 AND FirstName = 'Lisa' AND LastName = 'Wangler') OR (Id = 689 AND FirstName = 'Max' AND LastName = 'Gucker') OR (Id = 691 AND FirstName = 'Patrick' AND LastName = 'McConnell') OR (Id = 704 AND FirstName = 'Kyle' AND LastName = 'Sirl') OR (Id = 705 AND FirstName = 'Sara' AND LastName = 'Ayesteran') OR (Id = 713 AND FirstName = 'Cameron' AND LastName = 'ORourke') OR (Id = 731 AND FirstName = 'Emma' AND LastName = 'Nagel') OR (Id = 739 AND FirstName = 'Juliet' AND LastName = 'Pintos') OR (Id = 833 AND FirstName = 'Nate' AND LastName = 'Brown') OR (Id = 834 AND FirstName = 'Ryan' AND LastName = 'Curtis') OR (Id = 838 AND FirstName = 'Ben' AND LastName = 'Daniels') OR (Id = 844 AND FirstName = 'Brycen' AND LastName = 'Holliday')) <> 59 THROW 50010, 'Athlete Ids do not match this database', 1;
IF (SELECT COUNT(*) FROM Meets WHERE (Name = '2019 Oberlin College High School Meet #3' AND Date = '2019-02-02') OR (Name = '2022 Division I District Track Meet' AND Date = '2022-05-20') OR (Name = '2008 Wolverine Invitational' AND Date = '2008-04-04') OR (Name = '2018 New London Relays' AND Date = '2018-04-07') OR (Name = '2019 New London Relays' AND Date = '2019-04-06') OR (Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16') OR (Name = '2019 Triway Invitational' AND Date = '2019-04-20')) <> 7 THROW 50011, 'Meet Ids do not match this database', 1;
IF (SELECT COUNT(*) FROM Events WHERE (Id = 1 AND Name = 'Long Jump' AND Gender = 1) OR (Id = 2 AND Name = 'Shot Put' AND Gender = 2) OR (Id = 3 AND Name = 'Pole Vault' AND Gender = 1) OR (Id = 10 AND Name = 'Seated Shot Put' AND Gender = 2) OR (Id = 11 AND Name = 'High Jump' AND Gender = 2) OR (Id = 13 AND Name = 'Discus' AND Gender = 1) OR (Id = 14 AND Name = 'High Jump' AND Gender = 1) OR (Id = 17 AND Name = 'Pole Vault' AND Gender = 2) OR (Id = 22 AND Name = 'Shot Put' AND Gender = 1) OR (Id = 24 AND Name = 'Discus' AND Gender = 2) OR (Id = 25 AND Name = 'Long Jump' AND Gender = 2) OR (Id = 37 AND Name = '400M' AND Gender = 1) OR (Id = 47 AND Name = 'Seated 100M' AND Gender = 2) OR (Id = 51 AND Name = 'Seated 400M' AND Gender = 2) OR (Id = 67 AND Name = 'Seated 800M' AND Gender = 2) OR (Id = 99 AND Name = '4x100M Relay' AND Gender = 1) OR (Id = 100 AND Name = '800M Sprint Medley' AND Gender = 1) OR (Id = 101 AND Name = '4K Distance Medley' AND Gender = 1) OR (Id = 103 AND Name = '4K Distance Medley' AND Gender = 2) OR (Id = 108 AND Name = '4x400M Relay' AND Gender = 1) OR (Id = 113 AND Name = '800M Sprint Medley' AND Gender = 2) OR (Id = 114 AND Name = '4x200M Relay' AND Gender = 1) OR (Id = 115 AND Name = '4x400M Relay' AND Gender = 2) OR (Id = 116 AND Name = '4x400M Relay' AND Gender = 1)) <> 24 THROW 50012, 'Event Ids do not match this database', 1;
-- 2008 Wolverine Invitational: 400M Cameron ORourke 61.09
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2008 Wolverine Invitational' AND Date = '2008-04-04') AND EventId = 37 AND AthleteId = 713)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (713, (SELECT Id FROM Meets WHERE Name = '2008 Wolverine Invitational' AND Date = '2008-04-04'), 37, NULL, 61.09, 0, 0, 0, 0);
-- 2008 Wolverine Invitational: 4x200M Relay Kyle Sirl, Zane Hemming, Cameron ORourke, Jimmy Leahy 94.06
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2008 Wolverine Invitational' AND Date = '2008-04-04') AND EventId = 114 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2008 Wolverine Invitational' AND Date = '2008-04-04'), 114, NULL, 94.06, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 704);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 287);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 713);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 146);
END
-- 2018 New London Relays: Long Jump Brianna Gallagher 162.25
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 25 AND AthleteId = 484)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (484, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 25, 162.25, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Long Jump Emily Romano 153.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 25 AND AthleteId = 670)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (670, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 25, 153.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Long Jump Maddie Boltz 147.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 25 AND AthleteId = 668)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (668, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 25, 147.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Shot Put Lisa Wangler 404.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 2 AND AthleteId = 688)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (688, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 2, 404.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Shot Put Logan Stewart 374.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 2 AND AthleteId = 635)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (635, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 2, 374.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Shot Put Meagan Scheck 338.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 2 AND AthleteId = 213)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (213, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 2, 338.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Discus Lisa Wangler 1228.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 24 AND AthleteId = 688)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (688, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 24, 1228.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Discus Logan Stewart 1034.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 24 AND AthleteId = 635)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (635, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 24, 1034.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Discus Meagan Scheck 915.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 24 AND AthleteId = 213)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (213, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 24, 915.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: High Jump Nick Bailey 63.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 14 AND AthleteId = 588)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (588, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 14, 63.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: High Jump Ryan Curtis 60.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 14 AND AthleteId = 834)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (834, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 14, 60.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: High Jump Adam Oren 60.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 14 AND AthleteId = 293)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (293, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 14, 60.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Long Jump John Manzuk 227.5
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 1 AND AthleteId = 631)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (631, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 1, 227.5, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Long Jump Eli Haynes 222.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 1 AND AthleteId = 75)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (75, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 1, 222.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Long Jump Jacob Nau 211.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 1 AND AthleteId = 603)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (603, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 1, 211.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Shot Put Nate Clingan 594.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 22 AND AthleteId = 546)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (546, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 22, 594.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Shot Put Jake Kemp 468.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 22 AND AthleteId = 525)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (525, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 22, 468.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Shot Put Tommy Sword 448.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 22 AND AthleteId = 88)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (88, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 22, 448.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Discus Nate Clingan 1597.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 13 AND AthleteId = 546)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (546, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 13, 1597.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Discus Jake Kemp 1209.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 13 AND AthleteId = 525)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (525, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 13, 1209.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: Discus Cory Starman 1032.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 13 AND AthleteId = 541)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (541, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 13, 1032.0, NULL, 0, 0, 0, 0);
-- 2018 New London Relays: 800M Sprint Medley Nyah Hansen, Emily Romano, Sara Ayesteran, Juliet Pintos 137.7
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 113 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 113, NULL, 137.7, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 614);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 670);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 705);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 739);
END
-- 2018 New London Relays: 4K Distance Medley Tommie Kurtz, Brianna Gallagher, Sara Naples, Jenna Oliver 859.8
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 103 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 103, NULL, 859.8, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 484);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 464);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 87);
END
-- 2018 New London Relays: 800M Sprint Medley John Manzuk, Eli Haynes, Zack Munchick, Blake Wilson 103.1
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 100 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 100, NULL, 103.1, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 631);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 75);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 171);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 572);
END
-- 2018 New London Relays: 4K Distance Medley Tristan Vaughan, Brycen Holliday, Nick Bailey, Max Gucker 705.9
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07') AND EventId = 101 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2018 New London Relays' AND Date = '2018-04-07'), 101, NULL, 705.9, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 642);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 844);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 588);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 689);
END
-- 2019 New London Relays: Discus Meagan Scheck 1134.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 24 AND AthleteId = 213)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (213, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 24, 1134.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Discus Megan Van Meter 884.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 24 AND AthleteId = (SELECT Id FROM Athletes WHERE FirstName = 'Megan' AND LastName = 'Van Meter' AND Gender = 2))
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Megan' AND LastName = 'Van Meter' AND Gender = 2), (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 24, 884.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Discus Maddie Marshall 814.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 24 AND AthleteId = 681)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (681, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 24, 814.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Discus John Manzuk 1518.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 13 AND AthleteId = 631)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (631, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 13, 1518.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Discus Jake Kemp 1412.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 13 AND AthleteId = 525)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (525, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 13, 1412.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Discus Jordan Smith 1274.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 13 AND AthleteId = (SELECT Id FROM Athletes WHERE FirstName = 'Jordan' AND LastName = 'Smith' AND Gender = 1))
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Jordan' AND LastName = 'Smith' AND Gender = 1), (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 13, 1274.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Shot Put Meagan Scheck 358.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 2 AND AthleteId = 213)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (213, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 2, 358.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Shot Put Megan Van Meter 288.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 2 AND AthleteId = (SELECT Id FROM Athletes WHERE FirstName = 'Megan' AND LastName = 'Van Meter' AND Gender = 2))
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Megan' AND LastName = 'Van Meter' AND Gender = 2), (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 2, 288.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Shot Put Jake Kemp 477.5
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 22 AND AthleteId = 525)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (525, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 22, 477.5, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Shot Put Ethan Sterling 450.5
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 22 AND AthleteId = 215)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (215, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 22, 450.5, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Shot Put Jordan Smith 412.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 22 AND AthleteId = (SELECT Id FROM Athletes WHERE FirstName = 'Jordan' AND LastName = 'Smith' AND Gender = 1))
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Jordan' AND LastName = 'Smith' AND Gender = 1), (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 22, 412.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Long Jump Maddie Boltz 185.5
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 25 AND AthleteId = 668)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (668, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 25, 185.5, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Long Jump Dana Thur 167.5
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 25 AND AthleteId = 230)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (230, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 25, 167.5, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Long Jump Ashley Kizzen 164.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 25 AND AthleteId = 582)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (582, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 25, 164.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Long Jump Michael Williams 230.5
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 1 AND AthleteId = 37)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (37, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 1, 230.5, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Long Jump Jacob Nau 214.5
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 1 AND AthleteId = 603)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (603, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 1, 214.5, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Long Jump John Manzuk 201.25
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 1 AND AthleteId = 631)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (631, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 1, 201.25, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: High Jump Ashley Sigmon 57.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 11 AND AthleteId = 194)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (194, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 11, 57.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: High Jump DeAnte Taylor 60.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 14 AND AthleteId = 345)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (345, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 14, 60.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Pole Vault Maddie Marshall 72.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 17 AND AthleteId = 681)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (681, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 17, 72.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Pole Vault Ben Daniels 120.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 3 AND AthleteId = 838)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (838, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 3, 120.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: Pole Vault Nate Brown 108.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 3 AND AthleteId = 833)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (833, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 3, 108.0, NULL, 0, 0, 0, 0);
-- 2019 New London Relays: 4x100M Relay Michael Williams, Frank Stepp, Jacob Nau, Blake Wilson 46.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 99 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 99, NULL, 46.0, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 37);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 505);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 603);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 572);
END
-- 2019 New London Relays: 4K Distance Medley Emma Nagel, Liv Wells, Maria Porras, Makaela Salyer 892.3
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 103 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 103, NULL, 892.3, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 731);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 121);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 414);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 509);
END
-- 2019 New London Relays: 4K Distance Medley Thomas Lormeau, Cade Cass, Andrew Vasel, Max Gucker 715.9
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 101 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 101, NULL, 715.9, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 164);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 317);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 395);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 689);
END
-- 2019 New London Relays: 800M Sprint Medley Michael Williams, John Manzuk, Jacob Nau, Blake Wilson 103.7
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06') AND EventId = 100 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2019 New London Relays' AND Date = '2019-04-06'), 100, NULL, 103.7, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 37);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 631);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 603);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 572);
END
-- 2019 Ravenna-Streetsboro Double Dual: 4x100M Relay John Manzuk, Michael Williams, Austin Ebaugh, Blake Wilson 45.98
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16') AND EventId = 99 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16'), 99, NULL, 45.98, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 631);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 37);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 198);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 572);
END
-- 2019 Ravenna-Streetsboro Double Dual: 4x200M Relay Austin Ebaugh, Frank Stepp, Cade Cass, Blake Wilson 97.7
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16') AND EventId = 114 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16'), 114, NULL, 97.7, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 198);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 505);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 317);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 572);
END
-- 2019 Ravenna-Streetsboro Double Dual: 4x400M Relay Nick Ziegler, Andrew Hibler, Max Gucker, Asa Gynn 235.5
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16') AND EventId = 108 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2019 Ravenna-Streetsboro Double Dual' AND Date = '2019-04-16'), 108, NULL, 235.5, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 532);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 34);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 689);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 558);
END
-- 2019 Triway Invitational: Seated 100M Brooklynn Crist 20.56
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 Triway Invitational' AND Date = '2019-04-20') AND EventId = 47 AND AthleteId = 309)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (309, (SELECT Id FROM Meets WHERE Name = '2019 Triway Invitational' AND Date = '2019-04-20'), 47, NULL, 20.56, 0, 0, 0, 0);
-- 2019 Triway Invitational: Seated 100M Bridgette Donahue 58.75
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 Triway Invitational' AND Date = '2019-04-20') AND EventId = 47 AND AthleteId = (SELECT Id FROM Athletes WHERE FirstName = 'Bridgette' AND LastName = 'Donahue' AND Gender = 2))
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Bridgette' AND LastName = 'Donahue' AND Gender = 2), (SELECT Id FROM Meets WHERE Name = '2019 Triway Invitational' AND Date = '2019-04-20'), 47, NULL, 58.75, 0, 0, 0, 0);
-- 2019 Triway Invitational: Seated 400M Brooklynn Crist 88.24
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 Triway Invitational' AND Date = '2019-04-20') AND EventId = 51 AND AthleteId = 309)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (309, (SELECT Id FROM Meets WHERE Name = '2019 Triway Invitational' AND Date = '2019-04-20'), 51, NULL, 88.24, 0, 0, 0, 0);
-- 2019 Triway Invitational: Seated 800M Brooklynn Crist 195.58
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 Triway Invitational' AND Date = '2019-04-20') AND EventId = 67 AND AthleteId = 309)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (309, (SELECT Id FROM Meets WHERE Name = '2019 Triway Invitational' AND Date = '2019-04-20'), 67, NULL, 195.58, 0, 0, 0, 0);
-- 2019 Triway Invitational: Seated Shot Put Brooklynn Crist 180.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 Triway Invitational' AND Date = '2019-04-20') AND EventId = 10 AND AthleteId = 309)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (309, (SELECT Id FROM Meets WHERE Name = '2019 Triway Invitational' AND Date = '2019-04-20'), 10, NULL, 180.0, 0, 0, 0, 0);
-- 2019 Triway Invitational: Seated Shot Put Bridgette Donahue 87.5
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 Triway Invitational' AND Date = '2019-04-20') AND EventId = 10 AND AthleteId = (SELECT Id FROM Athletes WHERE FirstName = 'Bridgette' AND LastName = 'Donahue' AND Gender = 2))
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Bridgette' AND LastName = 'Donahue' AND Gender = 2), (SELECT Id FROM Meets WHERE Name = '2019 Triway Invitational' AND Date = '2019-04-20'), 10, NULL, 87.5, 0, 0, 0, 0);
-- 2019 Oberlin College High School Meet #3: 4x400M Relay Max Gucker, Brett Gucker, Andrew Vasel, Patrick McConnell 280.94
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2019 Oberlin College High School Meet #3' AND Date = '2019-02-02') AND EventId = 116 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, (SELECT Id FROM Meets WHERE Name = '2019 Oberlin College High School Meet #3' AND Date = '2019-02-02'), 116, NULL, 280.94, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 689);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 359);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 395);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 691);
END
-- 2022 Division I District Track Meet: 4x400M Relay Reese Barnett, Julia Armogida, Brooklyn Cavey, Alyssa Stoner 258.77
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2022 Division I District Track Meet' AND Date = '2022-05-20') AND EventId = 115 AND AthleteId IS NULL)
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
