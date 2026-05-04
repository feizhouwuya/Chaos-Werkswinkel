using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChaosWorkshop
{
    [RequireComponent(typeof(TimelineController))]
    [RequireComponent(typeof(DeckController))]
    public class BattleManager : MonoBehaviour
    {
        [Header("战场")]
        public float arenaMin = 0f;
        public float arenaMax = 20f;
        public float playerStartPosition = 4f;
        public float enemyStartPosition = 16f;

        [Header("抽牌")]
        public int startingHandSize = 5;
        public int drawPerPlayerAction = 1;
        public int drawOnChaosWave = 3;
        public int energyGainOnActionStart = 1;
        public float freeMovePerAction = 2f;

        [Header("运行时")]
        public BattleState state = BattleState.WaitingTimeline;
        public CombatUnit activeUnit;
        public CombatUnit selectedTarget;
        public List<CombatUnit> units = new List<CombatUnit>();

        public event Action BattleChanged;
        public event Action<CardDefinition, CombatUnit, CombatUnit> CardPlayed;
        public event Action<string> LogMessage;

        public TimelineController Timeline { get; private set; }
        public DeckController Deck { get; private set; }
        public bool PlayerPlayedCardThisAction { get; private set; }
        public bool PlayerFreeMoveUsedThisAction { get; private set; }
        public bool LastDrawCameFromChaosWave { get; private set; }
        public CardDefinition HoveredCard { get; private set; }
        public bool CanPlayerUseFreeMove => state == BattleState.PlayerActing
            && activeUnit != null
            && activeUnit.team == Team.Player
            && !PlayerFreeMoveUsedThisAction;

        private void Awake()
        {
            Timeline = GetComponent<TimelineController>();
            Deck = GetComponent<DeckController>();
            Timeline.ChaosWaveTriggered += ResolveChaosWave;
        }

        public void ApplyConfig(BattleConfig config)
        {
            if (config == null)
            {
                return;
            }

            arenaMin = config.arenaMin;
            arenaMax = config.arenaMax;
            startingHandSize = config.startingHandSize;
            drawPerPlayerAction = config.drawPerPlayerAction;
            drawOnChaosWave = config.drawOnChaosWave;
            energyGainOnActionStart = config.energyGainOnActionStart;
            freeMovePerAction = config.freeMovePerAction;

            if (Timeline != null)
            {
                Timeline.timelineLength = config.timelineLength;
                Timeline.chaosWaveSpeed = config.chaosWaveSpeed;
                Timeline.chaosWaveProgress = 0f;
            }

            if (Deck != null)
            {
                Deck.maxHandSize = config.maxHandSize;
            }
        }

        private void Update()
        {
            if (state != BattleState.WaitingTimeline)
            {
                return;
            }

            Timeline.Tick(Time.deltaTime, units);
            CombatUnit ready = Timeline.GetNextReadyUnit(units);
            if (ready != null)
            {
                BeginUnitAction(ready);
            }
        }

        public void StartBattle(CombatUnit player, CombatUnit enemy, IEnumerable<CardDefinition> playerDeck)
        {
            if (player == null || enemy == null)
            {
                Debug.LogWarning("BattleManager.StartBattle requires both player and enemy units.");
                return;
            }

            UnbindCurrentUnits();
            units.Clear();
            units.Add(player);
            units.Add(enemy);

            player.SetPosition(playerStartPosition, arenaMin, arenaMax);
            enemy.SetPosition(enemyStartPosition, arenaMin, arenaMax);
            player.Changed += HandleUnitChanged;
            enemy.Changed += HandleUnitChanged;
            player.Died += HandleUnitDied;
            enemy.Died += HandleUnitDied;
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.battleManager = this;
            }

            Deck.BuildDeck(playerDeck);
            Deck.Draw(startingHandSize);
            if (Timeline != null)
            {
                Timeline.chaosWaveProgress = 0f;
            }
            selectedTarget = enemy;
            state = BattleState.WaitingTimeline;
            activeUnit = null;
            PlayerPlayedCardThisAction = false;
            PlayerFreeMoveUsedThisAction = false;
            LastDrawCameFromChaosWave = false;
            HoveredCard = null;
            Log("战斗开始。");
            NotifyChanged();
        }

        private void UnbindCurrentUnits()
        {
            for (int i = 0; i < units.Count; i++)
            {
                CombatUnit unit = units[i];
                if (unit == null)
                {
                    continue;
                }

                unit.Changed -= HandleUnitChanged;
                unit.Died -= HandleUnitDied;
            }
        }

        public void SetHoveredCard(CardDefinition card)
        {
            if (HoveredCard == card)
            {
                return;
            }

            HoveredCard = card;
            NotifyChanged();
        }

        public bool UsePlayerFreeMoveToward()
        {
            return UsePlayerFreeMove(true);
        }

        public bool UsePlayerFreeMoveAway()
        {
            return UsePlayerFreeMove(false);
        }

        public void SelectTarget(CombatUnit target)
        {
            if (target != null && target.IsAlive)
            {
                Team actingTeam = activeUnit != null ? activeUnit.team : Team.Player;
                if (target.team != actingTeam)
                {
                    selectedTarget = target;
                }

                NotifyChanged();
            }
        }

        public bool CanPlay(CardDefinition card)
        {
            if (state != BattleState.PlayerActing || activeUnit == null || card == null)
            {
                return false;
            }

            if (PlayerPlayedCardThisAction)
            {
                return false;
            }

            return activeUnit.CanSpendEnergy(card.cost) && ResolveTargets(card, activeUnit).Count > 0;
        }

        public void PlayCard(CardDefinition card)
        {
            if (!CanPlay(card))
            {
                Log("当前无法出牌。");
                return;
            }

            if (!activeUnit.SpendEnergy(card.cost))
            {
                Log("能量不足。");
                return;
            }

            List<CombatUnit> targets = ResolveTargets(card, activeUnit);
            bool playerCard = activeUnit.team == Team.Player;
            for (int i = 0; i < targets.Count; i++)
            {
                ApplyCardToTarget(card, activeUnit, targets[i]);
            }

            if (playerCard)
            {
                PlayerPlayedCardThisAction = true;
            }

            Deck.Discard(card);
            CardPlayed?.Invoke(card, activeUnit, targets.Count > 0 ? targets[0] : null);
            Log(activeUnit.displayName + " 使用了" + card.displayName + "。");
            CheckBattleEnd();

            if (playerCard)
            {
                TryAutoEndPlayerAction();
                return;
            }

            NotifyChanged();
        }

        public void EndPlayerAction()
        {
            if (state != BattleState.PlayerActing || activeUnit == null)
            {
                return;
            }

            EndActiveAction();
        }

        public CombatUnit GetFirstAliveEnemy(Team team)
        {
            for (int i = 0; i < units.Count; i++)
            {
                CombatUnit unit = units[i];
                if (unit != null && unit.IsAlive && unit.team != team)
                {
                    return unit;
                }
            }

            return null;
        }

        public float DistanceBetween(CombatUnit a, CombatUnit b)
        {
            if (a == null || b == null)
            {
                return float.PositiveInfinity;
            }

            return Mathf.Abs(a.ArenaPosition - b.ArenaPosition);
        }

        public void EnemyUseBasicAction(CombatUnit enemy)
        {
            CombatUnit target = GetFirstAliveEnemy(enemy.team);
            if (target == null)
            {
                EndActiveAction();
                return;
            }

            float distance = DistanceBetween(enemy, target);
            if (distance <= enemy.AttackRange)
            {
                target.TakeDamage(enemy.BaseDamage);
                Log(enemy.displayName + " 攻击了" + target.displayName + "。");
            }
            else
            {
                float direction = target.ArenaPosition > enemy.ArenaPosition ? 1f : -1f;
                enemy.MoveBy(direction * 2f, arenaMin, arenaMax);
                Log(enemy.displayName + " 向前逼近。");
            }

            CheckBattleEnd();
            EndActiveAction();
        }

        private void BeginUnitAction(CombatUnit unit)
        {
            activeUnit = unit;
            PlayerPlayedCardThisAction = false;
            PlayerFreeMoveUsedThisAction = false;
            activeUnit.ConsumeReady(Timeline.timelineLength);
            if (!activeUnit.IsAlive)
            {
                CheckBattleEnd();
                EndActiveAction();
                return;
            }

            activeUnit.GainEnergy(energyGainOnActionStart);

            if (unit.team == Team.Player)
            {
                state = BattleState.PlayerActing;
                LastDrawCameFromChaosWave = false;
                Deck.Draw(drawPerPlayerAction);
                selectedTarget = GetFirstAliveEnemy(unit.team);
                Log(unit.displayName + " 可以行动。");
            }
            else
            {
                state = BattleState.EnemyActing;
                Log(unit.displayName + " 可以行动。");
                EnemyAI enemyAI = unit.GetComponent<EnemyAI>();
                if (enemyAI != null)
                {
                    enemyAI.battleManager = this;
                    enemyAI.TakeAction(unit);
                }
                else
                {
                    EnemyUseBasicAction(unit);
                }
            }

            NotifyChanged();
        }

        private void EndActiveAction()
        {
            activeUnit = null;
            PlayerPlayedCardThisAction = false;
            PlayerFreeMoveUsedThisAction = false;
            HoveredCard = null;
            if (state != BattleState.Victory && state != BattleState.Defeat)
            {
                state = BattleState.WaitingTimeline;
            }

            NotifyChanged();
        }

        public void CompleteEnemyAction()
        {
            if (state != BattleState.EnemyActing)
            {
                return;
            }

            CheckBattleEnd();
            EndActiveAction();
        }

        public CombatUnit GetPlayerUnit()
        {
            for (int i = 0; i < units.Count; i++)
            {
                CombatUnit unit = units[i];
                if (unit != null && unit.IsAlive && unit.team == Team.Player)
                {
                    return unit;
                }
            }

            return null;
        }

        public void MoveUnitToward(CombatUnit mover, CombatUnit target, float amount)
        {
            if (mover == null || target == null)
            {
                return;
            }

            float direction = target.ArenaPosition > mover.ArenaPosition ? 1f : -1f;
            mover.MoveBy(direction * amount, arenaMin, arenaMax);
        }

        public void MoveUnitAway(CombatUnit mover, CombatUnit target, float amount)
        {
            if (mover == null || target == null)
            {
                return;
            }

            float direction = target.ArenaPosition > mover.ArenaPosition ? -1f : 1f;
            mover.MoveBy(direction * amount, arenaMin, arenaMax);
        }

        public void WriteLog(string message)
        {
            Log(message);
        }

        private List<CombatUnit> ResolveTargets(CardDefinition card, CombatUnit user)
        {
            List<CombatUnit> result = new List<CombatUnit>();
            if (card.targetRule == CardTargetRule.Self)
            {
                result.Add(user);
                return result;
            }

            if (card.targetRule == CardTargetRule.SingleEnemy)
            {
                CombatUnit target = GetSelectedEnemyTarget(user.team);
                if (target != null)
                {
                    result.Add(target);
                }

                return result;
            }

            for (int i = 0; i < units.Count; i++)
            {
                CombatUnit unit = units[i];
                if (unit != null && unit.IsAlive && unit.team != user.team)
                {
                    result.Add(unit);
                }
            }

            return result;
        }

        private bool UsePlayerFreeMove(bool toward)
        {
            if (!CanPlayerUseFreeMove)
            {
                return false;
            }

            CombatUnit target = GetSelectedEnemyTarget(activeUnit.team);
            if (target == null)
            {
                return false;
            }

            if (toward)
            {
                MoveUnitToward(activeUnit, target, freeMovePerAction);
                Log(activeUnit.displayName + " 前进了" + freeMovePerAction.ToString("0") + "格。");
            }
            else
            {
                MoveUnitAway(activeUnit, target, freeMovePerAction);
                Log(activeUnit.displayName + " 后退了" + freeMovePerAction.ToString("0") + "格。");
            }

            PlayerFreeMoveUsedThisAction = true;
            TryAutoEndPlayerAction();
            return true;
        }

        private CombatUnit GetSelectedEnemyTarget(Team actingTeam)
        {
            if (selectedTarget != null && selectedTarget.IsAlive && selectedTarget.team != actingTeam)
            {
                return selectedTarget;
            }

            return GetFirstAliveEnemy(actingTeam);
        }

        private void TryAutoEndPlayerAction()
        {
            if (state != BattleState.PlayerActing || activeUnit == null)
            {
                NotifyChanged();
                return;
            }

            if (PlayerPlayedCardThisAction && PlayerFreeMoveUsedThisAction)
            {
                EndActiveAction();
                return;
            }

            NotifyChanged();
        }

        private void ApplyCardToTarget(CardDefinition card, CombatUnit user, CombatUnit target)
        {
            MoveFromCard(card, user, target);
            float distanceAfterMove = DistanceBetween(user, target);

            if (card.GetDamage(user) > 0)
            {
                int ammoCost = card.ammoCost > 0 ? card.ammoCost : 0;
                bool rangedCard = card.GetRange(user) >= 8 || ammoCost > 0;
                if (ammoCost > 0 && user.RangedAmmo < ammoCost)
                {
                    Log("弹药不足。");
                }
                else if (distanceAfterMove <= card.GetRange(user))
                {
                    if (rangedCard)
                    {
                        user.ConsumeAmmo(ammoCost);
                    }

                    int hitCount = Mathf.Max(1, card.hits);
                    for (int hit = 0; hit < hitCount; hit++)
                    {
                        int damage = card.GetDamage(user);
                        if (target.WeaknessStacks > 0)
                        {
                            damage += card.bonusDamageIfTargetHasWeakness;
                        }

                        if (user.MovedThisAction)
                        {
                            damage += card.bonusDamageIfMovedThisAction;
                        }

                        target.TakeDamage(damage);
                    }

                    if (card.applyWeakness > 0)
                    {
                        target.ApplyWeakness(card.applyWeakness);
                    }

                    if (card.applyBleed > 0)
                    {
                        target.ApplyBleed(card.applyBleed);
                    }
                }
                else
                {
                    Log("目标超出范围。");
                }
            }

            if (card.shield > 0)
            {
                user.GainShield(card.shield);
            }

            if (card.gainEvasion > 0)
            {
                user.GainEvasion(card.gainEvasion);
            }

            if (card.recoverAmmo > 0)
            {
                user.RecoverAmmo(card.recoverAmmo);
            }

            if (card.draw > 0)
            {
                Deck.Draw(card.draw);
            }

            if (card.gainEnergy != 0)
            {
                user.GainEnergy(card.gainEnergy);
            }

            if (card.timelineChange != 0)
            {
                target.ChangeTimeline(card.timelineChange, Timeline.timelineLength);
            }

            if (card.nextWeaponDamageBonus > 0)
            {
                user.AddNextWeaponDamageBonus(card.nextWeaponDamageBonus);
            }

            for (int i = 0; i < card.effects.Count; i++)
            {
                ApplyEffect(card.effects[i], user, target);
            }
        }

        private void MoveFromCard(CardDefinition card, CombatUnit user, CombatUnit target)
        {
            if (card.moveMode == MoveMode.None || Mathf.Approximately(card.moveDistance, 0f))
            {
                return;
            }

            CombatUnit referenceTarget = target != user ? target : GetFirstAliveEnemy(user.team);
            if (referenceTarget == null)
            {
                referenceTarget = target;
            }

            float directionToTarget = referenceTarget.ArenaPosition > user.ArenaPosition ? 1f : -1f;
            float delta = 0f;
            switch (card.moveMode)
            {
                case MoveMode.TowardTarget:
                    delta = directionToTarget * card.moveDistance;
                    break;
                case MoveMode.AwayFromTarget:
                    delta = -directionToTarget * card.moveDistance;
                    break;
                case MoveMode.FixedForward:
                    delta = card.moveDistance;
                    break;
                case MoveMode.FixedBackward:
                    delta = -card.moveDistance;
                    break;
            }

            user.MoveBy(delta, arenaMin, arenaMax);
        }

        private void ApplyEffect(CardEffect effect, CombatUnit user, CombatUnit target)
        {
            CombatUnit receiver = target;
            if (receiver == null)
            {
                return;
            }

            switch (effect.type)
            {
                case CardEffectType.Damage:
                    receiver.TakeDamage(effect.amount);
                    break;
                case CardEffectType.Move:
                    receiver.MoveBy(effect.amount, arenaMin, arenaMax);
                    EnemyAI movedEnemyAI = receiver.GetComponent<EnemyAI>();
                    if (movedEnemyAI != null)
                    {
                        movedEnemyAI.RegisterForcedMove(effect.amount);
                    }

                    break;
                case CardEffectType.ChangeSpeed:
                    receiver.ChangeSpeed(effect.amount);
                    break;
                case CardEffectType.ChangeTimeline:
                    receiver.ChangeTimeline(effect.amount, Timeline.timelineLength);
                    break;
                case CardEffectType.GainEnergy:
                    user.GainEnergy(effect.amount);
                    break;
                case CardEffectType.ApplyWeakness:
                    receiver.ApplyWeakness(effect.amount);
                    break;
                case CardEffectType.DrawCards:
                    Deck.Draw(effect.amount);
                    break;
                case CardEffectType.GainShield:
                    user.GainShield(effect.amount);
                    break;
                case CardEffectType.ApplyBleed:
                    receiver.ApplyBleed(effect.amount);
                    break;
                case CardEffectType.GainEvasion:
                    user.GainEvasion(effect.amount);
                    break;
                case CardEffectType.RecoverAmmo:
                    user.RecoverAmmo(effect.amount);
                    break;
            }
        }

        private void ResolveChaosWave()
        {
            for (int i = 0; i < units.Count; i++)
            {
                CombatUnit unit = units[i];
                if (unit != null && unit.IsAlive)
                {
                    unit.RestoreEnergy();
                    if (unit.Weapon != null && unit.Weapon.ammoRecoverOnChaosWave > 0)
                    {
                        unit.RecoverAmmo(unit.Weapon.ammoRecoverOnChaosWave);
                    }
                }
            }

            for (int i = 0; i < units.Count; i++)
            {
                CombatUnit unit = units[i];
                if (unit != null && unit.IsAlive && unit.team == Team.Enemy)
                {
                    EnemyAI enemyAI = unit.GetComponent<EnemyAI>();
                    if (enemyAI != null)
                    {
                        enemyAI.battleManager = this;
                        enemyAI.OnChaosWaveResolved();
                    }
                }
            }

            LastDrawCameFromChaosWave = true;
            int drawn = Deck.Draw(drawOnChaosWave, true);
            int discarded = Deck.DiscardToHandLimit();
            if (drawn > 0)
            {
                Log("混沌浪潮：抽取了" + drawn + "张牌。");
            }
            if (discarded > 0)
            {
                Log("混沌浪潮：超出上限，弃掉了" + discarded + "张牌。");
            }

            int roll = UnityEngine.Random.Range(0, 10);
            if (roll == 0)
            {
                for (int i = 0; i < units.Count; i++)
                {
                    if (units[i] != null && units[i].IsAlive)
                    {
                        units[i].ChangeSpeed(1f);
                    }
                }

                Log("混沌浪潮：全体速度提升。");
            }
            else if (roll == 1)
            {
                for (int i = 0; i < units.Count; i++)
                {
                    if (units[i] != null && units[i].IsAlive)
                    {
                        units[i].ChangeTimeline(2f, Timeline.timelineLength);
                    }
                }

                Log("混沌浪潮：全体行动条前进。");
            }
            else if (roll == 2)
            {
                CombatUnit player = units.Find(unit => unit != null && unit.team == Team.Player && unit.IsAlive);
                if (player != null)
                {
                    player.Heal(3);
                }

                Log("混沌浪潮：玩家恢复生命。");
            }
            else if (roll == 3)
            {
                for (int i = 0; i < units.Count; i++)
                {
                    if (units[i] != null && units[i].IsAlive && units[i].team == Team.Enemy)
                    {
                        units[i].ChangeTimeline(-2f, Timeline.timelineLength);
                    }
                }

                Log("混沌浪潮：敌方行动被延后。");
            }
            else if (roll == 4)
            {
                CombatUnit player = units.Find(unit => unit != null && unit.team == Team.Player && unit.IsAlive);
                if (player != null)
                {
                    player.GainEnergy(-1);
                }

                Log("混沌浪潮：玩家失去能量。");
            }
            else
            {
                Log("混沌浪潮：额外抽牌。");
            }

            NotifyChanged();
        }

        private void HandleUnitChanged(CombatUnit unit)
        {
            NotifyChanged();
        }

        private void HandleUnitDied(CombatUnit unit)
        {
            Log(unit.displayName + " 被击倒。");
            CheckBattleEnd();
        }

        private void CheckBattleEnd()
        {
            bool playerAlive = units.Exists(unit => unit != null && unit.team == Team.Player && unit.IsAlive);
            bool enemyAlive = units.Exists(unit => unit != null && unit.team == Team.Enemy && unit.IsAlive);

            if (!enemyAlive)
            {
                state = BattleState.Victory;
                Log("胜利。");
            }
            else if (!playerAlive)
            {
                state = BattleState.Defeat;
                Log("失败。");
            }
        }

        private void NotifyChanged()
        {
            BattleChanged?.Invoke();
        }

        private void Log(string message)
        {
            Debug.Log("[混沌工坊] " + message);
            LogMessage?.Invoke(message);
        }
    }
}
