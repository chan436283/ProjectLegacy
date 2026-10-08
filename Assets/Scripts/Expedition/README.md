# 원정 월드 지도

`ExpeditionMap`은 스테이지별 월드 지도 프리팹의 루트 컴포넌트다.
지도 배경·지점은 SpriteRenderer로 배치하고 캐릭터도 같은 월드 좌표에서 표현한다.
지도 위치를 코드에서 정렬하지 않으며 북쪽·남쪽·좌우 방향과 무관하게 노드 연결로 이동 가능 여부를 판단한다.
마을의 스테이지 선택 UI인 `StageSelectionPanel` / `MapPoint`는 기존 Canvas UI로 유지한다.

## Unity 구성

```text
ExpeditionMapRoot (ExpeditionMap)
  Background (SpriteRenderer)
  Points
    Start (ExpeditionMapPoint, BoxCollider2D)
      Marker (SpriteRenderer)
      CurrentIndicator
    Destination (ExpeditionMapPoint, BoxCollider2D)
      Marker (SpriteRenderer)
  Party (지도에서 사용할 캐릭터 표현)
```

1. 루트에 ExpeditionMap을 붙이고 Start Point와 Background를 연결한다.
   포인트는 비활성 오브젝트를 포함해 자식에서 자동 수집한다. 별도 Points 목록이나 Stage Id는 없다.
   StageDefinition의 Map Prefab에는 이 루트 컴포넌트를 연결한다.
   Background는 카메라 경계 연결을 위한 선택 항목이다. `map.Background`로 참조한다.
2. 지점마다 Node Id, Hit Area(Collider2D), Marker(SpriteRenderer)를 연결한다.
   Hit Area를 비우면 같은 오브젝트의 Collider2D를 사용한다. 클릭을 받는 콜라이더는 지점 자신 또는 자식에 둔다.
   Marker는 지점의 자식 SpriteRenderer로 연결한다. Collider2D는 그 자식 아래에 두지 않는다.
   이동 가능한 동안 Marker만 원래 크기의 1.5배까지 1.2초 간격으로 왕복 확대한다(MapPoint와 동일한 연출).
   별도 Available Indicator나 Pulse Target 연결은 필요 없다. 갱신 시 주기를 유지하고 입력 잠금·비활성화 시 원래 크기로 복원한다.
   Marker의 Sprite는 발견 여부와 관계없이 그대로 사용한다. Unknown Icon은 필요하지 않다.
   이동 가능한 지점은 초기 Marker 색상, 이동 불가능한 지점(현재 위치 포함)은 회색으로 표시한다.
   초기 알파는 유지하며, 일시적인 입력 잠금은 색상을 바꾸지 않는다.
3. Display Name은 공용 UI에서 사용할 선택 문자열이다. 비워둘 수 있고 지점은 텍스트를 직접 표시하지 않는다.
   공용 UI는 `point.DisplayName`으로 이름을 읽는다. 미발견 이름을 숨기려면 `point.IsDiscovered`를 함께 확인한다.
   이 문자열이 포인트 이름의 설정 원본이며 실행 시 원정 데이터에도 복사된다.
   Current Indicator만 선택 장식 자식 오브젝트로 유지한다. 지점 자신이나 부모를 연결하지 않는다.
   방문·완료 표시와 Label, Available Indicator는 제거했다. 클릭은 Hit Area에서만 처리한다.
4. 지도 카메라에 **Physics2DRaycaster**를 추가하고 Event Mask에 지점 레이어를 포함한다.
   씬에 EventSystem과 **InputSystemUIInputModule**이 있어야 한다.
   UI 팝업은 기존 Canvas/GraphicRaycaster를 사용해 월드 클릭보다 앞에서 입력을 받도록 구성한다.
   왼쪽 클릭만 이동 요청으로 처리하며 오른쪽 클릭·드래그는 제외한다.
5. 지도 프리팹은 Canvas가 아닌 월드 공간의 ExpeditionMapRoot 아래에 생성한다.
   카메라, 원정 진행 컨트롤러와 고정 HUD는 지도 프리팹 밖에 둔다.

