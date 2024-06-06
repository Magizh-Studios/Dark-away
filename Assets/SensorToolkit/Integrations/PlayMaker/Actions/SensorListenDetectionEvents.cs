#if PLAYMAKER

using System.Collections;
using HutongGames.PlayMaker;

namespace Micosmo.SensorToolkit.PlayMaker {

    [ActionCategory("SensorToolkit")]
    [Tooltip("Subscribe to a sensors OnDetection, OnLostDetection events.")]
    public class SensorListenDetectionEvents : SensorToolkitAction<Sensor> {

        public enum EventType { NewDetection, LostDetection }

        [ActionSection("New Detection")]

        [Tooltip("Event fired when a new object was detected")]
        public FsmEvent newDetectionEvent;

        [UIHint(UIHint.Variable)]
        [Tooltip("Stores the gameobject that was newly detected")]
        public FsmGameObject storeNewDetection;

        [ActionSection("Detection Lost")]

        [Tooltip("Event fired when a detection was lost")]
        public FsmEvent lostDetectionEvent;

        [UIHint(UIHint.Variable)]
        [Tooltip("Stores the gameobject whose detection was lost")]
        public FsmGameObject storeLostDetection;

        [ActionSection("Any Detections")]

        [Tooltip("Event fired when there were previously no detections and now there is at least one detection.")]
        public FsmEvent someDetectionEvent;

        [Tooltip("Event fired when all detections are lost.")]
        public FsmEvent noDetectionEvent;

        public override void Reset() {
            base.Reset();
            newDetectionEvent = null;
            storeNewDetection = null;
            lostDetectionEvent = null;
            storeLostDetection = null;
            someDetectionEvent = null;
            noDetectionEvent = null;
        }

        public override void OnEnter() {
            if (typedSensor == null) {
                return;
            }
            typedSensor.OnDetected.AddListener(OnDetectionHandler);
            typedSensor.OnLostDetection.AddListener(DetectionLostHandler);
            typedSensor.OnSomeDetection.AddListener(OnSomeDetectionHandler);
            typedSensor.OnNoDetection.AddListener(OnNoDetectionHandler);
        }

        public override void OnExit() {
            if (typedSensor == null) {
                return;
            }
            typedSensor.OnDetected.RemoveListener(OnDetectionHandler);
            typedSensor.OnLostDetection.RemoveListener(DetectionLostHandler);
            typedSensor.OnSomeDetection.RemoveListener(OnSomeDetectionHandler);
            typedSensor.OnNoDetection.RemoveListener(OnNoDetectionHandler);
        }

        void OnDetectionHandler(UnityEngine.GameObject go, Sensor sensor) {
            if (!storeNewDetection.IsNone) {
                storeNewDetection.Value = go;
            }
            Fsm.Event(newDetectionEvent);
        }

        void DetectionLostHandler(UnityEngine.GameObject go, Sensor sensor) {
            if (!storeLostDetection.IsNone) {
                storeLostDetection.Value = go;
            }
            Fsm.Event(lostDetectionEvent);
        }

        void OnSomeDetectionHandler() {
            Fsm.Event(someDetectionEvent);
        }

        void OnNoDetectionHandler() {
            Fsm.Event(noDetectionEvent);
        }
    }
}

#endif