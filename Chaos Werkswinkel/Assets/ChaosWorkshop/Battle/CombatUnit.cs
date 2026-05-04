using System;
using UnityEngine;

namespace ChaosWorkshop
{
    public class CombatUnit : MonoBehaviour
    {
        [Header("Identity")]
        public string displayName = "单位";
        public Team team;
        public CharacterArchetype archetype;

        [Header("Stats")]
        public int maxHealth = 30;
        public int maxEnergy = 3;
        public WeaponKind weaponKind = WeaponKind.LongSword;
        public int overdraftLimit;
        public int attackRangeOverride;
        public int baseDamageOverride;
        public float speedOverride;

        [Header("Battle Runtime")]
        [SerializeField] private int health;
        [SerializeField] private int energy;
        [SerializeField] private int shield;
        [SerializeField] private float timelineProgress;
        [SerializeField] private float speed;
        [SerializeField] private float arenaPosition;
        [SerializeField] private int weaknessStacks;
        [SerializeField] private int bleedStacks;
        [SerializeField] private int evasionStacks;
        [SerializeField] private int sinDebt;
        [SerializeField] private int nextWeaponDamageBonus;
        [SerializeField] private int rangedAmmo;
        [SerializeField] private bool movedThisAction;

        public event Action<CombatUnit> Changed;
        public event Action<CombatUnit> Died;
        public event Action<CombatUnit, int> Damaged;

        public WeaponTemplate Weapon { get; private set; }
        public bool IsAlive => health > 0;
        public int Health => health;
        public int Energy => energy;
        public int MaxEnergy => maxEnergy;
        public int Shield => shield;
        public float TimelineProgress => timelineProgress;
        public float Speed => speed;
        public float ArenaPosition => arenaPosition;
        public int WeaknessStacks => weaknessStacks;
        public int BleedStacks => bleedStacks;
        public int EvasionStacks => evasionStacks;
        public int SinDebt => sinDebt;
        public bool MovedThisAction => movedThisAction;
        public int AttackRange => attackRangeOverride > 0 ? attackRangeOverride : Weapon != null ? Weapon.attackRange : 1;
        public int BaseDamage => baseDamageOverride > 0 ? baseDamageOverride + nextWeaponDamageBonus : Weapon != null ? Weapon.baseDamage + nextWeaponDamageBonus : 1 + nextWeaponDamageBonus;
        public int RangedAmmo => rangedAmmo;

        public bool CanSpendEnergy(int amount)
        {
            return amount <= 0 || energy + overdraftLimit >= amount;
        }

        public void Initialize(CharacterDefinition definition, float startPosition)
        {
            if (definition == null)
            {
                Debug.LogWarning("CharacterDefinition is missing.");
                return;
            }

            displayName = definition.displayName;
            team = definition.team;
            archetype = definition.archetype;
            maxHealth = definition.maxHealth;
            maxEnergy = definition.maxEnergy;
            weaponKind = definition.startingWeapon;
            overdraftLimit = definition.overdraftLimit;
            attackRangeOverride = definition.attackRangeOverride;
            baseDamageOverride = definition.baseDamageOverride;
            speedOverride = definition.speedOverride;
            InitializeRuntime(startPosition);
        }

        public void Initialize(EnemyDefinition definition, float startPosition)
        {
            if (definition == null)
            {
                Debug.LogWarning("EnemyDefinition is missing.");
                return;
            }

            displayName = definition.displayName;
            team = Team.Enemy;
            archetype = CharacterArchetype.Enemy;
            maxHealth = definition.maxHealth;
            maxEnergy = 3;
            weaponKind = WeaponKind.BattleAxe;
            overdraftLimit = 0;
            attackRangeOverride = definition.attackRange;
            baseDamageOverride = definition.baseDamage;
            speedOverride = definition.speed;
            InitializeRuntime(startPosition);
        }

        public void InitializeRuntime(float startPosition)
        {
            Weapon = WeaponLibrary.Create(weaponKind);
            health = maxHealth;
            energy = maxEnergy;
            shield = 0;
            speed = speedOverride > 0f ? speedOverride : Weapon.initialSpeed;
            rangedAmmo = Weapon.rangedAmmo;
            timelineProgress = 0f;
            arenaPosition = startPosition;
            weaknessStacks = 0;
            bleedStacks = 0;
            evasionStacks = 0;
            sinDebt = 0;
            nextWeaponDamageBonus = 0;
            movedThisAction = false;
            NotifyChanged();
        }

