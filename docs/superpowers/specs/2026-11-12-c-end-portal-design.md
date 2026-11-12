# C 端用户上报与领养申请 设计说明

- 日期：2026-11-12
- 状态：已确认，待评审后进入实现计划
- 涉及系统：流浪猫救助档案管理系统（ASP.NET Web Forms + 三层架构 + SQL Server）

## 1. 背景与目标

现状：系统只有管理员与工作人员两种身份，工作人员可访问全部业务页面；领养由工作人员直接登记；没有任何面向公众的入口。

本次调整后：

1. 普通用户（C 端）只能浏览待领养猫、上报流浪猫、提交领养申请、查看自己的上报与申请。
2. 全部档案、诊疗、领养、回访、统计、账号管理权限收回给管理员。
3. 上报与领养申请必须经管理员审核后才生效。
4. 待领养列表公开可浏览，任何写操作需登录。

不在本次范围：短信/邮件通知、地图定位、在线协议、多图上传、匿名上报、复杂审批流。

### 已确认的决策

| 问题 | 结论 |
| --- | --- |
| 角色模型 | 两角色：管理员 / 普通用户 |
| 审核方式 | 管理员审核后生效，可驳回并填理由 |
| 访问范围 | 待领养列表公开，操作需登录 |
| 存量 Role=2 账号 | 全部作为普通用户，不改数据 |

## 2. 角色与权限

`RoleCodes`：`Admin = 1`、`User = 2`。原 `Staff` 改名为 `User`，常量值保持 2，因此存量账号无需迁移数据。

| 功能 | 管理员 | 普通用户 | 未登录 |
| --- | --- | --- | --- |
| 浏览待领养列表/详情 | ✓ | ✓ | ✓ |
| 上报流浪猫 | ✓ | ✓ | ✗ |
| 提交领养申请 | ✓ | ✓ | ✗ |
| 我的上报/我的申请 | ✓ | ✓ | ✗ |
| 档案查询与维护 | ✓ | ✗ | ✗ |
| 诊疗、领养登记、回访记录 | ✓ | ✗ | ✗ |
| 统计报表 | ✓ | ✗ | ✗ |
| 账号管理 | ✓ | ✗ | ✗ |
| 上报/申请审核 | ✓ | ✗ | ✗ |
| 修改自己的密码 | ✓ | ✓ | ✗ |

实现方式：

- 公开页继承 `Page`，不经过登录拦截。
- C 端操作页继承 `PageBase`，未登录跳转登录页；`PageBase.Leave` 增加可选的 `returnUrl` 参数，登录成功后回到原页面（仅允许站内相对路径）。
- 管理页统一继承 `AdminPageBase`；现有 `PageBase` 管理页全部改为 `AdminPageBase`。
- 登录页身份选项文案改为“普通用户 / 管理员”，校验逻辑不变。
- `Register.aspx` 继续只创建 `RoleCodes.User`。

## 3. 页面清单

### 新增：公开

| 页面 | 内容 |
| --- | --- |
| `Adoptable.aspx` | 待领养猫分页列表，只读；未登录也可访问 |
| `AdoptableDetail.aspx` | 猫的基本信息、照片、救助简介；“申请领养”按钮未登录时跳登录页并带返回地址 |

公开页只展示猫的信息与救助站联系方式，不展示上报人、申请人及任何账号信息。

### 新增：登录用户

| 页面 | 内容 |
| --- | --- |
| `ReportAdd.aspx` | 上报流浪猫：发现地点、发现时间、描述、照片（可选）、联系电话 |
| `ApplyAdoption.aspx` | 对指定猫提交申请：姓名、电话、住址、申请理由 |
| `MyCenter.aspx` | 两个页签：我的上报、我的申请，显示状态与驳回理由 |

### 新增：管理员

| 页面 | 内容 |
| --- | --- |
| `ReportList.aspx` | 上报列表（待审核/已采纳/已驳回筛选）；采纳时补全建档字段，驳回需填理由 |
| `ApplicationList.aspx` | 领养申请列表；通过时生成领养记录，驳回需填理由 |

