# 流浪猫救助档案管理系统设计说明

日期：2026-11-03

本文确定系统做什么、数据怎么放、代码怎么分层、页面有哪些、出错怎么处理、怎样算做完。实现按本文执行。论文正文由学生本人撰写，不在本文范围内。

## 1. 目标与约束

系统供一家小型流浪猫救助站内部使用，名称是「流浪猫救助档案管理系统」。工作人员把猫从接收、建档、医疗、领养到回访的记录留在系统里。外部人员不能注册，也不能浏览。

技术选择对着软件学院 .NET 方向的评分要求：

- Visual Studio 2022，ASP.NET Web Forms，.NET Framework 4.8，C#
- SQL Server 2012 及以上，本地可用 Express。连接默认使用 Windows 集成认证，数据库名 `StrayCatArchive`。若本机实例是 `localhost\SQLEXPRESS`，只改 `Web.config` 里的 Data Source
- 表示层、业务层、数据层三层，外加一个实体项目
- 数据访问使用 ADO.NET 和存储过程
- 动态 aspx 页面 27 个，全站一个母版页
- 样式集中在一个 CSS 文件，脚本集中在一个 JS 文件，页面同时使用验证控件
- 至少 6 张业务表，至少两对主外键
- 列表具备模糊查询、组合条件和分页
- 编程难度保持在中等，不引入 Entity Framework、ASP.NET Identity 和前端框架

运行方式是 Visual Studio 自带的 IIS Express。

## 2. 范围

系统包含登录、猫咪档案、救助接收、医疗记录、领养登记、回访记录、状态流转、账号管理和年度统计。

系统不包含对外注册、领养人登录、捐赠、志愿者、物资库存、数据字典维护界面、Excel 导出、图表库、邮件和短信。性别、状态等固定取值写在程序里，论文中用数据字典说明，不另建字典表。

同一救助站的已登录用户可以查看全部档案。不做按人隔离的数据权限。

## 3. 角色

| 角色 | 代码 | 能做的事 |
| --- | --- | --- |
| 管理员 | 1 | 工作人员的全部功能，加上账号管理和年度统计 |
| 工作人员 | 2 | 档案、接收、医疗、领养、回访和状态流转；修改自己的密码 |

工作人员看不到「用户管理」和「数据统计」菜单。直接输入这两个模块的地址时，系统回到首页并提示「没有权限访问该页面」。

初始账号只有一个管理员：用户名 `admin`，密码 `Admin@123`，姓名「系统管理员」。密码以盐 `0123456789ABCDEF0123456789ABCDEF` 与密码拼接后做 UTF-8 SHA256，十六进制小写摘要为 `9ddc3e471e45a5eab820bdecae8f62255d38a83619b4ce81caa8456909af3fa1`。种子脚本写入该盐和摘要。

## 4. 业务流程

一只猫的记录按这个顺序出现：

1. 工作人员在新增档案页同时填写档案和接收信息，一次保存写入两张表。
2. 新猫状态为「在站照料」，编号在保存时生成。
3. 治疗过程中可以新增多条医疗记录。
4. 可以领养时，把状态改为「待领养」。
5. 登记一条有效领养后，状态变为「已领养」。同一只猫同一时间只能有一条有效领养。
6. 退养时，该领养改为「已退养」，猫回到「待领养」，记录保留。之后可以再次领养。
7. 有效领养下可以新增多条回访。至少一条回访后，才能结案。
8. 死亡、转出或送回时，从「在站照料」或「待领养」改为「已终止」，并填写原因。

「已结案」和「已终止」的档案可以查询，不能再修改档案、接收、医疗、领养、回访和状态。

## 5. 状态规则

档案状态：

| 代码 | 名称 |
| --- | --- |
| 1 | 在站照料 |
| 2 | 待领养 |
| 3 | 已领养 |
| 4 | 已结案 |
| 5 | 已终止 |

允许的变化：

