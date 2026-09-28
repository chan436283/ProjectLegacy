using System;
using System.Collections.Generic;
using CWFramework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>능력치 행의 좌우 버튼, 현재 수치와 남은 포인트를 연결합니다.</summary>
public sealed class StatAllocationUI : CUIBehaviour
{
    [Serializable]
    private sealed class StatRow
    {
        public PrimaryStatType stat;
        public Button decreaseButton;
        public TMP_Text valueText;
        public Button increaseButton;

        [NonSerialized] public UnityAction decreaseAction;
        [NonSerialized] public UnityAction increaseAction;
    }

    [Header("Allocation Rules")]
    [SerializeField, Min(1)] private int baseValue = 5;
    [SerializeField, Min(1)] private int minimumValue = 1;
    [SerializeField, Min(0)] private int totalPoints = 20;

    [Header("UI References")]
    [SerializeField] private TMP_Text remainingPointsText;
    [SerializeField] private StatRow[] rows;

    private PrimaryStatAllocation allocation;

    public event Action AllocationChanged;

    public int RemainingPoints => GetAllocation().RemainingPoints;

    protected override void OnAwake()
    {
        // 외部의 New Game 처리로 이미 초기화되었다면 그 데이터를 유지합니다.
        GetAllocation();
    }

    protected override void OnEnabled()
    {
        if (!ValidateReferences())
        {
            Debug.LogError("능력치 8종의 버튼/수치와 남은 포인트 텍스트를 중복 없이 연결해야 합니다.", this);
            enabled = false;
            return;
        }

        GetAllocation();
        foreach (StatRow row in rows)
        {
            row.decreaseAction ??= () => ChangeValue(row, false);
            row.increaseAction ??= () => ChangeValue(row, true);
            row.decreaseButton.onClick.AddListener(row.decreaseAction);
            row.increaseButton.onClick.AddListener(row.increaseAction);
        }

        Refresh();
    }

    protected override void OnDisabled()
    {
        if (rows == null) return;
        foreach (StatRow row in rows)
        {
            if (row == null) continue;
            if (row.decreaseButton != null && row.decreaseAction != null)
                row.decreaseButton.onClick.RemoveListener(row.decreaseAction);
            if (row.increaseButton != null && row.increaseAction != null)
                row.increaseButton.onClick.RemoveListener(row.increaseAction);
        }
    }

    public void ResetAllocation()
    {
        // 패널을 오갈 때는 유지하고, 새 게임을 시작할 때만 초기화합니다.
        GetAllocation().Reset();
        if (ValidateReferences()) Refresh();
    }

    public PrimaryStats CreatePrimaryStats() => GetAllocation().CreatePrimaryStats();

    private PrimaryStatAllocation GetAllocation()
    {
        return allocation ??= new PrimaryStatAllocation(baseValue, minimumValue, totalPoints);
    }

    private void ChangeValue(StatRow row, bool increase)
    {
        Button button = increase ? row.increaseButton : row.decreaseButton;
        // CanvasGroup이 패널 전환 중에 차단하는 입력도 무시합니다.
        if (!isActiveAndEnabled || !button.IsActive() || !button.IsInteractable()) return;

        bool changed = increase
            ? allocation.TryIncrease(row.stat)
            : allocation.TryDecrease(row.stat);
        if (changed) Refresh();
    }

    private void Refresh()
    {
        remainingPointsText.text = allocation.RemainingPoints.ToString();
        foreach (StatRow row in rows)
        {
            row.valueText.text = allocation.GetValue(row.stat).ToString();
            row.decreaseButton.interactable = allocation.CanDecrease(row.stat);
            row.increaseButton.interactable = allocation.CanIncrease(row.stat);
        }
        AllocationChanged?.Invoke();
    }

    private bool ValidateReferences()
    {
        if (remainingPointsText == null || rows == null ||
            rows.Length != Enum.GetValues(typeof(PrimaryStatType)).Length)
            return false;

        var stats = new HashSet<PrimaryStatType>();
        var buttons = new HashSet<Button>();
        var texts = new HashSet<TMP_Text> { remainingPointsText };
        foreach (StatRow row in rows)
        {
            if (row == null || !Enum.IsDefined(typeof(PrimaryStatType), row.stat) ||
                !stats.Add(row.stat) || row.decreaseButton == null || row.increaseButton == null ||
                row.valueText == null || !buttons.Add(row.decreaseButton) ||
                !buttons.Add(row.increaseButton) || !texts.Add(row.valueText))
                return false;
        }

        return true;
    }
}
