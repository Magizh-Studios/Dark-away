using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {
    public class STPrefs : ScriptableObject {

        public static Color defaultCyan = new Color(51 / 255f, 1f, 1f);
        static Color[] defaultVisibilityGradientColours = new Color[] {
            new Color(.2f, 1f, 1f),
            new Color(.21f, 1f, .74f),
            new Color(.21f, 1f, .47f),
            new Color(.22f, 1f, .22f),
            new Color(.48f, 1f, .23f),
            new Color(.75f, 1f, .23f),
            new Color(1f, 1f, .24f),
            new Color(1f, .75f, .25f),
            new Color(1f, .5f, .25f),
            new Color(1f, .26f, .26f)
        };

        public static Color RedEditorTextColour => Instance?.redEditorTextColour ?? new Color(1f, .2f, .2f);
        public static Color ActiveSensorEditorColour => Instance?.activeSensorEditorColour ?? new Color(51 / 255f, 1f, 1f, .4f);
        public static Color SignalBoundsColour => Instance?.signalBoundsColour ?? defaultCyan;
        public static bool ShowEyeIconInSignal => Instance?.showEyeIconInSignal ?? true;
        public static Color RangeColour => Instance?.rangeColour ?? defaultCyan;
        public static Color CastingRayColour => Instance?.castingRayColour ?? defaultCyan;
        public static Color CastingBlockedRayColour => Instance?.castingBlockedRayColour ?? Color.red;
        public static Color CastingShapeColour => Instance?.castingShapeColour ?? Color.green;
        public static Color RayHitNormalColour => Instance?.rayHitNormalColour ?? Color.yellow;
        public static Color LOSFovColour => Instance?.losFovColour ?? Color.yellow;
        public static Color[] RayVisibilityGradient => Instance?.rayVisibilityGradient ?? defaultVisibilityGradientColours;
        public static Color LOSRayBlockedColour => Instance?.losRayBlockedColour ?? Color.red;
        public static Color AvoidColour => Instance?.avoidColour ?? Color.red;
        public static Color SeekColour => Instance?.seekColour ?? Color.yellow;
        public static Color SteeringVectorColour => Instance?.steeringVectorColour ?? Color.green;

        [Header("Sensor Editors")]
        [SerializeField] Color redEditorTextColour = new Color(1f, .2f, .2f);
        [SerializeField] Color activeSensorEditorColour = new Color(51 / 255f, 1f, 1f, .4f);

        [Header("Detected Signal Widgets")]
        [SerializeField] Color signalBoundsColour = defaultCyan;
        [SerializeField] bool showEyeIconInSignal = true;

        [Header("Range Sensor Widgets")]
        [SerializeField] Color rangeColour = defaultCyan;

        [Header("Casting Sensor Widgets")]
        [SerializeField] Color castingRayColour = defaultCyan;
        [SerializeField] Color castingBlockedRayColour = Color.red;
        [SerializeField] Color castingShapeColour = Color.green;
        [SerializeField] Color rayHitNormalColour = Color.yellow;

        [Header("LOS Sensor Widgets")]
        [SerializeField] Color losFovColour = Color.yellow;
        [SerializeField] Color[] rayVisibilityGradient = defaultVisibilityGradientColours;
        [SerializeField] Color losRayBlockedColour = Color.red;

        [Header("Steering Sensor Widgets")]
        [SerializeField] Color avoidColour = Color.red;
        [SerializeField] Color seekColour = Color.yellow;
        [SerializeField] Color steeringVectorColour = Color.green;

        static STPrefs instance;
        static STPrefs Instance {
            get {
#if UNITY_EDITOR
                if (instance == null) {
                    var instances = Resources.FindObjectsOfTypeAll<STPrefs>();
                    if (instances.Length > 1) {
                        for (var i = 1; i < instances.Length; i++) {
                            Debug.LogError("Duplicate SensorToolkit preferences", instances[i]);
                        }
                    }
                    instance = instances.Length > 0 ? instances[0] : null;
                }
                return instance;
#else
                return null;
#endif
            }
        }
    }
}