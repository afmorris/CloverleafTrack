-- Allow 'X' and 'XX' as bar outcomes (BRAIN.md [C88]).
-- A vaulter/jumper who passes the third attempt at a height (XXP) and then misses the next bar once is out on cumulative misses, so the last bar's
-- ladder entry is a lone 'X' (or 'XX'): fewer than three misses at that height, but still the end of the competition. The ladder view already renders
-- any outcome that is not a clear or a pass as a miss, so no code change is needed.
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_PerformanceBars_Outcome')
    ALTER TABLE [dbo].[PerformanceBars] DROP CONSTRAINT [CK_PerformanceBars_Outcome];
ALTER TABLE [dbo].[PerformanceBars] ADD CONSTRAINT [CK_PerformanceBars_Outcome]
    CHECK ([Outcome] IN ('O', 'XO', 'XXO', 'XXX', 'P', 'XP', 'XXP', 'X', 'XX'));