### 修改：现有页面

以下页面由 `PageBase` 改为 `AdminPageBase`：

`CatList`、`CatAdd`、`CatEdit`、`CatDetail`、`CatStatus`、`RescueEdit`、`RescueDetail`、`MedicalList`、`MedicalAdd`、`MedicalEdit`、`MedicalDetail`、`AdoptionList`、`AdoptionAdd`、`AdoptionEdit`、`AdoptionDetail`、`FollowUpList`、`FollowUpAdd`、`FollowUpEdit`、`FollowUpDetail`，以及 `UserList`、`UserAdd`、`UserEdit`。

`Site.Master` 菜单按 `IsAdmin` 控制：普通用户只看到“可领养猫”“上报流浪猫”“我的中心”“修改密码”。

## 4. 数据模型

新增两张表，不改动已有六张表。

### 4.1 StrayReport（流浪猫上报）

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| ReportId | INT IDENTITY PK | |
| UserId | INT NOT NULL | 上报人，FK SysUser |
| FoundPlace | NVARCHAR(200) NOT NULL | 发现地点 |
| FoundDate | DATE NOT NULL | 发现日期，不得晚于今天 |
| Description | NVARCHAR(500) NULL | 描述 |
| PhotoPath | NVARCHAR(200) NULL | 照片相对路径 |
| ContactPhone | NVARCHAR(20) NOT NULL | 联系电话 |
| Status | TINYINT NOT NULL DEFAULT 1 | 1 待审核，2 已采纳，3 已驳回 |
| RejectReason | NVARCHAR(200) NULL | 驳回理由 |
| CatId | INT NULL | 采纳后生成的档案，FK CatArchive |
| CreateTime | DATETIME NOT NULL DEFAULT GETDATE() | |
| AuditUserId | INT NULL | 审核人 |
| AuditTime | DATETIME NULL | 审核时间 |

约束：

- `CHECK (Status IN (1,2,3))`
- `CHECK ((Status = 2 AND CatId IS NOT NULL) OR Status <> 2)`
- `CHECK ((Status = 3 AND RejectReason IS NOT NULL) OR Status <> 3)`
- 索引：`(Status, CreateTime DESC)`，`(UserId)`

### 4.2 AdoptionApplication（领养申请）

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| ApplicationId | INT IDENTITY PK | |
| CatId | INT NOT NULL | FK CatArchive |
| UserId | INT NOT NULL | 申请人，FK SysUser |
| ApplicantName | NVARCHAR(50) NOT NULL | |
| Phone | NVARCHAR(20) NOT NULL | |
| Address | NVARCHAR(200) NOT NULL | |
| Reason | NVARCHAR(500) NULL | 申请理由 |
| Status | TINYINT NOT NULL DEFAULT 1 | 1 待审核，2 已通过，3 已驳回 |
| RejectReason | NVARCHAR(200) NULL | |
| AdoptionId | INT NULL | 通过后生成的领养记录，FK AdoptionRecord |
| CreateTime | DATETIME NOT NULL DEFAULT GETDATE() | |
| AuditUserId | INT NULL | |
| AuditTime | DATETIME NULL | |

约束：

- `CHECK (Status IN (1,2,3))`
- `CHECK ((Status = 2 AND AdoptionId IS NOT NULL) OR Status <> 2)`
- `CHECK ((Status = 3 AND RejectReason IS NOT NULL) OR Status <> 3)`
- 筛选唯一索引：`UX_Application_Pending (UserId, CatId) WHERE Status = 1`，防止重复提交待审核申请
- 索引：`(CatId, Status)`，`(UserId, CreateTime DESC)`

## 5. 存储过程

新增 `Database/07_CitizenPortal.sql`，风格与现有脚本一致（可重复执行）。

