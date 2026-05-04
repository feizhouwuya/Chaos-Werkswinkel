using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChaosWorkshop
{
    public class TimelineController : MonoBehaviour
    {
        [Header("Timeline")]
        public float timelineLength = 12f;
        public float chaosWaveProgress;
        public float chaosWaveSpeed = 1f;

        public event Action ChaosWaveTriggered;

        public void Tick(float deltaTime, IReadOnlyList<CombatUnit> units)
        {
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i] != null)
                {
                    units[i].TickTimeline(deltaTime);
                }
            }

            chaosWaveProgress += chaosWaveSpeed * deltaTime;
            if (chaosWaveProgress >= timelineLength)
            {
                chaosWaveProgress -= timelineLength;
                ChaosWaveTriggered?.Invoke();
            }
        }

        public CombatUnit GetNextReadyUnit(IReadOnlyList<CombatUnit> units)
        {
            CombatUnit best = null;
            float bestProgress = float.MinValue;

            for (int i = 0; i < units.Count; i++)
            {
                CombatUnit unit = units[i];
                if (unit == null || !unit.IsReady(timelineLength))
                {
                    continue;
                }

                if (unit.TimelineProgress > bestProgress)
                {
                    best = unit;
                    bestProgress = unit.TimelineProgress;
                }
            }

            return best;
        }
    }
}
