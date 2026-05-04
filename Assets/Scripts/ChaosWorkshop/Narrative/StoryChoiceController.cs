using System.Collections.Generic;
using UnityEngine;

namespace ChaosWorkshop
{
    public class StoryChoiceController : MonoBehaviour
    {
        public RunProgress runProgress;
        public CombatUnit player;
        public List<StoryChoice> choices = new List<StoryChoice>();

        public void Choose(int index)
        {
            if (index < 0 || index >= choices.Count)
            {
                Debug.LogWarning("Story choice index out of range.");
                return;
            }

            if (runProgress != null)
            {
                runProgress.AddReward(choices[index], player);
            }
        }
    }
}
