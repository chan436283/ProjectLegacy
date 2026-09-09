using System;

public sealed class StatModifierStatus : BattleStatus
{
    private readonly BattleStatType statType;
    private readonly StatModifier modifier;
    private StatValue appliedStat;

    public StatModifierStatus(string statusId, BattleStatType statType,
        StatModifierType modifierType, float value, int durationTurns)
        : base(statusId, durationTurns)
    {
        if (!Enum.IsDefined(typeof(BattleStatType), statType))
            throw new ArgumentOutOfRangeException(nameof(statType));
        if (!Enum.IsDefined(typeof(StatModifierType), modifierType))
            throw new ArgumentOutOfRangeException(nameof(modifierType));
        if (float.IsNaN(value) || float.IsInfinity(value))
            throw new ArgumentOutOfRangeException(nameof(value));
        this.statType = statType;
        modifier = new StatModifier(value, modifierType, statusId);
    }

    public override void OnApply(CharacterStats stats)
    {
        appliedStat = stats.Battle.Get(statType);
        appliedStat.AddModifier(modifier);
        stats.RefreshResourceLimits();
    }

    public override void OnRemove(CharacterStats stats)
    {
        appliedStat?.RemoveModifier(modifier);
        appliedStat = null;
        stats.RefreshResourceLimits();
    }
}
