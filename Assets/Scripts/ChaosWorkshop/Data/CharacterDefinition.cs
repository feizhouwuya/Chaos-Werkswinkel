using System.Collections.Generic;
using UnityEngine;

namespace ChaosWorkshop
{
    [CreateAssetMenu(menuName = "Chaos Workshop/Character Definition", fileName = "CharacterDefinition")]
    public class CharacterDefinition : ScriptableObject
    {
        public string characterId = "character_id";
        public string displayName = "Character";
        public CharacterArchetype archetype;
        public Team team;
        public int maxHealth = 30;
        public int maxEnergy = 3;
        public WeaponKind startingWeapon = WeaponKind.LongSword;
        public List<CardDefinition> startingDeck = new List<CardDefinition>();
    }
}
