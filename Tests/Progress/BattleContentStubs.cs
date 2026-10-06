// 콘텐츠 설정 테스트용 대역. 실제 BattleUnit 생성과 Unity 수명 주기는 Play Mode 검증 대상입니다.
public sealed class BattleUnit
{
    public BattleSide Side { get; set; } = BattleSide.Enemy;
    public BattleControlType ControlType { get; set; } = BattleControlType.AI;
}
namespace UnityEngine
{
    public class GameObject { }
    public sealed class MinAttribute : System.Attribute { public MinAttribute(float min) { } }
}

namespace UnityEngine
{
    public sealed class HideInInspector : System.Attribute { }
    public interface ISerializationCallbackReceiver
    {
        void OnBeforeSerialize();
        void OnAfterDeserialize();
    }
}
namespace UnityEngine.Serialization
{
    public sealed class FormerlySerializedAsAttribute : System.Attribute
    {
        public FormerlySerializedAsAttribute(string oldName) { }
    }
}
