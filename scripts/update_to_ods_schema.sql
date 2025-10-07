-- Script to migrate existing Emblue tables to the new ods.emblue_* schema expected by the application.
-- The script is idempotent and can be executed multiple times. It targets Microsoft SQL Server.

/*
Summary of changes applied by this script:
    1. Ensure the ods schema exists.
    2. Rename legacy EMB_PRE_* tables (if they are still present) to the new emblue_* names and move them to the ods schema.
    3. Create the ods.emblue_TaskExecutionLog table when it is missing.
    4. Create the ods.emblue_TaskExecutionFile table when it is missing, including the unique index on (FileId, ReportType).
    5. Add the TaskExecutionFileId column and required foreign keys to ods.emblue_DailyActivityDetail and ods.emblue_DailyActionSummary.
    6. Harmonise column definitions with the EF Core model (lengths, nullability and date precisions).

Steps 5 and 6 that tighten nullability perform safety checks before enforcing NOT NULL constraints. If existing rows violate
those constraints the script raises an error so you can fix the data manually before re-running the script.
*/

------------------------------------------------------------
-- 1. Ensure ods schema exists
------------------------------------------------------------
IF SCHEMA_ID(N'ods') IS NULL
BEGIN
    EXEC(N'CREATE SCHEMA ods');
END;
GO

