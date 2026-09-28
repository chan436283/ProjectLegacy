using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CompanionGenerationConfig", menuName = "ProjectLegacy/Character/Companion Generation Config")]
public sealed class CompanionGenerationConfig : ScriptableObject
{
    public int startingCount = 4;
    // 조합 수 계산에 사용하는 메모리와 시간을 제한합니다.
    public const int MaximumSupportedTotal = 10000;
    public int minimumTotalStatPoints = 40;
    public int maximumTotalStatPoints = 100;
    public CompanionPreset[] presets = Array.Empty<CompanionPreset>();

    public void Validate()
    {
        if (startingCount < 1 || minimumTotalStatPoints < 0 ||
            maximumTotalStatPoints < minimumTotalStatPoints || maximumTotalStatPoints > MaximumSupportedTotal ||
            presets == null || presets.Length == 0)
            throw new ArgumentException("동료 수, 프리셋 수 또는 능력치 총합 범위가 올바르지 않습니다 (총합 최대 10000).");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var preset in presets)
        {
            if (preset == null) throw new ArgumentException("동료 프리셋 참조가 비어 있습니다.");
            if (preset.totalStatBonus < 0 ||
                preset.totalStatBonus > MaximumSupportedTotal - maximumTotalStatPoints)
                throw new ArgumentException("프리셋 보너스는 0 이상이며 보너스 포함 최대 총합은 10000 이하여야 합니다.");
            // 범위 전체를 배분할 수 있어야 합니다. 추첨 결과를 잘라내지 않습니다.
            preset.Validate(minimumTotalStatPoints + preset.totalStatBonus);
            preset.Validate(maximumTotalStatPoints + preset.totalStatBonus);
            if (!ids.Add(preset.presetId.Trim()))
                throw new ArgumentException("동료 프리셋 ID는 중복될 수 없습니다.");
        }
    }
}
