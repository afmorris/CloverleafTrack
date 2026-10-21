-- Second one-by-one review with the owner: Spencer 800 -> 3:34.7 (sheet; the DB had truncated 3:34), Sewalish/Usry 200 back to the original DB values (27.65 / 32.58) after my 27.45 / 33.58 misreading. Old values in BRAIN.md [C117]. Id-independent; re-runnable.
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT = (SELECT TOP 1 Id FROM Meets WHERE Name=N'2025 Woodridge Dual' AND CAST(Date AS date)='2025-04-15');
DECLARE @n INT;
IF @m IS NULL THROW 50140, 'meet missing', 1;
UPDATE p SET p.TimeSeconds=214.7 FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND a.FirstName+N' '+a.LastName=N'Olivia Spencer' AND e.Gender=2 AND e.Name=N'800M' AND ABS(p.TimeSeconds-204.7)<0.006;
SET @n = @@ROWCOUNT; IF @n > 1 THROW 50141, 'more than one row', 1;
UPDATE p SET p.TimeSeconds=27.65 FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND a.FirstName+N' '+a.LastName=N'Clayton Sewalish' AND e.Gender=1 AND e.Name=N'200M' AND ABS(p.TimeSeconds-27.45)<0.006;
SET @n = @@ROWCOUNT; IF @n > 1 THROW 50142, 'more than one row', 1;
UPDATE p SET p.TimeSeconds=32.58 FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND a.FirstName+N' '+a.LastName=N'Amir Usry' AND e.Gender=1 AND e.Name=N'200M' AND ABS(p.TimeSeconds-33.58)<0.006;
SET @n = @@ROWCOUNT; IF @n > 1 THROW 50143, 'more than one row', 1;
COMMIT TRANSACTION;
EXEC sp_RebuildLeaderboards;