| 过程 | 作用 |
| --- | --- |
| `usp_Report_Create` | 插入上报，状态固定 1 |
| `usp_Report_ListByUser` | 某个用户的上报分页列表 |
| `usp_Report_Search` | 管理员按状态/关键字分页查询 |
| `usp_Report_Approve` | 事务内 `EXEC usp_Cat_CreateWithRescue` 建档，回写 CatId、状态 2、审核人/时间；要求当前状态为 1 |
| `usp_Report_Reject` | 状态 1 → 3，写驳回理由；要求当前状态为 1 |
| `usp_Application_Create` | 校验猫为“待领养”、无重复待审核申请后插入 |
| `usp_Application_ListByUser` | 某个用户的申请分页列表 |
| `usp_Application_Search` | 管理员按状态/关键字分页查询 |
| `usp_Application_Approve` | 事务内 `EXEC usp_Adoption_Create` 生成领养记录并把猫改为已领养，回写 AdoptionId、状态 2、审核人/时间；要求当前状态为 1 |
| `usp_Application_Reject` | 状态 1 → 3，写驳回理由 |

审核类过程统一使用 `SET XACT_ABORT ON` + `TRY/CATCH`，并在读取目标行时加 `UPDLOCK, HOLDLOCK`，避免两个管理员同时审核同一条记录，或审核通过时猫状态已被其他操作改变。

## 6. 业务层与数据访问层

### 6.1 模型

`StrayReport`、`AdoptionApplication` 放入新增的 `StrayCat.Model/PortalRecords.cs`。

新增常量：

- `ReportStatuses`：`Pending = 1`、`Approved = 2`、`Rejected = 3`
- `ApplicationStatuses`：`Pending = 1`、`Approved = 2`、`Rejected = 3`

`Messages` 增加：上报成功、申请成功、重复申请、非待领养不可申请、审核状态已变化、驳回理由必填等。

### 6.2 BLL

`ReportBll`：

```text
Create(StrayReport report, int userId)          -> OperationResult<int>
ListByUser(int userId, int pageIndex, int pageSize) -> PagedResult<StrayReport>
Search(byte? status, string keyword, int page, int size) -> PagedResult<StrayReport>
Approve(int reportId, CatArchive cat, RescueRecord rescue, int adminId) -> OperationResult<int>
Reject(int reportId, string reason, int adminId) -> OperationResult<int>
```

`Approve` 的档案字段由管理员在审核表单补全；`ReportBll` 用 `FieldRules`/`CatRules` 校验后调用存储过程完成原子写入。`FoundWay` 固定为“市民送交”，`Location` 取上报地点，`InitialHealth` 取审核表单内容。

`ApplicationBll`：

```text
Create(AdoptionApplication application, int userId) -> OperationResult<int>
ListByUser(int userId, int pageIndex, int pageSize) -> PagedResult<AdoptionApplication>
Search(byte? status, int pageIndex, int pageSize) -> PagedResult<AdoptionApplication>
Approve(int applicationId, int adminId) -> OperationResult<int>
Reject(int applicationId, string reason, int adminId) -> OperationResult<int>
```

### 6.3 DAL

`ReportDal`、`ApplicationDal`，方法一一对应上述存储过程，映射函数放入 `RecordMaps`。所有调用经 `DbHelper`，保持“页面不出现 SQL”的现有约束。

## 7. 流程

### 7.1 上报流浪猫

```text
用户填写 ReportAdd → ReportBll.Create
  → FieldRules 校验（地点必填、日期不晚于今天、电话格式、描述长度）
  → usp_Report_Create（Status=1）
  → 跳 MyCenter，提示“已提交，等待管理员处理”

管理员打开 ReportList（默认筛待审核）
  → 采纳：弹出建档表单（临时名、性别、年龄、毛色、绝育、健康情况等）
      → ReportBll.Approve
          → usp_Report_Approve：EXEC usp_Cat_CreateWithRescue + 回写 CatId
  → 驳回：填写理由 → usp_Report_Reject
```

照片：上报时可选上传到现有图片目录，`PhotoPath` 保存相对路径；采纳建档时默认带入档案照片，管理员可替换。

### 7.2 领养申请

