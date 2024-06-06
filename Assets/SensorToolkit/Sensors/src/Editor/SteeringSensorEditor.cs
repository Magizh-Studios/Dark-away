using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Micosmo.SensorToolkit.Editors {

    [CustomEditor(typeof(SteeringSensor))]
    [CanEditMultipleObjects]
    public class SteeringSensorEditor : BasePulsableEditor<SteeringSensor> {
        SerializedProperty isSpherical;
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
        SerializedProperty characterController;
        SerializedProperty locomotion;

        protected SteeringSensor sensor;

        protected override bool canTest => true;

        protected override void OnEnable() {
            base.OnEnable();

            if (serializedObject == null) return;
            sensor = serializedObject.targetObject as SteeringSensor;

            isSpherical = serializedObject.FindProperty("isSpherical");
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
            characterController = serializedObject.FindProperty("CharacterController");
            locomotion = serializedObject.FindProperty("locomotion");
        }

        protected override void OnPulsableGUI() {
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(isSpherical);
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

            SteeringSensor.ShowInterestGizmos = SteerSystemLayout("Interest", interest, SteeringSensor.ShowInterestGizmos);

            EditorGUILayout.Space();

            SteeringSensor.ShowDangerGizmos = SteerSystemLayout("Danger", danger, SteeringSensor.ShowDangerGizmos);

            EditorGUILayout.Space();

            SteeringSensor.ShowVelocityGizmos = SteerSystemLayout("Velocity", vo, SteeringSensor.ShowVelocityGizmos);

            EditorGUILayout.Space();

            SteeringSensor.ShowDecisionGizmos = SteerSystemLayout("Decision", decision, SteeringSensor.ShowDecisionGizmos);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(locomotionMode);

            var locmode = (LocomotionMode)locomotionMode.enumValueIndex;

            if (locmode != LocomotionMode.None) {
                if (locmode == LocomotionMode.RigidBodyFlying || locmode == LocomotionMode.RigidBodyCharacter) {
                    EditorGUILayout.PropertyField(rigidBody);
                } else {
                    EditorGUILayout.PropertyField(characterController);
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