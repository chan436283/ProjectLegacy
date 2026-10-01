# 지도에서 원정 시작

현재 흐름: 마을 원정 버튼 → 같은 씬의 지도 패널 → 지점 아이콘 선택 → 중앙 상단 이름 확인 → 출정 버튼 → 해당 진입 씬.
원정 버튼과 지도 닫기는 씬 로드나 저장을 하지 않는다. 출정 버튼을 누르면 진행 데이터를
저장하고, 스테이지의 `entryScene`을 로드한다. 현재 예제는 기존 `GameScene` 전투
프로토타입으로 진입한다. 원정 내 분기·이벤트·귀환 결과 처리는 아직 구현하지 않는다.

## Unity 연결

1. TownScene Canvas 아래, 메인 메뉴의 **자식이 아닌 형제**로 지도 패널을 만든다.
   메뉴보다 위에 표시되도록 뒤쪽 sibling에 배치한다. 전체 영역을 덮는 Image의
   Raycast Target을 켜서 뒤쪽 클릭을 막는다. 이 Image에 지도 이미지를 넣어도 된다.
2. 지도 패널 루트에 `UIPanel`과 `StageSelectionPanel`을 붙인다. CanvasGroup은
   UIPanel이 요구한다. 지도, 지점 버튼, 닫기 버튼, 상태 문구는 모두 루트의 자식으로 둔다.
3. 원하는 지도 위치에 Image + Button을 배치한다. 위치·아이콘 Sprite·크기는 RectTransform과
   Image로 직접 설정한다. 별도 레이아웃 그룹을 사용하지 않으면 원하는 위치를 유지한다.
4. `StageSelectionPanel`의 Panel, Close Button, Depart Button을 연결한다.
   중앙 상단에 스테이지 이름용 TMP Text 하나를 만들고 Stage Info Text에 연결한다.
   안내 문구·선택한 스테이지 이름·오류를 이 Text 하나로 표시한다.
   기존 Selected Stage Name Text 참조는 자동 유지된다. 기존 Status Text 오브젝트는 제거하거나 숨긴다.
   지점 루트에 `MapPoint` 컴포넌트를 붙여 Stage, Button, Available을 설정한다.
   패널의 Points에는 지도에 배치한 MapPoint 인스턴스를 연결한다.
   Selected Indicator에는 선택 표시용 자식 오브젝트를 연결한다.
   선택 인디케이터는 선택 사항이며 버튼 자체나 부모를 연결하지 않는다.
   인디케이터 Image의 Raycast Target은 꺼둔다. 지점별 Label 항목은 제거했다.
   Button의 OnClick은 비워둔다. 코드에서 이벤트를 연결한다.
5. `TownController.stageSelectionPanel`에 위 컴포넌트를 연결한다.
   기존 원정 버튼 연결은 그대로 사용한다. 지도 루트는 시작 시 자동으로 숨겨진다.
6. `Assets/Data/Expedition/TestStage.asset`을 첫 지점에 연결한다.
   새 지점은 Create > ProjectLegacy > Expedition > Stage로 추가한다.
   Stage ID는 지도 안에서 고유해야 하며, Entry Scene은 Build Profiles의 씬 목록에 있어야 한다.

지도 배치와 Inspector 연결은 아직 씬에 적용하지 않았다. 참조가 없으면 원정 버튼은
설정 안내를 표시하고 씬을 전환하지 않는다. 지도와 가문 구성원 패널은 동시에 열리지
않으며, 출발 중에는 지도 지점과 닫기 버튼의 중복 입력을 차단한다. 저장 또는 씬 로드
시작에 실패하면 지도에서 오류를 표시하고 다시 선택하거나 닫을 수 있다.

## 원정 컨텍스트

진입 씬에서 `GameSession.CurrentExpedition`의 StageId, StageName, EntryScene을
조회한다. 출발 시 문자열을 복사하므로 이후 에셋 수정의 영향을 받지 않는다.
현재는 메모리에만 보관하며 원정 중 저장/이어하기는 지원하지 않는다. 원정을 끝낼 때
`GameSession.EndExpedition()`으로 해제한다. 새 게임 시작과 저장 불러오기 성공 시에도
해제한다. 씬을 직접 실행하면 원정 컨텍스트가 없을 수 있다.

진입 씬 로드가 수락된 이후 원정 정보를 설정한다. 씬 안에서 발생하는 전투 초기화
오류나 원정 진행 오류의 복구는 이후 원정 플로우에서 다룬다.

## 선택과 출정

