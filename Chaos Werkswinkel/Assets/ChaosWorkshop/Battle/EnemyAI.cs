using UnityEngine;

namespace ChaosWorkshop
{
    [RequireComponent(typeof(CombatUnit))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Definition")]
        public string enemyId = "rogue_puppet";
        public EnemyDefinition definition;

        [Header("Runtime State")]
        public int powerStacks;
        public bool markedPlayer;
        public bool panicShot;
        public bool postWaveAction;
        public int missedByDistanceCount;
        public string lastUsedActionId;
        public int repeatedTimelineControlCount;
        public int randomDistanceActionsInRow;
        public bool lowHealthGuardUsed;
        public int bossPhase = 1;

        public BattleManager battleManager;
        private CombatUnit unit;

        private void Awake()
        {
            unit = GetComponent<CombatUnit>();
            EnsureDefinition();
        }

        private void OnValidate()
        {
            if (definition == null && !string.IsNullOrEmpty(enemyId))
            {
                definition = EnemyLibrary.CreateDefinition(enemyId);
            }
        }

        public void Configure(string id)
        {
            enemyId = id;
            definition = EnemyLibrary.CreateDefinition(enemyId);
            unit = GetComponent<CombatUnit>();
            EnemyLibrary.ApplyDefinition(unit, definition);
        }

        public void Configure(EnemyDefinition enemyDefinition)
        {
            definition = enemyDefinition;
            if (definition != null)
            {
                enemyId = definition.enemyId;
            }

            unit = GetComponent<CombatUnit>();
            if (definition != null && unit != null)
            {
                EnemyLibrary.ApplyDefinition(unit, definition);
            }
        }

        public void TakeAction(CombatUnit enemy)
        {
            if (battleManager == null || enemy == null)
            {
                return;
            }

            unit = enemy;
            EnsureDefinition();
            if (battleManager.GetPlayerUnit() == null)
            {
                battleManager.CompleteEnemyAction();
                return;
            }

            switch (definition.enemyId)
            {
                case "rustblade_hunter":
                    ActRustbladeHunter();
                    break;
                case "steam_guard":
                    ActSteamGuard();
                    break;
                case "gun_wanderer":
                    ActGunWanderer();
                    break;
                case "chaos_apprentice":
                    ActChaosApprentice();
                    break;
                case "broken_axle_knight":
                    ActBrokenAxleKnight();
                    break;
                case "redline_sniper":
                    ActRedlineSniper();
                    break;
                case "furnace_steward":
                    ActFurnaceSteward();
                    break;
                case "first_chaos_core":
                    ActFirstChaosCore();
                    break;
                default:
                    ActRoguePuppet();
                    break;
            }

            battleManager.CompleteEnemyAction();
        }

        public void OnChaosWaveResolved()
        {
            EnsureDefinition();
            postWaveAction = true;

            if (definition.enemyId == "furnace_steward")
            {
                unit.GainShield(14);
                battleManager.WriteLog(unit.displayName + " 封锁了熔炉之门。");
            }
            else if (definition.enemyId == "first_chaos_core")
            {
                UpdateBossPhase();
                if (bossPhase == 1)
                {
                    unit.GainShield(10);
                    battleManager.WriteLog(unit.displayName + " 凝聚了浪潮护甲。");
                }
                else if (bossPhase == 3)
                {
                    RandomizeDistance(2f);
                    unit.ChangeTimeline(2f, battleManager.Timeline.timelineLength);
                    battleManager.WriteLog(unit.displayName + " 以混沌浪潮扰乱了战场。");
                }
            }
        }

        public void RegisterForcedMove(float amount)
        {
            if (definition != null && definition.enemyId == "broken_axle_knight" && !Mathf.Approximately(amount, 0f))
            {
                powerStacks = Mathf.Clamp(powerStacks + 1, 0, 4);
                battleManager.WriteLog(unit.displayName + " 将位移转化为了力量。");
            }
        }

        private void ActRoguePuppet()
        {
            CombatUnit player = battleManager.GetPlayerUnit();
            float distance = battleManager.DistanceBetween(unit, player);
            if (distance <= 2f)
            {
                int damage = missedByDistanceCount >= 2 ? 8 : 6;
                missedByDistanceCount = 0;
                Attack(player, damage, 1, "残损挥击");
            }
            else
            {
                missedByDistanceCount++;
                MoveToward(player, 2f, "蹒跚逼近");
            }
        }

        private void ActRustbladeHunter()
        {
            CombatUnit player = battleManager.GetPlayerUnit();
            float distance = battleManager.DistanceBetween(unit, player);
            if (distance <= 1f)
            {
                Attack(player, 4, 2, "锈刃连斩");
                return;
            }

            float move = distance <= 4f ? 3f : 2f;
            MoveToward(player, move, "贴地突进");
        }

        private void ActSteamGuard()
        {
            CombatUnit player = battleManager.GetPlayerUnit();
            float distance = battleManager.DistanceBetween(unit, player);
            if (distance <= 2f)
            {
                if (postWaveAction)
                {
                    unit.GainShield(6);
                    postWaveAction = false;
                }

                Attack(player, 11, 1, "蒸汽重击");
            }
            else
            {
                unit.GainShield(postWaveAction ? 14 : 8);
                postWaveAction = false;
                MoveToward(player, 2f, "盾压推进");
            }
        }

        private void ActGunWanderer()
        {
            CombatUnit player = battleManager.GetPlayerUnit();
            float distance = battleManager.DistanceBetween(unit, player);
            if (distance >= 4f && distance <= 9f)
            {
                int damage = panicShot ? 9 : 7;
                panicShot = false;
                Attack(player, damage, 1, "粗制射击");
            }
            else if (distance <= 3f)
            {
                panicShot = true;
                MoveAway(player, 3f, "惊惶后撤");
            }
            else
            {
                MoveToward(player, 2f, "调整射线");
            }
        }

        private void ActChaosApprentice()
        {
            CombatUnit player = battleManager.GetPlayerUnit();
            float distance = battleManager.DistanceBetween(unit, player);
            bool canDelay = player.TimelineProgress >= 8f && repeatedTimelineControlCount == 0 && unit.Health > unit.maxHealth * 0.35f;
            if (postWaveAction)
            {
                unit.ChangeTimeline(2f, battleManager.Timeline.timelineLength);
                postWaveAction = false;
            }

            if (canDelay)
            {
                player.ChangeTimeline(-2f, battleManager.Timeline.timelineLength);
                repeatedTimelineControlCount = 1;
                Use("扭曲秩序");
            }
            else if (distance <= 3f)
            {
                repeatedTimelineControlCount = 0;
                Attack(player, 7, 1, "混沌短击");
            }
            else
            {
                repeatedTimelineControlCount = 0;
                MoveToward(player, 2f, "闪烁逼近");
            }
        }

        private void ActBrokenAxleKnight()
        {
            CombatUnit player = battleManager.GetPlayerUnit();
            float distance = battleManager.DistanceBetween(unit, player);
            if (!lowHealthGuardUsed && unit.Health <= unit.maxHealth * 0.4f && unit.Shield == 0)
            {
                lowHealthGuardUsed = true;
                unit.GainShield(12);
                Use("断轮守势");
                return;
            }

            if (powerStacks >= 2 && distance <= 5f)
            {
                powerStacks = Mathf.Max(0, powerStacks - 2);
                MoveToward(player, 2f, "裂轴突进");
                if (battleManager.DistanceBetween(unit, player) <= 3f)
                {
                    Attack(player, 16, 1, "裂轴突进");
                }

                return;
            }

            if (distance <= 3f)
            {
                Attack(player, 12 + powerStacks, 1, "骑士横扫");
            }
            else
            {
                unit.GainShield(6);
                MoveToward(player, 2f, "轴步推进");
            }
        }

        private void ActRedlineSniper()
        {
            CombatUnit player = battleManager.GetPlayerUnit();
            float distance = battleManager.DistanceBetween(unit, player);
            if (distance <= 1f)
            {
                unit.GainEvasion(1);
                MoveAway(player, 4f, "翻跃后撤");
            }
            else if (distance >= 6f && markedPlayer)
            {
                markedPlayer = false;
                Attack(player, 14, 1, "红线贯穿射击");
            }
            else if (distance >= 6f)
            {
                markedPlayer = true;
                Use("红线瞄准");
            }
            else
            {
                Attack(player, 8, 1, "腰射");
            }
        }

        private void ActFurnaceSteward()
        {
            CombatUnit player = battleManager.GetPlayerUnit();
            float distance = battleManager.DistanceBetween(unit, player);
            if (distance <= 3f)
            {
                int damage = unit.Shield >= 20 ? 18 : 14;
                Attack(player, damage, 1, "执事重锤");
            }
            else
            {
                unit.GainShield(6);
                MoveToward(player, 2f, "煤压推进");
            }
        }

        private void ActFirstChaosCore()
        {
            UpdateBossPhase();
            CombatUnit player = battleManager.GetPlayerUnit();
            float distance = battleManager.DistanceBetween(unit, player);

            if (bossPhase == 1)
            {
                if (distance <= 3f)
                {
                    Attack(player, 12, 1, "炉掌重击");
                }
                else
                {
                    MoveToward(player, 2f, "炉轨推进");
                }
            }
            else if (bossPhase == 2)
            {
                if (unit.Speed < 3f)
                {
                    unit.ChangeSpeed(1f);
                }

                if (distance <= 3f)
                {
                    PullTogether(player, 2f);
                    Attack(player, 11, 1, "磁链牵引");
                }
                else if (distance <= 8f)
                {
                    PullTogether(player, 3f);
                    unit.ChangeTimeline(1f, battleManager.Timeline.timelineLength);
                    Use("牵引脉冲");
                }
                else
                {
                    MoveToward(player, 2f, "磁轨推进");
                }
            }
            else
            {
                if (distance <= 3f)
                {
                    randomDistanceActionsInRow = 0;
                    Attack(player, 13, 1, "灼燃重击");
                }
                else if (distance <= 6f)
                {
                    randomDistanceActionsInRow = 0;
                    Attack(player, 9, 1, "熔线喷射");
                }
                else if (randomDistanceActionsInRow < 2)
                {
                    randomDistanceActionsInRow++;
                    RandomizeDistance(2f);
                    unit.ChangeTimeline(1f, battleManager.Timeline.timelineLength);
                    Use("混沌校准");
                }
                else
                {
                    randomDistanceActionsInRow = 0;
                    MoveToward(player, 2f, "可预测推进");
                }
            }
        }

        private void UpdateBossPhase()
        {
            float hpPercent = unit.maxHealth > 0 ? unit.Health / (float)unit.maxHealth : 0f;
            bossPhase = hpPercent > 0.7f ? 1 : hpPercent > 0.35f ? 2 : 3;
        }

        private void Attack(CombatUnit target, int damage, int hits, string actionName)
        {
            int hitCount = Mathf.Max(1, hits);
            for (int i = 0; i < hitCount; i++)
            {
                target.TakeDamage(damage);
            }

            Use(actionName);
        }

        private void MoveToward(CombatUnit target, float amount, string actionName)
        {
            battleManager.MoveUnitToward(unit, target, amount);
            Use(actionName);
        }

        private void MoveAway(CombatUnit target, float amount, string actionName)
        {
            battleManager.MoveUnitAway(unit, target, amount);
            Use(actionName);
        }

        private void PullTogether(CombatUnit target, float amount)
        {
            battleManager.MoveUnitToward(unit, target, amount * 0.5f);
            battleManager.MoveUnitToward(target, unit, amount * 0.5f);
        }

        private void RandomizeDistance(float amount)
        {
            CombatUnit player = battleManager.GetPlayerUnit();
            float direction = Random.value < 0.5f ? -1f : 1f;
            player.MoveBy(direction * amount, battleManager.arenaMin, battleManager.arenaMax);
        }

        private void Use(string actionName)
        {
            lastUsedActionId = actionName;
            battleManager.WriteLog(unit.displayName + " 使用了" + actionName + "。");
        }

        private void EnsureDefinition()
        {
            if (definition == null)
            {
                definition = EnemyLibrary.CreateDefinition(enemyId);
            }
            else if (string.IsNullOrEmpty(enemyId))
            {
                enemyId = definition.enemyId;
            }
        }
    }
}
