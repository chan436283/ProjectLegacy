using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public sealed class EnemyPlacement
{
    public BattleUnit prefab;
    public BattleRow row = BattleRow.Front;
    // 화면 위에서 아래 순서로 1부터 시작합니다. 빈자리는 채우지 않습니다.
    [Min(1)] public int position = 1;
}

/// <summary>한 번의 전투에 함께 등장할 적과 각 개체의 고정 자리입니다.</summary>
[CreateAssetMenu(fileName = "EnemyGroup", menuName = "ProjectLegacy/Battle/Enemy Group")]
public sealed class EnemyGroup : ScriptableObject, ISerializationCallbackReceiver
{
    public string groupId;
    // 개체당 한 항목. 같은 프리팹을 여러 번 지정할 수 있습니다.
    public EnemyPlacement[] placements = Array.Empty<EnemyPlacement>();

    [SerializeField, HideInInspector, FormerlySerializedAs("enemies")]
    private BattleUnit[] legacyEnemies = Array.Empty<BattleUnit>();

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(groupId) || groupId != groupId.Trim())
            throw new ArgumentException("적 그룹 ID는 앞뒤 공백 없이 지정해야 합니다.");
        if (placements == null || placements.Length == 0)
            throw new ArgumentException("적 그룹에 최소 한 개체가 필요합니다.");
        var occupied = new HashSet<(BattleRow row, int position)>();
        foreach (var placement in placements)
        {
            if (placement == null)
                throw new ArgumentException("비어 있는 적 배치 항목이 있습니다.");
            if (!Enum.IsDefined(typeof(BattleRow), placement.row) || placement.position < 1)
                throw new ArgumentException("적 배치는 전열/후열과 1 이상의 자리 번호가 필요합니다.");
            if (!occupied.Add((placement.row, placement.position)))
                throw new ArgumentException($"적 {placement.row} {placement.position}번 자리가 중복되었습니다.");
            BattleUnit enemy = placement.prefab;
            if (enemy == null || enemy.Side != BattleSide.Enemy || enemy.ControlType != BattleControlType.AI)
                throw new ArgumentException("적군/AI로 설정한 BattleUnit 프리팹이 필요합니다.");
#if UNITY_EDITOR
            if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(enemy))
                throw new ArgumentException("씬 개체가 아닌 적 프리팹을 지정해야 합니다.");
#endif
        }
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        // 기존 프리팹 목록을 잃지 않고 옛 배열 순서를 전열 자리로 옮깁니다.
        if (legacyEnemies == null || legacyEnemies.Length == 0) return;
        if (placements == null || placements.Length == 0)
        {
            placements = new EnemyPlacement[legacyEnemies.Length];
            for (int i = 0; i < legacyEnemies.Length; i++)
                placements[i] = new EnemyPlacement { prefab = legacyEnemies[i], position = i + 1 };
        }
        legacyEnemies = Array.Empty<BattleUnit>();
    }
}
