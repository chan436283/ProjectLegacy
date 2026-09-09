using System;
using System.Collections.Generic;

public sealed class BattleAction
{
    private static readonly IReadOnlyList<BattleUnit> NoTargets =
        Array.Empty<BattleUnit>();

    public BattleUnit Actor { get; }
    public BattleCommandType CommandType { get; }
    public BattleSkill Skill { get; }
    public IReadOnlyList<BattleUnit> Targets { get; }

    private BattleAction(
        BattleUnit actor,
        BattleCommandType commandType,
        BattleSkill skill,
        IReadOnlyList<BattleUnit> targets)
    {
        Actor = actor ?? throw new ArgumentNullException(nameof(actor));
        CommandType = commandType;
        Skill = skill;
        Targets = CopyTargets(targets);
    }

    public static BattleAction Attack(
        BattleUnit actor,
        BattleUnit target)
    {
        if (actor == null)
            throw new ArgumentNullException(nameof(actor));

        if (actor.BasicAttackSkill == null)
            throw new InvalidOperationException(
                $"{actor.name}에 기본 공격 스킬이 할당되지 않았습니다.");

        return new BattleAction(
            actor,
            BattleCommandType.Attack,
            actor.BasicAttackSkill,
            new[] { target });
    }

    public static BattleAction UseSkill(
        BattleUnit actor,
        BattleSkill skill,
        IReadOnlyList<BattleUnit> targets)
    {
        if (skill == null)
            throw new ArgumentNullException(nameof(skill));

        return new BattleAction(
            actor,
            BattleCommandType.Skill,
            skill,
            targets);
    }

    public static BattleAction Defend(BattleUnit actor)
    {
        if (actor == null)
            throw new ArgumentNullException(nameof(actor));
        if (actor.DefendSkill == null)
            throw new InvalidOperationException($"{actor.name}에 방어 스킬이 할당되지 않았습니다.");

        return new BattleAction(
            actor,
            BattleCommandType.Defend,
            actor.DefendSkill,
            new[] { actor });
    }

    private static IReadOnlyList<BattleUnit> CopyTargets(
        IReadOnlyList<BattleUnit> targets)
    {
        if (targets == null || targets.Count == 0)
            return NoTargets;

        BattleUnit[] copy = new BattleUnit[targets.Count];

        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null)
                throw new ArgumentException(
                    "행동 대상에는 null을 포함할 수 없습니다.",
                    nameof(targets));

            copy[i] = targets[i];
        }

        return copy;
    }
}
