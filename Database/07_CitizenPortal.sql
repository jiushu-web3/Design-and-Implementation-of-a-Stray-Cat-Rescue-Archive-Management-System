SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
USE StrayCatArchive;
GO

IF OBJECT_ID(N'dbo.StrayReport', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StrayReport
    (
        ReportId INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_StrayReport PRIMARY KEY,
        UserId INT NOT NULL,
        FoundPlace NVARCHAR(200) NOT NULL,
        FoundDate DATE NOT NULL,
        Description NVARCHAR(500) NULL,
        PhotoPath NVARCHAR(200) NULL,
        ContactPhone NVARCHAR(20) NOT NULL,
        Status TINYINT NOT NULL CONSTRAINT DF_StrayReport_Status DEFAULT (1),
        RejectReason NVARCHAR(200) NULL,
        CatId INT NULL,
        CreateTime DATETIME NOT NULL CONSTRAINT DF_StrayReport_CreateTime DEFAULT (GETDATE()),
        AuditUserId INT NULL,
        AuditTime DATETIME NULL,
        CONSTRAINT FK_StrayReport_User FOREIGN KEY (UserId) REFERENCES dbo.SysUser (UserId),
        CONSTRAINT FK_StrayReport_Cat FOREIGN KEY (CatId) REFERENCES dbo.CatArchive (CatId),
        CONSTRAINT FK_StrayReport_Audit FOREIGN KEY (AuditUserId) REFERENCES dbo.SysUser (UserId),
        CONSTRAINT CK_StrayReport_Status CHECK (Status IN (1, 2, 3)),
        CONSTRAINT CK_StrayReport_Approved CHECK (Status <> 2 OR CatId IS NOT NULL),
        CONSTRAINT CK_StrayReport_Rejected CHECK (Status <> 3 OR RejectReason IS NOT NULL)
    );
    CREATE INDEX IX_StrayReport_Status ON dbo.StrayReport (Status, CreateTime DESC);
    CREATE INDEX IX_StrayReport_User ON dbo.StrayReport (UserId, CreateTime DESC);
END
GO

IF OBJECT_ID(N'dbo.AdoptionApplication', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AdoptionApplication
    (
        ApplicationId INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_AdoptionApplication PRIMARY KEY,
        CatId INT NOT NULL,
        UserId INT NOT NULL,
        ApplicantName NVARCHAR(50) NOT NULL,
        Phone NVARCHAR(20) NOT NULL,
        Address NVARCHAR(200) NOT NULL,
        Reason NVARCHAR(500) NULL,
        Status TINYINT NOT NULL CONSTRAINT DF_AdoptionApplication_Status DEFAULT (1),
        RejectReason NVARCHAR(200) NULL,
        AdoptionId INT NULL,
        CreateTime DATETIME NOT NULL CONSTRAINT DF_AdoptionApplication_CreateTime DEFAULT (GETDATE()),
        AuditUserId INT NULL,
        AuditTime DATETIME NULL,
        CONSTRAINT FK_AdoptionApplication_Cat FOREIGN KEY (CatId) REFERENCES dbo.CatArchive (CatId),
        CONSTRAINT FK_AdoptionApplication_User FOREIGN KEY (UserId) REFERENCES dbo.SysUser (UserId),
        CONSTRAINT FK_AdoptionApplication_Adoption FOREIGN KEY (AdoptionId) REFERENCES dbo.AdoptionRecord (AdoptionId),
        CONSTRAINT FK_AdoptionApplication_Audit FOREIGN KEY (AuditUserId) REFERENCES dbo.SysUser (UserId),
        CONSTRAINT CK_AdoptionApplication_Status CHECK (Status IN (1, 2, 3)),
        CONSTRAINT CK_AdoptionApplication_Approved CHECK (Status <> 2 OR AdoptionId IS NOT NULL),
        CONSTRAINT CK_AdoptionApplication_Rejected CHECK (Status <> 3 OR RejectReason IS NOT NULL)
    );
    CREATE UNIQUE INDEX UX_AdoptionApplication_Pending
        ON dbo.AdoptionApplication (UserId, CatId) WHERE Status = 1;
    CREATE INDEX IX_AdoptionApplication_Cat ON dbo.AdoptionApplication (CatId, Status);
    CREATE INDEX IX_AdoptionApplication_User ON dbo.AdoptionApplication (UserId, CreateTime DESC);
END
GO

IF OBJECT_ID(N'dbo.usp_Report_Create', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Report_Create;
GO
CREATE PROCEDURE dbo.usp_Report_Create
    @UserId INT,
    @FoundPlace NVARCHAR(200),
    @FoundDate DATE,
    @Description NVARCHAR(500),
    @PhotoPath NVARCHAR(200),
    @ContactPhone NVARCHAR(20),
    @ReportId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.StrayReport (UserId, FoundPlace, FoundDate, Description, PhotoPath, ContactPhone, Status)
    VALUES (@UserId, LTRIM(RTRIM(@FoundPlace)), @FoundDate, @Description, @PhotoPath, @ContactPhone, 1);
    SET @ReportId = CONVERT(INT, SCOPE_IDENTITY());
END
GO

IF OBJECT_ID(N'dbo.usp_Report_ListByUser', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Report_ListByUser;
GO
CREATE PROCEDURE dbo.usp_Report_ListByUser
    @UserId INT,
    @PageIndex INT,
    @PageSize INT,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageIndex < 1 SET @PageIndex = 1;
    IF @PageSize < 1 OR @PageSize > 50 SET @PageSize = 10;

    SELECT @TotalCount = COUNT(*) FROM dbo.StrayReport WHERE UserId = @UserId;

    SELECT r.ReportId, r.UserId, r.FoundPlace, r.FoundDate, r.Description, r.PhotoPath,
           r.ContactPhone, r.Status, r.RejectReason, r.CatId, r.CreateTime,
           r.AuditUserId, r.AuditTime
    FROM dbo.StrayReport AS r
    WHERE r.UserId = @UserId
    ORDER BY r.CreateTime DESC, r.ReportId DESC
    OFFSET (@PageIndex - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

IF OBJECT_ID(N'dbo.usp_Report_Search', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Report_Search;
GO
CREATE PROCEDURE dbo.usp_Report_Search
    @Status TINYINT,
    @Keyword NVARCHAR(50),
    @PageIndex INT,
    @PageSize INT,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageIndex < 1 SET @PageIndex = 1;
    IF @PageSize < 1 OR @PageSize > 50 SET @PageSize = 10;

    DECLARE @Like NVARCHAR(120) = NULL;
    IF @Keyword IS NOT NULL AND LEN(LTRIM(RTRIM(@Keyword))) > 0
    BEGIN
        DECLARE @Text NVARCHAR(50) = LTRIM(RTRIM(@Keyword));
        SET @Text = REPLACE(REPLACE(REPLACE(@Text, N'[', N'[[]'), N'%', N'[%]'), N'_', N'[_]');
        SET @Like = N'%' + @Text + N'%';
    END

    SELECT @TotalCount = COUNT(*)
    FROM dbo.StrayReport AS r
    INNER JOIN dbo.SysUser AS u ON u.UserId = r.UserId
    WHERE (@Status IS NULL OR r.Status = @Status)
      AND (@Like IS NULL OR r.FoundPlace LIKE @Like OR u.UserName LIKE @Like OR u.RealName LIKE @Like);

    SELECT r.ReportId, r.UserId, r.FoundPlace, r.FoundDate, r.Description, r.PhotoPath,
           r.ContactPhone, r.Status, r.RejectReason, r.CatId, r.CreateTime,
           r.AuditUserId, r.AuditTime, u.UserName, u.RealName
    FROM dbo.StrayReport AS r
    INNER JOIN dbo.SysUser AS u ON u.UserId = r.UserId
    WHERE (@Status IS NULL OR r.Status = @Status)
      AND (@Like IS NULL OR r.FoundPlace LIKE @Like OR u.UserName LIKE @Like OR u.RealName LIKE @Like)
    ORDER BY r.CreateTime DESC, r.ReportId DESC
    OFFSET (@PageIndex - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

IF OBJECT_ID(N'dbo.usp_Report_Approve', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Report_Approve;
GO
CREATE PROCEDURE dbo.usp_Report_Approve
    @ReportId INT,
    @AuditUserId INT,
    @TempName NVARCHAR(50),
    @Gender TINYINT,
    @EstimatedAge NVARCHAR(20),
    @Color NVARCHAR(50),
    @IsNeutered TINYINT,
    @PhotoPath NVARCHAR(200),
    @Remark NVARCHAR(500),
    @RescueTime DATETIME,
    @InitialHealth NVARCHAR(500),
    @CatId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @Status TINYINT, @Place NVARCHAR(200), @Description NVARCHAR(500), @Photo NVARCHAR(200);
        SELECT @Status = Status, @Place = FoundPlace, @Description = Description, @Photo = PhotoPath
        FROM dbo.StrayReport WITH (UPDLOCK, HOLDLOCK)
        WHERE ReportId = @ReportId;

        IF @Status IS NULL
            RAISERROR(N'记录不存在或已被删除', 16, 1);
        IF @Status <> 1
            RAISERROR(N'当前状态不允许此操作', 16, 1);

        DECLARE @NewCatId INT;
        DECLARE @UsePhoto NVARCHAR(200) = COALESCE(@PhotoPath, @Photo);
        EXEC dbo.usp_Cat_CreateWithRescue
            @TempName = @TempName,
            @Gender = @Gender,
            @EstimatedAge = @EstimatedAge,
            @Color = @Color,
            @IsNeutered = @IsNeutered,
            @PhotoPath = @UsePhoto,
            @Remark = @Remark,
            @CreateUserId = @AuditUserId,
            @RescueTime = @RescueTime,
            @Location = @Place,
            @FoundWay = N'市民送交',
            @InitialHealth = @InitialHealth,
            @RescueRemark = @Description,
            @CatId = @NewCatId OUTPUT;

        UPDATE dbo.StrayReport
        SET Status = 2, CatId = @NewCatId, AuditUserId = @AuditUserId, AuditTime = GETDATE()
        WHERE ReportId = @ReportId;

        COMMIT TRANSACTION;
        SET @CatId = @NewCatId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

IF OBJECT_ID(N'dbo.usp_Report_Reject', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Report_Reject;
GO
CREATE PROCEDURE dbo.usp_Report_Reject
    @ReportId INT,
    @AuditUserId INT,
    @Reason NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.StrayReport
    SET Status = 3, RejectReason = @Reason, AuditUserId = @AuditUserId, AuditTime = GETDATE()
    WHERE ReportId = @ReportId AND Status = 1;

    IF @@ROWCOUNT = 0
        RAISERROR(N'当前状态不允许此操作', 16, 1);
END
GO

IF OBJECT_ID(N'dbo.usp_Application_Create', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Application_Create;
GO
CREATE PROCEDURE dbo.usp_Application_Create
    @CatId INT,
    @UserId INT,
    @ApplicantName NVARCHAR(50),
    @Phone NVARCHAR(20),
    @Address NVARCHAR(200),
    @Reason NVARCHAR(500),
    @ApplicationId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.CatArchive WHERE CatId = @CatId AND Status = 2)
    BEGIN
        RAISERROR(N'该猫当前不可申请领养', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.AdoptionApplication
        (CatId, UserId, ApplicantName, Phone, Address, Reason, Status)
    VALUES
        (@CatId, @UserId, LTRIM(RTRIM(@ApplicantName)), @Phone, LTRIM(RTRIM(@Address)), @Reason, 1);
    SET @ApplicationId = CONVERT(INT, SCOPE_IDENTITY());
END
GO

IF OBJECT_ID(N'dbo.usp_Application_ListByUser', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Application_ListByUser;
GO
CREATE PROCEDURE dbo.usp_Application_ListByUser
    @UserId INT,
    @PageIndex INT,
    @PageSize INT,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageIndex < 1 SET @PageIndex = 1;
    IF @PageSize < 1 OR @PageSize > 50 SET @PageSize = 10;

    SELECT @TotalCount = COUNT(*) FROM dbo.AdoptionApplication WHERE UserId = @UserId;

    SELECT a.ApplicationId, a.CatId, a.UserId, a.ApplicantName, a.Phone, a.Address, a.Reason,
           a.Status, a.RejectReason, a.AdoptionId, a.CreateTime, a.AuditUserId, a.AuditTime,
           c.CatNo, c.TempName
    FROM dbo.AdoptionApplication AS a
    INNER JOIN dbo.CatArchive AS c ON c.CatId = a.CatId
    WHERE a.UserId = @UserId
    ORDER BY a.CreateTime DESC, a.ApplicationId DESC
    OFFSET (@PageIndex - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

IF OBJECT_ID(N'dbo.usp_Application_Search', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Application_Search;
GO
CREATE PROCEDURE dbo.usp_Application_Search
    @Status TINYINT,
    @PageIndex INT,
    @PageSize INT,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageIndex < 1 SET @PageIndex = 1;
    IF @PageSize < 1 OR @PageSize > 50 SET @PageSize = 10;

    SELECT @TotalCount = COUNT(*) FROM dbo.AdoptionApplication WHERE (@Status IS NULL OR Status = @Status);

    SELECT a.ApplicationId, a.CatId, a.UserId, a.ApplicantName, a.Phone, a.Address, a.Reason,
           a.Status, a.RejectReason, a.AdoptionId, a.CreateTime, a.AuditUserId, a.AuditTime,
           c.CatNo, c.TempName
    FROM dbo.AdoptionApplication AS a
    INNER JOIN dbo.CatArchive AS c ON c.CatId = a.CatId
    WHERE (@Status IS NULL OR a.Status = @Status)
    ORDER BY a.CreateTime DESC, a.ApplicationId DESC
    OFFSET (@PageIndex - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

IF OBJECT_ID(N'dbo.usp_Application_Approve', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Application_Approve;
GO
CREATE PROCEDURE dbo.usp_Application_Approve
    @ApplicationId INT,
    @AuditUserId INT,
    @AdoptionId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @CatId INT, @Name NVARCHAR(50), @Phone NVARCHAR(20), @Address NVARCHAR(200), @Status TINYINT;
        SELECT @CatId = CatId, @Name = ApplicantName, @Phone = Phone, @Address = Address, @Status = Status
        FROM dbo.AdoptionApplication WITH (UPDLOCK, HOLDLOCK)
        WHERE ApplicationId = @ApplicationId;

        IF @Status IS NULL
            RAISERROR(N'记录不存在或已被删除', 16, 1);
        IF @Status <> 1
            RAISERROR(N'当前状态不允许此操作', 16, 1);

        DECLARE @NewAdoptionId INT;
        DECLARE @AdoptDate DATE = CAST(GETDATE() AS DATE);
        EXEC dbo.usp_Adoption_Create
            @CatId = @CatId,
            @AdopterName = @Name,
            @AdopterPhone = @Phone,
            @AdopterAddress = @Address,
            @AdoptDate = @AdoptDate,
            @HandlerUserId = @AuditUserId,
            @Remark = N'来自领养申请',
            @AdoptionId = @NewAdoptionId OUTPUT;

        UPDATE dbo.AdoptionApplication
        SET Status = 2, AdoptionId = @NewAdoptionId, AuditUserId = @AuditUserId, AuditTime = GETDATE()
        WHERE ApplicationId = @ApplicationId;

        COMMIT TRANSACTION;
        SET @AdoptionId = @NewAdoptionId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

IF OBJECT_ID(N'dbo.usp_Application_Reject', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Application_Reject;
GO
CREATE PROCEDURE dbo.usp_Application_Reject
    @ApplicationId INT,
    @AuditUserId INT,
    @Reason NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.AdoptionApplication
    SET Status = 3, RejectReason = @Reason, AuditUserId = @AuditUserId, AuditTime = GETDATE()
    WHERE ApplicationId = @ApplicationId AND Status = 1;

    IF @@ROWCOUNT = 0
        RAISERROR(N'当前状态不允许此操作', 16, 1);
END
GO

IF OBJECT_ID(N'dbo.usp_Cat_ListAdoptable', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Cat_ListAdoptable;
GO
CREATE PROCEDURE dbo.usp_Cat_ListAdoptable
    @PageIndex INT,
    @PageSize INT,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageIndex < 1 SET @PageIndex = 1;
    IF @PageSize < 1 OR @PageSize > 50 SET @PageSize = 12;

    SELECT @TotalCount = COUNT(*) FROM dbo.CatArchive WHERE Status = 2;

    SELECT c.CatId, c.CatNo, c.TempName, c.Gender, c.Status, c.EstimatedAge, c.Color,
           c.IsNeutered, c.PhotoPath, c.Remark,
           res.RescueTime,
           ISNULL(m.MedicalCount, 0) AS MedicalCount,
           ISNULL(a.AdoptionCount, 0) AS AdoptionCount
    FROM dbo.CatArchive AS c
    LEFT JOIN dbo.RescueRecord AS res ON res.CatId = c.CatId
    OUTER APPLY (SELECT COUNT(*) AS MedicalCount FROM dbo.MedicalRecord WHERE CatId = c.CatId) AS m
    OUTER APPLY (SELECT COUNT(*) AS AdoptionCount FROM dbo.AdoptionRecord WHERE CatId = c.CatId) AS a
    WHERE c.Status = 2
    ORDER BY c.CatId DESC
    OFFSET (@PageIndex - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

IF OBJECT_ID(N'dbo.usp_Cat_GetPublicDetail', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Cat_GetPublicDetail;
GO
CREATE PROCEDURE dbo.usp_Cat_GetPublicDetail
    @CatId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.CatId, c.CatNo, c.TempName, c.Gender, c.EstimatedAge, c.Color,
           c.IsNeutered, c.PhotoPath, c.Remark, c.Status,
           c.TerminateReason, c.CreateUserId, c.CreateTime, c.UpdateTime,
           r.Location, r.FoundWay, r.RescueTime, r.InitialHealth,
           ISNULL(r.RescueId, 0) AS RescueId,
           ISNULL(r.ReceiverUserId, 0) AS ReceiverUserId,
           CAST(NULL AS NVARCHAR(500)) AS RescueRemark,
           ISNULL((SELECT COUNT(*) FROM dbo.MedicalRecord WHERE CatId = c.CatId), 0) AS MedicalCount,
           ISNULL((SELECT COUNT(*) FROM dbo.AdoptionRecord WHERE CatId = c.CatId), 0) AS AdoptionCount
    FROM dbo.CatArchive AS c
    LEFT JOIN dbo.RescueRecord AS r ON r.CatId = c.CatId
    WHERE c.CatId = @CatId AND c.Status = 2;
END
GO
