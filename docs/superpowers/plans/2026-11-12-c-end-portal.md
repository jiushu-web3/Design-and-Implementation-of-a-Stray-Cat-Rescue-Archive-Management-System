# 阶段一：角色权限与 C 端入口 实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 把现有“工作人员”收回为“普通用户”，并新增流浪猫上报、领养申请及其管理员审核闭环。

**Architecture:** 沿用现有 ASP.NET Web Forms 三层结构：页面 → BLL → DAL → DbHelper → 存储过程。新增两张表与一组 `usp_Report_*` / `usp_Application_*` 过程；审核通过时在存储过程事务内复用 `usp_Cat_CreateWithRescue` 与 `usp_Adoption_Create`，保证建档/领养与状态回写原子完成。

**Tech Stack:** ASP.NET Web Forms (.NET Framework 4.8)、SQL Server (SQLEXPRESS)、xUnit、MSBuild / dotnet test。

**Spec:**
- `docs/superpowers/specs/2026-11-12-overall-system-design.md`
- `docs/superpowers/specs/2026-11-12-c-end-portal-design.md`

## Global Constraints

- 角色值不变：`Admin = 1`、`User = 2`（原 `Staff` 仅改名）。
- 页面不出现 SQL，不直接引用 `StrayCat.DAL`；中文提示统一放 `Messages`。
- 存储过程命名：`usp_模块_动作`；脚本可重复执行（`IF OBJECT_ID(...) IS NULL` / `IF OBJECT_ID(...) IS NOT NULL DROP`）。
- 审核类过程必须 `SET XACT_ABORT ON` + `TRY/CATCH` + `UPDLOCK, HOLDLOCK`。
- 新增页面需在 `StrayCat.Web.csproj` 中同时登记 `<Content Include>` 与 `<Compile Include>`。
- 测试命令：`dotnet test StrayCat.Tests\StrayCat.Tests.csproj -v q`。
- 构建命令：`& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" StrayCat.sln /p:Configuration=Debug /m /v:m /nologo`。

---

### Task 1: 数据库增量脚本

**Files:**
- Create: `Database/07_CitizenPortal.sql`

**Interfaces:**
- Produces:
  - 表 `dbo.StrayReport`、`dbo.AdoptionApplication`
  - `usp_Report_Create`、`usp_Report_ListByUser`、`usp_Report_Search`、`usp_Report_Approve`、`usp_Report_Reject`
  - `usp_Application_Create`、`usp_Application_ListByUser`、`usp_Application_Search`、`usp_Application_Approve`、`usp_Application_Reject`
  - `usp_Cat_ListAdoptable`、`usp_Cat_GetPublicDetail`

- [ ] **Step 1: 建表**

```sql
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
```

- [ ] **Step 2: 上报相关过程**

```sql
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
        EXEC dbo.usp_Cat_CreateWithRescue
            @TempName = @TempName,
            @Gender = @Gender,
            @EstimatedAge = @EstimatedAge,
            @Color = @Color,
            @IsNeutered = @IsNeutered,
            @PhotoPath = COALESCE(@PhotoPath, @Photo),
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
```

- [ ] **Step 3: 领养申请过程**

```sql
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
        RAISERROR(N'该猫当前不可申请领养', 16, 1);

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
        EXEC dbo.usp_Adoption_Create
            @CatId = @CatId,
            @AdopterName = @Name,
            @AdopterPhone = @Phone,
            @AdopterAddress = @Address,
            @AdoptDate = CAST(GETDATE() AS DATE),
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
```

- [ ] **Step 4: 公开查询过程**

```sql
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
           c.TerminateReason, c.CreateUserId, c.CreateTime,
           r.Location, r.RescueTime, r.InitialHealth,
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
```

- [ ] **Step 5: 执行脚本并验证**

```powershell
$sql = Get-Content "Database\07_CitizenPortal.sql" -Raw
$conn = New-Object System.Data.SqlClient.SqlConnection "Data Source=localhost\SQLEXPRESS;Initial Catalog=StrayCatArchive;Integrated Security=True"
$conn.Open()
foreach ($batch in ($sql -split "(?m)^\s*GO\s*$")) {
    if ($batch.Trim()) { $cmd = $conn.CreateCommand(); $cmd.CommandText = $batch; $cmd.ExecuteNonQuery() | Out-Null }
}
$conn.Close()
```

验证：`SELECT COUNT(*) FROM dbo.StrayReport;` 与 `SELECT COUNT(*) FROM dbo.AdoptionApplication;` 均返回 0，无报错。

- [ ] **Step 6: 提交**

```bash
git add Database/07_CitizenPortal.sql
git commit -m "Add citizen portal tables and procedures"
```

---

### Task 2: 模型与文案

**Files:**
- Create: `StrayCat.Model/PortalRecords.cs`
- Create: `StrayCat.Model/ReportStatuses.cs`
- Create: `StrayCat.Model/ApplicationStatuses.cs`
- Modify: `StrayCat.Model/Messages.cs`
- Test: `StrayCat.Tests/PortalStatusTests.cs`

**Interfaces:**
- Produces:
  - `PublicCat`（属性：`CatId, CatNo, TempName, Gender, Status, EstimatedAge, Color, IsNeutered, PhotoPath, Remark, RescueTime, MedicalCount, AdoptionCount`）
  - `StrayReport`（属性：`ReportId, UserId, UserName, RealName, FoundPlace, FoundDate, Description, PhotoPath, ContactPhone, Status, RejectReason, CatId, CreateTime, AuditUserId, AuditTime`）
  - `AdoptionApplication`（属性：`ApplicationId, CatId, UserId, ApplicantName, Phone, Address, Reason, Status, RejectReason, AdoptionId, CreateTime, AuditUserId, AuditTime, CatNo, TempName`）
  - `ReportStatuses.Pending/Approved/Rejected`、`ReportStatuses.Name(byte)`
  - `ApplicationStatuses.Pending/Approved/Rejected`、`ApplicationStatuses.Name(byte)`

- [ ] **Step 1: 写失败测试**

```csharp
using StrayCat.Model;
using Xunit;

namespace StrayCat.Tests
{
    public class PortalStatusTests
    {
        [Fact]
        public void ReportStatusNames()
        {
            Assert.Equal("待审核", ReportStatuses.Name(ReportStatuses.Pending));
            Assert.Equal("已采纳", ReportStatuses.Name(ReportStatuses.Approved));
            Assert.Equal("已驳回", ReportStatuses.Name(ReportStatuses.Rejected));
        }

        [Fact]
        public void ApplicationStatusNames()
        {
            Assert.Equal("待审核", ApplicationStatuses.Name(ApplicationStatuses.Pending));
            Assert.Equal("已通过", ApplicationStatuses.Name(ApplicationStatuses.Approved));
            Assert.Equal("已驳回", ApplicationStatuses.Name(ApplicationStatuses.Rejected));
        }
    }
}
```

- [ ] **Step 2: 运行并确认失败**

