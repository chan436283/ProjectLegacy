using System;
using CWFramework;
using DG.Tweening;
using UnityEngine;

/// <summary>게임 UI 패널의 공통 표시, 숨김, 전환 연출과 입력 상태를 관리합니다.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class UIPanel : CUIBehaviour
{
    [SerializeField, Min(0f)] private float showDuration = 0.4f;
    [SerializeField, Min(0f)] private float hideDuration = 0.2f;
    [SerializeField] private Ease showEase = Ease.OutCubic;
    [SerializeField] private Ease hideEase = Ease.InCubic;

    private CanvasGroup canvasGroup;
    private Vector3 visibleScale;
    private Tween scaleTween;
    private bool initialized;

    protected override void OnAwake() => Initialize();

    protected override void OnEnabled()
    {
        // 부모나 컴포넌트가 다시 활성화될 때 입력 차단이 남지 않게 합니다.
        if (initialized) SetInputEnabled(true);
    }

    // 비활성 상태로 시작한 패널도 Awake 이전에 준비할 수 있습니다.
    private void Initialize()
    {
        if (initialized) return;
        canvasGroup = GetComponent<CanvasGroup>();
        visibleScale = RectTransform.localScale;
        initialized = true;
    }

    public void Show(Action onComplete = null)
    {
        Initialize();
        StopAnimation();
        if (!gameObject.activeSelf) RectTransform.localScale = Vector3.zero;
        gameObject.SetActive(true);
        SetInputEnabled(false);

        if (showDuration <= 0f)
        {
            CompleteShow(onComplete);
            return;
        }

        scaleTween = RectTransform.DOScale(visibleScale, showDuration)
            .SetEase(showEase)
            .SetUpdate(true)
            .OnComplete(() => CompleteShow(onComplete));
    }

    public void Hide(Action onComplete = null)
    {
        Initialize();
        StopAnimation();
        SetInputEnabled(false);

        if (!gameObject.activeSelf || hideDuration <= 0f)
        {
            CompleteHide(onComplete);
            return;
        }

        scaleTween = RectTransform.DOScale(Vector3.zero, hideDuration)
            .SetEase(hideEase)
            .SetUpdate(true)
            .OnComplete(() => CompleteHide(onComplete));
    }

    public void SetVisibleImmediate(bool visible)
    {
        Initialize();
        StopAnimation();
        RectTransform.localScale = visibleScale;
        gameObject.SetActive(visible);
        SetInputEnabled(visible);
    }

    private void CompleteShow(Action onComplete)
    {
        scaleTween = null;
        RectTransform.localScale = visibleScale;
        SetInputEnabled(true);
        onComplete?.Invoke();
    }

    private void CompleteHide(Action onComplete)
    {
        scaleTween = null;
        gameObject.SetActive(false);
        RectTransform.localScale = visibleScale;
        onComplete?.Invoke();
    }

    private void SetInputEnabled(bool value)
    {
        canvasGroup.interactable = value;
        // 연출 중 클릭이 패널 뒤의 UI로 통과하지 않도록 합니다.
        canvasGroup.blocksRaycasts = gameObject.activeInHierarchy;
    }

    private void StopAnimation()
    {
        scaleTween?.Kill();
        scaleTween = null;
    }

    protected override void OnDisabled()
    {
        StopAnimation();
        if (!initialized) return;
        RectTransform.localScale = visibleScale;
        SetInputEnabled(false);
    }
}
