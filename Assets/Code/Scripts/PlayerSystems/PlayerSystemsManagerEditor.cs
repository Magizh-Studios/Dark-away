using PlayerSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

[CustomEditor(typeof(PlayerSystemsManager))]
public class PlayerSystemsManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {

        base.OnInspectorGUI();
        PlayerSystemsManager manager = (PlayerSystemsManager)target;


        if (manager.toCheckInteractables)
            GUI.backgroundColor = Color.green;
        else
            GUI.backgroundColor = Color.red;

        if (GUILayout.Button(new GUIContent("Toggle Interatable System")))
        {
            manager.ToggleInteract();
        }


        if (manager.toCheckCollectables)
            GUI.backgroundColor = Color.green;
        else
            GUI.backgroundColor = Color.red;

        if (GUILayout.Button(new GUIContent("Toggle Collectable System")))
        {

            manager.ToggleCollect();
        }



    }
}
