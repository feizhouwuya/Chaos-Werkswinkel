using UnityEngine;
using UnityEngine.UI;

namespace ChaosWorkshop
{
    public class BattleHud : MonoBehaviour
    {
        [Header("References")]
        public BattleManager battleManager;

        [Header("Legacy Text Blocks")]
        public Text playerText;
        public Text enemyText;

        [Header("Detailed Text")]
        public Text playerNameText;
        public Text enemyNameText;
        public Text playerStatsText;
        public Text enemyStatsText;
        public Text playerStatusText;
        public Text enemyStatusText;
        public Text playerDamagePopupText;
        public Text enemyDamagePopupText;
        public Text playerEnergyValueText;
        public Text stateText;
        public Text logText;
        public Text distanceText;
        public Text cardDetailText;

        [Header("Image Slots")]
        public Image playerEnergyOrbImage;
        public Image playerPortraitImage;
        public Image enemyPortraitImage;
        public UIUnitImageBinding playerPortraitBinding = new UIUnitImageBinding();
        public UIUnitImageBinding enemyPortraitBinding = new UIUnitImageBinding();
        public UIImageArtworkSlot[] artworkSlots = new UIImageArtworkSlot[0];

        [Header("Controls")]
        public Slider chaosWaveSlider;
        public Button endActionButton;
        public Button moveTowardButton;
        public Button moveAwayButton;

        [Header("Arena Track")]
        public RectTransform arenaTrack;
        public RectTransform playerMarker;
        public RectTransform enemyMarker;
        public RectTransform playerRangeBar;
        public RectTransform enemyRangeBar;

        private string latestLog = string.Empty;
        private PopupState playerPopup;
        private PopupState enemyPopup;

        private const float PopupDuration = 0.75f;
        private const float PopupRiseDistance = 26f;
        private const string EndActionLabel = "结束行动";
        private const string MoveTowardLabel = "前进";
        private const string MoveAwayLabel = "后退";

        private struct PopupState
        {
            public bool active;
            public float timer;
            public Vector2 basePosition;
        }

        private void OnEnable()
        {
            ApplyArtwork();
            InitializePopupText(ref playerPopup, playerDamagePopupText);
            InitializePopupText(ref enemyPopup, enemyDamagePopupText);
            BindButtons();

            if (battleManager != null)
            {
                battleManager.BattleChanged += Refresh;
                battleManager.LogMessage += HandleLog;
                SubscribeToUnitDamage();
            }

            Refresh();
        }

        private void OnDisable()
        {
            UnbindButtons();

            if (battleManager != null)
            {
                battleManager.BattleChanged -= Refresh;
                battleManager.LogMessage -= HandleLog;
                UnsubscribeFromUnitDamage();
            }
        }

        private void Update()
        {
            UpdatePopup(ref playerPopup, playerDamagePopupText);
            UpdatePopup(ref enemyPopup, enemyDamagePopupText);
        }

