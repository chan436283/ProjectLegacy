// Production formation/unit/controller logic is compiled above; Unity lifecycle/rendering require Play Mode.
using System;

public class CharacterStatsComponent { public CharacterStats Stats = new(); }
public class CharacterStats
{
    public bool IsDead;
    public BattleStats Battle = new();
    public float TakeDamage(float amount) => amount;
}
public class BattleStats { public StatValue Speed = new(); }
public class StatValue { public float Value = 10; }
public class BattleSkill { }
public class BattleUnitView { }
public class AIActionSet { }
public class AIActionPattern { public void SetActionSet(AIActionSet set) { } }
public class BattleUnitStatusView
{
    public void SetUnit(BattleUnit unit) { }
    public void SetActive(bool active) { }
}
namespace CWFramework
{
    public class CBehaviour
    {
        public string name = "test";
        public UnityEngine.Transform transform = new();
        public UnityEngine.GameObject gameObject = new();
        public bool isActiveAndEnabled = true;
        public static int Destroyed;
        protected virtual void OnAwake() { }
        protected virtual void OnStarted() { }
        public T GetComponent<T>() where T : class => null;
        protected static BattleUnit Instantiate(BattleUnit prefab, UnityEngine.Transform parent)
            => Program.Unit(prefab.Side, prefab.Row, prefab.FormationPosition);
        protected static void Destroy(UnityEngine.GameObject obj) { Destroyed++; }
    }
}
namespace UnityEngine
{
    public struct Vector3 { public float x, y, z; }
    public class Transform { public string name = "slot"; public Vector3 position; }
    public class GameObject { public void SetActive(bool active) { } }
    public class ScriptableObject { }
    public class SerializeField : Attribute { }
    public class HideInInspector : Attribute { }
    public class HeaderAttribute : Attribute { public HeaderAttribute(string text) { } }
    public class TooltipAttribute : Attribute { public TooltipAttribute(string text) { } }
    public class MinAttribute : Attribute { public MinAttribute(float min) { } }
    public class DisallowMultipleComponent : Attribute { }
    public class RequireComponent : Attribute { public RequireComponent(Type type) { } }
    public class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
    public interface ISerializationCallbackReceiver { void OnBeforeSerialize(); void OnAfterDeserialize(); }
    public static class Debug { public static void LogWarning(string message, object context) { } }
}
namespace UnityEngine.Serialization
{
    public class FormerlySerializedAsAttribute : Attribute { public FormerlySerializedAsAttribute(string name) { } }
}
