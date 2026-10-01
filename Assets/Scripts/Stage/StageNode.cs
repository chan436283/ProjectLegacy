using System;

// 기존 직렬화 값 중 Start와 Ending은 유지합니다.
public enum StageNodeType { Start = 0, Normal = 1, Ending = 4 }

[Serializable]
public sealed class StageRoute
{
    public string routeId;
    public string targetNodeId;
    public string label;
}

/// <summary>스테이지 에셋에 포함되는 고정 지점입니다.</summary>
[Serializable]
public sealed class StageNode
{
    public string nodeId;
    public string displayName;
    public StageNodeType kind;
    public StageContent content;
    public string endingId;
    public StageRoute[] routes = Array.Empty<StageRoute>();
}
