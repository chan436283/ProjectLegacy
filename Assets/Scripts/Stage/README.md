# 지도에서 원정 시작

현재 흐름: 마을 원정 버튼 → 같은 씬의 지도 패널 → 지점 아이콘 선택 → 중앙 상단 이름 확인 → 출정 버튼 → 해당 진입 씬.
원정 버튼과 지도 닫기는 씬 로드나 저장을 하지 않는다. 출정 버튼을 누르면 진행 데이터를
저장하고, 스테이지의 `entryScene`을 로드한다. 현재 예제는 기존 `GameScene` 전투
프로토타입으로 진입한다. 원정 씬에는 ExpeditionController를 연결해 맵 이동·전투·기본 이벤트·귀환을 진행한다.

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

## 맵 프리팹 정의와 진행 API

StageDefinition은 stageId, displayName, entryScene, mapPrefab을 보관한다.
별도의 StageNode/StageRoute와 nodes/startNodeId 설정은 제거했다.
맵 프리팹의 ExpeditionMap이 Start Point를 지정하고 자식 ExpeditionMapPoint를 자동 수집한다.
각 포인트가 Node Id, 선택적 Display Name, Content, Is Ending/Ending Id, Connections를 보관한다.
Connections에는 같은 맵 안의 포인트인 Target만 지정한다. 경로 ID는 출발/목적지 ID로 자동 생성한다.
콘텐츠는 BattleContent/EventContent 에셋으로 연결하며, 비어 있으면 이동 전용 포인트다.
시작점이나 종료점에도 콘텐츠를 연결할 수 있다. 보스전 종료점은 Is Ending과 BattleContent를 함께 설정한다.

설정은 맵 프리팹 한 곳에서 작성한다. BuildMapData는 이를 검증된 실행용 ExpeditionMapData로 복사한다.
중복/빈 ID, 외부 목적지, 순환, 미도달 포인트, 종료점 외 막다른 길, 잘못된 콘텐츠를 거절한다.
시작점 하나와 종료점 한 개 이상이 필요하며 합류를 허용한다. 기존과 같이 순환은 지원하지 않는다.
이름은 선택 사항이다. 콘텐츠가 없는 일반 포인트도 허용한다.

StageDefinition의 Validate Stage 메뉴와 출정 시 맵 구성을 검증한다.
기존 SO 에셋의 노드 설정은 자동 이전하지 않는다. Unity에서 맵 프리팹에 연결·콘텐츠를 옮기고
StageDefinition의 Map Prefab을 연결해야 한다. 노드/종료 ID를 유지하면 해당 발견 기록이 연결된다. 이전 수동 경로 ID는 자동 이전하지 않으며 출발점 재방문 시 새 ID로 기록된다.
자세한 설정은 `Assets/Scripts/Expedition/README.md`를 참고한다.

```csharp
// TownController: 메타데이터 생성 및 누적 기록 연결. 아직 시작 노드에 진입하지 않는다.
var run = stage.CreateRun(GameSession.Current?.FamilyName);
GameSession.StartExpedition(run);
// GameScene 진행 컨트롤러: 실제 맵 인스턴스를 만든 뒤 원정을 초기화한다.
var map = Instantiate(run.MapPrefab, mapRoot);
map.RouteRequested += OnRouteRequested;
map.Show(run);
// 콘텐츠 실행기가 전투 승리/이벤트 완료 후 호출한다.
// run.CompleteCurrentNode(enteredNodeId);
```

`CreateRun` 직후 Phase는 Uninitialized이고 AvailableRoutes는 비어 있다.
첫 Show가 맵 데이터를 복사하고 시작점에 진입한다. 같은 원정을 다시 Show해도 진행을 초기화하지 않는다.
ChooseRoute는 현재 콘텐츠 처리가 끝났고 현재 지점에 연결된 경로일 때만 성공한다.
콘텐츠가 없는 포인트는 자동 완료한다. 콘텐츠가 있으면 ResolvingNode에서 외부 실행기의 완료를 기다린다.
완료 콜백의 nodeId가 현재 노드와 다르거나 이미 완료되었으면 거절한다.
종료점 콘텐츠가 완료되면 Phase=Completed, EndingId와 발견 기록을 설정하고 이동을 막는다.
전투 패배/취소 시 CompleteCurrentNode를 호출하지 않는다.
연결/이름/종류는 초기화 시 복사하고 콘텐츠 SO 참조는 공유한다.