------------------------------------------------------------
-- 2. Rename legacy tables when required
------------------------------------------------------------
IF OBJECT_ID(N'[dbo].[EMB_PRE_DailyActivityDetail]', N'U') IS NOT NULL
    AND OBJECT_ID(N'[ods].[emblue_DailyActivityDetail]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[EMB_PRE_DailyActivityDetail]', N'emblue_DailyActivityDetail';
    ALTER SCHEMA ods TRANSFER dbo.emblue_DailyActivityDetail;
END;
GO

IF OBJECT_ID(N'[dbo].[EMB_PRE_DailyActionSummary]', N'U') IS NOT NULL
    AND OBJECT_ID(N'[ods].[emblue_DailyActionSummary]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[EMB_PRE_DailyActionSummary]', N'emblue_DailyActionSummary';
    ALTER SCHEMA ods TRANSFER dbo.emblue_DailyActionSummary;
END;
GO

IF OBJECT_ID(N'[dbo].[EMB_PRE_TaskExecutionLog]', N'U') IS NOT NULL
    AND OBJECT_ID(N'[ods].[emblue_TaskExecutionLog]', N'U') IS NULL
BEGIN
    EXEC sp_rename N'[dbo].[EMB_PRE_TaskExecutionLog]', N'emblue_TaskExecutionLog';
    ALTER SCHEMA ods TRANSFER dbo.emblue_TaskExecutionLog;
END;
GO

------------------------------------------------------------
-- 3. Create ods.emblue_TaskExecutionLog when missing
------------------------------------------------------------
IF OBJECT_ID(N'[ods].[emblue_TaskExecutionLog]', N'U') IS NULL
BEGIN
    CREATE TABLE [ods].[emblue_TaskExecutionLog]
    (
        [Id] INT IDENTITY(1, 1) NOT NULL CONSTRAINT [PK_emblue_TaskExecutionLog] PRIMARY KEY,
        [TaskName] NVARCHAR(128) NOT NULL,
        [Parameters] NVARCHAR(512) NULL,
        [StartedAtUtc] DATETIME2(7) NOT NULL,
        [CompletedAtUtc] DATETIME2(7) NULL,
        [Status] NVARCHAR(32) NOT NULL,
        [Message] NVARCHAR(1024) NULL
    );
END;
GO

------------------------------------------------------------
-- 4. Create ods.emblue_TaskExecutionFile when missing
------------------------------------------------------------
IF OBJECT_ID(N'[ods].[emblue_TaskExecutionFile]', N'U') IS NULL
BEGIN
    CREATE TABLE [ods].[emblue_TaskExecutionFile]
    (
        [Id] INT IDENTITY(1, 1) NOT NULL CONSTRAINT [PK_emblue_TaskExecutionFile] PRIMARY KEY,
        [TaskExecutionLogId] INT NOT NULL,
        [ReportType] TINYINT NOT NULL,
        [FileId] INT NOT NULL,
        [FileName] NVARCHAR(512) NOT NULL,
        [ReportDate] DATE NOT NULL,
        [CreatedAtUtc] DATETIME2(0) NOT NULL,
        [ProcessedAtUtc] DATETIME2(0) NULL
    );

    ALTER TABLE [ods].[emblue_TaskExecutionFile]
        ADD CONSTRAINT [FK_emblue_TaskExecutionFile_TaskExecutionLog]
        FOREIGN KEY ([TaskExecutionLogId]) REFERENCES [ods].[emblue_TaskExecutionLog]([Id]) ON DELETE CASCADE;

    CREATE UNIQUE INDEX [IX_emblue_TaskExecutionFile_FileId_ReportType]
        ON [ods].[emblue_TaskExecutionFile] ([FileId], [ReportType]);
END;
GO

------------------------------------------------------------
-- 5. Ensure ods.emblue_DailyActivityDetail exists with expected columns
------------------------------------------------------------
IF OBJECT_ID(N'[ods].[emblue_DailyActivityDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [ods].[emblue_DailyActivityDetail]
    (
        [Id] INT IDENTITY(1, 1) NOT NULL CONSTRAINT [PK_emblue_DailyActivityDetail] PRIMARY KEY,
        [TaskExecutionFileId] INT NOT NULL,
        [Email] NVARCHAR(256) NOT NULL,
        [SendDate] DATETIME2(7) NULL,
        [ActivityDate] DATETIME2(7) NULL,
        [Campaign] NVARCHAR(256) NOT NULL,
        [Action] NVARCHAR(256) NOT NULL,
        [ActionType] NVARCHAR(128) NOT NULL,
        [Activity] NVARCHAR(128) NOT NULL,
        [Description] NVARCHAR(512) NULL,
        [Tag] NVARCHAR(128) NULL,
        [Account] NVARCHAR(128) NULL,
        [Category] NVARCHAR(128) NULL,
        [SegmentCategory] NVARCHAR(128) NULL
    );
END;
GO

IF COL_LENGTH(N'ods.emblue_DailyActivityDetail', N'TaskExecutionFileId') IS NULL
BEGIN
    ALTER TABLE [ods].[emblue_DailyActivityDetail]
        ADD [TaskExecutionFileId] INT NULL;
END;
GO

-- Harmonise column definitions (lengths and nullability)
ALTER TABLE [ods].[emblue_DailyActivityDetail]
    ALTER COLUMN [Email] NVARCHAR(256) NOT NULL;
ALTER TABLE [ods].[emblue_DailyActivityDetail]
    ALTER COLUMN [Campaign] NVARCHAR(256) NOT NULL;
ALTER TABLE [ods].[emblue_DailyActivityDetail]
    ALTER COLUMN [Action] NVARCHAR(256) NOT NULL;
ALTER TABLE [ods].[emblue_DailyActivityDetail]
    ALTER COLUMN [ActionType] NVARCHAR(128) NOT NULL;
ALTER TABLE [ods].[emblue_DailyActivityDetail]
    ALTER COLUMN [Activity] NVARCHAR(128) NOT NULL;
ALTER TABLE [ods].[emblue_DailyActivityDetail]
    ALTER COLUMN [Description] NVARCHAR(512) NULL;
ALTER TABLE [ods].[emblue_DailyActivityDetail]
    ALTER COLUMN [Tag] NVARCHAR(128) NULL;
ALTER TABLE [ods].[emblue_DailyActivityDetail]
    ALTER COLUMN [Account] NVARCHAR(128) NULL;
ALTER TABLE [ods].[emblue_DailyActivityDetail]
    ALTER COLUMN [Category] NVARCHAR(128) NULL;
ALTER TABLE [ods].[emblue_DailyActivityDetail]
    ALTER COLUMN [SegmentCategory] NVARCHAR(128) NULL;
ALTER TABLE [ods].[emblue_DailyActivityDetail]
    ALTER COLUMN [SendDate] DATETIME2(7) NULL;
ALTER TABLE [ods].[emblue_DailyActivityDetail]
    ALTER COLUMN [ActivityDate] DATETIME2(7) NULL;
GO

-- Enforce NOT NULL on TaskExecutionFileId once the column is populated
IF EXISTS (SELECT 1 FROM [ods].[emblue_DailyActivityDetail] WHERE [TaskExecutionFileId] IS NULL)
BEGIN
    RAISERROR('Populate ods.emblue_DailyActivityDetail.TaskExecutionFileId before enforcing NOT NULL.', 16, 1);
END
ELSE
BEGIN
    ALTER TABLE [ods].[emblue_DailyActivityDetail]
        ALTER COLUMN [TaskExecutionFileId] INT NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_emblue_DailyActivityDetail_TaskExecutionFile'
        AND parent_object_id = OBJECT_ID(N'ods.emblue_DailyActivityDetail')
)
BEGIN
    ALTER TABLE [ods].[emblue_DailyActivityDetail]
        ADD CONSTRAINT [FK_emblue_DailyActivityDetail_TaskExecutionFile]
        FOREIGN KEY ([TaskExecutionFileId])
        REFERENCES [ods].[emblue_TaskExecutionFile]([Id])
        ON DELETE CASCADE;
END;
GO

------------------------------------------------------------
-- 6. Ensure ods.emblue_DailyActionSummary exists with expected columns
------------------------------------------------------------
IF OBJECT_ID(N'[ods].[emblue_DailyActionSummary]', N'U') IS NULL
BEGIN
    CREATE TABLE [ods].[emblue_DailyActionSummary]
    (
        [Id] INT IDENTITY(1, 1) NOT NULL CONSTRAINT [PK_emblue_DailyActionSummary] PRIMARY KEY,
        [TaskExecutionFileId] INT NOT NULL,
        [Campaign] NVARCHAR(256) NOT NULL,
        [Action] NVARCHAR(256) NOT NULL,
        [Type] NVARCHAR(128) NOT NULL,
        [Subject] NVARCHAR(512) NOT NULL,
        [Sender] NVARCHAR(256) NOT NULL,
        [TrustedSender] NVARCHAR(64) NULL,
        [TouchRules] NVARCHAR(128) NULL,
        [Date] DATETIME2(7) NULL,
        [Recipients] NVARCHAR(256) NULL,
        [Sent] NVARCHAR(256) NULL,
        [Bounces] NVARCHAR(256) NULL,
        [Effective] NVARCHAR(256) NULL,
        [Opens] NVARCHAR(256) NULL,
        [UniqueOpens] NVARCHAR(256) NULL,
        [Clicks] NVARCHAR(256) NULL,
        [UniqueClicks] NVARCHAR(256) NULL,
        [Virals] NVARCHAR(256) NULL,
        [Subscribers] NVARCHAR(256) NULL,
        [Unsubscribers] NVARCHAR(256) NULL,
        [Dr] NVARCHAR(64) NULL,
        [Br] NVARCHAR(64) NULL,
        [Or] NVARCHAR(64) NULL,
        [Uor] NVARCHAR(64) NULL,
        [Ctr] NVARCHAR(64) NULL,
        [Ctor] NVARCHAR(64) NULL,
        [Ctuor] NVARCHAR(64) NULL,
        [Vr] NVARCHAR(64) NULL
    );
END;
GO

IF COL_LENGTH(N'ods.emblue_DailyActionSummary', N'TaskExecutionFileId') IS NULL
BEGIN
    ALTER TABLE [ods].[emblue_DailyActionSummary]
        ADD [TaskExecutionFileId] INT NULL;
END;
GO

ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Campaign] NVARCHAR(256) NOT NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Action] NVARCHAR(256) NOT NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Type] NVARCHAR(128) NOT NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Subject] NVARCHAR(512) NOT NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Sender] NVARCHAR(256) NOT NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [TrustedSender] NVARCHAR(64) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [TouchRules] NVARCHAR(128) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Date] DATETIME2(7) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Recipients] NVARCHAR(256) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Sent] NVARCHAR(256) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Bounces] NVARCHAR(256) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Effective] NVARCHAR(256) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Opens] NVARCHAR(256) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [UniqueOpens] NVARCHAR(256) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Clicks] NVARCHAR(256) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [UniqueClicks] NVARCHAR(256) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Virals] NVARCHAR(256) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Subscribers] NVARCHAR(256) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Unsubscribers] NVARCHAR(256) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Dr] NVARCHAR(64) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Br] NVARCHAR(64) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Or] NVARCHAR(64) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Uor] NVARCHAR(64) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Ctr] NVARCHAR(64) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Ctor] NVARCHAR(64) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Ctuor] NVARCHAR(64) NULL;
ALTER TABLE [ods].[emblue_DailyActionSummary]
    ALTER COLUMN [Vr] NVARCHAR(64) NULL;
GO

IF EXISTS (SELECT 1 FROM [ods].[emblue_DailyActionSummary] WHERE [TaskExecutionFileId] IS NULL)
BEGIN
    RAISERROR('Populate ods.emblue_DailyActionSummary.TaskExecutionFileId before enforcing NOT NULL.', 16, 1);
END
ELSE
BEGIN
    ALTER TABLE [ods].[emblue_DailyActionSummary]
        ALTER COLUMN [TaskExecutionFileId] INT NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = N'FK_emblue_DailyActionSummary_TaskExecutionFile'
        AND parent_object_id = OBJECT_ID(N'ods.emblue_DailyActionSummary')
)
BEGIN
    ALTER TABLE [ods].[emblue_DailyActionSummary]
        ADD CONSTRAINT [FK_emblue_DailyActionSummary_TaskExecutionFile]
        FOREIGN KEY ([TaskExecutionFileId])
        REFERENCES [ods].[emblue_TaskExecutionFile]([Id])
        ON DELETE CASCADE;
END;
GO
