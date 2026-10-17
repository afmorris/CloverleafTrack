-- Adds MeetSummaries: AI-generated meet recaps, reviewed in /Admin/MeetSummaries before any public use.
-- Purely additive; no existing table or code path reads it except the admin review page.
IF OBJECT_ID('dbo.MeetSummaries', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[MeetSummaries] (
        [Id]          INT            IDENTITY (1, 1) NOT NULL,
        [MeetId]      INT            NOT NULL,
        [SummaryText] NVARCHAR (MAX) NOT NULL,
        [InputHash]   VARCHAR (64)   NOT NULL,   -- SHA-256 hex of fact sheet + prompt version
        [Model]       VARCHAR (64)   NOT NULL,
        [Status]      SMALLINT       NOT NULL,   -- 1 = Draft, 2 = Approved (SummaryStatus enum)
        [GeneratedAt] DATETIME2 (7)  NOT NULL CONSTRAINT [DF_MeetSummaries_GeneratedAt] DEFAULT (GETUTCDATE()),
        [ReviewedAt]  DATETIME2 (7)  NULL,
        PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_MeetSummaries_Meets] FOREIGN KEY ([MeetId]) REFERENCES [dbo].[Meets] ([Id]),
        CONSTRAINT [UQ_MeetSummaries_MeetId] UNIQUE NONCLUSTERED ([MeetId])
    );
END
