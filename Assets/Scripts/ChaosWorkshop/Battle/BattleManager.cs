using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChaosWorkshop
{
    [RequireComponent(typeof(TimelineController))]
    [RequireComponent(typeof(DeckController))]
    public class BattleManager : MonoBehaviour
    {
        [Header("Arena")]
        public float arenaMin = 0f;
        public float arenaMax = 20f;
        public float playerStartPosition = 4f;
        public float enemyStartPosition = 16f;

        [Header("Draw")]
        public int drawPerPlayerAction = 3;

        [Header("Runtime")]
        public BattleState state = BattleState.WaitingTimeline;
        public CombatUnit activeUnit;
        public CombatUnit selectedTarget;
        public List<CombatUnit> units = new List<CombatUnit>();

        public event Action BattleChanged;
        public event Action<CardDefinition, CombatUnit, CombatUnit> CardPlayed;
        public event Action<string> LogMessage;

        public TimelineController Timeline { get; private set; }
        public DeckController Deck { get; private set; }

        private void Awake()
        {
            Timeline = GetComponent<TimelineController>();
            Deck = GetComponent<DeckController>();
            Timeline.ChaosWaveTriggered += ResolveChaosWave;
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
            units.Clear();
            units.Add(player);
            units.Add(enemy);

            player.SetPosition(playerStartPosition, arenaMin, arenaMax);
            enemy.SetPosition(enemyStartPosition, arenaMin, arenaMax);
            player.Changed += HandleUnitChanged;
            enemy.Changed += HandleUnitChanged;
            player.Died += HandleUnitDied;
            enemy.Died += HandleUnitDied;

            Deck.BuildDeck(playerDeck);
            Deck.Draw(drawPerPlayerAction);
            selectedTarget = enemy;
            state = BattleState.WaitingTimeline;
            activeUnit = null;
            Log("Battle started.");
            NotifyChanged();
        }

        public void SelectTarget(CombatUnit target)
        {
            if (target != null && target.IsAlive)
            {
                selectedTarget = target;
                NotifyChanged();
            }
        }

        public bool CanPlay(CardDefinition card)
        {
            if (state != BattleState.PlayerActing || activeUnit == null || card == null)
            {
                return false;
            }

            return activeUnit.Energy >= card.cost && ResolveTargets(card, activeUnit).Count > 0;
        }

        public void PlayCard(CardDefinition card)
        {
            if (!CanPlay(card))
            {
                Log("Cannot play card.");
                return;
            }

            if (!activeUnit.SpendEnergy(card.cost))
            {
                Log("Not enough energy.");
                return;
            }

            List<CombatUnit> targets = ResolveTargets(card, activeUnit);
            for (int i = 0; i < targets.Count; i++)
            {
                ApplyCardToTarget(card, activeUnit, targets[i]);
            }

            Deck.Discard(card);
            CardPlayed?.Invoke(card, activeUnit, targets.Count > 0 ? targets[0] : null);
            Log(activeUnit.displayName + " used " + card.displayName + ".");
            CheckBattleEnd();
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
                Log(enemy.displayName + " attacked " + target.displayName + ".");
            }
            else
            {
                float direction = target.ArenaPosition > enemy.ArenaPosition ? 1f : -1f;
                enemy.MoveBy(direction * 2f, arenaMin, arenaMax);
                Log(enemy.displayName + " moved closer.");
            }

            CheckBattleEnd();
            EndActiveAction();
        }

        private void BeginUnitAction(CombatUnit unit)
        {
            activeUnit = unit;
            activeUnit.ConsumeReady(Timeline.timelineLength);

            if (unit.team == Team.Player)
            {
                state = BattleState.PlayerActing;
                Deck.Draw(drawPerPlayerAction);
                selectedTarget = GetFirstAliveEnemy(unit.team);
                Log(unit.displayName + " can act.");
            }
            else
            {
                state = BattleState.EnemyActing;
                Log(unit.displayName + " can act.");
                EnemyUseBasicAction(unit);
            }

            NotifyChanged();
        }

        private void EndActiveAction()
        {
            activeUnit = null;
            state = BattleState.WaitingTimeline;
            NotifyChanged();
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
                CombatUnit target = selectedTarget != null && selectedTarget.IsAlive ? selectedTarget : GetFirstAliveEnemy(user.team);
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

        private void ApplyCardToTarget(CardDefinition card, CombatUnit user, CombatUnit target)
        {
            MoveFromCard(card, user, target);

            if (card.GetDamage(user) > 0)
            {
                bool rangedCard = card.GetRange(user) >= 8;
                if (rangedCard && user.RangedAmmo == 0 && user.weaponKind == WeaponKind.HandCrossbow)
                {
                    Log("No ranged ammo.");
                }
                else if (DistanceBetween(user, target) <= card.GetRange(user))
                {
                    if (rangedCard && user.weaponKind == WeaponKind.HandCrossbow)
                    {
                        user.ConsumeAmmo();
                    }

                    target.TakeDamage(card.GetDamage(user));
                }
                else
                {
                    Log("Target is out of range.");
                }
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

            float directionToTarget = target.ArenaPosition > user.ArenaPosition ? 1f : -1f;
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
                }
            }

            int roll = UnityEngine.Random.Range(0, 3);
            if (roll == 0)
            {
                CombatUnit player = units.Find(unit => unit != null && unit.team == Team.Player && unit.IsAlive);
                if (player != null)
                {
                    player.ChangeSpeed(1f);
                    Log("Chaos Wave: player speed increased.");
                }
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

                Log("Chaos Wave: all units rushed forward.");
            }
            else
            {
                Deck.Draw(1);
                Log("Chaos Wave: drew one card.");
            }

            NotifyChanged();
        }

        private void HandleUnitChanged(CombatUnit unit)
        {
            NotifyChanged();
        }

        private void HandleUnitDied(CombatUnit unit)
        {
            Log(unit.displayName + " fell.");
            CheckBattleEnd();
        }

        private void CheckBattleEnd()
        {
            bool playerAlive = units.Exists(unit => unit != null && unit.team == Team.Player && unit.IsAlive);
            bool enemyAlive = units.Exists(unit => unit != null && unit.team == Team.Enemy && unit.IsAlive);

            if (!enemyAlive)
            {
                state = BattleState.Victory;
                Log("Victory.");
            }
            else if (!playerAlive)
            {
                state = BattleState.Defeat;
                Log("Defeat.");
            }
        }

        private void NotifyChanged()
        {
            BattleChanged?.Invoke();
        }

        private void Log(string message)
        {
            Debug.Log("[Chaos Workshop] " + message);
            LogMessage?.Invoke(message);
        }
    }
}
