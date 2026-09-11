using System.Collections;
using UnityEngine;

public sealed class SkillExecutionContext
{
    public BattleAction Action { get; }
    public BattleUnitView View { get; }
    public Vector3 Origin { get; }
    private bool moved;
    private bool facingChanged;
    private readonly bool[] originalFacing;
    private string movementBool;

    public void BeginMovementAnimation(SkillAnimationSettings settings)
    {
        EndMovementAnimation();
        if (settings != null && settings.ParameterType == ParameterType.Bool)
        {
            movementBool = settings.ParameterKey;
            View?.SetMovementAnimationBool(movementBool, true);
        }
        else View?.PlaySkillAnimation(settings);
    }

    public void EndMovementAnimation()
    {
        if (View != null) View.SetMovementAnimationBool(movementBool, false);
        movementBool = null;
    }

    public void FaceTowards(Vector3 destination)
    {
        if (View == null) return;
        facingChanged = true;
        View.FaceTowards(destination);
    }

    public void RestoreFacing()
    {
        if (facingChanged && View != null) View.RestoreFacing(originalFacing);
        facingChanged = false;
    }

    public SkillExecutionContext(BattleAction action)
    {
        Action = action;
        View = action.Actor.View;
        Origin = action.Actor.transform.position;
        originalFacing = View != null ? View.CaptureFacing() : null;
    }

    public IEnumerator MoveTo(Vector3 destination, float duration,
        BattleMoveType moveType = BattleMoveType.Linear, float jumpHeight = 1f)
    {
        moved = true;
        return moveType switch
        {
            BattleMoveType.Linear => BattleMovement.MoveLinear(Action.Actor.transform, destination, duration),
            BattleMoveType.Parabolic => BattleMovement.MoveParabolic(Action.Actor.transform, destination, duration, jumpHeight),
            _ => throw new System.ArgumentOutOfRangeException(nameof(moveType))
        };
    }

    public void RestorePosition()
    {
        EndMovementAnimation();
        RestoreFacing();
        if (moved && Action.Actor != null)
            Action.Actor.transform.position = Origin;
    }
}
