using System;
using System.Numerics;

/// <summary>고정 총합에서 가능한 정수 스탯 조합을 동일 확률로 선택합니다.</summary>
internal sealed class UniformStatAllocation
{
    private readonly CompanionPreset.StatRange[] ranges;
    private readonly BigInteger[,] counts;
    private readonly int remainingTotal;

    public BigInteger CombinationCount => counts[0, remainingTotal];

    // 호출 전에 프리셋과 총합을 검증해야 합니다.
    public UniformStatAllocation(CompanionPreset.StatRange[] ranges, int total)
    {
        this.ranges = ranges;
        remainingTotal = total;
        foreach (var range in ranges) remainingTotal -= range.minimum;
        counts = new BigInteger[ranges.Length + 1, remainingTotal + 1];
        counts[ranges.Length, 0] = BigInteger.One;
        for (int i = ranges.Length - 1; i >= 0; i--)
        {
            int capacity = ranges[i].maximum - ranges[i].minimum;
            BigInteger window = BigInteger.Zero;
            for (int sum = 0; sum <= remainingTotal; sum++)
            {
                window += counts[i + 1, sum];
                if (sum > capacity) window -= counts[i + 1, sum - capacity - 1];
                counts[i, sum] = window;
            }
        }
    }

    public PrimaryStats Sample(Random random) => AtRank(RandomBelow(random, CombinationCount));

    internal PrimaryStats AtRank(BigInteger rank)
    {
        if (rank < 0 || rank >= CombinationCount) throw new ArgumentOutOfRangeException(nameof(rank));
        var result = new PrimaryStats();
        int remaining = remainingTotal;
        for (int i = 0; i < ranges.Length; i++)
        {
            int limit = Math.Min(remaining, ranges[i].maximum - ranges[i].minimum);
            for (int extra = 0; extra <= limit; extra++)
            {
                BigInteger ways = counts[i + 1, remaining - extra];
                if (rank >= ways) { rank -= ways; continue; }
                result.Get(ranges[i].stat).SetBaseValue(ranges[i].minimum + extra);
                remaining -= extra;
                break;
            }
        }
        return result;
    }

    private static BigInteger RandomBelow(Random random, BigInteger upperExclusive)
    {
        if (upperExclusive <= 0) throw new ArgumentOutOfRangeException(nameof(upperExclusive));
        if (upperExclusive == 1) return BigInteger.Zero;
        byte[] maximum = (upperExclusive - 1).ToByteArray();
        int length = maximum.Length;
        while (length > 1 && maximum[length - 1] == 0) length--;
        int mask = 1;
        while (mask < maximum[length - 1]) mask = (mask << 1) | 1;
        // 마지막 부호 바이트를 0으로 유지하여 양수로 해석합니다.
        var bytes = new byte[length + 1];
        BigInteger value;
        do
        {
            random.NextBytes(bytes);
            bytes[length] = 0;
            bytes[length - 1] &= (byte)mask;
            value = new BigInteger(bytes);
        } while (value >= upperExclusive);
        return value;
    }
}
