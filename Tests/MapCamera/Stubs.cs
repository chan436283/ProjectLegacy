// Actual controller is compiled; Unity input delivery and camera rendering need Play Mode checks.
using System;
namespace UnityEngine
{
    public class SerializeField : Attribute { }
    public class DisallowMultipleComponent : Attribute { }
    public class TooltipAttribute : Attribute { public TooltipAttribute(string text) { } }
    public class MinAttribute : Attribute { public MinAttribute(float value) { } }
    public class Transform { public Vector3 position; }
    public class Camera
    {
        public static Camera main;
        public bool orthographic = true, isActiveAndEnabled = true;
        public float orthographicSize = 5, aspect = 2;
        public Rect pixelRect;
        public Transform transform = new();
    }
    public class GameObject
    {
        public object field;
        public T GetComponentInParent<T>() where T : class => field as T;
    }
    public class SpriteRenderer { public Bounds bounds; }
    public struct Bounds
    {
        public Vector3 min, size;
        public Bounds(Vector3 min, Vector3 size) { this.min = min; this.size = size; }
    }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static implicit operator Vector2(Vector3 v) => new(v.x, v.y);
    }
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero => default;
        public float magnitude => MathF.Sqrt(x*x+y*y);
        public static Vector2 ClampMagnitude(Vector2 v, float length) => v.magnitude > length ? v * (length/v.magnitude) : v;
        public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.x+b.x,a.y+b.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.x-b.x,a.y-b.y);
        public static Vector2 operator *(Vector2 a, float b) => new(a.x*b,a.y*b);
        public static bool operator ==(Vector2 a, Vector2 b) => a.x==b.x && a.y==b.y;
        public static bool operator !=(Vector2 a, Vector2 b) => !(a==b);
        public override bool Equals(object value) => value is Vector2 v && this==v;
        public override int GetHashCode() => HashCode.Combine(x,y);
    }
    public struct Rect
    {
        public float xMin, yMin, width, height;
        public Rect(float x, float y, float width, float height) { xMin=x; yMin=y; this.width=width; this.height=height; }
        public float xMax => xMin+width;
        public float yMax => yMin+height;
        public Vector2 center => new(xMin+width/2,yMin+height/2);
    }
    public static class Mathf
    {
        public static float Min(float a,float b) => Math.Min(a,b);
        public static float Max(float a,float b) => Math.Max(a,b);
        public static float Clamp(float value,float min,float max) => Math.Clamp(value,min,max);
    }
    public static class Time { public static float unscaledDeltaTime; }
    public static class Application
    {
        public static bool isFocused = true;
        public static event Action<bool> focusChanged;
        public static void SetFocus(bool value) { isFocused = value; focusChanged?.Invoke(value); }
    }
    public enum CursorLockMode { None, Locked, Confined }
    public static class Cursor { public static CursorLockMode lockState; }
    public static class Debug { public static void LogError(string text,object context) { } }
}
namespace CWFramework
{
    public class CBehaviour
    {
        public bool enabled = true;
        public T GetComponent<T>() where T : class => null;
        protected virtual void OnAwake() { }
        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
    }
}
namespace UnityEngine.InputSystem
{
    public class ButtonControl { public bool isPressed; }
    public class PositionControl { public Vector2 value; public Vector2 ReadValue() => value; }
    public class Mouse
    {
        public static Mouse current;
        public PositionControl position = new();
        public ButtonControl leftButton = new(), rightButton = new(), middleButton = new();
    }
    public class Keyboard
    {
        public static Keyboard current;
        public ButtonControl aKey=new(), dKey=new(), sKey=new(), wKey=new(),
            leftArrowKey=new(), rightArrowKey=new(), downArrowKey=new(), upArrowKey=new();
    }
}
namespace TMPro { public class TMP_InputField { public bool isFocused; } }
namespace UnityEngine.UI { public class InputField { public bool isFocused; } }
namespace UnityEngine.EventSystems
{
    public class EventSystem
    {
        public static EventSystem current;
        public GameObject currentSelectedGameObject;
        public bool pointerOverUI;
        public bool IsPointerOverGameObject() => pointerOverUI;
    }
}
