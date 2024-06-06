using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Micosmo.SensorToolkit.Example {

    public class Convert3DTo2D : MonoBehaviour {
        public List<GameObject> ToConvert;

        public void DoConvert() {
            var all = All();
            foreach (var t in all) {
                var p = t.localPosition;
                var s = t.localScale;
                t.localPosition = new Vector3(p.x, p.z, -p.y);
                t.localScale = new Vector3(s.x, s.z, s.y);
            }
        }

        HashSet<Transform> All() {
            var set = new HashSet<Transform>();
            var list = new List<Transform>();

            foreach (var go in ToConvert) {
                list.Clear();
                Descend(go.transform, list);
                set.UnionWith(list);
            }

            return set;
        }

        void Descend(Transform node, List<Transform> list) {
            list.Add(node);
            for (var i = 0; i < node.childCount; i++) {
                Descend(node.GetChild(i), list);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Convert3DTo2D))]
    public class Convert3DTo2DEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            if (GUILayout.Button("Convert")) {
                (serializedObject.targetObject as Convert3DTo2D).DoConvert();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}