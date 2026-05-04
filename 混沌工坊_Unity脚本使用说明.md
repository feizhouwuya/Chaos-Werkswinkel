# 《混沌工坊》Unity C# 脚本使用说明

## 1. 脚本内容

本目录已生成一套可运行的 2D/灰盒战斗原型脚本，位置为：

- `Assets/Scripts/ChaosWorkshop/Core`：枚举与基础类型。
- `Assets/Scripts/ChaosWorkshop/Data`：武器、卡牌、角色数据。
- `Assets/Scripts/ChaosWorkshop/Battle`：行动条、混沌浪潮、单位、卡组、战斗流程。
- `Assets/Scripts/ChaosWorkshop/UI`：手牌按钮、HUD、单位世界坐标显示。
- `Assets/Scripts/ChaosWorkshop/Demo`：一键生成 Demo 战斗。
- `Assets/Scripts/ChaosWorkshop/Narrative`：剧情选择和奖励接入点。

## 2. 快速运行 Demo

1. 用 Unity 打开当前工程或把 `Assets` 文件夹复制到 Unity 工程中。
2. 新建一个空场景。
3. 创建空物体，命名为 `Chaos Workshop Demo`。
4. 给该物体挂载 `ChaosWorkshopDemoBootstrap`。
5. 勾选 `Create Demo On Start`。
6. 点击 Play。

运行后会自动创建：

- 玩家：陆夕云原型，长剑，速度 3，攻击范围 2，基础伤害 4。
- 敌人：训练敌人，战斧。
- 战斗管理器、行动条、卡组、基础手牌 UI、混沌浪潮条。

玩家行动时可以点击手牌出牌，点击 `End Action` 结束本次行动。

## 3. 核心机制对应关系

- 行动条：`TimelineController.timelineLength`，默认 10。
- 单位行动推进：`CombatUnit.TickTimeline`，按单位速度每秒增长。
- 混沌浪潮：`TimelineController.chaosWaveSpeed`，默认每秒 1 单位；满 10 后恢复所有存活单位能量，并触发随机效果。
- 距离系统：`BattleManager.arenaMin` 到 `arenaMax`，默认 0 到 20。
- 武器模板：`WeaponLibrary.Create` 内置长剑、战锤、短刀、手弩、长枪、太刀、战斧。
- 卡牌费用：`CardDefinition.cost`。
- 卡牌伤害、移动、速度、行动条、抽牌、弱点等效果：`CardDefinition` 与 `CardEffect`。

## 4. 创建正式角色

在 Unity Project 面板中：

1. 右键选择 `Create > Chaos Workshop > Character Definition`。
2. 设置角色名称、阵营、最大生命、最大能量和初始武器。
3. 把初始卡牌资源拖入 `Starting Deck`。

目前脚本内置的初始武器数值来自策划案：

- 长剑：范围 2，伤害 4，速度 3。
- 战锤：范围 3，伤害 10，速度 1。
- 短刀：范围 1，伤害 2，速度 5。
- 手弩：范围 10，伤害 4，速度 5。
- 长枪：范围 5，伤害 5，速度 2。
- 太刀：范围 2，伤害 10，速度 1。
- 战斧：范围 3，伤害 5，速度 2。

太刀策划案中的括号数值可后续做成“架势切换”卡牌或武器状态；当前默认使用伤害 10、速度 1。

## 5. 创建卡牌

右键选择 `Create > Chaos Workshop > Card Definition`。

常用字段：

- `Display Name`：卡牌名。
- `Description`：卡牌描述。
- `Cost`：费用。
- `Target Rule`：目标规则，可选自己、单个敌人、所有敌人。
- `Damage`：额外伤害。若 `Use Weapon Damage` 开启，则最终伤害为武器基础伤害加该值。
- `Use Weapon Range`：开启时使用武器攻击范围。
- `Move Mode` 与 `Move Distance`：出牌时的位移。
- `Effects`：追加效果列表。

推荐示例：

- 弱点攻击：目标敌人，费用 1，使用武器伤害，追加 `ApplyWeakness = 2`。
- 辗转步：目标敌人，费用 1，`MoveMode = TowardTarget`，`MoveDistance = 2`。
- 撤步：目标敌人，费用 0，`MoveMode = AwayFromTarget`，追加 `GainEnergy = 1`。
- 扰乱时序：目标敌人，费用 1，追加 `ChangeTimeline = -3`。

## 6. 接入自己的场景 UI

最少需要：

- 场景中一个 `BattleManager`，它会自动要求同物体上存在 `TimelineController` 和 `DeckController`。
- 玩家和敌人物体各挂一个 `CombatUnit`。
- 手牌区域挂 `HandView`，并设置 `BattleManager` 与 `CardButtonView` 预制体。
- HUD 挂 `BattleHud`，绑定文本、混沌浪潮 Slider、结束行动 Button。

如果不想手动搭 UI，可以继续使用 `ChaosWorkshopDemoBootstrap` 自动生成的 Demo UI，再逐步替换为正式美术界面。

## 7. 剧情选择与敌方卡牌奖励

`StoryChoiceController` 和 `RunProgress` 是剧情分支接口：

- 每个 `StoryChoice` 可配置标题、正文、奖励。
- 奖励可包含一张卡牌、治疗量、最大能量提升。
- 调用 `StoryChoiceController.Choose(index)` 即可把奖励加入本轮进度。

正式 Roguelike 流程可按以下顺序串联：

战斗胜利 -> 打开剧情选择 UI -> 选择奖励 -> 把奖励卡加入 `RunProgress.deck` -> 进入下一场战斗。

## 8. 后续扩展建议

- 将敌人从单敌人扩展为多敌人队列。
- 为太刀加入“高速低伤 / 低速高伤”架势切换。
- 为海洛安加入预支费用：允许能量为负，并在下一次混沌浪潮扣除。
- 为拉缪加入手弩弹药恢复卡、短刀连击卡。
- 将 `EnemyUseBasicAction` 替换为敌人专属卡组 AI。
