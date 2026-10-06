# 원정 내부 이동 지도

`ExpeditionMapPanel`은 GameScene에서 사용할 스테이지 내부 지도 UI다.
마을에서 원정 스테이지를 고르는 기존 `StageSelectionPanel` / `MapPoint`와 별도로 사용한다.
지도 배경과 지점의 RectTransform 위치는 Unity에서 직접 배치한다.
코드는 지점을 정렬하거나 좌→우 진행을 가정하지 않는다. 북쪽·남쪽·서쪽·대각선 모두 같은 방식으로 처리한다.

## 연결 데이터와 지도 표시

각 `StageNode`가 나가는 `StageRoute[] routes`를 갖고, 각 항목의 `targetNodeId`가 도착 지점을 가리킨다.
예를 들어 1번 지점의 routes에 2번과 3번을 넣으면, 1번의 콘텐츠 완료 후 2번과 3번 버튼이 활성화된다.
`routeId`와 `label`은 기존 이동 요청·발견 기록과의 호환을 위해 유지한다.
피로도·식량 등 이동 비용이나 추가 개방 조건은 아직 구현하지 않는다.

지도에는 배경과 지점 버튼만 연결한다. 별도 경로 UI, 연결선, 중간 지점 설정은 필요하지 않다.
길의 모양은 지도 배경 이미지로 표현하며, 이동 가능한 지점은 버튼과 Available Indicator로 표시한다.

## Unity 구성

```text
ExpeditionMapPanel (UIPanel, ExpeditionMapPanel)
  MapArea
    Background (Image)
    Points
      Start (Button, CanvasGroup, ExpeditionMapPoint)
        Label (TMP_Text)
        CurrentIndicator
      DestinationA (Button, CanvasGroup, ExpeditionMapPoint)
        Label (TMP_Text)
        AvailableIndicator
      DestinationB (Button, CanvasGroup, ExpeditionMapPoint)
        Label (TMP_Text)
        AvailableIndicator
```

1. 지도 루트에 `UIPanel`과 `ExpeditionMapPanel`을 추가한다.
   Panel, Stage Id, Points를 연결한다. Stage Id는 `StageDefinition.stageId`와 같아야 한다.
   Stage Name Text / Current Node Text는 선택 사항이다. Points에는 해당 스테이지의 모든 지점을 한 번씩 연결한다.
   지도 루트는 초기화 시 숨겨지며 외부에서 `Open(run)`으로 연다.
2. Background Image에 지도 스프라이트를 연결한다. 배경과 지도 영역의 크기·비율은 Unity에서 맞춘다.
   지점에는 Layout Group을 사용하지 않고, 지도 위의 원하는 위치에 RectTransform을 배치한다.
   같은 지도 내 요소에 일관된 앵커/부모를 사용해 해상도 변경에도 배경과 위치가 함께 유지되도록 한다.
3. 지점마다 `ExpeditionMapPoint`의 Node Id를 `StageNode.nodeId`와 맞추고 Button과 Label을 연결한다.
   Button에 클릭을 받을 Image가 있어야 한다. Button OnClick은 비워둔다. 코드가 리스너를 연결한다.
   Icon을 연결한다면 초기 Sprite가 발견 후 아이콘이다. Unknown Icon에는 미발견용 공통 스프라이트를 지정한다.
   Current / Available / Visited / Completed Indicator는 선택 사항인 장식 자식 오브젝트다.
   지점 자체나 부모를 Indicator에 연결하지 않는다. 장식 그래픽의 Raycast Target은 꺼둔다.

스테이지마다 지도 배경과 배치가 다른 경우 각각의 지도 루트/프리팹을 만들고 Stage Id를 지정한다.
어떤 지도를 생성해 열지는 이후 GameScene 원정 컨트롤러에서 결정한다.
지도 루트를 ScrollRect의 Content로 사용해도 된다. 자동 스크롤·줌·이동 마커 애니메이션은 구현 범위에 포함하지 않았다.
이전 경로 표시 컴포넌트를 씬에 배치했다면 해당 경로 오브젝트와 중간 지점은 Unity에서 제거한다.

