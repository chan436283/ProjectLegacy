using System;
using System.Collections.Generic;

static class Program
{
    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        Console.WriteLine("PASS: " + message);
    }

    static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }

    static void Main()
    {
        var formula = new BattleStatFormulaConfig();
        var input = new PrimaryStats();
        input.Strength.SetBaseValue(15);
        input.Strength.AddModifier(new StatModifier(100, StatModifierType.Flat, "battle"));
        var game = new GameData(" 가문 ", " 주인공 ", input, formula);
        var ally = new CharacterData("동료", input, formula);
        game.AddCompanion(ally);
        Check(game.FamilyName == "가문" && game.Protagonist.Name == "주인공", "trim names");
        Check(game.Protagonist.Id != ally.Id, "unique character identities");
        Check(game.Protagonist.Stats.PrimaryStats.Strength.Value == 15, "exclude temporary modifiers");
        input.Strength.SetBaseValue(99);
        game.Protagonist.Stats.PrimaryStats.Strength.SetBaseValue(20);
        Check(ally.Stats.PrimaryStats.Strength.Value == 15, "independent character stats");
        Check(ally.Stats.Battle.PhysicalAttack.Value == 40, "derive battle stats from input");
        Check(ally.Stats.CurrentHp == ally.Stats.Battle.MaxHp.Value, "start with full resources");
        ally.Stats.TakeDamage(20);
        Check(game.FindCharacter(ally.Id).Stats.CurrentHp == 130, "retain resource changes in roster");
        Throws<InvalidOperationException>(() => game.AddCompanion(ally));
        Throws<InvalidOperationException>(() => game.AddCompanion(game.Protagonist));
        Throws<ArgumentNullException>(() => game.AddCompanion(null));
        Throws<NotSupportedException>(() => ((IList<CharacterData>)game.Companions).Clear());
        Check(!game.RemoveCompanion(game.Protagonist.Id), "protect protagonist from companion removal");
        Check(game.RemoveCompanion(ally.Id) && game.FindCharacter(ally.Id) == null, "remove companion");
        Throws<ArgumentException>(() => new GameData(" ", "hero", input, formula));
        Throws<ArgumentException>(() => new CharacterData(" ", input, formula));
        Throws<ArgumentNullException>(() => new CharacterData("hero", null, formula));
        Throws<ArgumentNullException>(() => new CharacterData("hero", input, null));
        foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, -1f })
        {
            input.Luck.SetBaseValue(invalid);
            Throws<ArgumentException>(() => new CharacterData("hero", input, formula));
        }
        Console.WriteLine("PASS: invalid input and roster mutation guards");
    }
}

// Unity-independent harness: production data and stat calculations are compiled above.
namespace UnityEngine
{
    public class ScriptableObject { }
    public sealed class SerializeField : Attribute { }
    public sealed class HeaderAttribute : Attribute { public HeaderAttribute(string text) { } }
    public sealed class CreateAssetMenuAttribute : Attribute
    {
        public string fileName;
        public string menuName;
    }
    public static class Mathf
    {
        public static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max);
    }
}
