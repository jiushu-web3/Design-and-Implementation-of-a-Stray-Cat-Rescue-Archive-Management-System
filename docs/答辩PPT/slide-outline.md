# Slide Outline

## Meta
- Topic: 流浪猫救助档案管理系统的设计与实现（毕业设计答辩）
- Scenario: 线下答辩，答辩委员会评审，约 15 分钟，17 页
- Content Source: provided（项目源码、架构设计文档、UML 图、系统截图、学长参考稿）
- Research: skip — 内容全部来自用户自有项目与文档，行业数据不编造
- Style: 暖调手账（沿用系统设计令牌）— 米白纸感底 `#FBF7F1`，珊瑚橘 `#E8836A` 作强调，抹茶绿 `#7E9E76` 作正向，深棕 `#3B332F` 正文；标题 Noto Sans SC Bold，正文 Noto Sans SC Regular；装饰为细分割线 + 珊瑚小色块 + 极淡圆形弧
- Slide Count: 17
- Generated At: 2026-11-12T10:45:00+08:00

## Source Materials
- 项目源码：`StrayCat.sln`（Web 29 个 aspx / BLL 9 类 / DAL 7 类 / Model / Tests 8 个测试类）
- 架构文档：`docs/架构设计文档.md`（分层、状态机、41 个存储过程、调用链）
- UML 素材：`docs/uml/png/*.png`（状态图直接用于 P8）
- 系统截图：`docs/screenshots/数据统计页.png`（用于 P15，标注演示数据）
- 参考稿：学长《实训(2).pdf》13 页（论文六章骨架）
- 固定元素：大连外国语大学校徽（`image-001.png`，右上角全片一致）

## Slide-by-Slide Outline
1. **Slide 1 — Cover** — 流浪猫救助档案管理系统 · 设计与实现 | Layout hint: L02 | Image provided: yes(logo) | Chart: no
2. **Slide 2 — Agenda** — 六章一条线：先讲为什么做，再讲怎么设计，最后证明它能跑 | Layout hint: L07 | Image provided: no | Chart: no
3. **Slide 3 — Background** — 救助靠微信群和纸笔，猫一换人管，档案就断了 | Layout hint: L07 | Image provided: no | Chart: no
4. **Slide 4 — Landscape** — 现有工具要么只管发布领养，要么只管看病开药 | Layout hint: L08 | Image provided: no | Chart: no
5. **Slide 5 — Requirement** — 两类角色、六个模块、29 个页面，就为一件事：档案不断链 | Layout hint: L07 | Image provided: no | Chart: no
6. **Slide 6 — Feasibility** — 技术、成本、操作三条线都成立，而且都能在答辩现场验证 | Layout hint: L07 | Image provided: no | Chart: no
7. **Slide 7 — Process** — 一条主线走完：救助到结案，中间只有两个回退点 | Layout hint: L13 | Image provided: no | Chart: no
8. **Slide 8 — StateMachine** — 5 个状态、7 条迁移，结案必须先有回访 | Layout hint: L17 | Image provided: yes(状态图) | Chart: yes
9. **Slide 9 — Architecture** — 四个项目单向依赖，Web 层在工程上就碰不到数据库 | Layout hint: L15 | Image provided: no | Chart: no
10. **Slide 10 — Modules** — 六个模块 29 个页面，41 个存储过程按前缀收口 | Layout hint: L19 | Image provided: no | Chart: no
11. **Slide 11 — Database** — 6 张表 4 对主从，业务规则靠约束写进库里 | Layout hint: L05 | Image provided: no | Chart: no
12. **Slide 12 — Impl·Auth** — 登录不只是比密码：验证码 + 加盐哈希 + 身份一致性 | Layout hint: L05 | Image provided: placeholder | Chart: no
13. **Slide 13 — Impl·Archive** — 档案中心：五项检索、一个事务写两表、详情一次往返拿全 | Layout hint: L07 | Image provided: placeholder | Chart: no
14. **Slide 14 — Impl·Adoption** — 领养不是插一条记录：登记、退养、回访都在事务里改状态 | Layout hint: L13 | Image provided: placeholder | Chart: no
15. **Slide 15 — Impl·Stat** — 年度接收 8 只、领养 5 只，62.5% 的领养率一眼可见 | Layout hint: L17 | Image provided: yes(统计页截图) | Chart: yes
16. **Slide 16 — Testing** — 26 个用例全过，绕过前端也写不进脏数据 | Layout hint: L07 | Image provided: no | Chart: no
17. **Slide 17 — Closing** — 做成了什么、还差什么，请各位老师指正 | Layout hint: L20 | Image provided: yes(logo) | Chart: no

## Visual Rhythm Notes
- Chart-bearing slides at positions: 8（状态机）, 11（表关系）, 15（统计趋势）
- Dark accent slides at positions: 1（封面）, 17（收尾）——深棕 `#3B332F` 底 + 米白字，呼应系统深色侧边栏
- Slides are viewed on large screens, often from a distance. Small text is unreadable. Every text element must be sized generously and fill its container space rather than floating in emptiness.
- Slides should feel visually rich and layered, not like text pasted on rectangles. Decorative elements add polish, and SVG charts communicate data far more effectively than text or numbers alone.
- 全片统一页眉：左侧章节眉标（24px 灰）+ 右侧校徽；统一页脚：takeaway 条 + 页码
- 截图位（P12-P14）使用带标注的占位框，等用户补拍真实截图后替换
