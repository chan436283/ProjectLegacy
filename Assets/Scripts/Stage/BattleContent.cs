using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>전투별 적 그룹 후보와 상대 등장 가중치입니다.</summary>
[Serializable]
public sealed class EnemyGroupEntry
{
    public EnemyGroup group;
    [Min(0)] public float weight = 1f;
}

[CreateAssetMenu(fileName = "BattleContent", menuName = "ProjectLegacy/Expedition/Battle Content")]
public sealed class BattleContent : StageContent
{
    public override StageContentType Type => StageContentType.Battle;
    public EnemyGroupEntry[] enemyGroups = Array.Empty<EnemyGroupEntry>();
    // 비워두면 전투 씬의 기본 배경을 사용합니다.
    public GameObject backgroundPrefab;
    public bool allowEscape = true;

    public override void Validate()
    {
        base.Validate();
        if (enemyGroups == null || enemyGroups.Length == 0)
            throw new ArgumentException("전투 콘텐츠에 최소 한 개의 적 그룹이 필요합니다.");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        double totalWeight = 0;
        foreach (var entry in enemyGroups)
        {
            if (entry == null || entry.group == null) throw new ArgumentException("비어 있는 적 그룹 참조가 있습니다.");
            if (float.IsNaN(entry.weight) || float.IsInfinity(entry.weight) || entry.weight < 0)
                throw new ArgumentException("가중치는 0 이상의 유한한 숫자여야 합니다.");
            totalWeight += entry.weight;
            var group = entry.group;
            group.Validate();
            if (!ids.Add(group.groupId))
                throw new ArgumentException("후보 그룹 ID가 중복되었습니다.");
        }
        if (totalWeight <= 0) throw new ArgumentException("양수 가중치를 가진 적 그룹이 필요합니다.");
    }

    /// <summary>전투 시작 시 한 번 호출하고 결과를 보관합니다. 후보 그룹의 상대 가중치로 선택합니다. 0은 제외합니다.</summary>
    public BattleEncounter CreateEncounter(System.Random random)
    {
        if (random == null) throw new ArgumentNullException(nameof(random));
        Validate();
        double totalWeight = 0;
        int activeCount = 0;
        EnemyGroup lastActiveGroup = null;
        foreach (var entry in enemyGroups)
        {
            if (entry.weight <= 0) continue;
            totalWeight += entry.weight;
            activeCount++;
            lastActiveGroup = entry.group;
        }
        if (activeCount == 1) return new BattleEncounter(this, lastActiveGroup);

        double roll = random.NextDouble() * totalWeight;
        double cumulative = 0;
        foreach (var entry in enemyGroups)
        {
            if (entry.weight <= 0) continue;
            cumulative += entry.weight;
            if (roll < cumulative) return new BattleEncounter(this, entry.group);
        }
        // 부동소수점 경계 반올림 시에도 0 가중치 후보를 선택하지 않습니다.
        return new BattleEncounter(this, lastActiveGroup);
    }
}
