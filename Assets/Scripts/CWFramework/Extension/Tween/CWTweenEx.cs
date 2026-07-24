using DG.Tweening;
using UnityEngine;

public static class CWTweenEx
{
    public static Tween DoRandomArcPath2D(Transform tr, Vector3 targetPos, float duration = 0.4f, float minBend = 0.4f, float maxBend = 2.8f, float bendByDistance = 0.7f, float jitter = 0.1f, bool randomSide = true, int fixedSide = 1, bool rotateToTarget = true
, float spriteAngleOffset = 0f, bool alwaysWorldUpArc = false)
    {
        Vector3 p0 = tr.position;
        Vector3 p3 = targetPos;

        Vector3 dir = (p3 - p0);
        float dist = dir.magnitude;

        if (dist < 0.0001f)
            return tr.DOMove(p3, duration);

        Vector3 perp = alwaysWorldUpArc ? Vector3.up : new Vector3(-dir.y, dir.x, 0f).normalized;

        float side = randomSide
            ? (Random.value < 0.5f ? -1f : 1f)
            : Mathf.Sign(Mathf.Clamp(fixedSide, -1, 1));

        float baseBend = Mathf.Clamp(dist * bendByDistance, minBend, maxBend);
        float bend = Mathf.Clamp(baseBend * Random.Range(0.75f, 1.25f), minBend, maxBend);

        float t1 = Mathf.Clamp01(0.33f + Random.Range(-jitter, jitter));
        float t2 = Mathf.Clamp01(0.66f + Random.Range(-jitter, jitter));
        if (t2 <= t1) t2 = Mathf.Min(0.95f, t1 + 0.2f);

        Vector3 p1 = Vector3.Lerp(p0, p3, t1) + perp * bend * side;
        Vector3 p2 = Vector3.Lerp(p0, p3, t2) + perp * bend * side;

        Vector3 lastPos = tr.position;

        return tr.DOPath(
                new[] { p1, p2, p3 },
                duration,
                PathType.CatmullRom,
                PathMode.TopDown2D
            )
            .SetEase(Ease.InOutSine)
            .OnUpdate(() =>
            {
                if (!rotateToTarget) return;

                Vector3 currentPos = tr.position;
                Vector3 velocity = currentPos - lastPos;

                if (velocity.sqrMagnitude > 0.000001f)
                {
                    float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg + spriteAngleOffset;
                    Quaternion targetRot = Quaternion.Euler(0, 0, angle);
                    tr.rotation = Quaternion.Lerp(tr.rotation, targetRot, 20f * Time.deltaTime);
                }

                lastPos = currentPos;
            });
    }

    public static Tween DoProjectileArc2D(
        Transform tr,
        Vector3 targetPos,
        float duration,
        float height = 3f,
        bool rotateToTarget = true,
        float spriteAngleOffset = 0f)
    {
        Vector3 startPos = tr.position;
        Vector3 endPos = targetPos;

        float apexY = Mathf.Max(startPos.y, endPos.y) + height;

        Vector3 lastPos = startPos;

        return DOTween.To(
                () => 0f,
                t =>
                {
                    Vector3 pos = Vector3.Lerp(startPos, endPos, t);

                    float arc = 4f * t * (1f - t);
                    pos.y = Mathf.Lerp(startPos.y, endPos.y, t) + arc * (apexY - Mathf.Lerp(startPos.y, endPos.y, 0.5f));

                    tr.position = pos;

                    if (rotateToTarget)
                    {
                        Vector3 velocity = pos - lastPos;

                        if (velocity.sqrMagnitude > 0.000001f)
                        {
                            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg + spriteAngleOffset;
                            tr.rotation = Quaternion.Lerp(
                                tr.rotation,
                                Quaternion.Euler(0, 0, angle),
                                20f * Time.deltaTime
                            );
                        }
                    }

                    lastPos = pos;
                },
                1f,
                duration
            )
            .SetEase(Ease.Linear);
    }
}
