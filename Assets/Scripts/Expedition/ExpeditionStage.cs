using System;
using UnityEngine;

/// <summary>지도 지점이 시작할 원정의 정의입니다. 진행 상태는 별도로 관리합니다.</summary>
[CreateAssetMenu(fileName = "ExpeditionStage", menuName = "ProjectLegacy/Expedition/Stage")]
public sealed class ExpeditionStage : ScriptableObject
{
    public string stageId;
    public string displayName;
    public string entryScene = "GameScene";

    public string GetDisplayName(string familyName)
    {
        string resolvedFamilyName = string.IsNullOrWhiteSpace(familyName)
            ? "이름 없는 가문" : familyName.Trim();
        return displayName?.Replace("{FamilyName}", resolvedFamilyName);
    }

    public ExpeditionRunData CreateRun(string familyName = null)
    {
        return new ExpeditionRunData(stageId, GetDisplayName(familyName), entryScene);
    }
}