패널을 열 때는 미선택 상태로 시작하며 상단에 `원정 지점을 선택해주세요.`를 표시하고 출정 버튼은 비활성이다.
지점을 클릭하면 선택 표시와 상단 이름만 바뀐다. 출정 버튼을 누를 때만
`DepartureRequested` 이벤트가 발생하여 TownController가 원정을 시작한다.
진입 불가능한 지점은 버튼 비활성 상태이며 진입 가능 트윈도 정지한다.
출발 중에는 모든 입력을 차단하고 실패하면 선택을 유지한 채 다시 출정할 수 있다.
닫았다 열면 이전 선택은 초기화한다.

`SetStageAvailable(stageId, available)`로 진행 상황에 맞게 지점을 열거나 잠글 수 있다.
선택 중인 지점이 잠기면 선택을 해제하고 출정 버튼도 비활성화한다. 이 상태는 현재
패널 메모리에서만 유지하며 해금 조건과 저장은 추후 진행 시스템에서 관리한다.
호버 효과는 각 지점 Button의 Transition(Color Tint / Sprite Swap 등)으로 설정한다.
호버나 키보드 포커스만으로 스테이지가 선택되지는 않는다.

## 가문명을 포함한 스테이지 이름

Display Name에 `{FamilyName} 남서 가도` 또는 `{FamilyName} 북동 가도`를 입력한다.
토큰은 대소문자를 구분한다. 지도는 열 때 전달받은 가문명으로 표시하고, 출정할 때도
`GetDisplayName`으로 치환하여 원정 StageName에 저장한다. 에셋의 템플릿은 변경하지
않는다. 토큰이 없는 기존 이름은 그대로 표시하며, 가문명이 없는 직접 실행에서는
`이름 없는 가문`으로 표시한다.

## MapPoint 프리팹과 진입 가능 연출

MapPoint를 붙인 루트를 Project 창으로 드래그해 프리팹으로 만든다. 지도에 배치한
인스턴스마다 Stage와 Available을 지정하고 패널의 Points에 연결한다.
기존 패널 내부 MapPoint 항목은 컴포넌트 참조로 바뀌었으므로 다시 연결해야 한다.
현재 TownScene의 비어 있던 항목은 빈 컴포넌트 참조로 변경했다.

추천 구조:

```text
MapPoint (Button, MapPoint)
├── Icon (Image, Pulse Target)
└── Selection (Image, Selected Indicator)
```

Available Indicator는 제거했다. Pulse Target에 아이콘 RectTransform을 지정하면
진입 가능한 지점만 원래 크기와 1.06배 사이를 InOutSine으로 왕복한다.
Pulse Duration 기본값은 편도 1.2초(왕복 2.4초)이며 시간 배율이 0이어도 동작한다.
Pulse Target을 비우면 애니메이션을 사용하지 않는다. 클릭 영역을 안정적으로 유지하려면
버튼 루트 대신 자식 아이콘을 대상으로 지정한다. UIButtonPressEffect와 같은 Transform을
동시에 조절하지 않도록 대상을 분리한다.
잠금·출발·닫기·비활성화 시 트윈을 정지하고 원래 크기로 복원하며, 다시 열면 재개한다.
선택 변경만으로 이미 실행 중인 트윈을 재시작하지 않는다. 선택 인디케이터는 별도로 유지한다.
`MapPoint.SetAvailable()`을 직접 호출해도 패널에 반영된다.

## 고정 스테이지 정의와 진행 API

`StageDefinition` SO의 `nodes`는 스테이지 내부 직렬화 목록이다. 노드별 에셋은 만들지
않으며, 전투·이벤트에서 재사용할 정의만 `StageContent` SO로 분리했다.
StageContent는 추상 기반 클래스다. BattleContent와 EventContent가 상속하며 Type을 결정한다.
Create > ProjectLegacy > Expedition > Battle Content / Event Content로 만들고 contentId를 지정한다.
이벤트 선택지, 실제 전투 실행/승리 콜백은 이후 콘텐츠 구현에서 연결해야 한다.
휴식은 노드 콘텐츠가 아닌 원정 자원을 소비하는 별도 기능으로 구현할 예정이다.

노드의 kind는 StageNodeType: Start(0), Normal(1), Ending(4).
콘텐츠의 Type은 StageContentType: Battle(1), Event(2). 인스펙터에서 중복 지정하지 않는다.
기존 콘텐츠 에셋과 시작·종료 노드의 직렬화 숫자는 유지한다.
스테이지는 startNodeId와 nodes를 설정하고, 각 노드의 routes에 고유 routeId,
targetNodeId, 선택지 label을 입력한다. 종착점마다 고유 endingId를 지정한다.
Normal 노드는 콘텐츠 SO가 필수다. Start와 Ending은 콘텐츠를 선택적으로 연결한다.
노드의 구조적 종류와 콘텐츠 종류는 독립적이며 Ending + Battle로 보스전 엔딩을 구성할 수 있다.
시작은 하나, 종착점은 한 개 이상이며 순환·되돌아가기는 지원하지 않는다.
합류는 허용한다. 노드 ID와 경로 ID는 스테이지 안에서 고유하고 앞뒤 공백이 없어야 한다.
이름이나 목록 순서를 바꿔도 ID는 유지한다. 출시 후 ID 변경/삭제는 저장 마이그레이션이 필요하다.

