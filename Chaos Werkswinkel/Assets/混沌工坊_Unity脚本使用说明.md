# 《混沌工坊》Unity 脚本使用说明 1.1v

本文档对应当前 Unity 原型工程，基于 `Chaos Werkswinkel/Assets/ChaosWorkshop` 目录下的脚本整理而成。文件名仍保留 `v1`，正文版本已统一为 `1.1v`。

## 1. 适用范围

本文档描述的是“当前工程已实现的原型能力”，主要用于：

- 快速跑通 Demo。
- 确认脚本目录与数据入口。
- 区分设计文档中的目标机制和已实际落地的功能。
- 作为后续继续扩展 ScriptableObject、敌人 AI 和 UI 的接线参考。

## 2. 当前工程目录

核心脚本目录：

```text
Chaos Werkswinkel/Assets/ChaosWorkshop
```

子目录说明：

- `Core`：基础枚举与共享类型。
- `Data`：武器模板、角色定义、卡牌定义、敌人定义、原型工厂。
- `Battle`：战斗管理器、单位运行时、行动条、牌库、敌人 AI。
- `UI`：HUD、手牌区、卡牌按钮、单位世界视图、立绘绑定。
- `Demo`：一键生成可运行灰盒场景。
- `Narrative`：剧情选择与跑团奖励原型接口。

说明：

- 旧文档中出现的 `Assets/Scripts/ChaosWorkshop` 已不再是实际路径。
- 当前项目脚本真实位置是 `Assets/ChaosWorkshop`。

## 3. Unity 版本

项目当前记录的 Unity 版本：

```text
2022.3.62f3c1
```

## 4. 快速运行 Demo

推荐流程：

1. 使用 Unity 打开 `Chaos Werkswinkel/` 工程目录。
2. 新建空场景，或使用 `Assets/Scenes/SampleScene.unity`。
3. 创建一个空物体，例如 `Chaos Workshop Demo`。
4. 挂载 `ChaosWorkshopDemoBootstrap`。
5. 保持 `Create Demo On Start` 为勾选状态。
6. 点击 Play。

运行后会自动创建：

- `BattleManager`
- `TimelineController`
- `DeckController`
- 玩家单位与敌人单位
- 基础 HUD
- 手牌按钮
- 混沌浪潮条
- 战场距离轨道

当前 Demo 默认参数：

| 项目 | 当前值 |
|---|---|
| 玩家 | 陆夕云 |
| 玩家生命 | 48 |
| 玩家武器 | 长剑 |
| 玩家牌组 | 陆夕云初始牌组 10 张 |
| 敌人 | 失控傀儡 |
| 敌人生命 | 28 |
| 敌人速度 | 2 |
| 敌人范围 | 2 |
| 敌人伤害 | 6 |
| 玩家起点 | 4 |
| 敌人起点 | 11 |
| 初始距离 | 7 |

## 5. 已实现的核心战斗规则

当前脚本已经实现并可运行的基础规则：

| 系统 | 当前实现 |
|---|---|
| 行动条长度 | 12 |
| 混沌浪潮长度 | 12 |
| 混沌浪潮速度 | 1 格/秒 |
| 单位行动开始加能量 | +1 |
| 混沌浪潮回能 | 所有单位恢复至能量上限 |
| 战斗开始抽牌 | 5 |
| 玩家行动开始抽牌 | 1 |
| 混沌浪潮抽牌 | 3 |
| 手牌上限 | 10 |
| 战场长度 | 20 |
| 每回合免费移动 | 2 |
| 免费移动次数 | 每次行动 1 次 |

同帧规则：

- `TimelineController` 先推进所有单位与混沌浪潮。
- 若混沌浪潮先达到阈值，会先触发 `ResolveChaosWave()`。
- 之后 `BattleManager` 再选择准备完成的单位行动。

这与数值设计文档中的“浪潮先于单位行动结算”保持一致。

## 6. 主要运行时脚本

### 6.1 `BattleManager`

职责：

- 管理战斗状态切换。
- 驱动玩家和敌人的行动回合。
- 管理免费移动、出牌、目标选择、战斗结束判断。
- 结算混沌浪潮。

