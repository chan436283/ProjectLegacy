using System;
using CWFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ExpeditionState { Idle, Map, Battle, Event, Completed, Defeated, Faulted, Returning }

/// <summary>이벤트 실행 한 건의 토큰. 늦게 도착한 이전 이벤트 완료 요청을 구분합니다.</summary>
public sealed class ExpeditionEventRequest
{
    public string NodeId { get; }
    public EventContent Content { get; }
    internal ExpeditionEventRequest(string nodeId, EventContent content)
    { NodeId = nodeId; Content = content; }
}

/// <summary>GameScene의 원정 진행 담당. 화면 루트 밖의 항상 활성인 오브젝트에 배치합니다.</summary>
[DisallowMultipleComponent]
public sealed class ExpeditionController : CBehaviour
{
    [SerializeField] private bool startAutomatically = true;
    [SerializeField] private GameObject mapRoot;
    [SerializeField] private GameObject battleRoot;
    [SerializeField] private BattleController battleController;
    [SerializeField] private ExpeditionMapCameraController mapCamera;
    [SerializeField] private ExpeditionEventPanel eventPanel;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private Transform backgroundParent;
    [SerializeField] private GameObject defaultBattleBackground;
    [SerializeField] private Transform partyMarker;
    [SerializeField] private string returnScene = "TownScene";

    public ExpeditionState State { get; private set; }
    public ExpeditionRun Run { get; private set; }
    public ExpeditionMap Map { get; private set; }
    public BattleEncounter CurrentEncounter { get; private set; }
    public ExpeditionEventRequest CurrentEvent { get; private set; }
    public string LastError { get; private set; }
    public event Action<string> PointEntered;
    public event Action<ExpeditionEventRequest> EventRequested;
    public event Action<string> ExpeditionCompleted;
    public event Action ExpeditionDefeated;
    public event Action<string> ErrorOccurred;

    private readonly System.Random random = new();
    private GameObject battleBackground;
    private string battleNodeId;
    private BattleState? pendingBattleResult;
    private bool ownsMap;
    private bool subscribed;

    protected override void OnAwake()
    {
        // BattleController.Start보다 먼저 자동 시작을 차단합니다.
        if (battleController != null) battleController.SetStartAutomatically(false);
    }

    protected override void OnStarted()
    {
        if (startAutomatically && Run == null) Begin(GameSession.CurrentExpedition);
    }

    /// <summary>기본은 원정의 MapPrefab을 생성합니다. 생성형 맵은 준비된 인스턴스를 전달할 수 있습니다.</summary>
    public void Begin(ExpeditionRun run, ExpeditionMap mapInstance = null)
    {
        if (Run != null || State != ExpeditionState.Idle)
            throw new InvalidOperationException("이미 원정을 시작했습니다.");
        try
        {
            ValidateSetup();
            if (run == null) throw new InvalidOperationException("진행할 원정이 없습니다. 스테이지 선택에서 진입하세요.");
            if (mapInstance == null && run.MapPrefab == null)
                throw new InvalidOperationException("원정 맵 프리팹이 없습니다.");
            if (mapInstance != null && !mapInstance.transform.IsChildOf(mapRoot.transform))
                throw new InvalidOperationException("생성된 맵은 Map Root 아래에 배치해야 합니다.");
            battleController.SetStartAutomatically(false);
            if (battleController.State != BattleState.Idle)
                throw new InvalidOperationException("원정 시작 시 전투 컨트롤러는 Idle이어야 합니다.");
            battleRoot.SetActive(false);
            mapRoot.SetActive(true);
            eventPanel?.Hide();
            Run = run;
            ownsMap = mapInstance == null;
            Map = mapInstance != null ? mapInstance : Instantiate(run.MapPrefab, mapRoot.transform);
            Map.RouteRequested += Move;
            battleController.BattleEnded += OnBattleEnded;
            subscribed = true;
            mapCamera?.SetMapBackground(Map.Background);
            Map.SetBusy(true);
            Map.Show(Run);
            Map.SetBusy(true);
            EnterCurrentPoint();
        }
        catch (Exception exception) { Fail(exception); }
    }

