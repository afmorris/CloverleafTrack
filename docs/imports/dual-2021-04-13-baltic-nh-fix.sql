-- 2021 Woodridge Dual (owner correction 2): the pole vaulters started at 7'6", so Isaac Baltic's ladder is 7'6 O, 8' XXX (no 7' bar); Grace Goldsberry and Avery Kott are NH (status 2), not NM. Id-independent; re-runnable.
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT = (SELECT TOP 1 Id FROM Meets WHERE Name=N'2021 Woodridge Dual' AND CAST(Date AS date)='2021-04-13');
DECLARE @pid INT;
IF @m IS NULL THROW 50120, 'meet missing', 1;
SET @pid = (SELECT TOP 1 p.Id FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND a.FirstName+N' '+a.LastName=N'Isaac Baltic' AND e.Gender=1 AND e.Name=N'Pole Vault');
IF @pid IS NULL THROW 50121, 'Baltic PV missing', 1;
DELETE FROM PerformanceBars WHERE PerformanceId=@pid;
INSERT INTO PerformanceBars (PerformanceId, HeightInches, Outcome) VALUES (@pid, 90, 'O'), (@pid, 96, 'XXX');
UPDATE p SET p.ResultStatus=2 FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND e.Gender=2 AND e.Name=N'Pole Vault' AND p.ResultStatus=1 AND a.FirstName+N' '+a.LastName IN (N'Grace Goldsberry', N'Avery Kott');
COMMIT TRANSACTION;
EXEC sp_RebuildLeaderboards;
