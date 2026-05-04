using System.Collections.Generic;
using UnityEngine;

namespace ChaosWorkshop
{
    public enum EnemyTier
    {
        Normal,
        Elite,
        Boss
    }

    public enum EnemyActionKind
    {
        MoveToward,
        MoveAway,
        Attack,
        Shield,
        TimelineControl,
        Mark,
        Pull,
        Knockback,
        Special
    }

    [System.Serializable]
    public class EnemyActionDefinition
    {
        public string id;
        public string displayName;
        public EnemyActionKind kind;
        public int damage;
        public int hits = 1;
        public int range;
        public float moveToward;
        public float moveAway;
        public int shield;
        public float timelineChangeSelf;
        public float timelineChangePlayer;
        public float pull;
        public float knockback;
        public int cooldown;
        [TextArea(1, 4)] public string intentText;
    }

    [CreateAssetMenu(menuName = "Chaos Workshop/Enemy Definition", fileName = "EnemyDefinition")]
    public class EnemyDefinition : ScriptableObject
    {
        public string enemyId = "rogue_puppet";
        public string displayName = "Rogue Puppet";
        public EnemyTier tier = EnemyTier.Normal;
        public int maxHealth = 28;
        public float speed = 2f;
        public int attackRange = 2;
        public int baseDamage = 6;
        public float initialDistance = 7f;
        public List<EnemyActionDefinition> actions = new List<EnemyActionDefinition>();
    }
}
