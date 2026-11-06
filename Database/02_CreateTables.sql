SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
USE StrayCatArchive;
GO

IF OBJECT_ID(N'dbo.FollowUpRecord', N'U') IS NOT NULL DROP TABLE dbo.FollowUpRecord;
IF OBJECT_ID(N'dbo.AdoptionRecord', N'U') IS NOT NULL DROP TABLE dbo.AdoptionRecord;
IF OBJECT_ID(N'dbo.MedicalRecord', N'U') IS NOT NULL DROP TABLE dbo.MedicalRecord;
IF OBJECT_ID(N'dbo.RescueRecord', N'U') IS NOT NULL DROP TABLE dbo.RescueRecord;
IF OBJECT_ID(N'dbo.CatArchive', N'U') IS NOT NULL DROP TABLE dbo.CatArchive;
IF OBJECT_ID(N'dbo.SysUser', N'U') IS NOT NULL DROP TABLE dbo.SysUser;
GO

CREATE TABLE dbo.SysUser
(
    UserId INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_SysUser PRIMARY KEY,
    UserName NVARCHAR(20) NOT NULL,
    PasswordHash CHAR(64) NOT NULL,
    Salt CHAR(32) NOT NULL,
    RealName NVARCHAR(50) NOT NULL,
    Role TINYINT NOT NULL,
    Phone NVARCHAR(20) NULL,
    IsEnabled BIT NOT NULL CONSTRAINT DF_SysUser_IsEnabled DEFAULT (1),
    CreateTime DATETIME NOT NULL CONSTRAINT DF_SysUser_CreateTime DEFAULT (GETDATE()),
    CONSTRAINT UQ_SysUser_UserName UNIQUE (UserName),
    CONSTRAINT CK_SysUser_Role CHECK (Role IN (1, 2))
);
GO

CREATE TABLE dbo.CatArchive
(
    CatId INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_CatArchive PRIMARY KEY,
    CatNo CHAR(11) NOT NULL,
    TempName NVARCHAR(50) NOT NULL,
    Gender TINYINT NOT NULL,
    EstimatedAge NVARCHAR(20) NULL,
    Color NVARCHAR(50) NULL,
    IsNeutered TINYINT NOT NULL CONSTRAINT DF_CatArchive_IsNeutered DEFAULT (0),
    Status TINYINT NOT NULL,
    TerminateReason NVARCHAR(200) NULL,
    PhotoPath NVARCHAR(200) NULL,
    Remark NVARCHAR(500) NULL,
    CreateUserId INT NOT NULL,
    CreateTime DATETIME NOT NULL CONSTRAINT DF_CatArchive_CreateTime DEFAULT (GETDATE()),
    UpdateTime DATETIME NULL,
    CONSTRAINT UQ_CatArchive_CatNo UNIQUE (CatNo),
    CONSTRAINT FK_CatArchive_SysUser FOREIGN KEY (CreateUserId) REFERENCES dbo.SysUser (UserId),
    CONSTRAINT CK_CatArchive_Gender CHECK (Gender IN (0, 1, 2)),
    CONSTRAINT CK_CatArchive_IsNeutered CHECK (IsNeutered IN (0, 1, 2)),
    CONSTRAINT CK_CatArchive_Status CHECK (Status IN (1, 2, 3, 4, 5)),
    CONSTRAINT CK_CatArchive_Terminate CHECK (
        (Status = 5 AND TerminateReason IS NOT NULL AND LEN(LTRIM(RTRIM(TerminateReason))) > 0)
        OR (Status <> 5 AND TerminateReason IS NULL))
);
GO

CREATE INDEX IX_CatArchive_Status ON dbo.CatArchive (Status);
CREATE INDEX IX_CatArchive_TempName ON dbo.CatArchive (TempName);
GO

CREATE TABLE dbo.RescueRecord
(
    RescueId INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_RescueRecord PRIMARY KEY,
    CatId INT NOT NULL,
    RescueTime DATETIME NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    FoundWay NVARCHAR(50) NOT NULL,
    InitialHealth NVARCHAR(500) NOT NULL,
    ReceiverUserId INT NOT NULL,
    Remark NVARCHAR(500) NULL,
    CONSTRAINT UQ_RescueRecord_CatId UNIQUE (CatId),
    CONSTRAINT FK_RescueRecord_CatArchive FOREIGN KEY (CatId) REFERENCES dbo.CatArchive (CatId),
    CONSTRAINT FK_RescueRecord_SysUser FOREIGN KEY (ReceiverUserId) REFERENCES dbo.SysUser (UserId),
    CONSTRAINT CK_RescueRecord_FoundWay CHECK (FoundWay IN (N'市民送交', N'工作人员发现', N'其他机构转入'))
);
GO

