using System;
using System.Collections;
using UnityEngine;

// Steps contain configuration only. Per-action state belongs to SkillExecutionContext.
[Serializable]
public abstract class SkillSequenceStep
{
    public abstract IEnumerator Execute(SkillExecutionContext context);
}

[Serializable]
public sealed class DashToTargetStep : SkillSequenceStep
{
    [SerializeField] private SkillAnimationSettings animation = new("Dash");
    [Tooltip("대쉬 애니메이션 시작 후 이동을 시작하기까지의 대기 시간 (초)")]
    [SerializeField, Min(0f)] private float startDelay = 0.1f;
    [SerializeField, Min(0f)] private float duration = 0.2f;
    [SerializeField] private BattleMoveType moveType = BattleMoveType.Linear;
    [Tooltip("이동할 방향으로 본체 SpriteRenderer의 Flip X를 설정합니다.")]
    [SerializeField] private bool faceMovementDirection;
    [Tooltip("포물선 정점의 높이 (월드 단위)")]
    [SerializeField, Min(0f)] private float jumpHeight = 1f;
    [Tooltip("대상 중심에서 시전자 방향으로 떨어질 거리 (월드 단위)")]
    [SerializeField, Min(0f)] private float stoppingDistance = 0.8f;

    public override IEnumerator Execute(SkillExecutionContext context)
    {
        BattleUnit target = context.Action.Targets[0];

        if (target == null || !target.CanAct)
            throw new InvalidOperationException("대쉬 대상이 더 이상 유효하지 않습니다.");

        context.BeginMovementAnimation(animation);

        try
        {
            if (startDelay > 0f)
                yield return new WaitForSeconds(startDelay);

            if (target == null || !target.CanAct)
                throw new InvalidOperationException("대쉬 대상이 더 이상 유효하지 않습니다.");

            Vector3 targetPosition = target.transform.position;
            targetPosition.z = context.Origin.z;
            Vector3 direction = context.Origin - targetPosition;
            float distance = direction.magnitude;
            Vector3 destination = distance > 0.00001f
                ? targetPosition + direction / distance * Mathf.Min(stoppingDistance, distance)
                : context.Origin;

            if (faceMovementDirection) context.FaceTowards(destination);

            yield return context.MoveTo(destination, duration, moveType, jumpHeight);
        }
        finally
        {
            context.EndMovementAnimation();
        }
    }
}

[Serializable]
public sealed class PlayAnimationStep : SkillSequenceStep
{
    [SerializeField] private SkillAnimationSettings animation = new();

    public override IEnumerator Execute(SkillExecutionContext context)
    {
        context.View?.PlaySkillAnimation(animation);
        yield break;
    }
}

[Serializable]
public sealed class WaitForAnimationSignalStep : SkillSequenceStep
{
    [SerializeField] private string signal = "Hit";
    [SerializeField, Min(0.01f)] private float timeout = 3f;

    public override IEnumerator Execute(SkillExecutionContext context)
    {
        float elapsed = 0f;
        while (context.View != null && elapsed < Mathf.Max(0.01f, timeout))
        {
            if (context.View.ConsumeAnimationSignal(signal)) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }
        throw new InvalidOperationException($"스킬 애니메이션 신호 '{signal}'를 받지 못했습니다.");
    }
}

[Serializable]
public sealed class ApplyEffectsStep : SkillSequenceStep
{
    public override IEnumerator Execute(SkillExecutionContext context)
    {
        foreach (BattleEffect effect in context.Action.Skill.Effects)
        {
            if (effect == null) continue;
            foreach (BattleUnit target in context.Action.Targets)
                if (target != null && target.CanAct)
                    effect.Apply(context.Action.Actor, target);
        }
        yield break;
    }
}

[Serializable]
public sealed class WaitStep : SkillSequenceStep
{
    [SerializeField, Min(0f)] private float duration = 2f;

    public override IEnumerator Execute(SkillExecutionContext context)
    {
        if (duration > 0f) yield return new WaitForSeconds(duration);
    }
}

[Serializable]
public sealed class ReturnToOriginStep : SkillSequenceStep
{
    [SerializeField] private SkillAnimationSettings animation = new("Dash");
    [Tooltip("복귀 애니메이션 시작 후 이동까지의 대기 시간 (초)")]
    [SerializeField, Min(0f)] private float startDelay = 0.1f;
    [SerializeField, Min(0f)] private float duration = 0.2f;
    [SerializeField] private BattleMoveType moveType = BattleMoveType.Linear;
    [Tooltip("이동할 방향으로 본체 SpriteRenderer의 Flip X를 설정합니다.")]
    [SerializeField] private bool faceMovementDirection;
    [Tooltip("포물선 정점의 높이 (월드 단위)")]
    [SerializeField, Min(0f)] private float jumpHeight = 1f;

    public override IEnumerator Execute(SkillExecutionContext context)
    {
        context.BeginMovementAnimation(animation);
        try
        {
            if (startDelay > 0f)
                yield return new WaitForSeconds(startDelay);

            if (faceMovementDirection) context.FaceTowards(context.Origin);

            yield return context.MoveTo(context.Origin, duration, moveType, jumpHeight);
        }
        finally
        {
            context.EndMovementAnimation();
            context.RestoreFacing();
        }
    }
}