절차적 생성기는 포인트의 Configure와 맵의 SetStartPoint로 설정한 뒤 Show할 수 있다.
화면 없이 생성한 ExpeditionMapData를 run.Initialize(data)에 전달하는 방식도 지원한다.
콘텐츠 실행·화면 전환·귀환은 ExpeditionController가 담당한다. 절차적 맵 생성 알고리즘은 아직 없다.

## 발견 기록과 저장

이번 원정의 CurrentNodeId, VisitedNodes, TraversedRoutes, CompletedNodes, Phase는
`ExpeditionRun`에만 유지하며 원정 중 이어하기 저장은 아직 지원하지 않는다.
`GameData.GetExploration(stageId)`에는 누적 발견 노드·발견 경로·도달 종착점을 보관한다.
GameSession.StartExpedition이 현재 가문의 기록을 연결한다. UI의 검증용 CreateRun 호출만으로는
가문의 기록을 변경하지 않는다. 데이터 없는 테스트 실행은 임시 기록으로 진행한다.

노드 진입 시 그 노드와 나가는 경로의 존재를 발견한다. 목적지의 종류는 실제 방문 전까지
발견되지 않는다. 이미 발견한 곳도 다음 원정에서는 다시 완료해야 한다. UI는
Exploration.KnowsNode로 공개 여부를 판단하며 맵 프리팹이 전체 설계 정보를 담고 있다.

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

- `EnemyGroup` SO: groupId와 placements. 한 항목이 적 한 개체이며 prefab, row(Front/Back), position(1부터)을 지정한다. 같은 프리팹을 서로 다른 자리에 반복 지정할 수 있다.
- `BattleContent` SO: enemyGroups 엔트리 목록(그룹 참조 + weight), backgroundPrefab(선택), allowEscape.
- `BattleEncounter`: 이번 전투에서 선택된 그룹 ID, Enemies(프리팹·열·자리 번호), 배경, 도주 설정의 스냅샷.

