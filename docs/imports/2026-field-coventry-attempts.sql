-- 2026 Field-Coventry Double Dual (2026-04-28), from the scorer's hand-written sheets (scan_ggucker_2026-04-28).
-- Part 1: Macie Huegel's two marks corrected to the sheet (owner-approved): Shot Put 24' 2" -> 24' 6", Discus 65' 6" -> 65' 8".
-- Part 2: field-event attempt series (long jump 4 attempts; shot put/discus: each athlete's two throws, dashes = not taken) and
--         pole vault / high jump bar ladders. All display-only child rows; each block asserts the stored mark. BRAIN.md [C81].
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT = (SELECT Id FROM Meets WHERE Name = '2026 Field-Coventry Double Dual' AND CAST(Date AS date) = '2026-04-28');
IF @m IS NULL THROW 50050, 'Meet not found', 1;
UPDATE p SET DistanceInches = 294 FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Events e ON e.Id = p.EventId
 WHERE p.MeetId = @m AND a.FirstName = 'Macie' AND a.LastName = 'Huegel' AND e.Name = 'Shot Put' AND ABS(p.DistanceInches - 290) < 0.001;
UPDATE p SET DistanceInches = 788 FROM Performances p JOIN Athletes a ON a.Id = p.AthleteId JOIN Events e ON e.Id = p.EventId
 WHERE p.MeetId = @m AND a.FirstName = 'Macie' AND a.LastName = 'Huegel' AND e.Name = 'Discus' AND ABS(p.DistanceInches - 786) < 0.001;
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- ===== DB meet 292 =====

-- Elliot Jordan Long Jump (M): 19' 3", 19' 0.25", 18' 8.5", 17' 6"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15824 AND MeetId=292 AND ABS(DistanceInches-231.0)<0.006) THROW 50002, 'Mark mismatch for performance 15824', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15824;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15824, 1, 231.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15824, 2, 228.25, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15824, 3, 224.5, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15824, 4, 210.0, 0, 0);

-- Grady Pamer Long Jump (M): 19' 6.25", 19' 1.5", 19' 7", 19' 10.5"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15827 AND MeetId=292 AND ABS(DistanceInches-238.5)<0.006) THROW 50002, 'Mark mismatch for performance 15827', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15827;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15827, 1, 234.25, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15827, 2, 229.5, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15827, 3, 235.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15827, 4, 238.5, 0, 0);

-- Collin Saksa Long Jump (M): Foul, 15' 7", Foul, 16' 0"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15828 AND MeetId=292 AND ABS(DistanceInches-192.0)<0.006) THROW 50002, 'Mark mismatch for performance 15828', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15828;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15828, 1, NULL, 1, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15828, 2, 187.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15828, 3, NULL, 1, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15828, 4, 192.0, 0, 0);

-- Clayton Sewalish Long Jump (M): 18' 0.75", 17' 2", 17' 8", 17' 1.5"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15829 AND MeetId=292 AND ABS(DistanceInches-216.75)<0.006) THROW 50002, 'Mark mismatch for performance 15829', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15829;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15829, 1, 216.75, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15829, 2, 206.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15829, 3, 212.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15829, 4, 205.5, 0, 0);

-- Jackson Morris Long Jump (M): 15' 0.75", 15' 6.5", 15' 1.75", Foul
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15826 AND MeetId=292 AND ABS(DistanceInches-186.5)<0.006) THROW 50002, 'Mark mismatch for performance 15826', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15826;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15826, 1, 180.75, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15826, 2, 186.5, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15826, 3, 181.75, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15826, 4, NULL, 1, 0);

-- Weston Miller Long Jump (M): 12' 6.75", 11' 1", 11' 8", 13' 1"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15825 AND MeetId=292 AND ABS(DistanceInches-157.0)<0.006) THROW 50002, 'Mark mismatch for performance 15825', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15825;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15825, 1, 150.75, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15825, 2, 133.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15825, 3, 140.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15825, 4, 157.0, 0, 0);

-- Abigail Reinhardt Long Jump (F): Foul, 15' 6", 14' 9", 15' 6.75"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15830 AND MeetId=292 AND ABS(DistanceInches-186.75)<0.006) THROW 50002, 'Mark mismatch for performance 15830', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15830;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15830, 1, NULL, 1, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15830, 2, 186.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15830, 3, 177.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15830, 4, 186.75, 0, 0);

