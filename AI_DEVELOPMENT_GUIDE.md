# Katana&Dungeon 项目开发速查（供 AI 接手）

> 依据当前工作区的源码、Unity 配置和资源目录整理。未在 Unity Editor 中打开或构建项目；场景组件引用、目标平台 SDK/打包配置仍应在 Editor 与目标设备上复核。代码统计只覆盖 `Assets/Scripts`，第三方代码另计。

## 项目概况

- Unity `2022.3.17f1c1`，URP `14.0.9`，C# 主程序集由 Unity 默认 Assembly-CSharp 管理。
- 产品名 `Katana&Dungeon`，版本 `1.4.1`；Android/iOS 包标识目前均为 `com.shaderealm.acshy`。启动设置为 60 FPS、竖屏。
- 当前构建场景依次为 `Loading`、`GameStart`、`TestScene`、`NullScene`，均启用。正式入口和测试场景的边界需要结合场景内容复核。
- `Assets/Scripts` 有约 600 个 C# 文件。`Assets/Arts` 有约 29,782 个文件、约 3.37 GB；目录体量大，查找资源优先按场景、Prefab、AssetBundle manifest 的引用链定位。
- 仓库当前没有可用 Git 提交历史；本工作区也包含大量 Unity 生成目录和其他项目的兄弟目录。AI 修改时只操作本项目文件，并先确认目标文件的现有状态。

## 目录地图

| 路径 | 用途 |
| --- | --- |
| `Assets/Scripts/Manager` | GameManager、战斗、数据、存档、场景、UI/音频等全局管理器 |
| `Assets/Scripts/Game` | 角色、敌人、关卡内可交互对象、主城和环境行为 |
| `Assets/Scripts/Battle` | 战斗规则、房间、投射物、机关、掉落与战斗运行时数据 |
| `Assets/Scripts/BehaviorTree` | 玩家/敌人行为树动作、条件和控制器 |
| `Assets/Scripts/Item` | 道具效果、触发器和 Buff 行为 |
| `Assets/Scripts/UIPanel`、`Assets/Scripts/UI` | 主界面、战斗 UI、弹窗和通用 UI 组件 |
| `Assets/Scripts/DataManagerExpand`、`Assets/Scripts/TabData` | CSV/表格装载、解析和运行时表结构 |
| `Assets/Scripts/ScriptableObjects` | Buff、道具、技能、攻击与反馈等配置定义 |
| `Assets/Scripts/Archive`、`Assets/Scripts/ArchiveManagerExpand` | 存档数据结构及存档相关扩展 |
| `Assets/Scripts/GameTools` | 单例、事件、资源、UI、CRC 等项目内基础设施 |
| `Assets/AssetsPackage` | 运行时内容源：章节、敌人/玩家 Prefab、ScriptableObject、CSV、UI、音频/VFX 等 |
| `Assets/StreamingAssets` | 当前已生成的 AssetBundle 文件和 manifest；运行时设备端从这里加载 |
| `Assets/BundleAssets` | Bundle 内容或打包测试相关资源，改动前检查对应打包工具与引用 |
| `Assets/Scenes`、`Assets/Prefabs` | Unity 场景和通用 Prefab；Manager 根 Prefab 位于 `Assets/Prefabs/Managers` |
| `Assets/Editor` | 章节/战斗关卡编辑器和 Item 编辑器 |
| `Assets/Arts` | 项目自有及第三方美术、模型、动画、材质、音频、特效资源 |
| `Assets/Resources` | Unity `Resources.Load` 运行时资源及 URP/行为树配置；避免无意扩大 Resources 收录范围 |
| `Assets/Plugins` | 第三方代码/插件（见 SDK 与插件清单） |
| `Packages/manifest.json` | Unity Package Manager 依赖清单 |
| `ProjectSettings` | Unity 版本配套设置、构建场景、平台与渲染配置 |

`Assets/Arts` 下主要分类包括 `3rd_Party`、`Animation`、`AnimatorController`、`Audio`、`Material`、`Models`、`RoomPrefabs`、`Shaders`、`Textures`、`Vfxs`。`3rd_Party` 内有 Jade Mobile Bundle、MeshSmith、Polygon Dungeon Realms、Polygon Fantasy Hero Characters 等内容。

## 启动与主要运行链路

