using System;
using System.Collections;
using System.Collections.Generic;
using CWFramework;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BattleController))]
public sealed class BattleActionExecutor : CBehaviour
{
    [SerializeField]
    private BattleController battleController;

    private Coroutine execution;
    private BattleAction pendingTurn;
    private SkillExecutionContext activeContext;

    public bool IsExecuting { get; private set; }

    public event Action<BattleAction> ActionStarted;
    public event Action<BattleAction> ActionCompleted;

    protected override void OnAwake()
    {
        if (battleController == null)
            battleController = GetComponent<BattleController>();
    }

    public bool CanExecute(BattleAction action)
    {
        if (!isActiveAndEnabled || IsExecuting || action == null ||
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

        IsExecuting = true;
        battleController.BeginAction(action.Actor);
        pendingTurn = action;
        execution = StartCoroutine(ExecuteAction(action));
        return true;
    }

    private IEnumerator ExecuteAction(BattleAction action)
    {
        var context = new SkillExecutionContext(action);
        activeContext = context;
        var routines = new Stack<IEnumerator>();
        routines.Push(ExecuteSequence(context));
        bool succeeded = true;
        try
        {
            // Drive nested steps here so exceptions cannot leave the turn resolving forever.
            while (routines.Count > 0)
            {
                IEnumerator current = routines.Peek();
                bool next = false;
                object yielded = null;
                try
                {
                    next = current.MoveNext();
                    if (next) yielded = current.Current;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                    succeeded = false;
                }
                if (!succeeded) break;
                if (!next)
                {
                    (routines.Pop() as IDisposable)?.Dispose();
                    continue;
                }
                if (yielded is IEnumerator nested)
                    routines.Push(nested);
                else
                    yield return yielded;
            }
        }
        finally
        {
            while (routines.Count > 0)
                (routines.Pop() as IDisposable)?.Dispose();
            context.RestorePosition();
            activeContext = null;
            IsExecuting = false;
        }

        try
        {
            if (succeeded) ActionCompleted?.Invoke(action);
        }
        finally
        {
            CompletePendingTurn();
        }
    }

    private IEnumerator ExecuteSequence(SkillExecutionContext context)
    {
        BattleAction action = context.Action;
        
        if (!action.Actor.Stats.SpendMp(action.Skill.MpCost))
            throw new InvalidOperationException("스킬 MP가 부족합니다.");

        ActionStarted?.Invoke(action);
        // Copy the list: editing the asset during playback must not invalidate iteration.
        var steps = new List<SkillSequenceStep>(action.Skill.Sequence);
        foreach (SkillSequenceStep step in steps)
        {
            if (context.Action.Actor == null || !context.Action.Actor.CanAct)
                throw new InvalidOperationException("시전자가 더 이상 행동할 수 없습니다.");
            if (step == null)
                throw new InvalidOperationException("스킬 시퀀스에 비어 있는 단계가 있습니다.");
            yield return step.Execute(context);
        }
    }

    protected override void OnDisabled()
    {
        if (execution != null) StopCoroutine(execution);
        execution = null;
        // Restore explicitly as well: stopping a Unity coroutine may not dispose its iterator.
        activeContext?.RestorePosition();
        activeContext = null;
        IsExecuting = false;
        CompletePendingTurn();
    }

    protected override void OnEnabled()
    {
        // If the entire battle object was disabled, finish the interrupted turn on resume.
        if (!IsExecuting) CompletePendingTurn();
    }

    private void CompletePendingTurn()
    {
        if (pendingTurn == null || battleController == null || !battleController.isActiveAndEnabled)
            return;
        BattleAction completed = pendingTurn;
        pendingTurn = null;
        if (battleController.State == BattleState.ResolvingAction &&
            battleController.CurrentUnit == completed.Actor)
            battleController.CompleteCurrentTurn(completed.Skill.ActionValueMultiplier);
    }

    private static bool CanExecuteSkill(BattleAction action)
    {
        return action.Skill != null &&
               action.Skill.Sequence != null && action.Skill.Sequence.Count > 0 &&
               action.Skill.CanUse(action.Actor) &&
               action.Skill.AreValidTargets(
                   action.Actor,
                   action.Targets);
    }

}