Run: `dotnet test StrayCat.Tests\StrayCat.Tests.csproj --filter PortalStatusTests -v q`
Expected: 编译失败，`ReportStatuses` / `ApplicationStatuses` 不存在。

- [ ] **Step 3: 实现模型与文案**

```csharp
namespace StrayCat.Model
{
    public static class ReportStatuses
    {
        public const byte Pending = 1;
        public const byte Approved = 2;
        public const byte Rejected = 3;

        public static string Name(byte status)
        {
            if (status == Approved) return "已采纳";
            if (status == Rejected) return "已驳回";
            return "待审核";
        }
    }
}
```

```csharp
namespace StrayCat.Model
{
    public static class ApplicationStatuses
    {
        public const byte Pending = 1;
        public const byte Approved = 2;
        public const byte Rejected = 3;

        public static string Name(byte status)
        {
            if (status == Approved) return "已通过";
            if (status == Rejected) return "已驳回";
            return "待审核";
        }
    }
}
```

```csharp
namespace StrayCat.Model
{
    public class PublicCat
    {
        public int CatId { get; set; }
        public string CatNo { get; set; }
        public string TempName { get; set; }
        public byte Gender { get; set; }
        public byte Status { get; set; }
        public string EstimatedAge { get; set; }
        public string Color { get; set; }
        public byte IsNeutered { get; set; }
        public string PhotoPath { get; set; }
        public string Remark { get; set; }
        public System.DateTime? RescueTime { get; set; }
        public int MedicalCount { get; set; }
        public int AdoptionCount { get; set; }

        public string GenderName { get { return Lookups.GenderName(Gender); } }
        public string NeuteredName { get { return Lookups.NeuteredName(IsNeutered); } }
    }
}
```

```csharp
namespace StrayCat.Model
{
    public class StrayReport
    {
        public int ReportId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string RealName { get; set; }
        public string FoundPlace { get; set; }
        public System.DateTime FoundDate { get; set; }
        public string Description { get; set; }
        public string PhotoPath { get; set; }
        public string ContactPhone { get; set; }
        public byte Status { get; set; }
        public string RejectReason { get; set; }
        public int? CatId { get; set; }
        public System.DateTime CreateTime { get; set; }
        public int? AuditUserId { get; set; }
        public System.DateTime? AuditTime { get; set; }
    }
}
```

```csharp
namespace StrayCat.Model
{
    public class AdoptionApplication
    {
        public int ApplicationId { get; set; }
        public int CatId { get; set; }
        public int UserId { get; set; }
        public string ApplicantName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Reason { get; set; }
        public byte Status { get; set; }
        public string RejectReason { get; set; }
        public int? AdoptionId { get; set; }
        public System.DateTime CreateTime { get; set; }
        public int? AuditUserId { get; set; }
        public System.DateTime? AuditTime { get; set; }
        public string CatNo { get; set; }
        public string TempName { get; set; }
    }
}
```

在 `Messages.cs` 末尾追加：

```csharp
        public const string ReportSubmitted = "上报已提交，等待管理员处理";
        public const string ApplicationSubmitted = "申请已提交，等待管理员审核";
        public const string ApplicationExists = "你已提交过该猫的申请，请等待审核";
        public const string CatNotAdoptable = "该猫当前不可申请领养";
        public const string RejectReasonRequired = "请填写驳回理由";
        public const string AuditChanged = "该记录已被处理，请刷新后重试";
        public const string BadBreed = "请填写品种";
```

- [ ] **Step 4: 运行测试**

Run: `dotnet test StrayCat.Tests\StrayCat.Tests.csproj --filter PortalStatusTests -v q`
Expected: PASS。

- [ ] **Step 5: 提交**

```bash
git add StrayCat.Model/PortalRecords.cs StrayCat.Model/ReportStatuses.cs StrayCat.Model/ApplicationStatuses.cs StrayCat.Model/Messages.cs StrayCat.Tests/PortalStatusTests.cs
git commit -m "Add portal models, statuses and messages"
```

---

### Task 3: 数据访问层

**Files:**
- Create: `StrayCat.DAL/ReportDal.cs`
- Create: `StrayCat.DAL/ApplicationDal.cs`
- Modify: `StrayCat.DAL/RecordMaps.cs`
- Modify: `StrayCat.DAL/CatDal.cs`

**Interfaces:**
- Consumes: `DbHelper.Query/QueryPage/ExecuteReturnId/Execute`、`SqlParams.P`、`RecordMaps`
- Produces:
  - `ReportDal.Insert(StrayReport) -> OperationResult<int>`
  - `ReportDal.ListByUser(int, int, int) -> PagedResult<StrayReport>`
  - `ReportDal.Search(byte?, string, int, int) -> PagedResult<StrayReport>`
  - `ReportDal.Approve(int reportId, int auditUserId, CatArchive cat, System.DateTime rescueTime, string initialHealth) -> OperationResult<int>`
  - `ReportDal.Reject(int reportId, int auditUserId, string reason) -> OperationResult<int>`
  - `ApplicationDal.Insert(AdoptionApplication) -> OperationResult<int>`
  - `ApplicationDal.ListByUser(int, int, int) -> PagedResult<AdoptionApplication>`
  - `ApplicationDal.Search(byte?, int, int) -> PagedResult<AdoptionApplication>`
  - `ApplicationDal.Approve(int applicationId, int auditUserId) -> OperationResult<int>`
  - `ApplicationDal.Reject(int applicationId, int auditUserId, string reason) -> OperationResult<int>`
  - `CatDal.ListAdoptable(int, int) -> PagedResult<PublicCat>`、`CatDal.GetPublicDetail(int) -> CatDetail`

- [ ] **Step 1: 增加行映射**

在 `RecordMaps.cs` 末尾追加：

