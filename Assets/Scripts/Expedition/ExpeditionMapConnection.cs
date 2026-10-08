using System;

/// <summary>같은 맵 안의 다음 포인트 연결. 이동 비용·개방 조건은 추후 이곳에 추가합니다.</summary>
[Serializable]
public sealed class ExpeditionMapConnection
{
    public ExpeditionMapPoint target;
}
