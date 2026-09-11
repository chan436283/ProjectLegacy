using System.Collections;
using UnityEngine;

public enum BattleMoveType
{
    Linear = 0,
    Parabolic = 1
}

public static class BattleMovement
{
    public static IEnumerator MoveLinear(Transform actor, Vector3 destination, float duration) =>
        Move(actor, destination, duration, 0f, false);

    public static IEnumerator MoveParabolic(Transform actor, Vector3 destination, float duration, float height) =>
        Move(actor, destination, duration, Mathf.Max(0f, height), true);

    public static Vector3 EvaluateParabolic(Vector3 start, Vector3 end, float progress, float height)
    {
        float t = Mathf.Clamp01(progress);
        return Vector3.LerpUnclamped(start, end, t) +
               Vector3.up * (4f * Mathf.Max(0f, height) * t * (1f - t));
    }

    private static IEnumerator Move(Transform actor, Vector3 destination, float duration,
        float height, bool parabolic)
    {
        if (actor == null) yield break;
        Vector3 start = actor.position;
        float elapsed = 0f;
        while (actor != null && elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Jumps use uniform horizontal speed; straight dashes retain their ease-out.
            actor.position = parabolic
                ? EvaluateParabolic(start, destination, t, height)
                : Vector3.LerpUnclamped(start, destination, 1f - (1f - t) * (1f - t));
            yield return null;
        }
        if (actor != null) actor.position = destination;
    }
}
