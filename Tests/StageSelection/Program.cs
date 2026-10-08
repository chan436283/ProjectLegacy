using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

static class Program
{
    static void Set(object target, string name, object value) => target.GetType()
        .GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
    static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        Console.WriteLine("PASS: " + message);
    }
    static void Main()
    {
        Check(GameTextFormatter.Format(null, "Name", "A") == null, "null template preserved");
        Check(GameTextFormatter.Format("고정 이름", "Name", null) == "고정 이름", "plain text unchanged");
        Check(GameTextFormatter.Format("{Name}/{Name}", "Name", " A ") == " A / A ", "repeated tokens and value whitespace preserved");
        Check(GameTextFormatter.Format("{Name}", "Name", null) == "", "null value becomes empty");
        Check(GameTextFormatter.Format("<b>{Name}</b> {Unknown} {0}", "Name", "A") == "<b>A</b> {Unknown} {0}",
            "rich text and unrelated placeholders preserved");
        Check(GameTextFormatter.Format("{Name}", "Name", "{Name}") == "{Name}", "single token replacement is not recursive");
        var tokens = new System.Collections.Generic.Dictionary<string, string>
        {
            { "CharacterName", "{StageName}" }, { "StageName", "남서 가도" }, { "Empty", null }
        };
        Check(GameTextFormatter.Format("{CharacterName}: {StageName}/{StageName} {Unknown}{Empty}", tokens)
            == "{StageName}: 남서 가도/남서 가도 {Unknown}", "multiple tokens replaced without recursive substitution");
        Check(GameTextFormatter.Format(null, tokens) == null, "multi token null template preserved");
        bool invalidTokenRejected = false;
        try { GameTextFormatter.Format("text", "{Name}", "A"); }
        catch (ArgumentException) { invalidTokenRejected = true; }
        Check(invalidTokenRejected, "token names must exclude braces");
        var template = new StageDefinition { mapPrefab = new ExpeditionMap(), stageId = "road", displayName = "{FamilyName} 남서 가도" };
        Check(template.GetDisplayName(" 아르덴 ") == "아르덴 남서 가도", "resolve family token");
        Check(template.CreateRun("아르덴").StageName == "아르덴 남서 가도", "run stores resolved stage name");
        Check(template.GetDisplayName(null) == "이름 없는 가문 남서 가도", "missing family fallback");
        Check(template.displayName == "{FamilyName} 남서 가도", "template asset remains unchanged");
        var panel = new StageSelectionPanel();
        var close = new Button();
        var depart = new Button();
        var title = new TMP_Text();
        Set(panel, "panel", new UIPanel());
        Set(panel, "closeButton", close);
        Set(panel, "departButton", depart);
        Set(panel, "stageInfoText", title);
        var points = new MapPoint[2];
        var buttons = new[] { new Button(), new Button() };
        var selected = new[] { new GameObject(), new GameObject() };
        var pulseTargets = new[] { new RectTransform(), new RectTransform() };
        for (int i = 0; i < 2; i++)
        {
            var point = new MapPoint();
            Set(point, "stage", new StageDefinition { mapPrefab = new ExpeditionMap(), stageId = "stage" + i, displayName = "{FamilyName} 지점" + i });
            Set(point, "button", buttons[i]);
            Set(point, "available", i == 0);
            Set(point, "pulseTarget", pulseTargets[i]);
            Set(point, "selectedIndicator", selected[i]);
            points[i] = point;
        }
        Set(panel, "points", points);
        int departures = 0;
        StageDefinition requested = null;
        panel.DepartureRequested += stage => { departures++; requested = stage; };
        Check(panel.Open("아르덴") && !depart.interactable && title.text == "원정 지점을 선택해주세요.", "open without selection");
        Check(pulseTargets[0].lastTween.active && pulseTargets[1].lastTween == null && !buttons[1].interactable,
            "available point pulses, locked point does not");
        buttons[1].onClick.Invoke();
        depart.onClick.Invoke();
        Check(panel.SelectedStage == null && departures == 0, "locked point and empty departure rejected");
        buttons[0].onClick.Invoke();
        Check(departures == 0 && depart.interactable && title.text == "아르덴 지점0" && selected[0].activeSelf,
            "click selects without departure");
        Check(selected[0].rect.lastTween.active && selected[0].rect.anchoredPosition.y > 0,
            "selected indicator starts vertical animation");
        var indicatorTween = selected[0].rect.lastTween;
        buttons[0].onClick.Invoke();
        Check(selected[0].rect.lastTween == indicatorTween, "reselect does not restart indicator animation");
        panel.SetStageAvailable("stage1", true);
        buttons[1].onClick.Invoke();
        Check(!selected[0].activeSelf && selected[1].activeSelf && title.text == "아르덴 지점1", "switch selection");
        Check(!indicatorTween.active && selected[0].rect.anchoredPosition.y == 0,
            "deselection stops indicator and restores position");
        points[1].SetAvailable(false);
        Check(!pulseTargets[1].lastTween.active, "locking point stops pulse");
        Check(panel.SelectedStage == null && !depart.interactable && !selected[1].activeSelf && title.text == "원정 지점을 선택해주세요.",
            "locking selected point clears selection");
        buttons[0].onClick.Invoke();
        depart.onClick.Invoke();
        Check(!pulseTargets[0].lastTween.active, "departure stops pulse");
        depart.onClick.Invoke();
        close.onClick.Invoke();
        Check(departures == 1 && requested.stageId == "stage0" && panel.IsOpen && !close.interactable,
            "departure sends selected stage once and blocks close");
        panel.ShowError("실패");
        Check(pulseTargets[0].lastTween.active, "retry resumes pulse");
        Check(depart.interactable && title.text == "실패" && panel.SelectedStage.stageId == "stage0", "error retains selection for retry");
        buttons[0].onClick.Invoke();
        Check(title.text == "아르덴 지점0", "selecting point restores name after error");
        depart.onClick.Invoke();
        Check(departures == 2, "retry permitted");
        panel.ShowError("실패");
        close.onClick.Invoke();
        Check(!panel.IsOpen && !selected[0].activeSelf, "close clears indicator");
        Check(panel.Open("아르덴") && panel.SelectedStage == null && !depart.interactable, "reopen resets selection");
    }
}

