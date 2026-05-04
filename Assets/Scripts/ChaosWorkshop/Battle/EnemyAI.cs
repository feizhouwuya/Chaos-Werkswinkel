using UnityEngine;

namespace ChaosWorkshop
{
    public class EnemyAI : MonoBehaviour
    {
        public BattleManager battleManager;

        public void TakeAction(CombatUnit enemy)
        {
            if (battleManager == null || enemy == null)
            {
                return;
            }

            battleManager.EnemyUseBasicAction(enemy);
        }
    }
}