        public void ApplyArtwork()
        {
            UIArtworkUtility.ApplySlots(artworkSlots);

            if (playerPortraitBinding != null && playerPortraitBinding.targetImage == null)
            {
                playerPortraitBinding.targetImage = playerPortraitImage;
            }

            if (enemyPortraitBinding != null && enemyPortraitBinding.targetImage == null)
            {
                enemyPortraitBinding.targetImage = enemyPortraitImage;
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
                playerText.text = FormatUnit(player, false);
            }

            if (enemyText != null)
            {
                enemyText.text = FormatUnit(enemy, true);
            }

            RefreshUnitTextFragments(player, playerNameText, playerStatsText, playerStatusText, false);
            RefreshUnitTextFragments(enemy, enemyNameText, enemyStatsText, enemyStatusText, true);
            ApplyUnitPortraits(player, enemy);
            RefreshPlayerEnergy(player);

            if (stateText != null)
            {
                string activeName = battleManager.activeUnit != null ? battleManager.activeUnit.displayName : "无";
                stateText.text = "状态：" + GetStateLabel(battleManager.state) + "  当前行动：" + activeName;
            }

            if (logText != null)
            {
                logText.text = latestLog;
            }

            if (distanceText != null)
            {
                distanceText.text = BuildDistanceText(player, enemy);
            }

            if (cardDetailText != null)
            {
                cardDetailText.text = BuildCardDetailText(battleManager.HoveredCard);
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

            bool canUseFreeMove = battleManager.CanPlayerUseFreeMove;
            if (moveTowardButton != null)
            {
                moveTowardButton.interactable = canUseFreeMove;
            }

            if (moveAwayButton != null)
            {
                moveAwayButton.interactable = canUseFreeMove;
            }

            UpdateArenaTrack(player, enemy);
        }

        private string FormatUnit(CombatUnit unit, bool includeEnergy)
        {
            if (unit == null)
            {
                return "无";
            }

            string firstLine = unit.displayName + "\n生命 " + unit.Health + "/" + unit.maxHealth;
            if (includeEnergy)
            {
                firstLine += "  能量 " + unit.Energy + "/" + unit.MaxEnergy;
            }

            firstLine += "  护盾 " + unit.Shield;

            return firstLine
                + "\n位置 " + unit.ArenaPosition.ToString("0.0")
                + "  速度 " + unit.Speed.ToString("0.0")
                + "  行动条 " + unit.TimelineProgress.ToString("0.0")
                + "\n弱点 " + unit.WeaknessStacks
                + "  流血 " + unit.BleedStacks
                + "  闪避 " + unit.EvasionStacks
                + "  弹药 " + unit.RangedAmmo;
        }

        private void RefreshUnitTextFragments(CombatUnit unit, Text nameText, Text statsText, Text statusText, bool includeEnergy)
        {
            if (nameText != null)
            {
                nameText.text = unit != null ? unit.displayName : "无";
            }

            if (statsText != null)
            {
                statsText.text = unit == null
                    ? "生命 -"
                    : "生命 " + unit.Health + "/" + unit.maxHealth
                    + (includeEnergy ? "  能量 " + unit.Energy + "/" + unit.MaxEnergy : string.Empty)
                    + "  护盾 " + unit.Shield;
            }

            if (statusText != null)
            {
                statusText.text = unit == null
                    ? string.Empty
                    : "弱点 " + unit.WeaknessStacks
                    + "  流血 " + unit.BleedStacks
                    + "  闪避 " + unit.EvasionStacks
                    + "  弹药 " + unit.RangedAmmo;
            }
        }

        private void ApplyUnitPortraits(CombatUnit player, CombatUnit enemy)
        {
            if (playerPortraitBinding != null)
            {
                playerPortraitBinding.Apply(player);
            }

            if (enemyPortraitBinding != null)
            {
                enemyPortraitBinding.Apply(enemy);
            }
        }

        private void RefreshPlayerEnergy(CombatUnit player)
        {
            if (playerEnergyValueText != null)
            {
                playerEnergyValueText.text = player != null ? player.Energy.ToString() : "-";
                playerEnergyValueText.fontSize = player != null && player.Energy >= 10 ? 36 : 44;
            }

            if (playerEnergyOrbImage == null)
            {
                return;
            }

            if (player == null || player.MaxEnergy <= 0)
            {
                playerEnergyOrbImage.color = new Color(0.25f, 0.3f, 0.4f, 0.95f);
                playerEnergyOrbImage.rectTransform.localScale = Vector3.one * 0.78f;
                return;
            }

            float ratio = Mathf.Clamp01(player.Energy / (float)player.MaxEnergy);
            Color emptyColor = new Color(0.25f, 0.3f, 0.4f, 0.95f);
            Color fullColor = new Color(0.13f, 0.8f, 0.95f, 0.98f);
            playerEnergyOrbImage.color = Color.Lerp(emptyColor, fullColor, ratio);
            float scale = Mathf.Lerp(0.78f, 1f, ratio);
            playerEnergyOrbImage.rectTransform.localScale = new Vector3(scale, scale, 1f);
        }

        private void HandleLog(string message)
        {
            latestLog = message;
            Refresh();
        }

        private void BindButtons()
        {
            SetButtonLabel(endActionButton, EndActionLabel);
            SetButtonLabel(moveTowardButton, MoveTowardLabel);
            SetButtonLabel(moveAwayButton, MoveAwayLabel);
            BindButton(endActionButton, HandleEndActionClick);
            BindButton(moveTowardButton, HandleMoveTowardClick);
            BindButton(moveAwayButton, HandleMoveAwayClick);
        }

        private void UnbindButtons()
        {
            UnbindButton(endActionButton, HandleEndActionClick);
            UnbindButton(moveTowardButton, HandleMoveTowardClick);
            UnbindButton(moveAwayButton, HandleMoveAwayClick);
        }

        private void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null || action == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void UnbindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null || action == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
        }