当前特点：

- 主要是单玩家对单敌人的快速原型接口。
- 玩家回合默认只允许打出 1 张牌，再配合 1 次免费移动。
- 使用 `StartBattle(player, enemy, deck)` 启动战斗。

### 6.2 `TimelineController`

职责：

- 推进行动条。
- 推进混沌浪潮进度。
- 提供“谁先准备好”的读取逻辑。

当前特点：

- 行动条达到 12 即可行动。
- 选取准备好且行动值最高的单位。

### 6.3 `DeckController`

职责：

- 管理抽牌堆、弃牌堆、手牌。
- 处理洗牌、抽牌、超上限弃牌。

当前特点：

- 支持 `Draw(count)` 与 `DiscardToHandLimit()`。
- 当抽牌堆为空时会自动把弃牌堆洗回。

### 6.4 `CombatUnit`

职责：

- 保存单位运行时属性与状态。
- 处理生命、能量、护盾、行动条、位置、流血、弱点、闪避、弹药等。

当前已支持状态：

- 生命
- 能量
- 护盾
- 行动条
- 速度
- 战场位置
- 弱点
- 流血
- 闪避
- 罪债
- 下一次武器伤害加成
- 手弩弹药

当前实现边界：

- `sinDebt` 已写入运行时，并会降低下一次获得的护盾。
- 治疗同样受罪债影响的完整逻辑，当前并未完整展开。
- 告解层数、弃牌转收益等海洛安完整闭环尚未全部落地。

## 7. 数据结构说明

### 7.1 `WeaponLibrary`

当前内置武器模板：

| 枚举 | 名称 | 范围 | 伤害 | 速度 |
|---|---|---:|---:|---:|
| `LongSword` | Long Sword | 2 | 5 | 3 |
| `WarHammer` | War Hammer | 3 | 13 | 1 |
| `Dagger` | Dagger | 1 | 3 | 5 |
| `HandCrossbow` | Hand Crossbow | 10 | 5 | 5 |
| `Spear` | Spear | 5 | 7 | 2 |
| `TachiHeavy` | Tachi Heavy Stance | 2 | 13 | 1 |
| `TachiFast` | Tachi Fast Stance | 2 | 5 | 4 |
| `BattleAxe` | Battle Axe | 3 | 7 | 2 |

说明：

- 运行时显示名目前主要用于内部原型，不完全等同于中文设计文案。
- `HandCrossbow` 自带 3 点初始弹药，混沌浪潮恢复 1。

### 7.2 `CardDefinition`

当前核心字段：

- `cardId`
- `displayName`
- `description`
- `cost`
- `targetRule`
- `damage`
- `rangeOverride`
- `useWeaponDamage`
- `useWeaponRange`
- `hits`
- `ammoCost`
- `bonusDamageIfTargetHasWeakness`
- `bonusDamageIfMovedThisAction`
- `moveMode`
- `moveDistance`
- `movementCanChooseDirection`
- `shield`
- `applyWeakness`
- `applyBleed`
- `gainEvasion`
- `recoverAmmo`
- `draw`
- `gainEnergy`
- `timelineChange`
- `nextWeaponDamageBonus`
- `effects`

当前结算特点：

- 先处理位移，再检查距离并结算伤害。
- 多段伤害通过 `hits` 实现。
- 额外效果通过 `CardEffect` 列表扩展。

### 7.3 `BalancePrototypeFactory`

用途：

- 直接生成三名角色的初始牌组。
- 直接按 `unitId` 生成玩家或敌人的运行时单位。
- 在没有正式 ScriptableObject 资源时，作为原型数据入口。

已支持的角色与敌人：

- 玩家：`lu_xiyun`、`hai_luoan`、`lamu`
- 敌人：`rogue_puppet`、`rustblade_hunter`、`steam_guard`、`gun_wanderer`、`chaos_apprentice`、`broken_axle_knight`、`redline_sniper`、`furnace_steward`、`first_chaos_core`

## 8. 当前卡牌与角色落地情况

