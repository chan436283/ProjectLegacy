using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

static class Program
{
    static void Set(object target, string field, object value) => target.GetType()
        .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    static T Get<T>(object target, string field) => (T)target.GetType()
        .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    static void Invoke(object target, string method) => target.GetType()
        .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
    static void Check(bool value, string message)
    { if (!value) throw new Exception(message); Console.WriteLine("PASS: " + message); }
    static BattleContent Battle() => new BattleContent {
        contentId = "fight", backgroundPrefab = new GameObject(), enemyGroups = new[] {
            new EnemyGroupEntry { group = new EnemyGroup { groupId = "enemy", placements = new[] {
                new EnemyPlacement { prefab = new BattleUnit() } } } } } };
    static ExpeditionMap Map(out ExpeditionMapPoint[] points, bool startEvent = false)
    {
        points = new ExpeditionMapPoint[4];
        for (int i = 0; i < points.Length; i++)
        {
            var p = new ExpeditionMapPoint();
            Set(p, "hitArea", new Collider2D { transform = p.transform });
            Set(p, "marker", new SpriteRenderer { sprite = new Sprite(), transform = new Transform { parent = p.transform } });
            p.transform.position = new Vector3(i * 10, i * -10);
            points[i] = p;
        }
        for (int i = 0; i < points.Length; i++)
        {
            StageContent content = i == 1 || i == 3 ? Battle() :
                i == 2 || startEvent ? new EventContent { contentId = "event", description = "이벤트 설명" } : null;
            points[i].Configure("n" + i, "", content, i == 3, i == 3 ? "end" : null,
                i < 3 ? new[] { new ExpeditionMapConnection { target = points[i + 1] } } : null);
        }
        var map = new ExpeditionMap { children = points };
        map.SetStartPoint(points[0]);
        return map;
    }
    sealed class Fixture
    {
        public ExpeditionController controller = new();
        public GameObject mapRoot = new(), battleRoot = new();
        public BattleController battle = new();
        public ExpeditionMapCameraController camera = new();
        public ExpeditionMap map;
        public ExpeditionMapPoint[] points;
        public ExpeditionRun run;
        public Transform party = new() { position = new Vector3(0, 0, -2) };
        public Fixture(bool startEvent = false)
        {
            map = Map(out points, startEvent);
            map.transform.parent = mapRoot.transform;
            battle.transform.parent = battleRoot.transform;
            Set(controller, "mapRoot", mapRoot); Set(controller, "battleRoot", battleRoot);
            Set(controller, "battleController", battle); Set(controller, "mapCamera", camera);
            Set(controller, "partyMarker", party);
            run = new StageDefinition { stageId = "test", displayName = "test", mapPrefab = map }.CreateRun();
            GameSession.CurrentExpedition = run;
            GameSession.Current = new object();
            GameSession.FailSave = false;
        }
        public void Start() => controller.Begin(run, map);
        public void Click(int index) => points[index].OnPointerClick(new PointerEventData(null));
        public void Finish(BattleState state)
        {
            battle.Finish(state);
            Check(controller.State == ExpeditionState.Battle, "battle cleanup is deferred beyond BattleEnded dispatch");
            Invoke(controller, "Update");
        }
    }
    static void Main()
    {
        var f = new Fixture();
        ExpeditionEventRequest request = null;
        int completed = 0, entries = 0;
        f.controller.EventRequested += r => request = r;
        f.controller.ExpeditionCompleted += id => { if (id == "end") completed++; };
        f.controller.PointEntered += _ => entries++;
        f.Start();
        Check(f.controller.State == ExpeditionState.Map && f.mapRoot.activeSelf && !f.battleRoot.activeSelf &&
            f.camera.InputEnabled && !f.battle.AutoStart, "begin shows map and disables independent battle auto-start");
        Check(f.run.CurrentNodeId == "n0" && entries == 1, "begin initializes run from loaded map");
        f.Click(1);
        Check(f.controller.State == ExpeditionState.Battle && !f.mapRoot.activeSelf && f.battleRoot.activeSelf &&
            !f.camera.InputEnabled && f.battle.Starts == 1 && f.party.position.x == 10 && f.party.position.z == -2,
            "point click advances run, positions marker, and opens battle view");
        f.Click(1); f.Click(2);
        Check(f.battle.Starts == 1, "battle input lock rejects duplicate and adjacent travel");
        Check(f.controller.CurrentEncounter != null, "encounter is selected once and retained during battle");
        f.Finish(BattleState.Victory);
        Check(f.controller.State == ExpeditionState.Map && f.run.CurrentNodeId == "n1" &&
            f.run.Phase == ExpeditionRun.RunPhase.ChoosingRoute && f.battle.Resets == 1 && entries == 2,
            "victory completes content and returns to map without a duplicate entry notification");
        f.Click(2);
        Check(f.controller.State == ExpeditionState.Event && request != null && f.mapRoot.activeSelf &&
            !f.camera.InputEnabled && !f.points[3].CanSelect, "event receives a completion token and locks map input");
        f.controller.CompleteEvent(new ExpeditionEventRequest("n2", request.Content));
        Check(f.controller.State == ExpeditionState.Event, "unrelated completion token cannot complete an event");
        f.controller.CompleteEvent(request);
        f.controller.CompleteEvent(request);
        Check(f.controller.State == ExpeditionState.Map && f.run.CurrentNodeId == "n2", "event completes once and unlocks onward travel");
        f.Click(3);
        Check(f.battle.Starts == 2 && f.run.EndingId == null, "boss endpoint waits for second battle victory");
        f.controller.CompleteEvent(request);
        f.Finish(BattleState.Victory);
        Check(f.controller.State == ExpeditionState.Completed && completed == 1 && f.run.EndingId == "end" &&
            !f.camera.InputEnabled && f.map.IsBusy, "boss victory finishes expedition and leaves map locked for result UI");
        Application.CanLoad = false;
        f.controller.ReturnToTown();
        Check(f.controller.State == ExpeditionState.Completed && GameSession.CurrentExpedition == f.run,
            "missing return scene preserves result and expedition for retry");
        Application.CanLoad = true;
        GameSession.FailSave = true;
        f.controller.ReturnToTown();
        Check(f.controller.State == ExpeditionState.Completed, "failed save prevents return scene load");
        GameSession.FailSave = false;
        SceneManager.FailLoad = true;
        f.controller.ReturnToTown();
        Check(f.controller.State == ExpeditionState.Completed && GameSession.CurrentExpedition == f.run,
            "rejected scene load preserves expedition");
        SceneManager.FailLoad = false;
        f.controller.ReturnToTown();
        int loads = SceneManager.Loads;
        f.controller.ReturnToTown();
        Check(f.controller.State == ExpeditionState.Returning && GameSession.CurrentExpedition == null && SceneManager.Loads == loads,
            "accepted return clears current expedition and blocks double return");

        var defeat = new Fixture(); defeat.Start(); defeat.Click(1);
        int defeats = 0; defeat.controller.ExpeditionDefeated += () => defeats++;
        defeat.Finish(BattleState.Defeat);
        Check(defeat.controller.State == ExpeditionState.Defeated && defeats == 1 &&
            defeat.run.Phase == ExpeditionRun.RunPhase.ResolvingNode && defeat.run.EndingId == null,
            "defeat does not complete node or record ending");
        var failed = new Fixture(); failed.battle.FailStart = true; failed.Start(); failed.Click(1);
        Check(failed.controller.State == ExpeditionState.Faulted && !failed.camera.InputEnabled && failed.map.IsBusy,
            "battle setup failure exposes error and blocks further travel");
        var eventStart = new Fixture(true);
        var panel = new ExpeditionEventPanel();
        var text = new TMP_Text(); var button = new Button();
        Set(panel, "description", text); Set(panel, "continueButton", button);
        Set(eventStart.controller, "eventPanel", panel);
        eventStart.Start();
        Check(eventStart.controller.State == ExpeditionState.Event && text.text == "이벤트 설명" && button.interactable,
            "starting event opens default description panel before any movement");
        eventStart.controller.enabled = false; Invoke(eventStart.controller, "OnDisabled");
        button.onClick.Invoke();
        Check(eventStart.controller.State == ExpeditionState.Event && !panel.gameObject.activeSelf,
            "disabling controller suspends event panel without consuming content");
        eventStart.controller.enabled = true; Invoke(eventStart.controller, "OnEnabled");
        Check(panel.gameObject.activeSelf && button.interactable, "reenabling restores pending event panel");
        button.onClick.Invoke(); button.onClick.Invoke();
        Check(eventStart.controller.State == ExpeditionState.Map && !panel.gameObject.activeSelf,
            "default event button completes once and hides panel");
        eventStart.controller.enabled = false; Invoke(eventStart.controller, "OnDisabled");
        Check(!eventStart.camera.InputEnabled && eventStart.map.IsBusy, "disabled controller locks map input");
        eventStart.controller.enabled = true; Invoke(eventStart.controller, "OnEnabled");
        Check(eventStart.camera.InputEnabled && !eventStart.map.IsBusy, "reenabled map controller restores input");
        Invoke(eventStart.controller, "OnReleased"); eventStart.Click(1);
        Check(eventStart.battle.Starts == 0, "destroyed controller unsubscribes map requests");
        var missing = new Fixture(true); missing.Start();
        Check(missing.controller.State == ExpeditionState.Faulted && missing.run.Phase == ExpeditionRun.RunPhase.ResolvingNode,
            "missing event presenter fails explicitly without silently completing content");
        var auto = new Fixture();
        var generated = Map(out _);
        CWFramework.CBehaviour.InstantiateFactory = obj => obj is ExpeditionMap ? generated : obj;
        auto.controller.Begin(auto.run);
        Check(auto.controller.Map == generated && generated.transform.parent == auto.mapRoot.transform,
            "default begin instantiates run prefab under map root");
        Invoke(auto.controller, "OnReleased");
        Check(!generated.gameObject.activeSelf, "owned map instance is cleaned up on release");
    }
}
