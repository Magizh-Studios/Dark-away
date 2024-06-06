using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Micosmo.SensorToolkit.Editors {

    [CustomEditor(typeof(SteeringSensor2D))]
    [CanEditMultipleObjects]
    public class SteeringSensor2DEditor : BasePulsableEditor<SteeringSensor2D> {
        SerializedProperty resolution;
        SerializedProperty seek;
        SerializedProperty interest;
        SerializedProperty danger;
        SerializedProperty vo;
        SerializedProperty decision;
        SerializedProperty pulseMode;
        SerializedProperty pulseUpdateFunction;
        SerializedProperty pulseInterval;
        SerializedProperty locomotionMode;
        SerializedProperty rigidBody;
        SerializedProperty locomotion;

        protected SteeringSensor2D sensor;

        protected override bool canTest => true;

        protected override void OnEnable() {
            base.OnEnable();

            if (serializedObject == null) return;
            sensor = serializedObject.targetObject as SteeringSensor2D;

            resolution = serializedObject.FindProperty("resolution");
            seek = serializedObject.FindProperty("seek");
            interest = serializedObject.FindProperty("interest");
            danger = serializedObject.FindProperty("danger");
            vo = serializedObject.FindProperty("velocity");
            decision = serializedObject.FindProperty("decision");
            pulseMode = serializedObject.FindProperty("pulseRoutine.Mode");
            pulseUpdateFunction = serializedObject.FindProperty("pulseRoutine.UpdateFunction");
            pulseInterval = serializedObject.FindProperty("pulseRoutine.Interval");
            locomotionMode = serializedObject.FindProperty("LocomotionMode");
            rigidBody = serializedObject.FindProperty("RigidBody");
            locomotion = serializedObject.FindProperty("locomotion");
        }

        protected override void OnPulsableGUI() {
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(resolution);
            EditorGUILayout.PropertyField(pulseMode, new GUIContent("Pulse Mode"));
            if (sensor.PulseMode != PulseRoutine.Modes.Manual) {
                EditorGUILayout.PropertyField(pulseUpdateFunction);
            }
            if (sensor.PulseMode == PulseRoutine.Modes.FixedInterval) {
                EditorGUILayout.PropertyField(pulseInterval, new GUIContent("Pulse Interval"));
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(seek);

            EditorGUILayout.Space();

            SteeringSensor2D.ShowInterestGizmos = SteerSystemLayout("Interest", interest, SteeringSensor2D.ShowInterestGizmos);

            EditorGUILayout.Space();

            SteeringSensor2D.ShowDangerGizmos = SteerSystemLayout("Danger", danger, SteeringSensor2D.ShowDangerGizmos);

            EditorGUILayout.Space();

            SteeringSensor2D.ShowVelocityGizmos = SteerSystemLayout("Velocity", vo, SteeringSensor2D.ShowVelocityGizmos);

            EditorGUILayout.Space();

            SteeringSensor2D.ShowDecisionGizmos = SteerSystemLayout("Decision", decision, SteeringSensor2D.ShowDecisionGizmos);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(locomotionMode);

            var locmode = (LocomotionMode2D)locomotionMode.enumValueIndex;

            if (locmode != LocomotionMode2D.None) {
                if (locmode == LocomotionMode2D.RigidBody2D) {
                    EditorGUILayout.PropertyField(rigidBody);
                }
                EditorGUILayout.PropertyField(locomotion, true);
            }

            if (EditorGUI.EndChangeCheck()) {
                EditorState.StopAllTesting();
            }

            displayErrors();

            serializedObject.ApplyModifiedProperties();
        }

        void displayErrors() {

        }

        bool SteerSystemLayout(string labelText, SerializedProperty property, bool showGizmos) {
            const float gizmoWidth = 70;
            EditorGUILayout.BeginVertical(BackgroundStyle);
            var r = EditorGUILayout.BeginHorizontal();
            EditorGUI.PrefixLabel(r, new GUIContent(labelText));

            // We don't want to stop testing after clicking the checkbox to show gizmos for a steering system. Suspend the change
            // detection first.
            if (EditorGUI.EndChangeCheck()) {
                EditorState.StopAllTesting();
            }
            var prevShowGizmos = showGizmos;
            showGizmos = GUI.Toggle(new Rect(r.width - gizmoWidth, r.y, gizmoWidth, EditorGUIUtility.singleLineHeight), showGizmos, " Gizmos");
            if (IsTesting && prevShowGizmos != showGizmos) {
                SceneView.RepaintAll();
            }
            EditorGUI.BeginChangeCheck();

            r.width -= 30;
            EditorGUILayout.PropertyField(property, new GUIContent(""));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return showGizmos;
        }

        static GUIStyle backgroundStyle;
        static GUIStyle BackgroundStyle {
            get {
                if (backgroundStyle?.normal?.background == null) {
                    var texture = new Texture2D(1, 1);
                    texture.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f, 0.15f));
                    texture.Apply();
                    backgroundStyle = new GUIStyle();
                    backgroundStyle.normal.background = texture;
                }
                return backgroundStyle;
            }
        }
    }

}