-- Adds PerformanceRounds: per-round marks (Prelim / Semifinal / Final) for running and relay
-- performances. Additive, like PerformanceAttempts: a Performance NEVER requires round rows,
-- and Performances.TimeSeconds stays the single best (min) mark, so Leaderboards, percentiles,
-- PR/SB flags and scoring are unchanged. Run BEFORE deploying the code that reads/writes it.
-- Field-event prelim/final attempts are stored in PerformanceAttempts (AttemptNumber has no upper CHECK, so >6 attempts are allowed).
IF OBJECT_ID('dbo.PerformanceRounds', 'U') IS NULL
CREATE TABLE [dbo].[PerformanceRounds] (
    [Id]            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [PerformanceId] INT        NOT NULL,
    [Round]         TINYINT    NOT NULL,      -- 1 = Prelim, 2 = Semifinal, 3 = Final
    [TimeSeconds]   FLOAT(53)  NOT NULL,
    CONSTRAINT [FK_PerformanceRounds_Performances]
        FOREIGN KEY ([PerformanceId]) REFERENCES [dbo].[Performances]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_PerformanceRounds_Performance_Round] UNIQUE ([PerformanceId], [Round]),
    CONSTRAINT [CK_PerformanceRounds_Round] CHECK ([Round] BETWEEN 1 AND 3)
);
GO
