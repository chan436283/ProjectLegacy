using CWFramework;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>타이틀 메뉴와 새 게임 생성 패널 사이의 전환을 담당합니다.</summary>
public sealed class TitleFlowController : CBehaviour
{
    [SerializeField] private UIPanel mainMenuPanel;
    [SerializeField] private UIPanel familyNamePanel;
    [SerializeField] private UIPanel houseLordNamePanel;
    [SerializeField] private UIPanel statAllocationPanel;
    [SerializeField] private StatAllocationUI statAllocationUI;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text mainMenuStatusText;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_InputField familyNameInput;
    [SerializeField] private Button confirmFamilyNameButton;
    [SerializeField] private Button houseLordBackButton;
    [SerializeField] private TMP_InputField houseLordNameInput;
    [SerializeField] private Button confirmHouseLordNameButton;
    [SerializeField] private Button statsBackButton;
    [SerializeField] private Button confirmStatsButton;
    [SerializeField] private TMP_Text creationStatusText;

    [Header("New Game")]
    [SerializeField] private BattleStatFormulaConfig formulaConfig;
    [SerializeField] private CompanionGenerationConfig companionGenerationConfig;
    [SerializeField] private string destinationScene = "TownScene";

    [Header("Title Intro")]
    [SerializeField] private Image backgroundImage;
    [SerializeField, Min(0f)] private float introDelay = 1f;
    [SerializeField, Min(0f)] private float backgroundFadeDuration = 1.2f;

    // 생성 과정의 임시 데이터입니다. GameData는 능력치/동료 설정 이후에 만듭니다.
    public string FamilyName { get; private set; } = string.Empty;
    public string HouseLordName { get; private set; } = string.Empty;

    public PrimaryStats CreateHouseLordStats() => statAllocationUI.CreatePrimaryStats();

    private bool isReady;
    private bool introCompleted;
    private bool isTransitioning;
    private UIPanel currentPanel;
    private Color originalBackgroundColor;
    private Sequence introSequence;

    protected override void OnAwake()
    {
        isReady = mainMenuPanel != null && familyNamePanel != null &&
                  newGameButton != null && continueButton != null && backButton != null && familyNameInput != null &&
                  backgroundImage != null && houseLordNamePanel != null &&
                  confirmFamilyNameButton != null && houseLordBackButton != null &&
                  houseLordNameInput != null && statAllocationPanel != null &&
                  confirmHouseLordNameButton != null && statsBackButton != null &&
                  statAllocationUI != null && confirmStatsButton != null && formulaConfig != null;
        if (!isReady)
        {
            Debug.LogError("타이틀 UI 참조를 모두 연결해야 합니다.", this);
            enabled = false;
            return;
        }

        originalBackgroundColor = backgroundImage.color;
        mainMenuPanel.SetVisibleImmediate(false);
        familyNamePanel.SetVisibleImmediate(false);
        houseLordNamePanel.SetVisibleImmediate(false);
        statAllocationPanel.SetVisibleImmediate(false);
    }

    protected override void OnEnabled()
    {
        if (!isReady) return;
        newGameButton.onClick.AddListener(BeginNewGame);
        continueButton.onClick.AddListener(ContinueGame);
        RefreshContinueButton();
        backButton.onClick.AddListener(ReturnToMainMenu);
        confirmFamilyNameButton.onClick.AddListener(ConfirmFamilyName);
        houseLordBackButton.onClick.AddListener(ReturnToFamilyName);
        familyNameInput.onValueChanged.AddListener(OnFamilyNameChanged);
        RefreshFamilyNameButton();
        confirmHouseLordNameButton.onClick.AddListener(ConfirmHouseLordName);
        statsBackButton.onClick.AddListener(ReturnToHouseLordName);
        confirmStatsButton.onClick.AddListener(CompleteNewGame);
        statAllocationUI.AllocationChanged += RefreshCreationButton;
        houseLordNameInput.onValueChanged.AddListener(OnHouseLordNameChanged);
        RefreshHouseLordNameButton();
        RefreshCreationButton();

        // 중간에 비활성화된 경우에는 처음부터 다시 재생합니다.
        if (!introCompleted)
            PlayIntro();
        else
        {
            // 전환 중 중단되었다면 마지막으로 완전히 열린 패널로 복구합니다.
            currentPanel.SetVisibleImmediate(true);
            FocusCurrentPanel();
        }
    }

