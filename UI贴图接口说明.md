# 《混沌工坊》UI 贴图接口说明

本文档说明当前 UI 脚本中新增的贴图接口，方便后续补美术素材时直接在 Unity Inspector 中挂接。

## 1. 目标

本轮调整的目标是：

- 所有现有 UI 脚本都提供可扩展的贴图入口。
- 优先通过数据映射和 Inspector 配置接入素材，减少后续硬编码。
- 没有素材时仍保留当前灰盒原型的纯色 UI，不阻塞战斗验证。

涉及脚本：

- `Assets/ChaosWorkshop/UI/BattleHud.cs`
- `Assets/ChaosWorkshop/UI/HandView.cs`
- `Assets/ChaosWorkshop/UI/CardButtonView.cs`
- `Assets/ChaosWorkshop/UI/UnitWorldView.cs`
- `Assets/ChaosWorkshop/UI/UIArtworkBindings.cs`

## 2. 通用绑定结构

核心公共结构都在 `UIArtworkBindings.cs` 中。

### 2.1 静态 UI 图槽

`UIImageArtworkSlot`

用途：

- 给普通 `Image` 挂静态底图、装饰图、边框图。

常用字段：

- `target`：目标 `Image`
- `sprite`：要显示的 Sprite
- `preserveAspect`：是否保持比例
- `setNativeSize`：是否按 Sprite 原始尺寸设置
- `disableTargetWhenSpriteMissing`：缺图时是否隐藏
- `overrideColor` / `color`：是否覆盖颜色

适合场景：

- 面板底板
- HUD 装饰
- 卡牌通用边框
- 统一按钮底图

### 2.2 单位头像图集

`UIUnitArtworkLibrary`

用途：

- 根据单位信息自动找头像或角色图。

支持三种映射：

- `spritesByDisplayName`
- `spritesByArchetype`
- `spritesByWeapon`

优先级：

1. `displayName`
2. `CharacterArchetype`
3. `WeaponKind`
4. `defaultSprite`

### 2.3 卡牌贴图库

`UICardArtworkLibrary`

用途：

- 根据卡牌信息自动找插画或卡面图。

支持两种映射：

- `spritesByCardId`
- `spritesByDisplayName`

优先级：

1. `cardId`
2. `displayName`
3. `defaultSprite`

### 2.4 场景单位贴图

`UIUnitSpriteRendererBinding`

用途：

- 给 2D `SpriteRenderer` 角色挂图。

`UIUnitTextureRendererBinding`

用途：

- 给 `Renderer` / `MeshRenderer` 角色模型贴 `Texture`。
- 当前 Demo 里的 Cube 单位主要走这一套。

## 3. 各脚本新增接口

## 3.1 `BattleHud`

新增重点：

- `playerPortraitImage`
- `enemyPortraitImage`
- `playerPortraitBinding`
- `enemyPortraitBinding`
- `artworkSlots`

另外把玩家和敌人的文本拆成了：

- `playerNameText`
- `playerStatsText`
- `playerStatusText`
- `enemyNameText`
- `enemyStatsText`
- `enemyStatusText`

推荐接法：

1. 在 HUD 中放两个头像 `Image`。
2. 分别挂到 `playerPortraitImage` 和 `enemyPortraitImage`。
3. 在 `playerPortraitBinding.artwork` / `enemyPortraitBinding.artwork` 中填映射表。
4. 若需要 HUD 固定边框、背景板、图标装饰，把对应 `Image` 放进 `artworkSlots`。

## 3.2 `HandView`

新增重点：

- `cardArtworkLibrary`
- `artworkSlots`

推荐接法：

1. 把整套卡牌插画映射填进 `cardArtworkLibrary`。
2. 如果手牌区域还有额外底板、边框、分栏图，也可以挂到 `artworkSlots`。
3. `HandView` 会把这套卡图配置自动传给它生成的所有 `CardButtonView`。

## 3.3 `CardButtonView`

