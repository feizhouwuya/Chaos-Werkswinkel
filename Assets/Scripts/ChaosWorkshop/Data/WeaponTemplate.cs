using System;
using UnityEngine;

namespace ChaosWorkshop
{
    [Serializable]
    public class WeaponTemplate
    {
        public WeaponKind kind;
        public string displayName;
        public int attackRange;
        public int baseDamage;
        public float initialSpeed;
        public int rangedAmmo;

        public WeaponTemplate(WeaponKind kind, string displayName, int attackRange, int baseDamage, float initialSpeed, int rangedAmmo = 0)
        {
            this.kind = kind;
            this.displayName = displayName;
            this.attackRange = attackRange;
            this.baseDamage = baseDamage;
            this.initialSpeed = initialSpeed;
            this.rangedAmmo = rangedAmmo;
        }
    }

    public static class WeaponLibrary
    {
        public static WeaponTemplate Create(WeaponKind kind)
        {
            switch (kind)
            {
                case WeaponKind.LongSword:
                    return new WeaponTemplate(kind, "Long Sword", 2, 4, 3f);
                case WeaponKind.WarHammer:
                    return new WeaponTemplate(kind, "War Hammer", 3, 10, 1f);
                case WeaponKind.Dagger:
                    return new WeaponTemplate(kind, "Dagger", 1, 2, 5f);
                case WeaponKind.HandCrossbow:
                    return new WeaponTemplate(kind, "Hand Crossbow", 10, 4, 5f, 3);
                case WeaponKind.Spear:
                    return new WeaponTemplate(kind, "Spear", 5, 5, 2f);
                case WeaponKind.Tachi:
                    return new WeaponTemplate(kind, "Tachi", 2, 10, 1f);
                case WeaponKind.BattleAxe:
                    return new WeaponTemplate(kind, "Battle Axe", 3, 5, 2f);
                default:
                    Debug.LogWarning("Unknown weapon kind. Falling back to Long Sword.");
                    return Create(WeaponKind.LongSword);
            }
        }
    }
}
