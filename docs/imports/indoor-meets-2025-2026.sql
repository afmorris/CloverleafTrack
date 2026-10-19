-- Single-day meets from baumspage Hy-Tek results (BRAIN.md [C72]); Cloverleaf rows only. Each meet is skipped if a meet with that name and date exists.
-- Rules as in ne-district-2005-2010.sql. Athlete Ids are asserted before anything is inserted.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT, @p INT;
IF (SELECT COUNT(*) FROM Athletes WHERE (Id = 811 AND FirstName = 'Payten' AND LastName = 'Shetterly')) <> 1 THROW 50010, 'Athlete Ids do not match this database', 1;

-- ===== 2025 Kent State HS Meet #5 =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2025 Kent State HS Meet #5' AND Date = '2025-02-23')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2025 Kent State HS Meet #5', '2025-02-23', 28, 2, 0, 27, 3, 1, 3, 'https://www.athletic.net/TrackAndField/meet/587896');
SET @m = SCOPE_IDENTITY();
-- Girls Pole Vault: Payten Shetterly  final 106.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (811, @m, 19, 106.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2025 Akron Rust Buster HS Open =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2025 Akron Rust Buster HS Open' AND Date = '2025-12-13')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2025 Akron Rust Buster HS Open', '2025-12-13', 1, 2, 0, 47, 3, 1, 3, 'https://www.athletic.net/TrackAndField/meet/626478');
SET @m = SCOPE_IDENTITY();
-- Girls Pole Vault: Payten Shetterly  final 126.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (811, @m, 19, 126.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2026 Akron Rubber City HS Open =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2026 Akron Rubber City HS Open' AND Date = '2026-01-17')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2026 Akron Rubber City HS Open', '2026-01-17', 1, 2, 0, 47, 3, 1, 3, 'https://www.athletic.net/TrackAndField/meet/631604');
SET @m = SCOPE_IDENTITY();
-- Girls Pole Vault: Payten Shetterly  final 120.0
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (811, @m, 19, 120.0, NULL, 0, 0, 0, 0);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2026 Kent State HS Meet #1 =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2026 Kent State HS Meet #1' AND Date = '2026-01-24')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2026 Kent State HS Meet #1', '2026-01-24', 28, 2, 0, 47, 3, 1, 3, 'https://www.athletic.net/TrackAndField/meet/632988');
SET @m = SCOPE_IDENTITY();
-- Girls Pole Vault: Payten Shetterly  NH
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (811, @m, 19, NULL, NULL, 0, 0, 0, 2);
SET @p = SCOPE_IDENTITY();
END

-- ===== 2026 Ashland University HS Light Giver Open =====
IF NOT EXISTS (SELECT 1 FROM Meets WHERE Name = '2026 Ashland University HS Light Giver Open' AND Date = '2026-02-08')
BEGIN
INSERT INTO Meets (Name, Date, LocationId, Environment, HandTimed, SeasonId, EntryStatus, MeetLevel, MeetType, OfficialResultsUrl) VALUES ('2026 Ashland University HS Light Giver Open', '2026-02-08', 3, 2, 0, 47, 3, 1, 3, 'https://www.athletic.net/TrackAndField/meet/636245');
SET @m = SCOPE_IDENTITY();
-- Girls Pole Vault: Payten Shetterly  NH
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus) VALUES (811, @m, 19, NULL, NULL, 0, 0, 0, 2);
SET @p = SCOPE_IDENTITY();
END

EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
