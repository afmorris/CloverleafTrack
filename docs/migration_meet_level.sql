-- Adds Meets.MeetLevel (1 = Varsity, 2 = JV). Run BEFORE deploying the code that reads/writes it.
-- Existing rows default to Varsity; the backfill below flags the known JV-only meets.
IF COL_LENGTH('dbo.Meets', 'MeetLevel') IS NULL
    ALTER TABLE dbo.Meets ADD MeetLevel SMALLINT NOT NULL CONSTRAINT DF_Meets_MeetLevel DEFAULT (1);
GO

UPDATE dbo.Meets SET MeetLevel = 2
WHERE Name LIKE '%Smithie Frosh-Soph%' OR Name LIKE '%Stars of Track and Field%';
-- Review: SELECT Id, Name, Date FROM dbo.Meets WHERE MeetLevel = 2 ORDER BY Date;