```csharp
        public static StrayReport Report(SqlDataReader reader)
        {
            return new StrayReport
            {
                ReportId = reader.GetInt32(reader.GetOrdinal("ReportId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                UserName = DataMaps.Has(reader, "UserName") ? DataMaps.Text(reader, "UserName") : null,
                RealName = DataMaps.Has(reader, "RealName") ? DataMaps.Text(reader, "RealName") : null,
                FoundPlace = DataMaps.Text(reader, "FoundPlace"),
                FoundDate = reader.GetDateTime(reader.GetOrdinal("FoundDate")),
                Description = DataMaps.Text(reader, "Description"),
                PhotoPath = DataMaps.Text(reader, "PhotoPath"),
                ContactPhone = DataMaps.Text(reader, "ContactPhone"),
                Status = reader.GetByte(reader.GetOrdinal("Status")),
                RejectReason = DataMaps.Text(reader, "RejectReason"),
                CatId = DataMaps.NullableInt(reader, "CatId"),
                CreateTime = reader.GetDateTime(reader.GetOrdinal("CreateTime")),
                AuditUserId = DataMaps.NullableInt(reader, "AuditUserId"),
                AuditTime = DataMaps.Time(reader, "AuditTime")
            };
        }

        public static PublicCat PublicCat(SqlDataReader reader)
        {
            return new PublicCat
            {
                CatId = reader.GetInt32(reader.GetOrdinal("CatId")),
                CatNo = DataMaps.Text(reader, "CatNo"),
                TempName = DataMaps.Text(reader, "TempName"),
                Gender = reader.GetByte(reader.GetOrdinal("Gender")),
                Status = reader.GetByte(reader.GetOrdinal("Status")),
                EstimatedAge = DataMaps.Text(reader, "EstimatedAge"),
                Color = DataMaps.Text(reader, "Color"),
                IsNeutered = reader.GetByte(reader.GetOrdinal("IsNeutered")),
                PhotoPath = DataMaps.Text(reader, "PhotoPath"),
                Remark = DataMaps.Text(reader, "Remark"),
                RescueTime = DataMaps.Time(reader, "RescueTime"),
                MedicalCount = reader.GetInt32(reader.GetOrdinal("MedicalCount")),
                AdoptionCount = reader.GetInt32(reader.GetOrdinal("AdoptionCount"))
            };
        }

        public static AdoptionApplication Application(SqlDataReader reader)
        {
            return new AdoptionApplication
            {
                ApplicationId = reader.GetInt32(reader.GetOrdinal("ApplicationId")),
                CatId = reader.GetInt32(reader.GetOrdinal("CatId")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                ApplicantName = DataMaps.Text(reader, "ApplicantName"),
                Phone = DataMaps.Text(reader, "Phone"),
                Address = DataMaps.Text(reader, "Address"),
                Reason = DataMaps.Text(reader, "Reason"),
                Status = reader.GetByte(reader.GetOrdinal("Status")),
                RejectReason = DataMaps.Text(reader, "RejectReason"),
                AdoptionId = DataMaps.NullableInt(reader, "AdoptionId"),
                CreateTime = reader.GetDateTime(reader.GetOrdinal("CreateTime")),
                AuditUserId = DataMaps.NullableInt(reader, "AuditUserId"),
                AuditTime = DataMaps.Time(reader, "AuditTime"),
                CatNo = DataMaps.Text(reader, "CatNo"),
                TempName = DataMaps.Text(reader, "TempName")
            };
        }
```

`DataMaps` 目前没有 `Has` / `NullableInt`，在 `DataMaps.cs` 中补充：

```csharp
        public static bool Has(SqlDataReader reader, string name)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (string.Equals(reader.GetName(i), name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static int? NullableInt(SqlDataReader reader, string name)
        {
            int index = reader.GetOrdinal(name);
            return reader.IsDBNull(index) ? (int?)null : reader.GetInt32(index);
        }
```

- [ ] **Step 2: 实现 ReportDal**

```csharp
using System;
using System.Collections.Generic;
using StrayCat.Model;

namespace StrayCat.DAL
{
    public class ReportDal
    {
        private readonly DbHelper _db;

        public ReportDal() : this(new DbHelper()) { }

        public ReportDal(DbHelper db)
        {
            _db = db;
        }

        public OperationResult<int> Insert(StrayReport report)
        {
            return _db.ExecuteReturnId(
                "usp_Report_Create",
                "@ReportId",
                SqlParams.P("@UserId", report.UserId),
                SqlParams.P("@FoundPlace", report.FoundPlace),
                SqlParams.P("@FoundDate", report.FoundDate),
                SqlParams.P("@Description", report.Description),
                SqlParams.P("@PhotoPath", report.PhotoPath),
                SqlParams.P("@ContactPhone", report.ContactPhone));
        }

        public PagedResult<StrayReport> ListByUser(int userId, int pageIndex, int pageSize)
        {
            return _db.QueryPage(
                "usp_Report_ListByUser",
                RecordMaps.Report,
                SqlParams.P("@UserId", userId),
                SqlParams.P("@PageIndex", pageIndex),
                SqlParams.P("@PageSize", pageSize));
        }

        public PagedResult<StrayReport> Search(byte? status, string keyword, int pageIndex, int pageSize)
        {
            return _db.QueryPage(
                "usp_Report_Search",
                RecordMaps.Report,
                SqlParams.P("@Status", status),
                SqlParams.P("@Keyword", keyword),
                SqlParams.P("@PageIndex", pageIndex),
                SqlParams.P("@PageSize", pageSize));
        }

        public OperationResult<int> Approve(int reportId, int auditUserId, CatArchive cat, DateTime rescueTime, string initialHealth)
        {
            return _db.ExecuteReturnId(
                "usp_Report_Approve",
                "@CatId",
                SqlParams.P("@ReportId", reportId),
                SqlParams.P("@AuditUserId", auditUserId),
                SqlParams.P("@TempName", cat.TempName),
                SqlParams.P("@Gender", cat.Gender),
                SqlParams.P("@EstimatedAge", cat.EstimatedAge),
                SqlParams.P("@Color", cat.Color),
                SqlParams.P("@IsNeutered", cat.IsNeutered),
                SqlParams.P("@PhotoPath", cat.PhotoPath),
                SqlParams.P("@Remark", cat.Remark),
                SqlParams.P("@RescueTime", rescueTime),
                SqlParams.P("@InitialHealth", initialHealth));
        }

        public OperationResult<int> Reject(int reportId, int auditUserId, string reason)
        {
            return _db.Execute(
                "usp_Report_Reject",
                SqlParams.P("@ReportId", reportId),
                SqlParams.P("@AuditUserId", auditUserId),
                SqlParams.P("@Reason", reason));
        }
    }
}
```

- [ ] **Step 3: 实现 ApplicationDal**

```csharp
using System.Collections.Generic;
using StrayCat.Model;

namespace StrayCat.DAL
{
    public class ApplicationDal
    {
        private readonly DbHelper _db;

        public ApplicationDal() : this(new DbHelper()) { }

        public ApplicationDal(DbHelper db)
        {
            _db = db;
        }

        public OperationResult<int> Insert(AdoptionApplication application)
        {
            return _db.ExecuteReturnId(
                "usp_Application_Create",
                "@ApplicationId",
                SqlParams.P("@CatId", application.CatId),
                SqlParams.P("@UserId", application.UserId),
                SqlParams.P("@ApplicantName", application.ApplicantName),
                SqlParams.P("@Phone", application.Phone),
                SqlParams.P("@Address", application.Address),
                SqlParams.P("@Reason", application.Reason));
        }

        public PagedResult<AdoptionApplication> ListByUser(int userId, int pageIndex, int pageSize)
        {
            return _db.QueryPage(
                "usp_Application_ListByUser",
                RecordMaps.Application,
                SqlParams.P("@UserId", userId),
                SqlParams.P("@PageIndex", pageIndex),
                SqlParams.P("@PageSize", pageSize));
        }

        public PagedResult<AdoptionApplication> Search(byte? status, int pageIndex, int pageSize)
        {
            return _db.QueryPage(
                "usp_Application_Search",
                RecordMaps.Application,
                SqlParams.P("@Status", status),
                SqlParams.P("@PageIndex", pageIndex),
                SqlParams.P("@PageSize", pageSize));
        }

        public OperationResult<int> Approve(int applicationId, int auditUserId)
        {
            return _db.ExecuteReturnId(
                "usp_Application_Approve",
                "@AdoptionId",
                SqlParams.P("@ApplicationId", applicationId),
                SqlParams.P("@AuditUserId", auditUserId));
        }

        public OperationResult<int> Reject(int applicationId, int auditUserId, string reason)
        {
            return _db.Execute(
                "usp_Application_Reject",
                SqlParams.P("@ApplicationId", applicationId),
                SqlParams.P("@AuditUserId", auditUserId),
                SqlParams.P("@Reason", reason));
        }
    }
}
```

