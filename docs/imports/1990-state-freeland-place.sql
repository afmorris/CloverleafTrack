-- Dave Freeland finished 2nd in the 1990 OHSAA State 800M (1:52.40; 8 points on the official sheet). Id-independent; re-runnable.
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @p INT = (SELECT TOP 1 p.Id FROM Performances p JOIN Athletes a ON a.Id=p.AthleteId JOIN Meets m ON m.Id=p.MeetId JOIN Events e ON e.Id=p.EventId
  WHERE a.FirstName=N'Dave' AND a.LastName=N'Freeland' AND m.Name=N'1990 OHSAA Division I State Track and Field Championships' AND e.Name=N'800M' AND e.Gender=1 AND ABS(p.TimeSeconds-112.4)<0.006);
DECLARE @m INT = (SELECT Id FROM Meets WHERE Name=N'1990 OHSAA Division I State Track and Field Championships');
IF @p IS NULL THROW 50014, 'Freeland 800M not found', 1;
IF NOT EXISTS (SELECT 1 FROM MeetPlacings WHERE PerformanceId=@p AND MeetParticipantId IS NULL)
    INSERT INTO MeetPlacings (MeetId, PerformanceId, MeetParticipantId, Place, FullPoints, SplitPoints) VALUES (@m, @p, NULL, 2, 0, 0);
COMMIT TRANSACTION;