1. `GameManager.Awake()` 设置全局实例、常驻对象、分辨率/方向，并启动初始化协程。
2. 初始化 AssetBundle 资源管理、语言、时间、流程、UI、音频、战斗、场景、反馈、数据、交互、可破坏物和存档管理器。
3. 完成等待后启动 `MainSceneProcedure`。主流程按存档进度进入主界面或首次教程战斗；章节战斗由 `BattleManager` 和 `IBattleRules` 实现。
4. 流程由 `ProcedureManager` 持有当前 `IProcedure`。主界面、战斗切换分别在 `MainSceneProcedure`、`BattleSceneProcedure` 中处理。
5. `DataManager` 将 CSV 解析为运行时字典/表；玩法配置主要从 `Assets/AssetsPackage` 的 ScriptableObject 加载。
6. `ResourcesManager.GetAsset<T>()` 在编辑器模式下走 `AssetDatabase`；设备端走 `StreamingAssets` 中 AssetBundle 与配置索引。加载接口以回调返回。
7. 游戏事件由 `EventManager` 发布/订阅；道具、角色、UI 和 SDKManager 部分通过事件名解耦。

主要入口文件：`Assets/Scripts/Manager/GameManager.cs`、`Assets/Scripts/Manager/Procedure/ProcedureManager.cs`、`Assets/Scripts/Manager/Procedure/Scr/MainSceneProcedure.cs`、`Assets/Scripts/Manager/BattleManager.cs`、`Assets/Scripts/Manager/DataManager.cs`。

## 修改约定与 AI 工作建议

- Unity 资产的 `.meta` 文件包含 GUID。新增、移动或删除资产时使用 Unity Editor 或保留配套 `.meta`；不要手工重建 GUID，也不要仅按文件名替换 Prefab/场景引用。
- 改动 MonoBehaviour 序列化字段时检查相关场景和 Prefab 的 Inspector 引用。场景、Prefabs 与 `Assets/AssetsPackage` 的配置关系较紧，代码编译通过并不代表运行时引用正确。
- 表格字段由 `DataManagerExpand` 解析、`TabData` 承载；加字段时同步确认 CSV 列顺序/格式、加载逻辑和消费方。枚举名字可能通过 `Enum.Parse` 从表格文本读取，改名需一并迁移数据。
- 加载运行时资源时沿用 `ResourcesManager.GetAsset<T>` 和仓库使用的资源路径格式；资源重打包后核对 `Assets/StreamingAssets` 的 Bundle 与 manifest 是否一起更新。
- 存档结构在 `Assets/Scripts/Archive/Data`，版本号目前由 `ArchiveManager` 通过 PlayerPrefs 单独读取。变更字段需提供显式迁移和旧档回归验证，不要只更新数据类版本字符串。
- 遵循现有 `partial` 扩展管理器、事件常量和战斗规则接口的组织方式。不要在单个改动里顺手重构大体积第三方插件或导入美术包。
- `Assets/Scripts/Temp`、`Testxx.cs`、`TestScene`、`TEST_MODE` 属于明显测试/调试区域；确认发布行为前不要假定其已从构建中排除。
- 当前未发现 `Assets` 中的 Unity Test Framework 测试源码；修改核心玩法/存档/资源加载时，应至少做目标场景的 Editor 运行验证，设备端构建还需检查 AssetBundle 加载。

## SDK 与第三方插件盘点

### 平台/服务 SDK 接入痕迹

| 名称 | 当前证据 | 状态判断 |
| --- | --- | --- |
| Facebook Unity SDK | `Assets/FacebookSDK/SDK/Resources/FacebookSettings.asset` 存在；`SDKManager.cs` 中 `Facebook.Unity`、`FB.Init`、激活 App 的代码被注释；未找到 SDK 二进制或实际调用 | 工程保留配置/接入痕迹，未见当前启用的 Facebook 登录、分享或分析调用。配置中的 App ID/token 为空/占位值 |
| TGA Analytics（代码命名） | `SDKManager`、`TGAName.cs` 中有事件名、事件数据组装代码；`TGAnalytics.Track` 和初始化均被注释，未找到对应 SDK 实现/二进制 | 有埋点事件设计与事件总线触发，不等于埋点 SDK 正在上报；供应商全称和平台归属需向项目维护者确认 |
| Google Play Services Resolver | `Assets/PlayServicesResolver` 含 Google Jar/iOS/Version Handler 编辑器 DLL | 这是依赖解析工具，不是 Google 登录/广告/支付 SDK；当前未发现 Google Play Games 或 Google Mobile Ads 的项目代码/原生库 |
| 其他常见平台服务（Firebase、Adjust、AppsFlyer、TapTap、微信、IAP、Game Center、广告 SDK） | 在项目 C#、Packages 和原生库扩展名范围内未找到有效接入证据 | 不能据此证明历史上从未接过；本次静态盘点未发现当前工程中的实际代码或原生包 |

### 项目依赖插件（不是平台 SDK）