        public void TickTimeline(float deltaTime)
        {
            if (!IsAlive)
            {
                return;
            }

            timelineProgress += Mathf.Max(0f, speed) * deltaTime;
            NotifyChanged();
        }

        public bool IsReady(float timelineLength)
        {
            return IsAlive && timelineProgress >= timelineLength;
        }

        public void ConsumeReady(float timelineLength)
        {
            timelineProgress = Mathf.Max(0f, timelineProgress - timelineLength);
            movedThisAction = false;
            TriggerBleed();
            NotifyChanged();
        }

        public void RestoreEnergy()
        {
            energy = maxEnergy;
            NotifyChanged();
        }

        public bool SpendEnergy(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (energy + overdraftLimit < amount)
            {
                return false;
            }

            energy -= amount;
            if (energy < 0)
            {
                sinDebt += -energy;
            }

            NotifyChanged();
            return true;
        }

        public void GainEnergy(int amount)
        {
            energy = Mathf.Clamp(energy + amount, 0, maxEnergy);
            NotifyChanged();
        }

        public void TakeDamage(int amount)
        {
            int finalDamage = Mathf.Max(0, amount);
            if (evasionStacks > 0)
            {
                finalDamage = Mathf.Max(0, finalDamage - 999);
                evasionStacks--;
            }

            if (weaknessStacks > 0)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * (1f + weaknessStacks * 0.25f));
            }

            weaknessStacks = 0;
            if (shield > 0)
            {
                int blocked = Mathf.Min(shield, finalDamage);
                shield -= blocked;
                finalDamage -= blocked;
            }

            nextWeaponDamageBonus = 0;
            health = Mathf.Max(0, health - finalDamage);
            if (finalDamage > 0)
            {
                Damaged?.Invoke(this, finalDamage);
            }
            NotifyChanged();

            if (health <= 0)
            {
                Died?.Invoke(this);
            }
        }

        public void Heal(int amount)
        {
            health = Mathf.Clamp(health + amount, 0, maxHealth);
            NotifyChanged();
        }

        public void MoveBy(float amount, float arenaMin, float arenaMax)
        {
            if (!Mathf.Approximately(amount, 0f))
            {
                movedThisAction = true;
            }

            arenaPosition = Mathf.Clamp(arenaPosition + amount, arenaMin, arenaMax);
            NotifyChanged();
        }

        public void SetPosition(float value, float arenaMin, float arenaMax)
        {
            arenaPosition = Mathf.Clamp(value, arenaMin, arenaMax);
            NotifyChanged();
        }

        public void ChangeSpeed(float amount)
        {
            speed = Mathf.Max(0.1f, speed + amount);
            NotifyChanged();
        }

        public void ChangeTimeline(float amount, float timelineLength)
        {
            timelineProgress = Mathf.Clamp(timelineProgress + amount, 0f, timelineLength);
            NotifyChanged();
        }

        public void ApplyWeakness(int stacks)
        {
            weaknessStacks = Mathf.Clamp(weaknessStacks + stacks, 0, 3);
            NotifyChanged();
        }

        public void ApplyBleed(int stacks)
        {
            bleedStacks = Mathf.Max(0, bleedStacks + stacks);
            NotifyChanged();
        }

        public void GainShield(int amount)
        {
            int debtPenalty = Mathf.Max(0, sinDebt * 2);
            shield = Mathf.Max(0, shield + amount - debtPenalty);
            NotifyChanged();
        }

        public void GainEvasion(int stacks)
        {
            evasionStacks = Mathf.Max(0, evasionStacks + stacks);
            NotifyChanged();
        }

        public void AddNextWeaponDamageBonus(int amount)
        {
            nextWeaponDamageBonus += amount;
            NotifyChanged();
        }

        public bool ConsumeAmmo(int amount = 1)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (rangedAmmo < amount)
            {
                return false;
            }

            rangedAmmo -= amount;
            NotifyChanged();
            return true;
        }

        public void RecoverAmmo(int amount)
        {
            int maxAmmo = Weapon != null ? Weapon.rangedAmmo : 0;
            rangedAmmo = Mathf.Clamp(rangedAmmo + amount, 0, maxAmmo);
            NotifyChanged();
        }

        private void TriggerBleed()
        {
            if (bleedStacks <= 0)
            {
                return;
            }

            int bleedDamage = bleedStacks;
            bleedStacks = Mathf.Max(0, bleedStacks - 1);
            TakeDamage(bleedDamage);
        }

        private void NotifyChanged()
        {
            Changed?.Invoke(this);
        }
    }
}