        private void SetButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            Text labelText = button.GetComponentInChildren<Text>();
            if (labelText != null)
            {
                labelText.text = label;
            }
        }

        private void HandleEndActionClick()
        {
            if (battleManager != null)
            {
                battleManager.EndPlayerAction();
            }
        }

        private void HandleMoveTowardClick()
        {
            if (battleManager != null)
            {
                battleManager.UsePlayerFreeMoveToward();
            }
        }

        private void HandleMoveAwayClick()
        {
            if (battleManager != null)
            {
                battleManager.UsePlayerFreeMoveAway();
            }
        }

        private void HandleUnitDamaged(CombatUnit unit, int amount)
        {
            if (unit == null || amount <= 0)
            {
                return;
            }

            if (unit.team == Team.Player)
            {
                ActivatePopup(ref playerPopup, playerDamagePopupText, amount);
            }
            else if (unit.team == Team.Enemy)
            {
                ActivatePopup(ref enemyPopup, enemyDamagePopupText, amount);
            }
        }

        private string BuildDistanceText(CombatUnit player, CombatUnit enemy)
        {
            if (battleManager == null || player == null || enemy == null)
            {
                return "距离：-";
            }

            float distance = battleManager.DistanceBetween(player, enemy);
            bool playerCanHit = distance <= player.AttackRange;
            bool enemyCanHit = distance <= enemy.AttackRange;
            string rangeState = playerCanHit && enemyCanHit
                ? "双方都可攻击"
                : playerCanHit
                    ? "玩家可攻击"
                    : enemyCanHit
                        ? "敌人可攻击"
                        : "双方都未进入攻击范围";

            return "距离 " + distance.ToString("0.0")
                + " / 我方攻击范围 " + player.AttackRange
                + " / 敌方攻击范围 " + enemy.AttackRange
                + " / " + rangeState;
        }

        private string BuildCardDetailText(CardDefinition card)
        {
            if (card == null)
            {
                return "将鼠标移到卡牌上方查看详情。";
            }

            CombatUnit user = battleManager != null ? battleManager.activeUnit : null;
            int damage = user != null ? card.GetDamage(user) : card.damage;
            int range = user != null ? card.GetRange(user) : card.rangeOverride;
            string targetLabel = GetTargetLabel(card.targetRule);
            string rangeLabel = range > 0 ? range.ToString() : "无";

            return "卡牌：" + card.displayName
                + "  费用：" + card.cost
                + "  目标：" + targetLabel
                + "  射程：" + rangeLabel
                + "  伤害：" + damage
                + "\n" + card.description;
        }

        private void UpdateArenaTrack(CombatUnit player, CombatUnit enemy)
        {
            if (battleManager == null || arenaTrack == null)
            {
                return;
            }

            if (player == null || enemy == null)
            {
                SetVisible(playerMarker, false);
                SetVisible(enemyMarker, false);
                SetVisible(playerRangeBar, false);
                SetVisible(enemyRangeBar, false);
                return;
            }

            float width = arenaTrack.rect.width;
            if (width <= 0f)
            {
                return;
            }

            float playerX = PositionToTrack(player.ArenaPosition, width);
            float enemyX = PositionToTrack(enemy.ArenaPosition, width);

            SetVisible(playerMarker, true);
            SetVisible(enemyMarker, true);
            SetVisible(playerRangeBar, true);
            SetVisible(enemyRangeBar, true);

            SetMarkerPosition(playerMarker, playerX);
            SetMarkerPosition(enemyMarker, enemyX);
            SetRangeBar(playerRangeBar, player, enemy, playerX, width);
            SetRangeBar(enemyRangeBar, enemy, player, enemyX, width);
        }

        private float PositionToTrack(float arenaPosition, float trackWidth)
        {
            if (Mathf.Approximately(battleManager.arenaMax, battleManager.arenaMin))
            {
                return 0f;
            }

            return Mathf.Lerp(0f, trackWidth, Mathf.InverseLerp(battleManager.arenaMin, battleManager.arenaMax, arenaPosition));
        }

        private void SetMarkerPosition(RectTransform marker, float x)
        {
            if (marker == null)
            {
                return;
            }

            marker.anchorMin = new Vector2(0f, 0.5f);
            marker.anchorMax = new Vector2(0f, 0.5f);
            marker.pivot = new Vector2(0.5f, 0.5f);
            marker.anchoredPosition = new Vector2(x, 0f);
        }

