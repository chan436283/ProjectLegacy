using System;
using System.Collections.Generic;
using CWFramework;
using UnityEngine;

/// <summary>원정 월드 지도. 이동 요청만 전달하며 지점 완료나 전투 실행은 진행 컨트롤러가 담당합니다.</summary>
[DisallowMultipleComponent]
public sealed class ExpeditionMap : CBehaviour
{
    [SerializeField] private ExpeditionMapPoint startPoint;
    [SerializeField] private SpriteRenderer background;
    public SpriteRenderer Background => background;
    private ExpeditionMapPoint[] points = Array.Empty<ExpeditionMapPoint>();

    public bool IsVisible { get; private set; }
    public bool IsBusy { get; private set; }
    public ExpeditionRun Run { get; private set; }
    /// <summary>routeId를 전달합니다. 수신 측에서 ChooseRoute 및 콘텐츠 실행 후 SetBusy(false)를 호출합니다.</summary>
    public event Action<string> RouteRequested;

    private Dictionary<string, ExpeditionRun.MapNodeInfo> nodes;
    private readonly List<ExpeditionMapPoint> subscribedPoints = new();

    protected override void OnAwake()
    {
        if (Run == null) gameObject.SetActive(false);
    }

    /// <summary>프리팹 설정 또는 생성된 포인트를 실행용 데이터로 복사합니다. 표시 상태를 변경하지 않습니다.</summary>
    public ExpeditionMapData BuildMapData()
    {
        points = GetComponentsInChildren<ExpeditionMapPoint>(true);
        var members = new HashSet<ExpeditionMapPoint>(points);
        if (startPoint == null || !members.Contains(startPoint) || startPoint.IsEnding)
            throw new InvalidOperationException("맵 내부의 시작 포인트를 지정해야 합니다.");
        var data = new List<ExpeditionPointData>(points.Length);
        foreach (var point in points)
        {
            if (point.Connections == null) throw new InvalidOperationException("연결 목록이 비어 있습니다.");
            var routes = new List<ExpeditionConnectionData>();
            foreach (var connection in point.Connections)
            {
                if (connection == null || connection.target == null || !members.Contains(connection.target))
                    throw new InvalidOperationException("경로 목적지는 같은 맵 내부의 포인트여야 합니다.");
                routes.Add(ExpeditionConnectionData.FromEndpoints(point.NodeId, connection.target.NodeId));
            }
            data.Add(new ExpeditionPointData(point.NodeId, point.DisplayName,
                point == startPoint ? ExpeditionPointKind.Start : point.IsEnding ? ExpeditionPointKind.Ending : ExpeditionPointKind.Normal,
                point.Content, point.EndingId, routes));
        }
        return new ExpeditionMapData(startPoint.NodeId, data);
    }

    /// <summary>생성된 자식 포인트의 연결 설정을 마친 뒤, Show 전에 시작점을 지정합니다.</summary>
    public void SetStartPoint(ExpeditionMapPoint point)
    {
        if (Run != null) throw new InvalidOperationException("진행 중인 맵의 시작점은 변경할 수 없습니다.");
        startPoint = point;
    }