    private void ValidateSetup()
    {
        if (mapRoot == null || battleRoot == null || battleController == null ||
            !battleController.enabled || mapRoot == battleRoot ||
            transform.IsChildOf(mapRoot.transform) || transform.IsChildOf(battleRoot.transform) ||
            mapRoot.transform.IsChildOf(battleRoot.transform) || battleRoot.transform.IsChildOf(mapRoot.transform) ||
            !battleController.transform.IsChildOf(battleRoot.transform))
            throw new InvalidOperationException("독립된 Map/Battle Root와 그 밖의 ExpeditionController, Battle Root 내부의 BattleController를 연결하세요.");
        if (eventPanel != null && (eventPanel.transform.IsChildOf(battleRoot.transform) ||
            transform.IsChildOf(eventPanel.transform) || mapRoot.transform.IsChildOf(eventPanel.transform)))
            throw new InvalidOperationException("이벤트 패널은 전투 루트 밖에 별도로 배치하세요.");
        if ((enemyParent != null && !enemyParent.IsChildOf(battleRoot.transform)) ||
            (backgroundParent != null && !backgroundParent.IsChildOf(battleRoot.transform)))
            throw new InvalidOperationException("전투 적/배경 생성 위치는 Battle Root 내부여야 합니다.");
    }

    private void Move(string routeId)
    {
        if (State != ExpeditionState.Map || !isActiveAndEnabled) return;
        try
        {
            Map.SetBusy(true);
            mapCamera?.SetInputEnabled(false);
            Run.ChooseRoute(routeId);
            EnterCurrentPoint();
        }
        catch (Exception exception) { Fail(exception); }
    }

    private void EnterCurrentPoint(bool notifyEntry = true)
    {
        if (partyMarker != null && Map.TryGetPoint(Run.CurrentNodeId, out var point))
        {
            var position = point.transform.position;
            position.z = partyMarker.position.z;
            partyMarker.position = position;
        }
        if (notifyEntry) PointEntered?.Invoke(Run.CurrentNodeId);
        SaveProgress();
        if (Run.Phase == ExpeditionRun.RunPhase.Completed)
        {
            ShowMap(false);
            State = ExpeditionState.Completed;
            ExpeditionCompleted?.Invoke(Run.EndingId);
            return;
        }
        if (Run.Phase == ExpeditionRun.RunPhase.ChoosingRoute)
        {
            ShowMap(true);
            return;
        }
        switch (Run.CurrentContent)
        {
            case BattleContent battle: BeginBattle(battle); break;
            case EventContent content: BeginEvent(content); break;
            default: throw new InvalidOperationException("실행할 수 없는 원정 콘텐츠입니다.");
        }
    }

    private void ShowMap(bool allowMovement)
    {
        battleRoot.SetActive(false);
        mapRoot.SetActive(true);
        Map.Show(Run);
        Map.SetBusy(!allowMovement);
        mapCamera?.SetInputEnabled(allowMovement && isActiveAndEnabled);
        if (allowMovement) State = ExpeditionState.Map;
    }

    private void BeginBattle(BattleContent content)
    {
        CurrentEncounter = content.CreateEncounter(random);
        battleNodeId = Run.CurrentNodeId;
        State = ExpeditionState.Battle;
        mapCamera?.SetInputEnabled(false);
        Map.Hide();
        mapRoot.SetActive(false);
        ClearBackground();
        if (CurrentEncounter.BackgroundPrefab != null)
            battleBackground = Instantiate(CurrentEncounter.BackgroundPrefab, backgroundParent != null ? backgroundParent : battleRoot.transform);
        if (defaultBattleBackground != null) defaultBattleBackground.SetActive(battleBackground == null);
        battleRoot.SetActive(true);
        battleController.StartBattle(CurrentEncounter, enemyParent != null ? enemyParent : battleRoot.transform);
    }

    private void OnBattleEnded(BattleState result)
    {
        if (State == ExpeditionState.Battle && pendingBattleResult == null &&
            (result == BattleState.Victory || result == BattleState.Defeat)) pendingBattleResult = result;
    }

    private void Update()
    {
        // BattleEnded의 이벤트 전달이 끝난 뒤 Update에서 전투 오브젝트를 정리합니다.
        if (!pendingBattleResult.HasValue) return;
        var result = pendingBattleResult.Value;
        pendingBattleResult = null;
        try
        {
            battleRoot.SetActive(false);
            battleController.ResetBattle();
            ClearBackground();
            CurrentEncounter = null;
            if (result == BattleState.Defeat)
            {
                State = ExpeditionState.Defeated;
                SaveProgress();
                ExpeditionDefeated?.Invoke();
                return;
            }
            Run.CompleteCurrentNode(battleNodeId);
            EnterCurrentPoint(false);
        }
        catch (Exception exception) { Fail(exception); }
    }

