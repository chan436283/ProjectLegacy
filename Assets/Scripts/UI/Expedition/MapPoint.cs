using System;
using CWFramework;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>프리팹으로 재사용하는 지도 지점. 선택 상태와 진입 가능 연출을 담당합니다.</summary>
[DisallowMultipleComponent]
public sealed class MapPoint : CBehaviour
{
    [SerializeField] private ExpeditionStage stage;
    [SerializeField] private Button button;
    [SerializeField] private bool available = true;
    [SerializeField] private GameObject selectedIndicator;
    [Tooltip("버튼 안의 아이콘을 지정하세요. 버튼 클릭 영역은 크기가 변하지 않습니다.")]
    [SerializeField] private RectTransform pulseTarget;
    private float pulseScale = 1.5f;
    private float pulseDuration = 1.2f;
    // 선택 인디케이터의 이동 거리(UI 단위)와 편도 시간. 코드에서만 조정합니다.
    private const float SelectionMoveDistance = 6f;
    private const float SelectionMoveDuration = 0.65f;

    public ExpeditionStage Stage => stage;
    public Button Button => button;
    public bool Available => available;
    public event Action<MapPoint> Clicked;
    public event Action AvailabilityChanged;

    private bool initialized;
    private bool inputEnabled;
    private bool selected;
    private Vector3 originalScale;
    private Tween pulse;
    private RectTransform indicatorTransform;
    private Vector2 originalIndicatorPosition;
    private Tween selectionTween;

    public void Initialize()
    {
        if (initialized) return;
        if (button == null) button = GetComponent<Button>();
        if (stage == null || button == null)
            throw new InvalidOperationException("맵 포인트의 스테이지와 버튼을 연결해야 합니다.");
        stage.CreateRun();
        if (pulseTarget != null) originalScale = pulseTarget.localScale;
        if (selectedIndicator != null)
        {
            indicatorTransform = selectedIndicator.GetComponent<RectTransform>();
            if (indicatorTransform != null) originalIndicatorPosition = indicatorTransform.anchoredPosition;
        }
        button.onClick.AddListener(HandleClick);
        initialized = true;
        RefreshVisual();
    }

    public void SetAvailable(bool value)
    {
        if (available == value) return;
        available = value;
        RefreshVisual();
        AvailabilityChanged?.Invoke();
    }

    public void SetPresentation(bool canInput, bool isSelected)
    {
        Initialize();
        inputEnabled = canInput;
        selected = isSelected;
        RefreshVisual();
    }

    private void HandleClick()
    {
        if (isActiveAndEnabled && inputEnabled && available) Clicked?.Invoke(this);
    }

    private void RefreshVisual()
    {
        if (!initialized) return;
        button.interactable = inputEnabled && available;
        if (selectedIndicator != null) selectedIndicator.SetActive(selected && available);
        if (isActiveAndEnabled && inputEnabled && selected && available && indicatorTransform != null)
        {
            if (selectionTween == null)
                selectionTween = indicatorTransform.DOAnchorPosY(
                        originalIndicatorPosition.y + SelectionMoveDistance, SelectionMoveDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(true);
        }
        else StopSelectionTween();
        bool animate = isActiveAndEnabled && inputEnabled && available && pulseTarget != null;
        if (!animate) { StopPulse(); return; }
        if (pulse != null) return; // 선택 변경 때마다 주기를 재시작하지 않습니다.
        pulse = pulseTarget.DOScale(originalScale * pulseScale, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void StopPulse()
    {
        pulse?.Kill();
        pulse = null;
        if (initialized && pulseTarget != null) pulseTarget.localScale = originalScale;
    }

    private void StopSelectionTween()
    {
        selectionTween?.Kill();
        selectionTween = null;
        if (indicatorTransform != null) indicatorTransform.anchoredPosition = originalIndicatorPosition;
    }

    protected override void OnEnabled() => RefreshVisual();

    protected override void OnDisabled()
    {
        StopPulse();
        StopSelectionTween();
        if (selectedIndicator != null) selectedIndicator.SetActive(false);
    }

    protected override void OnReleased()
    {
        StopPulse();
        StopSelectionTween();
        if (initialized && button != null) button.onClick.RemoveListener(HandleClick);
    }
}
