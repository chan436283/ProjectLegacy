using System;

[Serializable]
public sealed class PrimaryStats
{
    public StatValue Strength = new(10f);
    public StatValue Constitution = new(10f);
    public StatValue Dexterity = new(10f);
    public StatValue Agility = new(10f);
    public StatValue Intelligence = new(10f);
    public StatValue Wisdom = new(10f);
    public StatValue Charisma = new(10f);
    public StatValue Luck = new(10f);

    public StatValue Get(PrimaryStatType type)
    {
        return type switch
        {
            PrimaryStatType.Strength => Strength,
            PrimaryStatType.Constitution => Constitution,
            PrimaryStatType.Dexterity => Dexterity,
            PrimaryStatType.Agility => Agility,
            PrimaryStatType.Intelligence => Intelligence,
            PrimaryStatType.Wisdom => Wisdom,
            PrimaryStatType.Charisma => Charisma,
            PrimaryStatType.Luck => Luck,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
