using System;
using System.Collections.Generic;
using CWFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>원정 내부 지도 UI. 이동 요청만 전달하며 지점 완료나 전투 실행은 진행 컨트롤러가 담당합니다.</summary>
[DisallowMultipleComponent]
public sealed class ExpeditionMapPanel : CBehaviour
{
    [SerializeField] private string stageId;
    [SerializeField] private UIPanel panel;
    [SerializeField] private TMP_Text stageNameText;
    [SerializeField] private TMP_Text currentNodeText;
    [SerializeField] private ExpeditionMapPoint[] points = Array.Empty<ExpeditionMapPoint>();

    public bool IsOpen { get; private set; }
    public bool IsBusy { get; private set; }
    public ExpeditionRun Run { get; private set; }
    /// <summary>routeId를 전달합니다. 수신 측에서 ChooseRoute 및 콘텐츠 실행 후 SetBusy(false)를 호출합니다.</summary>
    public event Action<string> RouteRequested;

    private Dictionary<string, ExpeditionRun.MapNodeInfo> nodes;
    private readonly List<ExpeditionMapPoint> subscribedPoints = new();

    protected override void OnAwake()
    {
        if (Run != null) return; // 비활성 패널을 Open으로 처음 여는 경우를 보존합니다.
        if (panel == null) panel = GetComponent<UIPanel>();
        if (panel != null) panel.SetVisibleImmediate(false);
    }

    public void Open(ExpeditionRun run)
    {
        if (run == null) throw new ArgumentNullException(nameof(run));
        if (!enabled) throw new InvalidOperationException("비활성 지도 컴포넌트를 열 수 없습니다.");
        if (panel == null) panel = GetComponent<UIPanel>();
        if (panel == null || points == null || string.IsNullOrWhiteSpace(stageId) ||
            stageId != run.StageId || run.CurrentNodeId == null)
            throw new InvalidOperationException("지도 패널·스테이지 ID·지점과 초기화된 원정을 확인하세요.");

        var nextNodes = new Dictionary<string, ExpeditionRun.MapNodeInfo>(StringComparer.Ordinal);
        foreach (var node in run.GetMapNodes())
            nextNodes.Add(node.NodeId, node);
        var pointById = new Dictionary<string, ExpeditionMapPoint>(StringComparer.Ordinal);
        var buttons = new HashSet<Button>();
        foreach (var point in points)
        {
            if (point == null) throw new InvalidOperationException("지도 지점 참조가 비어 있습니다.");
            point.Initialize();
            if (!nextNodes.ContainsKey(point.NodeId) || pointById.ContainsKey(point.NodeId) || !buttons.Add(point.Button))
                throw new InvalidOperationException("지도 지점의 ID·버튼이 중복되었거나 원정에 없는 지점입니다.");
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
        IsOpen = true;
        panel.SetVisibleImmediate(true);
        Refresh();
    }

    /// <summary>외부에서 이동·콘텐츠 완료·발견 기록이 바뀌면 호출합니다.</summary>
    public void Refresh()
    {
        if (Run == null || nodes == null) return;
        var knownRoutes = new HashSet<string>(Run.Exploration.DiscoveredRoutes, StringComparer.Ordinal);
        var visited = new HashSet<string>(Run.VisitedNodes, StringComparer.Ordinal);
        var completed = new HashSet<string>(Run.CompletedNodes, StringComparer.Ordinal);
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
        bool inputEnabled = IsOpen && isActiveAndEnabled && !IsBusy && RouteRequested != null;
        foreach (var point in points)
        {
            string id = point.NodeId;
            bool discovered = Run.Exploration.KnowsNode(id);
            var state = id == Run.CurrentNodeId ? ExpeditionMapPointState.Current :
                availableNodes.Contains(id) ? ExpeditionMapPointState.Available :
                completed.Contains(id) ? ExpeditionMapPointState.Completed :
                visited.Contains(id) ? ExpeditionMapPointState.Visited :
                discovered || visibleNodes.Contains(id) ? ExpeditionMapPointState.Unvisited : ExpeditionMapPointState.Hidden;
            point.SetPresentation(nodes[id].DisplayName, discovered, state, inputEnabled, completed.Contains(id));
        }
        if (stageNameText != null) stageNameText.text = Run.StageName;
        if (currentNodeText != null) currentNodeText.text = Run.CurrentNodeName;
    }

    private void RequestMove(ExpeditionMapPoint point)
    {
        if (!IsOpen || !isActiveAndEnabled || IsBusy || Run == null || RouteRequested == null) return;
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

    public void SetBusy(bool value)
    {
        IsBusy = value;
        Refresh();
    }

    public void HideImmediate()
    {
        IsOpen = false;
        Refresh();
        if (panel != null) panel.SetVisibleImmediate(false);
    }

    protected override void OnDisabled()
    {
        IsOpen = false;
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
