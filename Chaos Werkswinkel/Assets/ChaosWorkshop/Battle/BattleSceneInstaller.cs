using System.Collections.Generic;
using UnityEngine;

namespace ChaosWorkshop
{
    public class BattleSceneInstaller : MonoBehaviour
    {
        [Header("Scenario")]
        public bool startScenarioOnStart = true;
        public BattleScenario scenario;
        public BattleConfig battleConfigOverride;

        [Header("Scene References")]
        public BattleManager battleManager;
        public BattleHud battleHud;
        public HandView handView;

        [Header("Unit References")]
        public CombatUnit playerUnit;
        public CombatUnit enemyUnit;
        public Transform playerSpawnParent;
        public Transform enemySpawnParent;

        private void Reset()
        {
            battleManager = GetComponent<BattleManager>();
        }

        private void Start()
        {
            if (startScenarioOnStart)
            {
                InstallAndStartScenario();
            }
        }

        [ContextMenu("Install And Start Scenario")]
        public void InstallAndStartScenario()
        {
            if (scenario == null)
            {
                Debug.LogWarning("BattleSceneInstaller requires a BattleScenario.");
                return;
            }

            ResolveSceneReferences();
            if (battleManager == null)
            {
                Debug.LogWarning("BattleSceneInstaller could not find a BattleManager.");
                return;
            }

            BattleConfig config = battleConfigOverride != null ? battleConfigOverride : scenario.battleConfig;
            battleManager.ApplyConfig(config);
            battleManager.playerStartPosition = scenario.playerStartPosition;
            battleManager.enemyStartPosition = scenario.enemyStartPosition;

            playerUnit = ResolveUnit(playerUnit, scenario.playerPrefab, playerSpawnParent);
            enemyUnit = ResolveUnit(enemyUnit, scenario.enemyPrefab, enemySpawnParent);
            if (playerUnit == null || enemyUnit == null)
            {
                Debug.LogWarning("BattleSceneInstaller needs player and enemy CombatUnit references or prefabs.");
                return;
            }

            if (scenario.playerCharacter == null)
            {
                Debug.LogWarning("BattleSceneInstaller requires a player CharacterDefinition.");
                return;
            }

            if (scenario.enemyDefinition == null)
            {
                Debug.LogWarning("BattleSceneInstaller requires an enemy EnemyDefinition.");
                return;
            }

            playerUnit.Initialize(scenario.playerCharacter, scenario.playerStartPosition);
            enemyUnit.Initialize(scenario.enemyDefinition, scenario.enemyStartPosition);

            EnemyAI enemyAI = enemyUnit.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.battleManager = battleManager;
                enemyAI.Configure(scenario.enemyDefinition);
            }

            BindUnitView(playerUnit);
            BindUnitView(enemyUnit);
            BindUi();

            IReadOnlyList<CardDefinition> startingDeck = scenario.playerCharacter.startingDeck;
            battleManager.StartBattle(playerUnit, enemyUnit, startingDeck ?? new List<CardDefinition>());
        }

        private void ResolveSceneReferences()
        {
            if (battleManager == null)
            {
                battleManager = GetComponent<BattleManager>();
            }

            if (battleManager == null)
            {
                battleManager = FindFirstObjectByType<BattleManager>();
            }

            if (battleHud == null)
            {
                battleHud = FindFirstObjectByType<BattleHud>();
            }

            if (handView == null)
            {
                handView = FindFirstObjectByType<HandView>();
            }
        }

        private CombatUnit ResolveUnit(CombatUnit existingUnit, CombatUnit prefab, Transform parent)
        {
            if (existingUnit != null)
            {
                return existingUnit;
            }

            if (prefab == null)
            {
                return null;
            }

            CombatUnit instance = parent != null ? Instantiate(prefab, parent) : Instantiate(prefab);
            instance.name = prefab.name;
            return instance;
        }

        private void BindUi()
        {
            if (battleHud != null)
            {
                battleHud.battleManager = battleManager;
            }

            if (handView != null)
            {
                handView.battleManager = battleManager;
            }
        }

        private void BindUnitView(CombatUnit unit)
        {
            if (unit == null)
            {
                return;
            }

            UnitWorldView[] views = unit.GetComponentsInChildren<UnitWorldView>(true);
            for (int i = 0; i < views.Length; i++)
            {
                views[i].unit = unit;
                views[i].battleManager = battleManager;
                views[i].ApplyArtwork();
            }
        }
    }
}
