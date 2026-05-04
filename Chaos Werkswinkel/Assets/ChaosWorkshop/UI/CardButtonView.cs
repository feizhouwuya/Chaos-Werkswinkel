using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChaosWorkshop
{
    [RequireComponent(typeof(Button))]
    public class CardButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Text")]
        public Text titleText;
        public Text descriptionText;
        public Text costText;

        [Header("Images")]
        public Image backgroundImage;
        public Image illustrationImage;
        public Image frameImage;
        public Image costBadgeImage;
        public UIImageArtworkSlot[] artworkSlots = new UIImageArtworkSlot[0];
        public UICardImageBinding illustrationBinding = new UICardImageBinding();
        public UICardImageBinding backgroundBinding = new UICardImageBinding();
        public UICardImageBinding frameBinding = new UICardImageBinding();
        public UICardImageBinding costBadgeBinding = new UICardImageBinding();

        private Button button;
        private CardDefinition card;
        private BattleManager battleManager;
        private UICardArtworkLibrary runtimeArtworkLibrary;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);
            EnsureBindingTargets();
            ApplyStaticArtwork();
        }

        public void SetArtworkLibrary(UICardArtworkLibrary artworkLibrary)
        {
            runtimeArtworkLibrary = artworkLibrary;
            RefreshArtwork();
        }

        public void ApplyStaticArtwork()
        {
            EnsureBindingTargets();
            UIArtworkUtility.ApplySlots(artworkSlots);
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

            RefreshArtwork();
            RefreshInteractable();
        }

        public void RefreshInteractable()
        {
            if (button != null)
            {
                button.interactable = battleManager != null && card != null && battleManager.CanPlay(card);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (battleManager != null && card != null)
            {
                battleManager.SetHoveredCard(card);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (battleManager != null && battleManager.HoveredCard == card)
            {
                battleManager.SetHoveredCard(null);
            }
        }

        private void HandleClick()
        {
            if (battleManager != null && card != null)
            {
                battleManager.SetHoveredCard(card);
                battleManager.PlayCard(card);
            }
        }

        private void EnsureBindingTargets()
        {
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
            }

            if (illustrationBinding != null && illustrationBinding.targetImage == null)
            {
                illustrationBinding.targetImage = illustrationImage;
            }

            if (backgroundBinding != null && backgroundBinding.targetImage == null)
            {
                backgroundBinding.targetImage = backgroundImage;
            }

            if (frameBinding != null && frameBinding.targetImage == null)
            {
                frameBinding.targetImage = frameImage;
            }

            if (costBadgeBinding != null && costBadgeBinding.targetImage == null)
            {
                costBadgeBinding.targetImage = costBadgeImage;
            }
        }

        private void RefreshArtwork()
        {
            EnsureBindingTargets();

            if (backgroundBinding != null)
            {
                backgroundBinding.Apply(card, runtimeArtworkLibrary);
            }

            if (illustrationBinding != null)
            {
                illustrationBinding.Apply(card, runtimeArtworkLibrary);
            }

            if (frameBinding != null)
            {
                frameBinding.Apply(card, runtimeArtworkLibrary);
            }

            if (costBadgeBinding != null)
            {
                costBadgeBinding.Apply(card, runtimeArtworkLibrary);
            }
        }
    }
}
