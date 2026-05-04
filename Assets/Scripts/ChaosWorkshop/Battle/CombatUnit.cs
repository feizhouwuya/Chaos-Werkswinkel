using System;
using UnityEngine;

namespace ChaosWorkshop
{
    public class CombatUnit : MonoBehaviour
    {
        [Header("Identity")]
        public string displayName = "Unit";
        public Team team;
        public CharacterArchetype archetype;

        [Header("Stats")]
        public int maxHealth = 30;
        public int maxEnergy = 3;
        public WeaponKind weaponKind = WeaponKind.LongSword;

        [Header("Battle Runtime")]
        [SerializeField] private int health;
        [SerializeField] private int energy;
        [SerializeField] private float timelineProgress;
        [SerializeField] private float speed;
        [SerializeField] private float arenaPosition;
        [SerializeField] private int weaknessStacks;
        [SerializeField] private int rangedAmmo;

        public event Action<CombatUnit> Changed;
        public event Action<CombatUnit> Died;

        public WeaponTemplate Weapon { get; private set; }
        public bool IsAlive => health > 0;
        public int Health => health;
        public int Energy => energy;
        public int MaxEnergy => maxEnergy;
        public float TimelineProgress => timelineProgress;
        public float Speed => speed;
        public float ArenaPosition => arenaPosition;
        public int WeaknessStacks => weaknessStacks;
        public int AttackRange => Weapon != null ? Weapon.attackRange : 1;
        public int BaseDamage => Weapon != null ? Weapon.baseDamage : 1;
        public int RangedAmmo => rangedAmmo;

        public void Initialize(CharacterDefinition definition, float startPosition)
        {
            displayName = definition.displayName;
            team = definition.team;
            archetype = definition.archetype;
            maxHealth = definition.maxHealth;
            maxEnergy = definition.maxEnergy;
            weaponKind = definition.startingWeapon;
            InitializeRuntime(startPosition);
        }

        public void InitializeRuntime(float startPosition)
        {
            Weapon = WeaponLibrary.Create(weaponKind);
            health = maxHealth;
            energy = maxEnergy;
            speed = Weapon.initialSpeed;
            rangedAmmo = Weapon.rangedAmmo;
            timelineProgress = 0f;
            arenaPosition = startPosition;
            weaknessStacks = 0;
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

            if (energy < amount)
            {
                return false;
            }

            energy -= amount;
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
            int finalDamage = Mathf.Max(0, amount + weaknessStacks);
            weaknessStacks = 0;
            health = Mathf.Max(0, health - finalDamage);
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
            weaknessStacks = Mathf.Max(0, weaknessStacks + stacks);
            NotifyChanged();
        }

        public bool ConsumeAmmo()
        {
            if (rangedAmmo <= 0)
            {
                return false;
            }

            rangedAmmo--;
            NotifyChanged();
            return true;
        }

        private void NotifyChanged()
        {
            Changed?.Invoke(this);
        }
    }
}
