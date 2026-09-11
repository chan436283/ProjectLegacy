using System.Collections;
using System.Collections.Generic;
using CWFramework;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BattleUnit))]
public sealed class BattleUnitView : CBehaviour
{
    private const float UnavailableBrightness = 0.55f;
    private const string HitAnimationTrigger = "Hurt";
    private const string DeathAnimationBool = "IsDead";
    private const float HitFlashDuration = 0.15f;
    private static readonly Color HitFlashColor = Color.red;

    [SerializeField] private Animator animator;
    [SerializeField] private TargetIndicator targetIndicator;
    [Tooltip("밝기를 변경할 캐릭터 본체만 지정합니다. 비워두면 이 오브젝트의 SpriteRenderer를 사용합니다.")]
    [SerializeField] private SpriteRenderer[] bodyRenderers;
    [Tooltip("Flip X가 꺼진 원본 스프라이트가 오른쪽을 바라보면 켭니다.")]
    [SerializeField] private bool spriteFacesRight = true;
    private CharacterStats observedStats;
    private readonly HashSet<string> skillTriggers = new();
    private readonly HashSet<string> skillBools = new();

    private Coroutine hitFlash;
    private bool isHitFlashing;

    [Header("Pointer Selection")]
    [Tooltip("유닛 로컬 좌표 기준 선택 사각형의 중심입니다. Sprite Flip X에는 영향받지 않습니다.")]
    [SerializeField] private Vector2 selectionOffset = Vector2.zero;
    [Tooltip("유닛 로컬 좌표 기준 선택 사각형의 가로/세로 크기입니다.")]
    [SerializeField] private Vector2 selectionSize = new(0.5f, 0.7f);

    private Color[] originalBodyColors;
    private SkillAnimationEventRelay animationEvents;

    public TargetSelectionState SelectionState { get; private set; }

    public bool TryHitSelection(Ray ray, out float distance)
    {
        distance = 0f;
        if (!isActiveAndEnabled || selectionSize.x <= 0f || selectionSize.y <= 0f)
            return false;

        // Keep the ray parameter in world units; do not normalize the local direction.
        Vector3 origin = transform.InverseTransformPoint(ray.origin);
        Vector3 direction = transform.InverseTransformVector(ray.direction);
        if (Mathf.Abs(direction.z) < 0.000001f) return false;
        float t = -origin.z / direction.z;
        if (t < 0f) return false;
        Vector3 point = origin + direction * t;
        if (Mathf.Abs(point.x - selectionOffset.x) > selectionSize.x * 0.5f ||
            Mathf.Abs(point.y - selectionOffset.y) > selectionSize.y * 0.5f)
            return false;
        distance = t;
        return true;
    }

    public int SelectionSortingLayer => SelectionRenderer != null
        ? SortingLayer.GetLayerValueFromID(SelectionRenderer.sortingLayerID) : 0;
    public int SelectionSortingOrder => SelectionRenderer != null ? SelectionRenderer.sortingOrder : 0;

