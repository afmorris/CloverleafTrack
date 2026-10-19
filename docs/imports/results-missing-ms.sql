-- Cloverleaf results present on MileSplit but missing from meets the DB already has (BRAIN.md [C74]). Inserts only; never touches an existing row.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @p INT;
IF (SELECT COUNT(*) FROM Athletes WHERE (Id = 6 AND FirstName = 'Zach' AND LastName = 'Greer') OR (Id = 8 AND FirstName = 'Reed' AND LastName = 'Kilgore') OR (Id = 12 AND FirstName = 'Tommie' AND LastName = 'Kurtz') OR (Id = 17 AND FirstName = 'Cameron' AND LastName = 'McNeal') OR (Id = 26 AND FirstName = 'Katie' AND LastName = 'Wershing') OR (Id = 31 AND FirstName = 'Estevan' AND LastName = 'Ortega') OR (Id = 34 AND FirstName = 'Andrew' AND LastName = 'Hibler') OR (Id = 64 AND FirstName = 'Cole' AND LastName = 'Yagersz') OR (Id = 88 AND FirstName = 'Tommy' AND LastName = 'Sword') OR (Id = 122 AND FirstName = 'John' AND LastName = 'Artwell') OR (Id = 133 AND FirstName = 'Dillon' AND LastName = 'West') OR (Id = 148 AND FirstName = 'Katelyn' AND LastName = 'Malena') OR (Id = 164 AND FirstName = 'Thomas' AND LastName = 'Lormeau') OR (Id = 295 AND FirstName = 'Paige' AND LastName = 'Munchick') OR (Id = 309 AND FirstName = 'Brooklynn' AND LastName = 'Crist') OR (Id = 311 AND FirstName = 'Alex' AND LastName = 'Derhammer') OR (Id = 359 AND FirstName = 'Brett' AND LastName = 'Gucker') OR (Id = 383 AND FirstName = 'Isaac' AND LastName = 'Baltic') OR (Id = 414 AND FirstName = 'Maria' AND LastName = 'Porras') OR (Id = 418 AND FirstName = 'Lexi' AND LastName = 'Licursi') OR (Id = 462 AND FirstName = 'Dylan' AND LastName = 'Wittman') OR (Id = 464 AND FirstName = 'Sara' AND LastName = 'Naples') OR (Id = 511 AND FirstName = 'Matt' AND LastName = 'Grissom') OR (Id = 532 AND FirstName = 'Nick' AND LastName = 'Ziegler') OR (Id = 564 AND FirstName = 'Hayden' AND LastName = 'Jarvis') OR (Id = 603 AND FirstName = 'Jacob' AND LastName = 'Nau') OR (Id = 642 AND FirstName = 'Tristan' AND LastName = 'Vaughan') OR (Id = 653 AND FirstName = 'Landon' AND LastName = 'Southam') OR (Id = 663 AND FirstName = 'Alex' AND LastName = 'Randall') OR (Id = 689 AND FirstName = 'Max' AND LastName = 'Gucker') OR (Id = 711 AND FirstName = 'Ray' AND LastName = 'Farnham') OR (Id = 723 AND FirstName = 'Ella' AND LastName = 'Loth') OR (Id = 731 AND FirstName = 'Emma' AND LastName = 'Nagel') OR (Id = 745 AND FirstName = 'Ben' AND LastName = 'Kott') OR (Id = 787 AND FirstName = 'Will' AND LastName = 'Rogalski') OR (Id = 832 AND FirstName = 'Royce' AND LastName = 'Zirkle') OR (Id = 833 AND FirstName = 'Nate' AND LastName = 'Brown')) <> 37 THROW 50010, 'Athlete Ids do not match this database', 1;
IF (SELECT COUNT(*) FROM Meets WHERE (Id = 15 AND Name = '2018 Oberlin College Grand Prix Series #3') OR (Id = 19 AND Name = '2019 Oberlin College High School Meet #4') OR (Id = 20 AND Name = '2020 Oberlin College High School Meet #1') OR (Id = 21 AND Name = '2019 Nikki Reynolds Invitational') OR (Id = 26 AND Name = '2019 Division II District Track Meet') OR (Id = 32 AND Name = '2017 Oberlin College Grand Prix Series #3') OR (Id = 70 AND Name = '2023 Ravenna-Streetsboro Double Dual') OR (Id = 78 AND Name = '2019 Cowen Redbird Invitational') OR (Id = 106 AND Name = '2023 Nikki Reynolds Invitational') OR (Id = 120 AND Name = '2019 Oberlin College High School Meet #3') OR (Id = 122 AND Name = '2022 Smithie Frosh-Soph Invitational') OR (Id = 147 AND Name = '2025 Bay Rocket Invitational') OR (Id = 183 AND Name = '2019 PTC Track Championships') OR (Id = 225 AND Name = '2022 Fairless Twilight Invitational') OR (Id = 269 AND Name = '2025 Division II District Track Meet') OR (Id = 276 AND Name = '2025 Metro Athletic Conference Championships')) <> 16 THROW 50011, 'Meet Ids do not match this database', 1;
IF (SELECT COUNT(*) FROM Events WHERE (Id = 2 AND Name = 'Shot Put' AND Gender = 2) OR (Id = 3 AND Name = 'Pole Vault' AND Gender = 1) OR (Id = 13 AND Name = 'Discus' AND Gender = 1) OR (Id = 22 AND Name = 'Shot Put' AND Gender = 1) OR (Id = 37 AND Name = '400M' AND Gender = 1) OR (Id = 46 AND Name = '400M' AND Gender = 2) OR (Id = 49 AND Name = '100M' AND Gender = 2) OR (Id = 50 AND Name = '800M' AND Gender = 2) OR (Id = 55 AND Name = '110M Hurdles' AND Gender = 1) OR (Id = 62 AND Name = '3200M' AND Gender = 1) OR (Id = 65 AND Name = '200M' AND Gender = 1) OR (Id = 104 AND Name = '4x800M Relay' AND Gender = 1) OR (Id = 107 AND Name = '4x400M Relay' AND Gender = 2) OR (Id = 112 AND Name = '4x800M Relay' AND Gender = 2) OR (Id = 116 AND Name = '4x400M Relay' AND Gender = 1)) <> 15 THROW 50012, 'Event Ids do not match this database', 1;
-- 2020 Oberlin College High School Meet #1: 4x800M Relay Max Gucker, Alex Derhammer, Brett Gucker, Andrew Hibler 543.7
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 20 AND EventId = 104 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, 20, 104, NULL, 543.7, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 689);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 311);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 359);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 34);
END
-- 2019 Oberlin College High School Meet #3: 4x400M Relay Tommie Kurtz, Sara Naples, Maria Porras, Paige Munchick 287.19
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 120 AND EventId = 107 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, 120, 107, NULL, 287.19, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 464);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 414);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 295);
END
-- 2019 Oberlin College High School Meet #3: 4x400M Relay Thomas Lormeau, Jacob Nau, Andrew Hibler, Alex Derhammer 249.11
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 120 AND EventId = 116 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, 120, 116, NULL, 249.11, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 164);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 603);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 34);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 311);
END
-- 2019 Oberlin College High School Meet #3: 4x800M Relay Thomas Lormeau, Max Gucker, Cole Yagersz, Alex Derhammer 548.86
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 120 AND EventId = 104 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, 120, 104, NULL, 548.86, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 164);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 689);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 64);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 311);
END
-- 2019 Oberlin College High School Meet #4: 4x800M Relay Thomas Lormeau, Max Gucker, Andrew Hibler, Alex Derhammer 535.77
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 19 AND EventId = 104 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, 19, 104, NULL, 535.77, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 164);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 689);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 34);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 311);
END
-- 2018 Oberlin College Grand Prix Series #3: 4x800M Relay Tommie Kurtz, Sara Naples, Katelyn Malena, Lexi Licursi 717.62
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 15 AND EventId = 112 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, 15, 112, NULL, 717.62, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 464);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 148);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 418);
END
-- 2017 Oberlin College Grand Prix Series #3: 4x800M Relay Tommie Kurtz, Emma Nagel, Sara Naples, Ella Loth 721.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 32 AND EventId = 112 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, 32, 112, NULL, 721.0, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 12);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 731);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 464);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 723);
END
-- 2017 Oberlin College Grand Prix Series #3: 4x800M Relay Tristan Vaughan, Dillon West, Estevan Ortega, Matt Grissom 584.93
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 32 AND EventId = 104 AND AthleteId IS NULL)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (NULL, 32, 104, NULL, 584.93, 0, 0, 0, 0);
    SET @p = SCOPE_IDENTITY();
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 642);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 133);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 31);
    INSERT INTO PerformanceAthletes (PerformanceId, AthleteId) VALUES (@p, 511);
