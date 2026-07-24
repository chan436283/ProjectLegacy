using UnityEngine;

[CreateAssetMenu(
    fileName = "BattleStatFormulaConfig",
    menuName = "ProjectLegacy/Character/Battle Stat Formula Config")]
public sealed class BattleStatFormulaConfig : ScriptableObject
{
    [Header("HP / MP")]
    public float baseHp = 50f;
    public float hpPerConstitution = 10f;
    public float baseMp = 20f;
    public float mpPerWisdom = 5f;
    public float mpPerIntelligence = 2f;

    [Header("공격")]
    public float basePhysicalAttack = 5f;
    public float physicalAttackPerStrength = 2f;
    public float physicalAttackPerDexterity = 0.5f;
    public float baseMagicAttack = 5f;
    public float magicAttackPerIntelligence = 2f;
    public float magicAttackPerWisdom = 0.5f;

    [Header("방어")]
    public float basePhysicalDefense = 0f;
    public float physicalDefensePerConstitution = 1.5f;
    public float physicalDefensePerStrength = 0.25f;
    public float baseMagicResistance = 0f;
    public float magicResistancePerWisdom = 1.5f;
    public float magicResistancePerIntelligence = 0.25f;

    [Header("명중 / 회피")]
    public float baseAccuracy = 75f;
    public float accuracyPerDexterity = 1.5f;
    public float accuracyPerLuck = 0.25f;
    public float baseEvasion = 5f;
    public float evasionPerAgility = 1f;
    public float evasionPerLuck = 0.2f;

    [Header("치명타")]
    public float baseCriticalChance = 5f;
    public float criticalChancePerDexterity = 0.25f;
    public float criticalChancePerLuck = 0.25f;
    public float baseCriticalDamage = 150f;

    [Header("속도")]
    public float baseSpeed = 85f;
    public float agilityToSpeed = 1f;
}
