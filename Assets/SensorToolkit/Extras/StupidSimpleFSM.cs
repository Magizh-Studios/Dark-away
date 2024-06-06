using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Micosmo.SensorToolkit.Extras {
    
    public abstract class StupidSimpleFSM : MonoBehaviour {
        public string CurrentStateName => GetNiceStateName(currentState);
        public string PreviousStateName => GetNiceStateName(prevState);

        static Regex rxNiceName = new Regex(@"^<(.*)>.*$", RegexOptions.Compiled);
        string GetNiceStateName(IEnumerator state) {
            if (state == null) {
                return "None";
            }
            return rxNiceName.Match(state.GetType().Name).Groups[1].Value;
        }

        IEnumerator currentState;
        IEnumerator prevState;

        protected virtual void OnDisable() {
            currentState = null;
            prevState = null;
        }

        public Coroutine SetFSMState(IEnumerator nextState) {
            if (currentState?.GetType() == nextState?.GetType()) {
                return null;
            }
            if (currentState != null) {
                StopCoroutine(currentState);
            }
            prevState = currentState;
            currentState = nextState;
            return StartCoroutine(currentState);
        }
    }
}

#if UNITY_EDITOR
namespace Micosmo.SensorToolkit.Extras.Editors {
    [CustomEditor(typeof(StupidSimpleFSM), true)]
    [CanEditMultipleObjects]
    public class StupidSimpleFSMEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            var fsm = target as StupidSimpleFSM;

            if (Application.isPlaying && fsm != null) {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("FSM Runtime", EditorStyles.boldLabel);
                EditorGUILayout.TextField("Current State", fsm.CurrentStateName);
                EditorGUILayout.TextField("Previous State", fsm.PreviousStateName);
            }
        }
    }
}
#endif