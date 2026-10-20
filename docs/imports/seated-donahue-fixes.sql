-- (1) Five Seated Shot Put marks were stored in TimeSeconds instead of DistanceInches (invisible to ranks/leaderboards/marks). Move them. Each guarded by the old value.
-- (2) Bridgette Donahue's wheelchair results missing from the DB (MileSplit raw results): 2018 Cowen Redbird, 2018 PTC, 2021 Medina Relays. BRAIN.md [C80].
SET XACT_ABORT ON;
BEGIN TRANSACTION;
UPDATE p SET DistanceInches = p.TimeSeconds, TimeSeconds = NULL
FROM Performances p JOIN Events e ON e.Id = p.EventId
WHERE e.Name = 'Seated Shot Put' AND p.TimeSeconds IS NOT NULL AND p.DistanceInches IS NULL AND p.ResultStatus = 0;
IF @@ROWCOUNT > 5 THROW 50040, 'More than 5 seated shot put marks to move: check before running', 1;   -- local had 5, live had 2 (the rest were already correct)

DECLARE @d INT = (SELECT Id FROM Athletes WHERE FirstName = 'Bridgette' AND LastName = 'Donahue');
DECLARE @e100 INT = (SELECT Id FROM Events WHERE Name = 'Seated 100M' AND Gender = 2 AND Environment = 1);
DECLARE @e400 INT = (SELECT Id FROM Events WHERE Name = 'Seated 400M' AND Gender = 2 AND Environment = 1);
DECLARE @esp INT = (SELECT Id FROM Events WHERE Name = 'Seated Shot Put' AND Gender = 2 AND Environment = 1);
DECLARE @cow INT = (SELECT Id FROM Meets WHERE Name = '2018 Cowen Redbird Invitational' AND CAST(Date AS date) = '2018-04-14');
DECLARE @ptc INT = (SELECT Id FROM Meets WHERE Name = '2018 PTC Track Championships' AND CAST(Date AS date) = '2018-05-09');
DECLARE @med INT = (SELECT Id FROM Meets WHERE Name = '2021 Tom Fasko Medina Relays' AND CAST(Date AS date) = '2021-04-10');
IF @d IS NULL OR @e100 IS NULL OR @e400 IS NULL OR @esp IS NULL OR @cow IS NULL OR @ptc IS NULL OR @med IS NULL THROW 50041, 'Athlete, event or meet not found', 1;

DECLARE @rows TABLE (m INT, e INT, t FLOAT, dist FLOAT);
INSERT INTO @rows VALUES (@cow, @e100, 49.85, NULL), (@cow, @e400, 206.55, NULL), (@cow, @esp, NULL, 82),
                         (@ptc, @e100, 42.78, NULL), (@ptc, @e400, 209.46, NULL), (@ptc, @esp, NULL, 84.75),
                         (@med, @e100, 34.61, NULL);
INSERT INTO Performances (AthleteId, MeetId, EventId, DistanceInches, TimeSeconds, SchoolRecord, SeasonBest, PersonalBest, ResultStatus)
SELECT @d, r.m, r.e, r.dist, r.t, 0, 0, 0, 0 FROM @rows r
WHERE NOT EXISTS (SELECT 1 FROM Performances p WHERE p.AthleteId = @d AND p.MeetId = r.m AND p.EventId = r.e);
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
