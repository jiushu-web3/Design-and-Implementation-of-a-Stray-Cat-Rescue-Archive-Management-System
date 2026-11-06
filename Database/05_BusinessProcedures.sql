SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
USE StrayCatArchive;
GO

IF OBJECT_ID(N'dbo.usp_User_Insert', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_User_Insert;
GO
CREATE PROCEDURE dbo.usp_User_Insert
    @UserName NVARCHAR(20),
    @PasswordHash CHAR(64),
    @Salt CHAR(32),
    @RealName NVARCHAR(50),
    @Role TINYINT,
    @Phone NVARCHAR(20),
    @IsEnabled BIT,
    @UserId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.SysUser (UserName, PasswordHash, Salt, RealName, Role, Phone, IsEnabled)
    VALUES (@UserName, @PasswordHash, @Salt, @RealName, @Role, @Phone, @IsEnabled);
    SET @UserId = CONVERT(INT, SCOPE_IDENTITY());
END
GO

IF OBJECT_ID(N'dbo.usp_User_Update', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_User_Update;
GO
CREATE PROCEDURE dbo.usp_User_Update
    @UserId INT,
    @OperatorId INT,
    @RealName NVARCHAR(50),
    @Phone NVARCHAR(20),
    @Role TINYINT,
    @IsEnabled BIT,
    @PasswordHash CHAR(64),
    @Salt CHAR(32)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @OldRole TINYINT;
    DECLARE @OldEnabled BIT;
    SELECT @OldRole = Role, @OldEnabled = IsEnabled FROM dbo.SysUser WHERE UserId = @UserId;
    IF @OldRole IS NULL
    BEGIN
        RAISERROR(N'记录不存在或已被删除', 16, 1);
        RETURN;
    END
    IF @IsEnabled = 0 AND @UserId = @OperatorId
    BEGIN
        RAISERROR(N'不能停用当前登录账号', 16, 1);
        RETURN;
    END
    IF @OldRole = 1 AND @OldEnabled = 1 AND (@Role <> 1 OR @IsEnabled = 0)
       AND NOT EXISTS (SELECT 1 FROM dbo.SysUser WHERE Role = 1 AND IsEnabled = 1 AND UserId <> @UserId)
    BEGIN
        RAISERROR(N'不能停用或降级最后一个管理员', 16, 1);
        RETURN;
    END
    UPDATE dbo.SysUser
    SET RealName = @RealName,
        Phone = @Phone,
        Role = @Role,
        IsEnabled = @IsEnabled,
        PasswordHash = COALESCE(@PasswordHash, PasswordHash),
        Salt = COALESCE(@Salt, Salt)
    WHERE UserId = @UserId;
END
GO

IF OBJECT_ID(N'dbo.usp_User_SetEnabled', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_User_SetEnabled;
GO
CREATE PROCEDURE dbo.usp_User_SetEnabled
    @UserId INT,
    @OperatorId INT,
    @IsEnabled BIT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @OldRole TINYINT;
    DECLARE @OldEnabled BIT;
    SELECT @OldRole = Role, @OldEnabled = IsEnabled FROM dbo.SysUser WHERE UserId = @UserId;
    IF @OldRole IS NULL
    BEGIN
        RAISERROR(N'记录不存在或已被删除', 16, 1);
        RETURN;
    END
    IF @IsEnabled = 0 AND @UserId = @OperatorId
    BEGIN
        RAISERROR(N'不能停用当前登录账号', 16, 1);
        RETURN;
    END
    IF @IsEnabled = 0 AND @OldRole = 1 AND @OldEnabled = 1
       AND NOT EXISTS (SELECT 1 FROM dbo.SysUser WHERE Role = 1 AND IsEnabled = 1 AND UserId <> @UserId)
    BEGIN
        RAISERROR(N'不能停用或降级最后一个管理员', 16, 1);
        RETURN;
    END
    UPDATE dbo.SysUser SET IsEnabled = @IsEnabled WHERE UserId = @UserId;
END
GO

IF OBJECT_ID(N'dbo.usp_User_ChangePassword', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_User_ChangePassword;
GO
CREATE PROCEDURE dbo.usp_User_ChangePassword
    @UserId INT,
    @PasswordHash CHAR(64),
    @Salt CHAR(32)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.SysUser SET PasswordHash = @PasswordHash, Salt = @Salt WHERE UserId = @UserId;
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR(N'记录不存在或已被删除', 16, 1);
        RETURN;
    END
END
GO

IF OBJECT_ID(N'dbo.usp_User_GetById', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_User_GetById;
GO
CREATE PROCEDURE dbo.usp_User_GetById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId, UserName, PasswordHash, Salt, RealName, Role, Phone, IsEnabled, CreateTime
    FROM dbo.SysUser
    WHERE UserId = @UserId;
END
GO

IF OBJECT_ID(N'dbo.usp_User_Search', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_User_Search;
GO
CREATE PROCEDURE dbo.usp_User_Search
    @Keyword NVARCHAR(50),
    @PageIndex INT,
    @PageSize INT,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageIndex IS NULL OR @PageIndex < 1 SET @PageIndex = 1;
    IF @PageIndex > 100000 SET @PageIndex = 100000;
    IF @PageSize IS NULL OR @PageSize < 1 OR @PageSize > 50 SET @PageSize = 10;

    DECLARE @Like NVARCHAR(120) = NULL;
    IF @Keyword IS NOT NULL AND LEN(LTRIM(RTRIM(@Keyword))) > 0
    BEGIN
        DECLARE @Text NVARCHAR(50) = LTRIM(RTRIM(@Keyword));
        SET @Text = REPLACE(REPLACE(REPLACE(@Text, N'[', N'[[]'), N'%', N'[%]'), N'_', N'[_]');
        SET @Like = N'%' + @Text + N'%';
    END

    SELECT @TotalCount = COUNT(*)
    FROM dbo.SysUser
    WHERE @Like IS NULL OR UserName LIKE @Like OR RealName LIKE @Like;

    SELECT UserId, UserName, PasswordHash, Salt, RealName, Role, Phone, IsEnabled, CreateTime
    FROM dbo.SysUser
    WHERE @Like IS NULL OR UserName LIKE @Like OR RealName LIKE @Like
    ORDER BY UserId DESC
    OFFSET (@PageIndex - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

IF OBJECT_ID(N'dbo.usp_User_CountEnabledAdmins', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_User_CountEnabledAdmins;
GO
CREATE PROCEDURE dbo.usp_User_CountEnabledAdmins
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM dbo.SysUser WHERE Role = 1 AND IsEnabled = 1;
END
GO

IF OBJECT_ID(N'dbo.usp_Cat_CreateWithRescue', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Cat_CreateWithRescue;
GO
CREATE PROCEDURE dbo.usp_Cat_CreateWithRescue
    @TempName NVARCHAR(50),
    @Gender TINYINT,
    @EstimatedAge NVARCHAR(20),
    @Color NVARCHAR(50),
    @IsNeutered TINYINT,
    @PhotoPath NVARCHAR(200),
    @Remark NVARCHAR(500),
    @CreateUserId INT,
    @RescueTime DATETIME,
    @Location NVARCHAR(200),
    @FoundWay NVARCHAR(50),
    @InitialHealth NVARCHAR(500),
    @RescueRemark NVARCHAR(500),
    @CatId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        IF @TempName IS NULL OR LEN(LTRIM(RTRIM(@TempName))) = 0
           OR @Location IS NULL OR LEN(LTRIM(RTRIM(@Location))) = 0
           OR @InitialHealth IS NULL OR LEN(LTRIM(RTRIM(@InitialHealth))) = 0
            RAISERROR(N'请填写必填项', 16, 1);
        IF @FoundWay NOT IN (N'市民送交', N'工作人员发现', N'其他机构转入')
            RAISERROR(N'请选择发现方式', 16, 1);
        IF CAST(@RescueTime AS DATE) > CAST(GETDATE() AS DATE)
            RAISERROR(N'日期不能晚于今天', 16, 1);

        BEGIN TRANSACTION;
        DECLARE @Year INT = YEAR(GETDATE());
        DECLARE @Prefix CHAR(7) = 'CAT' + CONVERT(CHAR(4), @Year);
        DECLARE @MaxSeq INT;
        SELECT @MaxSeq = MAX(TRY_CONVERT(INT, SUBSTRING(CatNo, 8, 4)))
        FROM dbo.CatArchive WITH (UPDLOCK, HOLDLOCK)
        WHERE CatNo LIKE @Prefix + '[0-9][0-9][0-9][0-9]';
        IF @MaxSeq IS NULL SET @MaxSeq = 0;
        IF @MaxSeq >= 9999
            RAISERROR(N'当年档案编号已用完', 16, 1);

        DECLARE @CatNo CHAR(11) = @Prefix + RIGHT('0000' + CONVERT(VARCHAR(4), @MaxSeq + 1), 4);
        INSERT INTO dbo.CatArchive
            (CatNo, TempName, Gender, EstimatedAge, Color, IsNeutered, Status, TerminateReason, PhotoPath, Remark, CreateUserId)
        VALUES
            (@CatNo, LTRIM(RTRIM(@TempName)), @Gender, @EstimatedAge, @Color, @IsNeutered, 1, NULL, @PhotoPath, @Remark, @CreateUserId);
        SET @CatId = CONVERT(INT, SCOPE_IDENTITY());
        INSERT INTO dbo.RescueRecord
            (CatId, RescueTime, Location, FoundWay, InitialHealth, ReceiverUserId, Remark)
        VALUES
            (@CatId, @RescueTime, LTRIM(RTRIM(@Location)), @FoundWay, LTRIM(RTRIM(@InitialHealth)), @CreateUserId, @RescueRemark);
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

IF OBJECT_ID(N'dbo.usp_Cat_Update', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Cat_Update;
GO
CREATE PROCEDURE dbo.usp_Cat_Update
    @CatId INT,
    @TempName NVARCHAR(50),
    @Gender TINYINT,
    @EstimatedAge NVARCHAR(20),
    @Color NVARCHAR(50),
    @IsNeutered TINYINT,
    @PhotoPath NVARCHAR(200),
    @Remark NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Status TINYINT;
    SELECT @Status = Status FROM dbo.CatArchive WHERE CatId = @CatId;
    IF @Status IS NULL
    BEGIN
        RAISERROR(N'记录不存在或已被删除', 16, 1);
        RETURN;
    END
    IF @Status IN (4, 5)
    BEGIN
        RAISERROR(N'当前状态不允许此操作', 16, 1);
        RETURN;
    END
    UPDATE dbo.CatArchive
    SET TempName = @TempName,
        Gender = @Gender,
        EstimatedAge = @EstimatedAge,
        Color = @Color,
        IsNeutered = @IsNeutered,
        PhotoPath = @PhotoPath,
        Remark = @Remark,
        UpdateTime = GETDATE()
    WHERE CatId = @CatId;
END
GO

IF OBJECT_ID(N'dbo.usp_Cat_GetDetail', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Cat_GetDetail;
GO
CREATE PROCEDURE dbo.usp_Cat_GetDetail
    @CatId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        c.CatId, c.CatNo, c.TempName, c.Gender, c.EstimatedAge, c.Color, c.IsNeutered,
        c.Status, c.TerminateReason, c.PhotoPath, c.Remark, c.CreateUserId, c.CreateTime, c.UpdateTime,
        r.RescueId, r.RescueTime, r.Location, r.FoundWay, r.InitialHealth, r.ReceiverUserId,
        r.Remark AS RescueRemark,
        (SELECT COUNT(*) FROM dbo.MedicalRecord AS m WHERE m.CatId = c.CatId) AS MedicalCount,
        (SELECT COUNT(*) FROM dbo.AdoptionRecord AS a WHERE a.CatId = c.CatId) AS AdoptionCount
    FROM dbo.CatArchive AS c
    INNER JOIN dbo.RescueRecord AS r ON r.CatId = c.CatId
    WHERE c.CatId = @CatId;
END
GO

IF OBJECT_ID(N'dbo.usp_Cat_Search', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Cat_Search;
GO
CREATE PROCEDURE dbo.usp_Cat_Search
    @Keyword NVARCHAR(50),
    @Status TINYINT,
    @Gender TINYINT,
    @RescueFrom DATETIME,
    @RescueTo DATETIME,
    @PageIndex INT,
    @PageSize INT,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    IF @PageIndex IS NULL OR @PageIndex < 1 SET @PageIndex = 1;
    IF @PageIndex > 100000 SET @PageIndex = 100000;
    IF @PageSize IS NULL OR @PageSize < 1 OR @PageSize > 50 SET @PageSize = 10;

    DECLARE @Like NVARCHAR(120) = NULL;
    IF @Keyword IS NOT NULL AND LEN(LTRIM(RTRIM(@Keyword))) > 0
    BEGIN
        DECLARE @Text NVARCHAR(50) = LTRIM(RTRIM(@Keyword));
        SET @Text = REPLACE(REPLACE(REPLACE(@Text, N'[', N'[[]'), N'%', N'[%]'), N'_', N'[_]');
        SET @Like = N'%' + @Text + N'%';
    END

    SELECT @TotalCount = COUNT(*)
    FROM dbo.CatArchive AS c
    INNER JOIN dbo.RescueRecord AS r ON r.CatId = c.CatId
    WHERE (@Like IS NULL OR c.CatNo LIKE @Like OR c.TempName LIKE @Like)
      AND (@Status IS NULL OR c.Status = @Status)
      AND (@Gender IS NULL OR c.Gender = @Gender)
      AND (@RescueFrom IS NULL OR r.RescueTime >= @RescueFrom)
      AND (@RescueTo IS NULL OR r.RescueTime < DATEADD(DAY, 1, CAST(@RescueTo AS DATE)));

    SELECT c.CatId, c.CatNo, c.TempName, c.Gender, c.Status, r.RescueTime
    FROM dbo.CatArchive AS c
    INNER JOIN dbo.RescueRecord AS r ON r.CatId = c.CatId
    WHERE (@Like IS NULL OR c.CatNo LIKE @Like OR c.TempName LIKE @Like)
      AND (@Status IS NULL OR c.Status = @Status)
      AND (@Gender IS NULL OR c.Gender = @Gender)
      AND (@RescueFrom IS NULL OR r.RescueTime >= @RescueFrom)
      AND (@RescueTo IS NULL OR r.RescueTime < DATEADD(DAY, 1, CAST(@RescueTo AS DATE)))
    ORDER BY c.CreateTime DESC, c.CatNo DESC
    OFFSET (@PageIndex - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

IF OBJECT_ID(N'dbo.usp_Cat_Delete', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Cat_Delete;
GO
CREATE PROCEDURE dbo.usp_Cat_Delete
    @CatId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.CatArchive WITH (UPDLOCK, HOLDLOCK) WHERE CatId = @CatId)
            RAISERROR(N'记录不存在或已被删除', 16, 1);
        IF EXISTS (SELECT 1 FROM dbo.MedicalRecord WHERE CatId = @CatId)
           OR EXISTS (SELECT 1 FROM dbo.AdoptionRecord WHERE CatId = @CatId)
            RAISERROR(N'存在医疗或领养记录，不能删除该档案', 16, 1);
        DELETE FROM dbo.RescueRecord WHERE CatId = @CatId;
        DELETE FROM dbo.CatArchive WHERE CatId = @CatId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

IF OBJECT_ID(N'dbo.usp_Cat_ChangeStatus', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Cat_ChangeStatus;
GO
CREATE PROCEDURE dbo.usp_Cat_ChangeStatus
    @CatId INT,
    @TargetStatus TINYINT,
    @TerminateReason NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Status TINYINT;
        SELECT @Status = Status FROM dbo.CatArchive WITH (UPDLOCK, HOLDLOCK) WHERE CatId = @CatId;
        IF @Status IS NULL
            RAISERROR(N'记录不存在或已被删除', 16, 1);

        DECLARE @Reason NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@TerminateReason)), N'');
        DECLARE @Allowed BIT = 0;
        IF @Status = 1 AND @TargetStatus = 2 SET @Allowed = 1;
        IF @Status = 2 AND @TargetStatus = 1 SET @Allowed = 1;
        IF @Status = 3 AND @TargetStatus = 4
        BEGIN
            IF NOT EXISTS (
                SELECT 1
                FROM dbo.FollowUpRecord AS f
                INNER JOIN dbo.AdoptionRecord AS a ON a.AdoptionId = f.AdoptionId
                WHERE a.CatId = @CatId AND a.Status = 1)
                RAISERROR(N'至少有一条回访才能结案', 16, 1);
            SET @Allowed = 1;
        END
        IF (@Status = 1 OR @Status = 2) AND @TargetStatus = 5
        BEGIN
            IF @Reason IS NULL
                RAISERROR(N'请填写终止原因', 16, 1);
            SET @Allowed = 1;
        END
        IF @Allowed = 0
            RAISERROR(N'当前状态不允许此操作', 16, 1);

        UPDATE dbo.CatArchive
        SET Status = @TargetStatus,
            TerminateReason = CASE WHEN @TargetStatus = 5 THEN @Reason ELSE NULL END,
            UpdateTime = GETDATE()
        WHERE CatId = @CatId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

IF OBJECT_ID(N'dbo.usp_Cat_Recent', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Cat_Recent;
GO
CREATE PROCEDURE dbo.usp_Cat_Recent
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (8)
        c.CatId, c.CatNo, c.TempName, c.Gender, c.Status, r.RescueTime
    FROM dbo.CatArchive AS c
    LEFT JOIN dbo.RescueRecord AS r ON r.CatId = c.CatId
    ORDER BY c.CreateTime DESC, c.CatNo DESC;
END
GO

IF OBJECT_ID(N'dbo.usp_Rescue_Update', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Rescue_Update;
GO
CREATE PROCEDURE dbo.usp_Rescue_Update
    @CatId INT,
    @RescueTime DATETIME,
    @Location NVARCHAR(200),
    @FoundWay NVARCHAR(50),
    @InitialHealth NVARCHAR(500),
    @Remark NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Status TINYINT;
    SELECT @Status = Status FROM dbo.CatArchive WHERE CatId = @CatId;
    IF @Status IS NULL
    BEGIN
        RAISERROR(N'记录不存在或已被删除', 16, 1);
        RETURN;
    END
    IF @Status IN (4, 5)
    BEGIN
        RAISERROR(N'当前状态不允许此操作', 16, 1);
        RETURN;
    END
    IF @FoundWay NOT IN (N'市民送交', N'工作人员发现', N'其他机构转入')
    BEGIN
        RAISERROR(N'请选择发现方式', 16, 1);
        RETURN;
    END
    IF CAST(@RescueTime AS DATE) > CAST(GETDATE() AS DATE)
    BEGIN
        RAISERROR(N'日期不能晚于今天', 16, 1);
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM dbo.MedicalRecord WHERE CatId = @CatId AND MedicalDate < CAST(@RescueTime AS DATE))
    BEGIN
        RAISERROR(N'医疗日期不能早于接收日期', 16, 1);
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM dbo.AdoptionRecord WHERE CatId = @CatId AND AdoptDate < CAST(@RescueTime AS DATE))
    BEGIN
        RAISERROR(N'领养日期不能早于接收日期', 16, 1);
        RETURN;
    END
    UPDATE dbo.RescueRecord
    SET RescueTime = @RescueTime,
        Location = @Location,
        FoundWay = @FoundWay,
        InitialHealth = @InitialHealth,
        Remark = @Remark
    WHERE CatId = @CatId;
END
GO

IF OBJECT_ID(N'dbo.usp_Rescue_GetByCatId', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Rescue_GetByCatId;
GO
CREATE PROCEDURE dbo.usp_Rescue_GetByCatId
    @CatId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RescueId, CatId, RescueTime, Location, FoundWay, InitialHealth, ReceiverUserId, Remark
    FROM dbo.RescueRecord
    WHERE CatId = @CatId;
END
GO

IF OBJECT_ID(N'dbo.usp_Medical_Insert', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Medical_Insert;
GO
CREATE PROCEDURE dbo.usp_Medical_Insert
    @CatId INT,
    @MedicalDate DATE,
    @Diagnosis NVARCHAR(200),
    @Treatment NVARCHAR(500),
    @Medicine NVARCHAR(200),
    @RecordUserId INT,
    @Remark NVARCHAR(500),
    @MedicalId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Status TINYINT;
    DECLARE @RescueDate DATE;
    SELECT @Status = c.Status, @RescueDate = CAST(r.RescueTime AS DATE)
    FROM dbo.CatArchive AS c
    LEFT JOIN dbo.RescueRecord AS r ON r.CatId = c.CatId
    WHERE c.CatId = @CatId;
    IF @Status IS NULL
    BEGIN
        RAISERROR(N'记录不存在或已被删除', 16, 1);
        RETURN;
    END
    IF @Status IN (4, 5)
    BEGIN
        RAISERROR(N'当前状态不允许此操作', 16, 1);
        RETURN;
    END
    IF @MedicalDate > CAST(GETDATE() AS DATE)
    BEGIN
        RAISERROR(N'日期不能晚于今天', 16, 1);
        RETURN;
    END
    IF @RescueDate IS NOT NULL AND @MedicalDate < @RescueDate
    BEGIN
        RAISERROR(N'医疗日期不能早于接收日期', 16, 1);
        RETURN;
    END
    INSERT INTO dbo.MedicalRecord (CatId, MedicalDate, Diagnosis, Treatment, Medicine, RecordUserId, Remark)
    VALUES (@CatId, @MedicalDate, @Diagnosis, @Treatment, @Medicine, @RecordUserId, @Remark);
    SET @MedicalId = CONVERT(INT, SCOPE_IDENTITY());
END
GO

IF OBJECT_ID(N'dbo.usp_Medical_Update', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Medical_Update;
GO
CREATE PROCEDURE dbo.usp_Medical_Update
    @MedicalId INT,
    @MedicalDate DATE,
    @Diagnosis NVARCHAR(200),
    @Treatment NVARCHAR(500),
    @Medicine NVARCHAR(200),
    @Remark NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @CatId INT;
    DECLARE @Status TINYINT;
    DECLARE @RescueDate DATE;
    SELECT @CatId = m.CatId, @Status = c.Status, @RescueDate = CAST(r.RescueTime AS DATE)
    FROM dbo.MedicalRecord AS m
    INNER JOIN dbo.CatArchive AS c ON c.CatId = m.CatId
    LEFT JOIN dbo.RescueRecord AS r ON r.CatId = c.CatId
    WHERE m.MedicalId = @MedicalId;
    IF @CatId IS NULL
    BEGIN
        RAISERROR(N'记录不存在或已被删除', 16, 1);
        RETURN;
    END
    IF @Status IN (4, 5)
    BEGIN
        RAISERROR(N'当前状态不允许此操作', 16, 1);
        RETURN;
    END
    IF @MedicalDate > CAST(GETDATE() AS DATE)
    BEGIN
        RAISERROR(N'日期不能晚于今天', 16, 1);
        RETURN;
    END
    IF @RescueDate IS NOT NULL AND @MedicalDate < @RescueDate
    BEGIN
        RAISERROR(N'医疗日期不能早于接收日期', 16, 1);
        RETURN;
    END
    UPDATE dbo.MedicalRecord
    SET MedicalDate = @MedicalDate, Diagnosis = @Diagnosis, Treatment = @Treatment, Medicine = @Medicine, Remark = @Remark
    WHERE MedicalId = @MedicalId;
END
GO

IF OBJECT_ID(N'dbo.usp_Medical_Delete', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Medical_Delete;
GO
CREATE PROCEDURE dbo.usp_Medical_Delete
    @MedicalId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Status TINYINT;
    SELECT @Status = c.Status
    FROM dbo.MedicalRecord AS m
    INNER JOIN dbo.CatArchive AS c ON c.CatId = m.CatId
    WHERE m.MedicalId = @MedicalId;
    IF @Status IS NULL
    BEGIN
        RAISERROR(N'记录不存在或已被删除', 16, 1);
        RETURN;
    END
    IF @Status IN (4, 5)
    BEGIN
        RAISERROR(N'当前状态不允许此操作', 16, 1);
        RETURN;
    END
    DELETE FROM dbo.MedicalRecord WHERE MedicalId = @MedicalId;
END
GO

IF OBJECT_ID(N'dbo.usp_Medical_GetById', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Medical_GetById;
GO
CREATE PROCEDURE dbo.usp_Medical_GetById
    @MedicalId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MedicalId, CatId, MedicalDate, Diagnosis, Treatment, Medicine, RecordUserId, Remark, CreateTime
    FROM dbo.MedicalRecord
    WHERE MedicalId = @MedicalId;
END
GO

IF OBJECT_ID(N'dbo.usp_Medical_ListByCat', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Medical_ListByCat;
GO
CREATE PROCEDURE dbo.usp_Medical_ListByCat
    @CatId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MedicalId, CatId, MedicalDate, Diagnosis, Treatment, Medicine, RecordUserId, Remark, CreateTime
    FROM dbo.MedicalRecord
    WHERE CatId = @CatId
    ORDER BY MedicalDate DESC, MedicalId DESC;
END
GO

IF OBJECT_ID(N'dbo.usp_Adoption_Create', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Adoption_Create;
GO
CREATE PROCEDURE dbo.usp_Adoption_Create
    @CatId INT,
    @AdopterName NVARCHAR(50),
    @AdopterPhone NVARCHAR(20),
    @AdopterAddress NVARCHAR(200),
    @AdoptDate DATE,
    @HandlerUserId INT,
    @Remark NVARCHAR(500),
    @AdoptionId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Status TINYINT;
        DECLARE @RescueDate DATE;
        SELECT @Status = c.Status, @RescueDate = CAST(r.RescueTime AS DATE)
        FROM dbo.CatArchive AS c WITH (UPDLOCK, HOLDLOCK)
        LEFT JOIN dbo.RescueRecord AS r ON r.CatId = c.CatId
        WHERE c.CatId = @CatId;
        IF @Status IS NULL
            RAISERROR(N'记录不存在或已被删除', 16, 1);
        IF @Status <> 2
            RAISERROR(N'当前状态不允许此操作', 16, 1);
        IF EXISTS (SELECT 1 FROM dbo.AdoptionRecord WITH (UPDLOCK, HOLDLOCK) WHERE CatId = @CatId AND Status = 1)
            RAISERROR(N'该猫已有一条有效领养', 16, 1);
        IF @AdoptDate > CAST(GETDATE() AS DATE)
            RAISERROR(N'日期不能晚于今天', 16, 1);
        IF @RescueDate IS NOT NULL AND @AdoptDate < @RescueDate
            RAISERROR(N'领养日期不能早于接收日期', 16, 1);

        INSERT INTO dbo.AdoptionRecord
            (CatId, AdopterName, AdopterPhone, AdopterAddress, AdoptDate, Status, ReturnDate, ReturnReason, HandlerUserId, Remark)
        VALUES
            (@CatId, @AdopterName, @AdopterPhone, @AdopterAddress, @AdoptDate, 1, NULL, NULL, @HandlerUserId, @Remark);
        SET @AdoptionId = CONVERT(INT, SCOPE_IDENTITY());
        UPDATE dbo.CatArchive
        SET Status = 3, TerminateReason = NULL, UpdateTime = GETDATE()
        WHERE CatId = @CatId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

IF OBJECT_ID(N'dbo.usp_Adoption_Update', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Adoption_Update;
GO
CREATE PROCEDURE dbo.usp_Adoption_Update
    @AdoptionId INT,
    @AdopterName NVARCHAR(50),
    @AdopterPhone NVARCHAR(20),
    @AdopterAddress NVARCHAR(200),
    @Remark NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AdoptionStatus TINYINT;
    DECLARE @CatStatus TINYINT;
    SELECT @AdoptionStatus = a.Status, @CatStatus = c.Status
    FROM dbo.AdoptionRecord AS a
    INNER JOIN dbo.CatArchive AS c ON c.CatId = a.CatId
    WHERE a.AdoptionId = @AdoptionId;
    IF @AdoptionStatus IS NULL
    BEGIN
        RAISERROR(N'记录不存在或已被删除', 16, 1);
        RETURN;
    END
    IF @AdoptionStatus <> 1 OR @CatStatus <> 3
    BEGIN
        RAISERROR(N'当前状态不允许此操作', 16, 1);
        RETURN;
    END
    UPDATE dbo.AdoptionRecord
    SET AdopterName = @AdopterName, AdopterPhone = @AdopterPhone, AdopterAddress = @AdopterAddress, Remark = @Remark
    WHERE AdoptionId = @AdoptionId;
END
GO

IF OBJECT_ID(N'dbo.usp_Adoption_Return', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Adoption_Return;
GO
CREATE PROCEDURE dbo.usp_Adoption_Return
    @AdoptionId INT,
    @ReturnDate DATE,
    @ReturnReason NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @AdoptionStatus TINYINT;
        DECLARE @CatStatus TINYINT;
        DECLARE @CatId INT;
        DECLARE @AdoptDate DATE;
        SELECT @AdoptionStatus = a.Status, @CatId = a.CatId, @AdoptDate = a.AdoptDate, @CatStatus = c.Status
        FROM dbo.AdoptionRecord AS a WITH (UPDLOCK, HOLDLOCK)
        INNER JOIN dbo.CatArchive AS c WITH (UPDLOCK, HOLDLOCK) ON c.CatId = a.CatId
        WHERE a.AdoptionId = @AdoptionId;
        IF @AdoptionStatus IS NULL
            RAISERROR(N'记录不存在或已被删除', 16, 1);
        IF @AdoptionStatus <> 1 OR @CatStatus <> 3
            RAISERROR(N'当前状态不允许此操作', 16, 1);
        IF @ReturnReason IS NULL OR LEN(LTRIM(RTRIM(@ReturnReason))) = 0
            RAISERROR(N'请填写必填项', 16, 1);
        IF @ReturnDate > CAST(GETDATE() AS DATE)
            RAISERROR(N'日期不能晚于今天', 16, 1);
        IF @ReturnDate < @AdoptDate
            RAISERROR(N'退养日期不能早于领养日期', 16, 1);

        UPDATE dbo.AdoptionRecord
        SET Status = 2, ReturnDate = @ReturnDate, ReturnReason = LTRIM(RTRIM(@ReturnReason))
        WHERE AdoptionId = @AdoptionId;
        UPDATE dbo.CatArchive
        SET Status = 2, TerminateReason = NULL, UpdateTime = GETDATE()
        WHERE CatId = @CatId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

IF OBJECT_ID(N'dbo.usp_Adoption_Delete', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Adoption_Delete;
GO
CREATE PROCEDURE dbo.usp_Adoption_Delete
    @AdoptionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @AdoptionStatus TINYINT;
        DECLARE @CatStatus TINYINT;
        DECLARE @CatId INT;
        DECLARE @FollowCount INT;
        SELECT @AdoptionStatus = a.Status, @CatId = a.CatId, @CatStatus = c.Status,
               @FollowCount = (SELECT COUNT(*) FROM dbo.FollowUpRecord AS f WHERE f.AdoptionId = a.AdoptionId)
        FROM dbo.AdoptionRecord AS a WITH (UPDLOCK, HOLDLOCK)
        INNER JOIN dbo.CatArchive AS c WITH (UPDLOCK, HOLDLOCK) ON c.CatId = a.CatId
        WHERE a.AdoptionId = @AdoptionId;
        IF @AdoptionStatus IS NULL
            RAISERROR(N'记录不存在或已被删除', 16, 1);
        IF @CatStatus IN (4, 5)
            RAISERROR(N'当前状态不允许此操作', 16, 1);
        IF @FollowCount > 0
            RAISERROR(N'存在回访记录，不能删除该领养', 16, 1);
        IF @AdoptionStatus = 1
            UPDATE dbo.CatArchive SET Status = 2, TerminateReason = NULL, UpdateTime = GETDATE() WHERE CatId = @CatId;
        DELETE FROM dbo.AdoptionRecord WHERE AdoptionId = @AdoptionId;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

IF OBJECT_ID(N'dbo.usp_Adoption_GetById', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Adoption_GetById;
GO
CREATE PROCEDURE dbo.usp_Adoption_GetById
    @AdoptionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.AdoptionId, a.CatId, a.AdopterName, a.AdopterPhone, a.AdopterAddress, a.AdoptDate,
           a.Status, a.ReturnDate, a.ReturnReason, a.HandlerUserId, a.Remark, a.CreateTime,
           (SELECT COUNT(*) FROM dbo.FollowUpRecord AS f WHERE f.AdoptionId = a.AdoptionId) AS FollowUpCount
    FROM dbo.AdoptionRecord AS a
    WHERE a.AdoptionId = @AdoptionId;
END
GO

IF OBJECT_ID(N'dbo.usp_Adoption_ListByCat', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Adoption_ListByCat;
GO
CREATE PROCEDURE dbo.usp_Adoption_ListByCat
    @CatId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT a.AdoptionId, a.CatId, a.AdopterName, a.AdopterPhone, a.AdopterAddress, a.AdoptDate,
           a.Status, a.ReturnDate, a.ReturnReason, a.HandlerUserId, a.Remark, a.CreateTime,
           (SELECT COUNT(*) FROM dbo.FollowUpRecord AS f WHERE f.AdoptionId = a.AdoptionId) AS FollowUpCount
    FROM dbo.AdoptionRecord AS a
    WHERE a.CatId = @CatId
    ORDER BY a.CreateTime DESC, a.AdoptionId DESC;
END
GO

IF OBJECT_ID(N'dbo.usp_FollowUp_Insert', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_FollowUp_Insert;
GO
CREATE PROCEDURE dbo.usp_FollowUp_Insert
    @AdoptionId INT,
    @FollowDate DATE,
    @FollowWay NVARCHAR(50),
    @CatCondition NVARCHAR(500),
    @RecordUserId INT,
    @Remark NVARCHAR(500),
    @FollowUpId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @CatStatus TINYINT;
    DECLARE @AdoptionStatus TINYINT;
    DECLARE @AdoptDate DATE;
    SELECT @CatStatus = c.Status, @AdoptionStatus = a.Status, @AdoptDate = a.AdoptDate
    FROM dbo.AdoptionRecord AS a
    INNER JOIN dbo.CatArchive AS c ON c.CatId = a.CatId
    WHERE a.AdoptionId = @AdoptionId;
    IF @AdoptionStatus IS NULL
    BEGIN
        RAISERROR(N'记录不存在或已被删除', 16, 1);
        RETURN;
    END
    IF @CatStatus <> 3 OR @AdoptionStatus <> 1
    BEGIN
        RAISERROR(N'当前状态不允许此操作', 16, 1);
        RETURN;
    END
    IF @FollowWay NOT IN (N'电话', N'上门', N'微信')
    BEGIN
        RAISERROR(N'请选择回访方式', 16, 1);
        RETURN;
    END
    IF @FollowDate > CAST(GETDATE() AS DATE)
    BEGIN
        RAISERROR(N'日期不能晚于今天', 16, 1);
        RETURN;
    END
    IF @FollowDate < @AdoptDate
    BEGIN
        RAISERROR(N'回访日期不能早于领养日期', 16, 1);
        RETURN;
    END
    INSERT INTO dbo.FollowUpRecord (AdoptionId, FollowDate, FollowWay, CatCondition, RecordUserId, Remark)
    VALUES (@AdoptionId, @FollowDate, @FollowWay, @CatCondition, @RecordUserId, @Remark);
    SET @FollowUpId = CONVERT(INT, SCOPE_IDENTITY());
END
GO

IF OBJECT_ID(N'dbo.usp_FollowUp_Update', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_FollowUp_Update;
GO
CREATE PROCEDURE dbo.usp_FollowUp_Update
    @FollowUpId INT,
    @FollowDate DATE,
    @FollowWay NVARCHAR(50),
    @CatCondition NVARCHAR(500),
    @Remark NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @CatStatus TINYINT;
    DECLARE @AdoptionStatus TINYINT;
    DECLARE @AdoptDate DATE;
    SELECT @CatStatus = c.Status, @AdoptionStatus = a.Status, @AdoptDate = a.AdoptDate
    FROM dbo.FollowUpRecord AS f
    INNER JOIN dbo.AdoptionRecord AS a ON a.AdoptionId = f.AdoptionId
    INNER JOIN dbo.CatArchive AS c ON c.CatId = a.CatId
    WHERE f.FollowUpId = @FollowUpId;
    IF @AdoptionStatus IS NULL
    BEGIN
        RAISERROR(N'记录不存在或已被删除', 16, 1);
        RETURN;
    END
    IF @CatStatus <> 3 OR @AdoptionStatus <> 1
    BEGIN
        RAISERROR(N'当前状态不允许此操作', 16, 1);
        RETURN;
    END
    IF @FollowWay NOT IN (N'电话', N'上门', N'微信')
    BEGIN
        RAISERROR(N'请选择回访方式', 16, 1);
        RETURN;
    END
    IF @FollowDate > CAST(GETDATE() AS DATE)
    BEGIN
        RAISERROR(N'日期不能晚于今天', 16, 1);
        RETURN;
    END
    IF @FollowDate < @AdoptDate
    BEGIN
        RAISERROR(N'回访日期不能早于领养日期', 16, 1);
        RETURN;
    END
    UPDATE dbo.FollowUpRecord
    SET FollowDate = @FollowDate, FollowWay = @FollowWay, CatCondition = @CatCondition, Remark = @Remark
    WHERE FollowUpId = @FollowUpId;
END
GO

IF OBJECT_ID(N'dbo.usp_FollowUp_Delete', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_FollowUp_Delete;
GO
CREATE PROCEDURE dbo.usp_FollowUp_Delete
    @FollowUpId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @CatStatus TINYINT;
    SELECT @CatStatus = c.Status
    FROM dbo.FollowUpRecord AS f
    INNER JOIN dbo.AdoptionRecord AS a ON a.AdoptionId = f.AdoptionId
    INNER JOIN dbo.CatArchive AS c ON c.CatId = a.CatId
    WHERE f.FollowUpId = @FollowUpId;
    IF @CatStatus IS NULL
    BEGIN
        RAISERROR(N'记录不存在或已被删除', 16, 1);
        RETURN;
    END
    IF @CatStatus IN (4, 5)
    BEGIN
        RAISERROR(N'当前状态不允许此操作', 16, 1);
        RETURN;
    END
    DELETE FROM dbo.FollowUpRecord WHERE FollowUpId = @FollowUpId;
END
GO

IF OBJECT_ID(N'dbo.usp_FollowUp_GetById', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_FollowUp_GetById;
GO
CREATE PROCEDURE dbo.usp_FollowUp_GetById
    @FollowUpId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FollowUpId, AdoptionId, FollowDate, FollowWay, CatCondition, RecordUserId, Remark, CreateTime
    FROM dbo.FollowUpRecord
    WHERE FollowUpId = @FollowUpId;
END
GO

IF OBJECT_ID(N'dbo.usp_FollowUp_ListByAdoption', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_FollowUp_ListByAdoption;
GO
CREATE PROCEDURE dbo.usp_FollowUp_ListByAdoption
    @AdoptionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT FollowUpId, AdoptionId, FollowDate, FollowWay, CatCondition, RecordUserId, Remark, CreateTime
    FROM dbo.FollowUpRecord
    WHERE AdoptionId = @AdoptionId
    ORDER BY FollowDate DESC, FollowUpId DESC;
END
GO

IF OBJECT_ID(N'dbo.usp_FollowUp_CountByAdoption', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_FollowUp_CountByAdoption;
GO
CREATE PROCEDURE dbo.usp_FollowUp_CountByAdoption
    @AdoptionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM dbo.FollowUpRecord WHERE AdoptionId = @AdoptionId;
END
GO

IF OBJECT_ID(N'dbo.usp_Stat_Monthly', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Stat_Monthly;
GO
CREATE PROCEDURE dbo.usp_Stat_Monthly
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        months.MonthNum,
        (SELECT COUNT(*) FROM dbo.RescueRecord WHERE YEAR(RescueTime) = @Year AND MONTH(RescueTime) = months.MonthNum) AS RescueCount,
        (SELECT COUNT(*) FROM dbo.AdoptionRecord WHERE YEAR(AdoptDate) = @Year AND MONTH(AdoptDate) = months.MonthNum) AS AdoptionCount
    FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12)) AS months(MonthNum)
    ORDER BY months.MonthNum;
END
GO

IF OBJECT_ID(N'dbo.usp_Stat_Years', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Stat_Years;
GO
CREATE PROCEDURE dbo.usp_Stat_Years
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DISTINCT YEAR(RescueTime) AS YearNum
    FROM dbo.RescueRecord
    ORDER BY YearNum;
END
GO
