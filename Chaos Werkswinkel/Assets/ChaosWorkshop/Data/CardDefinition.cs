using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChaosWorkshop
{
    [Serializable]
    public class CardEffect
    {
        public CardEffectType type;
        public int amount;
        public float duration;
    }

    [CreateAssetMenu(menuName = "Chaos Workshop/Card Definition", fileName = "CardDefinition")]
    public class CardDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string cardId = "card_id";
        public string displayName = "New Card";
        [TextArea(2, 6)] public string description;

        [Header("Cost and Target")]
        public int cost = 1;
        public CardTargetRule targetRule = CardTargetRule.SingleEnemy;

        [Header("Attack")]
        public int damage;
        public int rangeOverride;
        public bool useWeaponDamage = true;
        public bool useWeaponRange = true;
        public int hits = 1;
        public int ammoCost;
        public int bonusDamageIfTargetHasWeakness;
        public int bonusDamageIfMovedThisAction;

        [Header("Movement")]
        public MoveMode moveMode = MoveMode.None;
        public float moveDistance;
        public bool movementCanChooseDirection;

        [Header("Defense and Status")]
        public int shield;
        public int applyWeakness;
        public int applyBleed;
        public int gainEvasion;
        public int recoverAmmo;
        public int draw;
        public int gainEnergy;
        public int timelineChange;
        public int nextWeaponDamageBonus;

        [Header("Special Effects")]
        public List<CardEffect> effects = new List<CardEffect>();

        public int GetRange(CombatUnit user)
        {
            if (!useWeaponRange && rangeOverride > 0)
            {
                return rangeOverride;
            }

            return user != null ? user.AttackRange : rangeOverride;
        }

        public int GetDamage(CombatUnit user)
        {
            int value = useWeaponDamage && user != null ? user.BaseDamage : damage;
            int extra = useWeaponDamage && user != null ? damage : 0;
            return Mathf.Max(0, value + extra);
        }
    }
}
