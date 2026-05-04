using UnityEngine;

namespace ChaosWorkshop
{
    [CreateAssetMenu(menuName = "Chaos Workshop/Battle Config", fileName = "BattleConfig")]
    public class BattleConfig : ScriptableObject
    {
        [Header("Timeline")]
        public float timelineLength = 12f;
        public float chaosWaveSpeed = 1f;

        [Header("Arena")]
        public float arenaMin = 0f;
        public float arenaMax = 20f;

        [Header("Resources")]
        public int startingHandSize = 5;
        public int maxHandSize = 10;
        public int drawPerPlayerAction = 1;
        public int drawOnChaosWave = 3;
        public int energyGainOnActionStart = 1;
        public float freeMovePerAction = 2f;
    }
}