    private void BeginEvent(EventContent content)
    {
        State = ExpeditionState.Event;
        Map.Refresh();
        Map.SetBusy(true);
        mapCamera?.SetInputEnabled(false);
        var request = new ExpeditionEventRequest(Run.CurrentNodeId, content);
        CurrentEvent = request;
        if (eventPanel != null) eventPanel.Show(content, () => CompleteEvent(request));
        else if (EventRequested == null)
            throw new InvalidOperationException("이벤트 패널 또는 EventRequested 처리기를 연결하세요.");
        EventRequested?.Invoke(request);
    }

    public void CompleteEvent(ExpeditionEventRequest request)
    {
        if (!isActiveAndEnabled || State != ExpeditionState.Event || request == null ||
            !ReferenceEquals(request, CurrentEvent)) return;
        try
        {
            CurrentEvent = null;
            eventPanel?.Hide();
            Run.CompleteCurrentNode(request.NodeId);
            EnterCurrentPoint(false);
        }
        catch (Exception exception) { Fail(exception); }
    }

    /// <summary>결과 UI의 귀환 버튼에서 호출합니다. 씬 로드가 수락되면 현재 원정만 해제합니다.</summary>
    public void ReturnToTown()
    {
        if (!isActiveAndEnabled || (State != ExpeditionState.Completed && State != ExpeditionState.Defeated &&
            State != ExpeditionState.Faulted)) return;
        var previous = State;
        try
        {
            if (string.IsNullOrWhiteSpace(returnScene) || !Application.CanStreamedLevelBeLoaded(returnScene))
                throw new InvalidOperationException("귀환 씬이 빌드 목록에 없습니다.");
            if (GameSession.Current != null) GameSession.Save();
            State = ExpeditionState.Returning;
            if (SceneManager.LoadSceneAsync(returnScene) == null)
                throw new InvalidOperationException("귀환 씬을 불러오지 못했습니다.");
            if (GameSession.CurrentExpedition == Run) GameSession.EndExpedition();
        }
        catch (Exception exception)
        {
            State = previous;
            ReportError(exception);
        }
    }

    private void SaveProgress()
    {
        if (GameSession.Current == null || GameSession.CurrentExpedition != Run) return;
        try { GameSession.Save(); }
        catch (Exception exception) { ReportError(exception); }
    }

    private void ClearBackground()
    {
        if (battleBackground == null) return;
        battleBackground.SetActive(false);
        Destroy(battleBackground);
        battleBackground = null;
    }

    private void Fail(Exception exception)
    {
        State = ExpeditionState.Faulted;
        pendingBattleResult = null;
        CurrentEvent = null;
        if (Map != null) Map.SetBusy(true);
        mapCamera?.SetInputEnabled(false);
        eventPanel?.Hide();
        if (battleRoot != null) battleRoot.SetActive(false);
        ReportError(exception);
    }

    private void ReportError(Exception exception)
    {
        LastError = exception.Message;
        Debug.LogException(exception, this);
        ErrorOccurred?.Invoke(LastError);
    }

    protected override void OnDisabled()
    {
        if (Map != null) Map.SetBusy(true);
        if (mapCamera != null) mapCamera.SetInputEnabled(false);
        if (State == ExpeditionState.Event && eventPanel != null) eventPanel.Hide();
    }

    protected override void OnEnabled()
    {
        if (State == ExpeditionState.Event && CurrentEvent != null && eventPanel != null)
        {
            var request = CurrentEvent;
            eventPanel.Show(request.Content, () => CompleteEvent(request));
        }
        if (State != ExpeditionState.Map || Map == null) return;
        Map.SetBusy(false);
        mapCamera?.SetInputEnabled(true);
    }

    protected override void OnReleased()
    {
        if (subscribed)
        {
            if (Map != null)
            {
                Map.RouteRequested -= Move;
                Map.SetBusy(true);
            }
            if (battleController != null) battleController.BattleEnded -= OnBattleEnded;
        }
        if (eventPanel != null) eventPanel.Hide();
        if (mapCamera != null)
        {
            mapCamera.SetInputEnabled(false);
            mapCamera.SetMapBackground(null);
        }
        ClearBackground();
        if (ownsMap && Map != null) Destroy(Map.gameObject);
    }
}