각 포인트의 Connections에는 Target(같은 프리팹 안의 ExpeditionMapPoint)만 설정한다.
Content에는 BattleContent 또는 EventContent를 연결한다. 비워두면 바로 다음 이동을 선택할 수 있다.
종료 지점은 Is Ending을 켜고 고유 Ending Id를 지정한다. 종료점의 Connections는 비워둔다.
StageNode와 StageRoute는 제거했으며, 이동 비용·추가 개방 조건은 아직 구현하지 않는다.
Node Id는 스테이지 안에서 고유해야 한다. 경로 ID는 출발/목적지 ID로 자동 생성한다(예: n1->n2).
구분 문자는 이스케이프하며 방향별로 다른 ID를 사용한다. 같은 목적지의 중복 연결은 거절한다.
시작점 하나, 최소 하나의 종료점, 모든 포인트의 도달 가능성을 검증한다. 기존과 같이 순환 경로는 허용하지 않는다.
이동 경로 선이나 별도 경로 표시 오브젝트는 필요하지 않다.
이름·연결·콘텐츠는 맵의 첫 Show 시 ExpeditionRun에 복사한다. 이전 원정에서 발견한 지점은 공개하지만,
이동 가능한지는 현재 진행 상태로 판단한다. 포인트 상태는 Hidden / Idle / Available / Current만 구분한다.
방문·완료 기록은 원정 진행 데이터에 유지하되 지도에서 별도 표시하지 않는다.

## 원정 진행 컨트롤러

`ExpeditionController`가 GameScene 진입부터 맵/전투/이벤트 전환을 담당한다.
Start Automatically가 켜져 있으면 Start에서 GameSession.CurrentExpedition을 읽고 MapPrefab을 생성한다.
맵을 수동으로 Instantiate하거나 RouteRequested에 별도로 이동 처리기를 연결할 필요가 없다.
절차적으로 만든 맵은 Start Automatically를 끄고 `Begin(run, generatedMap)`으로 전달한다.

권장 하이어라키:

```text
ExpeditionController (항상 활성, 아래 화면 루트들의 형제)
ExpeditionMapRoot                 ← Map Root
  MapCamera + ExpeditionMapCameraController
  PartyMarker                    ← 선택 사항, 위치 표시용
  (스테이지의 ExpeditionMap 프리팹이 실행 중 생성됨)
BattleRoot                       ← Battle Root
  BattleCamera
  BattleController + BattleFormation + 행동 실행/입력 컴포넌트
  Allies
  Enemies                        ← 선택적 Enemy Parent
  Backgrounds                    ← 선택적 Background Parent
Canvas
  EventPanel (ExpeditionEventPanel)
  Result UI
```

- Map Root와 Battle Root는 서로 독립된 오브젝트로 연결한다. 두 루트 밖에 ExpeditionController를 둔다.
- Battle Controller는 Battle Root 내부의 기존 컴포넌트를 연결한다. 자동 전투 시작은 원정 컨트롤러가 Awake에서 끈다.
- 아군은 기존 BattleController의 Participants에 등록하고 BattleFormation의 슬롯을 연결한다. 아군 파티 생성·GameData 스탯 바인딩은 이 컴포넌트가 담당하지 않는다.
- Map Camera에 지도 카메라 이동 컴포넌트를 연결한다. 화면별 카메라를 각 Root 아래에 두면 루트 전환으로 함께 켜고 끈다.
- Enemy Parent와 Background Parent는 Battle Root 내부 생성 위치이며 비우면 Battle Root를 사용한다.
- Default Battle Background는 선택 사항이다. 콘텐츠에 배경 프리팹이 있으면 기본 배경을 숨기고 전투별 배경을 생성한다.
- Party Marker는 선택 사항이다. 포인트 진입 시 해당 월드 XY로 즉시 이동하며 Z는 유지한다. 경로를 따라 걷는 애니메이션은 아직 없다.
- Event Panel에 ExpeditionEventPanel을 연결하고 Description(TMP_Text), Continue Button을 연결한다. 설명 확인 후 완료하는 기본 이벤트 화면이다.
- Return Scene은 기본 TownScene이며 빌드 목록에 포함해야 한다. 결과 UI의 귀환 버튼에 `ReturnToTown()`을 연결한다.

