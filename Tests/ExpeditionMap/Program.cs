using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

static class Program
{
    static void Set(object target, string field, object value) => target.GetType()
        .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    static T Get<T>(object target, string field) => (T)target.GetType()
        .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    static void Invoke(object target, string method, params object[] args) => target.GetType()
        .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
    static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        Console.WriteLine("PASS: " + message);
    }
    static void Reject(Action action)
    {
        try { action(); } catch (ArgumentException) { return; } catch (InvalidOperationException) { return; }
        throw new Exception("Invalid map configuration accepted");
    }
    static StageDefinition Stage() => new StageDefinition {
        stageId = "map", displayName = "지도", mapPrefab = Panel(out _) };
    static ExpeditionMapPoint Point(string id, float x, float y)
    {
        var point = new ExpeditionMapPoint();
        Set(point, "nodeId", id);
        Set(point, "hitArea", new Collider2D { transform = point.transform });
        Set(point, "marker", new SpriteRenderer { sprite = new Sprite(), transform = new Transform { parent = point.transform } });
        Set(point, "currentIndicator", new GameObject());
        point.transform.localPosition = new Vector3(x, y);
        return point;
    }
    static ExpeditionMap Panel(out ExpeditionMapPoint[] points)
    {
        points = new[] { Point("start", 0, 0), Point("fork", 0, 100), Point("south_end", -20, -100), Point("west_end", -100, 100) };
        var panel = new ExpeditionMap();
        points[0].Configure("start", "입구", null, false, null, new[] {
            new ExpeditionMapConnection { target = points[1] },
            new ExpeditionMapConnection { target = points[2] } });
        points[1].Configure("fork", "", new EventContent { contentId = "event" }, false, null, new[] {
            new ExpeditionMapConnection { target = points[3] } });
        points[2].Configure("south_end", "", new EventContent { contentId = "south_event" }, true, "south", null);
        points[3].Configure("west_end", "", null, true, "west", null);
        panel.children = points;
        panel.SetStartPoint(points[0]);
        return panel;
    }
    static void Main()
    {
        var forward = ExpeditionConnectionData.FromEndpoints("n1", "n2");
        var backward = ExpeditionConnectionData.FromEndpoints("n2", "n1");
        Check(forward.routeId == "n1->n2" && backward.routeId == "n2->n1",
            "automatic route IDs distinguish travel direction");
        Check(ExpeditionConnectionData.FromEndpoints("a->b", "c").routeId !=
            ExpeditionConnectionData.FromEndpoints("a", "b->c").routeId,
            "automatic route IDs escape separators to prevent collisions");
        var stage = Stage();
        var run = stage.CreateRun();
        var panel = Panel(out var points);
        int requests = 0;
        string requested = null;
        panel.RouteRequested += id => { requests++; requested = id; };
        var originalIcon = Get<SpriteRenderer>(points[1], "marker").sprite;
        Get<SpriteRenderer>(points[1], "marker").color = new Color(0.2f, 0.7f, 0.9f, 0.6f);
        panel.Show(run);
        var animatedMarker = Get<SpriteRenderer>(points[1], "marker").transform;
        var pulse = animatedMarker.lastTween;
        Check(pulse != null && pulse.active && pulse.loopCount == -1 && pulse.independentUpdate &&
            animatedMarker.localScale.x == 1.5f && points[1].HitArea.transform.localScale.x == 1f,
            "available marker pulses like MapPoint while collider scale remains fixed");
        panel.Refresh();
        Check(animatedMarker.lastTween == pulse, "refresh does not restart a running pulse");
        Check(points[1].DisplayName == string.Empty, "point name is optional and has no label dependency");
        Set(points[1], "displayName", "폐허 입구");
        panel.Refresh();
        Check(points[1].DisplayName == "폐허 입구", "shared UI can read the authored string name");
        Check(points[0].State == ExpeditionMapPointState.Current && points[1].HitArea.enabled && points[2].HitArea.enabled,
            "north and south neighbors are selectable regardless of coordinates");
        Check(points[3].State == ExpeditionMapPointState.Hidden,
            "undiscovered distant nodes stay hidden without route UI objects");
        Check(!points[1].IsDiscovered && Get<SpriteRenderer>(points[1], "marker").sprite == originalIcon,
            "undiscovered destination uses its authored icon without an unknown icon");
        Check(points[2].transform.localPosition.y == -100 && points[3].transform.localPosition.x == -100,
            "opening the map preserves manually authored positions");
        Check(!Get<SpriteRenderer>(points[3], "marker").enabled && !points[3].HitArea.enabled,
            "hidden world marker and collider are disabled");
        points[1].OnPointerClick(new PointerEventData(null) { button = PointerEventData.InputButton.Right });
        points[1].OnPointerClick(new PointerEventData(null) { dragging = true });
        points[1].OnPointerClick(new PointerEventData(null) { position = new Vector2(20, 0) });
        Check(requests == 0, "right clicks and drags cannot request travel");
        points[3].OnPointerClick(new PointerEventData(null)); points[0].OnPointerClick(new PointerEventData(null));
        Check(requests == 0, "hidden and current nodes cannot request movement");
        points[2].OnPointerClick(new PointerEventData(null)); points[2].OnPointerClick(new PointerEventData(null)); points[1].OnPointerClick(new PointerEventData(null));
        Check(requests == 1 && requested == "start->south_end" && panel.IsBusy && run.CurrentNodeId == "start",
            "click emits one route request and locks input without advancing the expedition");
        Check(!pulse.active && animatedMarker.localScale.x == 1f,
            "pending move stops available pulse and restores scale");
        panel.Hide();
        Check(!panel.gameObject.activeSelf && !panel.IsVisible, "Hide deactivates the world map root without UIPanel");
        panel.Show(run); points[1].OnPointerClick(new PointerEventData(null));
        Check(requests == 1 && panel.IsBusy, "hide and reopen cannot bypass a pending request");
        panel.SetBusy(false); points[1].OnPointerClick(new PointerEventData(null));
        Check(requests == 2 && requested == "start->fork", "caller can release a rejected request and retry");
        run.ChooseRoute(requested); panel.SetBusy(false);
        Check(points[1].State == ExpeditionMapPointState.Current && points[1].IsDiscovered &&
            !points[3].HitArea.enabled && points[3].State == ExpeditionMapPointState.Idle &&
            !points[3].IsDiscovered && points[0].State == ExpeditionMapPointState.Idle,
            "node data reveals the next destination but keeps it locked until content completion");
        var currentColor = Get<SpriteRenderer>(points[1], "marker").color;
        Check(currentColor.r == 0.5f && currentColor.g == 0.5f && currentColor.b == 0.5f && currentColor.a == 0.6f,
            "unavailable current point turns gray while preserving authored alpha");
        run.CompleteCurrentNode("fork"); panel.Refresh();
        Check(points[3].HitArea.enabled && !points[2].HitArea.enabled,
            "completion unlocks the westward route");
        run.ChooseRoute("fork->west_end"); // Deliberately omit Refresh to simulate a stale UI.
        points[3].OnPointerClick(new PointerEventData(null));
        Check(requests == 2 && !points[3].HitArea.enabled, "stale clickable view rechecks live route eligibility");
        Check(run.Phase == ExpeditionRun.RunPhase.Completed && Get<GameObject>(points[3], "currentIndicator").activeSelf,
            "completed endpoint retains only current-position presentation");

        var remembered = stage.CreateRun(); remembered.AttachExploration(run.Exploration);
        panel.Show(remembered);
        var restoredColor = Get<SpriteRenderer>(points[1], "marker").color;
        Check(restoredColor.r == 0.2f && restoredColor.g == 0.7f && restoredColor.b == 0.9f && restoredColor.a == 0.6f,
            "available point restores its authored color");
        Check(points[1].IsDiscovered &&
            points[3].State == ExpeditionMapPointState.Idle && !points[3].HitArea.enabled,
            "past discoveries are visible but do not unlock routes or mark this run completed");
        panel.Hide(); points[1].OnPointerClick(new PointerEventData(null));
        Check(requests == 2, "hidden panel rejects movement");
        panel.Show(remembered); panel.Show(remembered); points[1].OnPointerClick(new PointerEventData(null));
        Check(requests == 3, "repeated Show does not duplicate click subscriptions");
        panel.SetBusy(false);
        Invoke(panel, "OnReleased"); points[1].OnPointerClick(new PointerEventData(null));
        Check(requests == 3, "released panel unsubscribes point events");

        var lifecyclePoint = Point("test", 0, 0);
        var lifecycleMarker = Get<SpriteRenderer>(lifecyclePoint, "marker").transform;
        lifecycleMarker.localScale = new Vector3(2, 3, 1);
        lifecyclePoint.SetPresentation(true, ExpeditionMapPointState.Available, true);
        var lifecyclePulse = lifecycleMarker.lastTween;
        lifecyclePoint.enabled = false;
        Invoke(lifecyclePoint, "OnDisabled");
        Check(!lifecyclePulse.active && lifecycleMarker.localScale.x == 2 && lifecycleMarker.localScale.y == 3,
            "disable kills pulse and restores authored nonuniform scale");
        lifecyclePoint.enabled = true;
        Invoke(lifecyclePoint, "OnEnabled");
        Check(lifecycleMarker.lastTween != lifecyclePulse && lifecycleMarker.lastTween.active,
            "reenable restarts eligible animation");
        lifecyclePoint.SetPresentation(true, ExpeditionMapPointState.Current, true);
        Check(!lifecycleMarker.lastTween.active && lifecycleMarker.localScale.x == 2,
            "current position does not pulse as an available destination");
        lifecyclePoint.SetPresentation(true, ExpeditionMapPointState.Available, true);
        Invoke(lifecyclePoint, "OnReleased");
        Check(!lifecycleMarker.lastTween.active && lifecycleMarker.localScale.x == 2,
            "destroy cleans up looping animation");
        var invalidMarker = Point("test", 0, 0);
        Set(invalidMarker, "marker", new SpriteRenderer { sprite = new Sprite(), transform = invalidMarker.transform });
        Reject(invalidMarker.Initialize);

        var original = Stage(); var snapshotRun = original.CreateRun();
        var snapshotPanel = Panel(out var snapshotPoints);
        snapshotPanel.RouteRequested += _ => { };
        var record = new StageExplorationData("map");
        snapshotRun.AttachExploration(record);
        Check(!snapshotRun.IsInitialized && snapshotRun.AvailableRoutes.Count == 0 && !record.KnowsNode("start"),
            "departure metadata does not start exploration before map initialization");
        snapshotPanel.Show(snapshotRun);
        Check(snapshotRun.IsInitialized && record.KnowsNode("start"),
            "loaded map initializes the run and preserves previously attached exploration storage");
        Set(snapshotPoints[1], "displayName", "edited");
        snapshotPoints[0].Connections[0].target = snapshotPoints[3];
        Check(snapshotRun.GetMapNodes()[0].Routes[0].TargetNodeId == "fork" && snapshotRun.CurrentNodeName == "입구",
            "run connections are copied from prefab points and do not follow later authoring edits");
        Reject(() => snapshotRun.Initialize(original.mapPrefab.BuildMapData()));
        var invalid = Panel(out var invalidPoints);
        invalidPoints[0].Connections[0].target = Point("outside", 0, 0);
        Reject(() => invalid.BuildMapData());
        invalidPoints[0].Connections[0].target = invalidPoints[1];
        Set(invalidPoints[1], "nodeId", "start"); Reject(() => invalid.BuildMapData());
        Set(invalidPoints[1], "nodeId", "fork");
        invalidPoints[0].Connections[0].target = invalidPoints[2]; Reject(() => invalid.BuildMapData());
        invalidPoints[0].Connections[0].target = invalidPoints[1];
        invalid.SetStartPoint(null); Reject(() => invalid.BuildMapData());
        invalid.SetStartPoint(invalidPoints[0]);
        Set(invalidPoints[1], "hitArea", invalidPoints[0].HitArea);
        var untouchedRun = original.CreateRun(); Reject(() => invalid.Show(untouchedRun));
        Check(!untouchedRun.IsInitialized, "invalid visual setup does not advance run state");
        Console.WriteLine("PASS: external targets, duplicate IDs, missing start and shared colliders are rejected");
        var noMap = new StageDefinition { stageId = "missing", displayName = "missing" };
        Reject(noMap.Validate);
    }
}
