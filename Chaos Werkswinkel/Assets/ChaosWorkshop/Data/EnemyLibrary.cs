using System.Collections.Generic;
using UnityEngine;

namespace ChaosWorkshop
{
    public static class EnemyLibrary
    {
        public static EnemyDefinition CreateDefinition(string enemyId)
        {
            EnemyDefinition definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            definition.enemyId = enemyId;

            switch (enemyId)
            {
                case "rustblade_hunter":
                    Fill(definition, "锈刃追猎者", EnemyTier.Normal, 24, 4f, 1, 4, 7f);
                    AddAction(definition, "ground_dash", "Ground Dash", EnemyActionKind.MoveToward, moveToward: 2f, intent: "Move toward the player.");
                    AddAction(definition, "rustblade_flurry", "Rustblade Flurry", EnemyActionKind.Attack, damage: 4, hits: 2, range: 1, intent: "Deal 4 damage twice.");
                    break;
                case "steam_guard":
                    Fill(definition, "蒸汽盾卫", EnemyTier.Normal, 42, 1f, 2, 11, 8f);
                    AddAction(definition, "shield_advance", "Shield Advance", EnemyActionKind.Shield, shield: 8, moveToward: 2f, intent: "Gain shield and advance.");
                    AddAction(definition, "steam_bash", "Steam Bash", EnemyActionKind.Attack, damage: 11, range: 2, intent: "Deal 11 damage.");
                    break;
                case "gun_wanderer":
                    Fill(definition, "枪械游民", EnemyTier.Normal, 26, 2f, 9, 7, 10f);
                    AddAction(definition, "rough_shot", "Rough Shot", EnemyActionKind.Attack, damage: 7, range: 9, intent: "Shoot from range.");
                    AddAction(definition, "panic_step", "Panic Step", EnemyActionKind.MoveAway, moveAway: 3f, intent: "Retreat, empowering the next shot.");
                    break;
                case "chaos_apprentice":
                    Fill(definition, "混沌学徒", EnemyTier.Normal, 34, 3f, 3, 7, 8f);
                    AddAction(definition, "twist_order", "Twist Order", EnemyActionKind.TimelineControl, timelineChangePlayer: -2f, intent: "Delay the player timeline.");
                    AddAction(definition, "chaos_short_hit", "Chaos Short Hit", EnemyActionKind.Attack, damage: 7, range: 3, intent: "Deal 7 damage.");
                    break;
                case "broken_axle_knight":
                    Fill(definition, "断轴骑士", EnemyTier.Elite, 85, 2f, 3, 12, 8f);
                    AddAction(definition, "axle_step", "Axle Step", EnemyActionKind.Shield, shield: 6, moveToward: 2f, intent: "Advance with shield.");
                    AddAction(definition, "knight_sweep", "Knight Sweep", EnemyActionKind.Attack, damage: 12, range: 3, intent: "Deal 12 damage.");
                    AddAction(definition, "split_axle_lunge", "Split Axle Lunge", EnemyActionKind.Special, damage: 16, range: 5, moveToward: 2f, intent: "Spend power for a heavy lunge.");
                    break;
                case "redline_sniper":
                    Fill(definition, "红线狙击手", EnemyTier.Elite, 70, 3f, 10, 9, 10f);
                    AddAction(definition, "redline_aim", "Redline Aim", EnemyActionKind.Mark, range: 10, intent: "Mark the player.");
                    AddAction(definition, "line_piercing_shot", "Line Piercing Shot", EnemyActionKind.Attack, damage: 14, range: 10, intent: "Deal 14 damage to a marked target.");
                    AddAction(definition, "hip_shot", "Hip Shot", EnemyActionKind.Attack, damage: 8, range: 5, intent: "Deal 8 damage.");
                    AddAction(definition, "vault_away", "Vault Away", EnemyActionKind.MoveAway, moveAway: 4f, intent: "Retreat and gain evasion.");
                    break;
                case "furnace_steward":
                    Fill(definition, "熔炉执事", EnemyTier.Elite, 100, 1f, 3, 14, 9f);
                    AddAction(definition, "furnace_gate", "Furnace Gate", EnemyActionKind.Shield, shield: 14, intent: "Gain shield on chaos wave.");
                    AddAction(definition, "steward_hammer", "Steward Hammer", EnemyActionKind.Attack, damage: 14, range: 3, intent: "Deal 14 damage.");
                    AddAction(definition, "coal_push", "Coal Push", EnemyActionKind.Shield, shield: 6, moveToward: 2f, intent: "Advance with shield.");
                    break;
                case "first_chaos_core":
                    Fill(definition, "初代混沌炉心", EnemyTier.Boss, 220, 2f, 3, 12, 9f);
                    AddAction(definition, "furnace_slap", "Furnace Slap", EnemyActionKind.Attack, damage: 12, range: 3, intent: "Phase 1 strike.");
                    AddAction(definition, "magnetic_chain", "Magnetic Chain", EnemyActionKind.Pull, damage: 11, range: 3, pull: 2f, intent: "Phase 2 strike and pull.");
                    AddAction(definition, "burning_bash", "Burning Bash", EnemyActionKind.Attack, damage: 13, range: 3, intent: "Phase 3 strike.");
                    break;
                default:
                    Fill(definition, "失控傀儡", EnemyTier.Normal, 28, 2f, 2, 6, 7f);
                    AddAction(definition, "shamble_close", "Shamble Close", EnemyActionKind.MoveToward, moveToward: 2f, intent: "Move toward the player.");
                    AddAction(definition, "damaged_swing", "Damaged Swing", EnemyActionKind.Attack, damage: 6, range: 2, intent: "Deal 6 damage.");
                    break;
            }

            return definition;
        }

        public static void ApplyDefinition(CombatUnit unit, EnemyDefinition definition)
        {
            unit.displayName = definition.displayName;
            unit.team = Team.Enemy;
            unit.archetype = CharacterArchetype.Enemy;
            unit.maxHealth = definition.maxHealth;
            unit.maxEnergy = 3;
            unit.weaponKind = WeaponKind.BattleAxe;
            unit.attackRangeOverride = definition.attackRange;
            unit.baseDamageOverride = definition.baseDamage;
            unit.speedOverride = definition.speed;
        }

        private static void Fill(EnemyDefinition definition, string name, EnemyTier tier, int hp, float speed, int range, int damage, float initialDistance)
        {
            definition.displayName = name;
            definition.tier = tier;
            definition.maxHealth = hp;
            definition.speed = speed;
            definition.attackRange = range;
            definition.baseDamage = damage;
            definition.initialDistance = initialDistance;
        }

        private static void AddAction(
            EnemyDefinition definition,
            string id,
            string name,
            EnemyActionKind kind,
            int damage = 0,
            int hits = 1,
            int range = 0,
            float moveToward = 0f,
            float moveAway = 0f,
            int shield = 0,
            float timelineChangeSelf = 0f,
            float timelineChangePlayer = 0f,
            float pull = 0f,
            float knockback = 0f,
            string intent = "")
        {
            definition.actions.Add(new EnemyActionDefinition
            {
                id = id,
                displayName = name,
                kind = kind,
                damage = damage,
                hits = hits,
                range = range,
                moveToward = moveToward,
                moveAway = moveAway,
                shield = shield,
                timelineChangeSelf = timelineChangeSelf,
                timelineChangePlayer = timelineChangePlayer,
                pull = pull,
                knockback = knockback,
                intentText = intent
            });
        }
    }
}
