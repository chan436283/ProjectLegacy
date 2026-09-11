using System;

// Runtime state. Never stored on the shared skill asset.
public abstract class BattleStatus
{
    public string StatusId { get; }
    public int RemainingTurns { get; private set; }

    protected BattleStatus(string statusId, int durationTurns)
    {
        if (string.IsNullOrWhiteSpace(statusId))
            throw new ArgumentException("상태 ID가 필요합니다.", nameof(statusId));
        StatusId = statusId;
        RemainingTurns = Math.Max(1, durationTurns);
    }

    public bool AdvanceTurn()
    {
        RemainingTurns = Math.Max(0, RemainingTurns - 1);
        return RemainingTurns == 0;
    }

    public virtual bool BlocksAllActions => false;
    public virtual bool AllowsSkill(BattleUnit actor, BattleSkill skill) => true;

    public virtual void OnApply(CharacterStats stats) { }
    public virtual void OnRemove(CharacterStats stats) { }

    public virtual float ModifyIncomingDamage(float amount) => amount;
}
