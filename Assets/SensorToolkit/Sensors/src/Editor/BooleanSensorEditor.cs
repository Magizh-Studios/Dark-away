using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Micosmo.SensorToolkit.Editors {

    [CustomEditor(typeof(BooleanSensor))]
    [CanEditMultipleObjects]
    public class BooleanSensorEditor : BaseSensorEditor<BooleanSensor> {
        SerializedProperty inputSensors;
        SerializedProperty operation;
        SerializedProperty signalProcessors;
        SerializedProperty onDetected;
        SerializedProperty onLostDetection;
        SerializedProperty onSomeDetection;
        SerializedProperty onNoDetection;

        bool showEvents = false;

        protected override bool canTest { get { return true; } }

        protected override void OnEnable() {
            base.OnEnable();

            if (serializedObject == null) {
                return;
            }

            inputSensors = serializedObject.FindProperty("InputSensors");
            operation = serializedObject.FindProperty("operation");
            signalProcessors = serializedObject.FindProperty("signalProcessors");
            onDetected = serializedObject.FindProperty("OnDetected");
            onLostDetection = serializedObject.FindProperty("OnLostDetection");
            onSomeDetection = serializedObject.FindProperty("OnSomeDetection");
            onNoDetection = serializedObject.FindProperty("OnNoDetection");
        }

        protected override void InspectorParameters() {
            EditorGUILayout.PropertyField(inputSensors, true);

            EditorGUILayout.PropertyField(operation);
            EditorGUILayout.PropertyField(signalProcessors, new GUIContent("Signal Processors"));

            EditorGUILayout.Space();

            if (showEvents = EditorGUILayout.Foldout(showEvents, "Events")) {
                EditorGUILayout.PropertyField(onDetected);
                EditorGUILayout.PropertyField(onLostDetection);
                EditorGUILayout.PropertyField(onSomeDetection);
                EditorGUILayout.PropertyField(onNoDetection);
            }
        }
    }

}