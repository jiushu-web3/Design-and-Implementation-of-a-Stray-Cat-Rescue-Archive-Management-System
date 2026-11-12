SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
USE StrayCatArchive;
GO

IF OBJECT_ID(N'dbo.usp_Medical_Recent', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Medical_Recent;
GO
CREATE PROCEDURE dbo.usp_Medical_Recent
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (50)
        m.MedicalId, m.CatId, m.MedicalDate, m.Diagnosis, m.Treatment, m.Medicine,
        m.RecordUserId, m.Remark, m.CreateTime, c.CatNo, c.TempName
    FROM dbo.MedicalRecord AS m
    INNER JOIN dbo.CatArchive AS c ON c.CatId = m.CatId
    ORDER BY m.MedicalDate DESC, m.MedicalId DESC;
END
GO

IF OBJECT_ID(N'dbo.usp_Adoption_Recent', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Adoption_Recent;
GO
CREATE PROCEDURE dbo.usp_Adoption_Recent
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (50)
        a.AdoptionId, a.CatId, a.AdopterName, a.AdopterPhone, a.AdopterAddress, a.AdoptDate,
        a.Status, a.ReturnDate, a.ReturnReason, a.HandlerUserId, a.Remark, a.CreateTime,
        (SELECT COUNT(*) FROM dbo.FollowUpRecord AS f WHERE f.AdoptionId = a.AdoptionId) AS FollowUpCount,
        c.CatNo, c.TempName
    FROM dbo.AdoptionRecord AS a
    INNER JOIN dbo.CatArchive AS c ON c.CatId = a.CatId
    ORDER BY a.AdoptDate DESC, a.AdoptionId DESC;
END
GO

IF OBJECT_ID(N'dbo.usp_FollowUp_Recent', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_FollowUp_Recent;
GO
CREATE PROCEDURE dbo.usp_FollowUp_Recent
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (50)
        f.FollowUpId, f.AdoptionId, f.FollowDate, f.FollowWay, f.CatCondition,
        f.RecordUserId, f.Remark, f.CreateTime, c.CatNo, c.TempName
    FROM dbo.FollowUpRecord AS f
    INNER JOIN dbo.AdoptionRecord AS a ON a.AdoptionId = f.AdoptionId
    INNER JOIN dbo.CatArchive AS c ON c.CatId = a.CatId
    ORDER BY f.FollowDate DESC, f.FollowUpId DESC;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.CatArchive)
BEGIN
    DECLARE @AdminId INT;
    SELECT @AdminId = UserId FROM dbo.SysUser WHERE UserName = N'admin' AND IsEnabled = 1;
    IF @AdminId IS NOT NULL
    BEGIN
        DECLARE @Cat1 INT, @Cat2 INT, @Cat3 INT, @Cat4 INT, @Adopt4 INT;
        DECLARE @Today DATE = CONVERT(DATE, GETDATE());

        INSERT INTO dbo.CatArchive
            (CatNo, TempName, Gender, EstimatedAge, Color, IsNeutered, Status, TerminateReason, Remark, CreateUserId, CreateTime)
        VALUES
            ('CAT20260001', N'小橘', 1, N'约 1 岁', N'橘色', 0, 1, NULL, N'活动图示例：在站照料', @AdminId, DATEADD(DAY, -20, GETDATE()));
        SET @Cat1 = SCOPE_IDENTITY();
        INSERT INTO dbo.RescueRecord (CatId, RescueTime, Location, FoundWay, InitialHealth, ReceiverUserId, Remark)
        VALUES (@Cat1, DATEADD(DAY, -20, GETDATE()), N'校园东门', N'工作人员发现', N'消瘦，右前爪轻微擦伤', @AdminId, NULL);
        INSERT INTO dbo.MedicalRecord (CatId, MedicalDate, Diagnosis, Treatment, Medicine, RecordUserId, Remark, CreateTime)
        VALUES (@Cat1, DATEADD(DAY, -18, @Today), N'外伤', N'清创包扎', N'碘伏', @AdminId, NULL, DATEADD(DAY, -18, GETDATE()));

        INSERT INTO dbo.CatArchive
            (CatNo, TempName, Gender, EstimatedAge, Color, IsNeutered, Status, TerminateReason, Remark, CreateUserId, CreateTime)
        VALUES
            ('CAT20260002', N'小白', 2, N'约 2 岁', N'白色', 1, 2, NULL, N'活动图示例：待领养', @AdminId, DATEADD(DAY, -35, GETDATE()));
        SET @Cat2 = SCOPE_IDENTITY();
        INSERT INTO dbo.RescueRecord (CatId, RescueTime, Location, FoundWay, InitialHealth, ReceiverUserId, Remark)
        VALUES (@Cat2, DATEADD(DAY, -35, GETDATE()), N'图书馆后', N'市民送交', N'精神尚可，已完成体检', @AdminId, NULL);
        INSERT INTO dbo.MedicalRecord (CatId, MedicalDate, Diagnosis, Treatment, Medicine, RecordUserId, Remark, CreateTime)
        VALUES (@Cat2, DATEADD(DAY, -30, @Today), N'健康评估通过', N'免疫', N'疫苗', @AdminId, NULL, DATEADD(DAY, -30, GETDATE()));

        INSERT INTO dbo.CatArchive
            (CatNo, TempName, Gender, EstimatedAge, Color, IsNeutered, Status, TerminateReason, Remark, CreateUserId, CreateTime)
        VALUES
            ('CAT20260003', N'小黑', 1, N'约 8 个月', N'黑色', 1, 3, NULL, N'活动图示例：已领养未回访', @AdminId, DATEADD(DAY, -50, GETDATE()));
        SET @Cat3 = SCOPE_IDENTITY();
        INSERT INTO dbo.RescueRecord (CatId, RescueTime, Location, FoundWay, InitialHealth, ReceiverUserId, Remark)
        VALUES (@Cat3, DATEADD(DAY, -50, GETDATE()), N'食堂北侧', N'工作人员发现', N'活泼，无明显外伤', @AdminId, NULL);
        INSERT INTO dbo.AdoptionRecord
            (CatId, AdopterName, AdopterPhone, AdopterAddress, AdoptDate, Status, ReturnDate, ReturnReason, HandlerUserId, Remark, CreateTime)
        VALUES
            (@Cat3, N'张敏', N'13800138001', N'大连市沙河口区', DATEADD(DAY, -7, @Today), 1, NULL, NULL, @AdminId, NULL, DATEADD(DAY, -7, GETDATE()));

        INSERT INTO dbo.CatArchive
            (CatNo, TempName, Gender, EstimatedAge, Color, IsNeutered, Status, TerminateReason, Remark, CreateUserId, CreateTime)
        VALUES
            ('CAT20260004', N'小花', 2, N'约 3 岁', N'三花', 1, 4, NULL, N'活动图示例：已结案', @AdminId, DATEADD(DAY, -80, GETDATE()));
        SET @Cat4 = SCOPE_IDENTITY();
        INSERT INTO dbo.RescueRecord (CatId, RescueTime, Location, FoundWay, InitialHealth, ReceiverUserId, Remark)
        VALUES (@Cat4, DATEADD(DAY, -80, GETDATE()), N'宿舍区', N'其他机构转入', N'已绝育，待观察', @AdminId, NULL);
        INSERT INTO dbo.AdoptionRecord
            (CatId, AdopterName, AdopterPhone, AdopterAddress, AdoptDate, Status, ReturnDate, ReturnReason, HandlerUserId, Remark, CreateTime)
        VALUES
            (@Cat4, N'李强', N'13800138002', N'大连市甘井子区', DATEADD(DAY, -40, @Today), 1, NULL, NULL, @AdminId, NULL, DATEADD(DAY, -40, GETDATE()));
        SET @Adopt4 = SCOPE_IDENTITY();
        INSERT INTO dbo.FollowUpRecord (AdoptionId, FollowDate, FollowWay, CatCondition, RecordUserId, Remark, CreateTime)
        VALUES (@Adopt4, DATEADD(DAY, -20, @Today), N'电话', N'饮食正常，已适应新家', @AdminId, NULL, DATEADD(DAY, -20, GETDATE()));
    END
END
GO
