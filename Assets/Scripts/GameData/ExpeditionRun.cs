using System;
using System.Collections.Generic;

/// <summary>맵 로드 후 초기화하는 원정 진행 정보입니다. 현재는 메모리에서만 유지합니다.</summary>
public sealed class ExpeditionRun
{
    public string StageId { get; }
    public string StageName { get; }
    public string EntryScene { get; }
    public ExpeditionMap MapPrefab { get; }
    public bool IsInitialized => current != null;

    public ExpeditionRun(string stageId, string stageName, string entryScene, ExpeditionMap mapPrefab = null)
    {
        if (string.IsNullOrWhiteSpace(stageId) || string.IsNullOrWhiteSpace(stageName) ||
            string.IsNullOrWhiteSpace(entryScene))
            throw new ArgumentException("스테이지 ID, 이름, 진입 씬이 필요합니다.");
        StageId = stageId.Trim();
        StageName = stageName.Trim();
        EntryScene = entryScene.Trim();
        MapPrefab = mapPrefab;
        Exploration = new StageExplorationData(StageId);
    }

    public enum RunPhase { Uninitialized = -1, ResolvingNode = 0, ChoosingRoute = 1, Completed = 2 }
    public sealed class RouteOption
    {
        public string RouteId { get; }
        public string TargetNodeId { get; }
        internal RouteOption(ExpeditionConnectionData route)
        { RouteId = route.routeId; TargetNodeId = route.targetNodeId; }
    }

    /// <summary>지도 UI용 읽기 전용 정보. 공개 여부는 Exploration으로 판단합니다.</summary>
    public sealed class MapNodeInfo
    {
        public string NodeId { get; }
        public string DisplayName { get; }
        public IReadOnlyList<RouteOption> Routes { get; }

        internal MapNodeInfo(string nodeId, string displayName, IReadOnlyList<RouteOption> routes)
        { NodeId = nodeId; DisplayName = displayName; Routes = routes; }
    }

    /// <summary>맵 초기화 시 확정한 경로를 반환합니다. 이후 포인트 설정 변경의 영향을 받지 않습니다.</summary>
    public IReadOnlyList<MapNodeInfo> GetMapNodes()
    {
        var result = new List<MapNodeInfo>(graph.Count);
        foreach (var node in graph.Values)
            result.Add(new MapNodeInfo(node.id, node.name, node.routes.AsReadOnly()));
        return result.AsReadOnly();
    }
    private sealed class NodeSnapshot
    {
        public string id, name, endingId;
        public ExpeditionPointKind kind;
        public StageContent content;
        public List<RouteOption> routes;
    }
    private readonly Dictionary<string, NodeSnapshot> graph = new(StringComparer.Ordinal);
    private readonly List<string> visited = new();
    private readonly List<string> traversed = new();
    private readonly List<string> completed = new();
    private NodeSnapshot current;
    public string CurrentNodeId => current?.id;
    public string CurrentNodeName => current?.name;
    public ExpeditionPointKind CurrentNodeKind => current != null ? current.kind : throw new InvalidOperationException("경로가 초기화되지 않았습니다.");
    public StageContent CurrentContent => current?.content;
    public string EndingId { get; private set; }
    public RunPhase Phase { get; private set; } = RunPhase.Uninitialized;
    public StageExplorationData Exploration { get; private set; }
    public IReadOnlyList<string> VisitedNodes => visited.AsReadOnly();
    public IReadOnlyList<string> TraversedRoutes => traversed.AsReadOnly();
    public IReadOnlyList<string> CompletedNodes => completed.AsReadOnly();
    public IReadOnlyList<RouteOption> AvailableRoutes => Phase == RunPhase.ChoosingRoute && current != null
        ? current.routes.AsReadOnly() : (IReadOnlyList<RouteOption>)Array.Empty<RouteOption>();

    public void Initialize(ExpeditionMapData map)
    {
        if (current != null) throw new InvalidOperationException("이미 초기화한 원정입니다.");
        if (map == null) throw new ArgumentNullException(nameof(map));
        foreach (var node in map.nodes)
        {
            var routes = new List<RouteOption>();
            foreach (var route in node.routes) routes.Add(new RouteOption(route));
            graph.Add(node.nodeId, new NodeSnapshot { id = node.nodeId, name = node.displayName,
                kind = node.kind, content = node.content, endingId = node.endingId, routes = routes });
        }
        Enter(map.startNodeId);
    }

    internal void AttachExploration(StageExplorationData record)
    {
        if (record == null) throw new ArgumentNullException(nameof(record));
        record.Merge(Exploration);
        Exploration = record;
    }

    private void Enter(string nodeId)
    {
        current = graph[nodeId];
        visited.Add(nodeId);
        Exploration.DiscoverNode(nodeId);
        // 연결 존재는 알지만 방문 전에는 목적지의 콘텐츠 정체를 발견하지 않습니다.
        foreach (var route in current.routes) Exploration.DiscoverRoute(route.RouteId);
        Phase = RunPhase.ResolvingNode;
        if (current.content == null)
            CompleteCurrentNode(nodeId);
    }

    /// <summary>전투 승리나 이벤트 완료 시 호출합니다. 늦게 도착한 이전 노드 콜백은 거절합니다.</summary>
    public void CompleteCurrentNode(string expectedNodeId)
    {
        if (current == null || current.id != expectedNodeId || Phase != RunPhase.ResolvingNode)
            throw new InvalidOperationException("현재 처리 중인 노드만 완료할 수 있습니다.");
        completed.Add(current.id);
        if (current.kind == ExpeditionPointKind.Ending)
        {
            EndingId = current.endingId;
            Exploration.ReachEnding(EndingId);
            Phase = RunPhase.Completed;
        }
        else Phase = RunPhase.ChoosingRoute;
    }

    public void ChooseRoute(string routeId)
    {
        if (current == null || Phase != RunPhase.ChoosingRoute)
            throw new InvalidOperationException("노드를 완료한 뒤 경로를 선택해야 합니다.");
        var route = current.routes.Find(r => r.RouteId == routeId);
        if (route == null) throw new ArgumentException("현재 지점에서 이동할 수 없는 경로입니다.", nameof(routeId));
        traversed.Add(routeId);
        Enter(route.TargetNodeId);
    }
}
