using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>한 게임의 가문, 주인공과 보유 동료를 보관합니다.</summary>
[Serializable]
public sealed class GameData
{
    [SerializeField] private string familyName;
    [SerializeField] private CharacterData protagonist;
    [SerializeField] private List<CharacterData> companions = new();

    public string FamilyName => familyName;
    public CharacterData Protagonist => protagonist;

    // 보유 동료 목록입니다. 전투 출전 인원 제한은 편성 단계에서 적용합니다.
    public IReadOnlyList<CharacterData> Companions => companions.AsReadOnly();

    public GameData(
        string familyName,
        string protagonistName,
        PrimaryStats primaryStats,
        BattleStatFormulaConfig formula)
    {
        if (string.IsNullOrWhiteSpace(familyName))
            throw new ArgumentException("가문명이 필요합니다.", nameof(familyName));

        this.familyName = familyName.Trim();
        protagonist = new CharacterData(protagonistName, primaryStats, formula);
    }

    public void AddCompanion(CharacterData companion)
    {
        if (companion == null)
            throw new ArgumentNullException(nameof(companion));
        if (FindCharacter(companion.Id) != null)
            throw new InvalidOperationException("이미 보유한 캐릭터입니다.");

        companions.Add(companion);
    }

    public bool RemoveCompanion(string characterId)
    {
        int index = companions.FindIndex(character => character.Id == characterId);
        if (index < 0)
            return false;

        companions.RemoveAt(index);
        return true;
    }

    public CharacterData FindCharacter(string characterId)
    {
        if (protagonist.Id == characterId)
            return protagonist;

        return companions.Find(character => character.Id == characterId);
    }
}