    protected override void OnDisabled()
    {
        introSequence?.Kill();
        introSequence = null;
        if (!isReady) return;
        if (mainMenuPanel != null) mainMenuPanel.SetVisibleImmediate(false);
        if (familyNamePanel != null) familyNamePanel.SetVisibleImmediate(false);
        if (houseLordNamePanel != null) houseLordNamePanel.SetVisibleImmediate(false);
        if (statAllocationPanel != null) statAllocationPanel.SetVisibleImmediate(false);
        isTransitioning = false;
        if (newGameButton != null) newGameButton.onClick.RemoveListener(BeginNewGame);
        if (continueButton != null) continueButton.onClick.RemoveListener(ContinueGame);
        if (backButton != null) backButton.onClick.RemoveListener(ReturnToMainMenu);
        if (confirmFamilyNameButton != null) confirmFamilyNameButton.onClick.RemoveListener(ConfirmFamilyName);
        if (houseLordBackButton != null) houseLordBackButton.onClick.RemoveListener(ReturnToFamilyName);
        if (familyNameInput != null) familyNameInput.onValueChanged.RemoveListener(OnFamilyNameChanged);
        if (confirmHouseLordNameButton != null) confirmHouseLordNameButton.onClick.RemoveListener(ConfirmHouseLordName);
        if (statsBackButton != null) statsBackButton.onClick.RemoveListener(ReturnToHouseLordName);
        if (confirmStatsButton != null) confirmStatsButton.onClick.RemoveListener(CompleteNewGame);
        if (statAllocationUI != null) statAllocationUI.AllocationChanged -= RefreshCreationButton;
        if (houseLordNameInput != null) houseLordNameInput.onValueChanged.RemoveListener(OnHouseLordNameChanged);
    }

    private void PlayIntro()
    {
        mainMenuPanel.SetVisibleImmediate(false);
        familyNamePanel.SetVisibleImmediate(false);
        houseLordNamePanel.SetVisibleImmediate(false);
        statAllocationPanel.SetVisibleImmediate(false);
        isTransitioning = true;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        backgroundImage.color = new Color(0f, 0f, 0f, originalBackgroundColor.a);

        // UI 연출은 게임의 Time.timeScale과 무관하게 진행합니다.
        introSequence = DOTween.Sequence()
            .SetUpdate(true)
            .AppendInterval(introDelay)
            .Append(backgroundImage
                .DOColor(originalBackgroundColor, backgroundFadeDuration)
                .SetEase(Ease.InOutSine))
            .OnComplete(() =>
            {
                introSequence = null;
                mainMenuPanel.Show(() =>
                {
                    introCompleted = true;
                    FinishTransition(mainMenuPanel);
                });
            });
    }

    public void BeginNewGame()
    {
        if (!CanChangePanel || currentPanel != mainMenuPanel) return;
        if (mainMenuStatusText != null) mainMenuStatusText.text = string.Empty;
        FamilyName = string.Empty;
        HouseLordName = string.Empty;
        statAllocationUI.ResetAllocation();
        familyNameInput.SetTextWithoutNotify(string.Empty);
        houseLordNameInput.SetTextWithoutNotify(string.Empty);
        RefreshHouseLordNameButton();
        RefreshFamilyNameButton();
        SwitchPanel(familyNamePanel);
    }

    public void ReturnToMainMenu()
    {
        if (!CanChangePanel || currentPanel != familyNamePanel) return;
        familyNameInput.DeactivateInputField();
        RefreshContinueButton();
        SwitchPanel(mainMenuPanel);
    }

    private void RefreshContinueButton()
    {
        continueButton.interactable = new GameSaveStore(GameSession.SavePath).HasSave;
    }

    public void ContinueGame()
    {
        if (!CanChangePanel || currentPanel != mainMenuPanel) return;
        RefreshContinueButton();
        if (!continueButton.interactable) return;

        try
        {
            if (string.IsNullOrWhiteSpace(destinationScene) ||
                !Application.CanStreamedLevelBeLoaded(destinationScene))
                throw new System.InvalidOperationException("이동할 씬이 빌드 목록에 없습니다.");
            if (!GameSession.TryLoad(formulaConfig))
                throw new System.IO.FileNotFoundException("저장 파일을 찾지 못했습니다.");

            isTransitioning = true;
            if (mainMenuStatusText != null) mainMenuStatusText.text = string.Empty;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            mainMenuPanel.Hide(LoadDestinationScene);
        }
        catch (System.Exception exception)
        {
            ShowContinueError(exception);
        }
    }

    private void ShowContinueError(System.Exception exception)
    {
        isTransitioning = false;
        RefreshContinueButton();
        if (mainMenuStatusText != null)
            mainMenuStatusText.text = "저장 데이터를 불러오지 못했습니다. 다시 시도해주세요.";
        Debug.LogException(exception, this);
    }

    public void ConfirmFamilyName()
    {
        if (!CanChangePanel || currentPanel != familyNamePanel) return;

        string name = familyNameInput.text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            RefreshFamilyNameButton();
            FocusCurrentPanel();
            return;
        }

