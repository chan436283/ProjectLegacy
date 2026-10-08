using System;
using System.Collections.Generic;

public enum ExpeditionPointKind { Start = 0, Normal = 1, Ending = 4 }

/// <summary>프리팹 또는 생성기가 전달하는 실행용 연결 값. Inspector 설정 원본이 아닙니다.</summary>
public sealed class ExpeditionConnectionData
{
    public string routeId { get; }
    public string targetNodeId { get; }
    /// <summary>방향을 포함한 내부 식별자. 구분 문자가 포함된 노드 ID도 충돌하지 않도록 이스케이프합니다.</summary>
    public static ExpeditionConnectionData FromEndpoints(string sourceNodeId, string targetNodeId)
    {
        if (string.IsNullOrWhiteSpace(sourceNodeId) || sourceNodeId != sourceNodeId.Trim() ||
            string.IsNullOrWhiteSpace(targetNodeId) || targetNodeId != targetNodeId.Trim())
            throw new ArgumentException("연결 양쪽에 유효한 노드 ID가 필요합니다.");
        return new ExpeditionConnectionData(
            Uri.EscapeDataString(sourceNodeId) + "->" + Uri.EscapeDataString(targetNodeId), targetNodeId);
    }

    public ExpeditionConnectionData(string routeId, string targetNodeId)
    { this.routeId = routeId; this.targetNodeId = targetNodeId; }
}

/// <summary>맵 포인트에서 읽어낸 실행용 값. Unity 컴포넌트를 참조하지 않습니다.</summary>
public sealed class ExpeditionPointData
{
    public string nodeId { get; }
    public string displayName { get; }
    public ExpeditionPointKind kind { get; }
    public StageContent content { get; }
    public string endingId { get; }
    public IReadOnlyList<ExpeditionConnectionData> routes { get; }
    public ExpeditionPointData(string nodeId, string displayName, ExpeditionPointKind kind,
        StageContent content, string endingId, IEnumerable<ExpeditionConnectionData> routes)
    {
        this.nodeId = nodeId; this.displayName = displayName ?? string.Empty; this.kind = kind;
        this.content = content; this.endingId = endingId;
        this.routes = new List<ExpeditionConnectionData>(routes ?? throw new ArgumentNullException(nameof(routes))).AsReadOnly();
    }
}

/// <summary>실행 시 생성하는 검증된 맵 스냅샷. 절차적 생성기도 같은 입력을 사용할 수 있습니다.</summary>
public sealed class ExpeditionMapData
{
    public string startNodeId { get; }
    public IReadOnlyList<ExpeditionPointData> nodes { get; }
    public ExpeditionMapData(string startNodeId, IEnumerable<ExpeditionPointData> nodes)
    {
        this.startNodeId = startNodeId;
        this.nodes = new List<ExpeditionPointData>(nodes ?? throw new ArgumentNullException(nameof(nodes))).AsReadOnly();
        Validate();
    }

    private void Validate()
    {
        if (nodes == null || nodes.Count == 0) throw new ArgumentException("스테이지 노드가 필요합니다.");
        var byId = new Dictionary<string, ExpeditionPointData>(StringComparer.Ordinal);
        var routeIds = new HashSet<string>(StringComparer.Ordinal);
        var endingIds = new HashSet<string>(StringComparer.Ordinal);
        int starts = 0;
        foreach (var node in nodes)
        {
            if (node == null || !ValidId(node.nodeId) ||
                !Enum.IsDefined(typeof(ExpeditionPointKind), node.kind) || node.routes == null || byId.ContainsKey(node.nodeId))
                throw new ArgumentException("노드 ID·종류·경로 목록을 확인하세요.");
            byId.Add(node.nodeId, node);
            if (node.kind == ExpeditionPointKind.Start) starts++;
            if (node.kind == ExpeditionPointKind.Ending)
            {
                if (!ValidId(node.endingId) || !endingIds.Add(node.endingId) || node.routes.Count != 0)
                    throw new ArgumentException("종착점은 고유한 결과 ID를 갖고 나가는 경로가 없어야 합니다.");
            }
            else if (node.routes.Count == 0) throw new ArgumentException("종착점 이외 노드에 막다른 길이 있습니다.");
            if (node.content != null) node.content.Validate();
            var targets = new HashSet<string>(StringComparer.Ordinal);
            foreach (var route in node.routes)
                if (route == null || !ValidId(route.routeId) || !routeIds.Add(route.routeId) ||
                    !ValidId(route.targetNodeId) || !targets.Add(route.targetNodeId))
                    throw new ArgumentException("경로 ID·대상을 확인하세요.");
        }
        if (!ValidId(startNodeId) || !byId.ContainsKey(startNodeId) || starts != 1 ||
            byId[startNodeId].kind != ExpeditionPointKind.Start || endingIds.Count == 0)
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
        if (processed != nodes.Count) throw new ArgumentException("고정 원정 경로에 순환이 있습니다.");
        var reached = new HashSet<string>();
        queue.Enqueue(startNodeId);
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            if (!reached.Add(id)) continue;
            foreach (var route in byId[id].routes) queue.Enqueue(route.targetNodeId);
        }
        if (reached.Count != nodes.Count) throw new ArgumentException("시작점에서 도달할 수 없는 노드가 있습니다.");
    }

    private static bool ValidId(string id) => !string.IsNullOrWhiteSpace(id) && id == id.Trim();
}
