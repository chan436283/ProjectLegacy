using System;

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
    public static void Run()
    {
        var enemy = new BattleUnit();
        var first = new EnemyGroup { groupId = "first", enemies = new[] { enemy, enemy } };
        var second = new EnemyGroup { groupId = "second", enemies = new[] { new BattleUnit() } };
        var battle = new BattleContent { contentId = "fight", enemyGroups = new[] { Entry(first), Entry(second) } };
        Check(battle.Type == StageContentType.Battle && new EventContent().Type == StageContentType.Event);
        var pick = new PickRoll(0);
        var encounter = battle.CreateEncounter(pick);
        Check(pick.Calls == 1 && encounter.EnemyGroupId == "first" && encounter.EnemyPrefabs.Count == 2);
        Check(encounter.EnemyPrefabs[0] == enemy && encounter.EnemyPrefabs[1] == enemy);
        Check(battle.CreateEncounter(new PickRoll(0.5)).EnemyGroupId == "second");
        first.enemies[0] = second.enemies[0]; first.groupId = "changed"; battle.allowEscape = false;
        Check(encounter.EnemyPrefabs[0] == enemy && encounter.EnemyGroupId == "first" && encounter.AllowEscape);
        battle.enemyGroups = new[] { Entry(second) };
        pick = new PickRoll(0);
        Check(battle.CreateEncounter(pick).EnemyGroupId == "second" && pick.Calls == 0);
        Reject(() => battle.CreateEncounter(null));
        battle.enemyGroups = Array.Empty<EnemyGroupEntry>(); Reject(battle.Validate);
        battle.enemyGroups = null; Reject(battle.Validate);
        battle.enemyGroups = new EnemyGroupEntry[] { null }; Reject(battle.Validate);
        battle.enemyGroups = new[] { Entry(second), Entry(second) }; Reject(battle.Validate);
        battle.enemyGroups = new[] { Entry(second) };
        second.enemies = Array.Empty<BattleUnit>(); Reject(battle.Validate);
        second.enemies = new BattleUnit[] { null }; Reject(battle.Validate);
        second.enemies = new[] { new BattleUnit { Side = BattleSide.Ally } }; Reject(battle.Validate);
        second.enemies = new[] { new BattleUnit { ControlType = BattleControlType.Player } }; Reject(battle.Validate);
        second.enemies = new[] { enemy }; second.groupId = " "; Reject(battle.Validate);
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
        var third = new EnemyGroup { groupId = "third", enemies = new[] { enemy } };
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
        Console.WriteLine("PASS: battle group selection, fixed encounter, snapshots, repeated enemies and validation");
    }
}
