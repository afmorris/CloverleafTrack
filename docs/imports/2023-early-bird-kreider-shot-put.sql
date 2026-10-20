-- 2023 Cloverleaf Early Bird Relays girls shot put: MileSplit Live mixed the Kreider sisters' attempts into one row (BRAIN.md [C92]).
-- Owner's correction: Jayce threw 22-9, 21-6.5, 20-11.5 (best 22' 9"); Karoline threw 24-2, 25-11, 27-2.5 (best 27' 2.5").
-- Old marks: Jayce 24' 2" (290) with four attempts 24-2, 22-9, 21-6.5, 20-11.5; Karoline 25' 11" (311) with no series.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT = (SELECT Id FROM Meets WHERE Name = N'2023 Cloverleaf Early Bird Relays' AND CAST(Date AS date) = '2023-03-25');
DECLARE @e INT = (SELECT Id FROM Events WHERE Name = N'Shot Put' AND Gender = 2 AND Environment = 1);
DECLARE @jayce INT = (SELECT p.Id FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId WHERE p.MeetId = @m AND p.EventId = @e AND a.FirstName = N'Jayce' AND a.LastName = N'Kreider');
DECLARE @karoline INT = (SELECT p.Id FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId WHERE p.MeetId = @m AND p.EventId = @e AND a.FirstName = N'Karoline' AND a.LastName = N'Kreider');
IF @m IS NULL OR @e IS NULL OR @jayce IS NULL OR @karoline IS NULL THROW 50090, 'Meet, event or a Kreider result not found', 1;
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id = @jayce AND ABS(DistanceInches - 290) < 0.001) OR NOT EXISTS (SELECT 1 FROM Performances WHERE Id = @karoline AND ABS(DistanceInches - 311) < 0.001)
    THROW 50091, 'A Kreider mark is not the expected old value', 1;
UPDATE Performances SET DistanceInches = 273 WHERE Id = @jayce;        -- 24' 2" -> 22' 9"
UPDATE Performances SET DistanceInches = 326.5 WHERE Id = @karoline;   -- 25' 11" -> 27' 2.5"
DELETE FROM PerformanceAttempts WHERE PerformanceId IN (@jayce, @karoline);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@jayce, 1, 273.0, 0, 0), (@jayce, 2, 258.5, 0, 0), (@jayce, 3, 251.5, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@karoline, 1, 290.0, 0, 0), (@karoline, 2, 311.0, 0, 0), (@karoline, 3, 326.5, 0, 0);
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