-- Keegan Wittman Long Jump (F): 13' 5", 13' 5", Foul, 14' 0.25"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15831 AND MeetId=292 AND ABS(DistanceInches-168.25)<0.006) THROW 50002, 'Mark mismatch for performance 15831', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15831;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15831, 1, 161.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15831, 2, 161.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15831, 3, NULL, 1, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15831, 4, 168.25, 0, 0);

-- Vienna Polen Shot Put (F): 16' 9", 17' 4"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15832 AND MeetId=292 AND ABS(DistanceInches-208.0)<0.006) THROW 50002, 'Mark mismatch for performance 15832', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15832;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15832, 1, 201.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15832, 2, 208.0, 0, 0);

-- Macie Huegel Shot Put (F): 22' 1", 24' 6"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15833 AND MeetId=292 AND ABS(DistanceInches-294.0)<0.006) THROW 50002, 'Mark mismatch for performance 15833', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15833;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15833, 1, 265.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15833, 2, 294.0, 0, 0);

-- Ciara Kremer Shot Put (F): 24' 1", 25' 6"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15834 AND MeetId=292 AND ABS(DistanceInches-306.0)<0.006) THROW 50002, 'Mark mismatch for performance 15834', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15834;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15834, 1, 289.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15834, 2, 306.0, 0, 0);

-- Anna Naftzger Shot Put (F): 24' 7", 24' 10"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15835 AND MeetId=292 AND ABS(DistanceInches-298.0)<0.006) THROW 50002, 'Mark mismatch for performance 15835', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15835;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15835, 1, 295.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15835, 2, 298.0, 0, 0);

-- Addi Reho Shot Put (F): 24' 11", 26' 9"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15836 AND MeetId=292 AND ABS(DistanceInches-321.0)<0.006) THROW 50002, 'Mark mismatch for performance 15836', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15836;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15836, 1, 299.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15836, 2, 321.0, 0, 0);

-- Berkeley Craddock Shot Put (F): 29' 9", 28' 7"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15837 AND MeetId=292 AND ABS(DistanceInches-357.0)<0.006) THROW 50002, 'Mark mismatch for performance 15837', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15837;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15837, 1, 357.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15837, 2, 343.0, 0, 0);

-- Amelia Ryncarz Shot Put (F): 31' 10", 30' 7"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15838 AND MeetId=292 AND ABS(DistanceInches-382.0)<0.006) THROW 50002, 'Mark mismatch for performance 15838', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15838;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15838, 1, 382.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15838, 2, 367.0, 0, 0);

-- Jordan Lavelle Shot Put (M): 21' 9", 23' 0.5"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15846 AND MeetId=292 AND ABS(DistanceInches-276.5)<0.006) THROW 50002, 'Mark mismatch for performance 15846', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15846;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15846, 1, 261.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15846, 2, 276.5, 0, 0);

-- Everett Ryncarz Shot Put (M): 26' 4.5", 27' 9"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15847 AND MeetId=292 AND ABS(DistanceInches-333.0)<0.006) THROW 50002, 'Mark mismatch for performance 15847', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15847;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15847, 1, 316.5, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15847, 2, 333.0, 0, 0);

-- Dayton Clutter Shot Put (M): 30' 8.5", 29' 11"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15848 AND MeetId=292 AND ABS(DistanceInches-368.5)<0.006) THROW 50002, 'Mark mismatch for performance 15848', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15848;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15848, 1, 368.5, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15848, 2, 359.0, 0, 0);

-- Scout Atsma Shot Put (M): 28' 0", 28' 8"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15849 AND MeetId=292 AND ABS(DistanceInches-344.0)<0.006) THROW 50002, 'Mark mismatch for performance 15849', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15849;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15849, 1, 336.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15849, 2, 344.0, 0, 0);

-- Preston Stark Shot Put (M): 28' 7.5", 30' 0"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15850 AND MeetId=292 AND ABS(DistanceInches-360.0)<0.006) THROW 50002, 'Mark mismatch for performance 15850', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15850;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15850, 1, 343.5, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15850, 2, 360.0, 0, 0);