스테이지 Inspector 컴포넌트 메뉴의 Validate Stage로 검증할 수 있다. 출정 시에도
동일 검증을 수행한다. 중복 ID, 없는 대상, 순환, 미도달 노드, 종착점 외 막다른 길,
누락된 필수 콘텐츠, 잘못된 콘텐츠 ID·종류 등을 거절한다. DAG이며 모든 노드가 도달 가능하고 모든 비종착점에
나가는 길이 있으므로 각 경로는 결국 종착점으로 이어진다.

콘텐츠 예제는 `Assets/Data/Expedition/StageContents`에 있고,
스테이지 에셋은 `Assets/Data/Expedition/Stages`에 있다.
새 스테이지 기본값은 시작→종착점만 있는 최소 지도다.

```csharp
// 실제 출정에서는 TownController가 CreateRun과 StartExpedition을 호출한다.
var run = stage.CreateRun(GameSession.Current?.FamilyName);
GameSession.StartExpedition(run);
// 콘텐츠가 없는 시작 노드는 자동 완료된다.
// 시작 콘텐츠가 있다면 실행 후 CompleteCurrentNode를 호출해야 한다.
run.ChooseRoute(run.AvailableRoutes[0].RouteId);
// CurrentContent를 종류별 실행기에 전달하고, 성공적으로 끝났을 때만 호출한다.
string enteredNodeId = run.CurrentNodeId;
run.CompleteCurrentNode(enteredNodeId);
// 전투 패배/취소 시에는 완료 호출을 하지 않는다.
GameSession.Save(); // 발견 기록을 디스크에 보존할 시점에 호출한다.
```

`ChooseRoute`는 현재 노드가 완료되고 실제 연결된 경로일 때만 성공한다.
콘텐츠가 없는 시작·종착점만 진입 시 자동 완료한다.
콘텐츠가 있으면 노드 종류와 무관하게 ResolvingNode에서 외부 실행기의 명시적 완료를 기다린다.
완료 콜백의 nodeId가 현재 노드와 다르거나 이미 완료되었으면 거절한다.
종착점 완료 시 Phase=Completed, EndingId와 엔딩 발견 기록이 설정되고 추가 이동은 거절한다.
보스전 패배·취소 시에는 CompleteCurrentNode를 호출하지 않으며 엔딩도 기록되지 않는다.
AvailableRoutes에는 현재 선택 가능한 경로만 노출된다. 에셋의 연결/이름/종류는 출발 시
복사하므로 진행 중 에셋 변경이 현재 경로를 바꾸지 않는다. 콘텐츠 SO 참조는 공유한다.

## 발견 기록과 저장

이번 원정의 CurrentNodeId, VisitedNodes, TraversedRoutes, CompletedNodes, Phase는
`ExpeditionRun`에만 유지하며 원정 중 이어하기 저장은 아직 지원하지 않는다.
`GameData.GetExploration(stageId)`에는 누적 발견 노드·발견 경로·도달 종착점을 보관한다.
GameSession.StartExpedition이 현재 가문의 기록을 연결한다. UI의 검증용 CreateRun 호출만으로는
가문의 기록을 변경하지 않는다. 데이터 없는 테스트 실행은 임시 기록으로 진행한다.

노드 진입 시 그 노드와 나가는 경로의 존재를 발견한다. 목적지의 종류는 실제 방문 전까지
발견되지 않는다. 이미 발견한 곳도 다음 원정에서는 다시 완료해야 한다. UI는
Exploration.KnowsNode로 공개 여부를 판단해야 하며 SO 자체는 전체 설계 정보를 담고 있다.

저장 버전은 2이며 누적 탐험 기록을 저장한다. 버전 1은 빈 탐험 기록으로 읽을 수 있다.
발견 시마다 자동 디스크 저장하지는 않는다. 원정 실행기가 안전한 완료 지점이나 귀환 시
GameSession.Save를 호출해야 한다. 종료 때 EndExpedition은 현재 원정만 해제하고 누적 기록은 유지한다.

검증: `dotnet run --project Tests/Progress/Progress.csproj`,
`dotnet run --project Tests/StageSelection/StageSelection.csproj`.
Unity Play Mode와 콘텐츠 실행은 별도로 확인해야 한다.

