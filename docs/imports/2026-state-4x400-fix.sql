-- 2026 OHSAA D3 State, girls 4x400 (performance 16487, meet 300).
-- The DB had 3:55.05 (235.05) - that is Clear Fork's 2nd-place time. Cloverleaf finished 3rd in 3:55.68
-- (splits 53.46 + 1:05.16 + 59.66 + 57.40 = 3:55.68) after a 4:02.05 prelim (57.46 + 1:04.01 + 1:00.38 + 1:00.20).
-- Source: athletic.net meet 656920, Womens Division 3 4x400 Relay. Cloverleaf's time is still the school record
-- (previous best 4:01.96, 2016). Requires docs/migration_performance_rounds.sql (already run).
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=16487 AND MeetId=300 AND ABS(TimeSeconds-235.05)<0.006) THROW 50004, 'Performance 16487 is not the expected 3:55.05 row', 1;

UPDATE Performances SET TimeSeconds = 235.68 WHERE Id = 16487;
DELETE FROM PerformanceRounds WHERE PerformanceId = 16487;
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (16487, 1, 242.05);
INSERT INTO PerformanceRounds (PerformanceId, Round, TimeSeconds) VALUES (16487, 3, 235.68);

COMMIT TRANSACTION;

-- Leaderboards/percentiles/flags must be recomputed after a time change:
EXEC sp_RebuildLeaderboards;