| 从 | 到 | 由谁触发 | 条件 |
| --- | --- | --- | --- |
| 在站照料 | 待领养 | 状态页 | 无额外条件 |
| 待领养 | 在站照料 | 状态页 | 无额外条件 |
| 待领养 | 已领养 | 新增领养保存 | 当前没有有效领养 |
| 已领养 | 待领养 | 领养页办理退养 | 该条领养当前有效，退养日期和原因必填 |
| 已领养 | 已结案 | 状态页 | 当前有效领养下至少一条回访 |
| 在站照料、待领养 | 已终止 | 状态页 | 终止原因必填 |

其他变化一律拒绝。状态页不提供「改为已领养」和「改为待领养（退养）」按钮，这两件事只随领养保存和退养发生，避免档案状态和领养记录不一致。

各状态下允许的写操作：

| 操作 | 在站照料 | 待领养 | 已领养 | 已结案 | 已终止 |
| --- | --- | --- | --- | --- | --- |
| 修改档案和接收 | 允许 | 允许 | 允许 | 拒绝 | 拒绝 |
| 医疗增删改 | 允许 | 允许 | 允许 | 拒绝 | 拒绝 |
| 新增领养 | 拒绝 | 允许 | 拒绝 | 拒绝 | 拒绝 |
| 修改有效领养的联系信息、办理退养 | 拒绝 | 拒绝 | 允许 | 拒绝 | 拒绝 |
| 新增和修改回访 | 拒绝 | 拒绝 | 允许，且只能挂在有效领养上 | 拒绝 | 拒绝 |
| 删除尚无回访的领养、删除回访 | 允许 | 允许 | 允许 | 拒绝 | 拒绝 |

档案尚未结案或终止时，才允许删除领养或回访，包括挂在已退养记录上的回访。该领养下还有回访时拒绝删除。删除一条「有效」领养且其下没有回访时，删掉该记录，并把猫改回「待领养」。删除一条「已退养」且没有回访的记录时，只删记录，不改当前档案状态。

删除猫的规则：存在任何医疗记录或任何领养记录时拒绝。只有接收记录时允许删除，接收记录在同一事务中先删掉。

## 6. 数据设计

六张表。外键删除行为一律为 NO ACTION，删除由存储过程按上一节的规则显式完成。

### 6.1 SysUser

| 列 | 类型 | 约束 |
| --- | --- | --- |
| UserId | INT IDENTITY | 主键 |
| UserName | NVARCHAR(20) | 非空，唯一，创建后不可改 |
| PasswordHash | CHAR(64) | 非空，SHA256 十六进制小写 |
| Salt | CHAR(32) | 非空，16 字节随机数的十六进制。种子管理员使用第 3 节的固定盐 |
| RealName | NVARCHAR(50) | 非空 |
| Role | TINYINT | 非空，1 管理员，2 工作人员 |
| Phone | NVARCHAR(20) | 可空 |
| IsEnabled | BIT | 非空，默认 1 |
| CreateTime | DATETIME | 非空，默认当前时间 |

账号只停用，不物理删除。不能停用当前登录者自己。不能把最后一个启用中的管理员停用，也不能把该管理员改成工作人员。建档人、接收人、医疗记录人、领养经办人和回访记录人都写成当前登录用户，页面上不另选。

### 6.2 CatArchive

| 列 | 类型 | 约束 |
| --- | --- | --- |
| CatId | INT IDENTITY | 主键 |
| CatNo | CHAR(11) | 非空，唯一。形式 `CAT` + 四位年 + 四位流水，例如 `CAT20260001` |
| TempName | NVARCHAR(50) | 非空 |
| Gender | TINYINT | 非空。0 未知，1 公，2 母 |
| EstimatedAge | NVARCHAR(20) | 可空，自由文本，如「约 1 岁」 |
| Color | NVARCHAR(50) | 可空 |
| IsNeutered | TINYINT | 非空，默认 0。0 未知，1 已绝育，2 未绝育 |
| Status | TINYINT | 非空 |
| TerminateReason | NVARCHAR(200) | 可空。状态为已终止时必须有值，其他状态为空 |
| PhotoPath | NVARCHAR(200) | 可空，网站内相对路径 |
| Remark | NVARCHAR(500) | 可空 |
| CreateUserId | INT | 非空，外键到 SysUser |
| CreateTime | DATETIME | 非空 |
| UpdateTime | DATETIME | 可空 |

