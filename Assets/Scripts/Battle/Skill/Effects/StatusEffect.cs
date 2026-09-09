using UnityEngine;

// Settings belong to the skill; each application creates a unit-owned status.
[System.Serializable]
public sealed class StatusEffect : BattleEffect
{
    [SerializeField] private string statusId = "defend";
    [SerializeField] private StatusEffectType statusEffectType;
    [SerializeField] private BattleStatType statType = BattleStatType.PhysicalAttack;
    [SerializeField] private StatModifierType modifierType = StatModifierType.AdditivePercent;
    [SerializeField] private float modifierValue = 0.2f;
    [SerializeField, Range(0f, 1f)] private float damageReduction = 0.5f;
    [Tooltip("대상의 턴 시작을 몇 번 맞으면 해제할지 설정합니다.")]
    [SerializeField, Min(1)] private int durationTurns = 1;

    public override void Apply(BattleUnit actor, BattleUnit target)
    {
        target.ApplyStatus(CreateStatus());
    }

    public BattleStatus CreateStatus()
    {
        return statusEffectType switch
        {
            StatusEffectType.DamageReduction => new DamageReductionStatus(
                statusId, damageReduction, durationTurns),
            StatusEffectType.StatModifier => new StatModifierStatus(
                statusId, statType, modifierType, modifierValue, durationTurns),
            _ => throw new System.ArgumentOutOfRangeException(nameof(statusEffectType))
        };
    }
}
