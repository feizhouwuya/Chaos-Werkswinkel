using System.Collections.Generic;
using UnityEngine;

namespace ChaosWorkshop
{
    [CreateAssetMenu(menuName = "Chaos Workshop/Character Definition", fileName = "CharacterDefinition")]
    public class CharacterDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string characterId = "character_id";
        public string displayName = "Character";
        public CharacterArchetype archetype;
        public Team team = Team.Player;

        [Header("Base Stats")]
        public int maxHealth = 30;
        public int maxEnergy = 3;
        public WeaponKind startingWeapon = WeaponKind.LongSword;
        public int overdraftLimit;
        public int attackRangeOverride;
        public int baseDamageOverride;
        public float speedOverride;

        [Header("Loadout")]
        public List<WeaponKind> availableWeapons = new List<WeaponKind>();
        public List<CardDefinition> startingDeck = new List<CardDefinition>();
    }
}
