SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
USE StrayCatArchive;
GO

IF OBJECT_ID(N'dbo.usp_User_GetByUserName', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_User_GetByUserName;
IF OBJECT_ID(N'dbo.usp_User_CountEnabled', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_User_CountEnabled;
IF OBJECT_ID(N'dbo.usp_Cat_HomeCounts', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Cat_HomeCounts;
IF OBJECT_ID(N'dbo.usp_Cat_Recent', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_Cat_Recent;
GO

CREATE PROCEDURE dbo.usp_User_GetByUserName
    @UserName NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId, UserName, PasswordHash, Salt, RealName, Role, Phone, IsEnabled, CreateTime
    FROM dbo.SysUser
    WHERE UserName = @UserName;
END
GO

CREATE PROCEDURE dbo.usp_User_CountEnabled
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) FROM dbo.SysUser WHERE IsEnabled = 1;
END
GO

CREATE PROCEDURE dbo.usp_Cat_HomeCounts
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.CatArchive WHERE Status = 1) AS InCareCount,
        (SELECT COUNT(*) FROM dbo.CatArchive WHERE Status = 2) AS WaitingCount,
        (SELECT COUNT(*)
         FROM dbo.CatArchive AS c
         WHERE c.Status = 3
           AND EXISTS (
                SELECT 1
                FROM dbo.AdoptionRecord AS a
                WHERE a.CatId = c.CatId
                  AND a.Status = 1
                  AND NOT EXISTS (
                      SELECT 1 FROM dbo.FollowUpRecord AS f WHERE f.AdoptionId = a.AdoptionId))) AS AdoptedWithoutFollowUpCount;
END
GO

CREATE PROCEDURE dbo.usp_Cat_Recent
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (8)
        c.CatId,
        c.CatNo,
        c.TempName,
        c.Gender,
        c.Status,
        r.RescueTime
    FROM dbo.CatArchive AS c
    LEFT JOIN dbo.RescueRecord AS r ON r.CatId = c.CatId
    ORDER BY c.CreateTime DESC, c.CatNo DESC;
END
GO
