using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class PathNodeManager : MonoBehaviour {
    public bool DEBUG_PATH_NODES = true;
    private List<PathNode> nodes;

    private void Awake() {
        InitializePathNodes();
    }

    private void InitializePathNodes() {
        nodes = new List<PathNode>(FindObjectsOfType<PathNode>());
    }

    public void Revaluate() {
        InitializePathNodes();
    }

    private void OnDrawGizmos() {

        if (!DEBUG_PATH_NODES) return;

        foreach (PathNode pathNode in nodes) {
            switch (pathNode.NodeType) {
                case NodeType.OutSide:
                    Gizmos.color = Color.white;
                    break;
                case NodeType.Inside:
                    Gizmos.color = Color.magenta;
                    break;
                case NodeType.RoofTop:
                    Gizmos.color = Color.blue;
                    break;
            }

            Gizmos.DrawSphere(pathNode.transform.position, 0.3f);

            // Draw indication for inspected area
            if (pathNode.InspectedArea) {
                Gizmos.color = Color.green; // Color for the wireframe cube
                Gizmos.DrawWireSphere(pathNode.transform.position, 0.3f);

                // Draw a label text
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.green;
                Handles.Label(pathNode.transform.position + Vector3.up * 0.5f, "Inspected", style);
            }
        }
    }
}

[CustomEditor(typeof(PathNodeManager))]
public class PathNodeManagerEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        PathNodeManager pathNodeManager = (PathNodeManager)target;

        if (GUILayout.Button("Revaluate")) {
            pathNodeManager.Revaluate();
        }
    }
}
