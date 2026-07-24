using System;

public enum StatModifierType
{
    Flat,
    AdditivePercent,
    MultiplicativePercent
}

[Serializable]
public sealed class StatModifier
{
    public float Value;
    public StatModifierType Type;
    public int Order;
    public string SourceId;

    public StatModifier(
        float value,
        StatModifierType type,
        string sourceId,
        int order = 0)
    {
        Value = value;
        Type = type;
        SourceId = sourceId;
        Order = order;
    }
}
