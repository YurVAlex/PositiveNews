/* ==========================================================================
   POSITIVE NEWS DATABASE - INITIAL SETUP
   ==========================================================================
   Description: 
   Creates the schema for the PositiveNews aggregator MVP.
   Includes Vertical Partitioning for Articles, Unified Audit Logging,
   and preparation for future Social Auth.
   ========================================================================== */

/* --------------------------------------------------------------------------
   1. SCHEMAS
   -------------------------------------------------------------------------- */
-- Organizing tables by domain context.
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Admin') EXEC('CREATE SCHEMA [Admin]')
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Catalog') EXEC('CREATE SCHEMA [Catalog]')
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Community') EXEC('CREATE SCHEMA [Community]')
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Identity') EXEC('CREATE SCHEMA [Identity]')
GO

/* --------------------------------------------------------------------------
   2. INDEPENDENT TABLES (Dictionaries & Core Entities)
   -------------------------------------------------------------------------- */

-- [Identity].[Roles]
-- Simple lookup for permissions.
CREATE TABLE [Identity].[Roles](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] [nvarchar](50) NOT NULL UNIQUE -- Enum-like (e.g., 'Admin', 'User', 'Moderator')
);
GO

-- [Catalog].[Topics]
-- The categories articles belong to (e.g., 'Technology', 'Health').
CREATE TABLE [Catalog].[Topics](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] [nvarchar](200) NOT NULL,
    [Slug] [nvarchar](200) NOT NULL UNIQUE, -- For pretty URLs (e.g., /topic/clean-energy)
    [Description] [nvarchar](500) NULL
);
GO

-- [Identity].[Users]
-- The core user identity table.
CREATE TABLE [Identity].[Users](
    [Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Email] [nvarchar](300) NOT NULL UNIQUE,
    [EmailConfirmed] [bit] NOT NULL DEFAULT 0, -- Critical for preventing bot spam in comments.
    [Name] [nvarchar](200) NOT NULL,
    
    -- SECURITY NOTE: Nullable to support future Social Auth (Google/Apple).
    
    [PasswordHash] [nvarchar](max) NULL, 
    [LastLoginAt] [datetime2](7) NULL,
    [FailedLoginCount] [int] NOT NULL DEFAULT 0, -- Security: Used to lock account after too many tries.
    [AvatarPictureUrl] [nvarchar](1000) NULL,
    [CreatedAt] [datetime2](7) NOT NULL DEFAULT sysutcdatetime(),
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [ModeratedBy] [bigint] NULL -- Self-referencing FK (defined later) to track who banned/approved this user.
);
GO

/* --------------------------------------------------------------------------
   3. CATALOG & CONTENT TABLES
   -------------------------------------------------------------------------- */

-- [Catalog].[Sources]
-- Represents RSS feeds or APIs (e.g., Reuters, BBC).
CREATE TABLE [Catalog].[Sources](
    [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name] [nvarchar](200) NOT NULL,
    [Description] [nvarchar](1000) NULL,
    [BaseUrl] [nvarchar](500) NOT NULL,
    [FeedUrl] [nvarchar](500) NULL,
    [LogoUrl] [nvarchar](500) NULL,
    [ApiEndpoint] [nvarchar](500) NULL,
    
    -- SECURITY: There is no intention to store API keys in clear text. 
    -- Application must encrypt this before saving and decrypt in memory when used.

    [ApiEncryptedKey] [nvarchar](max) NULL, 
    
    [TrustScore] [decimal](5, 2) NOT NULL DEFAULT 1.0, -- Weighting factor for the algorithm.
    [DefaultLanguageCode] [nvarchar](10) NOT NULL DEFAULT 'en', -- Template for incoming articles.
    [CreatedAt] [datetime2](7) NOT NULL DEFAULT sysutcdatetime(),
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [ModeratedBy] [bigint] NULL,
    
    CONSTRAINT [FK_Sources_Moderator] FOREIGN KEY([ModeratedBy]) REFERENCES [Identity].[Users]([Id])
);
GO

