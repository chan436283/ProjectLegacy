using System;
using System.Collections.Generic;
using System.Text;
using CWFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>씬과 독립적으로 전달받은 구성원 목록을 조회하는 공용 창입니다.</summary>
public sealed class FamilyMembersPanel : CBehaviour
{
    [Serializable]
    private sealed class MemberRow
    {
        public Button button;
        public TMP_Text label;
    }

    [SerializeField] private UIPanel panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text identityText;
    [SerializeField] private TMP_Text primaryStatsText;
    [SerializeField] private TMP_Text battleStatsText;
    [SerializeField] private TMP_Text pageText;
    [SerializeField] private MemberRow[] rows;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    public bool IsOpen { get; private set; }
    public event Action Closed;

    private readonly List<CharacterData> members = new();
    private string houseLordId;
    private string selectedId;
    private int page;
    private bool transitioning;
    private bool ready;
    private static readonly string[] PrimaryLabels = { "STR", "CON", "DEX", "AGI", "INT", "WIS", "CHA", "LUK" };
    private static readonly string[] BattleLabels =
    {
        "최대 HP", "최대 MP", "물리 공격력", "물리 방어력", "마법 공격력", "마법 저항력",
        "명중", "회피", "치명타 확률", "치명타 피해", "속도"
    };

    protected override void OnAwake()
    {
        // 비활성 상태에서 ShowMembers가 먼저 초기화했다면 열기 도중 다시 숨기지 않습니다.
        if (ready) return;
        if (Initialize()) panel.SetVisibleImmediate(false);
    }

    private bool Initialize()
    {
        if (ready) return true;
        if (panel == null) panel = GetComponent<UIPanel>();
        ready = panel != null && titleText != null && identityText != null && primaryStatsText != null &&
                battleStatsText != null && pageText != null && previousButton != null && nextButton != null &&
                closeButton != null && rows != null && rows.Length > 0;
        if (ready)
            foreach (var row in rows)
                ready &= row != null && row.button != null && row.label != null;
        if (!ready)
        {
            Debug.LogError("가문 구성원 UI 참조를 모두 연결해야 합니다.", this);
            enabled = false;
            return false;
        }
        // 사용자 입력 이름을 TMP 태그로 해석하지 않습니다.
        titleText.richText = identityText.richText = false;
        for (int i = 0; i < rows.Length; i++)
        {
            int slot = i;
            rows[i].label.richText = false;
            rows[i].button.onClick.AddListener(() => SelectSlot(slot));
        }
        previousButton.onClick.AddListener(PreviousPage);
        nextButton.onClick.AddListener(NextPage);
        closeButton.onClick.AddListener(Close);
        return true;
    }

    public void ShowMembers(string familyName, string protagonistId, IEnumerable<CharacterData> characters)
    {
        // GameObject가 꺼져 있어 Awake가 아직 호출되지 않았어도 열 수 있습니다.
        if (!enabled || transitioning || IsOpen || !Initialize()) return;
        members.Clear();
        if (characters != null)
            foreach (var member in characters)
                if (member != null) members.Add(member);
        houseLordId = protagonistId;
        int selected = members.FindIndex(member => member.Id == selectedId);
        if (selected < 0) selected = 0;
        page = selected / rows.Length;
        selectedId = members.Count > 0 ? members[selected].Id : null;
        titleText.text = string.IsNullOrWhiteSpace(familyName) ? "가문 구성원" : $"{familyName} · 가문 구성원";
        RefreshPage();
        IsOpen = transitioning = true;
        panel.Show(() => { transitioning = false; closeButton.Select(); });
    }

    public void Close()
    {
        if (!IsOpen || transitioning) return;
        transitioning = true;
        panel.Hide(CompleteClose);
    }

    private void CompleteClose()
    {
        bool wasOpen = IsOpen;
        IsOpen = transitioning = false;
        // 같은 오브젝트에 붙어 있으면 Hide 완료 전에 OnDisabled가 호출됩니다.
        if (wasOpen) Closed?.Invoke();
    }

    private void PreviousPage() => ChangePage(-1);
    private void NextPage() => ChangePage(1);

