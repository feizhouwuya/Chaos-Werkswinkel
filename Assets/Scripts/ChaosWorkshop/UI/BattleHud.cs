using UnityEngine;
using UnityEngine.UI;

namespace ChaosWorkshop
{
    public class BattleHud : MonoBehaviour
    {
        public BattleManager battleManager;
        public Text playerText;
        public Text enemyText;
        public Text stateText;
        public Text logText;
        public Slider chaosWaveSlider;
        public Button endActionButton;

        private string latestLog = string.Empty;

        private void OnEnable()
        {
            if (battleManager != null)
            {
                battleManager.BattleChanged += Refresh;
                battleManager.LogMessage += HandleLog;
            }

            if (endActionButton != null)
            {
                endActionButton.onClick.AddListener(EndAction);
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.BattleChanged -= Refresh;
                battleManager.LogMessage -= HandleLog;
            }

            if (endActionButton != null)
            {
                endActionButton.onClick.RemoveListener(EndAction);
            }
        }

        public void Refresh()
        {
            if (battleManager == null)
            {
                return;
            }

            CombatUnit player = null;
            CombatUnit enemy = null;
            for (int i = 0; i < battleManager.units.Count; i++)
            {
                CombatUnit unit = battleManager.units[i];
                if (unit == null)
                {
                    continue;
                }

                if (unit.team == Team.Player)
                {
                    player = unit;
                }
                else if (unit.team == Team.Enemy)
                {
                    enemy = unit;
                }
            }

            if (playerText != null)
            {
                playerText.text = FormatUnit(player);
            }

            if (enemyText != null)
            {
                enemyText.text = FormatUnit(enemy);
            }

            if (stateText != null)
            {
                string activeName = battleManager.activeUnit != null ? battleManager.activeUnit.displayName : "None";
                stateText.text = "State: " + battleManager.state + " | Active: " + activeName;
            }

            if (logText != null)
            {
                logText.text = latestLog;
            }

            if (chaosWaveSlider != null)
            {
                chaosWaveSlider.maxValue = battleManager.Timeline.timelineLength;
                chaosWaveSlider.value = battleManager.Timeline.chaosWaveProgress;
            }

            if (endActionButton != null)
            {
                endActionButton.interactable = battleManager.state == BattleState.PlayerActing;
            }
        }

        private string FormatUnit(CombatUnit unit)
        {
            if (unit == null)
            {
                return "None";
            }

            return unit.displayName
                + "\nHP: " + unit.Health + "/" + unit.maxHealth
                + "  Energy: " + unit.Energy + "/" + unit.MaxEnergy
                + "\nPos: " + unit.ArenaPosition.ToString("0.0")
                + "  Speed: " + unit.Speed.ToString("0.0")
                + "  Timeline: " + unit.TimelineProgress.ToString("0.0");
        }

        private void HandleLog(string message)
        {
            latestLog = message;
            Refresh();
        }

        private void EndAction()
        {
            battleManager.EndPlayerAction();
        }
    }
}
