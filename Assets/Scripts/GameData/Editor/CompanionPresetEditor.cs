using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CompanionPreset))]
public sealed class CompanionPresetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var preset = (CompanionPreset)target;
        long minimumTotal = 0;
        long maximumTotal = 0;
        if (preset.statRanges != null)
        {
            foreach (var range in preset.statRanges)
            {
                if (range == null) continue;
                minimumTotal += range.minimum;
                maximumTotal += range.maximum;
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("능력치 범위 합계", EditorStyles.boldLabel);
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.LongField("최솟값 총합", minimumTotal);
            EditorGUILayout.LongField("최댓값 총합", maximumTotal);
        }
    }
}
