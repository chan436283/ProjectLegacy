using System;
using UnityEngine;

/// <summary>씬의 전투 오브젝트와 독립적으로 유지되는 인물 데이터입니다.</summary>
[Serializable]
public sealed class CharacterData
{
    [SerializeField] private string id;
    [SerializeField] private string characterName;
    [SerializeField] private CharacterStats stats;

    public string Id => id;
    public string Name => characterName;
    public CharacterStats Stats => stats;

    public CharacterData(
        string name,
        PrimaryStats primaryStats,
        BattleStatFormulaConfig formula)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("캐릭터 이름이 필요합니다.", nameof(name));
        if (primaryStats == null)
            throw new ArgumentNullException(nameof(primaryStats));
        if (formula == null)
            throw new ArgumentNullException(nameof(formula));

        id = Guid.NewGuid().ToString("N");
        characterName = name.Trim();
        stats = new CharacterStats();

        // 생성 입력의 기본값만 복사합니다. 임시 보정치와 참조는 공유하지 않습니다.
        foreach (AbilityStatType type in Enum.GetValues(typeof(AbilityStatType)))
        {
            StatValue source = primaryStats.Get(type);
            if (source == null || float.IsNaN(source.BaseValue) ||
                float.IsInfinity(source.BaseValue) || source.BaseValue < 0f)
            {
                throw new ArgumentException("기본 능력치는 유한한 0 이상의 값이어야 합니다.", nameof(primaryStats));
            }

            stats.PrimaryStats.Get(type).SetBaseValue(source.BaseValue);
        }

        stats.Initialize(formula);
    }
}