실행 흐름:

1. 맵 생성·초기화 → 시작점 진입. 시작점에도 콘텐츠가 있으면 바로 실행한다.
2. 포인트 클릭 → ChooseRoute → 현재 위치 변경 → 콘텐츠 확인.
3. 전투: 맵/지도 입력 끄기 → 배경 생성 → 적 그룹 한 번 선택 → 전투 시작.
4. 승리: BattleEnded 이벤트 전달 이후 Update에서 전투 루트를 끄고 생성 적·일시 상태 정리 → 포인트 완료 → 맵 복귀.
5. 패배: 포인트를 완료하지 않고 Defeated 상태와 ExpeditionDefeated 이벤트를 제공한다.
6. 이벤트: 맵을 유지하고 이동/카메라 입력을 잠근 뒤 설명 패널 표시 → 확인 시 완료.
7. 종료점 콘텐츠 완료: Completed 상태와 ExpeditionCompleted(endingId) 이벤트. 맵은 보이되 이동은 잠긴다.
8. 결과 UI에서 ReturnToTown → 저장 → 씬 로드 수락 → GameSession의 현재 원정 해제.

`BattleController.ResetBattle()`은 종료된 전투에서만(또는 Idle에서) 사용할 수 있다.
직접 생성한 적만 제거하며 기존 아군과 HP/MP를 유지한다. BattleStatus는 제거한다.
승리/패배 이벤트 안에서 즉시 호출하지 않고 모든 구독자가 결과를 받은 뒤 호출한다.

결과 UI는 `ExpeditionCompleted`, `ExpeditionDefeated`를 구독해 표시한다. 자동 귀환은 하지 않는다.
`ErrorOccurred`/`LastError`로 설정·저장 오류를 표시할 수 있다. 콘텐츠 실행 실패 시 Faulted로 잠그고 귀환할 수 있다.
귀환 저장/씬 로드 실패 시 결과 상태와 현재 원정을 유지하므로 버튼으로 재시도할 수 있다.
지도 진입/콘텐츠 완료 시 현재 세션의 누적 발견 기록을 저장한다. 저장 실패는 알리되 진행 완료를 취소하지 않는다.
보상·도주·이벤트 선택지 실행·왕복 경로·원정 중 저장 복구는 이번 구현에 포함하지 않는다.

커스텀 이벤트 실행기는 기본 Event Panel을 비우고 `EventRequested`를 구독한다.
전달받은 `ExpeditionEventRequest`를 보관하고 성공 시 `CompleteEvent(request)`를 호출한다.
이전 요청이나 중복 완료는 무시한다. Event Panel도 처리기도 없으면 자동 완료하지 않고 설정 오류로 처리한다.
`PointEntered(nodeId)`는 실제 진입 시에만 발생하며 전투 완료 후에는 중복 발생하지 않는다.

검증: `Tests/ExpeditionFlow`는 실제 컨트롤러·맵·진행·이벤트 패널 코드와 Unity/전투/씬 로딩 대역으로
맵 생성, 전투 전환, 연속 전투 요청, 이벤트 완료, 보스/패배 결과, 귀환 실패/재시도, 구독 정리를 검증한다.
`Tests/Formation`은 실제 BattleController의 적 정리와 두 번째 전투 시작을 검증한다.
Unity의 카메라·코루틴·Instantiate 생명주기 및 실제 입력은 Play Mode 확인이 필요하다.

## 개별 맵 API 연결 (커스텀 진행 컨트롤러 사용 시)

아래 예제는 ExpeditionController 대신 자체 진행 코드를 작성할 때 사용한다.


```csharp
// GameScene의 진행 컨트롤러에서 호출한다. 씬/프리팹 연결은 Unity에서 설정한다.
var run = GameSession.CurrentExpedition;
var map = Instantiate(run.MapPrefab, mapRoot);
map.RouteRequested += OnRouteRequested;
mapCamera.SetMapBackground(map.Background);
map.Show(run);
mapCamera.SetInputEnabled(true);

void OnRouteRequested(string routeId)
{
    // 지도는 요청을 보내기 전에 중복 클릭을 잠근다.
    run.ChooseRoute(routeId);
    map.Refresh();
    // 진행 컨트롤러가 현재 지점의 콘텐츠를 실행한다.
    // 실제 성공 시에만 run.CompleteCurrentNode(nodeId)를 호출한다.
    map.SetBusy(false);
}

// 전투 등으로 전환할 때
mapCamera.SetInputEnabled(false);
map.Hide();
```