-- Gordy Surrarrer Shot Put (M): 30' 8.5", 30' 9.5"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15851 AND MeetId=292 AND ABS(DistanceInches-369.5)<0.006) THROW 50002, 'Mark mismatch for performance 15851', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15851;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15851, 1, 368.5, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15851, 2, 369.5, 0, 0);

-- Gage Groetz Shot Put (M): 30' 3.5", 30' 6"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15852 AND MeetId=292 AND ABS(DistanceInches-366.0)<0.006) THROW 50002, 'Mark mismatch for performance 15852', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15852;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15852, 1, 363.5, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15852, 2, 366.0, 0, 0);

-- Isaac Montgomery Shot Put (M): 27' 9.5", 28' 4.5"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15853 AND MeetId=292 AND ABS(DistanceInches-340.5)<0.006) THROW 50002, 'Mark mismatch for performance 15853', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15853;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15853, 1, 333.5, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15853, 2, 340.5, 0, 0);

-- Braylon Lade Shot Put (M): 32' 9", 30' 4"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15854 AND MeetId=292 AND ABS(DistanceInches-393.0)<0.006) THROW 50002, 'Mark mismatch for performance 15854', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15854;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15854, 1, 393.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15854, 2, 364.0, 0, 0);

-- Jacob Atkinson Shot Put (M): 36' 6.5", 35' 4.5"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15855 AND MeetId=292 AND ABS(DistanceInches-438.5)<0.006) THROW 50002, 'Mark mismatch for performance 15855', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15855;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15855, 1, 438.5, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15855, 2, 424.5, 0, 0);

-- Jack Chonko Shot Put (M): 38' 4", 37' 4"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15856 AND MeetId=292 AND ABS(DistanceInches-460.0)<0.006) THROW 50002, 'Mark mismatch for performance 15856', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15856;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15856, 1, 460.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15856, 2, 448.0, 0, 0);

-- Ethan Martin Shot Put (M): 37' 2", 36' 5.5"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15857 AND MeetId=292 AND ABS(DistanceInches-446.0)<0.006) THROW 50002, 'Mark mismatch for performance 15857', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15857;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15857, 1, 446.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15857, 2, 437.5, 0, 0);

-- Evan Croft Shot Put (M): 37' 3.5", 36' 10"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15858 AND MeetId=292 AND ABS(DistanceInches-447.5)<0.006) THROW 50002, 'Mark mismatch for performance 15858', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15858;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15858, 1, 447.5, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15858, 2, 442.0, 0, 0);

-- Dominic Nau Shot Put (M): 41' 2", 40' 6"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15859 AND MeetId=292 AND ABS(DistanceInches-494.0)<0.006) THROW 50002, 'Mark mismatch for performance 15859', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15859;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15859, 1, 494.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15859, 2, 486.0, 0, 0);

-- Jake Schaefer Shot Put (M): 36' 1.5", 42' 3"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15860 AND MeetId=292 AND ABS(DistanceInches-507.0)<0.006) THROW 50002, 'Mark mismatch for performance 15860', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15860;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15860, 1, 433.5, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15860, 2, 507.0, 0, 0);

-- Trent Amheiser Shot Put (M): 41' 4", 42' 1"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15861 AND MeetId=292 AND ABS(DistanceInches-505.0)<0.006) THROW 50002, 'Mark mismatch for performance 15861', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15861;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15861, 1, 496.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15861, 2, 505.0, 0, 0);

-- Adam Gray Shot Put (M): 39' 10.5", 40' 1"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15862 AND MeetId=292 AND ABS(DistanceInches-481.0)<0.006) THROW 50002, 'Mark mismatch for performance 15862', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15862;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15862, 1, 478.5, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15862, 2, 481.0, 0, 0);

-- Vienna Polen Discus (F): 46' 1", 48' 4"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15843 AND MeetId=292 AND ABS(DistanceInches-580.0)<0.006) THROW 50002, 'Mark mismatch for performance 15843', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15843;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15843, 1, 553.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15843, 2, 580.0, 0, 0);

-- Ciara Kremer Discus (F): 51' 7", 57' 5"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15841 AND MeetId=292 AND ABS(DistanceInches-689.0)<0.006) THROW 50002, 'Mark mismatch for performance 15841', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15841;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15841, 1, 619.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15841, 2, 689.0, 0, 0);