END
-- 2025 Bay Rocket Invitational: 400M Katie Wershing 65.8
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 147 AND EventId = 46 AND AthleteId = 26)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (26, 147, 46, NULL, 65.8, 0, 0, 0, 0);
-- 2025 Metro Athletic Conference Championships: 400M Reed Kilgore 53.48
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 276 AND EventId = 37 AND AthleteId = 8)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (8, 276, 37, NULL, 53.48, 0, 0, 0, 0);
-- 2025 Division II District Track Meet: Shot Put Royce Zirkle 461.5
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 269 AND EventId = 22 AND AthleteId = 832)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (832, 269, 22, 461.5, NULL, 0, 0, 0, 0);
-- 2023 Ravenna-Streetsboro Double Dual: 200M Alex Randall 27.48
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 70 AND EventId = 65 AND AthleteId = 663)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (663, 70, 65, NULL, 27.48, 0, 0, 0, 0);
-- 2023 Ravenna-Streetsboro Double Dual: 3200M Landon Southam 779.13
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 70 AND EventId = 62 AND AthleteId = 653)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (653, 70, 62, NULL, 779.13, 0, 0, 0, 0);
-- 2023 Ravenna-Streetsboro Double Dual: Shot Put John Artwell 333.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 70 AND EventId = 22 AND AthleteId = 122)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (122, 70, 22, 333.0, NULL, 0, 0, 0, 0);
-- 2023 Nikki Reynolds Invitational: 110M Hurdles Isaac Baltic 18.67
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 106 AND EventId = 55 AND AthleteId = 383)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (383, 106, 55, NULL, 18.67, 0, 0, 0, 0);
-- 2022 Fairless Twilight Invitational: Shot Put Will Rogalski 493.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 225 AND EventId = 22 AND AthleteId = 787)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (787, 225, 22, 493.0, NULL, 0, 0, 0, 0);
-- 2022 Fairless Twilight Invitational: Pole Vault Ray Farnham 96.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 225 AND EventId = 3 AND AthleteId = 711)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (711, 225, 3, 96.0, NULL, 0, 0, 0, 0);
-- 2022 Smithie Frosh-Soph Invitational: 200M Hayden Jarvis 27.94
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 122 AND EventId = 65 AND AthleteId = 564)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (564, 122, 65, NULL, 27.94, 0, 0, 0, 0);
-- 2022 Smithie Frosh-Soph Invitational: Shot Put Dylan Wittman 362.5
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 122 AND EventId = 22 AND AthleteId = 462)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (462, 122, 22, 362.5, NULL, 0, 0, 0, 0);
-- 2019 Cowen Redbird Invitational: 100M Brooklynn Crist 21.18
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 78 AND EventId = 49 AND AthleteId = 309)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (309, 78, 49, NULL, 21.18, 0, 0, 0, 0);
-- 2019 Cowen Redbird Invitational: 400M Brooklynn Crist 80.52
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 78 AND EventId = 46 AND AthleteId = 309)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (309, 78, 46, NULL, 80.52, 0, 0, 0, 0);
-- 2019 Cowen Redbird Invitational: 800M Brooklynn Crist 176.71
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 78 AND EventId = 50 AND AthleteId = 309)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (309, 78, 50, NULL, 176.71, 0, 0, 0, 0);
-- 2019 Cowen Redbird Invitational: Shot Put Brooklynn Crist 180.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 78 AND EventId = 2 AND AthleteId = 309)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (309, 78, 2, 180.0, NULL, 0, 0, 0, 0);
-- 2019 Nikki Reynolds Invitational: Shot Put Tommy Sword 443.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 21 AND EventId = 22 AND AthleteId = 88)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (88, 21, 22, 443.0, NULL, 0, 0, 0, 0);
-- 2019 Nikki Reynolds Invitational: Shot Put Cameron McNeal 411.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 21 AND EventId = 22 AND AthleteId = 17)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (17, 21, 22, 411.0, NULL, 0, 0, 0, 0);
-- 2019 Nikki Reynolds Invitational: Discus Ben Kott 1328.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 21 AND EventId = 13 AND AthleteId = 745)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (745, 21, 13, 1328.0, NULL, 0, 0, 0, 0);
-- 2019 Nikki Reynolds Invitational: Discus Zach Greer 1180.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 21 AND EventId = 13 AND AthleteId = 6)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (6, 21, 13, 1180.0, NULL, 0, 0, 0, 0);
-- 2019 Nikki Reynolds Invitational: Pole Vault Nate Brown 126.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 21 AND EventId = 3 AND AthleteId = 833)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (833, 21, 3, 126.0, NULL, 0, 0, 0, 0);
-- 2019 Nikki Reynolds Invitational: Pole Vault Nick Ziegler 114.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 21 AND EventId = 3 AND AthleteId = 532)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (532, 21, 3, 114.0, NULL, 0, 0, 0, 0);
-- 2019 PTC Track Championships: Shot Put Megan Van Meter 309.5
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 183 AND EventId = 2 AND AthleteId = (SELECT Id FROM Athletes WHERE FirstName = 'Megan' AND LastName = 'Van Meter' AND Gender = 2))
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Megan' AND LastName = 'Van Meter' AND Gender = 2), 183, 2, 309.5, NULL, 0, 0, 0, 0);
-- 2019 PTC Track Championships: Shot Put Brooklynn Crist 157.0
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 183 AND EventId = 2 AND AthleteId = 309)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (309, 183, 2, 157.0, NULL, 0, 0, 0, 0);
-- 2019 Division II District Track Meet: Shot Put Megan Van Meter 314.75
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 26 AND EventId = 2 AND AthleteId = (SELECT Id FROM Athletes WHERE FirstName = 'Megan' AND LastName = 'Van Meter' AND Gender = 2))
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES ((SELECT Id FROM Athletes WHERE FirstName = 'Megan' AND LastName = 'Van Meter' AND Gender = 2), 26, 2, 314.75, NULL, 0, 0, 0, 0);
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
