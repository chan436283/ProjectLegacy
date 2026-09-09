using System;
using CWFramework;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BattleController))]
public sealed class BattleActionExecutor : CBehaviour
{
    [SerializeField]
    private BattleController battleController;

    public event Action<BattleAction> ActionStarted;
    public event Action<BattleAction> ActionCompleted;

    protected override void OnAwake()
    {
        if (battleController == null)
            battleController = GetComponent<BattleController>();
    }

    public bool CanExecute(BattleAction action)
    {
        if (action == null ||
            action.Actor == null ||
            !action.Actor.CanAct ||
            battleController.State != BattleState.WaitingForAction ||
            battleController.CurrentUnit != action.Actor)
        {
            return false;
        }

        return action.CommandType switch
        {
            BattleCommandType.Attack =>
                action.Skill == action.Actor.BasicAttackSkill &&
                CanExecuteSkill(action),
            BattleCommandType.Skill =>
                action.Actor.HasSkill(action.Skill) &&
                CanExecuteSkill(action),
            BattleCommandType.Defend =>
                action.Skill == action.Actor.DefendSkill && CanExecuteSkill(action),
            BattleCommandType.Item => false,
            _ => false
        };
    }

    public bool TryExecute(BattleAction action)
    {
        if (!CanExecute(action))
            return false;

        ActionStarted?.Invoke(action);

        float actionValueMultiplier = action.CommandType switch
        {
            BattleCommandType.Attack => ExecuteSkill(action),
            BattleCommandType.Skill => ExecuteSkill(action),
            BattleCommandType.Defend => ExecuteSkill(action),
            _ => throw new ArgumentOutOfRangeException()
        };

        ActionCompleted?.Invoke(action);
        battleController.CompleteCurrentTurn(actionValueMultiplier);
        return true;
    }

    private static bool CanExecuteSkill(BattleAction action)
    {
        return action.Skill != null &&
               action.Skill.CanUse(action.Actor) &&
               action.Skill.AreValidTargets(
                   action.Actor,
                   action.Targets);
    }

    private static float ExecuteSkill(BattleAction action)
    {
        if (!action.Actor.Stats.SpendMp(action.Skill.MpCost))
            throw new InvalidOperationException("스킬 MP가 부족합니다.");

        foreach (BattleEffect effect in action.Skill.Effects)
        {
            if (effect == null)
                continue;

            foreach (BattleUnit target in action.Targets)
                effect.Apply(action.Actor, target);
        }

        return action.Skill.ActionValueMultiplier;
    }

}
