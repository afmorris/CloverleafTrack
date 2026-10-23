-- Adds SeasonSummaries: the optional written "season in words" on /seasons/{name}/wrap (BRAIN.md [C124]).
-- Written in chat by an Opus subagent, imported as Draft, approved in /Admin/SeasonSummaries; only Approved text renders publicly.
-- Purely additive; only the wrap page and the admin review page read it.
IF OBJECT_ID('dbo.SeasonSummaries', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SeasonSummaries] (
        [Id]          INT            IDENTITY (1, 1) NOT NULL,
        [SeasonId]    INT            NOT NULL,
        [SummaryText] NVARCHAR (MAX) NOT NULL,
        [InputHash]   VARCHAR (64)   NOT NULL,   -- SHA-256 hex of fact sheet + prompt version
        [Model]       VARCHAR (64)   NOT NULL,
        [Status]      SMALLINT       NOT NULL,   -- 1 = Draft, 2 = Approved (SummaryStatus enum)
        [GeneratedAt] DATETIME2 (7)  NOT NULL CONSTRAINT [DF_SeasonSummaries_GeneratedAt] DEFAULT (GETUTCDATE()),
        [ReviewedAt]  DATETIME2 (7)  NULL,
        PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_SeasonSummaries_Seasons] FOREIGN KEY ([SeasonId]) REFERENCES [dbo].[Seasons] ([Id]),
        CONSTRAINT [UQ_SeasonSummaries_SeasonId] UNIQUE NONCLUSTERED ([SeasonId])
    );
END
