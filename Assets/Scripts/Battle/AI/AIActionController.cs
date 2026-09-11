using System.Collections;
using System.Collections.Generic;
using CWFramework;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BattleActionExecutor))]
public sealed class AIActionController : CBehaviour
{
    private BattleController battleController;
    private BattleActionExecutor executor;
    private BattleUnit requestedUnit;

    protected override void OnAwake()
    {
        battleController = GetComponent<BattleController>();
        executor = GetComponent<BattleActionExecutor>();
    }

    protected override void OnEnabled()
    {
        battleController.AIActionRequested += OnActionRequested;
        battleController.BattleStarted += ResetPatterns;
        battleController.ParticipantJoined += ResetPattern;
        if (battleController.State == BattleState.WaitingForAction)
            OnActionRequested(battleController.CurrentUnit);
    }

    protected override void OnDisabled()
    {
        battleController.AIActionRequested -= OnActionRequested;
        battleController.BattleStarted -= ResetPatterns;
        battleController.ParticipantJoined -= ResetPattern;
        StopAllCoroutines();
        requestedUnit = null;
    }

    private void ResetPatterns()
    {
        foreach (BattleUnit unit in battleController.Participants) ResetPattern(unit);
    }

    private void ResetPattern(BattleUnit unit)
    {
        if (unit != null) unit.ResetAIActionPattern();
    }

    private void OnActionRequested(BattleUnit unit)
    {
        if (unit == null || unit.ControlType != BattleControlType.AI || requestedUnit != null) return;
        requestedUnit = unit;
        StartCoroutine(ResolveRequest(unit));
    }

    private IEnumerator ResolveRequest(BattleUnit actor)
    {
        // Break synchronous turn chaining, including battles where everyone must pass.
        yield return null;
        requestedUnit = null;
        if (battleController.State != BattleState.WaitingForAction ||
            battleController.CurrentUnit != actor || actor == null ||
            actor.ControlType != BattleControlType.AI) yield break;

        AIActionPattern pattern = actor.AIActionPattern;
        BattleAction action = null;
        if (actor.CanPerformActions && pattern.TrySelect(entry =>
            {
                if (!entry.MeetsCondition(actor)) return false;
                action = CreateAction(actor, entry.Skill);
                return executor.CanExecute(action);
            }, out int index))
        {
            // TryExecute can complete synchronously; commit before it emits turn events.
            int previousIndex = pattern.CurrentActionIndex;
            AIActionSet previousSet = pattern.ActionSet;
            pattern.Commit(index);
            if (executor.TryExecute(action)) yield break;
            if (pattern.ActionSet == previousSet)
            {
                pattern.SetActionSet(previousSet);
                if (previousIndex >= 0) pattern.Commit(previousIndex);
            }
        }

        // No eligible action: preserve the cursor and spend this turn without a skill.
        battleController.BeginAction(actor);
        battleController.CompleteCurrentTurn();
    }

    private BattleAction CreateAction(BattleUnit actor, BattleSkill skill)
    {
        if (skill == null || !skill.CanUse(actor)) return null;
        var targets = new List<BattleUnit>();
        if (skill.TargetType == BattleTargetType.Self)
            targets.Add(actor);
        else
        {
            foreach (BattleUnit candidate in battleController.Participants)
                if (skill.IsValidTarget(actor, candidate)) targets.Add(candidate);
            if (targets.Count == 0) return null;
            if (skill.TargetType == BattleTargetType.SingleEnemy)
                targets = new List<BattleUnit> { targets[Random.Range(0, targets.Count)] };
            else if (skill.TargetType == BattleTargetType.SingleAlly)
            {
                BattleUnit target = targets[0];
                foreach (BattleUnit candidate in targets)
                    if (candidate.Stats.HpRatio < target.Stats.HpRatio) target = candidate;
                targets = new List<BattleUnit> { target };
            }
        }
        if (skill == actor.BasicAttackSkill && targets.Count == 1)
            return BattleAction.Attack(actor, targets[0]);
        if (skill == actor.DefendSkill && targets.Count == 1 && targets[0] == actor)
            return BattleAction.Defend(actor);
        return BattleAction.UseSkill(actor, skill, targets);
    }
}