-- Anna Naftzger Discus (F): 53' 3", 59' 6"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15842 AND MeetId=292 AND ABS(DistanceInches-714.0)<0.006) THROW 50002, 'Mark mismatch for performance 15842', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15842;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15842, 1, 639.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15842, 2, 714.0, 0, 0);

-- Macie Huegel Discus (F): 59' 3", 65' 8"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15840 AND MeetId=292 AND ABS(DistanceInches-788.0)<0.006) THROW 50002, 'Mark mismatch for performance 15840', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15840;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15840, 1, 711.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15840, 2, 788.0, 0, 0);

-- Berkeley Craddock Discus (F): 91' 9", 59' 4"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15839 AND MeetId=292 AND ABS(DistanceInches-1101.0)<0.006) THROW 50002, 'Mark mismatch for performance 15839', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15839;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15839, 1, 1101.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15839, 2, 712.0, 0, 0);

-- Addi Reho Discus (F): 98' 4", 94' 4"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15844 AND MeetId=292 AND ABS(DistanceInches-1180.0)<0.006) THROW 50002, 'Mark mismatch for performance 15844', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15844;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15844, 1, 1180.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15844, 2, 1132.0, 0, 0);

-- Jordan Lavelle Discus (M): 32' 9", 34' 6"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15870 AND MeetId=292 AND ABS(DistanceInches-414.0)<0.006) THROW 50002, 'Mark mismatch for performance 15870', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15870;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15870, 1, 393.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15870, 2, 414.0, 0, 0);

-- Everett Ryncarz Discus (M): 57' 10", 59' 9"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15866 AND MeetId=292 AND ABS(DistanceInches-717.0)<0.006) THROW 50002, 'Mark mismatch for performance 15866', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15866;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15866, 1, 694.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15866, 2, 717.0, 0, 0);

-- Isaac Montgomery Discus (M): 74' 5", 80' 8"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15868 AND MeetId=292 AND ABS(DistanceInches-968.0)<0.006) THROW 50002, 'Mark mismatch for performance 15868', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15868;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15868, 1, 893.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15868, 2, 968.0, 0, 0);

-- Gordy Surrarrer Discus (M): 66' 9", 70' 5"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15863 AND MeetId=292 AND ABS(DistanceInches-845.0)<0.006) THROW 50002, 'Mark mismatch for performance 15863', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15863;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15863, 1, 801.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15863, 2, 845.0, 0, 0);

-- Braylon Lade Discus (M): 90' 4", 75' 6"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15871 AND MeetId=292 AND ABS(DistanceInches-1084.0)<0.006) THROW 50002, 'Mark mismatch for performance 15871', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15871;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15871, 1, 1084.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15871, 2, 906.0, 0, 0);

-- Preston Stark Discus (M): 77' 2", 87' 7"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15864 AND MeetId=292 AND ABS(DistanceInches-1051.0)<0.006) THROW 50002, 'Mark mismatch for performance 15864', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15864;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15864, 1, 926.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15864, 2, 1051.0, 0, 0);

-- Jake Schaefer Discus (M): 82' 8", 75' 10"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15865 AND MeetId=292 AND ABS(DistanceInches-992.0)<0.006) THROW 50002, 'Mark mismatch for performance 15865', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15865;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15865, 1, 992.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15865, 2, 910.0, 0, 0);

-- Scout Atsma Discus (M): 73' 4", 70' 11"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15877 AND MeetId=292 AND ABS(DistanceInches-880.0)<0.006) THROW 50002, 'Mark mismatch for performance 15877', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15877;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15877, 1, 880.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15877, 2, 851.0, 0, 0);

-- Dayton Clutter Discus (M): 89' 2", 91' 6"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15875 AND MeetId=292 AND ABS(DistanceInches-1098.0)<0.006) THROW 50002, 'Mark mismatch for performance 15875', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15875;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15875, 1, 1070.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15875, 2, 1098.0, 0, 0);

-- Dominic Nau Discus (M): 93' 5", 90' 8"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15867 AND MeetId=292 AND ABS(DistanceInches-1121.0)<0.006) THROW 50002, 'Mark mismatch for performance 15867', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15867;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15867, 1, 1121.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15867, 2, 1088.0, 0, 0);

