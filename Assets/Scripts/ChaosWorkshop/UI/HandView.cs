using System.Collections.Generic;
using UnityEngine;

namespace ChaosWorkshop
{
    public class HandView : MonoBehaviour
    {
        public BattleManager battleManager;
        public CardButtonView cardPrefab;

        private readonly List<CardButtonView> spawnedCards = new List<CardButtonView>();

        private void OnEnable()
        {
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

        public void Refresh()
        {
            if (battleManager == null || cardPrefab == null)
            {
                return;
            }

            for (int i = 0; i < spawnedCards.Count; i++)
            {
                if (spawnedCards[i] != null)
                {
                    Destroy(spawnedCards[i].gameObject);
                }
            }

            spawnedCards.Clear();

            for (int i = 0; i < battleManager.Deck.hand.Count; i++)
            {
                CardButtonView view = Instantiate(cardPrefab, transform);
                view.gameObject.SetActive(true);
                view.Bind(battleManager.Deck.hand[i], battleManager);
                spawnedCards.Add(view);
            }
        }
    }
}