- `BuildMapData()`: 포인트 설정을 검증하고 컴포넌트 참조 없는 실행용 데이터로 복사.
- `Show(run)`: 최초 호출에서 원정 초기화, 연결 검증, 지도 루트 활성화, 현재 위치·이동 가능 애니메이션·발견 상태 표시.
- `Hide()`: 지도 루트 비활성화. 캐릭터 등 자식 오브젝트도 함께 숨겨진다.
- `Refresh()`: 외부에서 진행/발견 기록을 변경했을 때 표시 갱신.
- `SetBusy(bool)`: 이동 요청의 중복 입력 잠금. 같은 원정의 Hide/Show는 잠금을 유지한다.
- `RouteRequested(routeId)`: 이동 요청 이벤트. 실제 이동·전투·저장 처리는 진행 컨트롤러가 담당한다.

지도는 처음 Awake 시 숨겨지며 Show를 호출하면 표시된다. 고정 HUD는 별도 Canvas에서
`map.Run.StageName`, `map.Run.CurrentNodeName` 등을 읽어 표시한다.

## 프리팹 설정으로 이전 및 절차적 생성

기존 StageDefinition의 Nodes/Start Node Id 설정은 자동 이전되지 않는다.
씬·프리팹·SO 에셋은 수정하지 않았으므로, 각 포인트에 콘텐츠와 연결을 옮기고 Map Prefab과 Start Point를 연결해야 한다.
기존 노드/종료 ID의 발견 기록은 유지된다. 수동 경로 ID 기록은 자동 ID로 이전하지 않는다.
기존 저장의 경로 발견 표시는 해당 출발점을 다시 방문하면 새 ID로 기록된다.
`CreateRun()` 직후에는 `IsInitialized == false`, `CurrentNodeId == null`이며 첫 `Show` 후 시작점에 진입한다.
`GameSession.StartExpedition()`은 초기화 전에도 누적 기록을 연결할 수 있다.

절차적 생성기는 맵의 자식으로 포인트를 생성하고 `point.Configure(...)`로 설정한 뒤
`map.SetStartPoint(start)`와 `map.Show(run)`을 호출한다. 포인트의 시각 초기화 전에 구성을 완료한다.
화면 없이 경로를 생성하려면 `ExpeditionMapData`를 만들어 `run.Initialize(data)`에 전달할 수도 있다.
실행용 데이터 타입은 Inspector에서 별도로 작성하거나 보관하는 에셋이 아니다.
콘텐츠 에셋 참조는 공유하지만 포인트 컴포넌트 자체는 진행 그래프에서 참조하지 않는다.

## 이전 UI 지도에서 전환

ExpeditionMapPanel을 ExpeditionMap으로 이름 변경하고 스크립트 GUID를 보존했다.
`UIPanel`, `CanvasGroup`, `Button`, `Image`, `RectTransform`을 요구하지 않는다.
기존 컴포넌트 참조를 유지해도 월드용 SpriteRenderer와 Collider2D는 Unity에서 새로 연결해야 한다.
기존 UI용 컴포넌트와 Canvas 부모 관계는 Unity에서 정리한다.
호출 코드는 `Open(run)` → `Show(run)`, `HideImmediate()` → `Hide()`, `IsOpen` → `IsVisible`로 바꾼다.
이번 변경은 씬·프리팹 에셋을 직접 수정하지 않는다.

검증: `dotnet run --project Tests/ExpeditionMap/ExpeditionMap.csproj`.
노드 연결·발견 상태·중복 클릭·스냅샷 및 월드 마커·콜라이더 표시, 애니메이션 시작/유지/정리와 원래 크기 복원을 실제 코드와 Unity/DOTween 대역으로 검증한다.
Physics2DRaycaster, Sprite 정렬, Collider 크기, UI와의 입력 우선순위는 Play Mode에서 확인한다.

