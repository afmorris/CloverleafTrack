-- No-mark results (NM / NH / DNF) from MileSplit raw results (see BRAIN.md [C67], [C68]). Rows are the ones whose athlete has NO
-- Performance for that event at that meet; a stored mark is never touched. NH/ND lines with a seed mark still have no result.
-- Requires docs/migration_performance_result_status.sql. Athlete / event ids are asserted per block.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Brooklyn CAVEY: status 2 in Pole Vault at meet 11 (ms:510383)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 456 AND LastName = 'Cavey') THROW 50001, 'Athlete 456 is not CAVEY', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 11 AND EventId = 17 AND AthleteId = 456)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (456, 11, 17, NULL, NULL, 0, 0, 0, 2);

-- Payten SHETTERLY: status 2 in Pole Vault at meet 11 (ms:510383)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 811 AND LastName = 'Shetterly') THROW 50001, 'Athlete 811 is not SHETTERLY', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 11 AND EventId = 17 AND AthleteId = 811)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (811, 11, 17, NULL, NULL, 0, 0, 0, 2);

-- Cydnei Miller: status 2 in High Jump at meet 21 (ms:357412)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 182 AND LastName = 'Miller') THROW 50001, 'Athlete 182 is not Miller', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 11 AND Name = 'High Jump' AND Gender = 2) THROW 50002, 'Event 11 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 21 AND EventId = 11 AND AthleteId = 182)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (182, 21, 11, NULL, NULL, 0, 0, 0, 2);

-- Daniel Sommers: status 2 in High Jump at meet 21 (ms:357412)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 712 AND LastName = 'Sommers') THROW 50001, 'Athlete 712 is not Sommers', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 21 AND EventId = 14 AND AthleteId = 712)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (712, 21, 14, NULL, NULL, 0, 0, 0, 2);

-- Ben Kott: status 2 in High Jump at meet 21 (ms:357412)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 745 AND LastName = 'Kott') THROW 50001, 'Athlete 745 is not Kott', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 21 AND EventId = 14 AND AthleteId = 745)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (745, 21, 14, NULL, NULL, 0, 0, 0, 2);

-- Corina Cass: status 2 in Pole Vault at meet 21 (ms:357412)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 266 AND LastName = 'Cass') THROW 50001, 'Athlete 266 is not Cass', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 21 AND EventId = 17 AND AthleteId = 266)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (266, 21, 17, NULL, NULL, 0, 0, 0, 2);

-- Cassidy Smith: status 2 in Pole Vault at meet 21 (ms:357412)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 302 AND LastName = 'Smith') THROW 50001, 'Athlete 302 is not Smith', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 21 AND EventId = 17 AND AthleteId = 302)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (302, 21, 17, NULL, NULL, 0, 0, 0, 2);

-- Nathan Tomes: status 2 in High Jump at meet 25 (ms:597587)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 245 AND LastName = 'Tomes') THROW 50001, 'Athlete 245 is not Tomes', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 25 AND EventId = 14 AND AthleteId = 245)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (245, 25, 14, NULL, NULL, 0, 0, 0, 2);

-- Payten Shetterly: status 2 in Pole Vault at meet 25 (ms:597587)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 811 AND LastName = 'Shetterly') THROW 50001, 'Athlete 811 is not Shetterly', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 25 AND EventId = 17 AND AthleteId = 811)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (811, 25, 17, NULL, NULL, 0, 0, 0, 2);

-- Cydnei Miller: status 2 in High Jump at meet 26 (ms:357920)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 182 AND LastName = 'Miller') THROW 50001, 'Athlete 182 is not Miller', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 11 AND Name = 'High Jump' AND Gender = 2) THROW 50002, 'Event 11 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 26 AND EventId = 11 AND AthleteId = 182)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (182, 26, 11, NULL, NULL, 0, 0, 0, 2);

-- Jacob Nau: status 2 in High Jump at meet 26 (ms:357920)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 603 AND LastName = 'Nau') THROW 50001, 'Athlete 603 is not Nau', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 26 AND EventId = 14 AND AthleteId = 603)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (603, 26, 14, NULL, NULL, 0, 0, 0, 2);

-- Ben Kott: status 2 in High Jump at meet 26 (ms:357920)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 745 AND LastName = 'Kott') THROW 50001, 'Athlete 745 is not Kott', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 26 AND EventId = 14 AND AthleteId = 745)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (745, 26, 14, NULL, NULL, 0, 0, 0, 2);

