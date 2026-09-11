using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AIActionEntry))]
public sealed class AIActionEntryDrawer : PropertyDrawer
{
    private static bool ShowsHpRatio(SerializedProperty property)
    {
        SerializedProperty condition = property.FindPropertyRelative("condition");
        if (condition.hasMultipleDifferentValues) return true;

        var type = (AIActionConditionType)condition.intValue;
        return type == AIActionConditionType.HpRatioAtOrBelow ||
               type == AIActionConditionType.HpRatioAtOrAbove;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lines = property.isExpanded ? (ShowsHpRatio(property) ? 4 : 3) : 1;
        return EditorGUIUtility.singleLineHeight * lines +
               EditorGUIUtility.standardVerticalSpacing * (lines - 1);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        int previousIndent = EditorGUI.indentLevel;
        try
        {
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
            if (!property.isExpanded) return;

            EditorGUI.indentLevel++;
            DrawField(ref position, property.FindPropertyRelative("skill"));
            DrawField(ref position, property.FindPropertyRelative("condition"));
            if (ShowsHpRatio(property))
                DrawField(ref position, property.FindPropertyRelative("hpRatio"));
        }
        finally
        {
            EditorGUI.indentLevel = previousIndent;
            EditorGUI.EndProperty();
        }
    }

    private static void DrawField(ref Rect position, SerializedProperty property)
    {
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        EditorGUI.PropertyField(position, property);
    }
}
