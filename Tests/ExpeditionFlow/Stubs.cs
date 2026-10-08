// Unity rendering, lifecycle and raycast behavior still require Play Mode checks.
using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public class GameObject
    {
        public Transform transform = new();
        public bool activeSelf = true;
        public void SetActive(bool value) => activeSelf = value;
    }
    public class MonoBehaviour
    {
        public bool enabled = true;
        public GameObject gameObject = new();
        public bool isActiveAndEnabled => enabled && gameObject.activeSelf;
        public Transform transform => gameObject.transform;
        public Dictionary<Type, object> Components = new();
        public object[] children = Array.Empty<object>();
        public T[] GetComponentsInChildren<T>(bool includeInactive) => System.Linq.Enumerable.ToArray(System.Linq.Enumerable.OfType<T>(children));
        public T GetComponent<T>() where T : class => Components.TryGetValue(typeof(T), out var value) ? value as T : null;
    }
    public class ScriptableObject { }
    public class Sprite { }
    public class Collider2D { public bool enabled = true; public Transform transform = new(); }
    public class BoxCollider2D : Collider2D { }
    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a = 1f) { this.r = r; this.g = g; this.b = b; this.a = a; }
    }
    public class SpriteRenderer { public Color color = new(1, 1, 1, 1); public Sprite sprite; public bool enabled = true; public Transform transform = new(); }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z = 0) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 operator *(Vector3 v, float scale) => new(v.x*scale,v.y*scale,v.z*scale);
    }
    public class Transform
    {
        public Vector3 position;
        public Vector3 localPosition;
        public Vector3 localScale = new(1,1,1);
        public Transform parent;
        public DG.Tweening.Tween lastTween;
        public bool IsChildOf(Transform target)
        {
            for (Transform p = this; p != null; p = p.parent) if (p == target) return true;
            return false;
        }
    }
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public float sqrMagnitude => x*x+y*y;
        public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.x-b.x,a.y-b.y);
    }
    public class SerializeField : Attribute { }
    public class DisallowMultipleComponent : Attribute { }
    public class RequireComponent : Attribute { public RequireComponent(params Type[] types) { } }
    public class MinAttribute : Attribute { public MinAttribute(float min) { } }
    public class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
}
namespace UnityEngine.EventSystems
{
    public interface IPointerClickHandler { void OnPointerClick(PointerEventData data); }
    public class EventSystem { public static EventSystem current; public int pixelDragThreshold = 5; }
    public class PointerEventData
    {
        public PointerEventData(EventSystem system) { }
        public enum InputButton { Left, Right, Middle }
        public InputButton button;
        public bool dragging;
        public UnityEngine.Vector2 position, pressPosition;
    }
}
namespace UnityEngine.Scripting.APIUpdating
{
    public class MovedFromAttribute : Attribute
    {
        public MovedFromAttribute(bool autoUpdate, string sourceNamespace, string sourceAssembly, string sourceClassName) { }
    }
}
namespace CWFramework
{
    public class CBehaviour : UnityEngine.MonoBehaviour
    {
        protected virtual void OnAwake() { }
        protected virtual void OnStarted() { }
        public static Func<object, object> InstantiateFactory;
        protected static T Instantiate<T>(T original, UnityEngine.Transform parent) where T : class
        {
            var result = (T)(InstantiateFactory?.Invoke(original) ?? original);
            if (result is UnityEngine.MonoBehaviour component) component.transform.parent = parent;
            if (result is UnityEngine.GameObject go) go.transform.parent = parent;
            return result;
        }
        protected static void Destroy(UnityEngine.GameObject obj) { obj.SetActive(false); }
        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
        protected virtual void OnReleased() { }
    }
}
namespace UnityEngine.Serialization
{
    public class FormerlySerializedAsAttribute : Attribute { public FormerlySerializedAsAttribute(string name) { } }
}

namespace DG.Tweening
{
    public enum Ease { InOutSine }
    public enum LoopType { Yoyo }
    public class Tween
    {
        public bool active = true, independentUpdate;
        public int loopCount;
        public Tween SetEase(Ease ease) => this;
        public Tween SetLoops(int count, LoopType type) { loopCount = count; return this; }
        public Tween SetUpdate(bool independent) { independentUpdate = independent; return this; }
        public void Kill() => active = false;
    }
    public static class ShortcutExtensions
    {
        public static Tween DOScale(this UnityEngine.Transform target, UnityEngine.Vector3 scale, float duration)
        {
            target.localScale = scale;
            return target.lastTween = new Tween();
        }
    }
}

namespace UnityEngine
{
    public static class Application { public static bool CanLoad = true; public static bool CanStreamedLevelBeLoaded(string scene) => CanLoad; }
    public static class Debug { public static void LogException(Exception ex, object context) { } }
}
namespace UnityEngine.SceneManagement
{
    public static class SceneManager
    {
        public static bool FailLoad;
        public static int Loads;
        public static object LoadSceneAsync(string scene) { Loads++; return FailLoad ? null : new object(); }
    }
}
namespace UnityEngine.UI
{
    public class Button
    {
        public bool interactable;
        public ButtonEvent onClick = new();
        public sealed class ButtonEvent
        {
            private event Action Clicked;
            public void AddListener(Action listener) => Clicked += listener;
            public void RemoveListener(Action listener) => Clicked -= listener;
            public void Invoke() => Clicked?.Invoke();
        }
    }
}
namespace TMPro { public class TMP_Text { public string text; } }
public static class GameSession
{
    public static object Current;
    public static ExpeditionRun CurrentExpedition;
    public static bool FailSave;
    public static int Saves;
    public static void Save() { if (FailSave) throw new InvalidOperationException("save failed"); Saves++; }
    public static void EndExpedition() => CurrentExpedition = null;
}
public sealed class ExpeditionMapCameraController : UnityEngine.MonoBehaviour
{
    public bool InputEnabled;
    public void SetInputEnabled(bool value) => InputEnabled = value;
    public void SetMapBackground(UnityEngine.SpriteRenderer background) { }
}
public sealed class BattleController : UnityEngine.MonoBehaviour
{
    public BattleState State = BattleState.Idle;
    public bool AutoStart = true, FailStart;
    public int Starts, Resets;
    public event Action<BattleState> BattleEnded;
    public void SetStartAutomatically(bool value) => AutoStart = value;
    public void StartBattle(BattleEncounter encounter, UnityEngine.Transform parent)
    {
        if (FailStart) throw new InvalidOperationException("battle setup failed");
        if (State != BattleState.Idle) throw new InvalidOperationException("not idle");
        Starts++;
        State = BattleState.WaitingForAction;
    }
    public void Finish(BattleState state) { State = state; BattleEnded?.Invoke(state); }
    public void ResetBattle() { State = BattleState.Idle; Resets++; }
}
