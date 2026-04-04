-- ============================================================
-- CloverleafTrack — Full Database Schema
-- SQL Server (T-SQL)
-- ============================================================
-- Enum reference (defined in CloverleafTrack.Models/Enums/):
--
--   EventType (SMALLINT):
--     0 = Field           individual field event (jumps, throws, pole vault)
--     1 = Running         individual running event
--     2 = FieldRelay      relay of a field event
--     3 = RunningRelay    relay of a running event (4x100, 4x400, etc.)
--     4 = JumpRelay       relay of a jump event
--     5 = ThrowsRelay     relay of a throws event
--
--   EventCategory (SMALLINT):
--     0 = Sprints
--     1 = Distance
--     2 = Throws
--     3 = Jumps
--     4 = Relays
--     5 = Hurdles
--
--   Environment (SMALLINT):
--     1 = Outdoor
--     2 = Indoor
--
--   Gender (SMALLINT):
--     1 = Male
--     2 = Female
--     3 = Mixed   (used for mixed-gender relay events)
--
--   MeetEntryStatus (INT stored in Meets.EntryStatus):
--     0 = NotAvailable
--     1 = Scanned
--     2 = Placeholder
--     3 = Entered
--
--   SeasonStatus (INT stored in Seasons.Status):
--     1 = Draft
--     2 = Importing
--     3 = Partial
--     4 = RecordOnly
--     5 = Complete
-- ============================================================


-- ============================================================
-- SEASONS
-- ============================================================
CREATE TABLE [dbo].[Seasons] (
    [Id]              INT            IDENTITY (1, 1) NOT NULL,
    [Name]            NVARCHAR (100) NOT NULL,
    [StartDate]       DATE           NULL,
    [EndDate]         DATE           NULL,
    [IsCurrentSeason] BIT            DEFAULT ((0)) NOT NULL,
    [Notes]           NVARCHAR (250) NULL,
    [Status]          INT            DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


-- ============================================================
-- LOCATIONS
-- ============================================================
CREATE TABLE [dbo].[Locations] (
    [Id]      INT            IDENTITY (1, 1) NOT NULL,
    [Name]    NVARCHAR (200) NOT NULL,
    [City]    NVARCHAR (100) NULL,
    [State]   NVARCHAR (50)  NULL,
    [ZipCode] NVARCHAR (20)  NULL,
    [Country] NVARCHAR (50)  DEFAULT ('USA') NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


-- ============================================================
-- MEETS
-- ============================================================
CREATE TABLE [dbo].[Meets] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (200) NOT NULL,
    [Date]        DATETIME2 (3)  NOT NULL,
    [LocationId]  INT            NULL,
    [Environment] SMALLINT       NOT NULL,   -- See Environment enum above
    [HandTimed]   BIT            NOT NULL,
    [SeasonId]    INT            NOT NULL,
    [EntryStatus] INT            DEFAULT ((0)) NOT NULL,   -- See MeetEntryStatus enum above
    [EntryNotes]  NVARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Meets_Seasons]   FOREIGN KEY ([SeasonId])   REFERENCES [dbo].[Seasons]   ([Id]),
    CONSTRAINT [FK_Meets_Locations] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[Locations] ([Id])
);

CREATE NONCLUSTERED INDEX [IX_Meets_SeasonId]
    ON [dbo].[Meets]([SeasonId] ASC);

CREATE NONCLUSTERED INDEX [IX_Meets_LocationId]
    ON [dbo].[Meets]([LocationId] ASC);


