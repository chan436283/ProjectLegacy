using System;
using System.Reflection;

static class BattleContentTests
{
    static void Check(bool value) { if (!value) throw new Exception("Battle content invariant failed"); }
    static void Reject(Action action)
    {
        try { action(); } catch (ArgumentException) { return; }
        throw new Exception("Invalid battle content accepted");
    }
    sealed class PickRoll : Random
    {
        readonly double roll;
        public int Calls;
        public PickRoll(double roll) { this.roll = roll; }
        public override double NextDouble()
        {
            Check(roll >= 0 && roll < 1); Calls++; return roll;
        }
    }
    static EnemyGroupEntry Entry(EnemyGroup group, float weight = 1f) => new EnemyGroupEntry { group = group, weight = weight };
    static EnemyPlacement Place(BattleUnit prefab, int position = 1, BattleRow row = BattleRow.Front)
        => new EnemyPlacement { prefab = prefab, position = position, row = row };
    public static void Run()
    {
        var enemy = new BattleUnit();
        var first = new EnemyGroup { groupId = "first", placements = new[] { Place(enemy, 3), Place(enemy, 1, BattleRow.Back) } };
        var second = new EnemyGroup { groupId = "second", placements = new[] { Place(new BattleUnit()) } };
        var battle = new BattleContent { contentId = "fight", enemyGroups = new[] { Entry(first), Entry(second) } };
        Check(battle.Type == StageContentType.Battle && new EventContent().Type == StageContentType.Event);
        var pick = new PickRoll(0);
        var encounter = battle.CreateEncounter(pick);
        Check(pick.Calls == 1 && encounter.EnemyGroupId == "first" && encounter.Enemies.Count == 2);
        Check(encounter.Enemies[0].Prefab == enemy && encounter.Enemies[1].Prefab == enemy);
        Check(battle.CreateEncounter(new PickRoll(0.5)).EnemyGroupId == "second");
        first.placements[0].prefab = second.placements[0].prefab;
        first.placements[0].row = BattleRow.Back;
        first.placements[0].position = 2;
        Check(encounter.Enemies[0].Row == BattleRow.Front && encounter.Enemies[0].Position == 3);
        Check(encounter.Enemies[1].Row == BattleRow.Back && encounter.Enemies[1].Position == 1);
        first.groupId = "changed"; battle.allowEscape = false;
        Check(encounter.Enemies[0].Prefab == enemy && encounter.EnemyGroupId == "first" && encounter.AllowEscape);
        battle.enemyGroups = new[] { Entry(second) };
        pick = new PickRoll(0);
        Check(battle.CreateEncounter(pick).EnemyGroupId == "second" && pick.Calls == 0);
        Reject(() => battle.CreateEncounter(null));
        battle.enemyGroups = Array.Empty<EnemyGroupEntry>(); Reject(battle.Validate);
        battle.enemyGroups = null; Reject(battle.Validate);
        battle.enemyGroups = new EnemyGroupEntry[] { null }; Reject(battle.Validate);
        battle.enemyGroups = new[] { Entry(second), Entry(second) }; Reject(battle.Validate);
        battle.enemyGroups = new[] { Entry(second) };
        second.placements = Array.Empty<EnemyPlacement>(); Reject(battle.Validate);
        second.placements = new EnemyPlacement[] { null }; Reject(battle.Validate);
        second.placements = new[] { Place(new BattleUnit { Side = BattleSide.Ally }) }; Reject(battle.Validate);
        second.placements = new[] { Place(new BattleUnit { ControlType = BattleControlType.Player }) }; Reject(battle.Validate);
        second.placements = new[] { Place(enemy) }; second.groupId = " "; Reject(battle.Validate);
        second.groupId = "second"; battle.Validate();
        var randomA = new Random(123); var randomB = new Random(123);
        battle.enemyGroups = new[] { Entry(first), Entry(second) };
        bool sawFirst = false, sawSecond = false;
        for (int i = 0; i < 100; i++)
        {
            var a = battle.CreateEncounter(randomA); var b = battle.CreateEncounter(randomB);
            Check(a.EnemyGroupId == b.EnemyGroupId);
            sawFirst |= a.EnemyGroupId == first.groupId; sawSecond |= a.EnemyGroupId == second.groupId;
        }
        Check(sawFirst && sawSecond);
        // 6:3:1 비율과 구간 경계. 0 가중치 항목은 어느 위치에서도 선택되지 않습니다.
        var third = new EnemyGroup { groupId = "third", placements = new[] { Place(enemy) } };
        battle.enemyGroups = new[] { Entry(first, 6), Entry(second, 3), Entry(third, 1) };
        Check(battle.CreateEncounter(new PickRoll(0)).EnemyGroupId == first.groupId);
        Check(battle.CreateEncounter(new PickRoll(0.599)).EnemyGroupId == first.groupId);
        Check(battle.CreateEncounter(new PickRoll(0.6)).EnemyGroupId == second.groupId);
        Check(battle.CreateEncounter(new PickRoll(0.899)).EnemyGroupId == second.groupId);
        Check(battle.CreateEncounter(new PickRoll(0.9)).EnemyGroupId == third.groupId);
        Check(battle.CreateEncounter(new PickRoll(0.999)).EnemyGroupId == third.groupId);
        battle.enemyGroups = new[] { Entry(first, 0), Entry(second, 0.25f), Entry(third, 0) };
        pick = new PickRoll(0);
        Check(battle.CreateEncounter(pick).EnemyGroupId == second.groupId && pick.Calls == 0);
        foreach (float invalid in new[] { -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity, 0f })
        {
            battle.enemyGroups[1].weight = invalid; Reject(battle.Validate);
        }
        battle.enemyGroups = new[] { Entry(first, float.MaxValue), Entry(second, float.MaxValue) };
        Check(battle.CreateEncounter(new PickRoll(0.75)).EnemyGroupId == second.groupId);
        battle.enemyGroups = new[] { new EnemyGroupEntry() }; Reject(battle.Validate);
        var group = new EnemyGroup { groupId = "formation", placements = new[] { Place(enemy), Place(enemy) } };
        Reject(group.Validate); // 같은 프리팹도 같은 자리를 중복 점유할 수 없습니다.
        group.placements[1].row = BattleRow.Back;
        group.Validate(); // 각 열의 1번은 서로 다른 자리입니다.
        group.placements[1].position = 0; Reject(group.Validate);
        group.placements[1].position = -1; Reject(group.Validate);
        group.placements[1].position = 1;
        group.placements[1].row = (BattleRow)99; Reject(group.Validate);
        group.placements[1] = Place(null); Reject(group.Validate);
        group.placements = null; Reject(group.Validate);
        var legacy = new EnemyGroup { groupId = "legacy" };
        typeof(EnemyGroup).GetField("legacyEnemies", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(legacy, new[] { enemy, enemy });
        legacy.OnAfterDeserialize();
        legacy.Validate();
        Check(legacy.placements.Length == 2 && legacy.placements[0].prefab == enemy &&
            legacy.placements[1].row == BattleRow.Front && legacy.placements[1].position == 2);
        legacy.placements[0].row = BattleRow.Back;
        legacy.OnAfterDeserialize();
        Check(legacy.placements[0].row == BattleRow.Back);
        Console.WriteLine("PASS: battle group selection, fixed encounter, snapshots, repeated enemies and validation");
    }
}