    private void ChangePage(int delta)
    {
        if (!IsOpen || transitioning) return;
        page = Mathf.Clamp(page + delta, 0, Math.Max(0, (members.Count - 1) / rows.Length));
        if (members.Count > 0) selectedId = members[page * rows.Length].Id;
        RefreshPage();
        closeButton.Select();
    }

    private void SelectSlot(int slot)
    {
        if (!IsOpen || transitioning) return;
        int index = page * rows.Length + slot;
        if (index >= members.Count) return;
        selectedId = members[index].Id;
        RefreshPage();
    }

    private void RefreshPage()
    {
        for (int slot = 0; slot < rows.Length; slot++)
        {
            int index = page * rows.Length + slot;
            var row = rows[slot];
            row.button.gameObject.SetActive(index < members.Count);
            if (index >= members.Count) continue;
            var member = members[index];
            string marker = member.Id == selectedId ? "> " : string.Empty;
            string role = member.Id == houseLordId ? " [가주]" : string.Empty;
            row.label.text = $"{marker}{member.Name}{role}  Lv.{member.Stats.Level}";
        }
        int pages = Math.Max(1, (members.Count + rows.Length - 1) / rows.Length);
        pageText.text = $"{page + 1} / {pages}";
        previousButton.interactable = page > 0;
        nextButton.interactable = page + 1 < pages;
        ShowCharacter(members.Find(member => member.Id == selectedId));
    }

    private void ShowCharacter(CharacterData member)
    {
        if (member == null)
        {
            identityText.text = "표시할 구성원이 없습니다.";
            primaryStatsText.text = battleStatsText.text = string.Empty;
            return;
        }
        var stats = member.Stats;
        string role = member.Id == houseLordId ? "가주" : "동료";
        identityText.text = $"{member.Name} · {role} · Lv.{stats.Level}";
        var text = new StringBuilder();
        foreach (PrimaryStatType type in Enum.GetValues(typeof(PrimaryStatType)))
            AppendStatRow(text, PrimaryLabels[(int)type], $"{stats.PrimaryStats.Get(type).Value:0.#}");
        ConfigureStatAlignment(primaryStatsText);
        primaryStatsText.text = text.ToString();
        text.Clear();
        AppendStatRow(text, "HP", $"{stats.CurrentHp:0.#}/{stats.Battle.MaxHp.Value:0.#}");
        AppendStatRow(text, "MP", $"{stats.CurrentMp:0.#}/{stats.Battle.MaxMp.Value:0.#}");
        foreach (BattleStatType type in Enum.GetValues(typeof(BattleStatType)))
        {
            if (type == BattleStatType.MaxHp || type == BattleStatType.MaxMp) continue;
            bool percent = type == BattleStatType.CriticalChance || type == BattleStatType.CriticalDamage;
            AppendStatRow(text, BattleLabels[(int)type], $"{stats.Battle.Get(type).Value:0.#}{(percent ? "%" : "")}");
        }
        ConfigureStatAlignment(battleStatsText);
        battleStatsText.text = text.ToString();
    }

    private static void ConfigureStatAlignment(TMP_Text text)
    {
        text.richText = true;
        // 마지막 줄을 포함해 양 끝에 맞추되, 글자 사이가 아닌 공백만 늘립니다.
        text.horizontalAlignment = HorizontalAlignmentOptions.Flush;
        text.wordWrappingRatios = 0f;
    }

    private static void AppendStatRow(StringBuilder text, string label, string value)
    {
        // 이름 내부의 공백은 유지하고 이름과 값 사이 공백 하나만 확장합니다.
        if (text.Length > 0) text.Append('\n');
        text.Append("<nobr>").Append(label.Replace(' ', '\u00A0'))
            .Append(' ').Append(value).Append("</nobr>");
    }

    protected override void OnDisabled()
    {
        CompleteClose();
        // 이미 닫힌 패널을 재차 숨겨 UIPanel의 완료 처리를 방해하지 않습니다.
        if (panel != null && panel.gameObject.activeSelf) panel.SetVisibleImmediate(false);
    }
}