        private void SetRangeBar(RectTransform rangeBar, CombatUnit source, CombatUnit target, float sourceX, float trackWidth)
        {
            if (rangeBar == null || source == null || target == null)
            {
                return;
            }

            float pixelsPerUnit = trackWidth / Mathf.Max(0.01f, battleManager.arenaMax - battleManager.arenaMin);
            float fullWidth = source.AttackRange * pixelsPerUnit;
            bool targetOnRight = target.ArenaPosition >= source.ArenaPosition;
            float availableWidth = targetOnRight ? trackWidth - sourceX : sourceX;
            float clampedWidth = Mathf.Clamp(fullWidth, 0f, availableWidth);

            rangeBar.anchorMin = new Vector2(0f, 0.5f);
            rangeBar.anchorMax = new Vector2(0f, 0.5f);
            rangeBar.pivot = new Vector2(targetOnRight ? 0f : 1f, 0.5f);
            rangeBar.anchoredPosition = new Vector2(sourceX, 0f);
            rangeBar.sizeDelta = new Vector2(clampedWidth, rangeBar.sizeDelta.y);
        }

        private void SetVisible(RectTransform rect, bool visible)
        {
            if (rect != null)
            {
                rect.gameObject.SetActive(visible);
            }
        }

        private void ActivatePopup(ref PopupState popup, Text popupText, int amount)
        {
            if (popupText == null)
            {
                return;
            }

            popup.active = true;
            popup.timer = 0f;
            popup.basePosition = popupText.rectTransform.anchoredPosition;

            popupText.text = "-" + amount;
            popupText.color = new Color(1f, 0.2f, 0.2f, 1f);
            popupText.gameObject.SetActive(true);
            popupText.rectTransform.anchoredPosition = popup.basePosition;
        }

        private void InitializePopupText(ref PopupState popup, Text popupText)
        {
            if (popupText == null)
            {
                return;
            }

            popup.basePosition = popupText.rectTransform.anchoredPosition;
            popup.active = false;
            popup.timer = 0f;
            popupText.text = string.Empty;
            popupText.gameObject.SetActive(false);
        }

        private void UpdatePopup(ref PopupState popup, Text popupText)
        {
            if (!popup.active || popupText == null)
            {
                return;
            }

            popup.timer += Time.deltaTime;
            float t = Mathf.Clamp01(popup.timer / PopupDuration);
            popupText.rectTransform.anchoredPosition = popup.basePosition + new Vector2(0f, PopupRiseDistance * t);

            Color color = popupText.color;
            color.a = 1f - t;
            popupText.color = color;

            if (t >= 1f)
            {
                popup.active = false;
                popupText.gameObject.SetActive(false);
                popupText.rectTransform.anchoredPosition = popup.basePosition;
            }
        }

        private string GetStateLabel(BattleState state)
        {
            switch (state)
            {
                case BattleState.WaitingTimeline:
                    return "等待行动";
                case BattleState.PlayerActing:
                    return "玩家行动中";
                case BattleState.EnemyActing:
                    return "敌方行动中";
                case BattleState.Victory:
                    return "胜利";
                case BattleState.Defeat:
                    return "失败";
                default:
                    return state.ToString();
            }
        }

        private string GetTargetLabel(CardTargetRule targetRule)
        {
            switch (targetRule)
            {
                case CardTargetRule.Self:
                    return "自身";
                case CardTargetRule.SingleEnemy:
                    return "单体敌人";
                case CardTargetRule.AllEnemies:
                    return "全体敌人";
                default:
                    return targetRule.ToString();
            }
        }

        private void SubscribeToUnitDamage()
        {
            if (battleManager == null)
            {
                return;
            }

            for (int i = 0; i < battleManager.units.Count; i++)
            {
                CombatUnit unit = battleManager.units[i];
                if (unit != null)
                {
                    unit.Damaged += HandleUnitDamaged;
                }
            }
        }

        private void UnsubscribeFromUnitDamage()
        {
            if (battleManager == null)
            {
                return;
            }

            for (int i = 0; i < battleManager.units.Count; i++)
            {
                CombatUnit unit = battleManager.units[i];
                if (unit != null)
                {
                    unit.Damaged -= HandleUnitDamaged;
                }
            }
        }
    }
}
