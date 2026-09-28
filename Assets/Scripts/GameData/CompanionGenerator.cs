using System;
using System.Collections.Generic;

/// <summary>외부 난수원을 사용하며 설정 에셋이나 진행 데이터를 변경하지 않습니다.</summary>
public sealed class CompanionGenerator
{
    private readonly Random random;

    public CompanionGenerator(Random random)
    {
        this.random = random ?? throw new ArgumentNullException(nameof(random));
    }

    public IReadOnlyList<CharacterData> Generate(CompanionGenerationConfig config, BattleStatFormulaConfig formula,
        IReadOnlyList<string> characterNames)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        if (formula == null) throw new ArgumentNullException(nameof(formula));
        config.Validate();
        if (characterNames == null || characterNames.Count != config.startingCount)
            throw new ArgumentException("동료 수와 같은 수의 인물 이름이 필요합니다.", nameof(characterNames));
        foreach (string name in characterNames)
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("동료 이름은 비어 있을 수 없습니다.", nameof(characterNames));

        var result = new List<CharacterData>(config.startingCount);
        for (int i = 0; i < config.startingCount; i++)
        {
            // 동료마다 독립 추첨하므로 같은 프리셋을 여러 번 사용할 수 있습니다.
            var preset = config.presets[random.Next(config.presets.Length)];
            int total = random.Next(config.minimumTotalStatPoints,
                config.maximumTotalStatPoints + 1) + preset.totalStatBonus;
            var stats = new UniformStatAllocation(preset.statRanges, total).Sample(random);
            result.Add(new CharacterData(characterNames[i], stats, formula));
        }
        return result.AsReadOnly();
    }

}