-- Corina Cass: status 2 in Pole Vault at meet 26 (ms:357920)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 266 AND LastName = 'Cass') THROW 50001, 'Athlete 266 is not Cass', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 26 AND EventId = 17 AND AthleteId = 266)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (266, 26, 17, NULL, NULL, 0, 0, 0, 2);

-- Matt Butt: status 2 in Pole Vault at meet 49 (ms:273003)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 205 AND LastName = 'Butt') THROW 50001, 'Athlete 205 is not Butt', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 3 AND Name = 'Pole Vault' AND Gender = 1) THROW 50002, 'Event 3 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 49 AND EventId = 3 AND AthleteId = 205)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (205, 49, 3, NULL, NULL, 0, 0, 0, 2);

-- Ben Daniels: status 2 in Pole Vault at meet 49 (ms:273003)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 838 AND LastName = 'Daniels') THROW 50001, 'Athlete 838 is not Daniels', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 3 AND Name = 'Pole Vault' AND Gender = 1) THROW 50002, 'Event 3 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 49 AND EventId = 3 AND AthleteId = 838)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (838, 49, 3, NULL, NULL, 0, 0, 0, 2);

-- Maddie Grey: status 2 in Pole Vault at meet 49 (ms:273003)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 274 AND LastName = 'Grey') THROW 50001, 'Athlete 274 is not Grey', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 49 AND EventId = 17 AND AthleteId = 274)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (274, 49, 17, NULL, NULL, 0, 0, 0, 2);

-- Nolan McNeal: status 2 in High Jump at meet 54 (ms:426765)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 328 AND LastName = 'Mcneal') THROW 50001, 'Athlete 328 is not McNeal', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 54 AND EventId = 14 AND AthleteId = 328)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (328, 54, 14, NULL, NULL, 0, 0, 0, 2);

-- Laurell Brown: status 2 in Pole Vault at meet 54 (ms:426765)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 411 AND LastName = 'Brown') THROW 50001, 'Athlete 411 is not Brown', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 54 AND EventId = 17 AND AthleteId = 411)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (411, 54, 17, NULL, NULL, 0, 0, 0, 2);

-- Brooklyn Cavey: status 2 in Pole Vault at meet 57 (ms:542137)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 456 AND LastName = 'Cavey') THROW 50001, 'Athlete 456 is not Cavey', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 57 AND EventId = 17 AND AthleteId = 456)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (456, 57, 17, NULL, NULL, 0, 0, 0, 2);

-- Nathan Tomes: status 2 in High Jump at meet 64 (an:554436)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 245 AND LastName = 'Tomes') THROW 50001, 'Athlete 245 is not Tomes', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 64 AND EventId = 14 AND AthleteId = 245)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (245, 64, 14, NULL, NULL, 0, 0, 0, 2);

-- David Hunter: status 2 in High Jump at meet 64 (an:554436)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 488 AND LastName = 'Hunter') THROW 50001, 'Athlete 488 is not Hunter', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 64 AND EventId = 14 AND AthleteId = 488)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (488, 64, 14, NULL, NULL, 0, 0, 0, 2);

-- Payten Shetterly: status 2 in Pole Vault at meet 64 (an:554436)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 811 AND LastName = 'Shetterly') THROW 50001, 'Athlete 811 is not Shetterly', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 64 AND EventId = 17 AND AthleteId = 811)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (811, 64, 17, NULL, NULL, 0, 0, 0, 2);

-- Lincoln Baltic: status 2 in Pole Vault at meet 72 (ms:642009)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 14 AND LastName = 'Baltic') THROW 50001, 'Athlete 14 is not Baltic', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 3 AND Name = 'Pole Vault' AND Gender = 1) THROW 50002, 'Event 3 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 72 AND EventId = 3 AND AthleteId = 14)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (14, 72, 3, NULL, NULL, 0, 0, 0, 2);

-- Reed Kilgore: status 2 in High Jump at meet 72 (ms:642009)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 8 AND LastName = 'Kilgore') THROW 50001, 'Athlete 8 is not Kilgore', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 72 AND EventId = 14 AND AthleteId = 8)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (8, 72, 14, NULL, NULL, 0, 0, 0, 2);

-- Taylor Thur: status 2 in Pole Vault at meet 72 (ms:642009)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 667 AND LastName = 'Thur') THROW 50001, 'Athlete 667 is not Thur', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 72 AND EventId = 17 AND AthleteId = 667)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (667, 72, 17, NULL, NULL, 0, 0, 0, 2);

