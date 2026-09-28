using System;

/// <summary>출발 시 확정한 원정 정보입니다. 현재는 메모리에서만 유지합니다.</summary>
public sealed class ExpeditionRunData
{
    public string StageId { get; }
    public string StageName { get; }
    public string EntryScene { get; }

    public ExpeditionRunData(string stageId, string stageName, string entryScene)
    {
        if (string.IsNullOrWhiteSpace(stageId) || string.IsNullOrWhiteSpace(stageName) ||
            string.IsNullOrWhiteSpace(entryScene))
            throw new ArgumentException("스테이지 ID, 이름, 진입 씬이 필요합니다.");
        StageId = stageId.Trim();
        StageName = stageName.Trim();
        EntryScene = entryScene.Trim();
    }
}
