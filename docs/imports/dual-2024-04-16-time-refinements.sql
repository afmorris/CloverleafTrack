-- 2024-04-16 Norton-Springfield Double Dual: the DB held whole-second times; the scorer sheets give tenths. Old values in BRAIN.md [C106]. Id-independent; re-runnable.
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @m INT, @n INT;
SET @m = (SELECT TOP 1 Id FROM Meets WHERE Name=N'2024 Norton-Springfield Double Dual' AND CAST(Date AS date)='2024-04-16');
IF @m IS NULL THROW 50080, 'meet missing', 1;
UPDATE p SET p.TimeSeconds=720.99 FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND a.FirstName+N' '+a.LastName=N'Caleb Neville' AND e.Gender=1 AND e.Name=N'3200M' AND ABS(p.TimeSeconds-720.0)<0.006;
SET @n = @@ROWCOUNT; IF @n > 1 THROW 50081, 'more than one row matched', 1;
UPDATE p SET p.TimeSeconds=776.28 FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND a.FirstName+N' '+a.LastName=N'Noah Mion' AND e.Gender=1 AND e.Name=N'3200M' AND ABS(p.TimeSeconds-776.0)<0.006;
SET @n = @@ROWCOUNT; IF @n > 1 THROW 50081, 'more than one row matched', 1;
UPDATE p SET p.TimeSeconds=776.69 FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND a.FirstName+N' '+a.LastName=N'Austin McNeal' AND e.Gender=1 AND e.Name=N'3200M' AND ABS(p.TimeSeconds-776.0)<0.006;
SET @n = @@ROWCOUNT; IF @n > 1 THROW 50081, 'more than one row matched', 1;
UPDATE p SET p.TimeSeconds=130.1 FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND a.FirstName+N' '+a.LastName=N'Ben Myers' AND e.Gender=1 AND e.Name=N'800M' AND ABS(p.TimeSeconds-130.0)<0.006;
SET @n = @@ROWCOUNT; IF @n > 1 THROW 50081, 'more than one row matched', 1;
UPDATE p SET p.TimeSeconds=133.8 FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND a.FirstName+N' '+a.LastName=N'Matthew Jenkins' AND e.Gender=1 AND e.Name=N'800M' AND ABS(p.TimeSeconds-133.0)<0.006;
SET @n = @@ROWCOUNT; IF @n > 1 THROW 50081, 'more than one row matched', 1;
UPDATE p SET p.TimeSeconds=162.8 FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND a.FirstName+N' '+a.LastName=N'Chayton Bullis' AND e.Gender=1 AND e.Name=N'800M' AND ABS(p.TimeSeconds-162.0)<0.006;
SET @n = @@ROWCOUNT; IF @n > 1 THROW 50081, 'more than one row matched', 1;
UPDATE p SET p.TimeSeconds=806.1 FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Events e ON e.Id=p.EventId WHERE p.MeetId=@m AND a.FirstName+N' '+a.LastName=N'Megan Knapp' AND e.Gender=2 AND e.Name=N'3200M' AND ABS(p.TimeSeconds-806.0)<0.006;
SET @n = @@ROWCOUNT; IF @n > 1 THROW 50081, 'more than one row matched', 1;
COMMIT TRANSACTION;
EXEC sp_RebuildLeaderboards;
-- 7 changes