-- ============================================================
-- ATHLETES
-- ============================================================
CREATE TABLE [dbo].[Athletes] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]      NVARCHAR (100) NOT NULL,
    [LastName]       NVARCHAR (100) NOT NULL,
    [Gender]         SMALLINT       NOT NULL,   -- See Gender enum above
    [GraduationYear] INT            NOT NULL,
    [IsActive]       BIT            DEFAULT ((1)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


-- ============================================================
-- EVENTS
-- Individual and relay event definitions.
-- NOTE: Mixed-gender running relay events are stored in a
--       separate RunningRelayEvents table (UNIQUEIDENTIFIER Id,
--       no EventKey column). See that table definition below.
-- ============================================================
CREATE TABLE [dbo].[Events] (
    [Id]                     INT            IDENTITY (1, 1) NOT NULL,
    [Name]                   NVARCHAR (150) NOT NULL,
    [EventKey]               NVARCHAR (50)  NOT NULL,
    [EventType]              SMALLINT       NOT NULL,   -- See EventType enum above
    [Gender]                 SMALLINT       NOT NULL,   -- See Gender enum above
    [Environment]            SMALLINT       NOT NULL,   -- See Environment enum above
    [AthleteCount]           INT            NOT NULL,   -- Number of athletes per relay leg (1 for individual)
    [SortOrder]              INT            NULL,
    [EventCategory]          SMALLINT       NULL,       -- See EventCategory enum above
    [EventCategorySortOrder] INT            NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_Events_EventKey_Gender_Environment]
        UNIQUE NONCLUSTERED ([EventKey] ASC, [Gender] ASC, [Environment] ASC)
);

CREATE UNIQUE NONCLUSTERED INDEX [UX_Events_EventKey]
    ON [dbo].[Events]([EventKey] ASC);

CREATE NONCLUSTERED INDEX [IX_Events_EventType]
    ON [dbo].[Events]([EventType] ASC);

CREATE NONCLUSTERED INDEX [IX_Events_EventCategory]
    ON [dbo].[Events]([EventCategory] ASC);


-- ============================================================
-- PERFORMANCES
-- Core results table.
-- AthleteId is NULL for relay performances (athletes are
-- linked via PerformanceAthletes junction table instead).
-- Exactly one of DistanceInches or TimeSeconds must be non-NULL
-- (enforced by check constraint).
-- PersonalBest, SeasonBest, and SchoolRecord flags are set by
-- sp_RebuildLeaderboards — do NOT rely on them for relay rows
-- (they are not set for relay performances by the stored proc).
-- ============================================================
CREATE TABLE [dbo].[Performances] (
    [Id]                INT        IDENTITY (1, 1) NOT NULL,
    [AthleteId]         INT        NULL,            -- NULL for relay performances
    [MeetId]            INT        NOT NULL,
    [EventId]           INT        NOT NULL,
    [DistanceInches]    FLOAT (53) NULL,            -- Field events (higher = better)
    [TimeSeconds]       FLOAT (53) NULL,            -- Running events (lower = better)
    [SortedAthleteHash] CHAR (64)  NULL,            -- Hash of sorted relay athlete IDs; nullable, no NOT NULL constraint
    [SchoolRecord]      BIT        DEFAULT ((0)) NOT NULL,
    [SeasonBest]        BIT        DEFAULT ((0)) NOT NULL,
    [PersonalBest]      BIT        DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Performances_Athletes] FOREIGN KEY ([AthleteId]) REFERENCES [dbo].[Athletes] ([Id]),
    CONSTRAINT [FK_Performances_Meets]    FOREIGN KEY ([MeetId])    REFERENCES [dbo].[Meets]    ([Id]),
    CONSTRAINT [FK_Performances_Events]   FOREIGN KEY ([EventId])   REFERENCES [dbo].[Events]   ([Id]),
    CONSTRAINT [CK_Performances_DistanceOrTime]
        CHECK (
            ([DistanceInches] IS NOT NULL AND [TimeSeconds] IS NULL)
            OR ([DistanceInches] IS NULL AND [TimeSeconds] IS NOT NULL)
        )
);

CREATE NONCLUSTERED INDEX [IX_Performances_AthleteId]
    ON [dbo].[Performances]([AthleteId] ASC);

CREATE NONCLUSTERED INDEX [IX_Performances_EventId]
    ON [dbo].[Performances]([EventId] ASC);

CREATE NONCLUSTERED INDEX [IX_Performances_SortedAthleteHash]
    ON [dbo].[Performances]([SortedAthleteHash] ASC);

CREATE NONCLUSTERED INDEX [IX_Performances_MeetId]
    ON [dbo].[Performances]([MeetId] ASC);

CREATE NONCLUSTERED INDEX [IX_Performances_RecordFlags]
    ON [dbo].[Performances]([EventId] ASC, [SchoolRecord] ASC, [PersonalBest] ASC, [SeasonBest] ASC);


