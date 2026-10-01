using System;
using System.Collections.Generic;

/// <summary>임시 전투 효과와 Unity 오브젝트 참조를 제외한 저장 형식입니다.</summary>
[Serializable]
public sealed class GameSaveData
{
    public int version;
    public string familyName;
    public CharacterRecord protagonist;
    public List<CharacterRecord> companions;
    public List<ExplorationRecord> explorations;

    [Serializable]
    public sealed class ExplorationRecord
    {
        public string stageId;
        public List<string> nodes;
        public List<string> routes;
        public List<string> endings;
    }


    [Serializable]
    public sealed class CharacterRecord
    {
        public string id;
        public string name;
        public int level;
        // version 1: PrimaryStatType의 선언 순서. 형식 변경 시 버전을 올립니다.
        public float[] primaryValues;
        public float currentHp;
        public float currentMp;

        public static CharacterRecord Capture(CharacterData character)
        {
            var types = (PrimaryStatType[])Enum.GetValues(typeof(PrimaryStatType));
            var values = new float[types.Length];
            for (int i = 0; i < types.Length; i++)
                values[i] = character.Stats.PrimaryStats.Get(types[i]).BaseValue;
            return new CharacterRecord
            {
                id = character.Id, name = character.Name, level = character.Stats.Level,
                primaryValues = values, currentHp = character.Stats.CurrentHp,
                currentMp = character.Stats.CurrentMp
            };
        }

        public CharacterData Restore(BattleStatFormulaConfig formula)
        {
            var types = (PrimaryStatType[])Enum.GetValues(typeof(PrimaryStatType));
            if (primaryValues == null || primaryValues.Length != types.Length)
                throw new ArgumentException("저장된 능력치 개수가 올바르지 않습니다.");
            var primary = new PrimaryStats();
            for (int i = 0; i < types.Length; i++) primary.Get(types[i]).SetBaseValue(primaryValues[i]);
            return CharacterData.Restore(id, name, primary, level, currentHp, currentMp, formula);
        }
    }

    public static GameSaveData Capture(GameData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        var save = new GameSaveData
        {
            version = 2, familyName = data.FamilyName,
            protagonist = CharacterRecord.Capture(data.Protagonist),
            companions = new List<CharacterRecord>(),
            explorations = new List<ExplorationRecord>()
        };
        foreach (CharacterData companion in data.Companions)
            save.companions.Add(CharacterRecord.Capture(companion));
        foreach (var record in data.Explorations)
            save.explorations.Add(new ExplorationRecord { stageId = record.StageId,
                nodes = new List<string>(record.DiscoveredNodes), routes = new List<string>(record.DiscoveredRoutes),
                endings = new List<string>(record.ReachedEndings) });
        return save;
    }

    private static void RestoreIds(List<string> values, Action<string> add)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var id in values)
        {
            if (string.IsNullOrWhiteSpace(id) || id != id.Trim() || !seen.Add(id))
                throw new ArgumentException("발견 기록의 ID가 올바르지 않습니다.");
            add(id);
        }
    }

    public GameData Restore(BattleStatFormulaConfig formula)
    {
        if ((version != 1 && version != 2) || protagonist == null || companions == null ||
            (version == 2 && explorations == null))
            throw new ArgumentException("지원하지 않거나 손상된 저장 데이터입니다.");
        var restoredCompanions = new List<CharacterData>();
        foreach (CharacterRecord companion in companions)
        {
            if (companion == null) throw new ArgumentException("동료 저장 데이터가 없습니다.");
            restoredCompanions.Add(companion.Restore(formula));
        }
        var data = GameData.Restore(familyName, protagonist.Restore(formula), restoredCompanions);
        if (version == 2)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var record in explorations)
            {
                if (record == null || string.IsNullOrWhiteSpace(record.stageId) || !ids.Add(record.stageId) ||
                    record.nodes == null || record.routes == null || record.endings == null)
                    throw new ArgumentException("탐험 기록이 올바르지 않습니다.");
                var target = data.GetExploration(record.stageId);
                RestoreIds(record.nodes, target.DiscoverNode);
                RestoreIds(record.routes, target.DiscoverRoute);
                RestoreIds(record.endings, target.ReachEnding);
            }
        }
        return data;
    }
}
