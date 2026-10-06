using System;
using System.Collections.Generic;
using CWFramework;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public sealed class BattleFormation : CBehaviour
{
    [Tooltip("참가자 목록의 아군 순서대로 배치할 위치입니다.")]
    [SerializeField] private List<Transform> allySlots = new();

    [Tooltip("적 전열의 1번, 2번… 자리입니다. 화면 위에서 아래 순서로 연결합니다.")]
    [FormerlySerializedAs("enemySlots")]
    [SerializeField] private List<Transform> enemyFrontSlots = new();

    [Tooltip("적 후열의 1번, 2번… 자리입니다. 화면 위에서 아래 순서로 연결합니다.")]
    [SerializeField] private List<Transform> enemyBackSlots = new();

    public void PlaceParticipants(IReadOnlyList<BattleUnit> participants)
    {
        var placements = BuildPlacements(participants);
        foreach (var placement in placements)
            placement.unit.transform.position = placement.position;
    }

    public void ValidateParticipants(IReadOnlyList<BattleUnit> participants)
    {
        BuildPlacements(participants);
    }

    private List<(BattleUnit unit, Vector3 position)> BuildPlacements(IReadOnlyList<BattleUnit> participants)
    {
        if (participants == null) throw new ArgumentNullException(nameof(participants));

        var placements = new List<(BattleUnit unit, Vector3 position)>();
        var usedSlots = new HashSet<Transform>();
        var usedUnits = new HashSet<BattleUnit>();
        var enemyPositions = new HashSet<(BattleRow row, int position)>();
        int allyIndex = 0;

        // Validate and snapshot every destination before moving any unit.
        foreach (BattleUnit unit in participants)
        {
            if (unit == null || !usedUnits.Add(unit)) continue;
            List<Transform> slots;
            int index;
            switch (unit.Side)
            {
                case BattleSide.Ally:
                    slots = allySlots;
                    index = allyIndex++;
                    break;
                case BattleSide.Enemy:
                    slots = unit.Row switch
                    {
                        BattleRow.Front => enemyFrontSlots,
                        BattleRow.Back => enemyBackSlots,
                        _ => throw new InvalidOperationException($"알 수 없는 적 배치 열입니다. ({unit.name})")
                    };
                    if (unit.FormationPosition < 1)
                        throw new InvalidOperationException($"적 자리 번호는 1 이상이어야 합니다. ({unit.name})");
                    if (!enemyPositions.Add((unit.Row, unit.FormationPosition)))
                        throw new InvalidOperationException($"적 {unit.Row} {unit.FormationPosition}번 자리가 중복되었습니다.");
                    index = unit.FormationPosition - 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(unit.Side));
            }

            if (slots == null || index >= slots.Count || slots[index] == null)
                throw new InvalidOperationException($"{unit.Side} {unit.Row}의 {index + 1}번 배치 슬롯이 없습니다. ({unit.name})");
            Transform slot = slots[index];
            if (!usedSlots.Add(slot))
                throw new InvalidOperationException($"배치 슬롯 '{slot.name}'이 중복 지정되어 있습니다.");
            placements.Add((unit, slot.position));
        }

        return placements;
    }
}
