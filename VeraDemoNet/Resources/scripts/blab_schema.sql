IF OBJECT_ID('dbo.blabs', 'U') IS NOT NULL DROP TABLE [blabs]; 
GO
IF OBJECT_ID('dbo.comments', 'U') IS NOT NULL DROP TABLE [comments]; 
GO
IF OBJECT_ID('dbo.listeners', 'U') IS NOT NULL DROP TABLE [listeners]; 
GO
IF OBJECT_ID('dbo.users_history', 'U') IS NOT NULL DROP TABLE [users_history]; 
GO
IF OBJECT_ID('dbo.users', 'U') IS NOT NULL DROP TABLE [users]; 
GO

CREATE TABLE [blabs] (
    [blabid]    INT           NOT NULL IDENTITY,
    [blabber]   VARCHAR (100) NOT NULL,
    [content]   VARCHAR (250) DEFAULT (NULL) NULL,
    [timestamp] DATETIME      DEFAULT (NULL) NULL,
    PRIMARY KEY CLUSTERED ([blabid] ASC)
);
GO

CREATE TABLE [comments] (
    [commentid] INT           NOT NULL IDENTITY,
    [blabid]    INT           NOT NULL,
    [blabber]   VARCHAR (100) NOT NULL,
    [content]   VARCHAR (250) DEFAULT (NULL) NULL,
    [timestamp] DATETIME      DEFAULT (NULL) NULL,
    PRIMARY KEY CLUSTERED ([commentid] ASC)
);
GO

CREATE TABLE [listeners] (
    [blabber]  VARCHAR (100) NOT NULL,
    [listener] VARCHAR (100) NOT NULL,
    [status]   VARCHAR (20)  DEFAULT (NULL) NULL
);
GO

CREATE TABLE [users] (
    [username]      VARCHAR (100) NOT NULL,
    [password]      VARCHAR (100) DEFAULT (NULL) NULL,
    [password_hint] VARCHAR (100) DEFAULT (NULL) NULL,
    [created_at]    DATETIME      DEFAULT (NULL) NULL,
    [last_login]    DATETIME      DEFAULT (NULL) NULL,
    [real_name]     VARCHAR (100) DEFAULT (NULL) NULL,
    [blab_name]     VARCHAR (100) DEFAULT (NULL) NULL,
    PRIMARY KEY CLUSTERED ([username] ASC)
);
GO

CREATE TABLE [users_history] (
    [eventid]   INT           NOT NULL IDENTITY,
    [blabber]   VARCHAR (100) NOT NULL,
    [event]     VARCHAR (250) DEFAULT (NULL) NULL,
    [timestamp] DATETIME      DEFAULT (NULL) NULL,
    PRIMARY KEY CLUSTERED ([eventid] ASC)
);
GO