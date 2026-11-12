SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
USE StrayCatArchive;
GO

IF OBJECT_ID(N'dbo.usp_Cat_Delete', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Cat_Delete;
GO
CREATE PROCEDURE dbo.usp_Cat_Delete
    @CatId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.StrayReport WHERE CatId = @CatId)
    BEGIN
        RAISERROR(N'该档案来自市民上报，不能删除', 16, 1);
        RETURN;
    END
    IF EXISTS (SELECT 1 FROM dbo.AdoptionApplication WHERE CatId = @CatId)
    BEGIN
        RAISERROR(N'该档案存在领养申请，不能删除', 16, 1);
        RETURN;
    END
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

IF OBJECT_ID(N'dbo.usp_Adoption_Delete', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Adoption_Delete;
GO
CREATE PROCEDURE dbo.usp_Adoption_Delete
    @AdoptionId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.AdoptionApplication WHERE AdoptionId = @AdoptionId)
    BEGIN
        RAISERROR(N'该领养记录来自领养申请，不能删除', 16, 1);
        RETURN;
    END
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
