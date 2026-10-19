-- Adds PerformanceBars: the bar-by-bar ladder (O / XO / XXO / XXX / P ...) for high jump and pole vault.
-- Additive and display-only, like PerformanceAttempts/PerformanceRounds: Performances.DistanceInches stays the
-- best cleared height, so leaderboards, PRs, percentiles and scoring are unchanged. Run BEFORE deploying code that reads it.
IF OBJECT_ID('dbo.PerformanceBars', 'U') IS NULL
CREATE TABLE [dbo].[PerformanceBars] (
    [Id]            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [PerformanceId] INT         NOT NULL,
    [HeightInches]  FLOAT(53)   NOT NULL,
    [Outcome]       VARCHAR(3)  NOT NULL,   -- attempts at this height in order: X miss, O clear, P pass
    CONSTRAINT [FK_PerformanceBars_Performances]
        FOREIGN KEY ([PerformanceId]) REFERENCES [dbo].[Performances]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_PerformanceBars_Performance_Height] UNIQUE ([PerformanceId], [HeightInches]),
    CONSTRAINT [CK_PerformanceBars_Outcome] CHECK ([Outcome] IN ('O', 'XO', 'XXO', 'XXX', 'P', 'XP', 'XXP'))
);
GO