Create > ProjectLegacy > Battle > Enemy Group으로 그룹을 만든다.
적은 BattleUnit이 설정된 프리팹을 참조하며 Side=Enemy, ControlType=AI여야 한다.
프리팹에서 스탯·스킬·AI를 설정한다. 그룹의 placements에서 개체별 자리를 지정하며 배열 순서는 배치에 영향을 주지 않는다.
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
// 아군을 미리 등록하고 BattleController의 Start Automatically를 꺼둔다.
// 기존 적 참가자는 없어야 하며, 활성 BattleFormation에 필요한 슬롯을 연결한다.
battleController.StartBattle(encounter); // 적 생성 → 열/자리 전달 → 배치 → 전투 시작
// BackgroundPrefab을 배치하고 AllowEscape를 도주 처리에 적용한다.
// 전투 승리 시에만 run.CompleteCurrentNode(enteredNodeId)를 호출한다.
```

설정을 읽을 때마다 CreateEncounter를 다시 호출하면 재추첨되므로 보관한 결과를 사용한다.
적 프리팹 참조·열·자리 번호는 읽기 전용 값으로 복사하지만 프리팹 에셋 자체는 공유 참조다. 에셋을 전투 참가자로 직접 사용하거나
HP·상태 이상을 에셋에 기록하지 않는다. 콘텐츠와 그룹 검증은 출정 및 편성 생성 시 수행한다.
StartBattle(encounter)는 적을 생성하고 초기 자리를 설정한 뒤 전투를 시작한다. 배치 검증 실패 시 생성한 적을 정리하며 기존 참가자는 이동하지 않는다.
ExpeditionController가 전투 시작·배경 배치·승패 후 맵 복귀를 연결한다. 도주 실행과 보상 지급은 아직 구현하지 않았다.
보상 시스템과 별도의 일반전/보스전/이벤트전 enum은 이번 구조에 추가하지 않았다.

ExampleAmbush는 BattleContent, ExampleTraveler는 EventContent로 마이그레이션했다.
기존 에셋 GUID는 유지했다. ExampleAmbush는 적 프리팹/EnemyGroup을 연결해야 검증을 통과한다.
테스트는 그룹 선택·단일 그룹 고정·동일 시드 재현·초기 설정 복사·잘못된 편성 거절을 검증한다.
Unity 프리팹 생성과 실제 전투 수명 주기는 별도 Play Mode 검증 대상이다.


## 적 전열·후열 자리 연결

1. `BattleFormation`의 Enemy Front Slots / Enemy Back Slots에 각 열의 위치 Transform을 연결한다.
   각 목록은 화면 위에서 아래 순서이며 Element 0이 1번 자리, Element 1이 2번 자리다.
   열마다 목록 길이가 해당 열의 자리 수다. 사용하려는 자리에 Transform이 없으면 오류로 처리한다.
2. `EnemyGroup`의 Placements에 Prefab, Row, Position을 설정한다.
   예: 고블린 Front 1, 고블린 Front 3, 궁수 Back 2. Front 2와 Back 1은 빈자리로 유지된다.
   같은 열·자리 중복, 0 이하 번호, 잘못된 열, 누락/잘못된 프리팹은 그룹 검증에서 거절한다.
   씬의 슬롯 범위·Transform 누락/중복은 BattleFormation이 실제 배치 전에 검증한다.
3. 현재처럼 씬에 적을 직접 놓고 실행할 때는 각 적 인스턴스의 `BattleUnit`에서
   Row와 Formation Position을 지정하고 기존 Participants에 등록한다.
   이 경로에서는 프리팹의 기본 자리(Front 1)가 겹치지 않도록 인스턴스마다 설정해야 한다.
4. 그룹에서 생성하는 경로는 `StartBattle(encounter)`를 호출한다. 그룹의 자리가 프리팹의 기본 자리를 덮어쓴다.
   논리적인 자리는 `BattleUnit.Row`, `FormationPosition`으로 조회한다.
   `SetFormationPosition`은 논리 값만 변경하며 실제 화면 배치는 `BattleFormation.PlaceParticipants`가 수행한다.

아군은 기존 Ally Slots의 참가자 순서 배치를 유지한다. 아군 파티 편집은 아직 연결하지 않는다.
전열이 비거나 사망해도 후열을 앞으로 당기지 않으며, 캐릭터 직업·스킬에 따른 배치 제한도 없다.
이동/공격 애니메이션의 좌표는 논리적인 열·자리 번호를 바꾸지 않는다.
후열 공격 제한이나 열에 따른 능력치 보정은 이번 배치 기능에 포함하지 않는다.

이전 EnemyGroup의 enemies 배열은 역직렬화 시 Front 1, Front 2…로 옮겨 기존 프리팹 참조를 보존한다.
기존 BattleFormation의 Enemy Slots 참조는 Enemy Front Slots로 유지된다.
기존 슬롯의 시각적 위치가 전열·후열로 섞여 있었다면 Unity에서 두 목록을 의도대로 다시 연결해야 한다.
씬·프리팹·SO 에셋 자체는 이번 코드 변경으로 수정하지 않았다.

검증: `dotnet run --project Tests/Formation/Formation.csproj`는 실제 배치·유닛·전투 시작 코드를
Unity 대역으로 실행해 고정 자리, 빈자리, 중복/누락 거절, 실패 시 정리와 재시도를 확인한다.
`Tests/Progress`는 편성 스냅샷 및 이전 목록 변환을 포함한다.
실제 Unity 역직렬화, Instantiate 수명 주기와 화면 배치는 Play Mode에서 별도로 확인한다.
