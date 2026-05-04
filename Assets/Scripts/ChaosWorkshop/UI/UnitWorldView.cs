using UnityEngine;

namespace ChaosWorkshop
{
    public class UnitWorldView : MonoBehaviour
    {
        public CombatUnit unit;
        public BattleManager battleManager;
        public Vector3 leftWorld = new Vector3(-8f, 0f, 0f);
        public Vector3 rightWorld = new Vector3(8f, 0f, 0f);

        private void LateUpdate()
        {
            if (unit == null || battleManager == null)
            {
                return;
            }

            float t = Mathf.InverseLerp(battleManager.arenaMin, battleManager.arenaMax, unit.ArenaPosition);
            transform.position = Vector3.Lerp(leftWorld, rightWorld, t);
        }

        private void OnMouseDown()
        {
            if (battleManager != null && unit != null)
            {
                battleManager.SelectTarget(unit);
            }
        }
    }
}
