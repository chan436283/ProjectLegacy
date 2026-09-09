using CWFramework;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class BattleUnitStatusView : CUIBehaviour
{
    [Header("Target")]
    [SerializeField]
    private BattleUnit unit;

    [Header("HP")]
    [SerializeField]
    [FormerlySerializedAs("fillImage")]
    private Image hpFillImage;

    private CharacterStats boundStats;
    private bool hasStarted;

    protected override void OnStarted()
    {
        hasStarted = true;
        Bind(unit);
    }

    protected override void OnEnabled()
    {
        if (hasStarted)
            Bind(unit);
    }

    protected override void OnDisabled()
    {
        Unbind();
    }

    protected override void OnReleased()
    {
        Unbind();
    }

    public void SetUnit(BattleUnit value)
    {
        unit = value;

        if (hasStarted && isActiveAndEnabled)
            Bind(unit);
    }

    public void Refresh()
    {
        RefreshHp();
    }

    private void Bind(BattleUnit target)
    {
        CharacterStats targetStats = target != null
            ? target.Stats
            : null;

        if (boundStats == targetStats)
        {
            Refresh();
            return;
        }

        Unbind();
        boundStats = targetStats;

        if (boundStats != null)
            boundStats.HpChanged += OnHpChanged;

        Refresh();
    }

    private void Unbind()
    {
        if (boundStats != null)
            boundStats.HpChanged -= OnHpChanged;

        boundStats = null;
    }

    private void OnHpChanged(float currentHp, float maxHp)
    {
        SetHpFillAmount(CalculateRatio(currentHp, maxHp));
    }

    private void RefreshHp()
    {
        float ratio = boundStats != null
            ? boundStats.HpRatio
            : 0f;

        SetHpFillAmount(ratio);
    }

    private void SetHpFillAmount(float value)
    {
        if (hpFillImage == null)
            return;

        hpFillImage.fillAmount = Mathf.Clamp01(value);
    }

    private static float CalculateRatio(float current, float maximum)
    {
        return maximum > 0f
            ? current / maximum
            : 0f;
    }
}
