using System;
using CWFramework;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ExpeditionMapPointState { Hidden = 0, Idle = 1, Available = 4, Current = 5 }

/// <summary>월드 지도 지점. 카메라의 Physics2DRaycaster를 통해 클릭을 받습니다.</summary>
[DisallowMultipleComponent]
public sealed class ExpeditionMapPoint : CBehaviour, IPointerClickHandler
{
    [SerializeField] private string nodeId;
    [SerializeField] private string displayName;
    [SerializeField] private StageContent content;
    [SerializeField] private bool isEnding;
    [SerializeField] private string endingId;
    [SerializeField] private ExpeditionMapConnection[] connections = Array.Empty<ExpeditionMapConnection>();

    public StageContent Content => content;
    public bool IsEnding => isEnding;
    public string EndingId => endingId;
    public System.Collections.Generic.IReadOnlyList<ExpeditionMapConnection> Connections => connections;

    /// <summary>절차적 생성용. 맵을 Show하기 전에 호출합니다. 표시용 컴포넌트는 프리팹에서 연결합니다.</summary>
    public void Configure(string id, string name, StageContent nodeContent, bool ending,
        string resultId, ExpeditionMapConnection[] outgoing)
    {
        if (initialized) throw new InvalidOperationException("표시를 초기화한 포인트는 다시 설정할 수 없습니다.");
        nodeId = id; displayName = name; content = nodeContent; isEnding = ending; endingId = resultId;
        connections = outgoing == null ? Array.Empty<ExpeditionMapConnection>() : (ExpeditionMapConnection[])outgoing.Clone();
    }
    [SerializeField] private Collider2D hitArea;
    [SerializeField] private SpriteRenderer marker;
    [SerializeField] private GameObject currentIndicator;

    public string NodeId => nodeId;
    public string DisplayName => displayName ?? string.Empty;
    public bool IsDiscovered { get; private set; }
    public Collider2D HitArea => hitArea;
    public bool CanSelect => isActiveAndEnabled && canInput && State == ExpeditionMapPointState.Available;
    public ExpeditionMapPointState State { get; private set; }
    public event Action<ExpeditionMapPoint> Clicked;

    private Color originalColor;
    private bool initialized;
    private bool canInput;
    private Vector3 originalScale;
    private Tween pulse;
    private const float PulseScale = 1.5f;
    private const float PulseDuration = 1.2f;

    public void Initialize()
    {
        if (initialized) return;
        if (hitArea == null) hitArea = GetComponent<Collider2D>();
        if (marker == null) marker = GetComponent<SpriteRenderer>();
        if (string.IsNullOrWhiteSpace(nodeId) || nodeId != nodeId.Trim() || hitArea == null ||
            marker == null || marker.sprite == null)
            throw new InvalidOperationException("지도 지점의 ID, Collider2D, SpriteRenderer와 스프라이트를 연결해야 합니다.");
        if (hitArea.transform.IsChildOf(marker.transform))
            throw new InvalidOperationException("크기 애니메이션이 클릭 영역을 바꾸지 않도록 Marker는 Collider2D와 분리된 자식에 두세요.");
        originalColor = marker.color;
        originalScale = marker.transform.localScale;
        initialized = true;
        RefreshVisual();
    }

    public void SetPresentation(bool discovered, ExpeditionMapPointState state, bool inputEnabled)
    {
        Initialize();
        State = state;
        canInput = inputEnabled;
        IsDiscovered = discovered;
        RefreshVisual();
    }

    private static void SetIndicator(GameObject indicator, bool visible)
    {
        if (indicator != null) indicator.SetActive(visible);
    }

    private void RefreshVisual()
    {
        if (!initialized) return;
        // 경로상 이동 가능 여부를 표시합니다. 일시적인 입력 잠금은 색상에 반영하지 않습니다.
        marker.color = State == ExpeditionMapPointState.Available
            ? originalColor
            : new Color(0.5f, 0.5f, 0.5f, originalColor.a);
        marker.enabled = State != ExpeditionMapPointState.Hidden;
        hitArea.enabled = CanSelect;
        SetIndicator(currentIndicator, isActiveAndEnabled && State == ExpeditionMapPointState.Current);
        if (!CanSelect) { StopPulse(); return; }
        if (pulse != null) return; // 지도 갱신 때마다 애니메이션 주기를 다시 시작하지 않습니다.
        pulse = marker.transform.DOScale(originalScale * PulseScale, PulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void StopPulse()
    {
        pulse?.Kill();
        pulse = null;
        if (initialized && marker != null) marker.transform.localScale = originalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null || eventData.button != PointerEventData.InputButton.Left || eventData.dragging || !CanSelect) return;
        float threshold = EventSystem.current != null ? EventSystem.current.pixelDragThreshold : 5f;
        if ((eventData.position - eventData.pressPosition).sqrMagnitude > threshold * threshold) return;
        Clicked?.Invoke(this);
    }

    protected override void OnEnabled() => RefreshVisual();
    protected override void OnDisabled()
    {
        if (hitArea != null) hitArea.enabled = false;
        StopPulse();
        SetIndicator(currentIndicator, false);
    }

    protected override void OnReleased() => StopPulse();
}
