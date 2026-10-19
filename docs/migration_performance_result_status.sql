-- Adds Performances.ResultStatus so a competitor with NO mark can be recorded: NM (fouled out), NH (no height),
-- DNF, DQ. A no-mark row has BOTH TimeSeconds and DistanceInches NULL; ResultStatus = 0 (a normal mark) is unchanged.
-- sp_RebuildLeaderboards already filters every ranking / PB / SB / percentile / statistics step on
-- "<mark> IS NOT NULL", so no-mark rows are excluded from all of them without touching the procedure.
-- Run BEFORE deploying code that reads it.
IF COL_LENGTH('dbo.Performances', 'ResultStatus') IS NULL
    ALTER TABLE [dbo].[Performances] ADD [ResultStatus] TINYINT NOT NULL
        CONSTRAINT [DF_Performances_ResultStatus] DEFAULT (0);
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Performances_DistanceOrTime')
    ALTER TABLE [dbo].[Performances] DROP CONSTRAINT [CK_Performances_DistanceOrTime];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Performances_MarkOrStatus')
    ALTER TABLE [dbo].[Performances] WITH CHECK ADD CONSTRAINT [CK_Performances_MarkOrStatus]
        CHECK (
            ([ResultStatus] = 0 AND (
                ([DistanceInches] IS NOT NULL AND [TimeSeconds] IS NULL)
                OR ([DistanceInches] IS NULL AND [TimeSeconds] IS NOT NULL)))
            OR ([ResultStatus] BETWEEN 1 AND 4 AND [DistanceInches] IS NULL AND [TimeSeconds] IS NULL)
        );
GO
