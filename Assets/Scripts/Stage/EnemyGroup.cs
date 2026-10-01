using System;
using UnityEngine;

/// <summary>한 번의 전투에 함께 등장할 적 편성입니다. 배열 순서가 적 진형 슬롯 순서입니다.</summary>
[CreateAssetMenu(fileName = "EnemyGroup", menuName = "ProjectLegacy/Battle/Enemy Group")]
public sealed class EnemyGroup : ScriptableObject
{
    public string groupId;
    // 개체당 한 항목. 같은 프리팹을 여러 번 지정할 수 있습니다.
    public BattleUnit[] enemies = Array.Empty<BattleUnit>();

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(groupId) || groupId != groupId.Trim())
            throw new ArgumentException("적 그룹 ID는 앞뒤 공백 없이 지정해야 합니다.");
        if (enemies == null || enemies.Length == 0)
            throw new ArgumentException("적 그룹에 최소 한 개체가 필요합니다.");
        foreach (var enemy in enemies)
        {
            if (enemy == null || enemy.Side != BattleSide.Enemy || enemy.ControlType != BattleControlType.AI)
                throw new ArgumentException("적군/AI로 설정한 BattleUnit 프리팹이 필요합니다.");
#if UNITY_EDITOR
            if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(enemy))
                throw new ArgumentException("씬 개체가 아닌 적 프리팹을 지정해야 합니다.");
#endif
        }
    }
}
