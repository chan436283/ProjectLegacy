using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleSkill))]
public sealed class BattleSkillEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script", "effects", "sequence");
        EditorGUILayout.Space();
        DrawSequence();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("전투 효과 (Gameplay Effects)", EditorStyles.boldLabel);
        SerializedProperty effects = serializedObject.FindProperty("effects");

        for (int i = 0; i < effects.arraySize; i++)
        {
            SerializedProperty effect = effects.GetArrayElementAtIndex(i);
            int moveTo = i;
            bool remove;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    string label = effect.managedReferenceValue?.GetType().Name ?? "비어 있는 효과";
                    effect.isExpanded = EditorGUILayout.Foldout(effect.isExpanded, label, true);
                    using (new EditorGUI.DisabledScope(i == 0))
                        if (GUILayout.Button("↑", GUILayout.Width(28))) moveTo = i - 1;
                    using (new EditorGUI.DisabledScope(i == effects.arraySize - 1))
                        if (GUILayout.Button("↓", GUILayout.Width(28))) moveTo = i + 1;
                    remove = GUILayout.Button("삭제", GUILayout.Width(45));
                }

                if (effect.isExpanded && effect.managedReferenceValue != null)
                {
                    SerializedProperty child = effect.Copy();
                    SerializedProperty end = child.GetEndProperty();
                    if (child.NextVisible(true))
                    {
                        do
                        {
                            if (SerializedProperty.EqualContents(child, end)) break;
                            if (effect.managedReferenceValue is StatusEffect)
                            {
                                StatusEffectType type = (StatusEffectType)effect
                                    .FindPropertyRelative("statusEffectType").intValue;
                                if (child.name == "damageReduction" && type != StatusEffectType.DamageReduction)
                                    continue;
                                if ((child.name == "statType" || child.name == "modifierType" ||
                                     child.name == "modifierValue") && type != StatusEffectType.StatModifier)
                                    continue;
                            }
                            EditorGUILayout.PropertyField(child, true);
                        } while (child.NextVisible(false));
                    }
                }
            }

            if (remove)
            {
                effects.DeleteArrayElementAtIndex(i);
                break;
            }
            if (moveTo != i)
            {
                effects.MoveArrayElement(i, moveTo);
                break;
            }
        }

        if (GUILayout.Button("효과 추가"))
        {
            GenericMenu menu = new();
            menu.AddItem(new GUIContent("데미지 (Damage)"), false, () => AddEffect(new DamageEffect()));
            menu.AddItem(new GUIContent("회복 (Heal)"), false, () => AddEffect(new HealEffect()));
            menu.AddItem(new GUIContent("상태 부여 (Status)"), false, () => AddEffect(new StatusEffect()));
            menu.ShowAsContext();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void AddEffect(BattleEffect effect)
    {
        if (target == null) return;

        serializedObject.Update();
        SerializedProperty effects = serializedObject.FindProperty("effects");
        int index = effects.arraySize;
        effects.arraySize++;
        SerializedProperty element = effects.GetArrayElementAtIndex(index);
        element.managedReferenceValue = effect;
        element.isExpanded = true;
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSequence()
    {
        EditorGUILayout.LabelField("실행 시퀀스 (위에서 아래로 실행)", EditorStyles.boldLabel);
        SerializedProperty sequence = serializedObject.FindProperty("sequence");
        if (sequence.arraySize == 0)
            EditorGUILayout.HelpBox("단계를 추가해야 스킬을 실행할 수 있습니다.", MessageType.Warning);
        for (int i = 0; i < sequence.arraySize; i++)
        {
            SerializedProperty step = sequence.GetArrayElementAtIndex(i);
            int moveTo = i;
            bool remove;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"{i + 1}. {StepLabel(step.managedReferenceValue)}");
                    using (new EditorGUI.DisabledScope(i == 0))
                        if (GUILayout.Button("↑", GUILayout.Width(28))) moveTo = i - 1;
                    using (new EditorGUI.DisabledScope(i == sequence.arraySize - 1))
                        if (GUILayout.Button("↓", GUILayout.Width(28))) moveTo = i + 1;
                    remove = GUILayout.Button("삭제", GUILayout.Width(45));
                }
                SerializedProperty child = step.Copy();
                SerializedProperty end = child.GetEndProperty();
                if (child.NextVisible(true))
                    while (!SerializedProperty.EqualContents(child, end))
                    {
                        if ((step.managedReferenceValue is PlayAnimationStep ||
                             step.managedReferenceValue is DashToTargetStep ||
                             step.managedReferenceValue is ReturnToOriginStep) && child.name == "animation")
                            DrawAnimationSettings(child,
                                step.managedReferenceValue is DashToTargetStep ||
                                step.managedReferenceValue is ReturnToOriginStep);
                        else if (child.name != "jumpHeight" ||
                                 step.FindPropertyRelative("moveType").intValue == (int)BattleMoveType.Parabolic)
                            EditorGUILayout.PropertyField(child, true);
                        if (!child.NextVisible(false)) break;
                    }
            }
            if (remove)
            {
                sequence.DeleteArrayElementAtIndex(i);
                break;
            }
            if (moveTo != i)
            {
                sequence.MoveArrayElement(i, moveTo);
                break;
            }
        }
        if (GUILayout.Button("시퀀스 단계 추가"))
        {
            GenericMenu menu = new();
            AddStepMenu<DashToTargetStep>(menu);
            AddStepMenu<PlayAnimationStep>(menu);
            AddStepMenu<WaitForAnimationSignalStep>(menu);
            AddStepMenu<ApplyEffectsStep>(menu);
            AddStepMenu<WaitStep>(menu);
            AddStepMenu<ReturnToOriginStep>(menu);
            menu.ShowAsContext();
        }
    }

    private static void DrawAnimationSettings(SerializedProperty animation, bool isMovement)
    {
        EditorGUILayout.PropertyField(animation.FindPropertyRelative("parameterKey"));
        SerializedProperty parameterType = animation.FindPropertyRelative("parameterType");
        EditorGUILayout.PropertyField(parameterType);
        if (parameterType.intValue == (int)ParameterType.Bool)
        {
            EditorGUILayout.PropertyField(animation.FindPropertyRelative("boolValue"));
            EditorGUILayout.PropertyField(animation.FindPropertyRelative("maintainWhileStatusId"));
        }
    }

    private static string StepLabel(object step) => step switch
    {
        DashToTargetStep => "대상 앞으로 대쉬",
        PlayAnimationStep => "스킬 애니메이션 시작",
        WaitForAnimationSignalStep => "애니메이션 신호 대기",
        ApplyEffectsStep => "전투 효과 적용",
        WaitStep => "시간 대기",
        ReturnToOriginStep => "시작 위치로 복귀",
        _ => "비어 있는 단계"
    };

    private void AddStepMenu<T>(GenericMenu menu) where T : SkillSequenceStep, new()
    {
        menu.AddItem(new GUIContent(StepLabel(new T())), false, () =>
        {
            if (target == null) return;
            serializedObject.Update();
            SerializedProperty sequence = serializedObject.FindProperty("sequence");
            int index = sequence.arraySize++;
            sequence.GetArrayElementAtIndex(index).managedReferenceValue = new T();
            serializedObject.ApplyModifiedProperties();
        });
    }
}