-- Payten Shetterly: status 2 in Pole Vault at meet 72 (ms:642009)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 811 AND LastName = 'Shetterly') THROW 50001, 'Athlete 811 is not Shetterly', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 72 AND EventId = 17 AND AthleteId = 811)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (811, 72, 17, NULL, NULL, 0, 0, 0, 2);

-- DeAnte Taylor: status 2 in High Jump at meet 78 (ms:354269)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 345 AND LastName = 'Taylor') THROW 50001, 'Athlete 345 is not Taylor', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 78 AND EventId = 14 AND AthleteId = 345)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (345, 78, 14, NULL, NULL, 0, 0, 0, 2);

-- Ben Kott: status 2 in High Jump at meet 78 (ms:354269)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 745 AND LastName = 'Kott') THROW 50001, 'Athlete 745 is not Kott', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 78 AND EventId = 14 AND AthleteId = 745)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (745, 78, 14, NULL, NULL, 0, 0, 0, 2);

-- Payten Shetterly: status 2 in Pole Vault at meet 106 (an:511658)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 811 AND LastName = 'Shetterly') THROW 50001, 'Athlete 811 is not Shetterly', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 106 AND EventId = 17 AND AthleteId = 811)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (811, 106, 17, NULL, NULL, 0, 0, 0, 2);

-- Dylan Perman: status 2 in Pole Vault at meet 122 (an:476184)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 724 AND LastName = 'Perman') THROW 50001, 'Athlete 724 is not Perman', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 3 AND Name = 'Pole Vault' AND Gender = 1) THROW 50002, 'Event 3 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 122 AND EventId = 3 AND AthleteId = 724)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (724, 122, 3, NULL, NULL, 0, 0, 0, 2);

-- Nick Bailey: status 2 in High Jump at meet 125 (ms:314786)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 588 AND LastName = 'Bailey') THROW 50001, 'Athlete 588 is not Bailey', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 125 AND EventId = 14 AND AthleteId = 588)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (588, 125, 14, NULL, NULL, 0, 0, 0, 2);

-- Maddie Marshall: status 2 in Pole Vault at meet 125 (ms:314786)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 681 AND LastName = 'Marshall') THROW 50001, 'Athlete 681 is not Marshall', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 125 AND EventId = 17 AND AthleteId = 681)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (681, 125, 17, NULL, NULL, 0, 0, 0, 2);

-- Adam Oren: status 2 in High Jump at meet 160 (ms:321959)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 293 AND LastName = 'Oren') THROW 50001, 'Athlete 293 is not Oren', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 160 AND EventId = 14 AND AthleteId = 293)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (293, 160, 14, NULL, NULL, 0, 0, 0, 2);

-- Ryan Curtis: status 2 in High Jump at meet 160 (ms:321959)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 834 AND LastName = 'Curtis') THROW 50001, 'Athlete 834 is not Curtis', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 160 AND EventId = 14 AND AthleteId = 834)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (834, 160, 14, NULL, NULL, 0, 0, 0, 2);

-- Lexi Licursi: status 2 in High Jump at meet 161 (ms:279585)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 418 AND LastName = 'Licursi') THROW 50001, 'Athlete 418 is not Licursi', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 11 AND Name = 'High Jump' AND Gender = 2) THROW 50002, 'Event 11 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 161 AND EventId = 11 AND AthleteId = 418)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (418, 161, 11, NULL, NULL, 0, 0, 0, 2);

-- Dylan Craig: status 2 in High Jump at meet 161 (ms:279585)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 778 AND LastName = 'Craig') THROW 50001, 'Athlete 778 is not Craig', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 161 AND EventId = 14 AND AthleteId = 778)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (778, 161, 14, NULL, NULL, 0, 0, 0, 2);

-- Maddie Grey: status 2 in Pole Vault at meet 161 (ms:279585)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 274 AND LastName = 'Grey') THROW 50001, 'Athlete 274 is not Grey', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 161 AND EventId = 17 AND AthleteId = 274)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (274, 161, 17, NULL, NULL, 0, 0, 0, 2);

-- Colton Funai: status 2 in High Jump at meet 168 (an:465658)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 777 AND LastName = 'Funai') THROW 50001, 'Athlete 777 is not Funai', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 168 AND EventId = 14 AND AthleteId = 777)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (777, 168, 14, NULL, NULL, 0, 0, 0, 2);

