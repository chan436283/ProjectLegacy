using System;
using CWFramework;
using UnityEngine;
using UnityEngine.UI;

// Attach to a dedicated visual child, not to the unit's character graphics.
[DisallowMultipleComponent]
public sealed class TargetIndicator : CBehaviour
{
    [Tooltip("기준 위치에서 위아래로 이동하는 거리 (월드 단위, 부모 스케일과 무관)")]
    [SerializeField, Min(0f)] private float moveAmplitude = 0.08f;
    [Tooltip("위아래 이동 한 주기의 시간 (초)")]
    [SerializeField, Min(0.01f)] private float movePeriod = 1f;

    private Vector3 baseLocalPosition;
    private float movementScaleCorrection;
    private float elapsed;

    private SpriteRenderer[] sprites;
    private Graphic[] graphics;
    private bool initialized;

    public TargetIndicatorState State { get; private set; } = TargetIndicatorState.Hidden;

    protected override void OnAwake()
    {
        Initialize();
        Refresh();
    }

    private void Initialize()
    {
        if (initialized) return;
        // Cache once, including inactive children. Supports both world sprites and UI graphics.
        sprites = GetComponentsInChildren<SpriteRenderer>(true);
        graphics = GetComponentsInChildren<Graphic>(true);
        foreach (Graphic graphic in graphics)
            graphic.raycastTarget = false;
        baseLocalPosition = transform.localPosition;
        // Cache the parent's scale correction once along the movement axis.
        Transform parent = transform.parent;
        float movementScale = parent != null
            ? parent.TransformVector(Vector3.up).magnitude
            : 1f;
        movementScaleCorrection = movementScale > 0.00001f ? 1f / movementScale : 0f;
        initialized = true;
    }

    public void SetState(TargetIndicatorState state)
    {
        if (!Enum.IsDefined(typeof(TargetIndicatorState), state))
            throw new ArgumentOutOfRangeException(nameof(state));
        Initialize();
        bool wasVisible = State != TargetIndicatorState.Hidden;
        State = state;
        if (!wasVisible || state == TargetIndicatorState.Hidden)
            ResetPosition();
        Refresh();
    }

    private void Update()
    {
        if (!initialized || State == TargetIndicatorState.Hidden) return;
        
        float period = Mathf.Max(0.01f, movePeriod);
        elapsed = (elapsed + Time.unscaledDeltaTime) % period;
        float offset = Mathf.Sin(elapsed / period * Mathf.PI * 2f) * moveAmplitude * movementScaleCorrection;
        transform.localPosition = baseLocalPosition + Vector3.up * offset;
    }

    protected override void OnDisabled()
    {
        if (initialized) ResetPosition();
    }

    private void ResetPosition()
    {
        elapsed = 0f;
        transform.localPosition = baseLocalPosition;
    }

    private void Refresh()
    {
        bool visible = State != TargetIndicatorState.Hidden;
        foreach (SpriteRenderer sprite in sprites)
        {
            if (sprite == null) continue;
            sprite.enabled = visible;
        }
        foreach (Graphic graphic in graphics)
        {
            if (graphic == null) continue;
            graphic.enabled = visible;
        }
    }
}
