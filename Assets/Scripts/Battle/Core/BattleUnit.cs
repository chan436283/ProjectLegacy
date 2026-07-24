using System;
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

    public BattleSide Side => side;
    public BattleControlType ControlType => controlType;
    public CharacterStats Stats => statsComponent.Stats;
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

    protected override void OnAwake()
    {
        if (statsComponent == null)
            statsComponent = GetComponent<CharacterStatsComponent>();

        if (statsComponent == null)
            throw new InvalidOperationException(
                $"{name}에 {nameof(CharacterStatsComponent)}가 없습니다.");
    }
}
