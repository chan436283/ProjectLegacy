using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "BattleSkill",
    menuName = "ProjectLegacy/Battle/Skill")]
public sealed class BattleSkill : ScriptableObject
{
    [Header("Identity")]
    [SerializeField]
    private string skillId;

    [SerializeField]
    private string displayName;

    [SerializeField]
    private Sprite icon;

    [Header("Usage")]
    [SerializeField, Min(0f)]
    private float mpCost;

    [SerializeField]
    private BattleTargetType targetType = BattleTargetType.SingleEnemy;

    [SerializeField, Min(0.0001f)]
    private float actionValueMultiplier = 1f;

    [Header("Animation")]
    [SerializeField]
    private SkillAnimationSettings animation = new();

    [SerializeReference]
    private List<BattleEffect> effects = new();

    public SkillAnimationSettings Animation => animation;
    public string SkillId => skillId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public float MpCost => mpCost;
    public BattleTargetType TargetType => targetType;
    public float ActionValueMultiplier => actionValueMultiplier;
    public IReadOnlyList<BattleEffect> Effects => effects;

    public bool CanUse(BattleUnit actor)
    {
        return actor != null &&
               actor.CanAct &&
               actor.Stats.CurrentMp >= mpCost;
    }

    public bool AreValidTargets(
        BattleUnit actor,
        IReadOnlyList<BattleUnit> targets)
    {
        if (actor == null || targets == null)
            return false;

        if (targets.Count == 0)
            return false;

        bool requiresSingleTarget =
            targetType == BattleTargetType.Self ||
            targetType == BattleTargetType.SingleAlly ||
            targetType == BattleTargetType.SingleEnemy;

        if (requiresSingleTarget && targets.Count != 1)
        {
            return false;
        }

        foreach (BattleUnit target in targets)
        {
            if (!IsValidTarget(actor, target))
                return false;
        }

        return true;
    }

    public bool IsValidTarget(BattleUnit actor, BattleUnit target)
    {
        if (actor == null || target == null || !target.CanAct)
            return false;

        return targetType switch
        {
            BattleTargetType.Self => target == actor,
            BattleTargetType.SingleAlly => actor.IsAllyOf(target),
            BattleTargetType.AllAllies => actor.IsAllyOf(target),
            BattleTargetType.SingleEnemy => actor.IsEnemyOf(target),
            BattleTargetType.AllEnemies => actor.IsEnemyOf(target),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