## 공용 표시 문자열 치환

`GameTextFormatter.Format(template, token, value)`은 지정한 토큰을 치환한다.
토큰 이름은 중괄호 없이 전달한다: `Format("{FamilyName} 가도", "FamilyName", familyName)`.
여러 토큰은 `Format(template, IReadOnlyDictionary<string, string> tokens)`로 한 번에 치환한다.
치환 값 안의 토큰은 다시 해석하지 않는다. null 템플릿, 알 수 없는 토큰, TMP 태그는
그대로 유지하며, null 치환 값은 빈 문자열로 처리한다. 치환 값의 공백은 보존한다.
토큰 이름이 비어 있거나 중괄호를 포함하면 ArgumentException이 발생한다.

유틸은 세션이나 가문명 규칙을 알지 않는다. StageDefinition.GetDisplayName과 영지 UI가
가문명의 앞뒤 공백 제거 및 `이름 없는 가문` 기본값을 처리한 뒤 유틸을 호출한다.
`{0}` 위치 기반 포맷은 기존 CWFormatStringEx를 사용한다.

## 전투 콘텐츠와 적 그룹

- `EnemyGroup` SO: groupId와 enemies. 한 항목이 적 한 개체이며 같은 프리팹을 반복 지정할 수 있다.
- `BattleContent` SO: enemyGroups 엔트리 목록(그룹 참조 + weight), backgroundPrefab(선택), allowEscape.
- `BattleEncounter`: 이번 전투에서 선택된 그룹 ID, 적 프리팹 목록, 배경, 도주 설정의 스냅샷.

Create > ProjectLegacy > Battle > Enemy Group으로 그룹을 만든다.
적은 BattleUnit이 설정된 프리팹을 참조하며 Side=Enemy, ControlType=AI여야 한다.
프리팹에서 스탯·스킬·AI를 설정한다. enemies 배열 순서가 기존 BattleFormation의 적 슬롯 순서다.
같은 그룹을 여러 전투 콘텐츠에서 재사용할 수 있다. 후보 그룹은 ID가 고유해야 하며
각 엔트리의 weight로 등장 비율을 설정한다. 확률은 weight / 전체 weight 합이다.
예를 들어 6:3:1은 60%:30%:10%다. 가중치는 전투 콘텐츠마다 독립적으로 설정한다.
0 가중치는 추첨에서 제외하며, 음수·NaN·무한대 및 전부 0인 설정은 거절한다.
0 가중치 항목도 그룹 참조와 편성은 유효해야 한다.
양수 가중치 후보가 하나면 확정 편성이고 난수를 소비하지 않는다.
BattleContent 인스펙터 하단에 계산된 확률 또는 설정 오류를 표시한다.
EnemyGroupEntry는 별도 SO가 아닌 BattleContent 내부 직렬화 항목이다.

```csharp
var content = (BattleContent)run.CurrentContent;
var encounter = content.CreateEncounter(random); // System.Random. 전투 시작 시 한 번 호출해 보관
// 다음 단계의 전투 실행기가 EnemyPrefabs를 각각 Instantiate하고 참가자로 등록한다.
// 같은 프리팹이 두 번 있으면 서로 다른 개체 두 개를 생성해야 한다.
// BackgroundPrefab을 배치하고 AllowEscape를 도주 처리에 적용한다.
// 전투 승리 시에만 run.CompleteCurrentNode(enteredNodeId)를 호출한다.
```

설정을 읽을 때마다 CreateEncounter를 다시 호출하면 재추첨되므로 보관한 결과를 사용한다.
적 목록은 복사하지만 프리팹 에셋 자체는 공유 참조다. 에셋을 전투 참가자로 직접 사용하거나
HP·상태 이상을 에셋에 기록하지 않는다. 콘텐츠와 그룹 검증은 출정 및 편성 생성 시 수행한다.
실제 적 생성, 전투 시작 연결, 배경 배치, 도주 실행, 보상 지급은 아직 구현하지 않았다.
보상 시스템과 별도의 일반전/보스전/이벤트전 enum은 이번 구조에 추가하지 않았다.

ExampleAmbush는 BattleContent, ExampleTraveler는 EventContent로 마이그레이션했다.
기존 에셋 GUID는 유지했다. ExampleAmbush는 적 프리팹/EnemyGroup을 연결해야 검증을 통과한다.
테스트는 그룹 선택·단일 그룹 고정·동일 시드 재현·초기 설정 복사·잘못된 편성 거절을 검증한다.
Unity 프리팹 생성과 실제 전투 수명 주기는 별도 Play Mode 검증 대상이다.
