﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Micosmo.SensorToolkit.Editors {
    [CustomEditor(typeof(TriggerSensor2D))]
    [CanEditMultipleObjects]
    public class TriggerSensor2DEditor : BaseSensorEditor<TriggerSensor2D> {
        SerializedProperty ignoreList;
        SerializedProperty tagFilterEnabled;
        SerializedProperty tagFilter;
        SerializedProperty detectionMode;
        SerializedProperty signalProcessors;
        SerializedProperty runInSafeMode;
        SerializedProperty onDetected;
        SerializedProperty onLostDetection;
        SerializedProperty onSomeDetection;
        SerializedProperty onNoDetection;

        bool showEvents = false;

        protected override bool canTest { get { return false; } }

        protected override void OnEnable() {
            base.OnEnable();

            if (serializedObject == null) {
                return;
            }

            ignoreList = serializedObject.FindProperty("signalFilter.IgnoreList");
            tagFilterEnabled = serializedObject.FindProperty("signalFilter.EnableTagFilter");
            tagFilter = serializedObject.FindProperty("signalFilter.AllowedTags");
            detectionMode = serializedObject.FindProperty("DetectionMode");
            signalProcessors = serializedObject.FindProperty("signalProcessors");
            runInSafeMode = serializedObject.FindProperty("runInSafeMode");
            onDetected = serializedObject.FindProperty("OnDetected");
            onLostDetection = serializedObject.FindProperty("OnLostDetection");
            onSomeDetection = serializedObject.FindProperty("OnSomeDetection");
            onNoDetection = serializedObject.FindProperty("OnNoDetection");

            sensor.OnDetected.AddListener(detectionEventHandler);
            sensor.OnLostDetection.AddListener(detectionEventHandler);
        }

        protected override void OnDisable() {
            base.OnDisable();

            sensor.OnDetected.RemoveListener(detectionEventHandler);
            sensor.OnLostDetection.RemoveListener(detectionEventHandler);
        }

        protected override void InspectorParameters() {
            EditorGUILayout.PropertyField(ignoreList, true);
            EditorGUILayout.PropertyField(tagFilterEnabled);
            if (tagFilterEnabled.boolValue) {
                EditorGUILayout.PropertyField(tagFilter, true);
            }
            EditorGUILayout.PropertyField(detectionMode);
            EditorGUILayout.PropertyField(signalProcessors, new GUIContent("Signal Processors"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(runInSafeMode);

            EditorGUILayout.Space();

            if (showEvents = EditorGUILayout.Foldout(showEvents, "Events")) {
                EditorGUILayout.PropertyField(onDetected);
                EditorGUILayout.PropertyField(onLostDetection);
                EditorGUILayout.PropertyField(onSomeDetection);
                EditorGUILayout.PropertyField(onNoDetection);
            }

            displayErrors();
        }

        void displayErrors() {
            var showTriggerError = !checkForTriggers();
            var rb = sensor.GetComponent<Rigidbody2D>();
            var showRigidbodyError = rb == null;
            var showSleepmodeError = !showRigidbodyError && rb.sleepMode != RigidbodySleepMode2D.NeverSleep;

            if (showTriggerError || showRigidbodyError) {
                EditorGUILayout.Space();
            }
            if (showTriggerError) {
                EditorGUILayout.HelpBox("Needs active Trigger Collider to detect GameObjects!", MessageType.Warning);
            }
            if (showRigidbodyError) {
                EditorGUILayout.HelpBox("In order to detect GameObjects without RigidBodies the TriggerSensor must itself have a RigidBody! Recommend adding a kinematic RigidBody.", MessageType.Warning);
            }
            if (showSleepmodeError) {
                EditorGUILayout.HelpBox("The rigidbody which owns the trigger collider should have its 'Sleeping Mode' parameter set to 'Never Sleep'", MessageType.Warning);
            }
        }

        bool checkForTriggers() {
            var hasRB = sensor.GetComponent<Rigidbody2D>() != null;
            if (hasRB) {
                foreach (Collider2D c in sensor.GetComponentsInChildren<Collider2D>()) {
                    if (c.enabled && c.isTrigger) return true;
                }
            } else {
                foreach (Collider2D c in sensor.GetComponents<Collider2D>()) {
                    if (c.enabled && c.isTrigger) return true;
                }
            }
            return false;
        }

        void detectionEventHandler(GameObject g, Sensor s) {
            Repaint();
        }
    }

    [CustomEditor(typeof(TriggerSensor2D.Safety))]
    [CanEditMultipleObjects]
    public class TriggerSensor2DSafetyEditor : Editor {

        static string msg =
            "This component was added because you have a Trigger Sensor using the 'Run In Safe Mode' " +
            "option. It handles some quirks in Unity regarding missed trigger events because a " +
            "collider is disabled. Its not efficient to use safe mode if you plan " +
            "to use many Trigger Sensors. Please read the manual to learn how to avoid these quirks.";

        public override void OnInspectorGUI() {
            EditorGUILayout.HelpBox(msg, MessageType.Warning);
        }
    }
}