using System;
using System.Collections.Generic;

/// <summary>생성 중 능력치 배분과 포인트 예산을 관리합니다.</summary>
public sealed class PrimaryStatAllocation
{
    private readonly Dictionary<PrimaryStatType, int> values = new();

    public int BaseValue { get; }
    public int MinimumValue { get; }
    public int TotalPoints { get; }
    public int RemainingPoints { get; private set; }

    public PrimaryStatAllocation(int baseValue, int minimumValue, int totalPoints)
    {
        if (baseValue < 1) throw new ArgumentOutOfRangeException(nameof(baseValue));
        if (minimumValue < 1 || minimumValue > baseValue) throw new ArgumentOutOfRangeException(nameof(minimumValue));
        if (totalPoints < 0) throw new ArgumentOutOfRangeException(nameof(totalPoints));

        BaseValue = baseValue;
        MinimumValue = minimumValue;
        TotalPoints = totalPoints;
        Reset();
    }

    public int GetValue(PrimaryStatType type)
    {
        if (!values.TryGetValue(type, out int value))
            throw new ArgumentOutOfRangeException(nameof(type), type, null);
        return value;
    }

    public bool CanIncrease(PrimaryStatType type)
    {
        GetValue(type); // 잘못된 능력치 종류는 잔여 포인트와 관계없이 검증합니다.
        return RemainingPoints > 0;
    }
    public bool CanDecrease(PrimaryStatType type) => GetValue(type) > MinimumValue;

    public bool TryIncrease(PrimaryStatType type)
    {
        if (!CanIncrease(type)) return false;
        values[type]++;
        RemainingPoints--;
        return true;
    }

    public bool TryDecrease(PrimaryStatType type)
    {
        if (!CanDecrease(type)) return false;
        values[type]--;
        RemainingPoints++;
        return true;
    }

    public void Reset()
    {
        foreach (PrimaryStatType type in Enum.GetValues(typeof(PrimaryStatType)))
            values[type] = BaseValue;
        RemainingPoints = TotalPoints;
    }

    /// <summary>UI의 배분 상태와 독립적인 능력치를 생성합니다.</summary>
    public PrimaryStats CreatePrimaryStats()
    {
        var result = new PrimaryStats();
        foreach (var entry in values)
            result.Get(entry.Key).SetBaseValue(entry.Value);
        return result;
    }
}