        FamilyName = name;
        familyNameInput.SetTextWithoutNotify(name);
        familyNameInput.DeactivateInputField();
        SwitchPanel(houseLordNamePanel);
    }

    public void ReturnToFamilyName()
    {
        if (!CanChangePanel || currentPanel != houseLordNamePanel) return;
        houseLordNameInput.DeactivateInputField();
        RefreshFamilyNameButton();
        SwitchPanel(familyNamePanel);
    }

    private void OnFamilyNameChanged(string value)
    {
        // 이전 단계로 돌아와 수정한 이름은 다음 버튼을 눌러 다시 확정합니다.
        FamilyName = string.Empty;
        RefreshFamilyNameButton();
    }

    private void RefreshFamilyNameButton()
    {
        confirmFamilyNameButton.interactable = !string.IsNullOrWhiteSpace(familyNameInput.text);
    }

    public void ConfirmHouseLordName()
    {
        if (!CanChangePanel || currentPanel != houseLordNamePanel) return;
        string name = houseLordNameInput.text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            RefreshHouseLordNameButton();
            FocusCurrentPanel();
            return;
        }

        HouseLordName = name;
        houseLordNameInput.SetTextWithoutNotify(name);
        houseLordNameInput.DeactivateInputField();
        SwitchPanel(statAllocationPanel);
    }

    public void ReturnToHouseLordName()
    {
        if (!CanChangePanel || currentPanel != statAllocationPanel) return;
        RefreshHouseLordNameButton();
        SwitchPanel(houseLordNamePanel);
    }

    private void OnHouseLordNameChanged(string value)
    {
        HouseLordName = string.Empty;
        RefreshHouseLordNameButton();
    }

    private void RefreshHouseLordNameButton()
    {
        confirmHouseLordNameButton.interactable = !string.IsNullOrWhiteSpace(houseLordNameInput.text);
    }

    private bool CanChangePanel => isReady && introCompleted && !isTransitioning && isActiveAndEnabled;

    private void RefreshCreationButton()
    {
        confirmStatsButton.interactable = statAllocationUI.RemainingPoints == 0;
        if (creationStatusText != null)
            creationStatusText.text = statAllocationUI.RemainingPoints == 0
                ? string.Empty : "남은 포인트를 모두 배분해주세요.";
    }

    public void CompleteNewGame()
    {
        if (!CanChangePanel || currentPanel != statAllocationPanel) return;
        if (statAllocationUI.RemainingPoints != 0)
        {
            RefreshCreationButton();
            return;
        }

        try
        {
            if (string.IsNullOrWhiteSpace(destinationScene) ||
                !Application.CanStreamedLevelBeLoaded(destinationScene))
                throw new System.InvalidOperationException("이동할 씬이 빌드 목록에 없습니다.");

            var data = new GameData(FamilyName, HouseLordName, CreateHouseLordStats(), formulaConfig);
            if (companionGenerationConfig == null)
                throw new System.InvalidOperationException("동료 생성 설정이 필요합니다.");
            companionGenerationConfig.Validate();
            // 성별별 공통 이름 풀이 구현되면 이 임시 이름 생성을 교체합니다.
            var companionNames = new string[companionGenerationConfig.startingCount];
            for (int i = 0; i < companionNames.Length; i++) companionNames[i] = $"동료 {i + 1}";
            var companions = new CompanionGenerator(new System.Random()).Generate(
                companionGenerationConfig, formulaConfig, companionNames);
            foreach (var companion in companions) data.AddCompanion(companion);
            // 파일 저장이 성공한 뒤에만 세션 데이터와 화면을 전환합니다.
            GameSession.StartNewGame(data);
            isTransitioning = true;
            if (creationStatusText != null) creationStatusText.text = string.Empty;
            statAllocationPanel.Hide(LoadDestinationScene);
        }
        catch (System.Exception exception)
        {
            ShowCreationError(exception);
        }
    }

    private void LoadDestinationScene()
    {
        try
        {
            if (SceneManager.LoadSceneAsync(destinationScene) == null)
                throw new System.InvalidOperationException("씬을 불러오지 못했습니다.");
        }
        catch (System.Exception exception)
        {
            var sourcePanel = currentPanel;
            sourcePanel.Show(() =>
            {
                FinishTransition(sourcePanel);
                if (sourcePanel == mainMenuPanel) ShowContinueError(exception);
                else ShowCreationError(exception);
            });
        }
    }

    private void ShowCreationError(System.Exception exception)
    {
        isTransitioning = false;
        if (creationStatusText != null)
            creationStatusText.text = "게임을 시작하지 못했습니다. 다시 시도해주세요.";
        Debug.LogException(exception, this);
    }

    private void SwitchPanel(UIPanel nextPanel)
    {
        if (currentPanel == nextPanel) return;
        isTransitioning = true;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        // 퇴장이 끝난 후에만 다음 패널을 열어 두 연출이 겹치지 않게 합니다.
        currentPanel.Hide(() => nextPanel.Show(() => FinishTransition(nextPanel)));
    }

    private void FinishTransition(UIPanel panel)
    {
        currentPanel = panel;
        isTransitioning = false;
        FocusCurrentPanel();
    }

    private void FocusCurrentPanel()
    {
        if (currentPanel == familyNamePanel)
        {
            familyNameInput.Select();
            familyNameInput.ActivateInputField();
        }
        else if (currentPanel == houseLordNamePanel)
        {
            houseLordNameInput.Select();
            houseLordNameInput.ActivateInputField();
        }
        else if (currentPanel == statAllocationPanel)
        {
            statsBackButton.Select();
        }
        else
        {
            newGameButton.Select();
        }
    }
}
