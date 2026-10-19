-- Official (MileSplit) marks replace slightly different DB marks (BRAIN.md [C76]); each UPDATE is guarded by the old value and skipped if the row has rounds.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF (SELECT COUNT(*) FROM Athletes WHERE (Id = 12 AND FirstName = 'Tommie' AND LastName = 'Kurtz') OR (Id = 26 AND FirstName = 'Katie' AND LastName = 'Wershing') OR (Id = 87 AND FirstName = 'Jenna' AND LastName = 'Oliver') OR (Id = 108 AND FirstName = 'Timmy' AND LastName = 'Shores') OR (Id = 148 AND FirstName = 'Katelyn' AND LastName = 'Malena') OR (Id = 171 AND FirstName = 'Zack' AND LastName = 'Munchick') OR (Id = 252 AND FirstName = 'Zana' AND LastName = 'Ross') OR (Id = 362 AND FirstName = 'Karoline' AND LastName = 'Kreider') OR (Id = 365 AND FirstName = 'Isaac' AND LastName = 'Smith') OR (Id = 410 AND FirstName = 'Allison' AND LastName = 'Yagersz') OR (Id = 422 AND FirstName = 'Nathan' AND LastName = 'Naples') OR (Id = 505 AND FirstName = 'Frank' AND LastName = 'Stepp') OR (Id = 546 AND FirstName = 'Nate' AND LastName = 'Clingan') OR (Id = 581 AND FirstName = 'Alyssa' AND LastName = 'Stoner') OR (Id = 668 AND FirstName = 'Maddie' AND LastName = 'Boltz') OR (Id = 717 AND FirstName = 'Luke' AND LastName = 'Yuravak') OR (Id = 718 AND FirstName = 'Brooke' AND LastName = 'Gucker') OR (Id = 794 AND FirstName = 'Oakley' AND LastName = 'Kemp') OR (Id = 811 AND FirstName = 'Payten' AND LastName = 'Shetterly') OR (Id = 834 AND FirstName = 'Ryan' AND LastName = 'Curtis')) <> 20 THROW 50010, 'Athlete Ids do not match this database', 1;
-- 2025-05-17 2025 Metro Athletic Conference Championships | 400M | Isaac Smith: 53.19 -> 52.79
UPDATE Performances SET TimeSeconds = 52.79 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2025 Metro Athletic Conference Championships' AND CAST(Date AS date) = '2025-05-17') AND EventId = 37 AND AthleteId = 365 AND ABS(TimeSeconds - 53.19) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2025-05-17 2025 Metro Athletic Conference Championships | 400M | Katie Wershing: 63.38 -> 61.32
UPDATE Performances SET TimeSeconds = 61.32 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2025 Metro Athletic Conference Championships' AND CAST(Date AS date) = '2025-05-17') AND EventId = 46 AND AthleteId = 26 AND ABS(TimeSeconds - 63.38) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2024-05-03 2024 Fairless Twilight Invitational | Discus | Timmy Shores: 1477.00 -> 1476.00
UPDATE Performances SET DistanceInches = 1476.0 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2024 Fairless Twilight Invitational' AND CAST(Date AS date) = '2024-05-03') AND EventId = 13 AND AthleteId = 108 AND ABS(DistanceInches - 1477.0) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2024-05-03 2024 Fairless Twilight Invitational | Shot Put | Karoline Kreider: 391.00 -> 386.50
UPDATE Performances SET DistanceInches = 386.5 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2024 Fairless Twilight Invitational' AND CAST(Date AS date) = '2024-05-03') AND EventId = 2 AND AthleteId = 362 AND ABS(DistanceInches - 391.0) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2023-04-04 2023 Ravenna-Streetsboro Double Dual | 200M | Oakley Kemp: 25.19 -> 25.10
UPDATE Performances SET TimeSeconds = 25.1 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2023 Ravenna-Streetsboro Double Dual' AND CAST(Date AS date) = '2023-04-04') AND EventId = 65 AND AthleteId = 794 AND ABS(TimeSeconds - 25.19) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2023-05-05 2023 Fairless Twilight Invitational | Pole Vault | Payten Shetterly: 78.00 -> 72.00
UPDATE Performances SET DistanceInches = 72.0 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2023 Fairless Twilight Invitational' AND CAST(Date AS date) = '2023-05-05') AND EventId = 17 AND AthleteId = 811 AND ABS(DistanceInches - 78.0) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2022-05-09 2022 Smithie Frosh-Soph Invitational | 800M | Brooke Gucker: 162.65 -> 162.56
UPDATE Performances SET TimeSeconds = 162.56 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2022 Smithie Frosh-Soph Invitational' AND CAST(Date AS date) = '2022-05-09') AND EventId = 50 AND AthleteId = 718 AND ABS(TimeSeconds - 162.65) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2022-05-09 2022 Smithie Frosh-Soph Invitational | Shot Put | Timmy Shores: 464.00 -> 459.00
UPDATE Performances SET DistanceInches = 459.0 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2022 Smithie Frosh-Soph Invitational' AND CAST(Date AS date) = '2022-05-09') AND EventId = 22 AND AthleteId = 108 AND ABS(DistanceInches - 464.0) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2021-05-01 2021 Nikki Reynolds Invitational | 800M | Alyssa Stoner: 150.69 -> 150.09
UPDATE Performances SET TimeSeconds = 150.09 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2021 Nikki Reynolds Invitational' AND CAST(Date AS date) = '2021-05-01') AND EventId = 50 AND AthleteId = 581 AND ABS(TimeSeconds - 150.69) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2021-05-01 2021 Nikki Reynolds Invitational | Shot Put | Luke Yuravak: 493.25 -> 497.25
UPDATE Performances SET DistanceInches = 497.25 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2021 Nikki Reynolds Invitational' AND CAST(Date AS date) = '2021-05-01') AND EventId = 22 AND AthleteId = 717 AND ABS(DistanceInches - 493.25) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2021-05-21 2021 Division I District Track Meet	 | 1600M | Nathan Naples: 305.77 -> 305.05
UPDATE Performances SET TimeSeconds = 305.05 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2021 Division I District Track Meet	' AND CAST(Date AS date) = '2021-05-21') AND EventId = 56 AND AthleteId = 422 AND ABS(TimeSeconds - 305.77) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2021-05-21 2021 Division I District Track Meet	 | 800M | Allison Yagersz: 155.84 -> 155.73
UPDATE Performances SET TimeSeconds = 155.73 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2021 Division I District Track Meet	' AND CAST(Date AS date) = '2021-05-21') AND EventId = 50 AND AthleteId = 410 AND ABS(TimeSeconds - 155.84) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2018-05-09 2018 PTC Track Championships | 100M | Zack Munchick: 11.52 -> 11.73
UPDATE Performances SET TimeSeconds = 11.73 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 PTC Track Championships' AND CAST(Date AS date) = '2018-05-09') AND EventId = 45 AND AthleteId = 171 AND ABS(TimeSeconds - 11.52) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2018-05-09 2018 PTC Track Championships | 400M | Tommie Kurtz: 60.85 -> 62.54
UPDATE Performances SET TimeSeconds = 62.54 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 PTC Track Championships' AND CAST(Date AS date) = '2018-05-09') AND EventId = 46 AND AthleteId = 12 AND ABS(TimeSeconds - 60.85) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2018-05-09 2018 PTC Track Championships | 400M | Maddie Boltz: 65.72 -> 65.93
UPDATE Performances SET TimeSeconds = 65.93 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 PTC Track Championships' AND CAST(Date AS date) = '2018-05-09') AND EventId = 46 AND AthleteId = 668 AND ABS(TimeSeconds - 65.72) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2018-05-09 2018 PTC Track Championships | 400M | Frank Stepp: 52.12 -> 52.42
UPDATE Performances SET TimeSeconds = 52.42 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2018 PTC Track Championships' AND CAST(Date AS date) = '2018-05-09') AND EventId = 37 AND AthleteId = 505 AND ABS(TimeSeconds - 52.12) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2017-04-15 2017 Mansfield Mehock Relays | 1600M | Katelyn Malena: 379.31 -> 379.25
UPDATE Performances SET TimeSeconds = 379.25 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2017 Mansfield Mehock Relays' AND CAST(Date AS date) = '2017-04-15') AND EventId = 41 AND AthleteId = 148 AND ABS(TimeSeconds - 379.31) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2017-04-28 2017 Orange Relays | Discus | Nate Clingan: 1363.00 -> 1363.50
UPDATE Performances SET DistanceInches = 1363.5 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2017 Orange Relays' AND CAST(Date AS date) = '2017-04-28') AND EventId = 13 AND AthleteId = 546 AND ABS(DistanceInches - 1363.0) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2017-05-10 2017 PTC Track Championships | 400M | Jenna Oliver: 64.39 -> 64.48
UPDATE Performances SET TimeSeconds = 64.48 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2017 PTC Track Championships' AND CAST(Date AS date) = '2017-05-10') AND EventId = 46 AND AthleteId = 87 AND ABS(TimeSeconds - 64.39) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2017-05-10 2017 PTC Track Championships | 400M | Ryan Curtis: 55.11 -> 55.92
UPDATE Performances SET TimeSeconds = 55.92 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2017 PTC Track Championships' AND CAST(Date AS date) = '2017-05-10') AND EventId = 37 AND AthleteId = 834 AND ABS(TimeSeconds - 55.11) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2017-05-10 2017 PTC Track Championships | 300M Hurdles | Zana Ross: 48.70 -> 49.04
UPDATE Performances SET TimeSeconds = 49.04 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2017 PTC Track Championships' AND CAST(Date AS date) = '2017-05-10') AND EventId = 29 AND AthleteId = 252 AND ABS(TimeSeconds - 48.7) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
-- 2017-05-10 2017 PTC Track Championships | 200M | Jenna Oliver: 28.19 -> 28.60
UPDATE Performances SET TimeSeconds = 28.6 WHERE MeetId = (SELECT Id FROM Meets WHERE Name = '2017 PTC Track Championships' AND CAST(Date AS date) = '2017-05-10') AND EventId = 39 AND AthleteId = 87 AND ABS(TimeSeconds - 28.19) < 0.001
    AND NOT EXISTS (SELECT 1 FROM PerformanceRounds r WHERE r.PerformanceId = Performances.Id);
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