- [ ] **Step 4: 扩展 CatDal**

```csharp
        public PagedResult<PublicCat> ListAdoptable(int pageIndex, int pageSize)
        {
            return _db.QueryPage(
                "usp_Cat_ListAdoptable",
                RecordMaps.PublicCat,
                SqlParams.P("@PageIndex", pageIndex),
                SqlParams.P("@PageSize", pageSize));
        }

        public CatDetail GetPublicDetail(int catId)
        {
            List<CatDetail> rows = _db.Query(
                "usp_Cat_GetPublicDetail",
                RecordMaps.Detail,
                SqlParams.P("@CatId", catId));
            return rows.Count == 0 ? null : rows[0];
        }
```

`RecordMaps.Detail` 需要 `RescueId / ReceiverUserId / RescueRemark`，Task 1 的 `usp_Cat_GetPublicDetail` 已用 `ISNULL` 与 `CAST(NULL ...)` 补齐这些列，无需改映射。

- [ ] **Step 5: 构建验证**

Run: `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" StrayCat.sln /p:Configuration=Debug /m /v:m /nologo`
Expected: 0 errors。

- [ ] **Step 6: 提交**

```bash
git add StrayCat.DAL/ReportDal.cs StrayCat.DAL/ApplicationDal.cs StrayCat.DAL/RecordMaps.cs StrayCat.DAL/DataMaps.cs StrayCat.DAL/CatDal.cs Database/07_CitizenPortal.sql
git commit -m "Add report and application data access"
```

---

### Task 4: 业务层与单元测试

**Files:**
- Create: `StrayCat.BLL/ReportBll.cs`
- Create: `StrayCat.BLL/ApplicationBll.cs`
- Test: `StrayCat.Tests/ReportBllTests.cs`
- Test: `StrayCat.Tests/ApplicationBllTests.cs`

**Interfaces:**
- Consumes: `ReportDal`、`ApplicationDal`、`CatDal`、`FieldRules`、`Messages`
- Produces:
  - `ReportBll.Create(StrayReport, int) -> OperationResult<int>`
  - `ReportBll.ListByUser(int, int, int) -> PagedResult<StrayReport>`
  - `ReportBll.Search(byte?, string, int, int) -> PagedResult<StrayReport>`
  - `ReportBll.Approve(int, CatArchive, System.DateTime, string, int) -> OperationResult<int>`
  - `ReportBll.Reject(int, string, int) -> OperationResult<int>`
  - `ApplicationBll.Create(AdoptionApplication, int) -> OperationResult<int>`
  - `ApplicationBll.ListByUser(int, int, int) -> PagedResult<AdoptionApplication>`
  - `ApplicationBll.Search(byte?, int, int) -> PagedResult<AdoptionApplication>`
  - `ApplicationBll.Approve(int, int) -> OperationResult<int>`
  - `ApplicationBll.Reject(int, string, int) -> OperationResult<int>`

- [ ] **Step 1: 写失败测试**

```csharp
using System;
using StrayCat.BLL;
using StrayCat.Model;
using Xunit;

namespace StrayCat.Tests
{
    public class ReportBllTests
    {
        private const string Cs = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=StrayCatArchive;Integrated Security=True";

        [Fact]
        public void CreateRequiresFoundPlace()
        {
            var result = new ReportBll(Cs).Create(new StrayReport
            {
                FoundPlace = " ",
                FoundDate = DateTime.Today,
                ContactPhone = "13800138000"
            }, 1);

            Assert.False(result.Success);
            Assert.Equal(Messages.RequiredField, result.Message);
        }

        [Fact]
        public void CreateRejectsFutureDate()
        {
            var result = new ReportBll(Cs).Create(new StrayReport
            {
                FoundPlace = "一号楼后门",
                FoundDate = DateTime.Today.AddDays(1),
                ContactPhone = "13800138000"
            }, 1);

            Assert.False(result.Success);
            Assert.Equal(Messages.DateAfterToday, result.Message);
        }

        [Fact]
        public void CreateRejectsBadPhone()
        {
            var result = new ReportBll(Cs).Create(new StrayReport
            {
                FoundPlace = "一号楼后门",
                FoundDate = DateTime.Today,
                ContactPhone = "123"
            }, 1);

            Assert.False(result.Success);
            Assert.Equal(Messages.PhoneRule, result.Message);
        }
    }

    public class ApplicationBllTests
    {
        private const string Cs = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=StrayCatArchive;Integrated Security=True";

        [Fact]
        public void CreateRequiresApplicantName()
        {
            var result = new ApplicationBll(Cs).Create(new AdoptionApplication
            {
                CatId = 1,
                ApplicantName = " ",
                Phone = "13800138000",
                Address = "幸福路 1 号"
            }, 1);

            Assert.False(result.Success);
            Assert.Equal(Messages.RequiredField, result.Message);
        }

        [Fact]
        public void RejectRequiresReason()
        {
            var result = new ApplicationBll(Cs).Reject(1, " ", 1);
            Assert.False(result.Success);
            Assert.Equal(Messages.RejectReasonRequired, result.Message);
        }
    }
}
```

- [ ] **Step 2: 运行并确认失败**

Run: `dotnet test StrayCat.Tests\StrayCat.Tests.csproj --filter "ReportBllTests|ApplicationBllTests" -v q`
Expected: 编译失败，`ReportBll` / `ApplicationBll` 不存在。

- [ ] **Step 3: 实现 ReportBll**