- Odin Inspector / Sirenix Serialization：Inspector 扩展与序列化支持。
- Behavior Designer：行为树运行时/编辑器；项目 `Assets/Resources/BehaviorTree` 有行为树资产。
- DOTween / DOTween Pro：动画与补间。
- SRDebugger / SRF：调试菜单、运行时选项与诊断工具。
- FlexReader（`FlexFramework.Excel`）、Newtonsoft.Json、SharpZipLib：表格/数据处理依赖。
- Dynamic Bone、UIEffect、UHUDText、ConsolePro、RedBlueGames Mulligan Renamer、Reference Finder/Build Report 等。
- Package Manager 依赖还包括 URP、UGUI、TextMeshPro、Timeline、AI Navigation、Test Framework 等；完整版本见 `Packages/manifest.json` 与 `packages-lock.json`。

## 已知风险与待验证项

以下是静态检查得到的风险线索，不代表每项都已在运行时复现。

| 优先级 | 风险 | 证据与影响 |
| --- | --- | --- |
| 高 | 旧存档可能被静默重置 | `Assets/Scripts/Manager/ArchiveManager.cs` 的 `TemporaryArchiveData()` 仅识别 `1.4.0`；未知版本走 `NewArchive()`，反序列化异常也走新建存档。旧版迁移代码在 `ArchiveData_1_4_0.cs` 中被注释。升级时可能丢失玩家进度，需先备份并补迁移/错误提示 |
| 高 | 初始化完成标记可能早于数据实际可用 | `DataManager.Init()` 启动多个回调式资源加载后立即递减 `initNumber`；`IsInit` 只检查 `initNumber == 0`。`GameManager` 等待该标记后进入主流程，因此主界面/战斗可能在 CSV/Bundle 回调尚未完成时读取数据 |
| 高 | 无尽模式路径有未实现抛错 | `EndlessRules.LoadBattle(IArchiveBattleData)`、`EndlessRulesRuntimeData.SetLevelStructData()`（传入非 Endless 数据）以及 `ChapterRulesRuntimeData.SetLevelStructData()` 的未知类型分支会抛 `NotImplementedException`。相关存档恢复或错误类型输入会直接中断流程 |
| 中 | AssetBundle 配置读取缺少失败保护 | `ResourcesManager.LoadAssetBundleConfig()` 假设 `assetbundleconfig` 文件和 `AssetBundleConfigByte` 一定存在；若缺包/损坏，后续解引用可能异常，启动阶段无明确降级提示 |
| 中 | 场景对象名称是隐式契约 | 多处使用 `GameObject.Find("Entrance_Z")`、`DropItemSpawnPosition`、`EnemySpawnPoints`、`MeteorSpawnPoints` 等名称查找；重命名或 Prefab 层级调整可导致空引用，相关场景改动需同步验证 |
| 中 | 战斗规则当前固定为章节规则 | `BattleManager.LoadChapterBattle()` 当前实例化 `ChapterRules`，AdventureRules 选择仍是注释/TODO；如预期支持冒险规则，要确认模式入口而非只补规则类 |
| 中 | Bundle 清单/运行时数据可能不同步 | 88 个 Bundle 文件和 88 个 manifest 位于 `StreamingAssets`；任一资源或依赖改变后若未重新打包，编辑器模式可能正常而设备端缺资源。设备端配置加载应作为发布门禁 |
| 低 | 调试/测试内容未显式从构建排除 | `TestScene` 已在 Build Settings 启用，Player Settings 含 `TEST_MODE` define，代码中还有 `Temp`、`Testxx` 与测试分支。发布前应检查目标平台实际构建配置和测试入口 |
| 低 | 平台标识仍是通用值 | Player Settings 的公司名为 `DefaultCompany`，Android/iOS identifier 均为 `com.shaderealm.acshy`。上架前需核对品牌、包名、签名、权限和 SDK 合规设置 |

## 首选阅读顺序

1. `Assets/Scripts/Manager/GameManager.cs`：入口与初始化。
2. `Assets/Scripts/Manager/Procedure`：场景/玩法流程。
3. `Assets/Scripts/Manager/BattleManager.cs`、`Assets/Scripts/Battle/BattleRules`：战斗组织与规则。
4. `Assets/Scripts/Manager/DataManager.cs`、`Assets/Scripts/DataManagerExpand`、`Assets/Scripts/TabData`：配置加载链路。
5. `Assets/Scripts/GameTools/ResourceManager`、`Assets/Scripts/Manager/SaveManager.cs`、`Assets/Scripts/Manager/ArchiveManager.cs`：资源和存档。
6. 对照 `Assets/Prefabs/Managers`、`Assets/Scenes`、`Assets/AssetsPackage` 与 `Assets/StreamingAssets` 查找实际序列化引用。