-- [Catalog].[ArticlesMetadata]
-- The "Light" table. Contains only searchable fields.
-- Vertical Partitioning Strategy: Keeping this small makes feed queries fast.
CREATE TABLE [Catalog].[ArticlesMetadata](
    [Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SourceId] [int] NOT NULL,
    [ExternalId] [nvarchar](300) NULL, -- The ID provided by the Source (RSS GUID).
    [Title] [nvarchar](500) NOT NULL,
    [Author] [nvarchar](300) NULL,
    [Url] [nvarchar](1000) NOT NULL,
    [ImageUrl] [nvarchar](1000) NULL,
    
    -- TIMESTAMPS: PublishedAt is NOT NULL because sorting by "Unknown Date" breaks the feed UI.
    [PublishedAt] [datetime2](7) NOT NULL DEFAULT sysutcdatetime(), 
    [IngestedAt] [datetime2](7) NOT NULL DEFAULT sysutcdatetime(),
    
    -- ANALYSIS: Nullable because analysis happens asynchronously after ingestion.
    [AnalyzedAt] [datetime2](7) NULL, 
    [PositivityScore] [decimal](5, 4) NULL, -- 0.0000 to 1.0000
    
    [ViewCount] [bigint] NOT NULL DEFAULT 0,
    
    -- LOCALIZATION: Defaults 'und' (Undefined) and 'Global' allow the Ingestor to save 
    -- the article immediately, even if language detection absent.
    [LanguageCode] [nvarchar](10) NOT NULL DEFAULT 'und', 
    [RegionCode] [nvarchar](10) NOT NULL DEFAULT 'Global',
    
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [ModeratedBy] [bigint] NULL,

    CONSTRAINT [FK_ArticlesMeta_Source] FOREIGN KEY([SourceId]) REFERENCES [Catalog].[Sources]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ArticlesMeta_Moderator] FOREIGN KEY([ModeratedBy]) REFERENCES [Identity].[Users]([Id])
);
GO

-- [Catalog].[ArticlesContent]
-- The "Heavy" table. Contains blobs of text. 
-- Split from Metadata to reduce IO usage when listing articles.
CREATE TABLE [Catalog].[ArticlesContent](
    [Id] [bigint] NOT NULL PRIMARY KEY, -- 1-to-1 Relationship with Metadata
    
    -- NULLABLE JUSTIFICATION:
    -- 1. Ingestion saves Metadata first (Content is NULL).
    -- 2. Background worker scrapes the URL (ContentRaw populated).
    -- 3. AI cleans the text (ContentClean populated).
    -- If these were NOT NULL, the initial ingestion would fail.
    [ContentRaw] [nvarchar](max) NULL,
    [ContentClean] [nvarchar](max) NULL,
    [SummaryShort] [nvarchar](2000) NULL,

    CONSTRAINT [FK_ArticlesContent_Meta] FOREIGN KEY([Id]) REFERENCES [Catalog].[ArticlesMetadata]([Id]) ON DELETE CASCADE
);
GO

-- [Catalog].[IngestionRuns]
-- Logs the execution of the background scraper.
CREATE TABLE [Catalog].[IngestionRuns](
    [Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SourceId] [int] NOT NULL,
    [StartedAt] [datetime2](7) NOT NULL,
    [FinishedAt] [datetime2](7) NULL,
    [Status] [nvarchar](50) NOT NULL, -- Enum stored as String: 'Running', 'Success', 'Failed'
    [ItemsFetched] [int] NOT NULL DEFAULT 0,
    [ErrorMessage] [nvarchar](max) NULL,

    CONSTRAINT [FK_IngestionRuns_Source] FOREIGN KEY([SourceId]) REFERENCES [Catalog].[Sources]([Id]) ON DELETE CASCADE
);
GO

/* --------------------------------------------------------------------------
   4. COMMUNITY & INTERACTION TABLES
   -------------------------------------------------------------------------- */

-- [Community].[Comments]
CREATE TABLE [Community].[Comments](
    [Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ArticleId] [bigint] NOT NULL,
    [UserId] [bigint] NOT NULL,
    [ParentId] [bigint] NULL, -- For threaded replies.
    [Content] [nvarchar](2000) NOT NULL, -- Security: Prevent "Text Bloat" attacks.
    [CreatedAt] [datetime2](7) NOT NULL DEFAULT sysutcdatetime(),
    [EditedAt] [datetime2](7) NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [ModeratedBy] [bigint] NULL,

    CONSTRAINT [FK_Comments_Article] FOREIGN KEY([ArticleId]) REFERENCES [Catalog].[ArticlesMetadata]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Comments_User] FOREIGN KEY([UserId]) REFERENCES [Identity].[Users]([Id]), 
    CONSTRAINT [FK_Comments_Parent] FOREIGN KEY([ParentId]) REFERENCES [Community].[Comments]([Id]), 
    CONSTRAINT [FK_Comments_Moderator] FOREIGN KEY([ModeratedBy]) REFERENCES [Identity].[Users]([Id])
);
GO

