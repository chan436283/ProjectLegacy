using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>단일 저장 슬롯. 임시 파일을 완성한 후 기존 저장 파일을 교체합니다.</summary>
public sealed class GameSaveStore
{
    public string FilePath { get; }
    public bool HasSave => File.Exists(FilePath);

    public GameSaveStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("저장 경로가 필요합니다.");
        FilePath = Path.GetFullPath(filePath);
    }

    public void Save(GameData data)
    {
        string json = JsonUtility.ToJson(GameSaveData.Capture(data), true);
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
        string temporaryPath = FilePath + ".tmp";
        try
        {
            File.WriteAllText(temporaryPath, json, new UTF8Encoding(false));
            if (File.Exists(FilePath))
                File.Replace(temporaryPath, FilePath, FilePath + ".bak");
            else
                File.Move(temporaryPath, FilePath);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    public GameData Load(BattleStatFormulaConfig formula)
    {
        var save = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(FilePath, Encoding.UTF8));
        if (save == null) throw new InvalidDataException("저장 데이터가 비어 있습니다.");
        return save.Restore(formula);
    }
}
