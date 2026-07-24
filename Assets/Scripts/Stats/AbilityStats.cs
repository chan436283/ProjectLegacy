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

    public StatValue Get(AbilityStatType type)
    {
        return type switch
        {
            AbilityStatType.Strength => Strength,
            AbilityStatType.Constitution => Constitution,
            AbilityStatType.Dexterity => Dexterity,
            AbilityStatType.Agility => Agility,
            AbilityStatType.Intelligence => Intelligence,
            AbilityStatType.Wisdom => Wisdom,
            AbilityStatType.Charisma => Charisma,
            AbilityStatType.Luck => Luck,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