/* --------------------------------------------------------------------------
   5. JUNCTION TABLES (Relationships)
   -------------------------------------------------------------------------- */

-- [Identity].[UserRoles] (Many-to-Many)
CREATE TABLE [Identity].[UserRoles](
    [UserId] [bigint] NOT NULL,
    [RoleId] [int] NOT NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),

    CONSTRAINT [FK_UserRoles_User] FOREIGN KEY([UserId]) REFERENCES [Identity].[Users]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Role] FOREIGN KEY([RoleId]) REFERENCES [Identity].[Roles]([Id]) ON DELETE CASCADE
);
GO

-- [Catalog].[ArticleTopics] (Many-to-Many)
CREATE TABLE [Catalog].[ArticleTopics](
    [ArticleId] [bigint] NOT NULL,
    [TopicId] [int] NOT NULL,
    PRIMARY KEY CLUSTERED ([ArticleId] ASC, [TopicId] ASC),
    
    CONSTRAINT [FK_ArticleTopics_Article] FOREIGN KEY([ArticleId]) REFERENCES [Catalog].[ArticlesMetadata]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ArticleTopics_Topic] FOREIGN KEY([TopicId]) REFERENCES [Catalog].[Topics]([Id]) ON DELETE CASCADE
);
GO

-- [Identity].[UserSourceFilters] (Many-to-Many)
-- Users can "Mute" or "Follow" specific sources.
CREATE TABLE [Identity].[UserSourceFilters](
    [UserId] [bigint] NOT NULL,
    [SourceId] [int] NOT NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC, [SourceId] ASC),

    CONSTRAINT [FK_UserSourceFilters_User] FOREIGN KEY([UserId]) REFERENCES [Identity].[Users]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserSourceFilters_Source] FOREIGN KEY([SourceId]) REFERENCES [Catalog].[Sources]([Id]) ON DELETE CASCADE
);
GO

-- [Identity].[UserTopicFilters] (Many-to-Many)
CREATE TABLE [Identity].[UserTopicFilters](
    [UserId] [bigint] NOT NULL,
    [TopicId] [int] NOT NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC, [TopicId] ASC),

    CONSTRAINT [FK_UserTopicFilters_User] FOREIGN KEY([UserId]) REFERENCES [Identity].[Users]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserTopicFilters_Topic] FOREIGN KEY([TopicId]) REFERENCES [Catalog].[Topics]([Id]) ON DELETE CASCADE
);
GO

/* --------------------------------------------------------------------------
   6. USER PREFERENCES
   -------------------------------------------------------------------------- */

-- [Identity].[UserFeedPreferences]
-- Stores the personalization settings for the user's feed.
CREATE TABLE [Identity].[UserFeedPreferences](
    [UserId] [bigint] NOT NULL PRIMARY KEY,
    [MinPositivity] [decimal](3, 2) NOT NULL DEFAULT 0.5, -- The core feature: Filter out negative news.
    [SortBy] [nvarchar](50) NOT NULL DEFAULT 'Date', -- Enum: 'Date', 'Popularity', 'Positivity'
    [LanguageCode] [nvarchar](10) NULL, -- The language filter may be missing or none.
    [RegionCode] [nvarchar](10) NULL, -- May be missing or none.

    CONSTRAINT [FK_UserPrefs_User] FOREIGN KEY([UserId]) REFERENCES [Identity].[Users]([Id]) ON DELETE CASCADE
);
GO

/* --------------------------------------------------------------------------
   7. ADMIN & AUDITING
   -------------------------------------------------------------------------- */

-- [Admin].[AuditLogs]
-- Unified table for all moderation actions (Banning users, Deleting comments, etc.)
CREATE TABLE [Admin].[AuditLogs](
    [Id] [bigint] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [EntityType] [nvarchar](50) NOT NULL, -- Enum: 'Article', 'Comment', 'User'
    [EntityId] [bigint] NOT NULL,         -- Stores the ID of the object being modified.
    [ChangedField] [nvarchar](100) NULL,  -- e.g., 'IsActive', 'Title', 'SummaryShort'
    [OldValue] [nvarchar](max) NULL,
    [NewValue] [nvarchar](max) NULL,
    [Reason] [nvarchar](500) NULL,        -- Why was this done? (e.g., 'Hate Speech')
    [Note] [nvarchar](1000) NULL,         -- Moderator internal notes.
    [ModeratorId] [bigint] NOT NULL,
    [CreatedAt] [datetime2](7) NOT NULL DEFAULT sysutcdatetime(),

    CONSTRAINT [FK_AuditLogs_Moderator] FOREIGN KEY([ModeratorId]) REFERENCES [Identity].[Users]([Id])
);
GO

