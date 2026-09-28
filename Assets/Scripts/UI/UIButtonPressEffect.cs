using System.Collections;
using CWFramework;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Button의 클릭 동작을 유지하면서 누름 피드백만 추가합니다.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public sealed class UIButtonPressEffect : CUIBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler,
    ISubmitHandler, IDeselectHandler
{
    [Tooltip("비워두면 버튼 자체를 사용합니다. 필요하면 버튼 안의 시각 요소를 지정하세요.")]
    [SerializeField] private RectTransform scaleTarget;
    [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.94f;
    [SerializeField, Min(0f)] private float pressDuration = 0.07f;
    [SerializeField, Min(0f)] private float releaseDuration = 0.12f;

    private Button button;
    private Vector3 normalScale;
    private Tween scaleTween;
    private Coroutine watchRoutine;
    private int? heldPointerId;

    private bool CanInteract => isActiveAndEnabled && button.IsActive() && button.IsInteractable();

    protected override void OnAwake()
    {
        button = GetComponent<Button>();
        if (scaleTarget == null) scaleTarget = RectTransform;
        normalScale = scaleTarget.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanInteract || eventData.button != PointerEventData.InputButton.Left || heldPointerId.HasValue)
            return;

        heldPointerId = eventData.pointerId;
        AnimateScale(normalScale * pressedScale, pressDuration);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || heldPointerId != eventData.pointerId)
            return;

        heldPointerId = null;
        AnimateScale(normalScale, releaseDuration);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CanInteract && heldPointerId == eventData.pointerId)
            AnimateScale(normalScale * pressedScale, pressDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (heldPointerId == eventData.pointerId)
            AnimateScale(normalScale, releaseDuration);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (!CanInteract) return;
        StopAnimation();
        heldPointerId = null;
        scaleTween = DOTween.Sequence()
            .SetUpdate(true)
            .Append(scaleTarget.DOScale(normalScale * pressedScale, pressDuration).SetEase(Ease.OutQuad))
            .Append(scaleTarget.DOScale(normalScale, releaseDuration).SetEase(Ease.OutQuad))
            .OnComplete(() => scaleTween = null);
        WatchInteractability();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        heldPointerId = null;
        if (isActiveAndEnabled) AnimateScale(normalScale, releaseDuration);
    }

    private void AnimateScale(Vector3 target, float duration)
    {
        StopAnimation();
        if (!CanInteract)
        {
            ResetVisual();
            return;
        }

        if (duration <= 0f)
            scaleTarget.localScale = target;
        else
            scaleTween = scaleTarget.DOScale(target, duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() => scaleTween = null);

        WatchInteractability();
    }

    private void WatchInteractability()
    {
        if (watchRoutine == null && (heldPointerId.HasValue || scaleTween != null))
            watchRoutine = StartIfActive(WaitForRelease());
    }

    private IEnumerator WaitForRelease()
    {
        while (heldPointerId.HasValue || scaleTween != null)
        {
            if (!CanInteract)
            {
                ResetVisual();
                break;
            }

            yield return null;
        }

        watchRoutine = null;
    }

    private void StopAnimation()
    {
        scaleTween?.Kill();
        scaleTween = null;
    }

    private void ResetVisual()
    {
        StopAnimation();
        heldPointerId = null;
        if (scaleTarget != null) scaleTarget.localScale = normalScale;
    }

    protected override void OnDisabled()
    {
        StopAndClear(ref watchRoutine);
        ResetVisual();
    }
}
