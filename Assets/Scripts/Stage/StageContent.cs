using System;
using UnityEngine;

public enum StageContentType { Battle = 1, Event = 2 }

/// <summary>재사용하는 콘텐츠 설정. 실행 중 상태는 에셋에 저장하지 않습니다.</summary>
public abstract class StageContent : ScriptableObject
{
    public string contentId;
    public string description;
    public abstract StageContentType Type { get; }

    public virtual void Validate()
    {
        if (string.IsNullOrWhiteSpace(contentId) || contentId != contentId.Trim())
            throw new ArgumentException("콘텐츠 ID는 앞뒤 공백 없이 지정해야 합니다.");
        if (!Enum.IsDefined(typeof(StageContentType), Type))
            throw new ArgumentException("알 수 없는 콘텐츠 종류입니다.");
    }
}
