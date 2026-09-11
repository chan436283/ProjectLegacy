using System;
using System.Collections.Generic;
using UnityEngine;

public enum AIActionConditionType
{
    None,
    HpRatioAtOrBelow,
    HpRatioAtOrAbove
}

[Serializable]
public sealed class AIActionEntry
{
    [SerializeField] private BattleSkill skill;
    [SerializeField] private AIActionConditionType condition;
    [SerializeField, Range(0f, 1f)] private float hpRatio = 0.5f;

    public BattleSkill Skill => skill;

    public bool MeetsCondition(BattleUnit actor)
    {
        if (actor == null) return false;
        return condition switch
        {
            AIActionConditionType.None => true,
            AIActionConditionType.HpRatioAtOrBelow => actor.Stats.HpRatio <= hpRatio,
            AIActionConditionType.HpRatioAtOrAbove => actor.Stats.HpRatio >= hpRatio,
            _ => false
        };
    }
}

[CreateAssetMenu(fileName = "AIActionSet", menuName = "ProjectLegacy/Battle/AI Action Set")]
public sealed class AIActionSet : ScriptableObject
{
    [SerializeField] private List<AIActionEntry> actions = new();
    public IReadOnlyList<AIActionEntry> Actions => actions;
}
