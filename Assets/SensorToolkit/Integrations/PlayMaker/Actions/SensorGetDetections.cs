#if PLAYMAKER

using System.Linq;
using System.Collections;
using HutongGames.PlayMaker;

namespace Micosmo.SensorToolkit.PlayMaker {

    [ActionCategory("SensorToolkit")]
    [Tooltip("Queries the sensor for the GameObjects it detects. There's a handful of query types including: 'All' to get all detected objects and 'Nearest' to get only the nearest detected object by distance. The query can be fine-tuned to return objects with specific tags, or objects that have specific components.")]
    public class SensorGetDetections : SensorToolkitAction<Sensor> {

        public enum QueryType { All, ByDistance, ByDistanceToPoint, Nearest, NearestToPoint }

        [ActionSection("Inputs")]

        [ObjectType(typeof(QueryType))]
        public FsmEnum queryType;

        [Tooltip("Find only detected objects with this tag.")]
        public FsmString tag;

        [HideIf("hideTestPoint")]
        [Tooltip("Order detections by distance to this point.")]
        public FsmVector3 testPoint;

        [Tooltip("Set steering configurations each frame.")]
        public bool everyFrame;

        [ActionSection("Outputs")]

        [HideIf("isSingleResult")]
        [UIHint(UIHint.Variable)]
        [Tooltip("Stores the number of detections matching the query.")]
        public FsmInt storeDetectionCount;

        [HideIf("isSingleResult")]
        [UIHint(UIHint.Variable)]
        [ArrayEditor(VariableType.GameObject)]
        [Tooltip("Stores GameObjects detected by the sensor, if there is one.")]
        public FsmArray storeAllDetected;

        [HideIf("isSingleResult")]
        [UIHint(UIHint.Variable)]
        [ArrayEditor(VariableType.Object)]
        [Tooltip("Detections must have matching component. Store all the components here.")]
        public FsmArray storeAllComponents;

        [ActionSection("Outputs")]

        [HideIf("isArrayResult")]
        [UIHint(UIHint.Variable)]
        [Tooltip("Stores all GameObjects detected by the sensor.")]
        public FsmGameObject storeDetected;

        [HideIf("isArrayResult")]
        [UIHint(UIHint.Variable)]
        [ObjectType(typeof(UnityEngine.Component))]
        [Tooltip("Detections must have matching component. Store the component here.")]
        public FsmObject storeComponent;

        [ActionSection("Events")]
        [Tooltip("Fires this event if there is at least one detected GameObject that matches the search filters.")]
        public FsmEvent detectedEvent;
        [Tooltip("Fires this event if no GameObject is detected that matches the search filters.")]
        public FsmEvent noneDetectedEvent;

        QueryType _queryType => (QueryType)queryType.Value;

        string _tag => tag.Value;
        bool useTag => !string.IsNullOrEmpty(_tag);

        System.Type componentType => isSingleResult() ? storeComponent.ObjectType : storeAllComponents.ObjectType;
        bool useComponent => isSingleResult() ? !storeComponent.IsNone : !storeAllComponents.IsNone;

        public bool isArrayResult() => _queryType != QueryType.Nearest && _queryType != QueryType.NearestToPoint;
        public bool isSingleResult() => !isArrayResult();
        public bool hideTestPoint() => !(_queryType == QueryType.ByDistanceToPoint || _queryType == QueryType.NearestToPoint);

        public override void Reset() {
            base.Reset();
            queryType = QueryType.All;
            storeDetectionCount = null;
            storeAllComponents = null;
            storeAllDetected = null;
            storeComponent = null;
            storeDetected = null;
            testPoint = null;
            tag = null;
            detectedEvent = null;
            noneDetectedEvent = null;
            everyFrame = false;
        }

        public override void OnEnter() {
            DoAction();
            if (!everyFrame) {
                Finish();
            }
        }

        public override void OnUpdate() {
            DoAction();
        }

        void DoAction() {
            if (typedSensor == null) {
                return;
            }
            var isSomethingDetected = switchAction();
            if (isSomethingDetected) {
                Fsm.Event(detectedEvent);
            } else {
                Fsm.Event(noneDetectedEvent);
            }
        }

