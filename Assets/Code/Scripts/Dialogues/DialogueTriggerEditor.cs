using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueTrigger))]
public class DialogueTriggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DialogueTrigger dialogueTrigger = (DialogueTrigger)target;

        EditorGUILayout.Space();
        dialogueTrigger.dialogueItem = (DialogueItemSO)EditorGUILayout.ObjectField("Dialogue Item", dialogueTrigger.dialogueItem, typeof(DialogueItemSO), false);

        if (dialogueTrigger.dialogueItem != null)
        {
            EditorGUILayout.Space();
            dialogueTrigger.dialogueItem.duration = EditorGUILayout.FloatField("Duration (S)", dialogueTrigger.dialogueItem.duration);
            EditorGUILayout.Space();
            dialogueTrigger.dialogueItem.dialogueText = EditorGUILayout.TextArea(dialogueTrigger.dialogueItem.dialogueText, EditorStyles.textArea);
            EditorGUILayout.Space();
            dialogueTrigger.dialogueItem.textEaseMode = (Ease)EditorGUILayout.EnumPopup("Ease Mode",dialogueTrigger.dialogueItem.textEaseMode);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(dialogueTrigger);
            if (dialogueTrigger.dialogueItem != null)
            {
                EditorUtility.SetDirty(dialogueTrigger.dialogueItem);
            }
        }
    }
}
