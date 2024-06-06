using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Micosmo.SensorToolkit.Example {
    public class Comment : MonoBehaviour {
        [TextArea(3,40)]
        public string comment;
    }
}

#if UNITY_EDITOR
namespace Micosmo.SensorToolkit.Editors {

    [CustomEditor(typeof(Example.Comment))]
    public class CommentEditor : Editor {

        SerializedProperty text;

        void OnEnable() {
            text = serializedObject.FindProperty("comment");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(text);

            serializedObject.ApplyModifiedProperties();
        }

    }

}
#endif