/* --------------------------------------------------------------------------
   8. FINAL CONSTRAINTS
   -------------------------------------------------------------------------- */
ALTER TABLE [Identity].[Users] 
ADD CONSTRAINT [FK_Users_ModeratedBy] FOREIGN KEY([ModeratedBy]) REFERENCES [Identity].[Users]([Id]);
GO

/* --------------------------------------------------------------------------
   9. PERFORMANCE INDEXES
   -------------------------------------------------------------------------- */

-- FEED PERFORMANCE: "Show me the latest positive news"
-- Includes 'PositivityScore' to filter negative news instantly without table lookups.
CREATE NONCLUSTERED INDEX [IX_ArticlesMeta_Feed_Date] 
ON [Catalog].[ArticlesMetadata] ([LanguageCode], [RegionCode], [PublishedAt] DESC)
INCLUDE ([Title], [ImageUrl], [SourceId], [PositivityScore], [ViewCount])
WHERE [IsActive] = 1;
GO

-- Optimization for "Source Profile" pages (e.g., /source/reuters).
CREATE NONCLUSTERED INDEX [IX_ArticlesMeta_SourceId]
ON [Catalog].[ArticlesMetadata] ([SourceId])
INCLUDE ([PublishedAt], [IsActive]); -- Include IsActive to filter deleted items quickly.
GO

-- TOPIC LOOKUP: "Show me all articles about Technology"
-- Essential because the PK is (ArticleId, TopicId). This creates the reverse lookup.
CREATE NONCLUSTERED INDEX [IX_ArticleTopics_Topic_Lookup] 
ON [Catalog].[ArticleTopics] ([TopicId])
INCLUDE ([ArticleId]);
GO

-- DEDUPLICATION: "Does this article already exist?"
-- Used by the Ingestion Engine to prevent inserting duplicates.
CREATE UNIQUE NONCLUSTERED INDEX [IX_ArticlesMeta_Source_ExternalId] 
ON [Catalog].[ArticlesMetadata] ([SourceId], [ExternalId])
WHERE [ExternalId] IS NOT NULL;
GO

-- HISTORY: "Show me audit logs for this specific article"
CREATE NONCLUSTERED INDEX [IX_AuditLogs_Entity_History] 
ON [Admin].[AuditLogs] ([EntityType], [EntityId], [CreatedAt] DESC);
GO

-- COMMENTS: "Load comments for this article"
CREATE NONCLUSTERED INDEX [IX_Comments_Article_Thread] 
ON [Community].[Comments] ([ArticleId], [CreatedAt] ASC)
INCLUDE ([UserId], [Content])
WHERE [IsActive] = 1;
GO

-- 10. ENFORCE DATA INTEGRITY (Check Constraints)
-- Prevents "Impossible" numbers from breaking the algorithm.

ALTER TABLE [Catalog].[ArticlesMetadata]
ADD CONSTRAINT [CK_Articles_Positivity] CHECK ([PositivityScore] BETWEEN 0.0000 AND 1.0000);
GO

ALTER TABLE [Catalog].[Sources]
ADD CONSTRAINT [CK_Sources_Trust] CHECK ([TrustScore] >= 0.00);
GO

ALTER TABLE [Identity].[UserFeedPreferences]
ADD CONSTRAINT [CK_UserPrefs_MinPositivity] CHECK ([MinPositivity] BETWEEN 0.00 AND 1.00);
GO

-- 11. ENFORCE ENUM INTEGRITY (Without Lookup Tables)
-- Ensures only valid strings are saved.

ALTER TABLE [Catalog].[IngestionRuns]
ADD CONSTRAINT [CK_Ingestion_Status] CHECK ([Status] IN ('Running', 'Success', 'Failed', 'Partial'));
GO

ALTER TABLE [Admin].[AuditLogs]
ADD CONSTRAINT [CK_Audit_Entity] CHECK ([EntityType] IN ('Article', 'Comment', 'User', 'Source'));
-- To be supplemented...
GO
