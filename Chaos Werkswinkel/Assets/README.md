# 混沌工坊

《混沌工坊》是一个基于 Unity 2022.3 的 2D 半即时卡牌 Roguelike 原型仓库。当前整理版本为 `1.1v`，已统一根目录说明文档、Unity 工程内镜像文档与原型脚本说明。

## 项目结构

- `Chaos Werkswinkel/`：Unity 工程目录。
- `Chaos Werkswinkel/Assets/ChaosWorkshop`：核心原型脚本目录。
- `chaos_workshop_balance_v1.json`：脚本可读的数值草案，内容版本已更新到 `1.1v`。
- `混沌工坊_数值设计_v1.md`：核心战斗、角色、武器与混沌浪潮数值设计。
- `混沌工坊_1.1v总结说明.md`：本轮 `1.1v` 整理范围、问题与后续建议总结。
- `混沌工坊_Unity脚本使用说明.md`：Unity 原型、Demo 和运行时接线说明。
- `混沌工坊_敌人设计_v1.md`：第一章敌人设计与遭遇建议。
- `混沌工坊_敌人脚本使用说明.md`：敌人数据、AI 行为与场景接入说明。

## 文档约定

- 根目录文档为主版本，`Chaos Werkswinkel/Assets/` 下保留同步镜像，方便在 Unity 编辑器内直接查看。
- 文件名仍沿用 `_v1` 与 `_v1.json`，以减少路径变更；正文版本与 JSON 的 `version` 字段以 `1.1v` 为准。
- 战斗常量、关键命名和原型边界应同时对齐 Markdown、JSON 与 Unity 脚本。

## 快速开始

1. 使用 Unity `2022.3.62f3c1` 打开 `Chaos Werkswinkel/`。
2. 打开空场景，或直接使用 `Chaos Werkswinkel/Assets/Scenes/SampleScene.unity`。
3. 在场景中创建空物体并挂载 `ChaosWorkshopDemoBootstrap`，或直接调用它的 `BuildDemo()`。
4. 参考 `混沌工坊_Unity脚本使用说明.md` 验证行动条、混沌浪潮、手牌 UI 与 Demo 战斗流程。

## 当前原型范围

- 已完成：行动条、混沌浪潮、距离系统、基础卡牌、单敌人战斗、角色原型、敌人 AI、基础 HUD。
- 已明确为后续项：完整弃牌选择、海洛安的告解/赎罪闭环、完整混沌浪潮效果池、多敌人正式遭遇生成。
- `.gitignore` 已忽略 `Library`、`Temp`、`Obj`、`Build`、`Logs` 等 Unity 生成目录；后续整理仓库时不应继续提交这些产物。