## 월드 지도 카메라 이동

`ExpeditionMapCameraController`는 SpriteRenderer로 구성한 XY 평면 지도를 위한 독립 컴포넌트다.
기존 Canvas 지도 패널을 이동시키지 않으며, 지도 배경·지점·캐릭터를 바라보는 직교 카메라를 이동시킨다.

1. 지도 카메라 또는 원정 전용 오브젝트에 컴포넌트를 붙이고 Target Camera를 연결한다.
   비워두면 같은 오브젝트의 Camera, 다음으로 Camera.main을 찾는다.
   카메라는 Orthographic, 월드 회전 (0, 0, 0)으로 두고 Z 위치로 지도와 거리를 둔다.
2. Move Speed는 초당 월드 이동 거리다. Edge Margin Pixels는 카메라 표시 영역 안쪽의 감지 폭이며 기본값은 24px다.
   화면 전체 크기 대신 Camera.pixelRect를 사용하므로 부분 화면 카메라의 위치도 반영한다.
3. Edge Scrolling, Use WASD, Use Arrow Keys를 개별적으로 켜거나 끌 수 있다. 기본값은 모두 켜짐이다.
   키보드 입력이 있으면 가장자리 이동보다 우선한다. 대각선도 직선과 같은 속도로 이동한다.
4. Map Background에 지도 SpriteRenderer를 연결하면 카메라가 보이는 영역까지 고려해 이동을 제한한다.
   지도보다 화면이 큰 축은 지도의 중앙에 고정된다. 비워두면 이동 범위를 제한하지 않는다.
   스테이지 배경을 생성한 후 `SetMapBackground(background)`로 지정하거나 교체해도 된다.
5. 전투·팝업·다른 지도 이동 연출 중에는 `SetInputEnabled(false)`를 호출하고, 지도 탐색 시 true로 복구한다.
   또는 원정 지도 전용 오브젝트/컴포넌트를 비활성화한다. 카메라만 공유할 때도 입력 잠금은 필요하다.

게임 창에 포커스가 없거나 TMP/uGUI 텍스트 필드에 입력 중이면 이동하지 않는다.
커서가 카메라 영역 밖에 있거나 잠금 상태, 마우스 버튼을 누른 상태이면 가장자리 이동을 하지 않는다.
Block Edge Over UI가 켜져 있으면 UI 위에 있을 때도 가장자리 이동을 막는다.
장식용 전체 화면 Image가 있다면 Raycast Target을 꺼 불필요하게 이동을 막지 않도록 한다.
키보드 이동은 커서 위치와 무관하게 동작한다.

이동 시간은 unscaledDeltaTime을 사용하므로 timeScale=0에서도 지도 탐색이 가능하다.
모달 UI 등에서 멈추려면 명시적으로 입력을 잠근다. 확대·축소와 드래그 이동은 이 컴포넌트의 기능이 아니다.
새 Input System을 사용하며 별도 Input Actions 에셋 연결은 필요하지 않다.

검증: `dotnet run --project Tests/MapCamera/MapCamera.csproj`.
가장자리·부분 화면 좌표, 대각선 속도, 키보드 우선순위, 텍스트 입력/포커스 차단,
배경 경계 및 Z 위치 유지를 실제 컨트롤러 코드와 Unity 대역으로 검증한다.
실제 카메라 화면과 입력 이벤트는 Unity Play Mode에서 확인한다.

가장자리 이동은 지도 활성화 직후 대기 상태로 시작한다. 마우스가 카메라 화면 안의
가장자리 감지 구역을 제외한 영역에 한 번 들어와야 활성화된다. 초기 좌표 (0, 0)이나
화면 밖 좌표는 활성화 조건이 아니다. 같은 프레임에 읽은 마우스 좌표를 재사용한다.
WASD·방향키는 이 대기 상태와 무관하게 즉시 사용할 수 있다.
컴포넌트 재활성화, 게임 창 포커스 변경, 입력 잠금/해제, 지도 배경 교체 시 다시 대기한다.