编号在 `usp_Cat_CreateWithRescue` 的事务内生成。对当年编号加 `UPDLOCK, HOLDLOCK`，取最大流水加 1。流水超过 9999 时保存失败并提示。

索引：`Status`，`TempName`。

### 6.3 RescueRecord

一只猫一条。

| 列 | 类型 | 约束 |
| --- | --- | --- |
| RescueId | INT IDENTITY | 主键 |
| CatId | INT | 非空，唯一，外键到 CatArchive |
| RescueTime | DATETIME | 非空 |
| Location | NVARCHAR(200) | 非空 |
| FoundWay | NVARCHAR(50) | 非空。只允许：市民送交、工作人员发现、其他机构转入 |
| InitialHealth | NVARCHAR(500) | 非空 |
| ReceiverUserId | INT | 非空，外键到 SysUser |
| Remark | NVARCHAR(500) | 可空 |

索引：`RescueTime`。

### 6.4 MedicalRecord

一只猫多条。

| 列 | 类型 | 约束 |
| --- | --- | --- |
| MedicalId | INT IDENTITY | 主键 |
| CatId | INT | 非空，外键到 CatArchive |
| MedicalDate | DATE | 非空 |
| Diagnosis | NVARCHAR(200) | 非空 |
| Treatment | NVARCHAR(500) | 非空 |
| Medicine | NVARCHAR(200) | 可空 |
| RecordUserId | INT | 非空，外键到 SysUser |
| Remark | NVARCHAR(500) | 可空 |
| CreateTime | DATETIME | 非空 |

索引：`CatId`。

### 6.5 AdoptionRecord

一只猫可有多条。同一只猫只能有一条状态为有效的记录，用筛选唯一索引保证：

```sql
CREATE UNIQUE INDEX UX_Adoption_OneActive
ON AdoptionRecord(CatId)
WHERE Status = 1;
```

| 列 | 类型 | 约束 |
| --- | --- | --- |
| AdoptionId | INT IDENTITY | 主键 |
| CatId | INT | 非空，外键到 CatArchive |
| AdopterName | NVARCHAR(50) | 非空 |
| AdopterPhone | NVARCHAR(20) | 非空 |
| AdopterAddress | NVARCHAR(200) | 非空 |
| AdoptDate | DATE | 非空 |
| Status | TINYINT | 非空。1 有效，2 已退养 |
| ReturnDate | DATE | 可空。已退养时必填 |
| ReturnReason | NVARCHAR(200) | 可空。已退养时必填 |
| HandlerUserId | INT | 非空，外键到 SysUser |
| Remark | NVARCHAR(500) | 可空 |
| CreateTime | DATETIME | 非空 |

索引：`CatId`，`AdoptDate`。

### 6.6 FollowUpRecord

一条领养多条回访。

| 列 | 类型 | 约束 |
| --- | --- | --- |
| FollowUpId | INT IDENTITY | 主键 |
| AdoptionId | INT | 非空，外键到 AdoptionRecord |
| FollowDate | DATE | 非空 |
| FollowWay | NVARCHAR(50) | 非空。只允许：电话、上门、微信 |
| CatCondition | NVARCHAR(500) | 非空 |
| RecordUserId | INT | 非空，外键到 SysUser |
| Remark | NVARCHAR(500) | 可空 |
| CreateTime | DATETIME | 非空 |

索引：`AdoptionId`。

主从关系至少包括：猫对医疗、猫对接收、猫对领养、领养对回访，以及用户对被其经办的各类记录。

## 7. 解决方案结构

```
StrayCat.sln
Database/
  01_CreateDatabase.sql
  02_CreateTables.sql
  03_CreateProcedures.sql
  04_Seed.sql
StrayCat.Model/
StrayCat.DAL/
StrayCat.BLL/
StrayCat.Web/
```

引用方向只有一条：`StrayCat.Web` 引用业务层，业务层引用数据层，三层都引用实体项目。网站项目不引用数据层。