-- ============================================================
-- PERFORMANCE ATHLETES
-- Junction table linking relay performances to their athletes.
-- A relay Performance row has AthleteId = NULL; all team
-- members are stored here instead.
-- ============================================================
CREATE TABLE [dbo].[PerformanceAthletes] (
    [Id]            INT IDENTITY (1, 1) NOT NULL,
    [PerformanceId] INT NOT NULL,
    [AthleteId]     INT NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PerformanceAthletes_Performances] FOREIGN KEY ([PerformanceId]) REFERENCES [dbo].[Performances] ([Id]),
    CONSTRAINT [FK_PerformanceAthletes_Athletes]     FOREIGN KEY ([AthleteId])     REFERENCES [dbo].[Athletes]    ([Id])
);

CREATE NONCLUSTERED INDEX [IX_PerformanceAthletes_PerformanceId]
    ON [dbo].[PerformanceAthletes]([PerformanceId] ASC);

CREATE NONCLUSTERED INDEX [IX_PerformanceAthletes_AthleteId]
    ON [dbo].[PerformanceAthletes]([AthleteId] ASC);


-- ============================================================
-- LEADERBOARDS
-- All-time top-10 rankings per event, rebuilt from scratch by
-- sp_RebuildLeaderboards after every performance change.
-- Rank = 1 is used by the application as the school record
-- proxy for relay performances (since SchoolRecord flag on
-- relay Performance rows is not reliably set).
-- ============================================================
CREATE TABLE [dbo].[Leaderboards] (
    [Id]            INT      IDENTITY (1, 1) NOT NULL,
    [EventId]       INT      NOT NULL,
    [SeasonId]      INT      NOT NULL,
    [Gender]        SMALLINT NOT NULL,
    [Environment]   SMALLINT NOT NULL,
    [PerformanceId] INT      NOT NULL,
    [Rank]          INT      NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Leaderboards_Events]       FOREIGN KEY ([EventId])       REFERENCES [dbo].[Events]       ([Id]),
    CONSTRAINT [FK_Leaderboards_Seasons]      FOREIGN KEY ([SeasonId])      REFERENCES [dbo].[Seasons]      ([Id]),
    CONSTRAINT [FK_Leaderboards_Performances] FOREIGN KEY ([PerformanceId]) REFERENCES [dbo].[Performances] ([Id])
);


-- ============================================================
-- PERSONAL BEST HISTORY
-- Audit trail of when each athlete achieved a personal best
-- for each event. Not the same as the PersonalBest flag on
-- Performances (which marks the current best only).
-- ============================================================
CREATE TABLE [dbo].[PersonalBestHistory] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [AthleteId]     INT           NOT NULL,
    [EventId]       INT           NOT NULL,
    [PerformanceId] INT           NOT NULL,
    [AchievedDate]  DATETIME2 (3) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_PersonalBestHistory_Athlete_Event_Performance]
        UNIQUE NONCLUSTERED ([AthleteId] ASC, [EventId] ASC, [PerformanceId] ASC),
    CONSTRAINT [FK_PersonalBestHistory_Athletes]     FOREIGN KEY ([AthleteId])     REFERENCES [dbo].[Athletes]    ([Id]),
    CONSTRAINT [FK_PersonalBestHistory_Events]       FOREIGN KEY ([EventId])       REFERENCES [dbo].[Events]      ([Id]),
    CONSTRAINT [FK_PersonalBestHistory_Performances] FOREIGN KEY ([PerformanceId]) REFERENCES [dbo].[Performances]([Id])
);


