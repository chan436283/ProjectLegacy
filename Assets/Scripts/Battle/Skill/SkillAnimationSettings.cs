using System;
using UnityEngine;

[Serializable]
public sealed class SkillAnimationSettings
{
    [Tooltip("Animator 파라미터 이름. 비워두면 애니메이션을 실행하지 않습니다.")]
    [SerializeField] private string parameterKey;
    [SerializeField] private ParameterType parameterType;
    [SerializeField] private bool boolValue = true;

    [Tooltip("Bool에만 사용합니다. 연결된 상태가 해제되면 false로 설정합니다. 비워두면 설정값을 유지합니다.")]
    [SerializeField] private string maintainWhileStatusId;

    public string ParameterKey => parameterKey;
    public ParameterType ParameterType => parameterType;
    public bool BoolValue => boolValue;
    public string MaintainWhileStatusId => maintainWhileStatusId;

    public SkillAnimationSettings() { }

    public SkillAnimationSettings(string triggerKey)
    {
        parameterKey = triggerKey;
        parameterType = ParameterType.Trigger;
    }
}
