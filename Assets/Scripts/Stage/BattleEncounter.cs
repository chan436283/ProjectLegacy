using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>선택 시 복사한 초기 배치. 그룹 에셋을 수정해도 바뀌지 않습니다.</summary>
public readonly struct EnemySpawnData
{
    public BattleUnit Prefab { get; }
    public BattleRow Row { get; }
    public int Position { get; }

    internal EnemySpawnData(EnemyPlacement placement)
    {
        Prefab = placement.prefab;
        Row = placement.row;
        Position = placement.position;
    }
}

/// <summary>이번 전투의 확정된 초기 설정. HP·상태 이상 등은 생성된 BattleUnit에서 관리합니다.</summary>
public sealed class BattleEncounter
{
    public string ContentId { get; }
    public string EnemyGroupId { get; }
    public IReadOnlyList<EnemySpawnData> Enemies { get; }
    public GameObject BackgroundPrefab { get; }
    public bool AllowEscape { get; }

    internal BattleEncounter(BattleContent content, EnemyGroup group)
    {
        ContentId = content.contentId;
        EnemyGroupId = group.groupId;
        var enemies = new EnemySpawnData[group.placements.Length];
        for (int i = 0; i < enemies.Length; i++)
            enemies[i] = new EnemySpawnData(group.placements[i]);
        Enemies = Array.AsReadOnly(enemies);
        BackgroundPrefab = content.backgroundPrefab;
        AllowEscape = content.allowEscape;
    }
}
