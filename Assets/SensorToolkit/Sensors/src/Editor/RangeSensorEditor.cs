﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Micosmo.SensorToolkit.Editors {
    [CustomEditor(typeof(RangeSensor))]
    [CanEditMultipleObjects]
    public class RangeSensorEditor : BaseSensorEditor<RangeSensor> {
        SerializedProperty shape;
        SerializedProperty sphere;
        SerializedProperty box;
        SerializedProperty capsule;
        SerializedProperty ignoreList;
        SerializedProperty tagFilterEnabled;
        SerializedProperty tagFilter;
        SerializedProperty detectsOnLayers;
        SerializedProperty pulseMode;
        SerializedProperty pulseUpdateFunction;
        SerializedProperty pulseInterval;
        SerializedProperty detectionMode;
        SerializedProperty ignoreTriggerColliders;
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

            shape = serializedObject.FindProperty("Shape");
            sphere = serializedObject.FindProperty("Sphere");
            box = serializedObject.FindProperty("Box");
            capsule = serializedObject.FindProperty("Capsule");
            ignoreList = serializedObject.FindProperty("signalFilter.IgnoreList");
            tagFilterEnabled = serializedObject.FindProperty("signalFilter.EnableTagFilter");
            tagFilter = serializedObject.FindProperty("signalFilter.AllowedTags");
            detectsOnLayers = serializedObject.FindProperty("DetectsOnLayers");
            pulseMode = serializedObject.FindProperty("pulseRoutine.Mode");
            pulseUpdateFunction = serializedObject.FindProperty("pulseRoutine.UpdateFunction");
            pulseInterval = serializedObject.FindProperty("pulseRoutine.Interval");
            detectionMode = serializedObject.FindProperty("DetectionMode");
            ignoreTriggerColliders = serializedObject.FindProperty("IgnoreTriggerColliders");
            signalProcessors = serializedObject.FindProperty("signalProcessors");
            onDetected = serializedObject.FindProperty("OnDetected");
            onLostDetection = serializedObject.FindProperty("OnLostDetection");
            onSomeDetection = serializedObject.FindProperty("OnSomeDetection");
            onNoDetection = serializedObject.FindProperty("OnNoDetection");
        }

        protected override void InspectorParameters() {
            EditorGUILayout.PropertyField(shape);
            if (sensor.Shape == RangeSensor.Shapes.Sphere) {
                EditorUtils.InlinePropertyField(sphere);
            } else if (sensor.Shape == RangeSensor.Shapes.Box) {
                EditorUtils.InlinePropertyField(box);
            } else if (sensor.Shape == RangeSensor.Shapes.Capsule) {
                EditorUtils.InlinePropertyField(capsule);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ignoreList, true);
            EditorGUILayout.PropertyField(tagFilterEnabled);
            if (tagFilterEnabled.boolValue) {
                EditorGUILayout.PropertyField(tagFilter, true);
            }
            EditorGUILayout.PropertyField(detectsOnLayers);
            EditorGUILayout.PropertyField(detectionMode);
            EditorGUILayout.PropertyField(ignoreTriggerColliders);
            EditorGUILayout.PropertyField(signalProcessors, new GUIContent("Signal Processors"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(pulseMode, new GUIContent("Pulse Mode"));
            if (sensor.PulseMode != PulseRoutine.Modes.Manual) {
                EditorGUILayout.PropertyField(pulseUpdateFunction);
            }
            if (sensor.PulseMode == PulseRoutine.Modes.FixedInterval) {
                EditorGUILayout.PropertyField(pulseInterval, new GUIContent("Pulse Interval"));
            }

            EditorGUILayout.Space();

            if (showEvents = EditorGUILayout.Foldout(showEvents, "Events")) {
                EditorGUILayout.PropertyField(onDetected);
                EditorGUILayout.PropertyField(onLostDetection);
                EditorGUILayout.PropertyField(onSomeDetection);
                EditorGUILayout.PropertyField(onNoDetection);
            }

            EditorGUILayout.Space();

            BufferSizeInfo(sensor.CurrentBufferSize);
        }
    }
}