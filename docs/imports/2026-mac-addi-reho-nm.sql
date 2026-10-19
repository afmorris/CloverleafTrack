-- 2026 Metro Athletic Conference Championships (DB meet 297), girls discus: Addi Reho fouled all three throws
-- (athletic.net meet 637822, ?tab=field-series: place ND, Foul / Foul / Foul). No mark => a ResultStatus 1 (NM)
-- Performance with a 3-attempt all-foul series. Requires docs/migration_performance_result_status.sql.
-- Idempotent: skips if she already has any discus row at the meet. Excluded from rankings by sp_RebuildLeaderboards.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM Athletes WHERE Id = 529 AND FirstName = 'Addi' AND LastName = 'Reho') THROW 50001, 'Athlete 529 is not Addi Reho', 1;
IF NOT EXISTS (SELECT 1 FROM Events WHERE Id = 24 AND Name = 'Discus' AND Gender = 2 AND Environment = 1) THROW 50002, 'Event 24 is not outdoor girls Discus', 1;

IF NOT EXISTS (SELECT 1 FROM Performances WHERE MeetId = 297 AND EventId = 24 AND AthleteId = 529)
BEGIN
    INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
    VALUES (529, 297, 24, NULL, NULL, 0, 0, 0, 1);

    DECLARE @pid INT = SCOPE_IDENTITY();
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 1, NULL, 1, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 2, NULL, 1, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 3, NULL, 1, 0);
    PRINT 'Inserted NM for Addi Reho';
END
ELSE PRINT 'NOTE: Addi Reho already has a discus row at meet 297; skipped';

EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
