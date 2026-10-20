-- 2019 OHSAA Seated State shot put attempt series from the official results (https://www.baumspage.com/ohsaa/tf/2019/Results4.htm, event 431) (BRAIN.md [C94]).
-- Brooklynn Crist 15-2.5, 16-4.75, 16-1, 16-5.75 (best 16' 5.75" = 197.75); Bridgette Donahue 7-4.75, 7-2.5, 7-0.5, 7-3.25 (best 7' 4.75" = 88.75).
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @crist INT = (SELECT p.Id FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = N'Brooklynn' AND a.LastName = N'Crist' AND m.Name = N'2019 OHSAA Seated Division State Track and Field Championships' AND e.Name = N'Seated Shot Put' AND e.Gender = 2 AND ABS(p.DistanceInches - 197.75) < 0.001);
DECLARE @donahue INT = (SELECT p.Id FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Meets m ON m.Id = p.MeetId JOIN Events e ON e.Id = p.EventId
 WHERE a.FirstName = N'Bridgette' AND a.LastName = N'Donahue' AND m.Name = N'2019 OHSAA Seated Division State Track and Field Championships' AND e.Name = N'Seated Shot Put' AND e.Gender = 2 AND ABS(p.DistanceInches - 88.75) < 0.001);
IF @crist IS NULL OR @donahue IS NULL THROW 50096, 'A seated state shot put result was not found', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId IN (@crist, @donahue);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@crist, 1, 182.5, 0, 0), (@crist, 2, 196.75, 0, 0), (@crist, 3, 193.0, 0, 0), (@crist, 4, 197.75, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@donahue, 1, 88.75, 0, 0), (@donahue, 2, 86.5, 0, 0), (@donahue, 3, 84.5, 0, 0), (@donahue, 4, 87.25, 0, 0);
COMMIT TRANSACTION;