    private SpriteRenderer SelectionRenderer
    {
        get
        {
            if (bodyRenderers != null)
                foreach (SpriteRenderer body in bodyRenderers)
                    if (body != null) return body;
            return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Color previousColor = Gizmos.color;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(new Vector3(selectionOffset.x, selectionOffset.y, 0f),
            new Vector3(Mathf.Max(0f, selectionSize.x), Mathf.Max(0f, selectionSize.y), 0f));
        Gizmos.matrix = previousMatrix;
        Gizmos.color = previousColor;
    }

    public void SetTargetSelection(TargetSelectionState state)
    {
        SelectionState = state;
        RefreshBodyColors();
        SetTargetIndicator(state == TargetSelectionState.Selected
            ? TargetIndicatorState.Selected : TargetIndicatorState.Hidden);
    }

    private void RefreshBodyColors()
    {
        if (originalBodyColors == null) return;
        float brightness = SelectionState == TargetSelectionState.Unavailable ? UnavailableBrightness : 1f;
        for (int i = 0; i < bodyRenderers.Length; i++)
        {
            if (bodyRenderers[i] == null) continue;
            Color original = originalBodyColors[i];
            Color tint = isHitFlashing ? HitFlashColor : original;
            bodyRenderers[i].color = new Color(tint.r * brightness,
                tint.g * brightness, tint.b * brightness, original.a);
        }
    }

    private void StartHitFlash()
    {
        StopHitFlash();
        if (HitFlashDuration <= 0f || !isActiveAndEnabled) return;
        hitFlash = StartCoroutine(FlashOnHit());
    }

    private IEnumerator FlashOnHit()
    {
        isHitFlashing = true;
        RefreshBodyColors();
        yield return new WaitForSeconds(HitFlashDuration);
        isHitFlashing = false;
        hitFlash = null;
        RefreshBodyColors();
    }

    private void StopHitFlash()
    {
        StopAndClear(ref hitFlash);
        isHitFlashing = false;
        RefreshBodyColors();
    }

    public void SetTargetIndicator(TargetIndicatorState state)
    {
        if (targetIndicator != null)
            targetIndicator.SetState(state);
    }
    private BattleUnit unit;
    // A later skill using the same parameter replaces its previous binding.
    private readonly Dictionary<string, (string statusId, bool value)> maintainedBools = new();
    private readonly List<string> expiredKeys = new();

    protected override void OnAwake()
    {
        unit = GetComponent<BattleUnit>();

        if (bodyRenderers == null || bodyRenderers.Length == 0)
            bodyRenderers = GetComponents<SpriteRenderer>();

        originalBodyColors = new Color[bodyRenderers.Length];

        for (int i = 0; i < bodyRenderers.Length; i++)
            if (bodyRenderers[i] != null) originalBodyColors[i] = bodyRenderers[i].color;

        if (targetIndicator == null)
            targetIndicator = GetComponentInChildren<TargetIndicator>(true);

        if (targetIndicator != null)
        {
            Transform indicatorTransform = targetIndicator.transform;
            Vector3 parentScale = indicatorTransform.parent != null
                ? indicatorTransform.parent.lossyScale
                : Vector3.one;

            indicatorTransform.localScale = new Vector3(
                Mathf.Approximately(parentScale.x, 0f) ? 0f : 2f / parentScale.x,
                Mathf.Approximately(parentScale.y, 0f) ? 0f : 2f / parentScale.y,
                Mathf.Approximately(parentScale.z, 0f) ? 0f : 2f / parentScale.z);
        }

        SetTargetSelection(TargetSelectionState.None);

        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (animator != null)
        {
            animationEvents = animator.GetComponent<SkillAnimationEventRelay>();
            if (animationEvents == null)
                animationEvents = animator.gameObject.AddComponent<SkillAnimationEventRelay>();
        }
    }

    protected override void OnEnabled()
    {
        unit.StatusesChanged += RefreshAnimation;
        unit.DamageReceived += OnDamageReceived;
        // Read the component directly: BattleUnit.Awake may not have run yet.
        observedStats = GetComponent<CharacterStatsComponent>().Stats;
        observedStats.HpChanged += OnHpChanged;
        RefreshDeathAnimation();
        RefreshAnimation();
    }

    protected override void OnDisabled()
    {
        unit.StatusesChanged -= RefreshAnimation;
        unit.DamageReceived -= OnDamageReceived;
        if (observedStats != null) observedStats.HpChanged -= OnHpChanged;
        observedStats = null;
        ResetHitTrigger();
        StopHitFlash();
        SetTargetSelection(TargetSelectionState.None);
    }

    private void OnHpChanged(float currentHp, float maxHp) => RefreshDeathAnimation();

    private void RefreshDeathAnimation()
    {
        if (observedStats == null) return;
        bool dead = observedStats.IsDead;
        if (dead)
        {
            ResetHitTrigger();
            StopHitFlash();
            SetTargetSelection(TargetSelectionState.None);
            foreach (string key in skillTriggers)
                if (HasParameter(key, AnimatorControllerParameterType.Trigger)) animator.ResetTrigger(key);
            foreach (string key in skillBools)
                if (HasParameter(key, AnimatorControllerParameterType.Bool)) animator.SetBool(key, false);
            skillTriggers.Clear();
            skillBools.Clear();
            maintainedBools.Clear();
        }
        if (!string.IsNullOrWhiteSpace(DeathAnimationBool) &&
            HasParameter(DeathAnimationBool, AnimatorControllerParameterType.Bool))
            animator.SetBool(DeathAnimationBool, dead);
    }

    private void ResetHitTrigger()
    {
        if (!string.IsNullOrWhiteSpace(HitAnimationTrigger) &&
            HasParameter(HitAnimationTrigger, AnimatorControllerParameterType.Trigger))
            animator.ResetTrigger(HitAnimationTrigger);
    }

    private void OnDamageReceived(BattleDamageInfo damage)
    {
        // Lethal direct hits still flash; only the Hurt animation is suppressed on death.
        if (damage.AppliedAmount > 0f && damage.Source == BattleDamageSource.DirectAttack &&
            !damage.WasDefending)
            StartHitFlash();

        // A later lethal hit in the same frame must cancel a previously queued reaction.
        if (damage.IsDead) { ResetHitTrigger(); return; }
        if (!damage.ShouldPlayHitReaction) return;
        if (animator == null || !animator.isActiveAndEnabled ||
            string.IsNullOrWhiteSpace(HitAnimationTrigger)) return;
        if (!HasParameter(HitAnimationTrigger, AnimatorControllerParameterType.Trigger)) return;
        // Do not clear skill animation signals: this is a reaction, not a new skill.
        animator.ResetTrigger(HitAnimationTrigger);
        animator.SetTrigger(HitAnimationTrigger);
    }

    private bool HasParameter(string key, AnimatorControllerParameterType type)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return false;
        foreach (AnimatorControllerParameter parameter in animator.parameters)
            if (parameter.name == key && parameter.type == type)
                return true;
        return false;
    }