| 项目 | 内容 |
| --- | --- |
| StrayCat.Model | 表实体、`CatListItem`、`CatDetail`、`OperationResult<T>`、`PagedResult<T>` |
| StrayCat.DAL | `DbHelper` 和按表划分的数据类。只调用存储过程，把结果填成实体 |
| StrayCat.BLL | 按业务划分的类。负责规则、密码、事务性操作的调用顺序 |
| StrayCat.Web | aspx、母版页、CSS、JS、上传目录 |

`DbHelper` 提供 `ExecuteNonQuery`、`ExecuteScalar`、`ExecuteReader`。连接名是 `StrayCatDb`。

`OperationResult<T>` 包含 `Success`、`Message` 和 `Data`。业务规则失败时 `Success` 为 false，`Message` 是给用户看的中文。查询列表直接返回 `PagedResult<T>`。意外的数据库错误不包装成业务失败，继续向外抛。

密码规则：每个用户 16 字节随机盐，存 32 位十六进制。摘要是 `SHA256(UTF-8(盐的十六进制字符串 + 明文密码))` 的 64 位小写十六进制。登录时用同样规则计算再比较。用户不存在和密码错误都提示「用户名或密码错误」。账号停用提示「账号已停用」。

## 8. 存储过程

过程只做数据读写和必须放在同一事务里的状态更新。业务层先检查规则。过程对关键写入再检查一次，失败时 `RAISERROR` 中文说明，严重级别 16。数据层把这种用户错误转成 `OperationResult`。唯一索引冲突（错误号 2601 或 2627）也转成中文：用户名已存在、该猫已有接收记录、该猫已有一条有效领养。编号冲突提示「档案编号冲突，请重新保存」。

会改多张表的过程在过程内部使用一个事务：

| 过程 | 事务内完成的事 |
| --- | --- |
| usp_Cat_CreateWithRescue | 生成编号，插入档案和接收记录 |
| usp_Cat_Delete | 确认没有医疗和领养后，删除接收记录和档案 |
| usp_Adoption_Create | 确认猫为待领养后，插入有效领养，把猫改为已领养 |
| usp_Adoption_Return | 确认该领养有效且猫为已领养后，写成已退养，把猫改为待领养 |
| usp_Adoption_Delete | 档案已结案或已终止时拒绝。没有回访时删除。若删的是有效领养，把猫改为待领养 |
| usp_Cat_ChangeStatus | 只接受第 5 节列出的状态页变化，结案时检查回访条数，终止时检查原因 |

其余过程是单表读写：

- 用户：`usp_User_GetByUserName`、`usp_User_Insert`、`usp_User_Update`、`usp_User_SetEnabled`、`usp_User_ChangePassword`、`usp_User_GetById`、`usp_User_Search`、`usp_User_CountEnabled`
- 档案：`usp_Cat_Update`、`usp_Cat_GetDetail`、`usp_Cat_Search`、`usp_Cat_HomeCounts`、`usp_Cat_Recent`
- 接收：`usp_Rescue_Update`、`usp_Rescue_GetByCatId`
- 医疗：`usp_Medical_Insert`、`usp_Medical_Update`、`usp_Medical_Delete`、`usp_Medical_GetById`、`usp_Medical_ListByCat`
- 领养：`usp_Adoption_Update`、`usp_Adoption_GetById`、`usp_Adoption_ListByCat`
- 回访：`usp_FollowUp_Insert`、`usp_FollowUp_Update`、`usp_FollowUp_Delete`、`usp_FollowUp_GetById`、`usp_FollowUp_ListByAdoption`、`usp_FollowUp_CountByAdoption`
- 统计：`usp_Stat_Monthly`

`usp_Cat_Search` 的条件都是可选的：关键字（编号或临时名字，模糊）、状态、性别、接收时间起、接收时间止。状态和性别用 NULL 表示全部，因为性别代码 0 表示未知。日期含起止当天。排序为建档时间倒序、档案编号倒序。分页使用 `OFFSET / FETCH`，页码从 1 开始，每页 10 条，同时返回总条数。用户列表同样每页 10 条，关键字匹配用户名或姓名。

