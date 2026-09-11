using UnityEngine;

[System.Serializable]
public sealed class DamageEffect : BattleEffect
{
    [SerializeField]
    private BattleDamageType damageType = BattleDamageType.Physical;

    [SerializeField, Min(0f)]
    private float power = 1f;

    [SerializeField, Min(0f)]
    private float flatAmount;

    public override void Apply(
        BattleUnit actor,
        BattleUnit target)
    {
        float sourceValue = damageType switch
        {
            BattleDamageType.Physical =>
                actor.Stats.Battle.PhysicalAttack.Value,
            BattleDamageType.Magical =>
                actor.Stats.Battle.MagicAttack.Value,
            BattleDamageType.Fixed => 0f,
            _ => throw new System.ArgumentOutOfRangeException()
        };

        target.ReceiveDamage(sourceValue * power + flatAmount, BattleDamageSource.DirectAttack);
    }
}
