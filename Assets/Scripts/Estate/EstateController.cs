using System;
using System.Text;
using CWFramework;
using TMPro;
using UnityEngine;

/// <summary>영지 본진의 최소 화면. 확정된 가문과 가주 정보를 표시합니다.</summary>
public sealed class EstateController : CBehaviour
{
    [SerializeField] private BattleStatFormulaConfig formulaConfig;
    [SerializeField] private TMP_Text familyLabel;
    [SerializeField] private TMP_Text characterLabel;
    [SerializeField] private TMP_Text statsLabel;

    protected override void OnStarted()
    {
        try
        {
            // 에디터에서 영지 씬을 직접 실행해도 기존 저장 데이터를 확인할 수 있습니다.
            if (GameSession.Current == null && !GameSession.TryLoad(formulaConfig))
            {
                familyLabel.text = "영지";
                characterLabel.text = "타이틀에서 새 게임을 시작해주세요.";
                statsLabel.text = string.Empty;
                return;
            }

            GameData data = GameSession.Current;
            familyLabel.text = data.FamilyName + " 가문";
            characterLabel.text = "가주: " + data.Protagonist.Name;
            CharacterStats stats = data.Protagonist.Stats;
            var text = new StringBuilder();
            string[] labels = { "힘", "체력", "손재주", "민첩", "지능", "지혜", "매력", "행운" };
            var types = (PrimaryStatType[])Enum.GetValues(typeof(PrimaryStatType));
            for (int i = 0; i < types.Length; i++)
                text.AppendLine($"{labels[i]}  {stats.PrimaryStats.Get(types[i]).BaseValue:0.##}");
            text.AppendLine();
            text.AppendLine($"생명력  {stats.CurrentHp:0.##} / {stats.Battle.MaxHp.Value:0.##}");
            text.Append($"마나  {stats.CurrentMp:0.##} / {stats.Battle.MaxMp.Value:0.##}");
            statsLabel.text = text.ToString();
        }
        catch (Exception exception)
        {
            familyLabel.text = "영지";
            characterLabel.text = "저장 데이터를 불러오지 못했습니다.";
            statsLabel.text = string.Empty;
            Debug.LogException(exception, this);
        }
    }
}
