-- Adds AthleteSummaries: AI-written athlete career recaps, reviewed in /Admin/AthleteSummaries.
-- Purely additive; nothing public reads it yet.
IF OBJECT_ID('dbo.AthleteSummaries', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AthleteSummaries] (
        [Id]          INT            IDENTITY (1, 1) NOT NULL,
        [AthleteId]   INT            NOT NULL,
        [SummaryText] NVARCHAR (MAX) NOT NULL,
        [InputHash]   VARCHAR (64)   NOT NULL,   -- SHA-256 hex of fact sheet + prompt version
        [Model]       VARCHAR (64)   NOT NULL,
        [Status]      SMALLINT       NOT NULL,   -- 1 = Draft, 2 = Approved (SummaryStatus enum)
        [GeneratedAt] DATETIME2 (7)  NOT NULL CONSTRAINT [DF_AthleteSummaries_GeneratedAt] DEFAULT (GETUTCDATE()),
        [ReviewedAt]  DATETIME2 (7)  NULL,
        PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_AthleteSummaries_Athletes] FOREIGN KEY ([AthleteId]) REFERENCES [dbo].[Athletes] ([Id]),
        CONSTRAINT [UQ_AthleteSummaries_AthleteId] UNIQUE NONCLUSTERED ([AthleteId])
    );
END