`usp_Cat_HomeCounts` 返回三个数：在站照料、待领养、已领养且其有效领养的回访数为 0。`usp_Cat_Recent` 返回最近 8 条档案。`usp_Stat_Monthly` 接收年份，返回 12 行：月份、该月接收数、该月领养数。领养数按领养日期统计，后来退养的仍然计入领养当月。

## 9. 页面

第 9.1 节的 aspx 和 `Site.Master` 放在网站根目录。母版页顶栏是系统名称、当前姓名和退出，左侧是菜单。工作人员的菜单没有用户管理和数据统计。`Login.aspx` 和 `Error.aspx` 不使用这个母版，只共用同一个 CSS。

登录后 Session 保存 `UserId`、`UserName`、`RealName`、`Role`，超时 30 分钟。`PageBase` 发现没有 Session 时转到 `Login.aspx?msg=expired`。`AdminPageBase` 继承 `PageBase`，角色不是管理员时转到 `Default.aspx?msg=forbidden`。退出时清空 Session。

地址中的编号无效时，页面显示「记录不存在或已被删除」，不抛出未处理异常。

样式文件是 `Content/css/site.css`，用 DIV 和盒模型布置顶栏、侧栏和内容区。脚本文件是 `Content/js/site.js`，负责必填、长度和手机号的 DOM 校验。每个录入页同时放置 `RequiredFieldValidator` 或 `RegularExpressionValidator`。业务层保存前再检查一次。

照片在新增和编辑档案时上传。允许扩展名 `.jpg`、`.jpeg`、`.png`，大小不超过 2MB。文件用新的 GUID 命名，放在 `Uploads/cats/`，数据库只存相对路径。更换照片后删除旧文件；旧文件已经不在磁盘上时，不因此让保存失败。

### 9.1 页面清单

| 页面 | 谁能访问 | 作用 |
| --- | --- | --- |
| Login.aspx | 匿名 | 登录 |
| Default.aspx | 登录用户 | 三个数量、最近 8 条档案。管理员额外看到启用中的管理员和工作人员合计 |
| ChangePassword.aspx | 登录用户 | 校验原密码后修改自己的密码 |
| Error.aspx | 匿名 | 通用错误说明 |
| UserList.aspx | 管理员 | 账号分页和关键字查询，停用或启用 |
| UserAdd.aspx | 管理员 | 新建账号，可选择角色 |
| UserEdit.aspx | 管理员 | 修改姓名、电话、角色、启用状态，可选重置密码。用户名只读 |
| CatList.aspx | 登录用户 | 组合查询和分页，进入详情 |
| CatAdd.aspx | 登录用户 | 同时录入档案和接收记录 |
| CatEdit.aspx | 登录用户 | 修改档案字段和照片 |
| CatDetail.aspx | 登录用户 | 档案、照片、接收摘要、最近医疗、领养历史、可用操作。删除档案的入口只放在这里 |
| CatStatus.aspx | 登录用户 | 按第 5 节显示当前状态允许的按钮 |
| RescueEdit.aspx | 登录用户 | 修改这一只猫的接收记录 |
| RescueDetail.aspx | 登录用户 | 查看接收记录 |
| MedicalList.aspx | 登录用户 | 某只猫的医疗列表 |
| MedicalAdd.aspx | 登录用户 | 新增医疗 |
| MedicalEdit.aspx | 登录用户 | 修改医疗 |
| MedicalDetail.aspx | 登录用户 | 查看一条医疗 |
| AdoptionList.aspx | 登录用户 | 某只猫的领养历史 |
| AdoptionAdd.aspx | 登录用户 | 登记有效领养 |
| AdoptionEdit.aspx | 登录用户 | 修改联系信息，或办理退养 |
| AdoptionDetail.aspx | 登录用户 | 查看一条领养及其回访入口 |
| FollowUpList.aspx | 登录用户 | 某条领养的回访列表 |
| FollowUpAdd.aspx | 登录用户 | 新增回访 |
| FollowUpEdit.aspx | 登录用户 | 修改回访 |
| FollowUpDetail.aspx | 登录用户 | 查看一条回访 |
| Stat.aspx | 管理员 | 选择年份，显示 12 个月的接收数和领养数。年份从最早一条接收记录的年份到今年；没有接收记录时只有今年 |