```csharp
using System;
using System.Collections.Generic;
using StrayCat.DAL;
using StrayCat.Model;

namespace StrayCat.BLL
{
    public class ReportBll
    {
        private readonly ReportDal _reports;

        public ReportBll() : this(new DbHelper()) { }

        public ReportBll(string connectionString) : this(new DbHelper(connectionString)) { }

        private ReportBll(DbHelper db)
        {
            _reports = new ReportDal(db);
        }

        public OperationResult<int> Create(StrayReport report, int userId)
        {
            report.FoundPlace = FieldRules.Clean(report.FoundPlace);
            report.Description = FieldRules.Clean(report.Description);
            report.ContactPhone = FieldRules.Clean(report.ContactPhone);
            report.PhotoPath = FieldRules.Clean(report.PhotoPath);

            string error = FieldRules.First(
                FieldRules.Required(report.FoundPlace, 200),
                FieldRules.RealDate(report.FoundDate),
                FieldRules.NotAfterToday(report.FoundDate),
                FieldRules.Optional(report.Description, 500),
                FieldRules.Phone(report.ContactPhone, true),
                FieldRules.Optional(report.PhotoPath, 200));
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }

            report.UserId = userId;
            report.Status = ReportStatuses.Pending;
            return _reports.Insert(report);
        }

        public PagedResult<StrayReport> ListByUser(int userId, int pageIndex, int pageSize)
        {
            return _reports.ListByUser(userId, pageIndex, pageSize);
        }

        public PagedResult<StrayReport> Search(byte? status, string keyword, int pageIndex, int pageSize)
        {
            return _reports.Search(status, FieldRules.Clean(keyword), pageIndex, pageSize);
        }

        public OperationResult<int> Approve(int reportId, CatArchive cat, DateTime rescueTime, string initialHealth, int adminId)
        {
            cat.TempName = FieldRules.Clean(cat.TempName);
            cat.EstimatedAge = FieldRules.Clean(cat.EstimatedAge);
            cat.Color = FieldRules.Clean(cat.Color);
            cat.PhotoPath = FieldRules.Clean(cat.PhotoPath);
            cat.Remark = FieldRules.Clean(cat.Remark);
            initialHealth = FieldRules.Clean(initialHealth);

            string error = FieldRules.First(
                FieldRules.Required(cat.TempName, 50),
                FieldRules.Optional(cat.EstimatedAge, 20),
                FieldRules.Optional(cat.Color, 50),
                FieldRules.Optional(cat.Remark, 500),
                FieldRules.Required(initialHealth, 500),
                FieldRules.RealDate(rescueTime),
                FieldRules.NotAfterToday(rescueTime));
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            if (cat.Gender < 1 || cat.Gender > 2)
            {
                return OperationResult<int>.Fail(Messages.RequiredField);
            }
            if (cat.IsNeutered < 0 || cat.IsNeutered > 2)
            {
                return OperationResult<int>.Fail(Messages.RequiredField);
            }
            return _reports.Approve(reportId, adminId, cat, rescueTime, initialHealth);
        }

        public OperationResult<int> Reject(int reportId, string reason, int adminId)
        {
            reason = FieldRules.Clean(reason);
            string error = FieldRules.Required(reason, 200);
            if (error != null)
            {
                return OperationResult<int>.Fail(Messages.RejectReasonRequired);
            }
            return _reports.Reject(reportId, adminId, reason);
        }
    }
}
```

- [ ] **Step 4: 实现 ApplicationBll**

```csharp
using System;
using StrayCat.DAL;
using StrayCat.Model;

namespace StrayCat.BLL
{
    public class ApplicationBll
    {
        private readonly ApplicationDal _applications;
        private readonly CatDal _cats;

        public ApplicationBll() : this(new DbHelper()) { }

        public ApplicationBll(string connectionString) : this(new DbHelper(connectionString)) { }

        private ApplicationBll(DbHelper db)
        {
            _applications = new ApplicationDal(db);
            _cats = new CatDal(db);
        }

        public OperationResult<int> Create(AdoptionApplication application, int userId)
        {
            application.ApplicantName = FieldRules.Clean(application.ApplicantName);
            application.Phone = FieldRules.Clean(application.Phone);
            application.Address = FieldRules.Clean(application.Address);
            application.Reason = FieldRules.Clean(application.Reason);

            string error = FieldRules.First(
                FieldRules.Required(application.ApplicantName, 50),
                FieldRules.Phone(application.Phone, true),
                FieldRules.Required(application.Address, 200),
                FieldRules.Optional(application.Reason, 500));
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }

            CatDetail detail = _cats.GetDetail(application.CatId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (detail.Cat.Status != CatStatuses.WaitingAdoption)
            {
                return OperationResult<int>.Fail(Messages.CatNotAdoptable);
            }

            application.UserId = userId;
            application.Status = ApplicationStatuses.Pending;
            return _applications.Insert(application);
        }

        public PagedResult<AdoptionApplication> ListByUser(int userId, int pageIndex, int pageSize)
        {
            return _applications.ListByUser(userId, pageIndex, pageSize);
        }

        public PagedResult<AdoptionApplication> Search(byte? status, int pageIndex, int pageSize)
        {
            return _applications.Search(status, pageIndex, pageSize);
        }

        public OperationResult<int> Approve(int applicationId, int adminId)
        {
            return _applications.Approve(applicationId, adminId);
        }

        public OperationResult<int> Reject(int applicationId, string reason, int adminId)
        {
            reason = FieldRules.Clean(reason);
            if (FieldRules.Required(reason, 200) != null)
            {
                return OperationResult<int>.Fail(Messages.RejectReasonRequired);
            }
            return _applications.Reject(applicationId, adminId, reason);
        }
    }
}
```

- [ ] **Step 5: 运行测试**

Run: `dotnet test StrayCat.Tests\StrayCat.Tests.csproj --filter "ReportBllTests|ApplicationBllTests" -v q`
Expected: 5 个测试全部 PASS（无效输入在访问数据库前返回）。

- [ ] **Step 6: 提交**

```bash
git add StrayCat.BLL/ReportBll.cs StrayCat.BLL/ApplicationBll.cs StrayCat.Tests/ReportBllTests.cs StrayCat.Tests/ApplicationBllTests.cs
git commit -m "Add report and application business logic"
```

---

### Task 5: 角色改名与权限收回

**Files:**
- Modify: `StrayCat.Model/RoleCodes.cs`
- Modify: `StrayCat.Model/Lookups.cs:28-31`
- Modify: `StrayCat.Web/Login.aspx.cs:68`
- Modify: `StrayCat.Web/Login.aspx:17-18`
- Modify: `StrayCat.Web/PageHelper.cs:110-120`
- Modify: `StrayCat.Web/Site.Master`、`Site.Master.cs`
- Modify: 所有管理页的基类（见 Task 文件清单）

**Interfaces:**
- Produces: `RoleCodes.User`（值 2），`RoleCodes.Staff` 不再存在。

- [ ] **Step 1: 全局替换**

```powershell
Get-ChildItem -Recurse -Include *.cs,*.aspx -Path StrayCat.Web,StrayCat.BLL,StrayCat.Model,StrayCat.Tests |
  ForEach-Object {
    $text = Get-Content $_.FullName -Raw
    if ($text -match "RoleCodes\.Staff") {
      $text.Replace("RoleCodes.Staff", "RoleCodes.User") | Set-Content $_.FullName -NoNewline
    }
  }
```

然后修改 `RoleCodes.cs`：

```csharp
namespace StrayCat.Model
{
    public static class RoleCodes
    {
        public const byte Admin = 1;
        public const byte User = 2;
    }
}
```

`Lookups.RoleName` 中 `"工作人员"` 改为 `"普通用户"`；`PageHelper.BindRoles` 第 117 行改为：

