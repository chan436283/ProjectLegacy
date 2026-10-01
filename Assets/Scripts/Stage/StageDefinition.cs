using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>지도 지점이 시작할 원정의 정의입니다. 진행 상태는 별도로 관리합니다.</summary>
[CreateAssetMenu(fileName = "StageDefinition", menuName = "ProjectLegacy/Expedition/Stage")]
public sealed class StageDefinition : ScriptableObject
{
    public string stageId;
    public string displayName;
    public string entryScene = "GameScene";
    public string startNodeId = "start";
    public StageNode[] nodes =
    {
        new StageNode { nodeId = "start", displayName = "입구", kind = StageNodeType.Start,
            routes = new[] { new StageRoute { routeId = "to_exit", targetNodeId = "exit", label = "진행" } } },
        new StageNode { nodeId = "exit", displayName = "출구", kind = StageNodeType.Ending, endingId = "exit" }
    };

#if UNITY_EDITOR
    [ContextMenu("Validate Stage")]
    private void ValidateInEditor()
    {
        try { Validate(); Debug.Log("스테이지 경로 검증 완료", this); }
        catch (Exception exception) { Debug.LogException(exception, this); }
    }
#endif

    public void Validate()
    {
        // 기존 이름 템플릿과 진입 씬의 유효성도 검사합니다.
        new ExpeditionRun(stageId, GetDisplayName(null), entryScene);
        if (nodes == null || nodes.Length == 0) throw new ArgumentException("스테이지 노드가 필요합니다.");
        var byId = new Dictionary<string, StageNode>(StringComparer.Ordinal);
        var routeIds = new HashSet<string>(StringComparer.Ordinal);
        var endingIds = new HashSet<string>(StringComparer.Ordinal);
        int starts = 0;
        foreach (var node in nodes)
        {
            if (node == null || !ValidId(node.nodeId) || string.IsNullOrWhiteSpace(node.displayName) ||
                !Enum.IsDefined(typeof(StageNodeType), node.kind) || node.routes == null || byId.ContainsKey(node.nodeId))
                throw new ArgumentException("노드 ID·이름·종류·경로 목록을 확인하세요.");
            byId.Add(node.nodeId, node);
            if (node.kind == StageNodeType.Start) starts++;
            if (node.kind == StageNodeType.Ending)
            {
                if (!ValidId(node.endingId) || !endingIds.Add(node.endingId) || node.routes.Length != 0)
                    throw new ArgumentException("종착점은 고유한 결과 ID를 갖고 나가는 경로가 없어야 합니다.");
            }
            else if (node.routes.Length == 0) throw new ArgumentException("종착점 이외 노드에 막다른 길이 있습니다.");
            if (node.kind == StageNodeType.Normal && node.content == null)
                throw new ArgumentException("일반 노드에는 실행 콘텐츠가 필요합니다.");
            if (node.content != null) node.content.Validate();
            var targets = new HashSet<string>(StringComparer.Ordinal);
            foreach (var route in node.routes)
                if (route == null || !ValidId(route.routeId) || !routeIds.Add(route.routeId) ||
                    !ValidId(route.targetNodeId) || !targets.Add(route.targetNodeId) || string.IsNullOrWhiteSpace(route.label))
                    throw new ArgumentException("경로 ID·대상·선택지 이름을 확인하세요.");
        }
        if (!ValidId(startNodeId) || !byId.ContainsKey(startNodeId) || starts != 1 ||
            byId[startNodeId].kind != StageNodeType.Start || endingIds.Count == 0)
            throw new ArgumentException("시작 노드 하나와 최소 한 개의 종착점이 필요합니다.");
        var indegree = new Dictionary<string, int>();
        foreach (var id in byId.Keys) indegree[id] = 0;
        foreach (var node in nodes)
            foreach (var route in node.routes)
            {
                if (!byId.ContainsKey(route.targetNodeId)) throw new ArgumentException("존재하지 않는 연결 대상입니다.");
                indegree[route.targetNodeId]++;
            }
        var queue = new Queue<string>();
        foreach (var entry in indegree) if (entry.Value == 0) queue.Enqueue(entry.Key);
        int processed = 0;
        while (queue.Count > 0)
        {
            var node = byId[queue.Dequeue()];
            processed++;
            foreach (var route in node.routes) if (--indegree[route.targetNodeId] == 0) queue.Enqueue(route.targetNodeId);
        }
        if (processed != nodes.Length) throw new ArgumentException("고정 원정 경로에 순환이 있습니다.");
        var reached = new HashSet<string>();
        queue.Enqueue(startNodeId);
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            if (!reached.Add(id)) continue;
            foreach (var route in byId[id].routes) queue.Enqueue(route.targetNodeId);
        }
        if (reached.Count != nodes.Length) throw new ArgumentException("시작점에서 도달할 수 없는 노드가 있습니다.");
    }

    private static bool ValidId(string id) => !string.IsNullOrWhiteSpace(id) && id == id.Trim();

    public string GetDisplayName(string familyName)
    {
        string name = string.IsNullOrWhiteSpace(familyName) ? "이름 없는 가문" : familyName.Trim();
        return GameTextFormatter.Format(displayName, "FamilyName", name);
    }

    public ExpeditionRun CreateRun(string familyName = null)
    {
        Validate();
        var run = new ExpeditionRun(stageId, GetDisplayName(familyName), entryScene);
        run.Initialize(this);
        return run;
    }
}
