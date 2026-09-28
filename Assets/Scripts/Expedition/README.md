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