```csharp
list.Items.Add(new ListItem("普通用户", RoleCodes.User.ToString()));
```

- [ ] **Step 2: 修改页面基类**

把以下文件的类声明由 `: PageBase` 改为 `: AdminPageBase`：

`CatList.aspx.cs`、`CatAdd.aspx.cs`、`CatEdit.aspx.cs`、`CatDetail.aspx.cs`、`CatStatus.aspx.cs`、`RescueEdit.aspx.cs`、`RescueDetail.aspx.cs`、`MedicalList.aspx.cs`、`MedicalAdd.aspx.cs`、`MedicalEdit.aspx.cs`、`MedicalDetail.aspx.cs`、`AdoptionList.aspx.cs`、`AdoptionAdd.aspx.cs`、`AdoptionEdit.aspx.cs`、`AdoptionDetail.aspx.cs`、`FollowUpList.aspx.cs`、`FollowUpAdd.aspx.cs`、`FollowUpEdit.aspx.cs`、`FollowUpDetail.aspx.cs`。

`ChangePassword.aspx.cs` 保持 `PageBase`。

- [ ] **Step 3: 登录页与菜单**

`Login.aspx` 第 17-18 行文案改为“普通用户”“管理员”。

`Site.Master` 在 `AdminMenu` 之前增加对所有登录用户可见的 C 端菜单：

```aspx
<p class="menu-label">领养与上报</p>
<a href="Adoptable.aspx">可领养猫</a>
<a href="ReportAdd.aspx">上报流浪猫</a>
<a href="MyCenter.aspx">我的中心</a>
```

`AdminMenu` 保持 `Visible="false"`，由 `Site.Master.cs` 按 `IsAdmin` 打开。

- [ ] **Step 4: 构建验证**

Run: `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" StrayCat.sln /p:Configuration=Debug /m /v:m /nologo`
Expected: 0 errors；若仍有 `RoleCodes.Staff` 引用，逐个改为 `RoleCodes.User`。

- [ ] **Step 5: 手工验证（服务已在 8080 运行）**

- 用 `admin / Admin@123` 登录，能看到全部菜单。
- 新注册账号登录后只看到“可领养猫/上报流浪猫/我的中心/修改密码”和原有公开入口。
- 普通用户访问 `http://localhost:8080/CatList.aspx` 应跳回 `Default.aspx?msg=forbidden`。

- [ ] **Step 6: 提交**

```bash
git add -A
git commit -m "Restrict management pages to administrators"
```

---

### Task 6: 登录回跳（returnUrl）

**Files:**
- Modify: `StrayCat.Web/PageBase.cs`
- Modify: `StrayCat.Web/Login.aspx.cs`

**Interfaces:**
- Produces: `PageBase.Leave(string url, string returnUrl = null)`；登录成功后优先回跳站内 `returnUrl`。

- [ ] **Step 1: 修改 Leave 与拦截**

```csharp
        protected void Leave(string url, string returnUrl = null)
        {
            Redirecting = true;
            if (!string.IsNullOrEmpty(returnUrl))
            {
                url += (url.Contains("?") ? "&" : "?") + "returnUrl=" + System.Web.HttpUtility.UrlEncode(returnUrl);
            }
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }
```

`PageBase.OnInit` 中改为：

```csharp
Leave("~/Login.aspx?msg=expired", Request.RawUrl);
```

- [ ] **Step 2: 登录成功回跳**

在 `Login.aspx.cs` 的 `LoginButton_Click` 中，`LoginSession.Set(result.Data);` 之后替换跳转：

```csharp
            string target = Request.QueryString["returnUrl"];
            if (string.IsNullOrEmpty(target) || !target.StartsWith("/") || target.StartsWith("//"))
            {
                target = "~/Default.aspx";
            }
            Response.Redirect(target, false);
            Context.ApplicationInstance.CompleteRequest();
```

- [ ] **Step 3: 构建与手工验证**

未登录直接访问 `http://localhost:8080/ReportAdd.aspx`，登录后应回到 `ReportAdd.aspx` 而不是首页。

- [ ] **Step 4: 提交**

```bash
git add StrayCat.Web/PageBase.cs StrayCat.Web/Login.aspx.cs
git commit -m "Return to requested page after login"
```

---

### Task 7: 公开浏览页

**Files:**
- Create: `StrayCat.Web/Adoptable.aspx`、`Adoptable.aspx.cs`
- Create: `StrayCat.Web/AdoptableDetail.aspx`、`AdoptableDetail.aspx.cs`
- Modify: `StrayCat.Web/StrayCat.Web.csproj`

**Interfaces:**
- Consumes: `CatBll.ListAdoptable(int, int)`、`CatBll.GetPublicDetail(int)`（在 `CatBll` 中转发 `CatDal` 新方法）

- [ ] **Step 1: CatBll 增加转发**

```csharp
        public PagedResult<PublicCat> ListAdoptable(int pageIndex, int pageSize)
        {
            return _cats.ListAdoptable(pageIndex, pageSize);
        }

        public CatDetail GetPublicDetail(int catId)
        {
            return _cats.GetPublicDetail(catId);
        }
```

- [ ] **Step 2: Adoptable 页面**

`Adoptable.aspx` 使用 `Page`（不继承基类），控件：

```aspx
<asp:Repeater ID="ListRepeater" runat="server">
    <ItemTemplate>
        <a class="card" href='AdoptableDetail.aspx?id=<%# Eval("CatId") %>'>
            <img src='<%# Eval("PhotoPath") %>' alt="" />
            <h3><%#: Eval("TempName") %></h3>
            <p><%#: Eval("CatNo") %> · <%#: Eval("EstimatedAge") %></p>
        </a>
    </ItemTemplate>
</asp:Repeater>
<asp:Literal ID="PagerLiteral" runat="server" />
<asp:Label ID="EmptyLabel" runat="server" Visible="false" Text="暂无待领养猫咪" />
```

代码隐藏：

```csharp
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using StrayCat.BLL;
using StrayCat.Model;

namespace StrayCat.Web
{
    public partial class AdoptablePage : Page
    {
        protected Repeater ListRepeater;
        protected Literal PagerLiteral;
        protected Label EmptyLabel;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Bind();
            }
        }

        private void Bind()
        {
            int pageIndex = PageHelper.Positive(Request.QueryString["page"]);
            if (pageIndex < 1) pageIndex = 1;
            PagedResult<PublicCat> page = new CatBll().ListAdoptable(pageIndex, 12);
            ListRepeater.DataSource = page.Items;
            ListRepeater.DataBind();
            EmptyLabel.Visible = page.Items.Count == 0;
            PagerLiteral.Text = PageHelper.Pager("Adoptable.aspx", pageIndex, page.TotalCount, 12);
        }
    }
}
```

- [ ] **Step 3: AdoptableDetail 页面**

控件：`PhotoImage`、`NameLiteral`、`MetaLiteral`、`StoryLiteral`、`ApplyLink`、`MessageLabel`。

