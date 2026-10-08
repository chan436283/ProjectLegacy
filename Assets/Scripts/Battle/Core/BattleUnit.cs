using System;
using System.Collections.Generic;
using CWFramework;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterStatsComponent))]
public sealed class BattleUnit : CBehaviour
{
    [SerializeField]
    private BattleSide side;

    [SerializeField]
    private BattleControlType controlType;

    [SerializeField]
    private CharacterStatsComponent statsComponent;

    [SerializeField]
    private BattleUnitView view;

    [Header("Formation")]
    [SerializeField] private BattleRow row = BattleRow.Front;
    [Tooltip("열 안의 자리 번호입니다. 화면 위에서 아래 순서로 1부터 시작합니다.")]
    [SerializeField, Min(1)] private int formationPosition = 1;

    [Header("Battle Actions")]
    [SerializeField]
    private BattleSkill basicAttackSkill;

    [SerializeField]
    private List<BattleSkill> skills = new();

    [SerializeField]
    private BattleSkill defendSkill;

    [Header("AI Action Patterns")]
    [SerializeField] private List<AIActionSet> aiActionSets = new();
    [SerializeField, Min(0)] private int initialAIActionSetIndex;

    public AIActionPattern AIActionPattern { get; } = new();
    public IReadOnlyList<AIActionSet> AIActionSets => aiActionSets;

    public void SetAIActionSet(int index)
    {
        if (index < 0 || index >= aiActionSets.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        AIActionPattern.SetActionSet(aiActionSets[index]);
    }

    public void ResetAIActionPattern()
    {
        AIActionPattern.SetActionSet(initialAIActionSetIndex >= 0 &&
            initialAIActionSetIndex < aiActionSets.Count ? aiActionSets[initialAIActionSetIndex] : null);
    }

    // Keep CanAct as timeline/target eligibility; action restrictions must not hide living targets.
    public bool CanPerformActions => CanAct && !statuses.Exists(status => status.BlocksAllActions);

    public bool CanUseSkill(BattleSkill skill)
    {
        return skill != null && CanPerformActions &&
            !statuses.Exists(status => !status.AllowsSkill(this, skill));
    }

    private readonly List<BattleStatus> statuses = new();

    public event Action StatusesChanged;
    public event Action<BattleDamageInfo> DamageReceived;
    public bool IsDefending => HasStatus("defend");

    public bool HasStatus(string statusId) => statuses.Exists(status => status.StatusId == statusId);

    public BattleSide Side => side;
    public BattleRow Row => row;
    public int FormationPosition => formationPosition;
    public BattleControlType ControlType => controlType;
    public CharacterStats Stats => statsComponent.Stats;
    public BattleUnitView View => view;
    public BattleSkill BasicAttackSkill => basicAttackSkill;
    public IReadOnlyList<BattleSkill> Skills => skills;
    public BattleSkill DefendSkill => defendSkill;
    public IReadOnlyList<BattleStatus> Statuses => statuses.AsReadOnly();
    public bool CanAct => !Stats.IsDead;
    public float Speed => Stats.Battle.Speed.Value;

    public bool IsAllyOf(BattleUnit other)
    {
        return other != null && side == other.side;
    }

    public bool IsEnemyOf(BattleUnit other)
    {
        return other != null && side != other.side;
    }

    public void SetControlType(BattleControlType value)
    {
        controlType = value;
    }

    /// <summary>논리적인 자리를 설정합니다. 화면 배치는 BattleFormation에서 수행합니다.</summary>
    public void SetFormationPosition(BattleRow value, int position)
    {
        if (!Enum.IsDefined(typeof(BattleRow), value))
            throw new ArgumentOutOfRangeException(nameof(value));
        if (position < 1)
            throw new ArgumentOutOfRangeException(nameof(position));
        row = value;
        formationPosition = position;
    }

    public bool HasSkill(BattleSkill skill)
    {
        return skill != null && skills.Contains(skill);
    }

    public void BeginTurn()
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            if (!statuses[i].AdvanceTurn()) continue;
            BattleStatus expired = statuses[i];
            statuses.RemoveAt(i);
            expired.OnRemove(Stats);
        }
        StatusesChanged?.Invoke();
    }

    public void ApplyStatus(BattleStatus status)
    {
        if (status == null)
            throw new ArgumentNullException(nameof(status));

        // Reapplying the same ID replaces its strength and refreshes its duration.
        int index = statuses.FindIndex(active => active.StatusId == status.StatusId);
        if (index >= 0)
        {
            BattleStatus previous = statuses[index];
            statuses[index] = status;
            // Apply first so refreshing a maximum-resource buff does not temporarily clamp HP/MP.
            status.OnApply(Stats);
            previous.OnRemove(Stats);
        }
        else
        {
            statuses.Add(status);
            status.OnApply(Stats);
        }
        StatusesChanged?.Invoke();
    }

    /// <summary>전투 종료 시 일시적 효과를 제거합니다. HP/MP를 회복하거나 부활시키지 않습니다.</summary>
    public void ClearBattleStatuses()
    {
        foreach (var status in statuses) status.OnRemove(Stats);
        statuses.Clear();
        StatusesChanged?.Invoke();
    }

    public float ReceiveDamage(float amount, BattleDamageSource source = BattleDamageSource.DirectAttack)
    {
        bool wasDefending = IsDefending;
        float finalAmount = amount;
        foreach (BattleStatus status in statuses)
            finalAmount = status.ModifyIncomingDamage(finalAmount);
        float applied = Stats.TakeDamage(finalAmount);
        DamageReceived?.Invoke(new BattleDamageInfo(applied, source, wasDefending, Stats.IsDead));
        return applied;
    }

    protected override void OnAwake()
    {
        if (view == null)
            view = GetComponent<BattleUnitView>();

        if (statsComponent == null)
            statsComponent = GetComponent<CharacterStatsComponent>();

        if (statsComponent == null)
            throw new InvalidOperationException(
                $"{name}에 {nameof(CharacterStatsComponent)}가 없습니다.");
    }
}
