# 적 AI 설정

- 전투 매니저에 `AIActionController`를 추가한다. `BattleActionExecutor`도 필요하다.
- Create > ProjectLegacy > Battle > AI Action Set으로 행동셋 에셋을 만든다.
- Actions에 스킬을 순서대로 추가한다. 반복해서 같은 스킬을 넣어도 된다.
- Condition은 기본 None. Hp Ratio At Or Below + 0.5는 **행동하는 유닛 자신의 HP 50% 이하(경계 포함)**다. 조건은 스킬 종류와 무관하다.
- BattleUnit의 AI Action Sets에 에셋들을 등록하고 Initial AI Action Set Index를 지정한다. Control Type은 AI로 설정한다.
- 기본 공격/방어는 유닛의 해당 슬롯에, 다른 스킬은 Skills 목록에도 등록해야 한다. AI도 공통 실행기의 스킬 소유 검증을 따른다.
- 샘플 씬의 오크에는 AI_Orc_Default(공격, 공격, 방어)가 연결되어 있다.

각 유닛의 AIActionPattern.CurrentActionIndex는 **마지막으로 실행을 시작한 행동 위치**다. 초기값은 -1이며 첫 탐색은 0부터 시작한다. 사용 불가 스킬(MP 부족 포함), 조건 불충족, 유효 대상 없음은 건너뛴다. 최대 한 바퀴만 탐색하며 모두 불가능하면 인덱스를 유지하고 턴을 소비한다. 기본 공격으로 별도 대체하지 않는다.

단일 적 대상은 유효 대상 중 무작위, 단일 아군 대상은 HP 비율이 가장 낮은 아군(자신 포함), 전체 대상은 유효 대상 전원이다. Self는 자신이다.

페이즈 전환 시 `unit.SetAIActionSet(index)`를 호출한다. 선택한 행동셋의 처음부터 다시 시작하며 자동 페이즈 판정은 하지 않는다. 실행 중 교체하면 다음 행동부터 새 패턴을 사용한다.

상태이상 확장: BattleStatus.BlocksAllActions를 재정의하면 전체 행동을 막고 커서를 유지한다. AllowsSkill(actor, skill)을 재정의하면 특정 스킬을 제한하며 다음 가능한 항목을 찾는다. 공격 판별/상태 부여/지속시간 정책 자체는 후속 상태이상 구현 범위다. CanAct는 기존 생존/타임라인/대상 판정 의미를 유지하며, 실제 행동 제한은 CanPerformActions와 CanUseSkill로 판정한다.

검증: `dotnet run --project Tests/EnemyAI/EnemyAI.csproj`는 실제 순환/조건 소스를 최소 데이터 스텁으로 실행한다. Unity 코루틴·애니메이션 통합은 Play Mode에서 확인한다.
