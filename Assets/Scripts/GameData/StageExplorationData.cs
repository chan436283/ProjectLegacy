using System;
using System.Collections.Generic;

/// <summary>원정을 반복해도 유지하는 발견 기록. 이번 원정의 완료 상태와 별개입니다.</summary>
public sealed class StageExplorationData
{
    public string StageId { get; }
    private readonly HashSet<string> nodes = new(StringComparer.Ordinal);
    private readonly HashSet<string> routes = new(StringComparer.Ordinal);
    private readonly HashSet<string> endings = new(StringComparer.Ordinal);
    public IReadOnlyCollection<string> DiscoveredNodes => new List<string>(nodes).AsReadOnly();
    public IReadOnlyCollection<string> DiscoveredRoutes => new List<string>(routes).AsReadOnly();
    public IReadOnlyCollection<string> ReachedEndings => new List<string>(endings).AsReadOnly();
    public StageExplorationData(string stageId)
    {
        if (string.IsNullOrWhiteSpace(stageId)) throw new ArgumentException("스테이지 ID가 필요합니다.");
        StageId = stageId;
    }
    public bool KnowsNode(string id) => nodes.Contains(id);
    internal void DiscoverNode(string id) => nodes.Add(id);
    internal void DiscoverRoute(string id) => routes.Add(id);
    internal void ReachEnding(string id) => endings.Add(id);
    internal void Merge(StageExplorationData source)
    {
        if (source.StageId != StageId) throw new ArgumentException("다른 스테이지의 기록입니다.");
        nodes.UnionWith(source.nodes);
        routes.UnionWith(source.routes);
        endings.UnionWith(source.endings);
    }
}
