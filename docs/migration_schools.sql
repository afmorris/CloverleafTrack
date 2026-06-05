-- ============================================================
-- CloverleafTrack — Schools Rearchitecture Migration
--
-- Migrates team score/place data from Meets columns and
-- free-text SchoolName from MeetParticipants into a persistent
-- Schools lookup table with a new MeetTeamResults table.
--
-- RUN IN THREE PHASES:
--   Phase 1: Review distinct school names (read-only)
--   Phase 2: Create new tables and columns (DDL)
--   Phase 3: Migrate data and drop old columns
--
-- Phase 1 must be completed first so you can standardize
-- school names (e.g. against the OHSAA standard list) before
-- the Schools table is populated.
-- ============================================================


-- ============================================================
-- PHASE 1 — Review existing school names (READ-ONLY)
-- ============================================================
-- Run this query and standardize the names before running
-- Phase 2 + 3.  Update any rows in MeetParticipants directly
-- to fix typos or non-standard names before migrating.
-- ============================================================

SELECT
    SchoolName,
    COUNT(DISTINCT MeetId) AS MeetsUsedIn
FROM dbo.MeetParticipants
WHERE Deleted = 0
  AND SchoolName IS NOT NULL
  AND SchoolName != ''
GROUP BY SchoolName
ORDER BY SchoolName;

-- After reviewing, fix any names you want to standardize:
--   UPDATE dbo.MeetParticipants
--   SET SchoolName = 'Correct Name', DateUpdated = GETUTCDATE()
--   WHERE SchoolName = 'Typo Name';


-- ============================================================
-- PHASE 2 — DDL: create new tables and add columns
-- ============================================================
-- Run after you have standardized school names in Phase 1.
-- This phase is safe to run on a live database; it only adds
-- new structures and does not remove anything yet.
-- ============================================================

-- 2a: Schools lookup table
CREATE TABLE [dbo].[Schools] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (200) NOT NULL,
    [ShortName]   NVARCHAR (50)  NULL,
    [DateCreated] DATETIME2 (7)  DEFAULT (GETUTCDATE()) NOT NULL,
    [DateUpdated] DATETIME2 (7)  NULL,
    [Deleted]     BIT            DEFAULT ((0)) NOT NULL,
    [DateDeleted] DATETIME2 (7)  NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

-- 2b: Add SchoolId and Gender to MeetParticipants (nullable for now — filled in Phase 3)
ALTER TABLE [dbo].[MeetParticipants]
    ADD [SchoolId] INT      NULL,
        [Gender]   SMALLINT NULL;   -- NULL = all genders (future: gender-specific invitational fields)

-- 2c: MeetTeamResults table (replaces the 7 score/place columns on Meets)
--     One row per (meet × gender × opponent).
--     OpponentMeetParticipantId is NULL for invitational results (overall place, no specific opponent).
CREATE TABLE [dbo].[MeetTeamResults] (
    [Id]                        INT            IDENTITY (1, 1) NOT NULL,
    [MeetId]                    INT            NOT NULL,
    [Gender]                    SMALLINT       NOT NULL,   -- 1=Male, 2=Female (Gender enum)
    [OpponentMeetParticipantId] INT            NULL,       -- NULL for invitational
    [OurScore]                  DECIMAL (8,2)  NULL,
    [OpponentScore]             DECIMAL (8,2)  NULL,
    [Place]                     INT            NULL,
    [FieldSize]                 INT            NULL,
    [DateCreated]               DATETIME2 (7)  DEFAULT (GETUTCDATE()) NOT NULL,
    [DateUpdated]               DATETIME2 (7)  NULL,
    [Deleted]                   BIT            DEFAULT ((0)) NOT NULL,
    [DateDeleted]               DATETIME2 (7)  NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MeetTeamResults_Meets]
        FOREIGN KEY ([MeetId]) REFERENCES [dbo].[Meets] ([Id]),
    CONSTRAINT [FK_MeetTeamResults_MeetParticipants]
        FOREIGN KEY ([OpponentMeetParticipantId]) REFERENCES [dbo].[MeetParticipants] ([Id])
);

CREATE NONCLUSTERED INDEX [IX_MeetTeamResults_MeetId]
    ON [dbo].[MeetTeamResults]([MeetId] ASC);


-- ============================================================
-- PHASE 3 — Data migration and cleanup
-- ============================================================
-- Run after Phase 2.  Populates Schools, updates FK on
-- MeetParticipants, backfills MeetTeamResults from the old
-- Meets score columns, then drops the old columns.
--
-- Wrap in a transaction so you can roll back if anything
-- looks wrong before committing.
-- ============================================================

BEGIN TRANSACTION;

