using System;
using System.Collections.Generic;
using CWFramework;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>지도 위에 배치한 버튼과 원정 스테이지를 연결합니다.</summary>
public sealed class StageSelectionPanel : CBehaviour
{
    [SerializeField] private UIPanel panel;
    [SerializeField] private Button closeButton;
    [FormerlySerializedAs("selectedStageNameText")]
    [SerializeField] private TMP_Text stageInfoText;
    [SerializeField] private Button departButton;
    [SerializeField] private MapPoint[] points;

    public bool IsOpen { get; private set; }
    public event Action Closed;
    public event Action<StageDefinition> DepartureRequested;
    public StageDefinition SelectedStage => selectedPoint?.Stage;

    private MapPoint selectedPoint;
    private string familyName;
    private string errorMessage;

    private bool ready;
    private bool transitioning;
    private bool busy;

    protected override void OnAwake()
    {
        if (ready) return;
        if (Initialize()) panel.SetVisibleImmediate(false);
    }

    private bool Initialize()
    {
        if (ready) return true;
        if (panel == null) panel = GetComponent<UIPanel>();
        try
        {
            if (panel == null || closeButton == null || departButton == null || departButton == closeButton ||
                stageInfoText == null || points == null || points.Length == 0)
                throw new InvalidOperationException("지도 패널, 닫기·출정 버튼, 스테이지 안내 문구와 지도 지점을 연결해야 합니다.");
            var buttons = new HashSet<Button>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var point in points)
            {
                if (point == null) throw new InvalidOperationException("맵 포인트 참조가 비어 있습니다.");
                point.Initialize();
                if (point.Stage == null || point.Button == null ||
                    point.Button == closeButton || point.Button == departButton || !buttons.Add(point.Button))
                    throw new InvalidOperationException("지도 지점에 서로 다른 버튼과 스테이지를 연결해야 합니다.");
                var run = point.Stage.CreateRun();
                if (!ids.Add(run.StageId)) throw new InvalidOperationException("지도 스테이지 ID가 중복되었습니다.");
            }
            foreach (var point in points)
            {
                point.Clicked += Select;
                point.AvailabilityChanged += Refresh;
            }
            closeButton.onClick.AddListener(Close);
            departButton.onClick.AddListener(Depart);
            ready = true;
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            return false;
        }
    }

    public bool Open(string familyName = null)
    {
        if (!enabled || IsOpen || transitioning || !Initialize()) return false;
        this.familyName = familyName;
        selectedPoint = null;
        errorMessage = null;
        IsOpen = transitioning = true;
        SetBusy(false);
        panel.Show(() =>
        {
            transitioning = false;
            Refresh();
            closeButton.Select();
        });
        return true;
    }

    private void Select(MapPoint point)
    {
        if (!IsOpen || transitioning || busy || !isActiveAndEnabled || !point.Available) return;
        selectedPoint = point;
        errorMessage = null;
        Refresh();
    }

    private void Depart()
    {
        if (!IsOpen || transitioning || busy || !isActiveAndEnabled || selectedPoint == null ||
            !selectedPoint.Available) return;
        if (DepartureRequested == null)
        {
            ShowError("출정 기능이 연결되지 않았습니다.");
            return;
        }
        SetBusy(true);
        DepartureRequested.Invoke(selectedPoint.Stage);
    }

    /// <summary>진행 조건이 바뀌면 호출합니다. 선택된 지점이 잠기면 선택도 해제합니다.</summary>
    public void SetStageAvailable(string stageId, bool available)
    {
        if (!Initialize()) return;
        var point = Array.Find(points, p => string.Equals(p.Stage.stageId.Trim(), stageId?.Trim(), StringComparison.Ordinal));
        if (point == null) throw new ArgumentException("지도에 없는 스테이지입니다.", nameof(stageId));
        point.SetAvailable(available);
        if (!available && selectedPoint == point)
        {
            selectedPoint = null;
            errorMessage = null;
        }
        Refresh();
    }

    public void SetBusy(bool value)
    {
        busy = value;
        if (value) errorMessage = null;
        Refresh();
    }

    private void Refresh()
    {
        if (!ready) return;
        if (selectedPoint != null && !selectedPoint.Available)
        {
            selectedPoint = null;
            errorMessage = null;
        }
        bool canInput = IsOpen && !busy && !transitioning;
        closeButton.interactable = canInput;
        departButton.interactable = canInput && selectedPoint != null && selectedPoint.Available;
        stageInfoText.text = errorMessage ?? (selectedPoint != null
            ? selectedPoint.Stage.GetDisplayName(familyName) : "원정 지점을 선택해주세요.");
        foreach (var point in points)
        {
            point.SetPresentation(canInput, IsOpen && selectedPoint == point);
        }
    }

    public void ShowError(string message)
    {
        if (!ready) return;
        errorMessage = message;
        SetBusy(false);
        closeButton.Select();
    }

    public void Close()
    {
        if (!IsOpen || transitioning || busy) return;
        transitioning = true;
        Refresh();
        panel.Hide(CompleteClose);
    }

    public void HideImmediate()
    {
        if (panel != null && panel.gameObject.activeSelf) panel.SetVisibleImmediate(false);
        CompleteClose();
    }

    private void CompleteClose()
    {
        bool wasOpen = IsOpen;
        IsOpen = transitioning = busy = false;
        selectedPoint = null;
        errorMessage = null;
        Refresh();
        if (wasOpen) Closed?.Invoke();
    }

    protected override void OnDisabled() => HideImmediate();

    protected override void OnReleased()
    {
        if (!ready) return;
        if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        if (departButton != null) departButton.onClick.RemoveListener(Depart);
        foreach (var point in points)
        {
            if (point == null) continue;
            point.Clicked -= Select;
            point.AvailabilityChanged -= Refresh;
        }
    }
}
