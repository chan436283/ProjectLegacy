using System;
using System.Collections.Generic;
using System.Linq;
using CWFramework;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BattleController : CBehaviour
{
    private const float ActionValueEpsilon = 0.0001f;

    [Header("Action Value")]
    [SerializeField, Min(0.0001f)]
    private float actionValueConstant = 10000f;

    [SerializeField, Min(0.0001f)]
    private float firstRoundActionValue = 150f;

    [SerializeField, Min(0.0001f)]
    private float laterRoundActionValue = 150f;

    [SerializeField]
    private List<BattleUnit> participants = new();

    private readonly ActionValueTimeline timeline = new();

    public event Action BattleStarted;
    public event Action<int, float> RoundStarted;
    public event Action<BattleUnit> TurnStarted;
    public event Action<BattleUnit> TurnEnded;
    public event Action<BattleUnit> ParticipantJoined;
    public event Action<BattleState> BattleEnded;

    public BattleState State { get; private set; } = BattleState.Idle;
    public int RoundNumber { get; private set; }
    public float RemainingRoundActionValue { get; private set; }
    public float ElapsedActionValue { get; private set; }
    public BattleUnit CurrentUnit { get; private set; }
    public IReadOnlyList<BattleUnit> Participants => participants;

    public void StartBattle()
    {
        if (State != BattleState.Idle)
            throw new InvalidOperationException("이미 전투가 진행 중입니다.");

        RemoveInvalidAndDuplicateParticipants();

        if (participants.Count == 0)
            throw new InvalidOperationException("전투 참가자가 없습니다.");

        RoundNumber = 0;
        RemainingRoundActionValue = 0f;
        ElapsedActionValue = 0f;
        CurrentUnit = null;
        timeline.Initialize(participants, actionValueConstant);
        State = BattleState.RoundStarting;

        BattleStarted?.Invoke();

        if (!TryEndBattle())
        {
            BeginRound();
            BeginNextTurn();
        }
    }

    public void CompleteCurrentTurn()
    {
        if (State != BattleState.WaitingForAction || CurrentUnit == null)
            throw new InvalidOperationException("종료할 수 있는 유닛 턴이 없습니다.");

        State = BattleState.ResolvingAction;

        BattleUnit completedUnit = CurrentUnit;
        timeline.CompleteTurn(completedUnit);
        CurrentUnit = null;
        TurnEnded?.Invoke(completedUnit);

        if (!TryEndBattle())
            BeginNextTurn();
    }

    public void AddParticipant(
        BattleUnit unit,
        float initialActionRatio = 1f)
    {
        if (unit == null)
            throw new ArgumentNullException(nameof(unit));

        if (initialActionRatio < 0f)
            throw new ArgumentOutOfRangeException(
                nameof(initialActionRatio));

        if (State == BattleState.Victory ||
            State == BattleState.Defeat)
        {
            throw new InvalidOperationException(
                "종료된 전투에는 참가자를 추가할 수 없습니다.");
        }

        if (participants.Contains(unit))
            return;

        participants.Add(unit);

        if (State != BattleState.Idle)
            timeline.AddUnit(unit, initialActionRatio);

        ParticipantJoined?.Invoke(unit);
    }

    public bool RemoveParticipant(BattleUnit unit)
    {
        if (State != BattleState.Idle)
            throw new InvalidOperationException("전투 도중에는 참가자를 제거할 수 없습니다.");

        return participants.Remove(unit);
    }

    public float GetRemainingActionValue(BattleUnit unit)
    {
        return timeline.GetRemainingActionValue(unit);
    }

    public void AdvanceAction(BattleUnit unit, float ratio)
    {
        timeline.AdvanceAction(unit, ratio);
    }

    public void DelayAction(BattleUnit unit, float ratio)
    {
        timeline.DelayAction(unit, ratio);
    }

    private void BeginRound()
    {
        State = BattleState.RoundStarting;
        RoundNumber++;
        RemainingRoundActionValue = RoundNumber == 1
            ? firstRoundActionValue
            : laterRoundActionValue;

        RoundStarted?.Invoke(RoundNumber, RemainingRoundActionValue);
    }

    private void BeginNextTurn()
    {
        while (timeline.TryGetNext(
                   out BattleUnit nextUnit,
                   out float actionValueUntilTurn))
        {
            if (RemainingRoundActionValue <= ActionValueEpsilon &&
                actionValueUntilTurn > ActionValueEpsilon)
            {
                BeginRound();
                continue;
            }

            if (actionValueUntilTurn - RemainingRoundActionValue >
                ActionValueEpsilon)
            {
                AdvanceTimeline(RemainingRoundActionValue);
                BeginRound();
                continue;
            }

            AdvanceTimeline(actionValueUntilTurn);

            if (!nextUnit.CanAct)
                continue;

            CurrentUnit = nextUnit;
            State = BattleState.WaitingForAction;
            TurnStarted?.Invoke(nextUnit);
            return;
        }

        TryEndBattle();
    }

    private void AdvanceTimeline(float actionValue)
    {
        timeline.Advance(actionValue);
        RemainingRoundActionValue = Math.Max(
            0f,
            RemainingRoundActionValue - actionValue);
        ElapsedActionValue += actionValue;
    }

    private bool TryEndBattle()
    {
        bool hasLivingAlly = participants.Any(
            unit => unit != null &&
                    unit.Side == BattleSide.Ally &&
                    !unit.Stats.IsDead);

        bool hasLivingEnemy = participants.Any(
            unit => unit != null &&
                    unit.Side == BattleSide.Enemy &&
                    !unit.Stats.IsDead);

        if (hasLivingAlly && hasLivingEnemy)
            return false;

        State = hasLivingAlly
            ? BattleState.Victory
            : BattleState.Defeat;

        CurrentUnit = null;
        BattleEnded?.Invoke(State);
        return true;
    }

    private void RemoveInvalidAndDuplicateParticipants()
    {
        HashSet<BattleUnit> uniqueUnits = new();
        participants.RemoveAll(unit =>
            unit == null || !uniqueUnits.Add(unit));
    }
}