-- Ben Daniels: status 2 in Pole Vault at meet 174 (ms:282278)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 838 AND LastName = 'Daniels') THROW 50001, 'Athlete 838 is not Daniels', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 3 AND Name = 'Pole Vault' AND Gender = 1) THROW 50002, 'Event 3 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 174 AND EventId = 3 AND AthleteId = 838)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (838, 174, 3, NULL, NULL, 0, 0, 0, 2);

-- Brianna Gallagher: status 2 in High Jump at meet 174 (ms:282278)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 484 AND LastName = 'Gallagher') THROW 50001, 'Athlete 484 is not Gallagher', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 11 AND Name = 'High Jump' AND Gender = 2) THROW 50002, 'Event 11 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 174 AND EventId = 11 AND AthleteId = 484)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (484, 174, 11, NULL, NULL, 0, 0, 0, 2);

-- Maddie Grey: status 2 in Pole Vault at meet 174 (ms:282278)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 274 AND LastName = 'Grey') THROW 50001, 'Athlete 274 is not Grey', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 174 AND EventId = 17 AND AthleteId = 274)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (274, 174, 17, NULL, NULL, 0, 0, 0, 2);

-- Ben Kott: status 2 in High Jump at meet 183 (ms:357828)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 745 AND LastName = 'Kott') THROW 50001, 'Athlete 745 is not Kott', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 183 AND EventId = 14 AND AthleteId = 745)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (745, 183, 14, NULL, NULL, 0, 0, 0, 2);

-- Nolan McNeal: status 2 in High Jump at meet 185 (ms:429940)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 328 AND LastName = 'Mcneal') THROW 50001, 'Athlete 328 is not McNeal', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 185 AND EventId = 14 AND AthleteId = 328)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (328, 185, 14, NULL, NULL, 0, 0, 0, 2);

-- Lisa Wangler: status 2 in High Jump at meet 186 (ms:318138)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 688 AND LastName = 'Wangler') THROW 50001, 'Athlete 688 is not Wangler', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 11 AND Name = 'High Jump' AND Gender = 2) THROW 50002, 'Event 11 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 186 AND EventId = 11 AND AthleteId = 688)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (688, 186, 11, NULL, NULL, 0, 0, 0, 2);

-- Caleb Cass: status 2 in High Jump at meet 186 (ms:318138)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 132 AND LastName = 'Cass') THROW 50001, 'Athlete 132 is not Cass', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 186 AND EventId = 14 AND AthleteId = 132)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (132, 186, 14, NULL, NULL, 0, 0, 0, 2);

-- Adam Oren: status 2 in High Jump at meet 186 (ms:318138)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 293 AND LastName = 'Oren') THROW 50001, 'Athlete 293 is not Oren', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 186 AND EventId = 14 AND AthleteId = 293)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (293, 186, 14, NULL, NULL, 0, 0, 0, 2);

-- Maria Porras: status 3 in 1600M at meet 186 (ms:318138)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 414 AND LastName = 'Porras') THROW 50001, 'Athlete 414 is not Porras', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 41 AND Name = '1600M' AND Gender = 2) THROW 50002, 'Event 41 is not 1600M', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 186 AND EventId = 41 AND AthleteId = 414)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (414, 186, 41, NULL, NULL, 0, 0, 0, 3);

-- Ben Kott: status 1 in Discus at meet 194 (ms:422637)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 745 AND LastName = 'Kott') THROW 50001, 'Athlete 745 is not Kott', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 13 AND Name = 'Discus' AND Gender = 1) THROW 50002, 'Event 13 is not Discus', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 194 AND EventId = 13 AND AthleteId = 745)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (745, 194, 13, NULL, NULL, 0, 0, 0, 1);

-- Payten Shetterly: status 2 in High Jump at meet 233 (ms:613912)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 811 AND LastName = 'Shetterly') THROW 50001, 'Athlete 811 is not Shetterly', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 11 AND Name = 'High Jump' AND Gender = 2) THROW 50002, 'Event 11 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 233 AND EventId = 11 AND AthleteId = 811)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (811, 233, 11, NULL, NULL, 0, 0, 0, 2);

-- Will Jerome: status 2 in High Jump at meet 255 (an:474578)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 29 AND LastName = 'Jerome') THROW 50001, 'Athlete 29 is not Jerome', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 255 AND EventId = 14 AND AthleteId = 29)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (29, 255, 14, NULL, NULL, 0, 0, 0, 2);

