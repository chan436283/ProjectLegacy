using System;
using System.Collections.Generic;

static class CompanionGenerationTests
{
    static void Require(bool value)
    {
        if (!value) throw new Exception("Companion generation invariant failed");
    }

    static void Invalid(Action action)
    {
        try { action(); }
        catch (ArgumentException) { return; }
        throw new Exception("Invalid preset accepted");
    }

    public static void Run(BattleStatFormulaConfig formula)
    {
        var config = new CompanionGenerationConfig { presets = new CompanionPreset[6] };
        for (int i = 0; i < config.presets.Length; i++)
        {
            config.presets[i] = new CompanionPreset
            {
                presetId = "preset-" + i, displayName = "직종 " + i
            };
            foreach (var range in config.presets[i].statRanges) range.minimum = 4;
            config.presets[i].statRanges[i].minimum = 8;
            config.presets[i].statRanges[7].minimum = i + 1;
            config.presets[i].statRanges[7].maximum = i + 1;
        }
        var characterNames = new[] { "첫째", "둘째", "셋째", "넷째" };
        var selected = new HashSet<string>();
        var totals = new HashSet<float>();
        bool variedWithinParty = false;
        for (int seed = 0; seed < 100; seed++)
        {
            var first = new CompanionGenerator(new Random(seed)).Generate(config, formula, characterNames);
            var second = new CompanionGenerator(new Random(seed)).Generate(config, formula, characterNames);
            Require(first.Count == 4);
            var names = new HashSet<string>();
            var partyTotals = new HashSet<float>();
            var game = new GameData("가문", "주인공", new PrimaryStats(), formula);
            for (int i = 0; i < first.Count; i++)
            {
                var character = first[i];
                Require(names.Add(character.Name));

                Require(character.Name == second[i].Name && character.Id != second[i].Id);
                Require(character.Name == characterNames[i]);
                var preset = config.presets[(int)character.Stats.PrimaryStats.Luck.BaseValue - 1];
                selected.Add(preset.presetId);
                float sum = 0;
                foreach (var range in preset.statRanges)
                {
                    float value = character.Stats.PrimaryStats.Get(range.stat).BaseValue;
                    Require(value >= range.minimum && value <= range.maximum);
                    Require(value == second[i].Stats.PrimaryStats.Get(range.stat).BaseValue);
                    sum += value;
                }
                Require(sum >= config.minimumTotalStatPoints && sum <= config.maximumTotalStatPoints);
                totals.Add(sum);
                partyTotals.Add(sum);
                Require(character.Stats.CurrentHp == character.Stats.Battle.MaxHp.Value);
                game.AddCompanion(character);
            }
            variedWithinParty |= partyTotals.Count > 1;
            var loaded = GameSaveData.Capture(game).Restore(formula);
            for (int i = 0; i < 4; i++)
            {
                Require(loaded.Companions[i].Id == first[i].Id);
                foreach (PrimaryStatType type in Enum.GetValues(typeof(PrimaryStatType)))
                    Require(loaded.Companions[i].Stats.PrimaryStats.Get(type).BaseValue ==
                            first[i].Stats.PrimaryStats.Get(type).BaseValue);
            }
        }
        Require(selected.Count == 6);
        Require(variedWithinParty && totals.Count > 30);
        Require(config.presets[0].presetId == "preset-0" && config.presets[0].statRanges[0].minimum == 8);
        foreach (var preset in config.presets)
        {
            preset.statRanges[7].minimum = 5;
            preset.statRanges[7].maximum = 20;
        }
        Invalid(() => new CompanionGenerator(new Random(1)).Generate(config, formula, null));
        Invalid(() => new CompanionGenerator(new Random(1)).Generate(config, formula, new[] { "한명" }));
        Invalid(() => new CompanionGenerator(new Random(1)).Generate(config, formula, new[] { "첫째", " ", "셋째", "넷째" }));
        config.minimumTotalStatPoints = config.maximumTotalStatPoints = 37; // 최솟값 합
        new CompanionGenerator(new Random(1)).Generate(config, formula, characterNames);
        config.minimumTotalStatPoints = config.maximumTotalStatPoints = 160; // 최댓값 합
        var maximum = new CompanionGenerator(new Random(1)).Generate(config, formula, characterNames);
        Require(maximum[0].Stats.PrimaryStats.Strength.BaseValue == 20);
        config.maximumTotalStatPoints = 161;
        Invalid(config.Validate);
        config.minimumTotalStatPoints = 36;
        config.maximumTotalStatPoints = 100;
        Invalid(config.Validate);
        config.minimumTotalStatPoints = 101;
        Invalid(config.Validate);
        config.minimumTotalStatPoints = -1;
        Invalid(config.Validate);
        config.minimumTotalStatPoints = 40;
        config.presets[0].statRanges[0].stat = PrimaryStatType.Luck;
        Invalid(config.Validate);
        config.presets[0].statRanges[0].stat = PrimaryStatType.Strength;
        config.presets[1].presetId = config.presets[0].presetId;
        Invalid(config.Validate);
        config.presets[1].presetId = "preset-1";
        config.startingCount = 7;
        config.Validate();
        config.presets = Array.Empty<CompanionPreset>();
        Invalid(config.Validate);
        CheckUniformCombinations();
        CheckBonusAndRepeatedPresets(formula);
        Console.WriteLine("PASS: companion selection, deterministic stats, bounds, totals, save restoration and invalid presets (100 seeds)");
    }
    static void CheckUniformCombinations()
    {
        var preset = new CompanionPreset { presetId = "tiny", displayName = "작은 범위" };
        foreach (var range in preset.statRanges) range.minimum = range.maximum = 0;
        preset.statRanges[0].maximum = 3;
        preset.statRanges[1].maximum = 2;
        preset.statRanges[2].maximum = 4;
        for (int total = 0; total <= 9; total++)
        {
            var expected = new HashSet<string>();
            for (int a = 0; a <= 3; a++)
                for (int b = 0; b <= 2; b++)
                    for (int c = 0; c <= 4; c++)
                        if (a + b + c == total) expected.Add($"{a},{b},{c}");
            var sampler = new UniformStatAllocation(preset.statRanges, total);
            Require(sampler.CombinationCount == expected.Count);
            for (int rank = 0; rank < (int)sampler.CombinationCount; rank++)
            {
                var stats = sampler.AtRank(rank);
                Require(expected.Remove($"{stats.Strength.BaseValue},{stats.Constitution.BaseValue},{stats.Dexterity.BaseValue}"));
            }
            Require(expected.Count == 0);
        }
        // 좁은 예제의 모든 조합은 같은 확률로 나타나야 합니다.
        var sample = new UniformStatAllocation(preset.statRanges, 4);
        var histogram = new Dictionary<string, int>();
        var random = new Random(42);
        for (int i = 0; i < 24000; i++)
        {
            var stats = sample.Sample(random);
            string key = $"{stats.Strength.BaseValue},{stats.Constitution.BaseValue},{stats.Dexterity.BaseValue}";
            histogram.TryGetValue(key, out int count);
            histogram[key] = count + 1;
        }
        Require(histogram.Count == (int)sample.CombinationCount);
        double average = 24000.0 / histogram.Count;
        foreach (int count in histogram.Values) Require(Math.Abs(count - average) < average * 0.15);
        Console.WriteLine("PASS: exhaustive combination counts, one-to-one ranks and uniform sampling");
    }