CREATE INDEX IX_RescueRecord_RescueTime ON dbo.RescueRecord (RescueTime);
GO

CREATE TABLE dbo.MedicalRecord
(
    MedicalId INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_MedicalRecord PRIMARY KEY,
    CatId INT NOT NULL,
    MedicalDate DATE NOT NULL,
    Diagnosis NVARCHAR(200) NOT NULL,
    Treatment NVARCHAR(500) NOT NULL,
    Medicine NVARCHAR(200) NULL,
    RecordUserId INT NOT NULL,
    Remark NVARCHAR(500) NULL,
    CreateTime DATETIME NOT NULL CONSTRAINT DF_MedicalRecord_CreateTime DEFAULT (GETDATE()),
    CONSTRAINT FK_MedicalRecord_CatArchive FOREIGN KEY (CatId) REFERENCES dbo.CatArchive (CatId),
    CONSTRAINT FK_MedicalRecord_SysUser FOREIGN KEY (RecordUserId) REFERENCES dbo.SysUser (UserId)
);
GO

CREATE INDEX IX_MedicalRecord_CatId ON dbo.MedicalRecord (CatId);
GO

CREATE TABLE dbo.AdoptionRecord
(
    AdoptionId INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_AdoptionRecord PRIMARY KEY,
    CatId INT NOT NULL,
    AdopterName NVARCHAR(50) NOT NULL,
    AdopterPhone NVARCHAR(20) NOT NULL,
    AdopterAddress NVARCHAR(200) NOT NULL,
    AdoptDate DATE NOT NULL,
    Status TINYINT NOT NULL,
    ReturnDate DATE NULL,
    ReturnReason NVARCHAR(200) NULL,
    HandlerUserId INT NOT NULL,
    Remark NVARCHAR(500) NULL,
    CreateTime DATETIME NOT NULL CONSTRAINT DF_AdoptionRecord_CreateTime DEFAULT (GETDATE()),
    CONSTRAINT FK_AdoptionRecord_CatArchive FOREIGN KEY (CatId) REFERENCES dbo.CatArchive (CatId),
    CONSTRAINT FK_AdoptionRecord_SysUser FOREIGN KEY (HandlerUserId) REFERENCES dbo.SysUser (UserId),
    CONSTRAINT CK_AdoptionRecord_Status CHECK (Status IN (1, 2)),
    CONSTRAINT CK_AdoptionRecord_Return CHECK (
        (Status = 2 AND ReturnDate IS NOT NULL AND ReturnReason IS NOT NULL AND LEN(LTRIM(RTRIM(ReturnReason))) > 0)
        OR (Status = 1 AND ReturnDate IS NULL AND ReturnReason IS NULL))
);
GO

CREATE INDEX IX_AdoptionRecord_CatId ON dbo.AdoptionRecord (CatId);
CREATE INDEX IX_AdoptionRecord_AdoptDate ON dbo.AdoptionRecord (AdoptDate);
CREATE UNIQUE INDEX UX_Adoption_OneActive ON dbo.AdoptionRecord (CatId) WHERE Status = 1;
GO

CREATE TABLE dbo.FollowUpRecord
(
    FollowUpId INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_FollowUpRecord PRIMARY KEY,
    AdoptionId INT NOT NULL,
    FollowDate DATE NOT NULL,
    FollowWay NVARCHAR(50) NOT NULL,
    CatCondition NVARCHAR(500) NOT NULL,
    RecordUserId INT NOT NULL,
    Remark NVARCHAR(500) NULL,
    CreateTime DATETIME NOT NULL CONSTRAINT DF_FollowUpRecord_CreateTime DEFAULT (GETDATE()),
    CONSTRAINT FK_FollowUpRecord_AdoptionRecord FOREIGN KEY (AdoptionId) REFERENCES dbo.AdoptionRecord (AdoptionId),
    CONSTRAINT FK_FollowUpRecord_SysUser FOREIGN KEY (RecordUserId) REFERENCES dbo.SysUser (UserId),
    CONSTRAINT CK_FollowUpRecord_FollowWay CHECK (FollowWay IN (N'电话', N'上门', N'微信'))
);
GO

CREATE INDEX IX_FollowUpRecord_AdoptionId ON dbo.FollowUpRecord (AdoptionId);
GO
