using System;
using System.IO;
using UnityEngine;

/// <summary>현재 플레이 데이터를 씬과 독립적으로 유지합니다.</summary>
public static class GameSession
{
    public static GameData Current { get; private set; }
    public static ExpeditionRunData CurrentExpedition { get; private set; }

    public static void StartExpedition(ExpeditionRunData run)
    {
        CurrentExpedition = run ?? throw new ArgumentNullException(nameof(run));
    }

    public static void EndExpedition() => CurrentExpedition = null;
    public static string SavePath => Path.Combine(Application.persistentDataPath, "game-save.json");

    public static void StartNewGame(GameData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        new GameSaveStore(SavePath).Save(data);
        Current = data;
        EndExpedition();
    }

    public static bool TryLoad(BattleStatFormulaConfig formula)
    {
        var store = new GameSaveStore(SavePath);
        if (!store.HasSave) return false;
        Current = store.Load(formula);
        EndExpedition();
        return true;
    }

    public static void Save()
    {
        if (Current == null) throw new InvalidOperationException("진행 중인 게임이 없습니다.");
        new GameSaveStore(SavePath).Save(Current);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        Current = null;
        EndExpedition();
    }
}
