using UnityEngine;

/// <summary>이벤트 정의의 기본 틀. 선택지와 결과 설정은 추후 추가합니다.</summary>
[CreateAssetMenu(fileName = "EventContent", menuName = "ProjectLegacy/Expedition/Event Content")]
public sealed class EventContent : StageContent
{
    public override StageContentType Type => StageContentType.Event;
}
