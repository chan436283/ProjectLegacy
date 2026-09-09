using System;

public sealed class DamageReductionStatus : BattleStatus
{
    public float DamageReduction { get; }

    public DamageReductionStatus(string statusId, float damageReduction, int durationTurns)
        : base(statusId, durationTurns)
    {
        if (float.IsNaN(damageReduction) || damageReduction < 0f || damageReduction > 1f)
            throw new ArgumentOutOfRangeException(nameof(damageReduction));
        DamageReduction = damageReduction;
    }

    public override float ModifyIncomingDamage(float amount)
    {
        return amount * (1f - DamageReduction);
    }
}
