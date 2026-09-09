using System;

[Serializable]
public abstract class BattleEffect
{
    public abstract void Apply(
        BattleUnit actor,
        BattleUnit target);
}