共 27 个 aspx。医疗、领养和回访从档案详情进入，不放进左侧主菜单。主菜单只有首页、猫咪档案、用户管理、数据统计、修改密码和退出。

列表无数据时显示「暂无记录」。

### 9.2 录入校验

| 项 | 规则 |
| --- | --- |
| 用户名 | 4 到 20 位字母或数字 |
| 密码 | 6 到 20 位 |
| 手机号 | `1` 开头的 11 位，第二位为 3 到 9。账号电话可空，填写时同样校验 |
| 临时名字、接收地点、初步情况 | 必填，不超过列长度 |
| 发现方式、回访方式 | 必须是第 6 节列出的选项 |
| 诊断、处理、领养人姓名、住址、猫咪状况、终止原因、退养原因 | 必填，不超过列长度 |
| 日期 | 必填。日期不能晚于今天。医疗日期和领养日期不能早于接收日期。退养日期、回访日期不能早于该条领养的领养日期 |

## 10. 出错处理

业务失败时用户留在当前页，说明显示在表单上方。主要说明使用这些句子：

- 用户名或密码错误
- 账号已停用
- 用户名已存在
- 没有权限访问该页面
- 登录已过期，请重新登录
- 当前状态不允许此操作
- 请填写终止原因
- 至少有一条回访才能结案
- 该猫已有一条有效领养
- 存在医疗或领养记录，不能删除该档案
- 存在回访记录，不能删除该领养
- 不能停用当前登录账号
- 不能停用或降级最后一个管理员
- 只允许上传 jpg 或 png 图片
- 图片不能超过 2MB
- 记录不存在或已被删除
- 当年档案编号已用完

未预料的异常由 `Global.asax` 的 `Application_Error` 接收。日志追加到 `App_Data/logs/yyyyMMdd.txt`，内容是时间、请求地址、当前用户名和异常文本。日志不记录表单内容，避免写下密码。`Web.config` 中 `customErrors mode="On"`，浏览器只进入 `Error.aspx`，页面文字是「系统处理这次操作时出现了问题，请稍后再试。如果反复出现，请把发生时间和页面告诉管理员。」

## 11. 验收

在本地网站用浏览器逐条完成。全部通过才算这一版做完。

1. 使用 `admin` / `Admin@123` 登录成功。错误密码提示「用户名或密码错误」。停用后的工作人员不能登录。
2. 工作人员的菜单没有用户管理和统计。直接打开 `UserList.aspx` 和 `Stat.aspx` 时回到首页并提示没有权限。
3. 新增一只猫后，档案和一条接收记录同时存在，编号形如 `CAT20260001`。
4. 档案列表可以只按名字模糊查询，也可以同时限定状态、性别和接收日期，结果按每页 10 条分页。没有数据时显示「暂无记录」。
5. 状态只按第 5 节变化。待领养之前不能新增领养。退养后猫回到待领养，原记录仍可查看。没有回访时不能结案。
6. 只有接收记录的猫可以删除。有医疗或领养时删除被拒绝。有回访的领养不能删除。
7. 已结案和已终止的档案打不开保存操作。
8. 管理员可以新建工作人员并重置其密码。不能停用自己，也不能停用最后一个管理员。
9. 首页三个数量与数据一致。统计页所选年份的 12 个月数字与接收日期、领养日期一致。
10. 上传 pdf 或超过 2MB 的文件时，页面给出中文说明，档案不被写成坏路径。上传一张 jpg 后，详情页能显示该图。
11. 制造一次未处理异常后，浏览器进入 `Error.aspx`，当天日志文件中有记录，页面上看不到 SQL 文本。

论文的测试一章可以由学生依据这 11 条改写。本文不生成论文。

## 12. 实现时保持的边界

- 页面代码只调用业务层。
- 业务规则的中文说明集中在业务层和存储过程的 `RAISERROR`，页面不复制一套相反的规则。
- 不为了凑页面而增加没有字段和操作的空页。第 9 节的 27 个页面都按表中的用途实现。
- 不增加本文没有列出的模块。