```csharp
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int catId = PageHelper.Positive(Request.QueryString["id"]);
                CatDetail detail = catId == 0 ? null : new CatBll().GetPublicDetail(catId);
                if (detail == null)
                {
                    MessageLabel.Text = Messages.NotFound;
                    ApplyLink.Visible = false;
                    return;
                }
                PhotoImage.ImageUrl = detail.Cat.PhotoPath;
                NameLiteral.Text = PageHelper.Html(detail.Cat.TempName);
                MetaLiteral.Text = string.Format("{0} · {1} · {2}",
                    detail.Cat.CatNo,
                    Lookups.GenderName(detail.Cat.Gender),
                    detail.Cat.EstimatedAge);
                StoryLiteral.Text = PageHelper.Html(detail.Rescue.InitialHealth);
                ApplyLink.NavigateUrl = "ApplyAdoption.aspx?catId=" + catId;
            }
        }
```

- [ ] **Step 4: 注册到 csproj**

```xml
<Content Include="Adoptable.aspx" />
<Content Include="AdoptableDetail.aspx" />
<Compile Include="Adoptable.aspx.cs">
  <DependentUpon>Adoptable.aspx</DependentUpon>
  <SubType>ASPXCodeBehind</SubType>
</Compile>
<Compile Include="AdoptableDetail.aspx.cs">
  <DependentUpon>AdoptableDetail.aspx</DependentUpon>
  <SubType>ASPXCodeBehind</SubType>
</Compile>
```

- [ ] **Step 5: 构建并手工验证**

未登录访问 `http://localhost:8080/Adoptable.aspx`，应看到待领养猫（若数据库没有 `Status=2` 的记录，先在管理端把一只猫改为待领养）。

- [ ] **Step 6: 提交**

```bash
git add StrayCat.Web/Adoptable.aspx StrayCat.Web/Adoptable.aspx.cs StrayCat.Web/AdoptableDetail.aspx StrayCat.Web/AdoptableDetail.aspx.cs StrayCat.Web/StrayCat.Web.csproj StrayCat.BLL/CatBll.cs
git commit -m "Add public adoptable cat pages"
```

---

### Task 8: 上报页与我的中心

**Files:**
- Create: `StrayCat.Web/ReportAdd.aspx`、`ReportAdd.aspx.cs`
- Create: `StrayCat.Web/MyCenter.aspx`、`MyCenter.aspx.cs`
- Modify: `StrayCat.Web/StrayCat.Web.csproj`

**Interfaces:**
- Consumes: `ReportBll.Create/ListByUser`、`ApplicationBll.ListByUser`

- [ ] **Step 1: ReportAdd 页面**

控件：`PlaceBox`（MaxLength 200）、`DateBox`（yyyy-MM-dd）、`DescBox`（TextMode Multiline，MaxLength 500）、`PhoneBox`、`PhotoFile`、`MessageLabel`、`SaveButton`。

```csharp
        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            DateTime found;
            if (!PageHelper.ReadDate(DateBox.Text, out found))
            {
                MessageLabel.Text = Messages.BadDate;
                return;
            }

            var report = new StrayReport
            {
                FoundPlace = PlaceBox.Text,
                FoundDate = found,
                Description = DescBox.Text,
                ContactPhone = PhoneBox.Text
            };
            string photoError;
            report.PhotoPath = PageHelper.SavePhoto(PhotoFile, Server, out photoError);
            if (photoError != null)
            {
                MessageLabel.Text = photoError;
                return;
            }

            OperationResult<int> result = new ReportBll().Create(report, CurrentUser.UserId);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("~/MyCenter.aspx?msg=report");
        }
```

`PageHelper.SavePhoto` 的现有签名为 `SavePhoto(FileUpload upload, HttpServerUtility server, out string error)`，页面里传 `Server` 即可。

- [ ] **Step 2: MyCenter 页面**

两个 Repeater：`ReportRepeater`（列：地点、发现日期、状态、驳回理由）、`ApplicationRepeater`（列：猫编号/名字、申请时间、状态、驳回理由）。状态用 `ReportStatuses.Name` / `ApplicationStatuses.Name` 在页面绑定方法中转换。

```csharp
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindReports();
                BindApplications();
            }
        }

        private void BindReports()
        {
            int pageIndex = PageHelper.Positive(Request.QueryString["rp"]);
            PagedResult<StrayReport> page = new ReportBll()
                .ListByUser(CurrentUser.UserId, pageIndex < 1 ? 1 : pageIndex, 10);
            ReportRepeater.DataSource = page.Items;
            ReportRepeater.DataBind();
        }

        private void BindApplications()
        {
            int pageIndex = PageHelper.Positive(Request.QueryString["ap"]);
            PagedResult<AdoptionApplication> page = new ApplicationBll()
                .ListByUser(CurrentUser.UserId, pageIndex < 1 ? 1 : pageIndex, 10);
            ApplicationRepeater.DataSource = page.Items;
            ApplicationRepeater.DataBind();
        }
```

- [ ] **Step 3: 注册到 csproj 并构建**

同 Task 7 的 `<Content>` / `<Compile>` 写法。构建 0 errors。

- [ ] **Step 4: 手工验证**

普通用户提交一条上报，`MyCenter` 显示“待审核”；管理端尚未处理时状态不变。

- [ ] **Step 5: 提交**

```bash
git add StrayCat.Web/ReportAdd.aspx* StrayCat.Web/MyCenter.aspx* StrayCat.Web/PageHelper.cs StrayCat.Web/StrayCat.Web.csproj
git commit -m "Add stray report submission and user center"
```

---

### Task 9: 领养申请页

**Files:**
- Create: `StrayCat.Web/ApplyAdoption.aspx`、`ApplyAdoption.aspx.cs`
- Modify: `StrayCat.Web/StrayCat.Web.csproj`

**Interfaces:**
- Consumes: `ApplicationBll.Create`、`CatBll.GetDetail`

- [ ] **Step 1: 页面控件**

`NameBox`、`PhoneBox`、`AddressBox`、`ReasonBox`、`CatLiteral`、`MessageLabel`、`SaveButton`。

- [ ] **Step 2: 代码隐藏**

```csharp
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int catId = PageHelper.Positive(Request.QueryString["catId"]);
                CatDetail detail = catId == 0 ? null : new CatBll().GetDetail(catId);
                if (detail == null || detail.Cat.Status != CatStatuses.WaitingAdoption)
                {
                    MessageLabel.Text = Messages.CatNotAdoptable;
                    SaveButton.Visible = false;
                    return;
                }
                CatLiteral.Text = PageHelper.Html(detail.Cat.CatNo + " " + detail.Cat.TempName);
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            var application = new AdoptionApplication
            {
                CatId = PageHelper.Positive(Request.QueryString["catId"]),
                ApplicantName = NameBox.Text,
                Phone = PhoneBox.Text,
                Address = AddressBox.Text,
                Reason = ReasonBox.Text
            };
            OperationResult<int> result = new ApplicationBll().Create(application, CurrentUser.UserId);
            if (!result.Success)
            {
                MessageLabel.Text = result.Message;
                return;
            }
            Leave("~/MyCenter.aspx?msg=application");
        }
```