-- 3a: Populate Schools from all distinct (non-empty) names in MeetParticipants
INSERT INTO [dbo].[Schools] ([Name])
SELECT DISTINCT SchoolName
FROM [dbo].[MeetParticipants]
WHERE Deleted = 0
  AND SchoolName IS NOT NULL
  AND SchoolName != ''
ORDER BY SchoolName;

-- 3b: Set SchoolId on each MeetParticipant row from the Schools table
UPDATE mp
SET mp.SchoolId   = s.Id,
    mp.DateUpdated = GETUTCDATE()
FROM [dbo].[MeetParticipants] mp
INNER JOIN [dbo].[Schools] s ON s.Name = mp.SchoolName
WHERE mp.Deleted = 0;

-- Verify: any rows still missing a SchoolId?
-- (Should return 0 rows before proceeding.)
SELECT COUNT(*) AS MissingSchoolId
FROM [dbo].[MeetParticipants]
WHERE Deleted = 0
  AND SchoolId IS NULL;

-- 3c: Make SchoolId NOT NULL and add the FK constraint
ALTER TABLE [dbo].[MeetParticipants]
    ALTER COLUMN [SchoolId] INT NOT NULL;

ALTER TABLE [dbo].[MeetParticipants]
    ADD CONSTRAINT [FK_MeetParticipants_Schools]
        FOREIGN KEY ([SchoolId]) REFERENCES [dbo].[Schools] ([Id]);

-- 3d: Backfill MeetTeamResults from old Meets score columns
--     for Dual / DoubleDual meets (MeetType 1 or 2)

-- Boys scores (Dual / DoubleDual)
INSERT INTO [dbo].[MeetTeamResults] ([MeetId], [Gender], [OpponentMeetParticipantId], [OurScore], [OpponentScore])
SELECT
    m.Id,
    1,   -- Male
    (SELECT TOP 1 mp.Id
     FROM [dbo].[MeetParticipants] mp
     WHERE mp.MeetId = m.Id AND mp.Deleted = 0
     ORDER BY mp.SortOrder),
    m.BoysScore,
    m.BoysOpponentScore
FROM [dbo].[Meets] m
WHERE m.MeetType IN (1, 2)   -- Dual, DoubleDual
  AND (m.BoysScore IS NOT NULL OR m.BoysOpponentScore IS NOT NULL);

-- Girls scores (Dual / DoubleDual)
INSERT INTO [dbo].[MeetTeamResults] ([MeetId], [Gender], [OpponentMeetParticipantId], [OurScore], [OpponentScore])
SELECT
    m.Id,
    2,   -- Female
    (SELECT TOP 1 mp.Id
     FROM [dbo].[MeetParticipants] mp
     WHERE mp.MeetId = m.Id AND mp.Deleted = 0
     ORDER BY mp.SortOrder),
    m.GirlsScore,
    m.GirlsOpponentScore
FROM [dbo].[Meets] m
WHERE m.MeetType IN (1, 2)
  AND (m.GirlsScore IS NOT NULL OR m.GirlsOpponentScore IS NOT NULL);

-- Boys place (Invitational, MeetType = 3)
INSERT INTO [dbo].[MeetTeamResults] ([MeetId], [Gender], [OpponentMeetParticipantId], [Place], [FieldSize])
SELECT
    m.Id,
    1,   -- Male
    NULL,
    m.BoysPlace,
    m.FieldSize
FROM [dbo].[Meets] m
WHERE m.MeetType = 3
  AND m.BoysPlace IS NOT NULL;

-- Girls place (Invitational, MeetType = 3)
INSERT INTO [dbo].[MeetTeamResults] ([MeetId], [Gender], [OpponentMeetParticipantId], [Place], [FieldSize])
SELECT
    m.Id,
    2,   -- Female
    NULL,
    m.GirlsPlace,
    m.FieldSize
FROM [dbo].[Meets] m
WHERE m.MeetType = 3
  AND m.GirlsPlace IS NOT NULL;

-- 3e: Drop SchoolName from MeetParticipants (now redundant — use Schools.Name via SchoolId FK)
ALTER TABLE [dbo].[MeetParticipants]
    DROP COLUMN [SchoolName];

-- 3f: Drop old score / placement columns from Meets
ALTER TABLE [dbo].[Meets]
    DROP COLUMN [BoysScore],
                [BoysOpponentScore],
                [GirlsScore],
                [GirlsOpponentScore],
                [BoysPlace],
                [GirlsPlace],
                [FieldSize];

-- Review the results, then COMMIT or ROLLBACK:
-- ROLLBACK TRANSACTION;
COMMIT TRANSACTION;
