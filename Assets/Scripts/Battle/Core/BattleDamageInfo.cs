public enum BattleDamageSource
{
    DirectAttack,
    StatusEffect,
    Other
}

public readonly struct BattleDamageInfo
{
    public float AppliedAmount { get; }
    public BattleDamageSource Source { get; }
    public bool WasDefending { get; }
    public bool IsDead { get; }
    public bool ShouldPlayHitReaction => AppliedAmount > 0f &&
        Source == BattleDamageSource.DirectAttack && !WasDefending && !IsDead;

    public BattleDamageInfo(float appliedAmount, BattleDamageSource source, bool wasDefending, bool isDead)
    {
        AppliedAmount = appliedAmount;
        Source = source;
        WasDefending = wasDefending;
        IsDead = isDead;
    }
}