-- ============================================================
-- sp_RebuildLeaderboards
-- Called after every Performance insert, update, or delete.
-- Rebuilds the entire Leaderboards table from scratch.
-- Also resets and recalculates PersonalBest, SeasonBest, and
-- SchoolRecord flags on Performances.
-- NOTE: PersonalBest, SeasonBest, and SchoolRecord are only set
--       for individual performances (AthleteId IS NOT NULL).
--       Relay performances keep these flags at 0; the app uses
--       AllTimeRank = 1 from Leaderboards as the school record
--       proxy for relay rows.
-- ============================================================
CREATE PROCEDURE [dbo].[sp_RebuildLeaderboards]
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    BEGIN TRY
        PRINT 'Starting Leaderboards rebuild...';
        PRINT 'Timestamp: ' + CONVERT(VARCHAR, GETDATE(), 120);

        -- Step 1: Clear existing leaderboards
        TRUNCATE TABLE Leaderboards;

        -- Step 2: Reset all PersonalBest and SeasonBest flags
        UPDATE Performances
        SET PersonalBest = 0, SeasonBest = 0;

        -- Step 3: PersonalBest for DISTANCE events (individual only — AthleteId IS NOT NULL)
        WITH BestDistancePerformances AS (
            SELECT
                p.Id AS PerformanceId,
                RANK() OVER (
                    PARTITION BY p.AthleteId, p.EventId
                    ORDER BY p.DistanceInches DESC
                ) AS PerformanceRank
            FROM Performances p
            INNER JOIN Events e ON e.Id = p.EventId
            WHERE e.EventType IN (0, 2, 4, 5)   -- Field, FieldRelay, JumpRelay, ThrowsRelay
              AND p.DistanceInches IS NOT NULL
              AND p.AthleteId IS NOT NULL
        )
        UPDATE p
        SET p.PersonalBest = 1
        FROM Performances p
        INNER JOIN BestDistancePerformances bdp ON bdp.PerformanceId = p.Id
        WHERE bdp.PerformanceRank = 1;

        -- Step 4: PersonalBest for TIME events (individual only)
        WITH BestTimePerformances AS (
            SELECT
                p.Id AS PerformanceId,
                RANK() OVER (
                    PARTITION BY p.AthleteId, p.EventId
                    ORDER BY p.TimeSeconds ASC
                ) AS PerformanceRank
            FROM Performances p
            INNER JOIN Events e ON e.Id = p.EventId
            WHERE e.EventType IN (1, 3)   -- Running, RunningRelay
              AND p.TimeSeconds IS NOT NULL
              AND p.AthleteId IS NOT NULL
        )
        UPDATE p
        SET p.PersonalBest = 1
        FROM Performances p
        INNER JOIN BestTimePerformances btp ON btp.PerformanceId = p.Id
        WHERE btp.PerformanceRank = 1;

        -- Step 5: SeasonBest for DISTANCE events (individual only)
        WITH SeasonBestDistancePerformances AS (
            SELECT
                p.Id AS PerformanceId,
                RANK() OVER (
                    PARTITION BY p.AthleteId, p.EventId, m.SeasonId
                    ORDER BY p.DistanceInches DESC
                ) AS PerformanceRank
            FROM Performances p
            INNER JOIN Events e ON e.Id = p.EventId
            INNER JOIN Meets m ON m.Id = p.MeetId
            WHERE e.EventType IN (0, 2, 4, 5)
              AND p.DistanceInches IS NOT NULL
              AND p.AthleteId IS NOT NULL
        )
        UPDATE p
        SET p.SeasonBest = 1
        FROM Performances p
        INNER JOIN SeasonBestDistancePerformances sbdp ON sbdp.PerformanceId = p.Id
        WHERE sbdp.PerformanceRank = 1;

        -- Step 6: SeasonBest for TIME events (individual only)
        WITH SeasonBestTimePerformances AS (
            SELECT
                p.Id AS PerformanceId,
                RANK() OVER (
                    PARTITION BY p.AthleteId, p.EventId, m.SeasonId
                    ORDER BY p.TimeSeconds ASC
                ) AS PerformanceRank
            FROM Performances p
            INNER JOIN Events e ON e.Id = p.EventId
            INNER JOIN Meets m ON m.Id = p.MeetId
            WHERE e.EventType IN (1, 3)
              AND p.TimeSeconds IS NOT NULL
              AND p.AthleteId IS NOT NULL
        )
        UPDATE p
        SET p.SeasonBest = 1
        FROM Performances p
        INNER JOIN SeasonBestTimePerformances sbtp ON sbtp.PerformanceId = p.Id
        WHERE sbtp.PerformanceRank = 1;

        -- Step 7: Reset SchoolRecord flag on all Performances
        UPDATE Performances SET SchoolRecord = 0;

        -- Step 8: Set SchoolRecord = 1 for individual performances ranked #1 all-time
        --         Relay performances keep SchoolRecord = 0; the app uses AllTimeRank = 1
        --         from Leaderboards as the school record proxy for relay rows instead.
        UPDATE p
        SET p.SchoolRecord = 1
        FROM Performances p
        INNER JOIN Leaderboards lb ON lb.PerformanceId = p.Id
        WHERE lb.Rank = 1
          AND p.AthleteId IS NOT NULL;

        -- Step 9: Top 10 for DISTANCE-based events (includes relay — no AthleteId filter here)
        WITH BestAthletePerformances AS (
            SELECT
                p.Id AS PerformanceId,
                p.EventId,
                ROW_NUMBER() OVER (
                    PARTITION BY p.EventId, COALESCE(p.AthleteId, -1)
                    ORDER BY p.DistanceInches DESC
                ) AS AthleteRank
            FROM Performances p
            INNER JOIN Events e ON e.Id = p.EventId
            WHERE e.EventType IN (0, 2, 4, 5)
              AND p.DistanceInches IS NOT NULL
        ),
        RankedDistancePerformances AS (
            SELECT
                e.Id AS EventId,
                s.Id AS SeasonId,
                e.Gender,
                e.Environment,
                p.Id AS PerformanceId,
                RANK() OVER (
                    PARTITION BY e.Id, e.Gender, e.Environment
                    ORDER BY p.DistanceInches DESC
                ) AS Rank
            FROM BestAthletePerformances bap
            INNER JOIN Performances p ON p.Id = bap.PerformanceId
            INNER JOIN Events e ON e.Id = p.EventId
            INNER JOIN Meets m ON m.Id = p.MeetId
            INNER JOIN Seasons s ON s.Id = m.SeasonId
            WHERE bap.AthleteRank = 1
        )
        INSERT INTO Leaderboards (EventId, SeasonId, Gender, Environment, PerformanceId, Rank)
        SELECT EventId, SeasonId, Gender, Environment, PerformanceId, Rank
        FROM RankedDistancePerformances
        WHERE Rank <= 10;

        -- Step 10: Top 10 for TIME-based events (includes relay — no AthleteId filter here)
        WITH BestAthletePerformances AS (
            SELECT
                p.Id AS PerformanceId,
                p.EventId,
                ROW_NUMBER() OVER (
                    PARTITION BY p.EventId, COALESCE(p.AthleteId, -1)
                    ORDER BY p.TimeSeconds ASC
                ) AS AthleteRank
            FROM Performances p
            INNER JOIN Events e ON e.Id = p.EventId
            WHERE e.EventType IN (1, 3)
              AND p.TimeSeconds IS NOT NULL
        ),
        RankedTimePerformances AS (
            SELECT
                e.Id AS EventId,
                s.Id AS SeasonId,
                e.Gender,
                e.Environment,
                p.Id AS PerformanceId,
                RANK() OVER (
                    PARTITION BY e.Id, e.Gender, e.Environment
                    ORDER BY p.TimeSeconds ASC
                ) AS Rank
            FROM BestAthletePerformances bap
            INNER JOIN Performances p ON p.Id = bap.PerformanceId
            INNER JOIN Events e ON e.Id = p.EventId
            INNER JOIN Meets m ON m.Id = p.MeetId
            INNER JOIN Seasons s ON s.Id = m.SeasonId
            WHERE bap.AthleteRank = 1
        )
        INSERT INTO Leaderboards (EventId, SeasonId, Gender, Environment, PerformanceId, Rank)
        SELECT EventId, SeasonId, Gender, Environment, PerformanceId, Rank
        FROM RankedTimePerformances
        WHERE Rank <= 10;

        COMMIT TRANSACTION;
        PRINT 'Leaderboards rebuild completed successfully.';

    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage  NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT            = ERROR_SEVERITY();
        DECLARE @ErrorState    INT            = ERROR_STATE();

        PRINT 'ERROR: ' + @ErrorMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH;
END;
