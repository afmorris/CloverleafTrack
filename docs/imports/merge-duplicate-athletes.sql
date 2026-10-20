-- Merges duplicate athlete records (BRAIN.md [C85]). Athletes are found by exact first/last name + gender + class year (Ids differ between local and live).
-- For each pair: results at the same meet+event under both records must have the same mark (the duplicate is deleted); everything else is moved to the keeper.
-- Aborts (rolls back) if a duplicate result has a different mark, placings/entries/relay legs/series hang off it, or a name lookup is not unique.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @pairs TABLE (g SMALLINT, ky INT, dy INT, kf NVARCHAR(100), kl NVARCHAR(100), df NVARCHAR(100), dl NVARCHAR(100), nf NVARCHAR(100), nl NVARCHAR(100));
INSERT INTO @pairs VALUES
 (2, 2021, 2021, N'Bridgette', N'Donohue',  N'Bridgette', N'Donahue',   N'Bridgette', N'Donahue'),
 (2, 2007, 2007, N'Amber',     N'Hanson',   N'Amber',     N'Hansen',    N'Amber',     N'Hansen'),
 (1, 2022, 2022, N'Trevor',    N'Milbrandt',N'Trevor',    N'Millbrandt',N'Trevor',    N'Milbrandt'),
 (2, 2020, 2020, N'Megan',     N'VanMeter', N'Megan',     N'Van Meter', N'Megan',     N'Van Meter'),
 (2, 2007, 2007, N'Morgan',    N'ORourke',  N'Morgan',    N'O''Rourke', N'Morgan',    N'O''Rourke'),
 (1, 2008, 2007, N'Richard',   N'Morrison', N'Ricky',     N'Morrison',  N'Richard',   N'Morrison'),
 (1, 2012, 2012, N'Jon',       N'Hearn',    N'Jonathan',  N'Hearn',     N'Jon',       N'Hearn');