    public void Show(ExpeditionRun run)
    {
        if (run == null) throw new ArgumentNullException(nameof(run));
        if (!enabled) throw new InvalidOperationException("비활성 지도 컴포넌트를 열 수 없습니다.");
        var mapData = BuildMapData();
        // 시각 설정까지 검증한 뒤 진행 데이터를 변경합니다.
        var colliders = new HashSet<Collider2D>();
        foreach (var point in points)
        {
            point.Initialize();
            if (!colliders.Add(point.HitArea))
                throw new InvalidOperationException("지도 지점의 클릭 영역이 중복되었습니다.");
        }
        if (!run.IsInitialized) run.Initialize(mapData);

        var nextNodes = new Dictionary<string, ExpeditionRun.MapNodeInfo>(StringComparer.Ordinal);
        foreach (var node in run.GetMapNodes())
            nextNodes.Add(node.NodeId, node);
        var pointById = new Dictionary<string, ExpeditionMapPoint>(StringComparer.Ordinal);
        foreach (var point in points)
        {
            if (point == null) throw new InvalidOperationException("지도 지점 참조가 비어 있습니다.");
            if (!nextNodes.ContainsKey(point.NodeId) || pointById.ContainsKey(point.NodeId))
                throw new InvalidOperationException("지도 지점의 ID·클릭 영역이 중복되었거나 원정에 없는 지점입니다.");
            pointById.Add(point.NodeId, point);
        }
        if (pointById.Count != nextNodes.Count)
            throw new InvalidOperationException("원정의 모든 지점을 지도에 연결해야 합니다.");

        UnsubscribePoints();
        foreach (var point in points)
        {
            point.Clicked += RequestMove;
            subscribedPoints.Add(point);
        }
        if (Run != run) IsBusy = false;
        Run = run;
        nodes = nextNodes;
        IsVisible = true;
        gameObject.SetActive(true);
        Refresh();
    }

    /// <summary>외부에서 이동·콘텐츠 완료·발견 기록이 바뀌면 호출합니다.</summary>
    public void Refresh()
    {
        if (Run == null || nodes == null) return;
        var knownRoutes = new HashSet<string>(Run.Exploration.DiscoveredRoutes, StringComparer.Ordinal);
        var visited = new HashSet<string>(Run.VisitedNodes, StringComparer.Ordinal);
        var availableNodes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var route in Run.AvailableRoutes)
            availableNodes.Add(route.TargetNodeId);
        var visibleNodes = new HashSet<string>(visited, StringComparer.Ordinal);
        // 지점 공개 여부도 원정의 연결 데이터로 판단합니다. 지도에 별도 경로 오브젝트는 필요하지 않습니다.
        foreach (var node in nodes.Values)
        {
            foreach (var route in node.Routes)
            {
                if (!knownRoutes.Contains(route.RouteId)) continue;
                visibleNodes.Add(node.NodeId);
                visibleNodes.Add(route.TargetNodeId);
            }
        }
        bool inputEnabled = IsVisible && isActiveAndEnabled && !IsBusy && RouteRequested != null;
        foreach (var point in points)
        {
            string id = point.NodeId;
            bool discovered = Run.Exploration.KnowsNode(id);
            var state = id == Run.CurrentNodeId ? ExpeditionMapPointState.Current :
                availableNodes.Contains(id) ? ExpeditionMapPointState.Available :
                discovered || visibleNodes.Contains(id) ? ExpeditionMapPointState.Idle : ExpeditionMapPointState.Hidden;
            point.SetPresentation(discovered, state, inputEnabled);
        }
    }

    private void RequestMove(ExpeditionMapPoint point)
    {
        if (!IsVisible || !isActiveAndEnabled || IsBusy || Run == null || RouteRequested == null) return;
        // 화면 표시 이후 진행 상태가 바뀌었더라도 현재 경로를 기준으로 다시 판단합니다.
        foreach (var route in Run.AvailableRoutes)
        {
            if (route.TargetNodeId != point.NodeId) continue;
            SetBusy(true);
            RouteRequested.Invoke(route.RouteId);
            return;
        }
        Refresh();
    }

    public bool TryGetPoint(string nodeId, out ExpeditionMapPoint point)
    {
        point = Array.Find(points, candidate => candidate != null && candidate.NodeId == nodeId);
        return point != null;
    }

    public void SetBusy(bool value)
    {
        IsBusy = value;
        Refresh();
    }

    public void Hide()
    {
        IsVisible = false;
        Refresh();
        gameObject.SetActive(false);
    }

    protected override void OnDisabled()
    {
        IsVisible = false;
        Refresh();
    }

    private void UnsubscribePoints()
    {
        foreach (var point in subscribedPoints)
            if (point != null) point.Clicked -= RequestMove;
        subscribedPoints.Clear();
    }

    protected override void OnReleased() => UnsubscribePoints();
}
