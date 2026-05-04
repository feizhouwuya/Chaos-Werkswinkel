using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChaosWorkshop
{
    [Serializable]
    public class StoryChoiceReward
    {
        public CardDefinition cardReward;
        public int healAmount;
        public int maxEnergyBonus;
    }

    [Serializable]
    public class StoryChoice
    {
        public string title;
        [TextArea(2, 6)] public string body;
        public StoryChoiceReward reward;
    }

    public class RunProgress : MonoBehaviour
    {
        public List<CardDefinition> deck = new List<CardDefinition>();
        public int battlesWon;

        public void AddReward(StoryChoice choice, CombatUnit player)
        {
            if (choice == null || choice.reward == null)
            {
                return;
            }

            if (choice.reward.cardReward != null)
            {
                deck.Add(choice.reward.cardReward);
            }

            if (player != null)
            {
                player.Heal(choice.reward.healAmount);
                player.maxEnergy += Mathf.Max(0, choice.reward.maxEnergyBonus);
            }
        }
    }
}
