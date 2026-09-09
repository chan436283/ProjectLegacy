using UnityEngine;

[System.Serializable]
public sealed class HealEffect : BattleEffect
{
    [SerializeField, Min(0f)]
    private float magicAttackPower;

    [SerializeField, Min(0f)]
    private float flatAmount;

    public override void Apply(
        BattleUnit actor,
        BattleUnit target)
    {
        float amount =
            actor.Stats.Battle.MagicAttack.Value * magicAttackPower +
            flatAmount;

        target.Stats.Heal(amount);
    }
}
