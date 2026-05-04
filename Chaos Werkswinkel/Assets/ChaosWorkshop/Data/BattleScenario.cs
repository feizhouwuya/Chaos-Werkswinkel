using UnityEngine;

namespace ChaosWorkshop
{
    [CreateAssetMenu(menuName = "Chaos Workshop/Battle Scenario", fileName = "BattleScenario")]
    public class BattleScenario : ScriptableObject
    {
        [Header("Identity")]
        public string scenarioId = "battle_scenario";
        public string displayName = "Battle Scenario";
        [TextArea(2, 6)] public string description;

        [Header("Battle Setup")]
        public BattleConfig battleConfig;
        public CharacterDefinition playerCharacter;
        public EnemyDefinition enemyDefinition;

        [Header("Optional Prefabs")]
        public CombatUnit playerPrefab;
        public CombatUnit enemyPrefab;

        [Header("Start Positions")]
        public float playerStartPosition = 4f;
        public float enemyStartPosition = 11f;
    }
}
