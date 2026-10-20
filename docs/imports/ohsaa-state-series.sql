-- State-meet attempt series from the official OHSAA results (baumspage.com/ohsaa/tf/<year>/Results*.htm) (BRAIN.md [C94]).
-- Each performance is found by athlete + meet + event + best attempt (Ids differ between databases); a missing match is skipped, an existing series is left alone.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @pid INT;

-- Katelyn Young, 2016 OHSAA Division II State Track and Field Championships, Discus: 115-01, 115-01, 112-08
SET @pid = (SELECT TOP 1 p.Id FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Meets m ON m.Id=p.MeetId JOIN Events e ON e.Id=p.EventId WHERE a.FirstName=N'Katelyn' AND a.LastName=N'Young' AND a.Gender=2 AND m.Name=N'2016 OHSAA Division II State Track and Field Championships' AND e.Name=N'Discus' AND e.Gender=2 AND ABS(p.DistanceInches-1381.0)<0.006);
IF @pid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PerformanceAttempts WHERE PerformanceId=@pid)
BEGIN
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 1, 1381.0, 0, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 2, 1381.0, 0, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 3, 1352.0, 0, 0);
END

-- Katelyn Young, 2016 OHSAA Division II State Track and Field Championships, Shot Put: 35-04, FOUL, 34-04
SET @pid = (SELECT TOP 1 p.Id FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Meets m ON m.Id=p.MeetId JOIN Events e ON e.Id=p.EventId WHERE a.FirstName=N'Katelyn' AND a.LastName=N'Young' AND a.Gender=2 AND m.Name=N'2016 OHSAA Division II State Track and Field Championships' AND e.Name=N'Shot Put' AND e.Gender=2 AND ABS(p.DistanceInches-424.0)<0.006);
IF @pid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PerformanceAttempts WHERE PerformanceId=@pid)
BEGIN
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 1, 424.0, 0, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 2, NULL, 1, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 3, 412.0, 0, 0);
END

-- Katelyn Young, 2017 OHSAA Division II State Track and Field Championships, Discus: 110-02, FOUL, 125-10
SET @pid = (SELECT TOP 1 p.Id FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Meets m ON m.Id=p.MeetId JOIN Events e ON e.Id=p.EventId WHERE a.FirstName=N'Katelyn' AND a.LastName=N'Young' AND a.Gender=2 AND m.Name=N'2017 OHSAA Division II State Track and Field Championships' AND e.Name=N'Discus' AND e.Gender=2 AND ABS(p.DistanceInches-1510.0)<0.006);
IF @pid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PerformanceAttempts WHERE PerformanceId=@pid)
BEGIN
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 1, 1322.0, 0, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 2, NULL, 1, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 3, 1510.0, 0, 0);
END

-- Brooklynn Crist, 2018 OHSAA Seated Division State Track and Field Championships, Seated Shot Put: 15-05.50, 14-09.50, 15-02, 14-02
SET @pid = (SELECT TOP 1 p.Id FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Meets m ON m.Id=p.MeetId JOIN Events e ON e.Id=p.EventId WHERE a.FirstName=N'Brooklynn' AND a.LastName=N'Crist' AND a.Gender=2 AND m.Name=N'2018 OHSAA Seated Division State Track and Field Championships' AND e.Name=N'Seated Shot Put' AND e.Gender=2 AND ABS(p.DistanceInches-185.5)<0.006);
IF @pid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PerformanceAttempts WHERE PerformanceId=@pid)
BEGIN
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 1, 185.5, 0, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 2, 177.5, 0, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 3, 182.0, 0, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 4, 170.0, 0, 0);
END

-- Maddie Boltz, 2019 OHSAA Division II State Track and Field Championships, Long Jump: 13-10.50, FOUL, 14-02.75
SET @pid = (SELECT TOP 1 p.Id FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Meets m ON m.Id=p.MeetId JOIN Events e ON e.Id=p.EventId WHERE a.FirstName=N'Maddie' AND a.LastName=N'Boltz' AND a.Gender=2 AND m.Name=N'2019 OHSAA Division II State Track and Field Championships' AND e.Name=N'Long Jump' AND e.Gender=2 AND ABS(p.DistanceInches-170.75)<0.006);
IF @pid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PerformanceAttempts WHERE PerformanceId=@pid)
BEGIN
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 1, 166.5, 0, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 2, NULL, 1, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 3, 170.75, 0, 0);
END

-- Brooklynn Crist, 2021 OHSAA Seated Division State Track and Field Championship, Seated Shot Put: 16-10, 14-11.50, 16-04.75, 16-10
SET @pid = (SELECT TOP 1 p.Id FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Meets m ON m.Id=p.MeetId JOIN Events e ON e.Id=p.EventId WHERE a.FirstName=N'Brooklynn' AND a.LastName=N'Crist' AND a.Gender=2 AND m.Name=N'2021 OHSAA Seated Division State Track and Field Championship' AND e.Name=N'Seated Shot Put' AND e.Gender=2 AND ABS(p.DistanceInches-202.0)<0.006);
IF @pid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PerformanceAttempts WHERE PerformanceId=@pid)
BEGIN
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 1, 202.0, 0, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 2, 179.5, 0, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 3, 196.75, 0, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 4, 202.0, 0, 0);
END

-- Bridgette Donahue, 2021 OHSAA Seated Division State Track and Field Championship, Seated Shot Put: 7-01.50, 6-10.50, 6-11, 7-04
SET @pid = (SELECT TOP 1 p.Id FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Meets m ON m.Id=p.MeetId JOIN Events e ON e.Id=p.EventId WHERE a.FirstName=N'Bridgette' AND a.LastName=N'Donahue' AND a.Gender=2 AND m.Name=N'2021 OHSAA Seated Division State Track and Field Championship' AND e.Name=N'Seated Shot Put' AND e.Gender=2 AND ABS(p.DistanceInches-88.0)<0.006);
IF @pid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM PerformanceAttempts WHERE PerformanceId=@pid)
BEGIN
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 1, 85.5, 0, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 2, 82.5, 0, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 3, 83.0, 0, 0);
    INSERT INTO PerformanceAttempts (PerformanceId, AttemptNumber, DistanceInches, IsFoul, IsPass) VALUES (@pid, 4, 88.0, 0, 0);
END

COMMIT TRANSACTION;
