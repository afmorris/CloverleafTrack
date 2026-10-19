-- No-mark results (DQ / NH) for 2026 meets, from the athletic.net field-series / results pages (see BRAIN.md [C67], [C68]).
-- Each block asserts the athlete + event, and inserts a ResultStatus row ONLY if the athlete has no row for that event at
-- that meet (so a stored mark is never touched). NH rows carry no bar ladder: athletic.net shows only X X X, and the
-- starting height was not captured. Requires docs/migration_performance_result_status.sql.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Adriana Akers: DQ in 100M at meet 295 (athletic.net 653339 f/100m)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 848 AND FirstName = 'Adriana' AND LastName = 'Akers') THROW 50001, 'Athlete 848 is not Adriana Akers', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 49 AND Name = '100M' AND Gender = 2 AND Environment = 1) THROW 50002, 'Event 49 is not 100M', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 295 AND EventId = 49 AND AthleteId = 848)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (848, 295, 49, NULL, NULL, 0, 0, 0, 4);
ELSE PRINT 'NOTE: Adriana Akers already has a row for event 49 at meet 295; skipped';

-- Taylor Thur: NH in Pole Vault at meet 291 (athletic.net 651598 f/pv)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 667 AND FirstName = 'Taylor' AND LastName = 'Thur') THROW 50001, 'Athlete 667 is not Taylor Thur', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2 AND Environment = 1) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 291 AND EventId = 17 AND AthleteId = 667)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (667, 291, 17, NULL, NULL, 0, 0, 0, 2);
ELSE PRINT 'NOTE: Taylor Thur already has a row for event 17 at meet 291; skipped';

-- Yousef Fekieh: NH in High Jump at meet 287 (athletic.net 648714 m/hj)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 861 AND FirstName = 'Yousef' AND LastName = 'Fekieh') THROW 50001, 'Athlete 861 is not Yousef Fekieh', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1 AND Environment = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 287 AND EventId = 14 AND AthleteId = 861)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (861, 287, 14, NULL, NULL, 0, 0, 0, 2);
ELSE PRINT 'NOTE: Yousef Fekieh already has a row for event 14 at meet 287; skipped';

-- Clayton Sewalish: NH in High Jump at meet 287 (athletic.net 648714 m/hj)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 629 AND FirstName = 'Clayton' AND LastName = 'Sewalish') THROW 50001, 'Athlete 629 is not Clayton Sewalish', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 14 AND Name = 'High Jump' AND Gender = 1 AND Environment = 1) THROW 50002, 'Event 14 is not High Jump', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 287 AND EventId = 14 AND AthleteId = 629)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (629, 287, 14, NULL, NULL, 0, 0, 0, 2);
ELSE PRINT 'NOTE: Clayton Sewalish already has a row for event 14 at meet 287; skipped';

-- Delaney Ishler: NH in Pole Vault at meet 287 (athletic.net 648714 f/pv)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 7 AND FirstName = 'Delaney' AND LastName = 'Ishler') THROW 50001, 'Athlete 7 is not Delaney Ishler', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2 AND Environment = 1) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 287 AND EventId = 17 AND AthleteId = 7)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (7, 287, 17, NULL, NULL, 0, 0, 0, 2);
ELSE PRINT 'NOTE: Delaney Ishler already has a row for event 17 at meet 287; skipped';

-- Ciara Kremer: NH in Pole Vault at meet 287 (athletic.net 648714 f/pv)
IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 864 AND FirstName = 'Ciara' AND LastName = 'Kremer') THROW 50001, 'Athlete 864 is not Ciara Kremer', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 17 AND Name = 'Pole Vault' AND Gender = 2 AND Environment = 1) THROW 50002, 'Event 17 is not Pole Vault', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 287 AND EventId = 17 AND AthleteId = 864)
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (864, 287, 17, NULL, NULL, 0, 0, 0, 2);
ELSE PRINT 'NOTE: Ciara Kremer already has a row for event 17 at meet 287; skipped';

EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