// 입력 및 상태 전환만 검증합니다. 실제 Unity 생명주기·Tween·Raycast는 Play Mode 확인 대상입니다.
namespace UnityEngine
{
    public class GameObject
    {
        public bool activeSelf = true;
        public RectTransform rect = new();
        public T GetComponent<T>() where T : class => rect as T;
        public void SetActive(bool value) => activeSelf = value;
    }
    public class MonoBehaviour
    {
        public bool enabled = true;
        public bool isActiveAndEnabled => enabled;
        public GameObject gameObject = new();
        public T GetComponent<T>() where T : class => null;
    }
    public class ScriptableObject { }
    public struct Vector3
    {
        public float x, y, z;
        public static Vector3 operator *(Vector3 value, float scale) => new Vector3 { x = value.x * scale, y = value.y * scale, z = value.z * scale };
    }
    public struct Vector2 { public float x, y; }
    public class RectTransform { public Vector2 anchoredPosition; public Vector3 localScale = new Vector3 { x = 1, y = 1, z = 1 }; public DG.Tweening.Tween lastTween; }
    public sealed class DisallowMultipleComponent : Attribute { }
    public sealed class TooltipAttribute : Attribute { public TooltipAttribute(string text) { } }
    public sealed class RangeAttribute : Attribute { public RangeAttribute(float min, float max) { } }
    public sealed class MinAttribute : Attribute { public MinAttribute(float min) { } }
    public sealed class SerializeField : Attribute { }
    public sealed class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
    public static class Debug { public static void LogException(Exception exception, object context) => throw exception; }
}
namespace UnityEngine.Events
{
    public delegate void UnityAction();
    public class UnityEvent
    {
        private event UnityAction listeners;
        public void AddListener(UnityAction listener) => listeners += listener;
        public void RemoveListener(UnityAction listener) => listeners -= listener;
        public void Invoke() => listeners?.Invoke();
    }
}
namespace UnityEngine.UI
{
    public class Button
    {
        public bool interactable;
        public UnityEngine.Events.UnityEvent onClick = new();
        public void Select() { }
    }
}
namespace TMPro { public class TMP_Text { public bool richText; public string text; } }
namespace CWFramework
{
    public class CBehaviour : UnityEngine.MonoBehaviour
    {
        protected virtual void OnAwake() { }
        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
        protected virtual void OnReleased() { }
    }
}
public class UIPanel : UnityEngine.MonoBehaviour
{
    public void SetVisibleImmediate(bool visible) => gameObject.SetActive(visible);
    public void Show(Action completed) { gameObject.SetActive(true); completed?.Invoke(); }
    public void Hide(Action completed) { gameObject.SetActive(false); completed?.Invoke(); }
}

namespace UnityEngine.Serialization
{
    public sealed class FormerlySerializedAsAttribute : Attribute
    {
        public FormerlySerializedAsAttribute(string oldName) { }
    }
}

namespace DG.Tweening
{
    public enum Ease { InOutSine }
    public enum LoopType { Yoyo }
    public class Tween
    {
        public bool active = true;
        public Tween SetEase(Ease ease) => this;
        public Tween SetLoops(int count, LoopType type) => this;
        public Tween SetUpdate(bool independent) => this;
        public void Kill() => active = false;
    }
    public static class ShortcutExtensions
    {
        public static Tween DOAnchorPosY(this UnityEngine.RectTransform target, float endY, float duration)
        {
            target.anchoredPosition = new UnityEngine.Vector2 { x = target.anchoredPosition.x, y = endY };
            return target.lastTween = new Tween();
        }
        public static Tween DOScale(this UnityEngine.RectTransform target, UnityEngine.Vector3 scale, float duration)
        {
            target.localScale = scale;
            return target.lastTween = new Tween();
        }
    }
}