-- Laurell Brown: status 2 in Pole Vault at meet 255 (an:474578)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 411 AND LastName = 'Brown') THROW 50001, 'Athlete 411 is not Brown', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 255 AND EventId = 17 AND AthleteId = 411)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (411, 255, 17, NULL, NULL, 0, 0, 0, 2);

-- Nathan Tomes: status 2 in High Jump at meet 273 (ms:686929)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 245 AND LastName = 'Tomes') THROW 50001, 'Athlete 245 is not Tomes', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 273 AND EventId = 14 AND AthleteId = 245)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (245, 273, 14, NULL, NULL, 0, 0, 0, 2);

-- Taylor Thur: status 2 in Pole Vault at meet 273 (ms:686929)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 667 AND LastName = 'Thur') THROW 50001, 'Athlete 667 is not Thur', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 273 AND EventId = 17 AND AthleteId = 667)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (667, 273, 17, NULL, NULL, 0, 0, 0, 2);

-- Yousef Fekieh: status 1 in High Jump at meet 284 (ms:738840)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 861 AND LastName = 'Fekieh') THROW 50001, 'Athlete 861 is not Fekieh', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 284 AND EventId = 14 AND AthleteId = 861)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (861, 284, 14, NULL, NULL, 0, 0, 0, 1);

-- Payten Shetterly: status 2 in Pole Vault at meet 285 (an:631352)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 811 AND LastName = 'Shetterly') THROW 50001, 'Athlete 811 is not Shetterly', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 285 AND EventId = 17 AND AthleteId = 811)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (811, 285, 17, NULL, NULL, 0, 0, 0, 2);

-- Clayton Sewalish: status 2 in High Jump at meet 287 (ms:749055)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 629 AND LastName = 'Sewalish') THROW 50001, 'Athlete 629 is not Sewalish', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 287 AND EventId = 14 AND AthleteId = 629)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (629, 287, 14, NULL, NULL, 0, 0, 0, 2);

-- Yousef Fekieh: status 2 in High Jump at meet 287 (ms:749055)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 861 AND LastName = 'Fekieh') THROW 50001, 'Athlete 861 is not Fekieh', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 287 AND EventId = 14 AND AthleteId = 861)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (861, 287, 14, NULL, NULL, 0, 0, 0, 2);

-- Delaney Ishler: status 2 in Pole Vault at meet 287 (ms:749055)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 7 AND LastName = 'Ishler') THROW 50001, 'Athlete 7 is not Ishler', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 287 AND EventId = 17 AND AthleteId = 7)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (7, 287, 17, NULL, NULL, 0, 0, 0, 2);

-- Ciara Kremer: status 2 in Pole Vault at meet 287 (ms:749055)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 864 AND LastName = 'Kremer') THROW 50001, 'Athlete 864 is not Kremer', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 287 AND EventId = 17 AND AthleteId = 864)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (864, 287, 17, NULL, NULL, 0, 0, 0, 2);

-- Taylor Thur: status 2 in Pole Vault at meet 291 (ms:754116)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 667 AND LastName = 'Thur') THROW 50001, 'Athlete 667 is not Thur', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 291 AND EventId = 17 AND AthleteId = 667)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (667, 291, 17, NULL, NULL, 0, 0, 0, 2);

-- Payten Shetterly: status 2 in Pole Vault at meet 293 (an:652638)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 811 AND LastName = 'Shetterly') THROW 50001, 'Athlete 811 is not Shetterly', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 293 AND EventId = 17 AND AthleteId = 811)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (811, 293, 17, NULL, NULL, 0, 0, 0, 2);

-- Adriana Akers: status 4 in 100M at meet 295 (ms:760322)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 848 AND LastName = 'Akers') THROW 50001, 'Athlete 848 is not Akers', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 49 AND Name = '100M' AND Gender = 2) THROW 50002, 'Event 49 is not 100M', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 295 AND EventId = 49 AND AthleteId = 848)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (848, 295, 49, NULL, NULL, 0, 0, 0, 4);

-- Addison Reho: status 1 in Discus at meet 297 (ms:763691)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 529 AND LastName = 'Reho') THROW 50001, 'Athlete 529 is not Reho', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 24 AND Name = 'Discus' AND Gender = 2) THROW 50002, 'Event 24 is not Discus', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 297 AND EventId = 24 AND AthleteId = 529)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (529, 297, 24, NULL, NULL, 0, 0, 0, 1);

EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
