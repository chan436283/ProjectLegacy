using CWFramework;
using UnityEngine;


public sealed class CharacterStatsComponent : CBehaviour
{
    [SerializeField]
    private BattleStatFormulaConfig formulaConfig;

    [SerializeField]
    private CharacterStats stats = new();

    public CharacterStats Stats => stats;

    protected override void OnAwake()
    {
        if (formulaConfig == null)
        {
            Debug.LogError(
                $"{nameof(BattleStatFormulaConfig)}가 할당되지 않았습니다.",
                this);
            return;
        }

        stats.Initialize(formulaConfig);
    }

    public void RecalculateStats(bool preserveCurrentRatio = true)
    {
        stats.RecalculateDerivedStats(
            formulaConfig,
            preserveCurrentRatio);
    }
}
