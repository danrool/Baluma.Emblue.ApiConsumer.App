-- Script to create EMB_PRE_ prefixed tables for DailyActivityDetail and DailyActionSummary
-- This script targets Microsoft SQL Server

IF OBJECT_ID(N'[dbo].[EMB_PRE_DailyActivityDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[EMB_PRE_DailyActivityDetail]
    (
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_EMB_PRE_DailyActivityDetail] PRIMARY KEY,
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
END
GO

IF OBJECT_ID(N'[dbo].[EMB_PRE_DailyActionSummary]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[EMB_PRE_DailyActionSummary]
    (
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_EMB_PRE_DailyActionSummary] PRIMARY KEY,
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
END
GO

IF OBJECT_ID(N'[dbo].[EMB_PRE_TaskExecutionLog]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[EMB_PRE_TaskExecutionLog]
    (
        [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_EMB_PRE_TaskExecutionLog] PRIMARY KEY,
        [TaskName] NVARCHAR(128) NOT NULL,
        [Parameters] NVARCHAR(512) NULL,
        [StartedAtUtc] DATETIME2(7) NOT NULL,
        [CompletedAtUtc] DATETIME2(7) NULL,
        [Status] NVARCHAR(32) NOT NULL,
        [Message] NVARCHAR(1024) NULL
    );
END
GO
