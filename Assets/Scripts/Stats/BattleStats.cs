using System;

[Serializable]
public sealed class BattleStats
{
    public StatValue MaxHp = new();
    public StatValue MaxMp = new();

    public StatValue PhysicalAttack = new();
    public StatValue PhysicalDefense = new();

    public StatValue MagicAttack = new();
    public StatValue MagicResistance = new();

    public StatValue Accuracy = new();
    public StatValue Evasion = new();

    public StatValue CriticalChance = new();
    public StatValue CriticalDamage = new();

    public StatValue Speed = new();

    public StatValue Get(BattleStatType type)
    {
        return type switch
        {
            BattleStatType.MaxHp => MaxHp,
            BattleStatType.MaxMp => MaxMp,

            BattleStatType.PhysicalAttack => PhysicalAttack,
            BattleStatType.PhysicalDefense => PhysicalDefense,

            BattleStatType.MagicAttack => MagicAttack,
            BattleStatType.MagicResistance => MagicResistance,

            BattleStatType.Accuracy => Accuracy,
            BattleStatType.Evasion => Evasion,

            BattleStatType.CriticalChance => CriticalChance,
            BattleStatType.CriticalDamage => CriticalDamage,

            BattleStatType.Speed => Speed,

            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                null)
        };
    }
}
