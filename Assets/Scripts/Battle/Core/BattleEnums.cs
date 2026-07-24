public enum BattleSide
{
    Ally,
    Enemy
}

public enum BattleControlType
{
    Player,
    AI
}

public enum BattleState
{
    Idle,
    RoundStarting,
    WaitingForAction,
    ResolvingAction,
    Victory,
    Defeat
}
