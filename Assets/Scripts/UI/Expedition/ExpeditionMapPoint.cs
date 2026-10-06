using System;
using CWFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ExpeditionMapPointState { Hidden, Unvisited, Visited, Completed, Available, Current }

/// <summary>원정 내부 지점. 위치는 지도 배경 위에서 직접 배치하며 코드가 정렬하지 않습니다.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public sealed class ExpeditionMapPoint : CBehaviour
{
    [SerializeField] private string nodeId;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image icon;
    [SerializeField] private Sprite unknownIcon;
    [SerializeField] private GameObject currentIndicator;
    [SerializeField] private GameObject availableIndicator;
    [SerializeField] private GameObject visitedIndicator;
    [SerializeField] private GameObject completedIndicator;

    public string NodeId => nodeId;
    public Button Button => button;
    public RectTransform RectTransform => (RectTransform)transform;
    public ExpeditionMapPointState State { get; private set; }
    public event Action<ExpeditionMapPoint> Clicked;

    private CanvasGroup canvasGroup;
    private Sprite knownIcon;
    private bool initialized;
    private bool canInput;

    public void Initialize()
    {
        if (initialized) return;
        if (button == null) button = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (string.IsNullOrWhiteSpace(nodeId) || nodeId != nodeId.Trim() || button == null || label == null || canvasGroup == null)
            throw new InvalidOperationException("원정 지도 지점의 ID, Button, Label, CanvasGroup을 연결해야 합니다.");
        if (icon != null) knownIcon = icon.sprite;
        label.raycastTarget = false;
        button.onClick.AddListener(HandleClick);
        initialized = true;
    }

    public void SetPresentation(string displayName, bool discovered, ExpeditionMapPointState state, bool inputEnabled, bool completed = false)
    {
        Initialize();
        State = state;
        canInput = inputEnabled;
        label.text = discovered ? displayName : "?";
        if (icon != null)
        {
            icon.sprite = discovered ? knownIcon : unknownIcon;
        }
        bool visible = state != ExpeditionMapPointState.Hidden;
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        RefreshInput();
        SetIndicator(currentIndicator, state == ExpeditionMapPointState.Current);
        SetIndicator(availableIndicator, state == ExpeditionMapPointState.Available);
        SetIndicator(visitedIndicator, state == ExpeditionMapPointState.Visited);
        SetIndicator(completedIndicator, completed);
    }

    private static void SetIndicator(GameObject indicator, bool visible)
    {
        if (indicator != null) indicator.SetActive(visible);
    }

    private void RefreshInput()
    {
        if (!initialized) return;
        bool enabledInput = isActiveAndEnabled && canInput && State == ExpeditionMapPointState.Available;
        button.interactable = enabledInput;
        canvasGroup.interactable = enabledInput;
    }

    private void HandleClick()
    {
        if (isActiveAndEnabled && canInput && State == ExpeditionMapPointState.Available)
            Clicked?.Invoke(this);
    }

    protected override void OnEnabled() => RefreshInput();
    protected override void OnDisabled()
    {
        if (!initialized) return;
        button.interactable = false;
        canvasGroup.interactable = false;
    }
    protected override void OnReleased()
    {
        if (initialized && button != null) button.onClick.RemoveListener(HandleClick);
    }
}