### 8.1 陆夕云

当前落地程度最高，已具备：

- 基础长剑牌组
- 弱点叠加
- 弱点额外伤害
- 剑势增伤
- 疾步斩位移后命中

### 8.2 海洛安

当前已部分落地：

- 战锤基础牌组原型
- `overdraftLimit`
- 预支后记录 `sinDebt`
- 护盾受罪债惩罚

尚未完整落地：

- 弃牌转告解
- 告解消耗并为攻击或护盾加成
- 赎罪移除罪债并返还能量
- 浪潮溢出弃牌额外获得护盾

### 8.3 拉缪

当前已基本支持：

- 短刀与手弩牌组
- 多段攻击
- 弹药消耗与恢复
- 位移后额外伤害
- 流血

尚未完整落地：

- 更完整的双武器切换表现
- 与“从远距离切入近身后下一张短刀牌增伤”完全对齐的显式机制提示

## 9. 敌人与 AI

当前敌人由两层组成：

- `EnemyLibrary`：提供敌人数值定义与动作原型。
- `EnemyAI`：按 `enemyId` 编写具体行为逻辑。

已具备专属 AI 的敌人：

- 失控傀儡
- 锈刃追猎者
- 蒸汽盾卫
- 枪械游民
- 混沌学徒
- 断轴骑士
- 红线狙击手
- 熔炉执事
- 初代混沌炉心

若敌人没有挂 `EnemyAI`，`BattleManager` 会退回基础逻辑：

- 在攻击范围内则攻击
- 不在范围内则向前移动 2

## 10. 混沌浪潮当前脚本状态

设计文档中的浪潮效果池比当前原型更完整。当前脚本已部分实现：

- 全体回能
- 玩家抽 3
- 超上限弃牌
- 手弩恢复弹药
- 通知敌人 AI 处理浪潮后逻辑
- 若干随机效果：
  - 全体速度提升
  - 全体行动条前进
  - 玩家恢复生命
  - 敌人行动条延后
  - 玩家失去能量
  - 其余结果暂以日志形式表现为“额外抽牌”

说明：

- JSON 中的完整浪潮效果池尚未完全数据驱动接入。
- 1.1v 文档已明确这一差异，避免把“设计目标”误当成“当前已实现功能”。

## 11. 场景中手动接入

最小可运行配置：

1. 创建一个挂有 `BattleManager` 的物体。
2. 让该物体自动附带 `TimelineController` 与 `DeckController`。
3. 为玩家和敌人各创建一个挂 `CombatUnit` 的物体。
4. 需要敌人专属行为时，为敌人再挂 `EnemyAI`。
5. 调用：

```csharp
List<CardDefinition> deck = BalancePrototypeFactory.CreateStartingDeck(CharacterArchetype.Swordsman);
battleManager.StartBattle(player, enemy, deck);
```

如果需要快速生成运行时单位：

```csharp
CombatUnit player = BalancePrototypeFactory.CreateRuntimeUnit(playerObject, "lu_xiyun", Team.Player);
CombatUnit enemy = BalancePrototypeFactory.CreateRuntimeUnit(enemyObject, "rogue_puppet", Team.Enemy);
player.InitializeRuntime(4f);
enemy.InitializeRuntime(11f);
```

## 12. 当前边界与建议

当前原型还不是完整商业战斗系统，主要边界如下：

- 默认接口仍是单玩家、单敌人原型流程。
- 多敌人结构只完成了部分目标与接口准备。
- 弃牌选择 UI、正式剧情 UI、奖励界面仍未接入。
- 海洛安的完整告解/赎罪闭环还需补齐。
- 混沌浪潮效果池尚未完全数据驱动。
- 正式 ScriptableObject 资源工作流仍可继续完善。

建议后续优先级：

1. 把海洛安的弃牌收益闭环补全。
2. 把混沌浪潮效果改成从 JSON 或正式定义表驱动。
3. 将多敌人遭遇与敌人意图显示加入 HUD。
4. 让 `SampleScene` 或专用 Demo 场景成为固定验收入口。
