using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

static class Program
{
    public static void Set(object target, string field, object value) => target.GetType()
        .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        Console.WriteLine("PASS: " + name);
    }
    static void Reject(Action action)
    {
        try { action(); }
        catch (ArgumentException) { return; }
        catch (InvalidOperationException) { return; }
        throw new Exception("Invalid formation accepted");
    }
    public static BattleUnit Unit(BattleSide side = BattleSide.Enemy, BattleRow row = BattleRow.Front, int position = 1)
    {
        var unit = new BattleUnit();
        Set(unit, "side", side);
        Set(unit, "controlType", BattleControlType.AI);
        Set(unit, "statsComponent", new CharacterStatsComponent());
        unit.SetFormationPosition(row, position);
        return unit;
    }
    static BattleFormation Formation()
    {
        var formation = new BattleFormation();
        Set(formation, "allySlots", new List<Transform> { Slot(10), Slot(20) });
        Set(formation, "enemyFrontSlots", new List<Transform> { Slot(1), Slot(2), Slot(3) });
        Set(formation, "enemyBackSlots", new List<Transform> { Slot(4), Slot(5), Slot(6) });
        return formation;
    }
    static Transform Slot(float x) => new Transform { position = new Vector3 { x = x } };
    static BattleController Controller(BattleFormation formation, BattleUnit ally)
    {
        var controller = new BattleController();
        Set(controller, "formation", formation);
        Set(controller, "participants", new List<BattleUnit> { ally });
        return controller;
    }
    static void Main()
    {
        var formation = Formation();
        var front = Unit(position: 3);
        var back = Unit(row: BattleRow.Back);
        var ally = Unit(BattleSide.Ally);
        var secondAlly = Unit(BattleSide.Ally);
        formation.PlaceParticipants(new[] { back, ally, front, secondAlly, front, null });
        Check(front.transform.position.x == 3 && back.transform.position.x == 4,
            "explicit row/position preserves gaps regardless of participant order");
        Check(ally.transform.position.x == 10 && secondAlly.transform.position.x == 20,
            "allies keep sequential placement; duplicate references and nulls are ignored");
        formation.PlaceParticipants(new[] { front, back });
        Check(front.transform.position.x == 3 && back.transform.position.x == 4, "reordering does not change enemy seats");
        front.Stats.IsDead = true;
        formation.PlaceParticipants(new[] { back });
        Check(back.Row == BattleRow.Back && back.FormationPosition == 1 && back.transform.position.x == 4,
            "empty or dead front row does not pull back row forward");
        var a = Unit();
        var b = Unit();
        Reject(() => formation.PlaceParticipants(new[] { a, b }));
        Check(a.transform.position.x == 0 && b.transform.position.x == 0, "duplicate seat failure moves no units");
        b.SetFormationPosition(BattleRow.Back, 1);
        formation.PlaceParticipants(new[] { a, b });
        Check(a.transform.position.x == 1 && b.transform.position.x == 4, "same number in different rows is allowed");
        a.transform.position = default;
        b.SetFormationPosition(BattleRow.Back, 4);
        Reject(() => formation.PlaceParticipants(new[] { a, b }));
        Check(a.transform.position.x == 0, "missing seat is rejected before moving earlier units");
        b.SetFormationPosition(BattleRow.Back, 1);
        Set(formation, "enemyBackSlots", new List<Transform> { null });
        Reject(() => formation.PlaceParticipants(new[] { a, b }));
        Set(formation, "enemyBackSlots", null);
        Reject(() => formation.PlaceParticipants(new[] { b }));
        var sharedSlot = Slot(99);
        Set(formation, "enemyFrontSlots", new List<Transform> { sharedSlot });
        Set(formation, "enemyBackSlots", new List<Transform> { sharedSlot });
        Reject(() => formation.PlaceParticipants(new[] { a, b }));
        Set(b, "formationPosition", 0); Reject(() => formation.PlaceParticipants(new[] { b }));
        Set(b, "formationPosition", 1);
        Set(b, "row", (BattleRow)99); Reject(() => formation.PlaceParticipants(new[] { b }));
        Reject(() => a.SetFormationPosition(BattleRow.Back, 0));
        Reject(() => a.SetFormationPosition((BattleRow)99, 2));
        Check(a.Row == BattleRow.Front && a.FormationPosition == 1, "invalid setter leaves logical position unchanged");
        Reject(() => formation.PlaceParticipants(null));
        Console.WriteLine("PASS: missing transforms, shared transforms and malformed serialized positions rejected");

        var prefab = Unit();
        var group = new EnemyGroup { groupId = "test", placements = new[] {
            new EnemyPlacement { prefab = prefab, row = BattleRow.Front, position = 3 },
            new EnemyPlacement { prefab = prefab, row = BattleRow.Back, position = 1 }
        } };
        var content = new BattleContent { contentId = "test", enemyGroups = new[] { new EnemyGroupEntry { group = group } } };
        var encounter = content.CreateEncounter(new Random(1));
        var controller = Controller(Formation(), Unit(BattleSide.Ally));
        bool initializedAtStart = false;
        controller.BattleStarted += () => initializedAtStart = controller.Participants.Count == 3 &&
            controller.Participants[1].Row == BattleRow.Front && controller.Participants[1].FormationPosition == 3 &&
            controller.Participants[2].Row == BattleRow.Back && controller.Participants[2].transform.position.x == 4;
        controller.StartBattle(encounter);
        Check(initializedAtStart && controller.State == BattleState.WaitingForAction,
            "encounter spawns, assigns seats and places enemies before battle starts");
        Check(controller.Participants[1] != prefab && controller.Participants[2] != prefab &&
            controller.Participants[1] != controller.Participants[2] && prefab.FormationPosition == 1,
            "repeated prefab creates independent instances without changing asset");
        Reject(() => controller.StartBattle(encounter));
        Reject(controller.ResetBattle);
        var retainedAlly = controller.Participants[0];
        foreach (var unit in controller.Participants)
            if (unit.Side == BattleSide.Enemy) unit.Stats.IsDead = true;
        controller.BeginAction(controller.CurrentUnit);
        controller.CompleteCurrentTurn();
        Check(controller.State == BattleState.Victory, "encounter reaches terminal victory");
        int removedBeforeReset = CWFramework.CBehaviour.Destroyed;
        controller.ResetBattle();
        Check(controller.State == BattleState.Idle && controller.CurrentUnit == null &&
            controller.Participants.Count == 1 && controller.Participants[0] == retainedAlly &&
            CWFramework.CBehaviour.Destroyed == removedBeforeReset + 2,
            "reset removes spawned enemies and retains existing ally for next encounter");
        controller.StartBattle(encounter);
        Check(controller.State == BattleState.WaitingForAction && controller.Participants.Count == 3,
            "same battle controller can start a second encounter");
        var occupied = Controller(Formation(), Unit(BattleSide.Ally));
        occupied.AddParticipant(Unit());
        Reject(() => occupied.StartBattle(encounter));
        var badFormation = Formation();
        Set(badFormation, "enemyBackSlots", new List<Transform>());
        var waitingAlly = Unit(BattleSide.Ally);
        var failed = Controller(badFormation, waitingAlly);
        int destroyedBefore = CWFramework.CBehaviour.Destroyed;
        Reject(() => failed.StartBattle(encounter));
        Check(failed.State == BattleState.Idle && failed.Participants.Count == 1 && waitingAlly.transform.position.x == 0 &&
            CWFramework.CBehaviour.Destroyed == destroyedBefore + 2,
            "failed encounter removes spawned instances and leaves existing participants untouched");
        Set(failed, "formation", Formation());
        failed.StartBattle(encounter);
        Check(failed.Participants.Count == 3, "failed setup can be corrected and retried");
    }
}
