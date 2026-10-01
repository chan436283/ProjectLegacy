using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleContent))]
public sealed class BattleContentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var content = (BattleContent)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("그룹 등장 확률", EditorStyles.boldLabel);
        try
        {
            content.Validate();
        }
        catch (ArgumentException exception)
        {
            EditorGUILayout.HelpBox(exception.Message, MessageType.Warning);
            return;
        }

        double total = 0;
        foreach (var entry in content.enemyGroups) total += entry.weight;
        foreach (var entry in content.enemyGroups)
        {
            double percent = entry.weight / total * 100;
            EditorGUILayout.LabelField(entry.group.groupId,
                entry.weight == 0 ? "0% (추첨 제외)" : percent.ToString("G6") + "%");
        }
    }
}
