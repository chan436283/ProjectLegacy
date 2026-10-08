using System;
using UnityEngine;

/// <summary>스테이지 공통 정보. 지점과 경로·콘텐츠 설정은 맵 프리팹이 소유합니다.</summary>
[CreateAssetMenu(fileName = "StageDefinition", menuName = "ProjectLegacy/Expedition/Stage")]
public sealed class StageDefinition : ScriptableObject
{
    public string stageId;
    public string displayName;
    public string entryScene = "GameScene";
    public ExpeditionMap mapPrefab;

#if UNITY_EDITOR
    [ContextMenu("Validate Stage")]
    private void ValidateInEditor()
    {
        try { Validate(); Debug.Log("스테이지 맵 검증 완료", this); }
        catch (Exception exception) { Debug.LogException(exception, this); }
    }
#endif

    public void Validate()
    {
        new ExpeditionRun(stageId, GetDisplayName(null), entryScene);
        if (mapPrefab == null) throw new ArgumentException("원정 맵 프리팹을 연결해야 합니다.");
        mapPrefab.BuildMapData();
    }

    public string GetDisplayName(string familyName)
    {
        string name = string.IsNullOrWhiteSpace(familyName) ? "이름 없는 가문" : familyName.Trim();
        return GameTextFormatter.Format(displayName, "FamilyName", name);
    }

    public ExpeditionRun CreateRun(string familyName = null)
    {
        Validate();
        // 출정 시에는 메타데이터만 생성하고, 실제 맵 인스턴스가 준비되면 Show에서 초기화합니다.
        return new ExpeditionRun(stageId, GetDisplayName(familyName), entryScene, mapPrefab);
    }
}
