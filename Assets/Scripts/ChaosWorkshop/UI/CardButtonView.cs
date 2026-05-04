using UnityEngine;
using UnityEngine.UI;

namespace ChaosWorkshop
{
    [RequireComponent(typeof(Button))]
    public class CardButtonView : MonoBehaviour
    {
        public Text titleText;
        public Text descriptionText;
        public Text costText;

        private Button button;
        private CardDefinition card;
        private BattleManager battleManager;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);
        }

        public void Bind(CardDefinition cardDefinition, BattleManager manager)
        {
            card = cardDefinition;
            battleManager = manager;

            if (titleText != null)
            {
                titleText.text = card != null ? card.displayName : string.Empty;
            }

            if (descriptionText != null)
            {
                descriptionText.text = card != null ? card.description : string.Empty;
            }

            if (costText != null)
            {
                costText.text = card != null ? card.cost.ToString() : string.Empty;
            }

            RefreshInteractable();
        }

        public void RefreshInteractable()
        {
            if (button != null)
            {
                button.interactable = battleManager != null && card != null && battleManager.CanPlay(card);
            }
        }

        private void HandleClick()
        {
            if (battleManager != null && card != null)
            {
                battleManager.PlayCard(card);
            }
        }
    }
}
