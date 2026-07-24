using System;
using System.Collections.Generic;

[Serializable]
public sealed class StatValue
{
    public float BaseValue;

    [NonSerialized]
    private readonly List<StatModifier> modifiers = new();

    [NonSerialized]
    private bool isDirty = true;

    [NonSerialized]
    private float cachedValue;

    public float Value
    {
        get
        {
            if (isDirty)
            {
                cachedValue = CalculateFinalValue();
                isDirty = false;
            }

            return cachedValue;
        }
    }

    public StatValue(float baseValue = 0f)
    {
        BaseValue = baseValue;
    }

    public void SetBaseValue(float value)
    {
        if (Math.Abs(BaseValue - value) < 0.0001f)
            return;

        BaseValue = value;
        isDirty = true;
    }

    public void AddModifier(StatModifier modifier)
    {
        if (modifier == null)
            throw new ArgumentNullException(nameof(modifier));

        modifiers.Add(modifier);
        modifiers.Sort((a, b) => a.Order.CompareTo(b.Order));
        isDirty = true;
    }

    public bool RemoveModifier(StatModifier modifier)
    {
        bool removed = modifiers.Remove(modifier);

        if (removed)
            isDirty = true;

        return removed;
    }

    public int RemoveModifiersFromSource(string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            return 0;

        int removedCount = modifiers.RemoveAll(x => x.SourceId == sourceId);

        if (removedCount > 0)
            isDirty = true;

        return removedCount;
    }

    public void ClearModifiers()
    {
        if (modifiers.Count == 0)
            return;

        modifiers.Clear();
        isDirty = true;
    }

    private float CalculateFinalValue()
    {
        float flat = 0f;
        float additivePercent = 0f;
        float multiplicative = 1f;

        foreach (StatModifier modifier in modifiers)
        {
            switch (modifier.Type)
            {
                case StatModifierType.Flat:
                    flat += modifier.Value;
                    break;

                case StatModifierType.AdditivePercent:
                    additivePercent += modifier.Value;
                    break;

                case StatModifierType.MultiplicativePercent:
                    multiplicative *= 1f + modifier.Value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return (BaseValue + flat) * (1f + additivePercent) * multiplicative;
    }
}
