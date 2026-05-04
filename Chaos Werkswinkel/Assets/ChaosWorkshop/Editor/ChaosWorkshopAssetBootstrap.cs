using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ChaosWorkshop.Editor
{
    public static class ChaosWorkshopAssetBootstrap
    {
        private const string RootFolder = "Assets/ChaosWorkshop/Generated";
        private const string CardFolder = RootFolder + "/Cards";
        private const string CharacterFolder = RootFolder + "/Characters";
        private const string EnemyFolder = RootFolder + "/Enemies";
        private const string ConfigFolder = RootFolder + "/Configs";
        private const string ScenarioFolder = RootFolder + "/Scenarios";

        [MenuItem("Chaos Workshop/Generate Prototype Assets")]
        public static void GeneratePrototypeAssets()
        {
            EnsureFolders();

            Dictionary<string, CardDefinition> cards = GenerateCards();
            GenerateCharacters(cards);
            GenerateEnemies();
            GenerateBattleConfig();
            GenerateBattleScenario();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Chaos Workshop", "Prototype assets generated under Assets/ChaosWorkshop/Generated.", "OK");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "ChaosWorkshop");
            EnsureFolder("Assets/ChaosWorkshop", "Generated");
            EnsureFolder(RootFolder, "Cards");
            EnsureFolder(RootFolder, "Characters");
            EnsureFolder(RootFolder, "Enemies");
            EnsureFolder(RootFolder, "Configs");
            EnsureFolder(RootFolder, "Scenarios");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string combined = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(combined))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static Dictionary<string, CardDefinition> GenerateCards()
        {
            string[] cardIds =
            {
                "slash",
                "swift_step_slash",
                "acupoint_strike",
                "retreating_step",
                "sword_momentum",
                "gap_breaker",
                "hammer_blow",
                "conviction_charge",
                "guarded_advance",
                "shock_hammer",
                "divine_judgement_hammer",
                "sentence_declaration",
                "atonement_prayer",
                "penitent_bulwark",
                "silent_confession",
                "execution_preparation",
                "stab",
                "sleeve_bolt",
                "roll",
                "shadow_step",
                "backstab",
                "reload",
                "lacerate"
            };

            Dictionary<string, CardDefinition> result = new Dictionary<string, CardDefinition>();
            for (int i = 0; i < cardIds.Length; i++)
            {
                string cardId = cardIds[i];
                string path = CardFolder + "/" + cardId + ".asset";
                CardDefinition asset = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
                CardDefinition prototype = BalancePrototypeFactory.CreateCard(cardId);
                if (asset == null)
                {
                    asset = Object.Instantiate(prototype);
                    AssetDatabase.CreateAsset(asset, path);
                }
                else
                {
                    EditorUtility.CopySerializedManagedFieldsOnly(prototype, asset);
                    EditorUtility.SetDirty(asset);
                }

                result[cardId] = asset;
                if (prototype != null)
                {
                    Object.DestroyImmediate(prototype);
                }
            }

            return result;
        }

        private static void GenerateCharacters(Dictionary<string, CardDefinition> cards)
        {
            CreateOrUpdateCharacter(
                "lu_xiyun",
                "lu_xiyun.asset",
                "Lu Xiyun",
                CharacterArchetype.Swordsman,
                48,
                WeaponKind.LongSword,
                BuildDeck(cards, CharacterArchetype.Swordsman));

            CreateOrUpdateCharacter(
                "hai_luoan",
                "hai_luoan.asset",
                "Hai Luoan",
                CharacterArchetype.Nun,
                60,
                WeaponKind.WarHammer,
                BuildDeck(cards, CharacterArchetype.Nun),
                overdraftLimit: 3);

            CreateOrUpdateCharacter(
                "lamu",
                "lamu.asset",
                "Lamu",
                CharacterArchetype.Assassin,
                42,
                WeaponKind.Dagger,
                BuildDeck(cards, CharacterArchetype.Assassin));
        }

        private static void CreateOrUpdateCharacter(
            string characterId,
            string fileName,
            string displayName,
            CharacterArchetype archetype,
            int maxHealth,
            WeaponKind weapon,
            List<CardDefinition> startingDeck,
            int overdraftLimit = 0)
        {
            string path = CharacterFolder + "/" + fileName;
            CharacterDefinition asset = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<CharacterDefinition>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.characterId = characterId;
            asset.displayName = displayName;
            asset.archetype = archetype;
            asset.team = Team.Player;
            asset.maxHealth = maxHealth;
            asset.maxEnergy = 3;
            asset.startingWeapon = weapon;
            asset.overdraftLimit = overdraftLimit;
            asset.attackRangeOverride = 0;
            asset.baseDamageOverride = 0;
            asset.speedOverride = 0f;
            asset.availableWeapons = new List<WeaponKind> { weapon };
            asset.startingDeck = startingDeck;
            EditorUtility.SetDirty(asset);
        }

        private static void GenerateEnemies()
        {
            string[] enemyIds =
            {
                "rogue_puppet",
                "rustblade_hunter",
                "steam_guard",
                "gun_wanderer",
                "chaos_apprentice",
                "broken_axle_knight",
                "redline_sniper",
                "furnace_steward",
                "first_chaos_core"
            };

            for (int i = 0; i < enemyIds.Length; i++)
            {
                string enemyId = enemyIds[i];
                string path = EnemyFolder + "/" + enemyId + ".asset";
                EnemyDefinition asset = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
                EnemyDefinition prototype = EnemyLibrary.CreateDefinition(enemyId);
                if (asset == null)
                {
                    asset = Object.Instantiate(prototype);
                    AssetDatabase.CreateAsset(asset, path);
                }
                else
                {
                    EditorUtility.CopySerializedManagedFieldsOnly(prototype, asset);
                    EditorUtility.SetDirty(asset);
                }

                if (prototype != null)
                {
                    Object.DestroyImmediate(prototype);
                }
            }
        }

        private static void GenerateBattleConfig()
        {
            string path = ConfigFolder + "/default_battle_config.asset";
            BattleConfig asset = AssetDatabase.LoadAssetAtPath<BattleConfig>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<BattleConfig>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.timelineLength = 12f;
            asset.chaosWaveSpeed = 1f;
            asset.arenaMin = 0f;
            asset.arenaMax = 20f;
            asset.startingHandSize = 5;
            asset.maxHandSize = 10;
            asset.drawPerPlayerAction = 1;
            asset.drawOnChaosWave = 3;
            asset.energyGainOnActionStart = 1;
            asset.freeMovePerAction = 2f;
            EditorUtility.SetDirty(asset);
        }

        private static void GenerateBattleScenario()
        {
            string configPath = ConfigFolder + "/default_battle_config.asset";
            string playerPath = CharacterFolder + "/lu_xiyun.asset";
            string enemyPath = EnemyFolder + "/rogue_puppet.asset";
            string scenarioPath = ScenarioFolder + "/sample_battle_scenario.asset";

            BattleScenario asset = AssetDatabase.LoadAssetAtPath<BattleScenario>(scenarioPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<BattleScenario>();
                AssetDatabase.CreateAsset(asset, scenarioPath);
            }

            asset.scenarioId = "sample_battle_scenario";
            asset.displayName = "Sample Battle Scenario";
            asset.description = "A starter scene setup that mirrors the current playable prototype.";
            asset.battleConfig = AssetDatabase.LoadAssetAtPath<BattleConfig>(configPath);
            asset.playerCharacter = AssetDatabase.LoadAssetAtPath<CharacterDefinition>(playerPath);
            asset.enemyDefinition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(enemyPath);
            asset.playerPrefab = null;
            asset.enemyPrefab = null;
            asset.playerStartPosition = 4f;
            asset.enemyStartPosition = 11f;
            EditorUtility.SetDirty(asset);
        }

        private static List<CardDefinition> BuildDeck(Dictionary<string, CardDefinition> cards, CharacterArchetype archetype)
        {
            List<CardDefinition> result = new List<CardDefinition>();
            List<CardDefinition> prototypeDeck = BalancePrototypeFactory.CreateStartingDeck(archetype);
            for (int i = 0; i < prototypeDeck.Count; i++)
            {
                CardDefinition prototypeCard = prototypeDeck[i];
                if (prototypeCard == null || string.IsNullOrEmpty(prototypeCard.cardId))
                {
                    continue;
                }

                if (cards.TryGetValue(prototypeCard.cardId, out CardDefinition generatedCard))
                {
                    result.Add(generatedCard);
                }

                Object.DestroyImmediate(prototypeCard);
            }

            return result;
        }
    }
}
