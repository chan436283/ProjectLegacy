using System.Collections.Generic;
using CWFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(BattleActionExecutor))]
public sealed class PlayerActionController : CBehaviour
{
    [SerializeField] private PlayerActionUI actionUI;
    [SerializeField] private BattleActionExecutor actionExecutor;
    [SerializeField] private Camera targetCamera;
    private BattleController battleController;
    private BattleUnit actor;
    private BattleSkill skill;
    private BattleCommandType command;
    private readonly List<BattleUnit> validTargets = new();
    private readonly HashSet<BattleUnitView> selectionViews = new();
    private readonly List<BattleUnit> selectedTargets = new();
    private int selectionFrame;
    private Vector2 lastPointerPosition;
    public bool IsSelectingTarget => actor != null && skill != null;

    protected override void OnAwake()
    {
        battleController = GetComponent<BattleController>();
        if (actionExecutor == null) actionExecutor = GetComponent<BattleActionExecutor>();
        if (actionUI == null) actionUI = FindFirstObjectByType<PlayerActionUI>();
        if (targetCamera == null) targetCamera = Camera.main;
    }

    protected override void OnEnabled()
    {
        if (actionUI != null) actionUI.CommandSelected += OnCommandSelected;
        battleController.TurnEnded += OnTurnEnded;
        battleController.BattleEnded += OnBattleEnded;
    }

    protected override void OnDisabled()
    {
        if (actionUI != null) actionUI.CommandSelected -= OnCommandSelected;
        battleController.TurnEnded -= OnTurnEnded;
        battleController.BattleEnded -= OnBattleEnded;
        ClearSelection();
    }

    private bool CanSelect(BattleUnit unit) => unit != null &&
        unit.ControlType == BattleControlType.Player && unit.CanAct &&
        battleController.CurrentUnit == unit &&
        battleController.State == BattleState.WaitingForAction && !actionExecutor.IsExecuting;

    private void OnCommandSelected(BattleUnit unit, BattleCommandType selectedCommand)
    {
        if (IsSelectingTarget || !CanSelect(unit)) return;

        BattleSkill selectedSkill = selectedCommand switch
        {
            BattleCommandType.Attack => unit.BasicAttackSkill,
            BattleCommandType.Defend => unit.DefendSkill,
            _ => null
        };

        BeginSelection(unit, selectedSkill, selectedCommand);
    }

    // Skill menus can enter the same target-selection flow through this method.
    public bool SelectSkill(BattleUnit unit, BattleSkill selectedSkill)
    {
        if (unit == null || !unit.HasSkill(selectedSkill)) return false;
        return BeginSelection(unit, selectedSkill, BattleCommandType.Skill);
    }

    private bool BeginSelection(BattleUnit unit, BattleSkill selectedSkill, BattleCommandType selectedCommand)
    {
        if (IsSelectingTarget) return false;

        if (!CanSelect(unit) || selectedSkill == null || !selectedSkill.CanUse(unit))
        {
            Debug.LogWarning("사용 가능한 스킬이 연결되어 있는지 확인해주세요.", this);
            return false;
        }

        ClearSelection();
        actor = unit;
        skill = selectedSkill;
        command = selectedCommand;
        RefreshTargets();

        if (validTargets.Count == 0)
        {
            ClearSelection();
            return false;
        }

        selectionFrame = Time.frameCount;
        if (Mouse.current != null) lastPointerPosition = Mouse.current.position.ReadValue();
        if (actionUI != null) actionUI.SetTargetSelectionMode(true);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

        return true;
    }

    private bool IsSingleTarget => skill.TargetType == BattleTargetType.SingleEnemy ||
        skill.TargetType == BattleTargetType.SingleAlly;

    private void RefreshTargets()
    {
        validTargets.Clear();

        foreach (BattleUnit unit in battleController.Participants)
        {
            if (!skill.IsValidTarget(actor, unit)) continue;

            validTargets.Add(unit);
        }

        selectedTargets.RemoveAll(unit => !validTargets.Contains(unit));

        if (!IsSingleTarget)
        {
            selectedTargets.Clear();
            selectedTargets.AddRange(validTargets);
        }
        else if (selectedTargets.Count == 0 && validTargets.Count > 0)
            selectedTargets.Add(validTargets[0]);

        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        foreach (BattleUnit unit in battleController.Participants)
        {
            if (unit == null || unit.View == null) continue;
            selectionViews.Add(unit.View);
            TargetSelectionState state = !validTargets.Contains(unit)
                ? TargetSelectionState.Unavailable
                : selectedTargets.Contains(unit)
                    ? TargetSelectionState.Selected
                    : TargetSelectionState.Selectable;
            unit.View.SetTargetSelection(state);
        }
    }

