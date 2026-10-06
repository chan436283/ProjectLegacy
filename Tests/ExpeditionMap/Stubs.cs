// Unity rendering, lifecycle and raycast behavior still require Play Mode checks.
using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public class GameObject
    {
        public bool activeSelf = true;
        public void SetActive(bool value) => activeSelf = value;
    }
    public class MonoBehaviour
    {
        public bool enabled = true;
        public GameObject gameObject = new();
        public bool isActiveAndEnabled => enabled && gameObject.activeSelf;
        public Transform transform = new RectTransform();
        public Dictionary<Type, object> Components = new();
        public T GetComponent<T>() where T : class => Components.TryGetValue(typeof(T), out var value) ? value as T : null;
    }
    public class ScriptableObject { }
    public class Sprite { }
    public class CanvasGroup { public float alpha = 1; public bool interactable = true, blocksRaycasts = true; }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z = 0) { this.x = x; this.y = y; this.z = z; }
    }
    public class Transform { public Vector3 localPosition; }
    public class RectTransform : Transform { }
    public class SerializeField : Attribute { }
    public class DisallowMultipleComponent : Attribute { }
    public class RequireComponent : Attribute { public RequireComponent(params Type[] types) { } }
    public class MinAttribute : Attribute { public MinAttribute(float min) { } }
    public class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
}
namespace UnityEngine.Events
{
    public class UnityEvent
    {
        private event Action listeners;
        public void AddListener(Action listener) => listeners += listener;
        public void RemoveListener(Action listener) => listeners -= listener;
        public void Invoke() => listeners?.Invoke();
    }
}
namespace UnityEngine.UI
{
    public class Button
    {
        public bool interactable = true;
        public UnityEngine.Events.UnityEvent onClick = new();
    }
    public class Image
    {
        public UnityEngine.Sprite sprite;
        public bool raycastTarget = true, enabled = true;
    }

}
namespace TMPro { public class TMP_Text { public string text; public bool raycastTarget; } }
namespace CWFramework
{
    public class CBehaviour : UnityEngine.MonoBehaviour
    {
        protected virtual void OnAwake() { }
        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
        protected virtual void OnReleased() { }
    }
}
namespace UnityEngine.Serialization
{
    public class FormerlySerializedAsAttribute : Attribute { public FormerlySerializedAsAttribute(string name) { } }
}
public class UIPanel : UnityEngine.MonoBehaviour
{
    public void SetVisibleImmediate(bool visible) => gameObject.SetActive(visible);
}