```text
公开页 AdoptableDetail → 点“申请领养”
  → 未登录：跳 Login.aspx?returnUrl=ApplyAdoption.aspx?catId=x
  → 已登录：填写姓名/电话/住址/理由 → ApplicationBll.Create
      → 校验猫状态为“待领养”
      → 校验本人对该猫没有待审核申请
      → usp_Application_Create
  → 跳 MyCenter，提示“申请已提交”

管理员 ApplicationList
  → 通过：ApplicationBll.Approve
      → usp_Application_Approve：再次校验猫状态
          → EXEC usp_Adoption_Create（生成领养记录、猫改为已领养）
          → 回写 AdoptionId、Status=2
  → 驳回：填理由 → usp_Application_Reject
```

### 7.3 并发与重复

- 同一条上报/申请只能被审核一次：过程内 `UPDLOCK, HOLDLOCK` + 状态条件更新。
- 同一用户对同一只猫只允许一条待审核申请，由筛选唯一索引兜底。
- 猫被别的申请领养后，其余待审核申请不会自动通过；管理员审核时会收到“当前状态不允许此操作”。

## 8. 状态机

```text
StrayReport:       1 待审核 ──采纳──> 2 已采纳
                        └──驳回──> 3 已驳回

AdoptionApplication: 1 待审核 ──通过──> 2 已通过
                          └──驳回──> 3 已驳回
```

2 和 3 为终态，不提供撤销；如需重来，由用户重新提交。

## 9. 错误处理

- 业务可预期错误：BLL 返回 `OperationResult.Fail(Messages.Xxx)`，页面显示。
- 数据库 `RAISERROR(...,16,1)`（编号 50000）由 `SqlErrorMapper` 原样透传。
- 唯一索引冲突（重复待审核申请）映射为“你已提交过申请，请等待审核”。
- 其他异常由 `Global.asax` 记录日志，页面跳 `Error.aspx`。

## 10. 迁移与兼容

- 存量 `Role=2` 账号不修改，语义从“工作人员”变为“普通用户”；上线后立即失去管理页访问权。
- 上线前确认至少一个可用的 `Role=1` 管理员账号（种子账号 `admin` 可用）。
- `RoleCodes` 中 `Staff` 改名为 `User`，同步更新所有引用点与登录页文案。
- 数据库脚本 `07_CitizenPortal.sql` 采用 `IF OBJECT_ID(...) IS NULL` / `IF NOT EXISTS` 写法，可重复执行。

## 11. 测试

单元测试（xUnit，不连库）：

- `ReportBll` / `ApplicationBll` 的字段校验与状态判断
- 重复申请、非待领养猫、已审核记录再次审核的返回值
- `ReportStatuses` / `ApplicationStatuses` 名称映射

集成测试（连本地 SQL Server）：

- 采纳上报后：档案与接收记录生成、上报状态与 CatId 正确
- 审核通过申请后：领养记录生成、猫状态为已领养、申请状态与 AdoptionId 正确
- 并发审核同一记录只成功一次

页面验证：

- 未登录访问公开页正常，访问 `/ReportAdd.aspx` 跳登录
- 普通用户访问 `/CatList.aspx`、`/Stat.aspx`、`/UserList.aspx` 被挡回首页
- 管理员可完成上报采纳与申请通过

## 12. 验收标准

1. 未登录用户能浏览待领养列表与详情。
2. 普通用户注册后只能看到“可领养猫、上报、我的中心、修改密码”。
3. 普通用户无法通过手输 URL 打开任何管理页。
4. 上报必须经管理员采纳后才生成档案，驳回时用户能看到理由。
5. 领养申请必须经管理员通过后才生成领养记录，且猫状态变为“已领养”。
6. 同一用户对同一只猫不能重复提交待审核申请。
7. 存量工作人员账号上线后不再具备管理权限。

## 13. 风险

| 风险 | 应对 |
| --- | --- |
| 存量账号失去管理权限后无人可管理 | 上线前确认 admin 可用；文档中注明 |
| 公开页泄露个人信息 | 公开查询只返回猫信息，不含上报人/申请人字段 |
| 照片上传占用空间或文件名冲突 | 复用现有图片保存逻辑，限制 1 张、2MB，文件名加时间戳 |
| 审核与领养并发 | 过程内行锁 + 状态条件更新，失败返回“当前状态不允许此操作” |
