-- PR-at-the-time / school-record-at-the-time (BRAIN.md [C118]).
-- Adds three columns to Performances and a rebuild procedure that sp_RebuildLeaderboards now calls (inside its transaction):
--   PRAtTime     BIT        - this mark beat every EARLIER mark by the same athlete in the same event, or it is the
--                             athlete's first recorded mark in the event (PreviousBest is NULL for that debut).
--   PreviousBest FLOAT NULL - the athlete's best earlier mark (seconds for running events, inches for field), NULL on a debut.
--   SRAtTime     BIT        - this mark beat every earlier mark by anyone in the event (relays included), i.e. it was the
--                             school record when it was set, whether or not it still is. The first mark in an event IS flagged:
--                             the earliest records in the database are the first marks gathered.
-- Unlike PersonalBest / SchoolRecord these are NOT reset to "current best only": a superseded PR keeps its credit.
-- "Earlier" is by meet day, then meet time, then the round the mark was run in (Prelim < Semi < Final; a mark with no round
-- rows counts as a final), and only then best-first, so a prelim never "follows" a final and only the best of otherwise-tied marks qualifies.
-- Idempotent. Run BEFORE deploying code that reads the columns, then EXEC sp_RebuildLeaderboards once to backfill.
IF COL_LENGTH('dbo.Performances', 'PRAtTime') IS NULL
    ALTER TABLE [dbo].[Performances] ADD [PRAtTime] BIT NOT NULL CONSTRAINT [DF_Performances_PRAtTime] DEFAULT (0);
GO
IF COL_LENGTH('dbo.Performances', 'PreviousBest') IS NULL
    ALTER TABLE [dbo].[Performances] ADD [PreviousBest] FLOAT NULL;
GO
IF COL_LENGTH('dbo.Performances', 'SRAtTime') IS NULL
    ALTER TABLE [dbo].[Performances] ADD [SRAtTime] BIT NOT NULL CONSTRAINT [DF_Performances_SRAtTime] DEFAULT (0);
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_RebuildRecordHistory]
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Performances
    SET PRAtTime = 0, PreviousBest = NULL, SRAtTime = 0
    WHERE PRAtTime = 1 OR PreviousBest IS NOT NULL OR SRAtTime = 1;

    -- Score: higher is always better (field = inches, running = negated seconds), so one MAX() window covers both.
    -- Order: day, meet time, round the mark was run in (prelim before final; no round rows = final), then best mark first, then Id. The frame excludes the current row, so PrevScore = best mark strictly before it.
    WITH Marks AS (
        SELECT
            p.Id,
            p.AthleteId,
            p.EventId,
            CAST(m.Date AS DATE) AS MeetDay,
            m.Date AS MeetDate,
            COALESCE((SELECT TOP 1 r.[Round] FROM PerformanceRounds r WHERE r.PerformanceId = p.Id AND r.TimeSeconds = p.TimeSeconds ORDER BY r.[Round]), 3) AS RoundSeq,
            CAST(CASE WHEN e.EventType IN (0, 2, 4, 5) THEN 1 ELSE 0 END AS BIT) AS IsField,
            CASE WHEN e.EventType IN (0, 2, 4, 5) THEN p.DistanceInches ELSE -p.TimeSeconds END AS Score
        FROM Performances p
        INNER JOIN Events e ON e.Id = p.EventId
        INNER JOIN Meets m ON m.Id = p.MeetId
        WHERE p.ResultStatus = 0
          AND p.AthleteId IS NOT NULL
          AND (p.DistanceInches IS NOT NULL OR p.TimeSeconds IS NOT NULL)
    ),
    Prior AS (
        SELECT
            Id, Score, IsField,
            MAX(Score) OVER (
                PARTITION BY AthleteId, EventId
                ORDER BY MeetDay, MeetDate, RoundSeq, Score DESC, Id
                ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING
            ) AS PrevScore
        FROM Marks
    )
    UPDATE p
    SET p.PRAtTime = 1,
        p.PreviousBest = CASE WHEN pr.PrevScore IS NULL THEN NULL
                              WHEN pr.IsField = 1 THEN pr.PrevScore
                              ELSE -pr.PrevScore END
    FROM Performances p
    INNER JOIN Prior pr ON pr.Id = p.Id
    WHERE pr.PrevScore IS NULL OR pr.Score > pr.PrevScore;

    -- School record at the time: same idea over the whole event population (relay rows have AthleteId NULL and are included).
    -- The event's first mark has no PrevScore and IS flagged: per the database it was the school record when set.
    WITH EventMarks AS (
        SELECT
            p.Id,
            p.EventId,
            CAST(m.Date AS DATE) AS MeetDay,
            m.Date AS MeetDate,
            COALESCE((SELECT TOP 1 r.[Round] FROM PerformanceRounds r WHERE r.PerformanceId = p.Id AND r.TimeSeconds = p.TimeSeconds ORDER BY r.[Round]), 3) AS RoundSeq,
            CASE WHEN e.EventType IN (0, 2, 4, 5) THEN p.DistanceInches ELSE -p.TimeSeconds END AS Score
        FROM Performances p
        INNER JOIN Events e ON e.Id = p.EventId
        INNER JOIN Meets m ON m.Id = p.MeetId
        WHERE p.ResultStatus = 0
          AND (p.DistanceInches IS NOT NULL OR p.TimeSeconds IS NOT NULL)
    ),
    EventPrior AS (
        SELECT
            Id, Score,
            MAX(Score) OVER (
                PARTITION BY EventId
                ORDER BY MeetDay, MeetDate, RoundSeq, Score DESC, Id
                ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING
            ) AS PrevScore
        FROM EventMarks
    )
    UPDATE p
    SET p.SRAtTime = 1
    FROM Performances p
    INNER JOIN EventPrior ep ON ep.Id = p.Id
    WHERE ep.PrevScore IS NULL OR ep.Score > ep.PrevScore;
END;
GO

-- Make sp_RebuildLeaderboards call it, without restating the whole procedure here: patch the live definition once.
DECLARE @def NVARCHAR(MAX) = OBJECT_DEFINITION(OBJECT_ID('dbo.sp_RebuildLeaderboards'));
IF @def IS NULL
    RAISERROR('sp_RebuildLeaderboards not found', 16, 1);
ELSE IF CHARINDEX('sp_RebuildRecordHistory', @def) = 0
BEGIN
    IF CHARINDEX('COMMIT TRANSACTION;', @def) = 0 OR CHARINDEX('CREATE PROCEDURE', @def) = 0
        RAISERROR('sp_RebuildLeaderboards has an unexpected shape; add "EXEC dbo.sp_RebuildRecordHistory;" before its COMMIT TRANSACTION by hand', 16, 1);
    ELSE
    BEGIN
        SET @def = REPLACE(@def, 'COMMIT TRANSACTION;',
                           'EXEC dbo.sp_RebuildRecordHistory;' + CHAR(13) + CHAR(10) + '        COMMIT TRANSACTION;');
        SET @def = STUFF(@def, CHARINDEX('CREATE PROCEDURE', @def), LEN('CREATE PROCEDURE'), 'ALTER PROCEDURE');
        EXEC (@def);
    END
END
GO
