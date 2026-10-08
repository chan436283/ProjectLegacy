using CWFramework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>XY 평면 지도를 바라보는 회전 없는 직교 카메라의 가장자리/키보드 이동입니다.</summary>
[DisallowMultipleComponent]
public sealed class ExpeditionMapCameraController : CBehaviour
{
    [SerializeField] private Camera targetCamera;
    [Tooltip("연결하면 배경의 월드 경계를 기준으로 카메라 이동을 제한합니다.")]
    [SerializeField] private SpriteRenderer mapBackground;
    [SerializeField, Min(0f)] private float moveSpeed = 8f;
    [SerializeField] private bool edgeScrolling = true;
    [SerializeField, Min(0f)] private float edgeMarginPixels = 24f;
    [SerializeField] private bool useWASD = true;
    [SerializeField] private bool useArrowKeys = true;
    [SerializeField] private bool blockEdgeOverUI = true;

    public bool InputEnabled { get; private set; } = true;
    private bool edgeScrollReady;

    protected override void OnEnabled()
    {
        edgeScrollReady = false;
        Application.focusChanged += HandleFocusChanged;
    }

    protected override void OnDisabled()
    {
        edgeScrollReady = false;
        Application.focusChanged -= HandleFocusChanged;
    }

    private void HandleFocusChanged(bool focused) => edgeScrollReady = false;

    protected override void OnAwake()
    {
        if (targetCamera == null) targetCamera = GetComponent<Camera>();
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null || !targetCamera.orthographic)
        {
            Debug.LogError("원정 지도 이동에는 직교 카메라를 연결해야 합니다.", this);
            enabled = false;
        }
    }

    /// <summary>전투·팝업·지도 이동 연출 중에는 false로 입력을 잠급니다.</summary>
    public void SetInputEnabled(bool value)
    {
        if (InputEnabled == value) return;
        InputEnabled = value;
        edgeScrollReady = false;
    }

    /// <summary>스테이지별 지도 배경을 생성한 후 경계로 지정할 수 있습니다. null이면 제한하지 않습니다.</summary>
    public void SetMapBackground(SpriteRenderer background)
    {
        if (mapBackground != background) edgeScrollReady = false;
        mapBackground = background;
    }

    private void Update()
    {
        if (!InputEnabled || !Application.isFocused || targetCamera == null ||
            !targetCamera.isActiveAndEnabled || !targetCamera.orthographic || IsEditingText())
        {
            edgeScrollReady = false;
            return;
        }

        Vector2 direction = ReadKeyboard();
        Mouse mouse = Mouse.current;
        // 키보드가 가장자리 이동보다 우선합니다. 서로 반대 방향으로 입력해도 상쇄하지 않습니다.
        if (!edgeScrolling || mouse == null) edgeScrollReady = false;
        if (edgeScrolling && mouse != null &&
            Cursor.lockState != CursorLockMode.Locked &&
            !mouse.leftButton.isPressed && !mouse.rightButton.isPressed && !mouse.middleButton.isPressed &&
            (!blockEdgeOverUI || EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
        {
            Vector2 pointer = mouse.position.ReadValue();
            Rect viewport = targetCamera.pixelRect;
            Vector2 edgeDirection = GetEdgeDirection(pointer, viewport, edgeMarginPixels);
            if (!edgeScrollReady)
            {
                // 초기값·화면 밖 좌표도 방향이 0이므로, 화면 내부인지 함께 확인합니다.
                edgeScrollReady = pointer.x > viewport.xMin && pointer.x < viewport.xMax &&
                    pointer.y > viewport.yMin && pointer.y < viewport.yMax && edgeDirection == Vector2.zero;
            }
            else if (direction == Vector2.zero)
                direction = edgeDirection;
        }

        Vector3 position = targetCamera.transform.position;
        Vector2 next = (Vector2)position + Vector2.ClampMagnitude(direction, 1f) *
            Mathf.Max(0f, moveSpeed) * Time.unscaledDeltaTime;
        if (mapBackground != null)
        {
            Bounds bounds = mapBackground.bounds;
            next = ClampToMap(next, new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y),
                targetCamera.orthographicSize, targetCamera.aspect);
        }
        targetCamera.transform.position = new Vector3(next.x, next.y, position.z);
    }

    private Vector2 ReadKeyboard()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return Vector2.zero;
        bool left = (useWASD && keyboard.aKey.isPressed) || (useArrowKeys && keyboard.leftArrowKey.isPressed);
        bool right = (useWASD && keyboard.dKey.isPressed) || (useArrowKeys && keyboard.rightArrowKey.isPressed);
        bool down = (useWASD && keyboard.sKey.isPressed) || (useArrowKeys && keyboard.downArrowKey.isPressed);
        bool up = (useWASD && keyboard.wKey.isPressed) || (useArrowKeys && keyboard.upArrowKey.isPressed);
        return new Vector2((right ? 1 : 0) - (left ? 1 : 0), (up ? 1 : 0) - (down ? 1 : 0));
    }

    private static bool IsEditingText()
    {
        GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        if (selected == null) return false;
        TMP_InputField tmp = selected.GetComponentInParent<TMP_InputField>();
        InputField legacy = selected.GetComponentInParent<InputField>();
        return (tmp != null && tmp.isFocused) || (legacy != null && legacy.isFocused);
    }

    /// <summary>픽셀 좌표를 기준으로 카메라 화면 내부의 가장자리만 판정합니다.</summary>
    internal static Vector2 GetEdgeDirection(Vector2 pointer, Rect viewport, float margin)
    {
        if (viewport.width <= 0f || viewport.height <= 0f || margin <= 0f ||
            pointer.x < viewport.xMin || pointer.x > viewport.xMax ||
            pointer.y < viewport.yMin || pointer.y > viewport.yMax) return Vector2.zero;

        // 작은 화면에서 좌/우 또는 상/하 영역이 겹치지 않도록 제한합니다.
        float horizontal = Mathf.Min(margin, viewport.width * 0.5f);
        float vertical = Mathf.Min(margin, viewport.height * 0.5f);
        Vector2 result = Vector2.zero;
        if (pointer.x < viewport.center.x && pointer.x <= viewport.xMin + horizontal) result.x = -1;
        else if (pointer.x > viewport.center.x && pointer.x >= viewport.xMax - horizontal) result.x = 1;
        if (pointer.y < viewport.center.y && pointer.y <= viewport.yMin + vertical) result.y = -1;
        else if (pointer.y > viewport.center.y && pointer.y >= viewport.yMax - vertical) result.y = 1;
        return Vector2.ClampMagnitude(result, 1f);
    }

    internal static Vector2 ClampToMap(Vector2 position, Rect bounds, float orthographicSize, float aspect)
    {
        float halfHeight = orthographicSize;
        float halfWidth = halfHeight * aspect;
        return new Vector2(
            bounds.width <= halfWidth * 2f ? bounds.center.x : Mathf.Clamp(position.x, bounds.xMin + halfWidth, bounds.xMax - halfWidth),
            bounds.height <= halfHeight * 2f ? bounds.center.y : Mathf.Clamp(position.y, bounds.yMin + halfHeight, bounds.yMax - halfHeight));
    }
}
