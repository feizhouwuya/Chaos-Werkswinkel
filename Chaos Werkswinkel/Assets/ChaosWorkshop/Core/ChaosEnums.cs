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
        TachiHeavy,
        TachiFast,
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
        DrawCards,
        GainShield,
        ApplyBleed,
        GainEvasion,
        RecoverAmmo
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
