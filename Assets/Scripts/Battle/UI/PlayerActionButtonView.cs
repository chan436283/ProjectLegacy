using System;
using CWFramework;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PlayerActionButtonView : CUIBehaviour,
    ISelectHandler,
    IDeselectHandler,
    IPointerEnterHandler
{
    [Header("References")]
    [SerializeField]
    private Button button;

    [SerializeField]
    private TMP_Text label;

    [SerializeField]
    private GameObject selectionObject;

    [Header("Selection")]
    [SerializeField]
    private Color normalTextColor = new(0.23f, 0.16f, 0.13f);

    [SerializeField]
    private Color selectedTextColor = new(1f, 0.95f, 0.82f);

    [SerializeField, Min(0f)]
    private float selectedOffset = 6f;

    [SerializeField, Min(0f)]
    private float transitionDuration = 0.08f;

    private Vector2 normalPosition;
    private bool isInitialized;

    public event Action Clicked;

    public Button Button => button;

    protected override void OnAwake()
    {
        if (button == null)
            button = GetComponent<Button>();

        normalPosition = RectTransform.anchoredPosition;
        isInitialized = true;

        button.onClick.AddListener(OnClicked);
        SetSelected(false, true);
    }

    protected override void OnReleased()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClicked);

        RectTransform.DOKill();

        if (label != null)
            label.DOKill();
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetSelected(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SetSelected(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && button.IsInteractable())
            button.Select();
    }

    public void SetSelected(bool value, bool immediate = false)
    {
        if (!isInitialized)
            return;

        if (selectionObject != null)
            selectionObject.SetActive(value);

        Vector2 targetPosition = normalPosition +
                                 Vector2.right * (value ? selectedOffset : 0f);
        Color targetColor = value
            ? selectedTextColor
            : normalTextColor;

        RectTransform.DOKill();

        if (label != null)
            label.DOKill();

        if (immediate || transitionDuration <= 0f)
        {
            RectTransform.anchoredPosition = targetPosition;

            if (label != null)
                label.color = targetColor;

            return;
        }

        RectTransform
            .DOAnchorPos(targetPosition, transitionDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);

        if (label != null)
        {
            label
                .DOColor(targetColor, transitionDuration)
                .SetUpdate(true);
        }
    }

    private void OnClicked()
    {
        Clicked?.Invoke();
    }
}
