using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleSkill))]
public sealed class BattleSkillEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script", "effects");

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
}
