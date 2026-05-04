# Chaos Workshop Agent Guide

This repository contains the Unity C# prototype and balance documentation for the 2D semi-real-time card roguelike **《混沌工坊》**.

## Project Overview

Core experience:

- A 12-slot action timeline controls unit turns.
- A 12-second Chaos Wave restores energy, draws cards, and triggers random effects.
- Distance determines whether weapon and card attacks can hit.
- Weapons define range, damage, speed, and deck identity.
- Characters are differentiated by speed, hand pressure, and resource mechanics.

Important current design files:

- `混沌工坊_数值设计_v1.md`: human-readable balance and design document.
- `chaos_workshop_balance_v1.json`: script-readable balance draft.
- `混沌工坊_Unity脚本使用说明.md`: Unity script usage guide.
- `混沌工坊_敌人设计_v1.md`: first chapter enemy design draft.
- `混沌工坊_敌人脚本使用说明.md`: enemy data and AI integration guide.
- `UI贴图接口说明.md`: UI artwork binding guide.
- `混沌工坊_1.1v总结说明.md`: summary of the current 1.1v consolidation pass.

## Repository Layout

- Repository root stores the source-of-truth Markdown docs and JSON balance file.
- The actual Unity project lives under `Chaos Werkswinkel/`.
- In-editor mirror documents live under `Chaos Werkswinkel/Assets/`.
- Legacy references to `Assets/Scripts/ChaosWorkshop` are outdated. The current script root is `Chaos Werkswinkel/Assets/ChaosWorkshop`.

## Repository Structure

```text
Chaos Werkswinkel/Assets/ChaosWorkshop/Core
  Shared enums and basic types.

Chaos Werkswinkel/Assets/ChaosWorkshop/Data
  ScriptableObject-style data definitions, prototype factories, battle config, and enemy libraries.

Chaos Werkswinkel/Assets/ChaosWorkshop/Battle
  Battle flow, units, deck handling, timeline, Chaos Wave, scene installer, and enemy AI.

Chaos Werkswinkel/Assets/ChaosWorkshop/UI
  HUD, hand view, card button view, unit world display, and artwork binding hooks.

Chaos Werkswinkel/Assets/ChaosWorkshop/Demo
  Demo bootstrap for quickly creating a playable prototype scene.

Chaos Werkswinkel/Assets/ChaosWorkshop/Editor
  Asset bootstrap helpers for generated prototype data.

Chaos Werkswinkel/Assets/ChaosWorkshop/Narrative
  Story choice and run reward integration.

Chaos Werkswinkel/Assets/ChaosWorkshop/Generated
  Generated prototype assets for cards, characters, enemies, configs, and scenarios.
```

## Current Design Constants

```text
Timeline length: 12
Chaos Wave length: 12
Chaos Wave speed: 1 slot/second
Energy max: 3
Energy gain on unit action start: 1
Chaos Wave energy effect: restore all units to 3 energy
Starting hand size: 5
Hand limit: 10
Draw on player action start: 1
Draw on Chaos Wave: 3
Arena length: 20
Free move per action: 2
```

If Chaos Wave and a speed-1 unit action trigger at the same time, resolve Chaos Wave first, then the unit action. This is intentional: speed-1 heavy characters should fully benefit from Chaos Wave.

## Character Design Notes

### Lu Xiyun

- Weapon: longsword.
- Speed: 3.
- Role: balanced melee tempo.
- Core mechanic: weakness stacking and burst timing.

### Hai Luo'an

- Weapon: warhammer.
- Speed: 1.
- Role: slow heavy hitter.
- Core mechanics: overdraft, sin debt, atonement, confession.
- She draws and consumes cards slowly, so overflow discard should become a benefit rather than pure punishment.

Important Hai Luo'an rules:

