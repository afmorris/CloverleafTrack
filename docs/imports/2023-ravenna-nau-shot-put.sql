-- Dominic Nau, 2023 Ravenna-Streetsboro Double Dual shot put (BRAIN.md [C93]). MileSplit and Live say 38' 2", the owner keeps the stored 31' 0".
-- Owner's decision: use Live's first three attempts (26-4, 29-9, 30-5) and replace the 38-2 with 31-0 so the series ends at the stored mark.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @pid INT = (SELECT p.Id FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = N'Dominic' AND a.LastName = N'Nau' AND m.Name = N'2023 Ravenna-Streetsboro Double Dual' AND CAST(m.Date AS date) = '2023-04-04' AND e.Name = N'Shot Put' AND ABS(p.DistanceInches - 372) < 0.001);
IF @pid IS NULL THROW 50095, 'Dominic Nau 31-0 shot put result not found', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId = @pid;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 1, 316.0, 0, 0), (@pid, 2, 357.0, 0, 0), (@pid, 3, 365.0, 0, 0), (@pid, 4, 372.0, 0, 0);
COMMIT TRANSACTION;