-- Jack Chonko Discus (M): 95' 6", 90' 5"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15876 AND MeetId=292 AND ABS(DistanceInches-1146.0)<0.006) THROW 50002, 'Mark mismatch for performance 15876', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15876;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15876, 1, 1146.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15876, 2, 1085.0, 0, 0);

-- Trent Amheiser Discus (M): 102' 10", 91' 7"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15879 AND MeetId=292 AND ABS(DistanceInches-1234.0)<0.006) THROW 50002, 'Mark mismatch for performance 15879', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15879;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15879, 1, 1234.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15879, 2, 1099.0, 0, 0);

-- Gage Groetz Discus (M): 72' 8", 72' 1"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15872 AND MeetId=292 AND ABS(DistanceInches-872.0)<0.006) THROW 50002, 'Mark mismatch for performance 15872', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15872;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15872, 1, 872.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15872, 2, 865.0, 0, 0);

-- Evan Croft Discus (M): 92' 9", 93' 9"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15874 AND MeetId=292 AND ABS(DistanceInches-1125.0)<0.006) THROW 50002, 'Mark mismatch for performance 15874', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15874;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15874, 1, 1113.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15874, 2, 1125.0, 0, 0);

-- Ethan Martin Discus (M): 116' 11", 77' 7"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15869 AND MeetId=292 AND ABS(DistanceInches-1403.0)<0.006) THROW 50002, 'Mark mismatch for performance 15869', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15869;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15869, 1, 1403.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15869, 2, 931.0, 0, 0);

-- Jacob Atkinson Discus (M): 104' 6", 101' 8"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15878 AND MeetId=292 AND ABS(DistanceInches-1254.0)<0.006) THROW 50002, 'Mark mismatch for performance 15878', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15878;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15878, 1, 1254.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15878, 2, 1220.0, 0, 0);

-- Adam Gray Discus (M): 122' 7", 126' 0"
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15873 AND MeetId=292 AND ABS(DistanceInches-1512.0)<0.006) THROW 50002, 'Mark mismatch for performance 15873', 1;
DELETE FROM PerformanceAttempts WHERE PerformanceId=15873;
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15873, 1, 1471.0, 0, 0);
INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (15873, 2, 1512.0, 0, 0);

COMMIT TRANSACTION;

-- 55 attempt series
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- ===== DB meet 292 =====

-- Lincoln Baltic Pole Vault (M): 7-8 P, 8-6 XO, 9-0 XO, 9-6 XXX
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15821 AND MeetId=292 AND ABS(DistanceInches-108.0)<0.006) THROW 50003, 'Mark mismatch for performance 15821', 1;
DELETE FROM PerformanceBars WHERE PerformanceId=15821;
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (15821, 92.0, 'P');
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (15821, 102.0, 'XO');
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (15821, 108.0, 'XO');
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (15821, 114.0, 'XXX');

-- Taylor Thur Pole Vault (F): 6-0 O, 6-6 O, 7-0 XXX
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15820 AND MeetId=292 AND ABS(DistanceInches-78.0)<0.006) THROW 50003, 'Mark mismatch for performance 15820', 1;
DELETE FROM PerformanceBars WHERE PerformanceId=15820;
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (15820, 72.0, 'O');
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (15820, 78.0, 'O');
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (15820, 84.0, 'XXX');

-- Keegan Wittman High Jump (F): 4-0 O, 4-3 XO, 4-6 XXX
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15823 AND MeetId=292 AND ABS(DistanceInches-51.0)<0.006) THROW 50003, 'Mark mismatch for performance 15823', 1;
DELETE FROM PerformanceBars WHERE PerformanceId=15823;
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (15823, 48.0, 'O');
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (15823, 51.0, 'XO');
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (15823, 54.0, 'XXX');

-- Abigail Reinhardt High Jump (F): 4-6 O, 4-8 O, 4-10 O, 5-0 XXX
IF NOT EXISTS (SELECT 1 FROM Performances WHERE Id=15822 AND MeetId=292 AND ABS(DistanceInches-58.0)<0.006) THROW 50003, 'Mark mismatch for performance 15822', 1;
DELETE FROM PerformanceBars WHERE PerformanceId=15822;
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (15822, 54.0, 'O');
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (15822, 56.0, 'O');
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (15822, 58.0, 'O');
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (15822, 60.0, 'XXX');

COMMIT TRANSACTION;

-- 4 bar ladders
