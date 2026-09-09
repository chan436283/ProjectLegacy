using System;
using UnityEngine;

[Serializable]
public sealed class CharacterStats
{
    [field: NonSerialized]
    public event Action<float, float> HpChanged;

    [field: SerializeField]
    public int Level { get; private set; } = 1;

    [field: SerializeField]
    public PrimaryStats PrimaryStats { get; private set; } = new();

    [field: SerializeField]
    public BattleStats Battle { get; private set; } = new();

    [field: SerializeField]
    public float CurrentHp { get; private set; }

    [field: SerializeField]
    public float CurrentMp { get; private set; }

    public bool IsDead => CurrentHp <= 0f;
    public float HpRatio => Battle.MaxHp.Value > 0f
        ? CurrentHp / Battle.MaxHp.Value
        : 0f;

    public void Initialize(BattleStatFormulaConfig formula)
    {
        if (formula == null)
            throw new ArgumentNullException(nameof(formula));

        RecalculateDerivedStats(formula);
        RestoreAll();
    }

    public void SetLevel(int level)
    {
        Level = Math.Max(1, level);
    }

    public void RecalculateDerivedStats(
        BattleStatFormulaConfig formula,
        bool preserveCurrentRatio = true)
    {
        if (formula == null)
            throw new ArgumentNullException(nameof(formula));

        float previousMaxHp = Battle.MaxHp.Value;
        float previousMaxMp = Battle.MaxMp.Value;
        float hpRatio = previousMaxHp > 0f ? CurrentHp / previousMaxHp : 1f;
        float mpRatio = previousMaxMp > 0f ? CurrentMp / previousMaxMp : 1f;

        float str = PrimaryStats.Strength.Value;
        float con = PrimaryStats.Constitution.Value;
        float dex = PrimaryStats.Dexterity.Value;
        float agi = PrimaryStats.Agility.Value;
        float intel = PrimaryStats.Intelligence.Value;
        float wis = PrimaryStats.Wisdom.Value;
        float luk = PrimaryStats.Luck.Value;

        Battle.MaxHp.SetBaseValue(
            formula.baseHp +
            con * formula.hpPerConstitution);

        Battle.MaxMp.SetBaseValue(
            formula.baseMp +
            wis * formula.mpPerWisdom +
            intel * formula.mpPerIntelligence);

        Battle.PhysicalAttack.SetBaseValue(
            formula.basePhysicalAttack +
            str * formula.physicalAttackPerStrength +
            dex * formula.physicalAttackPerDexterity);

        Battle.PhysicalDefense.SetBaseValue(
            formula.basePhysicalDefense +
            con * formula.physicalDefensePerConstitution +
            str * formula.physicalDefensePerStrength);

        Battle.MagicAttack.SetBaseValue(
            formula.baseMagicAttack +
            intel * formula.magicAttackPerIntelligence +
            wis * formula.magicAttackPerWisdom);

        Battle.MagicResistance.SetBaseValue(
            formula.baseMagicResistance +
            wis * formula.magicResistancePerWisdom +
            intel * formula.magicResistancePerIntelligence);

        Battle.Accuracy.SetBaseValue(
            formula.baseAccuracy +
            dex * formula.accuracyPerDexterity +
            luk * formula.accuracyPerLuck);

        Battle.Evasion.SetBaseValue(
            formula.baseEvasion +
            agi * formula.evasionPerAgility +
            luk * formula.evasionPerLuck);

        Battle.CriticalChance.SetBaseValue(
            formula.baseCriticalChance +
            dex * formula.criticalChancePerDexterity +
            luk * formula.criticalChancePerLuck);

        Battle.CriticalDamage.SetBaseValue(formula.baseCriticalDamage);

        Battle.Speed.SetBaseValue(
            formula.baseSpeed +
            agi * formula.agilityToSpeed);

        if (preserveCurrentRatio)
        {
            CurrentHp = Mathf.Clamp(Battle.MaxHp.Value * hpRatio, 0f, Battle.MaxHp.Value);
            CurrentMp = Mathf.Clamp(Battle.MaxMp.Value * mpRatio, 0f, Battle.MaxMp.Value);
        }
        else
        {
            ClampResources();
        }

        NotifyHpChanged();
    }

    public float TakeDamage(float amount)
    {
        float applied = Mathf.Clamp(amount, 0f, CurrentHp);
        CurrentHp -= applied;

        if (applied > 0f)
            NotifyHpChanged();

        return applied;
    }

    public float Heal(float amount)
    {
        float previous = CurrentHp;
        CurrentHp = Mathf.Clamp(CurrentHp + Math.Max(0f, amount), 0f, Battle.MaxHp.Value);
        float recovered = CurrentHp - previous;

        if (recovered > 0f)
            NotifyHpChanged();

        return recovered;
    }

    public bool SpendMp(float amount)
    {
        if (amount < 0f)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (CurrentMp < amount)
            return false;

        CurrentMp -= amount;
        return true;
    }

    public float RecoverMp(float amount)
    {
        float previous = CurrentMp;
        CurrentMp = Mathf.Clamp(CurrentMp + Math.Max(0f, amount), 0f, Battle.MaxMp.Value);
        return CurrentMp - previous;
    }

    public void RestoreAll()
    {
        CurrentHp = Battle.MaxHp.Value;
        CurrentMp = Battle.MaxMp.Value;
        NotifyHpChanged();
    }

    public void RefreshResourceLimits()
    {
        ClampResources();
        NotifyHpChanged();
    }

    private void ClampResources()
    {
        CurrentHp = Mathf.Clamp(CurrentHp, 0f, Math.Max(0f, Battle.MaxHp.Value));
        CurrentMp = Mathf.Clamp(CurrentMp, 0f, Math.Max(0f, Battle.MaxMp.Value));
    }

    private void NotifyHpChanged()
    {
        HpChanged?.Invoke(CurrentHp, Battle.MaxHp.Value);
    }
}
