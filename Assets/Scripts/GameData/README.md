# 게임 진행 데이터

`GameData`는 한 게임의 가문명, 주인공, 보유 동료를 보관한다.
`CharacterData`는 고유 ID, 이름, 기존 `CharacterStats`를 보관한다.
주인공은 동료 목록과 별도로 존재하며 동료 제거 API로 제거할 수 없다.
보유 동료 목록은 출전 편성이 아니므로 전투 인원 제한을 적용하지 않는다.

```csharp
var abilities = new PrimaryStats();
abilities.Strength.SetBaseValue(15f);
var progress = new GameData("아르덴", "레온", abilities, formulaConfig);
progress.AddCompanion(new CharacterData("미라", new PrimaryStats(), formulaConfig));
```

생성 시 기본 능력치를 복사하고 전투 능력치를 계산한 뒤 HP/MP를 채운다.
입력 데이터의 임시 보정치는 복사하지 않는다. 생성 중 배분 규칙은
`PrimaryStatAllocation`과 `StatAllocationUI`에서 검증한다.

전투 진입 시 기존 캐릭터에 `Initialize`를 다시 호출하면 HP/MP가 회복되므로
진입마다 호출하지 않는다. 전투 오브젝트 연결과 임시 전투 효과 정리는 후속 작업이다.

## 초기 설정 완료와 저장

`TitleFlowController`는 포인트를 모두 배분한 뒤 확인을 누르면 이름과 능력치로
`GameData`를 생성한다. `GameSession.StartNewGame(data)`가 파일 저장에 성공한 뒤
`GameSession.Current`를 교체하며, 패널 퇴장 후 `EstateScene`으로 이동한다.

저장 경로는 `Application.persistentDataPath/game-save.json`이다. 단일 슬롯이며
새 게임을 확정하면 기존 슬롯을 교체하고 이전 파일을 `.bak`으로 남긴다.
임시 파일 쓰기를 마친 뒤 파일을 교체한다. 저장 실패 시 씬을 이동하지 않는다.

`GameSaveData`는 버전, 가문명, 주인공과 동료의 ID·이름·레벨·기본 능력치·현재
HP/MP를 저장한다. 로드 시 전투 능력치를 계산 설정으로 재계산하고 HP/MP를
현재 최대치 안으로 제한한다. 임시 전투 효과는 저장하지 않는다.
`PrimaryStatType`의 순서를 변경하면 저장 버전과 마이그레이션도 함께 검토해야 한다.

`GameSession.Current`는 씬과 독립적인 메모리 데이터다. 이후 변경 사항의 저장은
`GameSession.Save()`로 수행한다. `GameSession.TryLoad(formulaConfig)`는 파일이
없으면 false를 반환하고, 잘못된 파일은 예외를 발생시킨다. 백업 자동 복구는 하지 않는다.

영지 씬은 현재 세션을 표시하며, 씬을 직접 실행한 경우 저장 파일을 불러온다.
Continue 버튼, 영지 운영, 탐험 상태와 전투 오브젝트 연결은 후속 작업이다.

## 씬 구성

- `TitleScene`: 새 게임 생성, 추후 이어하기와 설정 메뉴.
- `EstateScene`: 영지 본진. 파티·건물·출정 준비 등은 패널로 확장.
- 향후 `ExpeditionScene`: 원정 중 분기 경로와 이벤트.
- 현재 `GameScene`: 전투 프로토타입. 탐험과 연결할 때 `BattleScene` 역할로 정리.

영지 진행과 인물은 `GameData`에, 원정 중 위치·임시 강화·획득 보상은 추후
별도 원정 데이터에 보관한다. 씬 오브젝트 자체를 저장 데이터로 사용하지 않는다.

검증: `dotnet run --project Tests/Progress/Progress.csproj`
실제 데이터·능력치·저장 변환·파일 IO 소스를 최소 Unity 스텁으로 실행한다.
JSON 전송은 테스트에서 System.Text.Json으로 대체한다. 실제 Unity JSON 직렬화와
Play Mode의 씬 전환은 이 검증에 포함되지 않는다.

## 시작 동료 프리셋

Project 창의 `Create > ProjectLegacy > Character > Companion Preset`으로 직종별
에셋을 만든다. `presetId`는 풀 안에서 고유해야 하고 `displayName`은 직종 설명이다.
인물 이름은 프리셋에 포함하지 않는다. 생성기의 `characterNames` 인자로 시작 동료 수만큼
유효한 이름을 외부에서 전달한다. 추후 성별 선택 후 성별별 공통 이름 풀에서 작명한다.
현재 타이틀에서는 `동료 1`부터 순서대로 임시 이름을 전달한다.
`statRanges`에는 기본 능력치 8종을 한 번씩 지정한다. 최소·최대는 양 끝을 포함한다.
기존 `weight`는 제거했다. 직업별 성향은 능력치별 범위로 표현한다.

Config는 총합 범위(기본 40~100), 시작 인원, 프리셋 풀을 갖는다.
동료마다 독립적으로 프리셋을 추첨하며, 같은 프리셋의 반복 사용은 항상 허용한다.
프리셋 하나로 여러 명을 만들 수 있다. 풀 자체의 중복 ID 등록은 허용하지 않는다.
현재 설정 에셋은 `Random.asset` 하나로 4명을 생성한다. 기존 직업 프리셋도 유지한다.

프리셋의 `totalStatBonus`는 기본 총합 추첨 후 더하는 추가 포인트다. Config가
40~100이고 보너스가 10이면 최종 총합은 50~110이다. 보너스는 0 이상이어야 한다.
현재 무작위 프리셋 보너스는 예시값 10이며 Inspector에서 조절할 수 있다.
프리셋 최소 합 ≤ Config 최소 + 보너스, 프리셋 최대 합 ≥ Config 최대 + 보너스를
검증한다. 최종 총합은 Config 최대를 넘을 수 있지만 개별 스탯의 범위는 지킨다.
계산 자원 보호를 위해 보너스를 포함한 최종 총합은 10000까지 지원한다.

총합을 먼저 추첨한 다음, 그 총합과 개별 범위를 만족하는 모든 정수 조합을 같은
확률로 선택한다. 서로 다른 총합을 포함한 전체 조합이 등확률이라는 의미는 아니다.
`UniformStatAllocation`은 동적 계획법으로 남은 조합 수를 계산하고 BigInteger 순위를
균등 추첨한다. 부동소수점 근사나 포인트별 추첨을 사용하지 않는다. 스탯 순서 때문에
앞쪽 능력치가 유리해지는 현상도 없다. 극단적 분배를 강제하지 않으며, 결과 빈도는
실제로 가능한 조합 수에 따라 결정된다.

생성한 모든 동료를 추가한 뒤 한 번에 저장한다. 클래스·칭호·출신 프리셋 ID는
부여하지 않는다. 실제 이름·능력치가 저장되므로 프리셋 변경은 기존 인물에 영향이 없다.
동일 시드로 생성 과정을 재현할 수 있으나 캐릭터 ID는 매번 새로 생성된다.
프리셋과 전역 Unity 난수 상태는 변경하지 않는다. 알고리즘 변경 전 시드와는 결과가 다르다.

TitleScene의 `TitleFlowController.companionGenerationConfig`에 설정 에셋을 연결했다.
잘못된 설정은 저장 전에 오류를 표시한다. 기존 세이브 불러오기는 설정 없이 가능하다.