DECLARE @g SMALLINT, @ky INT, @dy INT, @kf NVARCHAR(100), @kl NVARCHAR(100), @df NVARCHAR(100), @dl NVARCHAR(100), @nf NVARCHAR(100), @nl NVARCHAR(100), @k INT, @d INT;
DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT g, ky, dy, kf, kl, df, dl, nf, nl FROM @pairs;
OPEN c; FETCH NEXT FROM c INTO @g, @ky, @dy, @kf, @kl, @df, @dl, @nf, @nl;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF (SELECT COUNT(*) FROM Athletes WHERE FirstName = @kf AND LastName = @kl AND Gender = @g AND GraduationYear = @ky) <> 1
       OR (SELECT COUNT(*) FROM Athletes WHERE FirstName = @df AND LastName = @dl AND Gender = @g AND GraduationYear = @dy) <> 1
        THROW 50070, 'Keeper or duplicate athlete not found exactly once', 1;
    SELECT @k = Id FROM Athletes WHERE FirstName = @kf AND LastName = @kl AND Gender = @g AND GraduationYear = @ky;
    SELECT @d = Id FROM Athletes WHERE FirstName = @df AND LastName = @dl AND Gender = @g AND GraduationYear = @dy;

    -- results at the same meet+event under both records
    CREATE TABLE #dup (DupPerf INT, KeepPerf INT);
    INSERT INTO #dup SELECT dp.Id, kp.Id FROM Performances dp JOIN Performances kp ON kp.MeetId = dp.MeetId AND kp.EventId = dp.EventId AND kp.AthleteId = @k WHERE dp.AthleteId = @d;
    IF EXISTS (SELECT 1 FROM #dup x JOIN Performances dp ON dp.Id = x.DupPerf JOIN Performances kp ON kp.Id = x.KeepPerf
               WHERE dp.ResultStatus <> kp.ResultStatus OR ABS(ISNULL(dp.TimeSeconds, -1) - ISNULL(kp.TimeSeconds, -1)) > 0.006 OR ABS(ISNULL(dp.DistanceInches, -1) - ISNULL(kp.DistanceInches, -1)) > 0.006)
        THROW 50071, 'A duplicate result has a different mark than the keeper''s', 1;
    IF EXISTS (SELECT 1 FROM MeetPlacings WHERE PerformanceId IN (SELECT DupPerf FROM #dup)) OR EXISTS (SELECT 1 FROM MeetEntries WHERE PerformanceId IN (SELECT DupPerf FROM #dup))
       OR EXISTS (SELECT 1 FROM PerformanceAthletes WHERE PerformanceId IN (SELECT DupPerf FROM #dup)) OR EXISTS (SELECT 1 FROM PerformanceAttempts WHERE PerformanceId IN (SELECT DupPerf FROM #dup))
       OR EXISTS (SELECT 1 FROM PerformanceBars WHERE PerformanceId IN (SELECT DupPerf FROM #dup)) OR EXISTS (SELECT 1 FROM PerformanceRounds WHERE PerformanceId IN (SELECT DupPerf FROM #dup))
        THROW 50072, 'A duplicate result has placings, entries, legs or series attached', 1;
    DELETE FROM PersonalBestHistory WHERE PerformanceId IN (SELECT DupPerf FROM #dup) OR AthleteId = @d;   -- derived; rebuilt below
    DELETE FROM Leaderboards WHERE PerformanceId IN (SELECT DupPerf FROM #dup);
    DELETE FROM PerformancePercentiles WHERE PerformanceId IN (SELECT DupPerf FROM #dup);
    DELETE FROM Performances WHERE Id IN (SELECT DupPerf FROM #dup);
    DROP TABLE #dup;

    UPDATE Performances SET AthleteId = @k WHERE AthleteId = @d;
    DELETE FROM PerformanceAthletes WHERE AthleteId = @d AND PerformanceId IN (SELECT PerformanceId FROM PerformanceAthletes WHERE AthleteId = @k);
    UPDATE PerformanceAthletes SET AthleteId = @k WHERE AthleteId = @d;
    UPDATE MeetEntries SET AthleteId = @k WHERE AthleteId = @d;
    DELETE FROM MeetEntryAthletes WHERE AthleteId = @d AND MeetEntryId IN (SELECT MeetEntryId FROM MeetEntryAthletes WHERE AthleteId = @k);
    UPDATE MeetEntryAthletes SET AthleteId = @k WHERE AthleteId = @d;
    IF EXISTS (SELECT 1 FROM AthleteSummaries WHERE AthleteId = @k) DELETE FROM AthleteSummaries WHERE AthleteId = @d; ELSE UPDATE AthleteSummaries SET AthleteId = @k WHERE AthleteId = @d;
    UPDATE Athletes SET FirstName = @nf, LastName = @nl WHERE Id = @k;
    DELETE FROM Athletes WHERE Id = @d;
    FETCH NEXT FROM c INTO @g, @ky, @dy, @kf, @kl, @df, @dl, @nf, @nl;
END
CLOSE c; DEALLOCATE c;

-- Sarah Hollopeter (class of 2011): empty record, Sierra Hollopeter is the athlete on every source
DECLARE @s INT = (SELECT Id FROM Athletes WHERE FirstName = N'Sarah' AND LastName = N'Hollopeter' AND Gender = 2 AND GraduationYear = 2011);
IF @s IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM Performances WHERE AthleteId = @s) OR EXISTS (SELECT 1 FROM PerformanceAthletes WHERE AthleteId = @s) OR EXISTS (SELECT 1 FROM MeetEntries WHERE AthleteId = @s)
       OR EXISTS (SELECT 1 FROM MeetEntryAthletes WHERE AthleteId = @s) OR EXISTS (SELECT 1 FROM AthleteSummaries WHERE AthleteId = @s) OR EXISTS (SELECT 1 FROM PersonalBestHistory WHERE AthleteId = @s)
        THROW 50073, 'Sarah Hollopeter is not empty', 1;
    DELETE FROM Athletes WHERE Id = @s;
END
EXEC sp_RebuildLeaderboards;
COMMIT TRANSACTION;