新增重点：

- `backgroundImage`
- `illustrationImage`
- `frameImage`
- `costBadgeImage`
- `backgroundBinding`
- `illustrationBinding`
- `frameBinding`
- `costBadgeBinding`
- `artworkSlots`

推荐接法：

- `illustrationImage`：角色牌或动作牌插画
- `frameImage`：职业框、稀有度框、发光框
- `backgroundImage`：整张卡底色或卡背风格
- `costBadgeImage`：费用角标、能量徽章

说明：

- 同一张卡的多种图层可以共用一套 `UICardArtworkLibrary`。
- 如果底板和费用徽章不需要按卡区分，直接在 `artworkSlots` 里挂死即可。

## 3.4 `UnitWorldView`

新增重点：

- `spriteRendererBinding`
- `textureRendererBinding`

推荐接法：

- 正式 2D 单位优先用 `SpriteRenderer`，配置 `spriteRendererBinding`
- 现有 3D/Cube 灰盒单位可配置 `textureRendererBinding`

注意：

- `textureProperty` 默认 `_MainTex`
- 如果后续改自定义材质或 Shader，需要同步调整这个字段

## 4. Demo 中已补好的挂点

`Assets/ChaosWorkshop/Demo/ChaosWorkshopDemoBootstrap.cs` 已同步调整：

- 运行时创建的 HUD 现在包含头像位。
- 运行时生成的卡牌预制体现在包含：
  - 插画位
  - 边框位
  - 费用徽章位
- 运行时创建的场景单位会自动把 `Renderer` 接到 `UnitWorldView.textureRendererBinding`

这意味着：

- 即使还没做正式预制体，也可以先在 Demo 生成的结构上验证挂图逻辑。

## 5. 推荐素材目录

建议按用途分目录，避免后面素材越来越多时难找：

- `Assets/Art/UI`：面板底图、按钮、框线、HUD 装饰
- `Assets/Art/Characters`：角色头像、单位立绘、敌人头像
- `Assets/Art/Cards`：卡牌插画、卡框、费用徽章
- `Assets/Art/Placeholder`：占位图、测试图

## 6. 缺图时的行为

当前接口是渐进式的：

- 没挂图时，HUD 和卡牌仍可以用当前纯色原型正常工作。
- `UIImageArtworkSlot.disableTargetWhenSpriteMissing` 可控制静态 `Image` 缺图时是否隐藏。
- `UIUnitImageBinding.hideTargetWhenSpriteMissing` 可控制头像位缺图时是否隐藏。
- `UIUnitSpriteRendererBinding.hideRendererWhenSpriteMissing` 可控制 2D 单位缺图时是否隐藏。
- `UIUnitTextureRendererBinding` 如果没找到贴图，默认保留当前模型显示，不会强制清空材质。

## 7. 推荐接入流程

建议后续美术接入按下面顺序做：

1. 先补角色头像。
2. 再补卡牌插画和卡框。
3. 最后再替换 HUD 底板、按钮和战场单位贴图。

如果要先快速跑通一版：

1. 给 `BattleHud` 的头像位挂占位头像。
2. 给 `HandView.cardArtworkLibrary` 先按 `cardId` 填几张关键卡的插画。
3. 给 `CardButtonView` 的 `frameImage` 和 `costBadgeImage` 挂统一样式图。
4. 给 `UnitWorldView.textureRendererBinding` 挂敌我占位贴图验证世界内显示。

## 8. 当前限制

- 现在的卡图映射仍是“按卡牌找 Sprite”，还没有单独拆出稀有度、职业主题、动画状态等更细粒度规则。
- `BattleHud` 的按钮图目前仍主要靠 `Image + 颜色`，如果后续需要按钮的 Normal/Highlighted/Pressed 多状态贴图，可以继续扩展到 `SpriteState`。
- `UnitWorldView` 目前只负责位置同步和贴图应用，不负责受击特效、待机动画或切图状态机。
