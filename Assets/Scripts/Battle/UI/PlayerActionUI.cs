using System;
using CWFramework;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class PlayerActionUI : CUIBehaviour
{
    [Header("References")]
    [SerializeField]
    private BattleController battleController;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private PlayerActionButtonView attackButton;

    [SerializeField]
    private PlayerActionButtonView defendButton;

    private float showDuration = 0.6f;

    private float hideDuration = 0.2f;

    [SerializeField]
    private Ease showEase = Ease.OutBack;

    [SerializeField]
    private Ease hideEase = Ease.InBack;

    private BattleUnit currentUnit;
    private Tween visibilityTween;
    private bool isSelectingTarget;
    private PlayerActionButtonView lastCommandButton;

    public event Action<BattleUnit, BattleCommandType> CommandSelected;

    public bool IsVisible { get; private set; }
    public BattleUnit CurrentUnit => currentUnit;

    protected override void OnAwake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (battleController == null)
            battleController = FindFirstObjectByType<BattleController>();

        if (battleController != null)
        {
            battleController.PlayerActionRequested += Show;
            battleController.TurnEnded += OnTurnEnded;
            battleController.BattleEnded += OnBattleEnded;
        }
        else
        {
            Debug.LogError(
                $"{nameof(BattleController)}를 찾을 수 없습니다.",
                this);
        }

        if (attackButton != null)
            attackButton.Clicked += OnAttackClicked;

        if (defendButton != null)
            defendButton.Clicked += OnDefendClicked;

        SetHiddenImmediately();
    }

    protected override void OnReleased()
    {
        if (battleController != null)
        {
            battleController.PlayerActionRequested -= Show;
            battleController.TurnEnded -= OnTurnEnded;
            battleController.BattleEnded -= OnBattleEnded;
        }

        if (attackButton != null)
            attackButton.Clicked -= OnAttackClicked;

        if (defendButton != null)
            defendButton.Clicked -= OnDefendClicked;

        visibilityTween?.Kill();
    }

    public void Show(BattleUnit unit)
    {
        if (unit == null ||
            unit.Side != BattleSide.Ally ||
            unit.ControlType != BattleControlType.Player)
        {
            return;
        }

        currentUnit = unit;
        isSelectingTarget = false;
        lastCommandButton = attackButton;
        IsVisible = true;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        visibilityTween?.Kill();
        RectTransform.localScale = Vector3.zero;
        visibilityTween = RectTransform
            .DOScale(Vector3.one, showDuration)
            .SetEase(showEase)
            .SetUpdate(true)
            .OnComplete(SelectDefaultButton);
    }

    public void Hide(bool immediate = false)
    {
        IsVisible = false;
        isSelectingTarget = false;
        currentUnit = null;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        ClearSelection();
        visibilityTween?.Kill();

        if (immediate || hideDuration <= 0f)
        {
            SetHiddenImmediately();
            return;
        }

        visibilityTween = RectTransform
            .DOScale(Vector3.zero, hideDuration)
            .SetEase(hideEase)
            .SetUpdate(true)
            .OnComplete(() => canvasGroup.alpha = 0f);
    }

    public void SetTargetSelectionMode(bool selecting, bool restoreFocus = false)
    {
        isSelectingTarget = selecting;
        if (!IsVisible) return;

        // Finish the opening animation so its callback cannot reclaim focus.
        visibilityTween?.Kill();
        RectTransform.localScale = Vector3.one;
        canvasGroup.alpha = selecting ? 0.45f : 1f;
        canvasGroup.interactable = !selecting;
        // Disabled commands must still absorb clicks over the battle field.
        canvasGroup.blocksRaycasts = true;

        if (selecting)
            ClearSelection();
        else if (restoreFocus)
            SelectDefaultButton();
    }

    private void SelectDefaultButton()
    {
        PlayerActionButtonView selectedButton = lastCommandButton != null
            ? lastCommandButton : attackButton;
        if (!IsVisible || isSelectingTarget ||
            selectedButton == null ||
            selectedButton.Button == null)
            return;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(
                selectedButton.Button.gameObject);
        else
            selectedButton.SetSelected(true);
    }

    private void SelectCommand(BattleCommandType commandType)
    {
        if (!IsVisible || isSelectingTarget || currentUnit == null)
            return;

        if (CommandSelected == null)
        {
            Debug.LogWarning(
                $"{commandType} 명령을 처리할 대상이 없습니다.",
                this);
            return;
        }

        BattleUnit selectedUnit = currentUnit;
        lastCommandButton = commandType == BattleCommandType.Defend
            ? defendButton : attackButton;

        CommandSelected.Invoke(selectedUnit, commandType);
    }

    private void OnAttackClicked()
    {
        SelectCommand(BattleCommandType.Attack);
    }

    private void OnDefendClicked()
    {
        SelectCommand(BattleCommandType.Defend);
    }

    private void OnTurnEnded(BattleUnit unit)
    {
        if (unit == currentUnit)
            Hide();
    }

    private void OnBattleEnded(BattleState state)
    {
        Hide();
    }

    private void ClearSelection()
    {
        attackButton?.SetSelected(false, true);
        defendButton?.SetSelected(false, true);

        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void SetHiddenImmediately()
    {
        visibilityTween?.Kill();
        RectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        IsVisible = false;
        isSelectingTarget = false;
        currentUnit = null;
    }
}
