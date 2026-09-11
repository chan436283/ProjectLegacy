// Standalone checks use the real selector/conditions with minimal Unity data stubs.
using System;
using System.Collections.Generic;
using System.Reflection;

static class Program
{
    static void Set(object obj, string field, object value) => obj.GetType()
        .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
    static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        Console.WriteLine("PASS: " + name);
    }
    static void Main()
    {
        var entries = new List<AIActionEntry>();
        for (int i = 0; i < 5; i++) entries.Add(new AIActionEntry());
        var set = new AIActionSet();
        Set(set, "actions", entries);
        var pattern = new AIActionPattern();
        pattern.SetActionSet(set);
        for (int i = 0; i < 11; i++)
        {
            Check(pattern.TrySelect(_ => true, out int n) && n == i % 5, "cycle " + i);
            pattern.Commit(n);
        }
        pattern.SetActionSet(set);
        Check(pattern.TrySelect(e => entries.IndexOf(e) >= 3, out int selected) && selected == 3,
            "blocked attacks skip directly to defense");
        Check(pattern.CurrentActionIndex == -1, "selection does not consume action");
        pattern.Commit(selected);
        Check(pattern.CurrentActionIndex == 3 && pattern.TrySelect(_ => true, out selected) && selected == 4,
            "next action follows executed defense");
        int calls = 0;
        Check(!pattern.TrySelect(_ => { calls++; return false; }, out _) && calls == 5 &&
            pattern.CurrentActionIndex == 3, "all blocked scans once and preserves cursor");
        var other = new AIActionPattern();
        other.SetActionSet(set);
        Check(other.CurrentActionIndex == -1 && pattern.CurrentActionIndex == 3, "independent unit cursors");
        pattern.SetActionSet(set);
        Check(pattern.CurrentActionIndex == -1, "phase switch resets cursor");
        var actor = new BattleUnit();
        actor.Stats.HpRatio = 1f;
        Check(entries[0].MeetsCondition(actor), "None accepts full HP");
        Set(entries[0], "condition", AIActionConditionType.HpRatioAtOrBelow);
        Check(!entries[0].MeetsCondition(actor), "HP above threshold rejected");
        actor.Stats.HpRatio = 0.5f;
        Check(entries[0].MeetsCondition(actor), "HP threshold inclusive");
        actor.Stats.HpRatio = 0.49f;
        Check(entries[0].MeetsCondition(actor), "HP below threshold accepted");
        actor.Stats.HpRatio = 1f;
        Check(pattern.TrySelect(e => e.MeetsCondition(actor), out selected) && selected == 1,
            "unmet condition skips forward");
        pattern.SetActionSet(new AIActionSet());
        Check(!pattern.TrySelect(_ => true, out _), "empty pattern");
        pattern.SetActionSet(null);
        Check(!pattern.TrySelect(_ => true, out _), "missing pattern");
    }
}
public class BattleSkill { }
public class BattleUnit { public Stats Stats = new(); }
public class Stats { public float HpRatio; }
namespace UnityEngine
{
    public class ScriptableObject { }
    public class SerializeField : Attribute { }
    public class RangeAttribute : Attribute { public RangeAttribute(float min, float max) { } }
    public class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
}
