using System;
using System.Collections.Generic;
using System.IO;

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

        CheckExpeditionSession();
        ExpeditionTests.Run();
        CheckSaveRoundTrip(formula);
        CompanionGenerationTests.Run(formula);
    }

    static void CheckExpeditionSession()
    {
        var run = new ExpeditionRun(" forest ", " 숲 ", " GameScene ");
        Check(run.StageId == "forest" && run.StageName == "숲" && run.EntryScene == "GameScene",
            "expedition entry snapshot trims fields");
        Throws<ArgumentException>(() => new ExpeditionRun("", "숲", "GameScene"));
        Throws<ArgumentException>(() => new ExpeditionRun("forest", " ", "GameScene"));
        Throws<ArgumentException>(() => new ExpeditionRun("forest", "숲", null));
        GameSession.StartExpedition(run);
        Throws<ArgumentNullException>(() => GameSession.StartExpedition(null));
        Check(GameSession.CurrentExpedition == run, "invalid expedition preserves current run");
        GameSession.EndExpedition();
        Check(GameSession.CurrentExpedition == null, "end expedition clears context");
    }

    static void CheckSaveRoundTrip(BattleStatFormulaConfig formula)
    {
        string directory = Path.Combine(Path.GetTempPath(), "legacy-save-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var primary = new PrimaryStats();
            foreach (PrimaryStatType type in Enum.GetValues(typeof(PrimaryStatType)))
                primary.Get(type).SetBaseValue(1);
            primary.Strength.SetBaseValue(53);
            var data = new GameData("테스트 가문", "레온", primary, formula);
            data.Protagonist.Stats.SetLevel(4);
            data.Protagonist.Stats.TakeDamage(12);
            data.Protagonist.Stats.SpendMp(3);
            data.AddCompanion(new CharacterData("미라", new PrimaryStats(), formula));
            data.Protagonist.Stats.PrimaryStats.Strength.AddModifier(new StatModifier(100, StatModifierType.Flat, "battle"));

            var store = new GameSaveStore(Path.Combine(directory, "save.json"));
            data.GetExploration("road").DiscoverNode("start");
            data.GetExploration("road").ReachEnding("village");
            store.Save(data);
            GameData loaded = store.Load(formula);
            Check(loaded.GetExploration("road").KnowsNode("start") && loaded.GetExploration("road").ReachedEndings.Count == 1, "exploration survives JSON and file round trip");
            Check(loaded.FamilyName == data.FamilyName && loaded.Protagonist.Name == "레온", "saved creation names");
            Check(loaded.Protagonist.Id == data.Protagonist.Id && loaded.Companions[0].Id == data.Companions[0].Id,
                "stable character identities after load");
            foreach (PrimaryStatType type in Enum.GetValues(typeof(PrimaryStatType)))
                Check(loaded.Protagonist.Stats.PrimaryStats.Get(type).Value == primary.Get(type).BaseValue,
                    "saved primary stat " + type);
            Check(loaded.Protagonist.Stats.Level == 4 && loaded.Protagonist.Stats.CurrentHp == 48 &&
                  loaded.Protagonist.Stats.CurrentMp == 24, "saved level and depleted HP/MP");
            Check(loaded.Protagonist.Stats.Battle.PhysicalAttack.Value == 111.5f, "derived stats rebuilt without temporary modifiers");
            store.Save(loaded);
            Check(File.Exists(store.FilePath + ".bak") && !File.Exists(store.FilePath + ".tmp"), "atomic replacement and backup");

            File.WriteAllText(store.FilePath, "{}");
            Throws<ArgumentException>(() => store.Load(formula));
            Check(File.Exists(store.FilePath + ".bak"), "invalid save does not delete backup");
            var badVersion = GameSaveData.Capture(data);
            badVersion.version = 99;
            Throws<ArgumentException>(() => badVersion.Restore(formula));
            var duplicate = GameSaveData.Capture(data);
            duplicate.companions[0].id = duplicate.protagonist.id;
            Throws<InvalidOperationException>(() => duplicate.Restore(formula));

            UnityEngine.Application.persistentDataPath = Path.Combine(directory, "session");
            GameSession.StartExpedition(new ExpeditionRun("forest", "숲", "GameScene"));
            GameSession.StartNewGame(data);
            Check(GameSession.CurrentExpedition == null, "new game clears expedition context");
            Check(GameSession.Current == data && File.Exists(GameSession.SavePath), "new game commits disk and session");
            GameSession.StartExpedition(new ExpeditionRun("forest", "숲", "GameScene"));
            Check(GameSession.TryLoad(formula) && GameSession.Current.Protagonist.Id == data.Protagonist.Id, "reload saved session");
            Check(GameSession.CurrentExpedition == null, "load clears transient expedition context");
            GameData previous = GameSession.Current;
            UnityEngine.Application.persistentDataPath = Path.Combine(directory, "blocked");
            Directory.CreateDirectory(GameSession.SavePath);
            Throws<IOException>(() => GameSession.StartNewGame(data));
            Check(GameSession.Current == previous, "failed save keeps previous session");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}

// Unity-independent harness: production data and stat calculations are compiled above.
namespace UnityEngine
{
    // Only the JSON transport is substituted; production save mapping and file IO run unchanged.
    public static class JsonUtility
    {
        public static string ToJson(object value, bool prettyPrint) => System.Text.Json.JsonSerializer.Serialize(
            value, value.GetType(), new System.Text.Json.JsonSerializerOptions { IncludeFields = true, WriteIndented = prettyPrint });
        public static T FromJson<T>(string text) => System.Text.Json.JsonSerializer.Deserialize<T>(
            text, new System.Text.Json.JsonSerializerOptions { IncludeFields = true });
    }
    public static class Application { public static string persistentDataPath; }
    public enum RuntimeInitializeLoadType { SubsystemRegistration }
    public sealed class RuntimeInitializeOnLoadMethodAttribute : Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType type) { }
    }
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
