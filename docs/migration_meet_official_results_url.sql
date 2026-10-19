-- Adds Meets.OfficialResultsUrl: link to the meet's official results when they are hosted externally
-- (athletic.net, oh.milesplit.com, a timing company, ...). Nullable; run BEFORE deploying the code that reads/writes it.
IF COL_LENGTH('dbo.Meets', 'OfficialResultsUrl') IS NULL
    ALTER TABLE dbo.Meets ADD OfficialResultsUrl NVARCHAR(500) NULL;
GO
