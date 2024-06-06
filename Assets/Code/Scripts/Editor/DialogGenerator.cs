using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class DialogGenerator : EditorWindow
{
    [MenuItem("Dialog/Generate")]
    public static void ShowGenerator()
    {
        GetWindow<DialogGenerator>().Show();
    }


    private string source;

    private List<DialogueItemSO> dialogues = new List<DialogueItemSO>();

    public class Dialogs
    {
        public List<string> dialogues { get; set; }
    }
    private void OnGUI()
    {
        if (GUILayout.Button(new GUIContent("Generate", "Click to Generate")))
        {
            source = File.ReadAllText("F:\\Unity Projects\\Dark-away\\Assets\\Code\\Scripts\\dialogs.json");

            var dialoguesList = JsonConvert.DeserializeObject<Dialogs>(source);
            Generate(dialoguesList.dialogues);
        }



    }

    private void Generate(List<string> dialoguesList)
    {
        int fileVersion = 0;

        for (int i = 0; i < dialoguesList.Count; i++)
        {
            var d = CreateInstance<DialogueItemSO>();
            d.dialogueText = dialoguesList[i];

            string filePath = "Assets/Code/Scripts/Dialogues/Dialogue_" + fileVersion + ".asset";

            if (File.Exists(filePath))
            {
                OverrideDialog(filePath, dialoguesList[i]);
                fileVersion++;
                continue;
            }

            fileVersion++;
            AssetDatabase.CreateAsset(d, filePath);
            AssetDatabase.Refresh();
        }
    }

    private void OverrideDialog(string dialogPath, string v)
    {
        var dialogue = AssetDatabase.LoadAssetAtPath<DialogueItemSO>(dialogPath);
        dialogue.dialogueText = v;
    }
}
