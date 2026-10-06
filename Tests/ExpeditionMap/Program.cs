using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        stageId = "map", displayName = "지도", nodes = new[] {
            new StageNode { nodeId = "start", displayName = "입구", kind = StageNodeType.Start, routes = new[] {
                new StageRoute { routeId = "north", targetNodeId = "fork", label = "북쪽 길" },
                new StageRoute { routeId = "south", targetNodeId = "south_end", label = "남쪽 길" } } },
            new StageNode { nodeId = "fork", displayName = "폐허", kind = StageNodeType.Normal,
                content = new EventContent { contentId = "event" }, routes = new[] {
                    new StageRoute { routeId = "west", targetNodeId = "west_end", label = "서쪽 길" } } },
            new StageNode { nodeId = "south_end", displayName = "남쪽 출구", kind = StageNodeType.Ending, endingId = "south",
                content = new EventContent { contentId = "south_event" } },
            new StageNode { nodeId = "west_end", displayName = "서쪽 출구", kind = StageNodeType.Ending, endingId = "west" }
        }
    };
    static ExpeditionMapPoint Point(string id, float x, float y)
    {
        var point = new ExpeditionMapPoint();
        Set(point, "nodeId", id);
        Set(point, "button", new Button());
        Set(point, "label", new TMP_Text());
        Set(point, "icon", new Image { sprite = new Sprite() });
        Set(point, "unknownIcon", new Sprite());
        Set(point, "completedIndicator", new GameObject());
        point.Components[typeof(CanvasGroup)] = new CanvasGroup();
        point.transform.localPosition = new Vector3(x, y);
        return point;
    }
    static ExpeditionMapPanel Panel(out ExpeditionMapPoint[] points)
    {
        points = new[] { Point("start", 0, 0), Point("fork", 0, 100), Point("south_end", -20, -100), Point("west_end", -100, 100) };
        var panel = new ExpeditionMapPanel();
        Set(panel, "stageId", "map"); Set(panel, "panel", new UIPanel());
        Set(panel, "points", points);
        return panel;
    }
    static void Main()
    {
        var stage = Stage();
        var run = stage.CreateRun();
        var panel = Panel(out var points);
        int requests = 0;
        string requested = null;
        panel.RouteRequested += id => { requests++; requested = id; };
        panel.Open(run);
        Check(points[0].State == ExpeditionMapPointState.Current && points[1].Button.interactable && points[2].Button.interactable,
            "north and south neighbors are selectable regardless of coordinates");
        Check(points[3].State == ExpeditionMapPointState.Hidden,
            "undiscovered distant nodes stay hidden without route UI objects");
        Check(Get<TMP_Text>(points[1], "label").text == "?" && Get<Image>(points[1], "icon").sprite == Get<Sprite>(points[1], "unknownIcon"),
            "reachable but undiscovered destination does not reveal name or icon");
        Check(points[2].transform.localPosition.y == -100 && points[3].transform.localPosition.x == -100,
            "opening the map preserves manually authored positions");
        points[3].Button.onClick.Invoke(); points[0].Button.onClick.Invoke();
        Check(requests == 0, "hidden and current nodes cannot request movement");
        points[2].Button.onClick.Invoke(); points[2].Button.onClick.Invoke(); points[1].Button.onClick.Invoke();
        Check(requests == 1 && requested == "south" && panel.IsBusy && run.CurrentNodeId == "start",
            "click emits one route request and locks input without advancing the expedition");
        panel.HideImmediate(); panel.Open(run); points[1].Button.onClick.Invoke();
        Check(requests == 1 && panel.IsBusy, "hide and reopen cannot bypass a pending request");
        panel.SetBusy(false); points[1].Button.onClick.Invoke();
        Check(requests == 2 && requested == "north", "caller can release a rejected request and retry");
        run.ChooseRoute(requested); panel.SetBusy(false);
        Check(points[1].State == ExpeditionMapPointState.Current && Get<TMP_Text>(points[1], "label").text == "폐허" &&
            !points[3].Button.interactable && points[3].State == ExpeditionMapPointState.Unvisited &&
            Get<TMP_Text>(points[3], "label").text == "?" && points[0].State == ExpeditionMapPointState.Completed,
            "node data reveals the next destination but keeps it locked until content completion");
        run.CompleteCurrentNode("fork"); panel.Refresh();
        Check(points[3].Button.interactable && !points[2].Button.interactable,
            "completion unlocks the westward route");
        run.ChooseRoute("west"); // Deliberately omit Refresh to simulate a stale UI.
        points[3].Button.onClick.Invoke();
        Check(requests == 2 && !points[3].Button.interactable, "stale clickable view rechecks live route eligibility");
        Check(run.Phase == ExpeditionRun.RunPhase.Completed && Get<GameObject>(points[3], "completedIndicator").activeSelf,
            "completed endpoint displays completion with current position");

        var remembered = stage.CreateRun(); remembered.AttachExploration(run.Exploration);
        panel.Open(remembered);
        Check(Get<TMP_Text>(points[1], "label").text == "폐허" &&
            points[3].State == ExpeditionMapPointState.Unvisited && !points[3].Button.interactable,
            "past discoveries are visible but do not unlock routes or mark this run completed");
        panel.HideImmediate(); points[1].Button.onClick.Invoke();
        Check(requests == 2, "hidden panel rejects movement");
        panel.Open(remembered); panel.Open(remembered); points[1].Button.onClick.Invoke();
        Check(requests == 3, "repeated Open does not duplicate click subscriptions");
        panel.SetBusy(false);
        Invoke(panel, "OnReleased"); points[1].Button.onClick.Invoke();
        Check(requests == 3, "released panel unsubscribes point events");

        var original = Stage(); var snapshotRun = original.CreateRun();
        original.nodes[1].displayName = "edited"; original.nodes[0].routes[0].targetNodeId = "edited";
        var mapNodes = snapshotRun.GetMapNodes();
        Check(mapNodes[1].DisplayName == "폐허" && mapNodes[0].Routes[0].TargetNodeId == "fork",
            "UI topology and names come from the run snapshot, not mutable stage assets");
        var invalid = Panel(out var invalidPoints);
        Set(invalid, "stageId", "other"); Reject(() => invalid.Open(snapshotRun));
        Set(invalid, "stageId", "map");
        Set(invalid, "points", invalidPoints.Take(3).ToArray()); Reject(() => invalid.Open(snapshotRun));
        Set(invalid, "points", invalidPoints);
        Set(invalidPoints[1], "nodeId", "start"); Reject(() => invalid.Open(snapshotRun));
        Set(invalidPoints[1], "nodeId", "fork");
        Set(invalidPoints[1], "button", invalidPoints[0].Button); Reject(() => invalid.Open(snapshotRun));
        Console.WriteLine("PASS: mismatched stage, missing/duplicate points and shared buttons are rejected");
        var snapshotPanel = Panel(out var snapshotPoints);
        snapshotPanel.RouteRequested += _ => { };
        snapshotPanel.Open(snapshotRun);
        Check(snapshotPoints[1].Button.interactable && snapshotPoints[2].Button.interactable &&
            snapshotPoints[3].State == ExpeditionMapPointState.Hidden,
            "snapshot connections drive the map after the source stage is edited");
    }
}
