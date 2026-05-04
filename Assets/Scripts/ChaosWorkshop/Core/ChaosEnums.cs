namespace ChaosWorkshop
{
    public enum Team
    {
        Player,
        Enemy
    }

    public enum CharacterArchetype
    {
        Swordsman,
        Nun,
        Assassin,
        Enemy
    }

    public enum WeaponKind
    {
        LongSword,
        WarHammer,
        Dagger,
        HandCrossbow,
        Spear,
        Tachi,
        BattleAxe
    }

    public enum CardTargetRule
    {
        Self,
        SingleEnemy,
        AllEnemies
    }

    public enum MoveMode
    {
        None,
        TowardTarget,
        AwayFromTarget,
        FixedForward,
        FixedBackward
    }

    public enum CardEffectType
    {
        Damage,
        Move,
        ChangeSpeed,
        ChangeTimeline,
        GainEnergy,
        ApplyWeakness,
        DrawCards
    }

    public enum BattleState
    {
        WaitingTimeline,
        PlayerActing,
        EnemyActing,
        Victory,
        Defeat
    }
}
