﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Micosmo.SensorToolkit
{
    /*
     * The Trigger Sensor detects objects that intersect a Trigger Collider. It works by listening 
     * for the events OnTriggerEnter and OnTriggerExit. The sensor has a similar role as the 
     * Range Sensor, with some unique advantages. The downside is that its more difficult to configure. 
     * There are some subtle complexities to Trigger Colliders in Unity that must be considered when 
     * using this sensor.
     */
    [AddComponentMenu("Sensors/2D Trigger Sensor")]
    [HelpURL("https://micosmo.com/sensortoolkit2/docs/manual/sensors/trigger")]
    public class TriggerSensor2D : BaseAreaSensor {

        #region Configurations
        [SerializeField]
        ObservableBool runInSafeMode = new ObservableBool();
        #endregion

        #region Events
#pragma warning disable
        public override event Action OnPulsed;
        #endregion

        #region Public
        // Change RunInSafeMode at runtime
        public bool RunInSafeMode {
            get => runInSafeMode.Value;
            set => runInSafeMode.Value = value;
        }

        public override void PulseAll() => Pulse();

        public override void Clear() {
            base.Clear();
            colliderCount.Clear();
            OnCleared?.Invoke();
        }
        #endregion

        #region Internals
        event Action OnCleared;

        Dictionary<Collider2D, int> colliderCount = new Dictionary<Collider2D, int>();
        Safety safety;

        // Not necessary to call Pulse on the TriggerSensor.
        protected override PulseJob GetPulseJob() {
            UpdateAllSignals();
            return default;
        }

        protected override void Awake() {
            base.Awake();

            if (runInSafeMode == null) {
                runInSafeMode = new ObservableBool();
            }

            runInSafeMode.OnChanged += RunInSafeModeChangedHandler;
            RunInSafeModeChangedHandler();
        }

        void OnDestroy() {
            runInSafeMode.OnChanged -= RunInSafeModeChangedHandler;
            if (safety != null) {
                Destroy(safety);
            }
        }

        void OnValidate() {
            if (runInSafeMode != null) {
                runInSafeMode.OnValidate();
            }
        }

        void RunInSafeModeChangedHandler() {
            if (RunInSafeMode && safety == null) {
                safety = gameObject.AddComponent<Safety>();
                safety.TriggerSensor = this;
            } else if (!RunInSafeMode && safety != null) {
                Destroy(safety);
                safety = null;
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            int currCount;
            if (!colliderCount.TryGetValue(other, out currCount)) {
                AddCollider(other, true);
                currCount = 0;
            }
            colliderCount[other] = currCount + 1;
        }

        void OnTriggerExit2D(Collider2D other) {
            int currCount;
            if (colliderCount.TryGetValue(other, out currCount)) {
                if (currCount == 1) {
                    colliderCount.Remove(other);
                    RemoveCollider(other, true);
                } else {
                    colliderCount[other] = currCount - 1;
                }
            }
        }
        #endregion

        #region Safety Implementation
        public class Safety : MonoBehaviour {
            TriggerSensor2D triggerSensor;
            public TriggerSensor2D TriggerSensor {
                set {
                    if (!ReferenceEquals(triggerSensor, value)) {
                        if (triggerSensor != null) {
                            triggerSensor.OnCleared -= ClearedHandler;
                        }
                        triggerSensor = value;
                        triggerStayTests.Clear();
                        if (triggerSensor != null) {
                            foreach (var colliderCount in triggerSensor.colliderCount) {
                                triggerStayTests[colliderCount.Key] = colliderCount.Value;
                            }
                            triggerSensor.OnCleared += ClearedHandler;
                        }
                    }
                }
                get {
                    return triggerSensor;
                }
            }

            Dictionary<Collider2D, int> triggerStayTests = new Dictionary<Collider2D, int>();
            bool didPhysicsRun = false;

            void FixedUpdate() {
                triggerStayTests.Clear();
                didPhysicsRun = true;
            }

            void OnTriggerStay2D(Collider2D other) {
                int currCount;
                if (!triggerStayTests.TryGetValue(other, out currCount)) {
                    currCount = 0;
                }
                triggerStayTests[other] = currCount + 1;
            }

            List<Collider2D> removeList = new List<Collider2D>();
            void Update() {
                if (!didPhysicsRun) {
                    return;
                }
                didPhysicsRun = false;

                removeList.Clear();
                foreach (var test in triggerStayTests) {
                    var collider = test.Key;
                    var count = test.Value;
                    int sensorCount;
                    if (!triggerSensor.colliderCount.TryGetValue(collider, out sensorCount)) {
                        sensorCount = 0;
                    }
                    for (int i = count; i > sensorCount; i--) {
                        triggerSensor.OnTriggerEnter2D(collider);
                    }
                }
                foreach (var colliderCount in triggerSensor.colliderCount) {
                    var collider = colliderCount.Key;
                    var sensorCount = colliderCount.Value;
                    int count;
                    if (!triggerStayTests.TryGetValue(collider, out count)) {
                        count = 0;
                    }
                    for (int i = count; i < sensorCount; i++) {
                        removeList.Add(collider);
                    }
                }
                foreach (var collider in removeList) {
                    triggerSensor.OnTriggerExit2D(collider);
                }
            }

            void ClearedHandler() {
                triggerStayTests.Clear();
            }
        }
        #endregion
    }
}