- [ ] **Step 3: 注册到 csproj、构建、手工验证**

重复申请同一只猫应显示“你已提交过该猫的申请，请等待审核”（由筛选唯一索引触发，`SqlErrorMapper` 增加映射）。

在 `SqlErrorMapper` 中增加：

```csharp
            if (message.IndexOf("UX_AdoptionApplication_Pending", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Messages.ApplicationExists;
            }
```

- [ ] **Step 4: 提交**

```bash
git add StrayCat.Web/ApplyAdoption.aspx* StrayCat.Web/StrayCat.Web.csproj StrayCat.DAL/SqlErrorMapper.cs
git commit -m "Add adoption application page"
```

---

### Task 10: 管理员上报审核页

**Files:**
- Create: `StrayCat.Web/ReportList.aspx`、`ReportList.aspx.cs`
- Modify: `StrayCat.Web/StrayCat.Web.csproj`

**Interfaces:**
- Consumes: `ReportBll.Search/Approve/Reject`

- [ ] **Step 1: 列表与筛选**

控件：`StatusList`（全部/待审核/已采纳/已驳回）、`KeywordBox`、`SearchButton`、`ReportRepeater`、`PagerLiteral`。

行内按钮：`CommandName="approve"` / `"reject"`，`CommandArgument='<%# Eval("ReportId") %>'`。

- [ ] **Step 2: 审核操作**

采纳：点击后跳 `ReportApprove.aspx?id=x`（建档表单页），或在本页展开表单。为控制篇幅，采用独立页 `ReportApprove.aspx`：

```csharp
// ReportApprove.aspx.cs
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int reportId = PageHelper.Positive(Request.QueryString["id"]);
                if (reportId == 0) { MessageLabel.Text = Messages.NotFound; return; }
                PlaceLiteral.Text = PageHelper.Html("上报编号 " + reportId);
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            DateTime rescue;
            if (!PageHelper.ReadDate(DateBox.Text, out rescue))
            {
                MessageLabel.Text = Messages.BadDate;
                return;
            }
            var cat = new CatArchive
            {
                TempName = NameBox.Text,
                Gender = byte.Parse(GenderList.SelectedValue),
                EstimatedAge = AgeBox.Text,
                Color = ColorBox.Text,
                IsNeutered = byte.Parse(NeuteredList.SelectedValue),
                PhotoPath = PageHelper.SavePhoto(PhotoFile, Server, out photoError)
            };
            if (photoError != null) { MessageLabel.Text = photoError; return; }

            OperationResult<int> result = new ReportBll().Approve(
                PageHelper.Positive(Request.QueryString["id"]),
                cat, rescue, HealthBox.Text, CurrentUser.UserId);
            if (!result.Success) { MessageLabel.Text = result.Message; return; }
            Leave("~/ReportList.aspx?msg=approved");
        }
```

驳回：在 `ReportList` 中点击后显示一个 `ReasonBox` + 确认按钮，调用：

```csharp
            OperationResult<int> result = new ReportBll().Reject(reportId, ReasonBox.Text, CurrentUser.UserId);
```

- [ ] **Step 3: 注册、构建、手工验证**

- 普通用户上报一条，管理员在 `ReportList` 采纳并补全档案，猫档案列表出现新记录，上报状态变“已采纳”。
- 驳回一条，用户 `MyCenter` 能看到驳回理由。

- [ ] **Step 4: 提交**

```bash
git add StrayCat.Web/ReportList.aspx* StrayCat.Web/ReportApprove.aspx* StrayCat.Web/StrayCat.Web.csproj
git commit -m "Add admin stray report review"
```

---

### Task 11: 管理员领养申请审核页

**Files:**
- Create: `StrayCat.Web/ApplicationList.aspx`、`ApplicationList.aspx.cs`
- Modify: `StrayCat.Web/StrayCat.Web.csproj`

**Interfaces:**
- Consumes: `ApplicationBll.Search/Approve/Reject`

- [ ] **Step 1: 列表**

控件：`StatusList`、`ApplicationRepeater`、`PagerLiteral`；行内按钮 `approve` / `reject`，显示申请人、猫编号/名字、申请理由。

- [ ] **Step 2: 审核**

```csharp
        protected void ApplicationRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "approve")
            {
                OperationResult<int> result = new ApplicationBll()
                    .Approve(int.Parse((string)e.CommandArgument), CurrentUser.UserId);
                Show(result);
            }
            else if (e.CommandName == "reject")
            {
                OperationResult<int> result = new ApplicationBll()
                    .Reject(int.Parse((string)e.CommandArgument), ReasonBox.Text, CurrentUser.UserId);
                Show(result);
            }
        }
```

- [ ] **Step 3: 注册、构建、手工验证**

- 用户申请一只待领养猫。
- 管理员通过后，该猫状态变“已领养”，领养记录出现，申请状态变“已通过”。
- 两只猫被同时申请时，先通过一只，另一条审核应提示“当前状态不允许此操作”。

- [ ] **Step 4: 提交**

```bash
git add StrayCat.Web/ApplicationList.aspx* StrayCat.Web/StrayCat.Web.csproj
git commit -m "Add admin adoption application review"
```

---

### Task 12: 回归与验收

**Files:**
- Modify: `docs/架构设计文档.md`（补充 C 端模块与权限说明）

- [ ] **Step 1: 运行全部测试**

Run: `dotnet test StrayCat.Tests\StrayCat.Tests.csproj -v q`
Expected: 全部 PASS。

- [ ] **Step 2: 完整构建**

Run: `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" StrayCat.sln /p:Configuration=Debug /m /v:m /nologo`
Expected: 0 errors。

- [ ] **Step 3: 手工验收清单**

- [ ] 未登录可浏览 `Adoptable.aspx` 与详情。
- [ ] 未登录点“申请领养”跳登录，登录后回到申请页。
- [ ] 普通用户看不到管理菜单，手输 `/CatList.aspx`、`/Stat.aspx`、`/UserList.aspx` 均被挡回首页。
- [ ] 上报 → 管理员采纳 → 生成档案；上报状态与 `CatId` 正确。
- [ ] 上报 → 驳回 → 用户在“我的中心”看到理由。
- [ ] 申请 → 管理员通过 → 生成领养记录、猫变已领养、申请状态与 `AdoptionId` 正确。
- [ ] 同一用户对同一只猫重复提交待审核申请被拒绝。
- [ ] 管理员仍可正常使用档案、医疗、领养、回访、统计、用户管理。

- [ ] **Step 4: 更新文档并提交**

在 `docs/架构设计文档.md` 增加“C 端入口与角色权限”一节，说明两角色、审核闭环、新增表与页面。

```bash
git add docs/架构设计文档.md
git commit -m "Document citizen portal permissions and flows"
```
