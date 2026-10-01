using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>이번 전투의 확정된 초기 설정. HP·상태 이상 등은 생성된 BattleUnit에서 관리합니다.</summary>
public sealed class BattleEncounter
{
    public string ContentId { get; }
    public string EnemyGroupId { get; }
    public IReadOnlyList<BattleUnit> EnemyPrefabs { get; }
    public GameObject BackgroundPrefab { get; }
    public bool AllowEscape { get; }

    internal BattleEncounter(BattleContent content, EnemyGroup group)
    {
        ContentId = content.contentId;
        EnemyGroupId = group.groupId;
        EnemyPrefabs = Array.AsReadOnly((BattleUnit[])group.enemies.Clone());
        BackgroundPrefab = content.backgroundPrefab;
        AllowEscape = content.allowEscape;
    }
}
