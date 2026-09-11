using System;
using System.Collections.Generic;
using CWFramework;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BattleFormation : CBehaviour
{
    [Tooltip("참가자 목록의 아군 순서대로 배치할 위치입니다.")]
    [SerializeField] private List<Transform> allySlots = new();

    [Tooltip("참가자 목록의 적군 순서대로 배치할 위치입니다.")]
    [SerializeField] private List<Transform> enemySlots = new();

    public void PlaceParticipants(IReadOnlyList<BattleUnit> participants)
    {
        if (participants == null) throw new ArgumentNullException(nameof(participants));

        var placements = new List<(BattleUnit unit, Vector3 position)>();
        var usedSlots = new HashSet<Transform>();
        var usedUnits = new HashSet<BattleUnit>();
        int allyIndex = 0;
        int enemyIndex = 0;

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
                    slots = enemySlots;
                    index = enemyIndex++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(unit.Side));
            }

            if (index >= slots.Count || slots[index] == null)
                throw new InvalidOperationException($"{unit.Side}의 {index}번 배치 슬롯이 없습니다. ({unit.name})");
            Transform slot = slots[index];
            if (!usedSlots.Add(slot))
                throw new InvalidOperationException($"배치 슬롯 '{slot.name}'이 중복 지정되어 있습니다.");
            placements.Add((unit, slot.position));
        }

        foreach (var placement in placements)
            placement.unit.transform.position = placement.position;
    }
}
