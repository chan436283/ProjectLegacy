using CWFramework;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BattleActionExecutor))]
public sealed class PlayerActionController : CBehaviour
{
    [SerializeField] private PlayerActionUI actionUI;
    [SerializeField] private BattleActionExecutor actionExecutor;

    protected override void OnAwake()
    {
        if (actionExecutor == null)
            actionExecutor = GetComponent<BattleActionExecutor>();
        if (actionUI == null)
            actionUI = FindFirstObjectByType<PlayerActionUI>();
    }

    protected override void OnEnabled()
    {
        if (actionUI != null)
            actionUI.CommandSelected += OnCommandSelected;
        else
            Debug.LogError("플레이어 행동 UI가 연결되지 않았습니다.", this);
    }

    protected override void OnDisabled()
    {
        if (actionUI != null)
            actionUI.CommandSelected -= OnCommandSelected;
    }

    private void OnCommandSelected(BattleUnit unit, BattleCommandType command)
    {
        if (unit == null || unit.ControlType != BattleControlType.Player)
            return;

        if (command != BattleCommandType.Defend)
        {
            Debug.LogWarning($"{command} 명령의 대상 선택은 아직 구현되지 않았습니다.", this);
            return;
        }

        if (unit.DefendSkill == null)
        {
            Debug.LogWarning($"{unit.name}에 방어 스킬이 할당되지 않았습니다.", unit);
            return;
        }

        if (actionExecutor == null || !actionExecutor.TryExecute(BattleAction.Defend(unit)))
            Debug.LogWarning("현재 방어 스킬을 실행할 수 없습니다.", this);
    }
}