    private void RefreshAnimation()
    {
        if (observedStats != null && observedStats.IsDead) return;
        if (animator == null || !animator.isActiveAndEnabled)
            return;

        expiredKeys.Clear();
        foreach (var entry in maintainedBools)
        {
            bool active = unit.HasStatus(entry.Value.statusId);
            if (HasParameter(entry.Key, AnimatorControllerParameterType.Bool))
                animator.SetBool(entry.Key, active && entry.Value.value);
            if (!active) expiredKeys.Add(entry.Key);
        }
        foreach (string key in expiredKeys) maintainedBools.Remove(key);
    }

    public void PlaySkillAnimation(SkillAnimationSettings settings)
    {
        if (observedStats != null && observedStats.IsDead) return;
        if (animationEvents != null) animationEvents.ClearSignals();
        if (settings == null || string.IsNullOrWhiteSpace(settings.ParameterKey))
            return;
        AnimatorControllerParameterType type;
        switch (settings.ParameterType)
        {
            case ParameterType.Trigger:
                type = AnimatorControllerParameterType.Trigger;
                break;
            case ParameterType.Bool:
                type = AnimatorControllerParameterType.Bool;
                break;
            default:
                Debug.LogWarning("지원하지 않는 애니메이션 파라미터 타입입니다.", this);
                return;
        }
        string key = settings.ParameterKey;
        if (key == DeathAnimationBool) return;
        if (!HasParameter(key, type))
        {
            Debug.LogWarning($"Animator에 {type} 파라미터 '{key}'가 없습니다.", this);
            return;
        }

        maintainedBools.Remove(key);
        if (type == AnimatorControllerParameterType.Trigger)
        {
            skillTriggers.Add(key);
            animator.SetTrigger(key);
            return;
        }

        skillBools.Add(key);
        bool value = settings.BoolValue;
        if (!string.IsNullOrWhiteSpace(settings.MaintainWhileStatusId))
        {
            maintainedBools[key] = (settings.MaintainWhileStatusId, value);
            value &= unit.HasStatus(settings.MaintainWhileStatusId);
        }
        animator.SetBool(key, value);
    }

    public bool[] CaptureFacing()
    {
        var flips = new bool[bodyRenderers.Length];
        for (int i = 0; i < flips.Length; i++)
            if (bodyRenderers[i] != null) flips[i] = bodyRenderers[i].flipX;
        return flips;
    }

    public void RestoreFacing(bool[] flips)
    {
        if (flips == null) return;
        for (int i = 0; i < flips.Length && i < bodyRenderers.Length; i++)
            if (bodyRenderers[i] != null) bodyRenderers[i].flipX = flips[i];
    }

    public void FaceTowards(Vector3 destination)
    {
        float dx = destination.x - transform.position.x;
        if (Mathf.Abs(dx) <= 0.00001f) return;
        foreach (SpriteRenderer body in bodyRenderers)
        {
            if (body == null) continue;
            bool unflippedFacesRight = spriteFacesRight != (body.transform.lossyScale.x < 0f);
            body.flipX = (dx > 0f) != unflippedFacesRight;
        }
    }

    // Movement owns its bool lifetime, independently of status-bound animations.
    public void SetMovementAnimationBool(string key, bool value)
    {
        if (string.IsNullOrWhiteSpace(key) || key == DeathAnimationBool) return;
        if (observedStats != null && observedStats.IsDead) value = false;
        maintainedBools.Remove(key);
        if (HasParameter(key, AnimatorControllerParameterType.Bool))
        {
            if (value) skillBools.Add(key);
            else skillBools.Remove(key);
            animator.SetBool(key, value);
        }
        else if (value)
            Debug.LogWarning($"Animator에 Bool 파라미터 '{key}'가 없습니다.", this);
    }

    public bool ConsumeAnimationSignal(string signal) =>
        animationEvents != null && animationEvents.Consume(signal);

}
