using System;
using System.Collections.Generic;

/// <summary>출발 시 확정한 원정 정보입니다. 현재는 메모리에서만 유지합니다.</summary>
public sealed class ExpeditionRun
{
    public string StageId { get; }
    public string StageName { get; }
    public string EntryScene { get; }

    public ExpeditionRun(string stageId, string stageName, string entryScene)
    {
        if (string.IsNullOrWhiteSpace(stageId) || string.IsNullOrWhiteSpace(stageName) ||
            string.IsNullOrWhiteSpace(entryScene))
            throw new ArgumentException("스테이지 ID, 이름, 진입 씬이 필요합니다.");
        StageId = stageId.Trim();
        StageName = stageName.Trim();
        EntryScene = entryScene.Trim();
    }

    public enum RunPhase { ResolvingNode, ChoosingRoute, Completed }
    public sealed class RouteOption
    {
        public string RouteId { get; }
        public string TargetNodeId { get; }
        public string Label { get; }
        internal RouteOption(StageRoute route)
        { RouteId = route.routeId; TargetNodeId = route.targetNodeId; Label = route.label; }
    }
    private sealed class NodeSnapshot
    {
        public string id, name, endingId;
        public StageNodeType kind;
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
    public StageNodeType CurrentNodeKind => current != null ? current.kind : throw new InvalidOperationException("경로가 초기화되지 않았습니다.");
    public StageContent CurrentContent => current?.content;
    public string EndingId { get; private set; }
    public RunPhase Phase { get; private set; }
    public StageExplorationData Exploration { get; private set; }
    public IReadOnlyList<string> VisitedNodes => visited.AsReadOnly();
    public IReadOnlyList<string> TraversedRoutes => traversed.AsReadOnly();
    public IReadOnlyList<string> CompletedNodes => completed.AsReadOnly();
    public IReadOnlyList<RouteOption> AvailableRoutes => Phase == RunPhase.ChoosingRoute && current != null
        ? current.routes.AsReadOnly() : (IReadOnlyList<RouteOption>)Array.Empty<RouteOption>();

    internal void Initialize(StageDefinition stage)
    {
        if (current != null) throw new InvalidOperationException("이미 초기화한 원정입니다.");
        Exploration = new StageExplorationData(StageId);
        foreach (var node in stage.nodes)
        {
            var routes = new List<RouteOption>();
            foreach (var route in node.routes) routes.Add(new RouteOption(route));
            graph.Add(node.nodeId, new NodeSnapshot { id = node.nodeId, name = node.displayName,
                kind = node.kind, content = node.content, endingId = node.endingId, routes = routes });
        }
        Enter(stage.startNodeId);
    }

    internal void AttachExploration(StageExplorationData record)
    {
        if (Exploration == null) return; // 기존 메타데이터 전용 생성자 호환
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
        if (current.kind == StageNodeType.Ending)
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
