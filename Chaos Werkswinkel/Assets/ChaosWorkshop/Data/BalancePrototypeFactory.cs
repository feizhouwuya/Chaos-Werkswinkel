using System.Collections.Generic;
using UnityEngine;

namespace ChaosWorkshop
{
    public static class BalancePrototypeFactory
    {
        public static List<CardDefinition> CreateStartingDeck(CharacterArchetype archetype)
        {
            List<CardDefinition> deck = new List<CardDefinition>();
            switch (archetype)
            {
                case CharacterArchetype.Swordsman:
                    Add(deck, "slash", 2);
                    Add(deck, "swift_step_slash", 3);
                    Add(deck, "acupoint_strike", 1);
                    Add(deck, "retreating_step", 2);
                    Add(deck, "sword_momentum", 1);
                    Add(deck, "gap_breaker", 1);
                    break;
                case CharacterArchetype.Nun:
                    Add(deck, "hammer_blow", 2);
                    Add(deck, "conviction_charge", 2);
                    Add(deck, "guarded_advance", 2);
                    Add(deck, "shock_hammer", 1);
                    Add(deck, "divine_judgement_hammer", 1);
                    Add(deck, "sentence_declaration", 1);
                    Add(deck, "atonement_prayer", 1);
                    Add(deck, "penitent_bulwark", 1);
                    Add(deck, "silent_confession", 1);
                    Add(deck, "execution_preparation", 1);
                    break;
                case CharacterArchetype.Assassin:
                    Add(deck, "stab", 2);
                    Add(deck, "sleeve_bolt", 3);
                    Add(deck, "roll", 2);
                    Add(deck, "shadow_step", 2);
                    Add(deck, "backstab", 1);
                    Add(deck, "reload", 1);
                    Add(deck, "lacerate", 1);
                    break;
            }

            return deck;
        }

        public static CombatUnit CreateRuntimeUnit(GameObject host, string unitId, Team team)
        {
            CombatUnit unit = host.GetComponent<CombatUnit>();
            if (unit == null)
            {
                unit = host.AddComponent<CombatUnit>();
            }

            unit.team = team;
            unit.maxEnergy = 3;
            unit.overdraftLimit = 0;
            unit.attackRangeOverride = 0;
            unit.baseDamageOverride = 0;
            unit.speedOverride = 0f;

            switch (unitId)
            {
                case "lu_xiyun":
                    unit.displayName = "陆夕云";
                    unit.archetype = CharacterArchetype.Swordsman;
                    unit.maxHealth = 48;
                    unit.weaponKind = WeaponKind.LongSword;
                    break;
                case "hai_luoan":
                    unit.displayName = "海洛安";
                    unit.archetype = CharacterArchetype.Nun;
                    unit.maxHealth = 60;
                    unit.weaponKind = WeaponKind.WarHammer;
                    unit.overdraftLimit = 3;
                    break;
                case "lamu":
                    unit.displayName = "拉缪";
                    unit.archetype = CharacterArchetype.Assassin;
                    unit.maxHealth = 42;
                    unit.weaponKind = WeaponKind.Dagger;
                    break;
                case "rustblade_hunter":
                    ApplyEnemy(host, unit, unitId);
                    break;
                case "steam_guard":
                    ApplyEnemy(host, unit, unitId);
                    break;
                case "gun_wanderer":
                    ApplyEnemy(host, unit, unitId);
                    break;
                case "chaos_apprentice":
                    ApplyEnemy(host, unit, unitId);
                    break;
                case "broken_axle_knight":
                    ApplyEnemy(host, unit, unitId);
                    break;
                case "redline_sniper":
                    ApplyEnemy(host, unit, unitId);
                    break;
                case "furnace_steward":
                    ApplyEnemy(host, unit, unitId);
                    break;
                case "first_chaos_core":
                    ApplyEnemy(host, unit, unitId);
                    break;
                default:
                    ApplyEnemy(host, unit, "rogue_puppet");
                    break;
            }

            return unit;
        }

        private static void ApplyEnemy(GameObject host, CombatUnit unit, string enemyId)
        {
            EnemyDefinition definition = EnemyLibrary.CreateDefinition(enemyId);
            EnemyLibrary.ApplyDefinition(unit, definition);
            EnemyAI ai = host.GetComponent<EnemyAI>();
            if (ai == null)
            {
                ai = host.AddComponent<EnemyAI>();
            }

            ai.Configure(enemyId);
        }

