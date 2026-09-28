using System;
using UnityEngine;

/// <summary>생성 시에만 사용하는 직종 성향입니다. 성장이나 행동을 제한하지 않습니다.</summary>
[CreateAssetMenu(fileName = "CompanionPreset", menuName = "ProjectLegacy/Character/Companion Preset")]
public sealed class CompanionPreset : ScriptableObject
{
    public string presetId;
    public string displayName;
    public int totalStatBonus;
    public StatRange[] statRanges = CreateDefaultRanges();

    [Serializable]
    public sealed class StatRange
    {
        public PrimaryStatType stat;
        public int minimum = 5;
        public int maximum = 20;
    }

    private static StatRange[] CreateDefaultRanges()
    {
        var types = (PrimaryStatType[])Enum.GetValues(typeof(PrimaryStatType));
        var ranges = new StatRange[types.Length];
        for (int i = 0; i < types.Length; i++)
            ranges[i] = new StatRange { stat = types[i] };
        return ranges;
    }

    public void Validate(int total)
    {
        if (string.IsNullOrWhiteSpace(presetId) || string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("동료 프리셋 ID와 표시 이름이 필요합니다.");

        var types = (PrimaryStatType[])Enum.GetValues(typeof(PrimaryStatType));
        var seen = new System.Collections.Generic.HashSet<PrimaryStatType>();
        long minimumSum = 0, maximumSum = 0;
        if (statRanges == null || statRanges.Length != types.Length)
            throw new ArgumentException($"{displayName}: 모든 기본 능력치의 범위가 필요합니다.");
        foreach (var range in statRanges)
        {
            if (range == null || !Enum.IsDefined(typeof(PrimaryStatType), range.stat) ||
                !seen.Add(range.stat) || range.minimum < 0 || range.maximum < range.minimum ||
                range.maximum > 16777216)
                throw new ArgumentException($"{displayName}: 능력치 종류·범위 설정이 잘못되었습니다.");
            minimumSum += range.minimum;
            maximumSum += range.maximum;
        }
        if (total < minimumSum || total > maximumSum)
            throw new ArgumentException($"{displayName}: 능력치 총합은 {minimumSum}~{maximumSum}이어야 합니다.");
    }
}
