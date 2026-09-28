using System;
using CWFramework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>마을 메뉴와 원정 출발을 관리합니다.</summary>
public sealed class TownController : CBehaviour
{
    [SerializeField] private UIPanel mainMenuPanel;
    [SerializeField] private Button expeditionButton;
    [SerializeField] private Button familyMembersButton;
    [SerializeField] private FamilyMembersPanel familyMembersPanel;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private BattleStatFormulaConfig formulaConfig;
    [SerializeField] private StageSelectionPanel stageSelectionPanel;

    private bool isReady;
    private bool isTransitioning;

    protected override void OnAwake()
    {
        isReady = mainMenuPanel != null && expeditionButton != null && statusText != null && formulaConfig != null;
        if (!isReady)
        {
            Debug.LogError("마을 UI 참조와 능력치 계산 설정을 연결해야 합니다.", this);
            enabled = false;
            return;
        }
        mainMenuPanel.SetVisibleImmediate(false);
    }

    protected override void OnEnabled()
    {
        if (!isReady) return;
        expeditionButton.onClick.AddListener(BeginExpedition);
        if (familyMembersButton != null) familyMembersButton.onClick.AddListener(OpenFamilyMembers);
        if (familyMembersPanel != null) familyMembersPanel.Closed += OnFamilyMembersClosed;
        if (stageSelectionPanel != null)
        {
            stageSelectionPanel.Closed += OnStageSelectionClosed;
            stageSelectionPanel.DepartureRequested += StartStage;
        }
        isTransitioning = true;
        statusText.text = string.Empty;
        expeditionButton.interactable = false;
        try
        {
            // 타이틀을 거치지 않고 실행한 경우에도 UI를 열기 전에 저장 데이터를 복원합니다.
            // 이미 진행 중인 세션이 있다면 디스크의 이전 상태로 덮어쓰지 않습니다.
            if (GameSession.Current == null)
            {
                bool loaded = GameSession.TryLoad(formulaConfig);
#if UNITY_EDITOR
                Debug.Log(loaded
                    ? $"타운 초기화: 저장 데이터를 불러왔습니다.\n{GameSession.SavePath}"
                    : $"타운 초기화: 저장 파일이 없어 빈 데이터로 테스트합니다.\n{GameSession.SavePath}", this);
#endif
            }
            // 저장이 없는 에디터 직접 실행도 현재 전투 테스트 씬으로 이동할 수 있습니다.
            expeditionButton.interactable = true;
        }
        catch (Exception exception)
        {
            statusText.text = "저장 데이터를 불러오지 못했습니다.";
            Debug.LogException(exception, this);
        }

        mainMenuPanel.Show(() =>
        {
            isTransitioning = false;
            if (expeditionButton.interactable) expeditionButton.Select();
        });
    }

    protected override void OnDisabled()
    {
        if (expeditionButton != null) expeditionButton.onClick.RemoveListener(BeginExpedition);
        if (familyMembersButton != null) familyMembersButton.onClick.RemoveListener(OpenFamilyMembers);
        if (familyMembersPanel != null) familyMembersPanel.Closed -= OnFamilyMembersClosed;
        if (stageSelectionPanel != null)
        {
            stageSelectionPanel.Closed -= OnStageSelectionClosed;
            stageSelectionPanel.DepartureRequested -= StartStage;
            stageSelectionPanel.HideImmediate();
        }
        if (mainMenuPanel != null) mainMenuPanel.SetVisibleImmediate(false);
        isTransitioning = false;
    }

    public void BeginExpedition()
    {
        if (!isReady || !isActiveAndEnabled || isTransitioning ||
            !expeditionButton.IsInteractable() || (familyMembersPanel != null && familyMembersPanel.IsOpen)) return;

        if (stageSelectionPanel == null || !stageSelectionPanel.Open(GameSession.Current?.FamilyName))
        {
            statusText.text = "원정 지도 설정을 확인해주세요.";
            return;
        }
        statusText.text = string.Empty;
        SetMenuInput(false);
    }

    private void OnStageSelectionClosed()
    {
        if (!isActiveAndEnabled || isTransitioning) return;
        SetMenuInput(true);
        expeditionButton.Select();
    }

    private void SetMenuInput(bool enabled)
    {
        mainMenuPanel.GetComponent<CanvasGroup>().interactable = enabled;
        if (familyMembersButton != null) familyMembersButton.interactable = enabled;
    }

    private void StartStage(ExpeditionStage stage)
    {
        if (!isReady || !isActiveAndEnabled || isTransitioning ||
            stageSelectionPanel == null || !stageSelectionPanel.IsOpen) return;
        try
        {
            if (stage == null) throw new InvalidOperationException("원정 스테이지가 없습니다.");
            var run = stage.CreateRun(GameSession.Current?.FamilyName);
            if (!Application.CanStreamedLevelBeLoaded(run.EntryScene))
                throw new InvalidOperationException("원정 진입 씬이 빌드 목록에 없습니다.");
            isTransitioning = true;
            stageSelectionPanel.SetBusy(true);
            if (GameSession.Current != null) GameSession.Save();
            if (SceneManager.LoadSceneAsync(run.EntryScene) == null)
                throw new InvalidOperationException("원정 씬을 불러오지 못했습니다.");
            // 비동기 씬 로드를 수락한 뒤, 다음 씬의 Awake 전에 원정 정보를 전달합니다.
            GameSession.StartExpedition(run);
        }
        catch (Exception exception)
        {
            isTransitioning = false;
            stageSelectionPanel.ShowError("원정을 시작하지 못했습니다. 설정을 확인하고 다시 시도해주세요.");
            Debug.LogException(exception, this);
        }
    }

    public void OpenFamilyMembers()
    {
        if (!isReady || isTransitioning || !isActiveAndEnabled || familyMembersPanel == null ||
            (stageSelectionPanel != null && stageSelectionPanel.IsOpen)) return;
        var data = GameSession.Current;
        var members = new System.Collections.Generic.List<CharacterData>();
        if (data != null)
        {
            members.Add(data.Protagonist);
            members.AddRange(data.Companions);
        }
        familyMembersPanel.ShowMembers(data?.FamilyName, data?.Protagonist.Id, members);
        if (familyMembersPanel.IsOpen)
        {
            mainMenuPanel.GetComponent<CanvasGroup>().interactable = false;
            if (familyMembersButton != null) familyMembersButton.interactable = false;
        }
    }

    private void OnFamilyMembersClosed()
    {
        if (!isActiveAndEnabled) return;
        mainMenuPanel.GetComponent<CanvasGroup>().interactable = true;
        if (familyMembersButton != null)
        {
            familyMembersButton.interactable = true;
            familyMembersButton.Select();
        }
    }

}
