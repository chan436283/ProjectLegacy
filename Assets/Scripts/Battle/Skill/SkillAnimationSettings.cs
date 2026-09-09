using System;
using UnityEngine;

[Serializable]
public sealed class SkillAnimationSettings
{
    [Tooltip("스킬 실행 시 재생할 애니메이션. None은 생략합니다.")]
    [SerializeField] private SkillAnimationType startAnimation;

    [Tooltip("시작 모션 이후 유지할 애니메이션. None은 생략합니다.")]
    [SerializeField] private SkillAnimationType loopAnimation;

    [Tooltip("유지 상태가 해제될 때 재생할 애니메이션. None은 생략합니다.")]
    [SerializeField] private SkillAnimationType endAnimation;

    [Tooltip("유지 모션과 연결할 StatusEffect의 상태 ID. 유지 모션을 사용할 때 지정합니다.")]
    [SerializeField] private string maintainWhileStatusId;

    public SkillAnimationType StartAnimation => startAnimation;
    public SkillAnimationType LoopAnimation => loopAnimation;
    public SkillAnimationType EndAnimation => endAnimation;
    public string MaintainWhileStatusId => maintainWhileStatusId;
}
