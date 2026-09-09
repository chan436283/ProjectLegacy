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

public enum BattleCommandType
{
    Attack,
    Skill,
    Defend,
    Item
}

public enum BattleTargetType
{
    Self,
    SingleAlly,
    AllAllies,
    SingleEnemy,
    AllEnemies
}

public enum BattleDamageType
{
    Physical,
    Magical,
    Fixed
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