        bool switchAction() {
            switch(_queryType) {
                case QueryType.All:
                    return DoAll();
                case QueryType.ByDistance:
                    return DoByDistance();
                case QueryType.ByDistanceToPoint:
                    return DoByDistanceToPoint();
                case QueryType.Nearest:
                    return DoNearest();
                case QueryType.NearestToPoint:
                    return DoNearestToPoint();
                default:
                    return false;
            }
        }

        bool DoAll() {
            if (useComponent) {
                var components = useTag
                    ? typedSensor.GetDetectedComponents(componentType, _tag).ToArray()
                    : typedSensor.GetDetectedComponents(componentType).ToArray();
                storeAllComponents.Values = components;
                storeAllDetected.Values = components.Select(c => c.gameObject).ToArray();
            } else {
                storeAllDetected.Values = useTag
                    ? typedSensor.GetDetections(_tag).ToArray()
                    : typedSensor.GetDetections().ToArray();
            }
            if (!storeDetectionCount.IsNone) {
                storeDetectionCount.Value = storeAllDetected.Values.Length;
            }
            return storeAllDetected.Values.Length > 0;
        }

        bool DoByDistance() {
            if (useComponent) {
                var components = useTag
                    ? typedSensor.GetDetectedComponentsByDistance(componentType, _tag).ToArray()
                    : typedSensor.GetDetectedComponentsByDistance(componentType).ToArray();
                storeAllComponents.Values = components;
                storeAllDetected.Values = components.Select(c => c.gameObject).ToArray();
            } else {
                storeAllDetected.Values = useTag
                    ? typedSensor.GetDetectionsByDistance(_tag).ToArray()
                    : typedSensor.GetDetectionsByDistance().ToArray();
            }
            if (!storeDetectionCount.IsNone) {
                storeDetectionCount.Value = storeAllDetected.Values.Length;
            }
            return storeAllDetected.Values.Length > 0;
        }

        bool DoByDistanceToPoint() {
            if (useComponent) {
                var components = useTag
                    ? typedSensor.GetDetectedComponentsByDistanceToPoint(testPoint.Value, componentType, _tag).ToArray()
                    : typedSensor.GetDetectedComponentsByDistanceToPoint(testPoint.Value, componentType).ToArray();
                storeAllComponents.Values = components;
                storeAllDetected.Values = components.Select(c => c.gameObject).ToArray();
            } else {
                storeAllDetected.Values = useTag
                    ? typedSensor.GetDetectionsByDistanceToPoint(testPoint.Value, _tag).ToArray()
                    : typedSensor.GetDetectionsByDistanceToPoint(testPoint.Value).ToArray();
            }
            if (!storeDetectionCount.IsNone) {
                storeDetectionCount.Value = storeAllDetected.Values.Length;
            }
            return storeAllDetected.Values.Length > 0;
        }

        bool DoNearest() {
            if (useComponent) {
                var component = useTag 
                    ? typedSensor.GetNearestComponent(componentType, _tag) 
                    : typedSensor.GetNearestComponent(componentType);
                storeComponent.Value = component;
                storeDetected.Value = component != null ? component.gameObject : null;
            } else {
                storeDetected.Value = useTag
                    ? typedSensor.GetNearestDetection(_tag)
                    : typedSensor.GetNearestDetection();
            }
            return storeDetected.Value != null;
        }

        bool DoNearestToPoint() {
            if (useComponent) {
                var component = useTag
                    ? typedSensor.GetNearestComponentToPoint(testPoint.Value, componentType, _tag)
                    : typedSensor.GetNearestComponentToPoint(testPoint.Value, componentType);
                storeComponent.Value = component;
                storeDetected.Value = component != null ? component.gameObject : null;
            } else {
                storeDetected.Value = useTag
                    ? typedSensor.GetNearestDetectionToPoint(testPoint.Value, _tag)
                    : typedSensor.GetNearestDetectionToPoint(testPoint.Value);
            }
            return storeDetected.Value != null;
        }
    }

}

#endif