        public static CardDefinition CreateCard(string id)
        {
            switch (id)
            {
                case "slash":
                    return Attack(id, "平斩", 1, 2, 5);
                case "swift_step_slash":
                    return Attack(id, "疾步斩", 1, 2, 4, card =>
                    {
                        card.moveMode = MoveMode.TowardTarget;
                        card.moveDistance = 3f;
                    });
                case "acupoint_strike":
                    return Attack(id, "点穴", 1, 2, 3, card => card.applyWeakness = 1);
                case "retreating_step":
                    return Self(id, "回身步", 0, card =>
                    {
                        card.moveMode = MoveMode.AwayFromTarget;
                        card.moveDistance = 2f;
                        card.gainEvasion = 1;
                    });
                case "sword_momentum":
                    return Self(id, "剑势", 1, card => card.nextWeaponDamageBonus = 4);
                case "gap_breaker":
                    return Attack(id, "破隙", 2, 2, 8, card => card.bonusDamageIfTargetHasWeakness = 6);
                case "hammer_blow":
                    return Attack(id, "重击", 2, 3, 17);
                case "conviction_charge":
                    return Attack(id, "断罪冲锋", 3, 3, 20, card =>
                    {
                        card.moveMode = MoveMode.TowardTarget;
                        card.moveDistance = 4f;
                    });
                case "guarded_advance":
                    return Self(id, "格挡推进", 2, card =>
                    {
                        card.shield = 12;
                        card.moveMode = MoveMode.TowardTarget;
                        card.moveDistance = 2f;
                    });
                case "shock_hammer":
                    return Attack(id, "震荡锤", 3, 3, 25, card => card.effects.Add(new CardEffect { type = CardEffectType.Move, amount = 2 }));
                case "divine_judgement_hammer":
                    return Attack(id, "神罚落锤", 4, 3, 36);
                case "sentence_declaration":
                    return Self(id, "罪罚宣告", 1, card => card.draw = 1);
                case "atonement_prayer":
                    return Self(id, "赎罪祷言", 1, card => card.draw = 1);
                case "penitent_bulwark":
                    return Self(id, "苦修壁垒", 0, card => card.shield = 10);
                case "silent_confession":
                    return Self(id, "沉默忏悔", 0, card => card.draw = 1);
                case "execution_preparation":
                    return Self(id, "执刑准备", 1, card => card.gainEnergy = 1);
                case "stab":
                    return Attack(id, "刺击", 1, 1, 3, card => card.hits = 2);
                case "sleeve_bolt":
                    return Attack(id, "袖弩", 1, 10, 5, card => card.ammoCost = 1);
                case "roll":
                    return Self(id, "翻滚", 0, card =>
                    {
                        card.moveMode = MoveMode.AwayFromTarget;
                        card.moveDistance = 3f;
                        card.movementCanChooseDirection = true;
                    });
                case "shadow_step":
                    return Self(id, "影步", 1, card =>
                    {
                        card.moveMode = MoveMode.TowardTarget;
                        card.moveDistance = 4f;
                        card.gainEvasion = 1;
                    });
                case "backstab":
                    return Attack(id, "背刺", 2, 1, 10, card => card.bonusDamageIfMovedThisAction = 5);
                case "reload":
                    return Self(id, "装填", 1, card =>
                    {
                        card.recoverAmmo = 2;
                        card.draw = 1;
                    });
                case "lacerate":
                    return Attack(id, "割裂", 1, 1, 3, card => card.applyBleed = 3);
                default:
                    return Attack("slash", "平斩", 1, 2, 5);
            }
        }

        private static void Add(List<CardDefinition> deck, string id, int count)
        {
            for (int i = 0; i < count; i++)
            {
                deck.Add(CreateCard(id));
            }
        }

        private static CardDefinition Attack(string id, string name, int cost, int range, int damage, System.Action<CardDefinition> configure = null)
        {
            CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>();
            card.cardId = id;
            card.displayName = name;
            card.description = name;
            card.cost = cost;
            card.targetRule = CardTargetRule.SingleEnemy;
            card.useWeaponDamage = false;
            card.useWeaponRange = false;
            card.rangeOverride = range;
            card.damage = damage;
            configure?.Invoke(card);
            return card;
        }

        private static CardDefinition Self(string id, string name, int cost, System.Action<CardDefinition> configure)
        {
            CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>();
            card.cardId = id;
            card.displayName = name;
            card.description = name;
            card.cost = cost;
            card.targetRule = CardTargetRule.Self;
            card.useWeaponDamage = false;
            card.useWeaponRange = false;
            configure?.Invoke(card);
            return card;
        }
    }
}