```text
Overdraft max: 3
Each overdrafted energy creates 1 Sin Debt
Sin Debt does not reduce speed or directly remove energy
Each Sin Debt reduces next shield or healing gain by 2
Discarding 1 card gives 1 Confession
Confession max: 5
Next warhammer attack or shield card consumes all Confession
Each Confession gives +2 damage or +2 shield
Overflow discard after Chaos Wave also gives +1 shield per discarded card
```

### Lamu

- Weapons: dagger and hand crossbow.
- Speed: 5.
- Role: high-frequency mobility and ranged/close switching.
- Core mechanics: ammo, movement, bleed, burst after engaging from distance.

## Balance Principles

- Speed controls action frequency.
- Chaos Wave controls resource rhythm.
- Distance controls attack windows.
- Weapon templates define deck construction.
- High-speed characters should spend many low-cost cards between Chaos Waves.
- Slow characters should create heavy turns from full Chaos Wave energy and hand overflow.
- Avoid making slow-character penalties too harsh. Their slowness is already a major cost.

## Editing Guidelines

- Keep gameplay constants synchronized between:
  - `混沌工坊_数值设计_v1.md`
  - `chaos_workshop_balance_v1.json`
  - relevant Unity scripts under `Chaos Werkswinkel/Assets/ChaosWorkshop`
- Keep root docs and `Chaos Werkswinkel/Assets/` mirror docs synchronized in the same change whenever wording or version tags move.
- Prefer data-driven changes through definitions and config files before hardcoding behavior.
- When adding card effects, extend existing effect structures if possible.
- Keep prototype code readable and conservative; avoid broad refactors unless the behavior requires them.
- Treat `Chaos Werkswinkel/Assets/ChaosWorkshop/Generated/*.asset` and matching `.meta` files as source assets once they are intentionally generated for the prototype.
- Do not commit Unity generated folders such as `Library`, `Temp`, `Obj`, `Build`, or `Logs`.

## Workflow Notes Learned On 2026-05-04

- In this Codex PowerShell environment, Chinese filenames and content are safest when commands use UTF-8 explicitly. Prefer `Get-Content -Encoding UTF8` and set `[Console]::OutputEncoding = [System.Text.Encoding]::UTF8` before Git commands that print Chinese paths.
- `git` may not be on `PATH`. A working fallback path on this machine is `C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\TeamFoundation\Team Explorer\Git\cmd\git.exe`.
- `rg` may be unavailable or blocked. Fall back to `Get-ChildItem -Recurse -File` and `Select-String` instead of assuming ripgrep works.
- When summarizing or staging the repo, expect Git to show tracked deletions under the old root `Assets/Scripts/ChaosWorkshop/*` layout together with additions under `Chaos Werkswinkel/Assets/ChaosWorkshop/*`. That is the current structure migration, not necessarily accidental data loss.
- For meaningful repository uploads, stage Unity source folders such as `Chaos Werkswinkel/Assets`, `Chaos Werkswinkel/Packages`, and `Chaos Werkswinkel/ProjectSettings`, while excluding `.vs`, `Library`, `Logs`, `Temp`, `obj`, `UserSettings`, generated `.csproj`, generated `.sln`, and `.vsconfig`.
- The root README and `Chaos Werkswinkel/Assets/README.md` are mirrored overview docs. If one is updated, the other should usually be updated in the same pass.

## Validation Checklist

Before handing work back:

- Confirm JSON config parses as UTF-8.
- Check changed C# scripts for obvious compile errors.
- If Unity is available, run the demo scene through `ChaosWorkshopDemoBootstrap`.
- Verify that cards with movement can close the initial distance quickly enough.
- Verify that speed-5 characters do not run out of cards too early after Chaos Wave draw changes.
- Verify that Hai Luo'an can benefit from discard overflow without creating unlimited damage or shield loops.

## Git Notes

Remote repository:

```text
https://github.com/feizhouwuya/-.git
```

Default branch:

```text
main
```

Use clear commit messages. For design-only changes, mention the affected system, for example:

```text
Tune Chaos Wave hand economy
Adjust Hai Luo'an confession mechanics
Update initial deck balance
```