    public void SelectTarget(BattleUnit target)
    {
        if (!IsSelectingTarget || !IsSingleTarget || !validTargets.Contains(target)) return;
        selectedTargets.Clear();
        selectedTargets.Add(target);
        UpdateIndicators();
    }

    public void ConfirmSelection()
    {
        if (!IsSelectingTarget || Time.frameCount == selectionFrame) return;
        if (!CanSelect(actor)) { ClearSelection(); return; }
        var previous = selectedTargets.ToArray();
        RefreshTargets();
        // Never silently substitute another target when confirming a stale preview.
        if (previous.Length != selectedTargets.Count) return;
        foreach (BattleUnit target in previous)
            if (!selectedTargets.Contains(target)) return;
        if (selectedTargets.Count == 0) return;
        BattleAction action = command switch
        {
            BattleCommandType.Attack => BattleAction.Attack(actor, selectedTargets[0]),
            BattleCommandType.Defend => BattleAction.Defend(actor),
            _ => BattleAction.UseSkill(actor, skill, selectedTargets)
        };
        if (!actionExecutor.CanExecute(action)) return;
        BattleUnit previousActor = actor;
        if (actionUI != null) actionUI.Hide();
        ClearSelection();
        if (!actionExecutor.TryExecute(action) && CanSelect(previousActor) && actionUI != null)
            actionUI.Show(previousActor);
    }

    public void CancelSelection()
    {
        if (!IsSelectingTarget) return;
        ClearSelection(restoreActionFocus: CanSelect(actor));
    }

    private void ClearSelection(bool restoreActionFocus = false)
    {
        bool wasSelecting = IsSelectingTarget;
        foreach (BattleUnitView view in selectionViews)
            if (view != null) view.SetTargetSelection(TargetSelectionState.None);
        selectionViews.Clear();
        validTargets.Clear();
        selectedTargets.Clear();
        actor = null;
        skill = null;
        if (wasSelecting && actionUI != null)
            actionUI.SetTargetSelectionMode(false, restoreActionFocus);
    }

    private void Update()
    {
        if (!IsSelectingTarget || Time.frameCount == selectionFrame) return;
        if (!CanSelect(actor)) { ClearSelection(); return; }

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if ((keyboard != null && keyboard.escapeKey.wasPressedThisFrame) ||
            (mouse != null && mouse.rightButton.wasPressedThisFrame))
        { CancelSelection(); return; }

        if (keyboard != null)
        {
            int step = keyboard.leftArrowKey.wasPressedThisFrame ? -1 :
                keyboard.rightArrowKey.wasPressedThisFrame ? 1 : 0;

            if (step != 0 && IsSingleTarget)
            {
                RefreshTargets();
                if (validTargets.Count > 0)
                {
                    int index = selectedTargets.Count > 0 ? validTargets.IndexOf(selectedTargets[0]) : 0;
                    SelectTarget(validTargets[(index + step + validTargets.Count) % validTargets.Count]);
                }
            }

            if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            { ConfirmSelection(); return; }
        }

        if (mouse == null || targetCamera == null) return;

        Vector2 position = mouse.position.ReadValue();
        bool moved = position != lastPointerPosition;
        lastPointerPosition = position;

        if (!moved && !mouse.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = targetCamera.ScreenPointToRay(position);
        BattleUnit hit = null;
        float nearest = float.PositiveInfinity;

        foreach (BattleUnit target in validTargets)
        {
            if (target == null || !target.CanAct) continue;
            BattleUnitView view = target.View;
            if (view == null || !view.TryHitSelection(ray, out float distance)) continue;
            // Prefer the foreground sprite when selection rectangles overlap.
            bool preferred = hit == null ||
                view.SelectionSortingLayer > hit.View.SelectionSortingLayer ||
                (view.SelectionSortingLayer == hit.View.SelectionSortingLayer &&
                 (view.SelectionSortingOrder > hit.View.SelectionSortingOrder ||
                  (view.SelectionSortingOrder == hit.View.SelectionSortingOrder && distance < nearest)));
            if (preferred) { hit = target; nearest = distance; }
        }

        if (hit == null) return;

        SelectTarget(hit);

        if (mouse.leftButton.wasPressedThisFrame) ConfirmSelection();
    }

    private void OnTurnEnded(BattleUnit unit) => ClearSelection();
    private void OnBattleEnded(BattleState state) => ClearSelection();
}
