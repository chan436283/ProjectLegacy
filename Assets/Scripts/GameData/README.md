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
입력 데이터의 임시 보정치는 복사하지 않는다. 포인트 예산·능력치 상한 등
캐릭터 생성 규칙은 이후 생성 단계에서 검증한다.

전투 진입 시 기존 캐릭터에 `Initialize`를 다시 호출하면 HP/MP가 회복되므로
진입마다 호출하지 않는다. 전투 오브젝트 연결과 임시 전투 효과 정리는 후속 작업이다.

이 모델은 Unity 직렬화가 가능한 데이터 구조다. 파일 저장/불러오기,
씬 간 수명 관리, 생성 UI, 탐험 상태와 전투 연결은 아직 구현하지 않는다.

검증: `dotnet run --project Tests/Progress/Progress.csproj`
실제 데이터·능력치 소스를 최소 Unity 스텁으로 실행한다.
Unity 직렬화와 씬 연결은 이 검증에 포함되지 않는다.
