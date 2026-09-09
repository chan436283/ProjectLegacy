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

    [Header("Battle Actions")]
    [SerializeField]
    private BattleSkill basicAttackSkill;

    [SerializeField]
    private List<BattleSkill> skills = new();

    [SerializeField]
    private BattleSkill defendSkill;

    private readonly List<BattleStatus> statuses = new();

    public BattleSide Side => side;
    public BattleControlType ControlType => controlType;
    public CharacterStats Stats => statsComponent.Stats;
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
    }

    public float ReceiveDamage(float amount)
    {
        float finalAmount = amount;
        foreach (BattleStatus status in statuses)
            finalAmount = status.ModifyIncomingDamage(finalAmount);
        return Stats.TakeDamage(finalAmount);
    }

    protected override void OnAwake()
    {
        if (statsComponent == null)
            statsComponent = GetComponent<CharacterStatsComponent>();

        if (statsComponent == null)
            throw new InvalidOperationException(
                $"{name}에 {nameof(CharacterStatsComponent)}가 없습니다.");
    }
}
