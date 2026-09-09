using System;
using System.Collections.Generic;
using System.Linq;
using CWFramework;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BattleController : CBehaviour
{
    private const float ActionValueEpsilon = 0.0001f;
    private const int MaxAllyPartySize = 8;

    [Header("Battle Flow")]
    [SerializeField]
    private bool startAutomatically = true;

    [Header("Action Value")]
    [SerializeField, Min(0.0001f)]
    private float actionValueConstant = 10000f;

    [SerializeField, Min(0.0001f)]
    private float firstRoundActionValue = 150f;

    [SerializeField, Min(0.0001f)]
    private float laterRoundActionValue = 150f;

    [SerializeField]
    private List<BattleUnit> participants = new();

    [Header("Ally Status UI")]
    [SerializeField]
    private List<BattleUnitStatusView> allyStatusViews = new();

    private readonly ActionValueTimeline timeline = new();

    public event Action BattleStarted;
    public event Action<int, float> RoundStarted;
    public event Action<BattleUnit> TurnStarted;
    public event Action<BattleUnit> TurnEnded;
    public event Action<BattleUnit> ParticipantJoined;
    public event Action<BattleUnit> PlayerActionRequested;
    public event Action<BattleUnit> AIActionRequested;
    public event Action<BattleState> BattleEnded;

    public BattleState State { get; private set; } = BattleState.Idle;
    public int RoundNumber { get; private set; }
    public float RemainingRoundActionValue { get; private set; }
    public float ElapsedActionValue { get; private set; }
    public BattleUnit CurrentUnit { get; private set; }
    public IReadOnlyList<BattleUnit> Participants => participants;

    protected override void OnStarted()
    {
        if (startAutomatically)
            StartBattle();
    }

    public void StartBattle()
    {
        if (State != BattleState.Idle)
            throw new InvalidOperationException("이미 전투가 진행 중입니다.");

        RemoveInvalidAndDuplicateParticipants();

        if (participants.Count == 0)
            throw new InvalidOperationException("전투 참가자가 없습니다.");

        RefreshAllyStatusViews();

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

    public void CompleteCurrentTurn(float actionValueMultiplier = 1f)
    {
        if (State != BattleState.WaitingForAction || CurrentUnit == null)
            throw new InvalidOperationException("종료할 수 있는 유닛 턴이 없습니다.");

        if (actionValueMultiplier <= 0f)
            throw new ArgumentOutOfRangeException(
                nameof(actionValueMultiplier));

        State = BattleState.ResolvingAction;

        BattleUnit completedUnit = CurrentUnit;
        timeline.CompleteTurn(completedUnit, actionValueMultiplier);
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

        if (unit.Side == BattleSide.Ally)
            RefreshAllyStatusViews();

        ParticipantJoined?.Invoke(unit);
    }

    public bool RemoveParticipant(BattleUnit unit)
    {
        if (State != BattleState.Idle)
            throw new InvalidOperationException("전투 도중에는 참가자를 제거할 수 없습니다.");

        bool removed = participants.Remove(unit);

        if (removed && unit != null && unit.Side == BattleSide.Ally)
            RefreshAllyStatusViews();

        return removed;
    }

    public void RefreshAllyStatusViews()
    {
        int allyIndex = 0;
        int availableViewCount = Math.Min(
            allyStatusViews.Count,
            MaxAllyPartySize);

        foreach (BattleUnit unit in participants)
        {
            if (unit == null || unit.Side != BattleSide.Ally)
                continue;

            if (allyIndex < availableViewCount)
                ShowAllyStatusView(allyIndex, unit);

            allyIndex++;
        }

        int usedViewCount = Math.Min(allyIndex, availableViewCount);

        for (int i = usedViewCount; i < allyStatusViews.Count; i++)
            HideAllyStatusView(i);

        if (allyIndex > MaxAllyPartySize)
        {
            Debug.LogWarning(
                $"아군 참가자가 최대 파티 인원인 {MaxAllyPartySize}명을 초과했습니다.",
                this);
        }
        else if (allyIndex > availableViewCount)
        {
            Debug.LogWarning(
                $"아군 상태 UI가 부족합니다. 필요: {allyIndex}, 할당: {availableViewCount}",
                this);
        }
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

            nextUnit.BeginTurn();
            CurrentUnit = nextUnit;
            State = BattleState.WaitingForAction;
            TurnStarted?.Invoke(nextUnit);

            if (CurrentUnit == nextUnit &&
                State == BattleState.WaitingForAction)
            {
                RequestAction(nextUnit);
            }

            return;
        }

        TryEndBattle();
    }

    private void RequestAction(BattleUnit unit)
    {
        switch (unit.ControlType)
        {
            case BattleControlType.Player:
                RequestPlayerAction(unit);
                break;

            case BattleControlType.AI:
                RequestAIAction(unit);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(unit.ControlType),
                    unit.ControlType,
                    null);
        }
    }

    private void RequestPlayerAction(BattleUnit unit)
    {
        if (PlayerActionRequested == null)
        {
            Debug.LogWarning(
                $"{unit.name}의 플레이어 행동 요청을 처리할 대상이 없습니다.",
                unit);
            return;
        }

        PlayerActionRequested.Invoke(unit);
    }

    private void RequestAIAction(BattleUnit unit)
    {
        if (AIActionRequested == null)
        {
            Debug.LogWarning(
                $"{unit.name}의 AI 행동 요청을 처리할 대상이 없습니다.",
                unit);
            return;
        }

        AIActionRequested.Invoke(unit);
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

    private void ShowAllyStatusView(int index, BattleUnit unit)
    {
        BattleUnitStatusView view = allyStatusViews[index];

        if (view == null)
            return;

        view.SetUnit(unit);
        view.SetActive(true);
    }

    private void HideAllyStatusView(int index)
    {
        BattleUnitStatusView view = allyStatusViews[index];

        if (view == null)
            return;

        view.SetUnit(null);
        view.SetActive(false);
    }
}
