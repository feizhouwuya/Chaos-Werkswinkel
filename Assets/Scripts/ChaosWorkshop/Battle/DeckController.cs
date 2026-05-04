using System.Collections.Generic;
using UnityEngine;

namespace ChaosWorkshop
{
    public class DeckController : MonoBehaviour
    {
        public List<CardDefinition> drawPile = new List<CardDefinition>();
        public List<CardDefinition> discardPile = new List<CardDefinition>();
        public List<CardDefinition> hand = new List<CardDefinition>();
        public int maxHandSize = 7;

        public void BuildDeck(IEnumerable<CardDefinition> cards)
        {
            drawPile.Clear();
            discardPile.Clear();
            hand.Clear();
            drawPile.AddRange(cards);
            Shuffle(drawPile);
        }

        public void Draw(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (hand.Count >= maxHandSize)
                {
                    return;
                }

                if (drawPile.Count == 0)
                {
                    ReshuffleDiscardIntoDraw();
                }

                if (drawPile.Count == 0)
                {
                    return;
                }

                CardDefinition card = drawPile[0];
                drawPile.RemoveAt(0);
                hand.Add(card);
            }
        }

        public void Discard(CardDefinition card)
        {
            if (hand.Remove(card))
            {
                discardPile.Add(card);
            }
        }

        public void DiscardHand()
        {
            discardPile.AddRange(hand);
            hand.Clear();
        }

        private void ReshuffleDiscardIntoDraw()
        {
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            Shuffle(drawPile);
        }

        private static void Shuffle<T>(IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int swapIndex = Random.Range(i, list.Count);
                T temp = list[i];
                list[i] = list[swapIndex];
                list[swapIndex] = temp;
            }
        }
    }
}
