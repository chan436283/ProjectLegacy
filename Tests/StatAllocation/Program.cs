using System;

static class Program
{
    static readonly PrimaryStatType[] Types = (PrimaryStatType[])Enum.GetValues(typeof(PrimaryStatType));

    static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
    }

    static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }

    static void CheckBudget(PrimaryStatAllocation allocation)
    {
        int spent = 0;
        foreach (var type in Types)
        {
            int value = allocation.GetValue(type);
            Check(value >= allocation.MinimumValue, "stat bounds");
            spent += value - allocation.BaseValue;
        }

        Check(allocation.RemainingPoints >= 0, "point bounds");
        Check(spent + allocation.RemainingPoints == allocation.TotalPoints, "point conservation");
    }

    static void Main()
    {
        var allocation = new PrimaryStatAllocation(5, 1, 20);
        foreach (var type in Types)
        {
            Check(allocation.GetValue(type) == 5, "initial value");
            for (int i = 0; i < 4; i++) Check(allocation.TryDecrease(type), "refund below default");
            Check(!allocation.TryDecrease(type), "minimum one");
        }
        Check(allocation.RemainingPoints == 52, "all base points refunded");
        while (allocation.TryIncrease(PrimaryStatType.Strength)) { }
        Check(allocation.GetValue(PrimaryStatType.Strength) == 53, "uncapped extreme build");
        foreach (var type in Types) Check(!allocation.TryIncrease(type), "shared budget exhausted");
        Check(allocation.TryDecrease(PrimaryStatType.Strength), "refund point");
        Check(allocation.TryIncrease(PrimaryStatType.Charisma), "spend refund on another stat");
        CheckBudget(allocation);
        Console.WriteLine("PASS: minimum 1, refunds below 5, STR 53 and shared budget");

        var snapshot = allocation.CreatePrimaryStats();
        allocation.Reset();
        Check(snapshot.Strength.BaseValue == 52 && snapshot.Charisma.BaseValue == 2, "snapshot survives reset");
        snapshot.Luck.SetBaseValue(99);
        Check(allocation.GetValue(PrimaryStatType.Luck) == 5, "snapshot has no shared stats");
        Check(allocation.RemainingPoints == 20, "reset budget");
        CheckBudget(allocation);
        Console.WriteLine("PASS: reset to 5/20 and independent PrimaryStats output");

        var random = new Random(42);
        for (int i = 0; i < 2000; i++)
        {
            var type = Types[random.Next(Types.Length)];
            if (random.Next(2) == 0) allocation.TryIncrease(type);
            else allocation.TryDecrease(type);
            CheckBudget(allocation);
        }
        Console.WriteLine("PASS: budget conserved across 2000 mixed adjustments");

        var custom = new PrimaryStatAllocation(5, 1, 0);
        Check(!custom.TryIncrease(PrimaryStatType.Strength), "zero bonus");
        Check(custom.TryDecrease(PrimaryStatType.Luck), "refund with zero bonus");
        Check(custom.TryIncrease(PrimaryStatType.Strength), "redistribute base points");
        CheckBudget(custom);
        Throws<ArgumentOutOfRangeException>(() => new PrimaryStatAllocation(0, 1, 20));
        Throws<ArgumentOutOfRangeException>(() => new PrimaryStatAllocation(5, 0, 20));
        Throws<ArgumentOutOfRangeException>(() => new PrimaryStatAllocation(5, 6, 20));
        Throws<ArgumentOutOfRangeException>(() => new PrimaryStatAllocation(5, 1, -1));
        Throws<ArgumentOutOfRangeException>(() => allocation.TryIncrease((PrimaryStatType)999));
        Console.WriteLine("PASS: custom rules and invalid input");
    }
}