    static void CheckBonusAndRepeatedPresets(BattleStatFormulaConfig formula)
    {
        var preset = new CompanionPreset { presetId = "random", displayName = "무작위", totalStatBonus = 10 };
        foreach (var range in preset.statRanges) { range.minimum = 1; range.maximum = 40; }
        var config = new CompanionGenerationConfig { presets = new[] { preset } };
        var names = new[] { "A", "B", "C", "D" };
        var totals = new HashSet<int>();
        bool extreme = false;
        for (int seed = 0; seed < 200; seed++)
        {
            var party = new CompanionGenerator(new Random(seed)).Generate(config, formula, names);
            Require(party.Count == 4 && party[0].Id != party[1].Id);
            foreach (var character in party)
            {
                int total = 0;
                foreach (var range in preset.statRanges)
                {
                    int value = (int)character.Stats.PrimaryStats.Get(range.stat).BaseValue;
                    Require(value >= 1 && value <= 40);
                    total += value;
                    extreme |= value >= 35;
                }
                Require(total >= 50 && total <= 110);
                totals.Add(total);
            }
        }
        Require(totals.Contains(50) && totals.Contains(110) && extreme);
        config.minimumTotalStatPoints = config.maximumTotalStatPoints = 100;
        foreach (var character in new CompanionGenerator(new Random(1)).Generate(config, formula, names))
        {
            float sum = 0;
            foreach (var range in preset.statRanges) sum += character.Stats.PrimaryStats.Get(range.stat).BaseValue;
            Require(sum == 110);
        }
        // 보너스는 기본 범위 폭(여기서는 0)보다 커도 허용합니다.
        preset.totalStatBonus = 61;
        config.Validate();
        // 기본 총합 100은 배분할 수 있어도 보너스 포함 110을 못 담으면 실패합니다.
        preset.totalStatBonus = 10;
        foreach (var range in preset.statRanges) range.maximum = 13;
        Invalid(config.Validate);
        foreach (var range in preset.statRanges) { range.minimum = 13; range.maximum = 40; }
        config.Validate(); // 최소 합 104는 기본 100보다 크지만 최종 110 이하이므로 유효
        preset.totalStatBonus = 3;
        Invalid(config.Validate);
        foreach (var range in preset.statRanges) range.minimum = 1;
        preset.totalStatBonus = int.MaxValue;
        Invalid(config.Validate);
        preset.totalStatBonus = -1;
        Invalid(config.Validate);
        preset.totalStatBonus = 0;
        config.Validate();
        Console.WriteLine("PASS: repeated preset, bonus bounds, varied totals and natural extreme allocations");
    }

}