## 표시와 이동 요청

- 현재 위치: Current Indicator. 현재 지점이 완료되었다면 Completed Indicator도 함께 켠다.
- 지금 갈 수 있는 지점: Available Indicator와 버튼 활성화. 클릭하면 `RouteRequested(routeId)`가 한 번 발생한다.
- 발견한 연결의 도착 지점은 표시하지만, 미발견 지점 이름은 `?`, 아이콘은 Unknown Icon으로 표시한다.
- 아직 발견하지 못한 연결 너머의 지점은 숨긴다.
- 이전 원정의 발견 기록은 표시를 공개하지만 이번 원정의 이동 가능 여부/완료 여부를 바꾸지 않는다.
- 전투·이벤트 처리 중이거나 원정 완료 상태이면 진행 데이터에 따라 이동 버튼을 비활성화한다.

이동 가능 여부는 `ExpeditionRun.AvailableRoutes`로 판단한다. 화면 좌표와 배열 순서는 영향을 주지 않는다.
`GetMapNodes()`로 출발 시 확정한 이름·연결을 읽으므로 실행 중 StageDefinition 에셋을 수정해도 UI 연결은 바뀌지 않는다.
스테이지 불일치, 누락/중복된 지점, 서로 다른 지점이 공유하는 버튼은 Open에서 거절한다.

UI는 원정 상태를 변경하거나 전투를 시작하지 않는다. 이후 진행 컨트롤러는 다음 계약으로 연결한다.

```csharp
// run은 GameSession.CurrentExpedition 또는 테스트용 stage.CreateRun() 결과다.
map.RouteRequested += OnRouteRequested; // Open 전에 구독한다.
map.Open(run);

void OnRouteRequested(string routeId)
{
    // 콜백이 들어오기 전에 지도 입력이 잠긴다.
    run.ChooseRoute(routeId);
    map.Refresh(); // 새 현재 위치와 발견 상태 표시

    // 여기서 진행 컨트롤러가 run.CurrentContent를 실행하거나 결과 UI를 연다.
    // 지도 표시가 필요 없으면 map.HideImmediate()를 호출한다.
    // 지점 완료는 실제 콘텐츠 성공 시에만 run.CompleteCurrentNode(nodeId)를 호출한다.
    map.SetBusy(false); // 콘텐츠 처리 중이면 AvailableRoutes가 비어 있어 이동은 계속 차단된다.
}

// 콘텐츠 완료 후 지도를 다시 표시할 때
map.SetBusy(false);
map.Open(run);

// 연결한 컨트롤러를 해제할 때
map.RouteRequested -= OnRouteRequested;
map.Refresh();
```

요청을 거절하거나 시작에 실패했다면 컨트롤러가 `SetBusy(false)`로 잠금을 해제한다.
외부에서 이동·완료·발견 기록을 변경하면 `Refresh()`를 호출한다.
같은 원정을 Hide/Open해도 진행 중인 요청의 잠금은 유지한다. 새로운 원정을 Open하면 잠금이 초기화된다.
지도는 클릭 시 현재 원정 경로를 다시 확인하므로 갱신이 늦어진 버튼으로 이미 지난 지점을 선택할 수 없다.

## 검증

`dotnet run --project Tests/ExpeditionMap/ExpeditionMap.csproj`

실제 UI 코드를 Unity 대역으로 실행한다. 별도 경로 UI 없이 북쪽·남쪽 분기와 서쪽 이동,
발견 상태, 콘텐츠 처리 잠금, 중복 클릭, 오래된 UI, 원정 스냅샷, 잘못된 지점 연결을 검증한다.
Canvas 렌더링·버튼 Raycast·Unity 수명 주기는 Unity Play Mode에서 별도로 확인해야 한다.
