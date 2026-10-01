using System;

static class ExpeditionTests
{
    static void Check(bool value) { if (!value) throw new Exception("Expedition invariant failed"); }
    static void Reject(Action action)
    {
        try { action(); } catch (ArgumentException) { return; } catch (InvalidOperationException) { return; }
        throw new Exception("Invalid expedition action accepted");
    }
    public static void Run()
    {
        var content = new BattleContent { contentId = "ambush", enemyGroups = new[] {
            new EnemyGroupEntry { group = new EnemyGroup { groupId = "ambush", enemies = new[] { new BattleUnit() } } } } };
        var stage = new StageDefinition { stageId = "road", displayName = "{FamilyName} 가도", nodes = new[] {
            Node("start", StageNodeType.Start, Route("left", "fight"), Route("right", "fork")),
            Node("fight", StageNodeType.Normal, Route("merge", "fork")),
            Node("fork", StageNodeType.Normal, Route("a", "end_a"), Route("b", "end_b")),
            Node("end_a", StageNodeType.Ending), Node("end_b", StageNodeType.Ending)
        }};
        stage.nodes[1].content = content;
        stage.nodes[2].content = new EventContent { contentId = "crossroads" };
        stage.nodes[3].endingId = "a"; stage.nodes[4].endingId = "b";
        stage.Validate();
        var game = new GameData("가문", "가주", new PrimaryStats(), new BattleStatFormulaConfig());
        var record = game.GetExploration("road");
        var run = stage.CreateRun("가문");
        run.AttachExploration(record);
        Check(run.AvailableRoutes.Count == 2 && run.CurrentNodeId == "start");
        Check(record.KnowsNode("start") && !record.KnowsNode("fight"));
        Reject(() => run.ChooseRoute("a"));
        run.ChooseRoute("left");
        Check(run.CurrentContent == content && run.AvailableRoutes.Count == 0);
        Reject(() => run.ChooseRoute("merge"));
        Reject(() => run.CompleteCurrentNode("fork"));
        run.CompleteCurrentNode("fight");
        Reject(() => run.CompleteCurrentNode("fight"));
        // 에셋 연결을 바꿔도 이미 출발한 원정 경로는 달라지지 않습니다.
        stage.nodes[1].routes[0].targetNodeId = "end_b";
        run.ChooseRoute("merge");
        Check(run.CurrentNodeId == "fork");
        stage.nodes[1].routes[0].targetNodeId = "fork";
        run.CompleteCurrentNode("fork");
        run.ChooseRoute("a");
        Check(run.Phase == ExpeditionRun.RunPhase.Completed && run.EndingId == "a");
        Reject(() => run.ChooseRoute("b"));
        Check(!record.KnowsNode("end_b") && record.ReachedEndings.Count == 1);
        var second = stage.CreateRun(); second.AttachExploration(record);
        Check(second.CurrentNodeId == "start" && second.CompletedNodes.Count == 1 && record.KnowsNode("fight"));
        second.ChooseRoute("right"); second.CompleteCurrentNode("fork"); second.ChooseRoute("b");
        Check(record.ReachedEndings.Count == 2 && second.TraversedRoutes.Count == 2);
        var save = GameSaveData.Capture(game);
        var loaded = save.Restore(new BattleStatFormulaConfig());
        Check(loaded.GetExploration("road").ReachedEndings.Count == 2);
        save.version = 1; save.explorations = null;
        Check(save.Restore(new BattleStatFormulaConfig()).Explorations.Count == 0);
        stage.nodes[0].routes[0].targetNodeId = "missing"; Reject(stage.Validate);
        stage.nodes[0].routes[0].targetNodeId = "start"; Reject(stage.Validate);
        stage.nodes[0].routes[0].targetNodeId = "fork"; Reject(stage.Validate); // 중복 대상/미도달
        stage.nodes[0].routes[0].targetNodeId = "fight";
        stage.nodes[1].content = null; Reject(stage.Validate); stage.nodes[1].content = content;
        stage.nodes[1].routes[0].routeId = "left"; Reject(stage.Validate); stage.nodes[1].routes[0].routeId = "merge";
        stage.nodes[3].routes = new[] { Route("cycle", "start") }; Reject(stage.Validate);
        stage.nodes[3].routes = Array.Empty<StageRoute>();
        stage.nodes[4].endingId = "a"; Reject(stage.Validate); stage.nodes[4].endingId = "b";
        stage.Validate();
        // 시작 이벤트와 보스 종착점도 콘텐츠가 완료될 때까지 기다립니다.
        var intro = new EventContent { contentId = "intro" };
        stage.nodes[0].content = intro;
        stage.nodes[3].content = content;
        var bossRun = stage.CreateRun();
        Check(bossRun.CurrentNodeKind == StageNodeType.Start && bossRun.CurrentContent == intro);
        Check(bossRun.Phase == ExpeditionRun.RunPhase.ResolvingNode && bossRun.CompletedNodes.Count == 0);
        Reject(() => bossRun.ChooseRoute("right"));
        bossRun.CompleteCurrentNode("start");
        bossRun.ChooseRoute("right"); bossRun.CompleteCurrentNode("fork"); bossRun.ChooseRoute("a");
        Check(bossRun.CurrentNodeKind == StageNodeType.Ending && bossRun.CurrentContent.Type == StageContentType.Battle);
        Check(bossRun.Phase == ExpeditionRun.RunPhase.ResolvingNode && bossRun.EndingId == null);
        Check(bossRun.Exploration.ReachedEndings.Count == 0 && bossRun.AvailableRoutes.Count == 0);
        Reject(() => bossRun.ChooseRoute("b"));
        Reject(() => bossRun.CompleteCurrentNode("fork"));
        bossRun.CompleteCurrentNode("end_a");
        Check(bossRun.Phase == ExpeditionRun.RunPhase.Completed && bossRun.EndingId == "a");
        Check(bossRun.Exploration.ReachedEndings.Count == 1);
        Reject(() => bossRun.CompleteCurrentNode("end_a"));
        stage.nodes[3].content = intro; // 이벤트 엔딩도 동일한 완료 흐름
        var eventRun = stage.CreateRun(); eventRun.CompleteCurrentNode("start");
        eventRun.ChooseRoute("right"); eventRun.CompleteCurrentNode("fork"); eventRun.ChooseRoute("a");
        Check(eventRun.Phase == ExpeditionRun.RunPhase.ResolvingNode && eventRun.CurrentContent == intro);
        eventRun.CompleteCurrentNode("end_a"); Check(eventRun.Phase == ExpeditionRun.RunPhase.Completed);
        intro.contentId = " "; Reject(stage.Validate); intro.contentId = "intro";
        stage.nodes[1].kind = (StageNodeType)99; Reject(stage.Validate); stage.nodes[1].kind = StageNodeType.Normal;
        stage.Validate();
        BattleContentTests.Run();
        Console.WriteLine("PASS: authored graph validation, branching/merge/endings, completion guards, snapshot, exploration persistence and v1 migration");
    }
    static StageNode Node(string id, StageNodeType kind, params StageRoute[] routes) =>
        new StageNode { nodeId = id, displayName = id, kind = kind, routes = routes };
    static StageRoute Route(string id, string target) => new StageRoute { routeId = id, targetNodeId = target, label = id };
}
