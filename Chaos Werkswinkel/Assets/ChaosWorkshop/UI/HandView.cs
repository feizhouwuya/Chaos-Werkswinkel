using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChaosWorkshop
{
    public class HandView : MonoBehaviour
    {
        public BattleManager battleManager;
        public CardButtonView cardPrefab;
        public UICardArtworkLibrary cardArtworkLibrary = new UICardArtworkLibrary();
        public UIImageArtworkSlot[] artworkSlots = new UIImageArtworkSlot[0];

        private readonly List<CardButtonView> spawnedCards = new List<CardButtonView>();

        private void OnEnable()
        {
            ApplyArtwork();

            if (battleManager != null)
            {
                battleManager.BattleChanged += Refresh;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.BattleChanged -= Refresh;
            }
        }

        public void ApplyArtwork()
        {
            UIArtworkUtility.ApplySlots(artworkSlots);
            if (cardPrefab != null)
            {
                cardPrefab.SetArtworkLibrary(cardArtworkLibrary);
                cardPrefab.ApplyStaticArtwork();
            }
        }

        public void Refresh()
        {
            if (battleManager == null || cardPrefab == null)
            {
                return;
            }

            EnsureCardCount(battleManager.Deck.hand.Count);
            for (int i = 0; i < spawnedCards.Count; i++)
            {
                bool active = i < battleManager.Deck.hand.Count;
                spawnedCards[i].gameObject.SetActive(active);
                if (active)
                {
                    spawnedCards[i].SetArtworkLibrary(cardArtworkLibrary);
                    spawnedCards[i].Bind(battleManager.Deck.hand[i], battleManager);
                }
            }

            RectTransform rect = transform as RectTransform;
            if (rect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
        }

        private void EnsureCardCount(int count)
        {
            while (spawnedCards.Count < count)
            {
                CardButtonView view = Instantiate(cardPrefab, transform);
                view.SetArtworkLibrary(cardArtworkLibrary);
                view.ApplyStaticArtwork();
                view.gameObject.SetActive(false);
                spawnedCards.Add(view);
            }
        }
    }
}
