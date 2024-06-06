#if PLAYMAKER

using System.Collections;
using HutongGames.PlayMaker;

namespace Micosmo.SensorToolkit.PlayMaker {

    public abstract class SensorToolkitAction3DOr2D<T1, T2> : FsmStateAction where T1 : BasePulsableSensor where T2 : BasePulsableSensor {

        [HideIf("isSpecificSensorMode")]
        [RequiredField, DisplayOrder(0)]
        [Tooltip("The GameObject that owns the sensor.")]
        public FsmOwnerDefault gameObject;

        [HideIf("isGameObjectMode")]
        [DisplayOrder(1)]
        [ObjectType(typeof(BasePulsableSensor))]
        [Title("Specific Sensor")]
        [Tooltip("You may optionally specify the sensor to act on here. This is useful when there are multiple sensors on the same GameObject.")]
        public FsmObject sensor;

        public bool isGameObjectMode() => (gameObject.OwnerOption == OwnerDefaultOption.SpecifyGameObject) && !isSpecificSensorMode();
        public bool isSpecificSensorMode() => sensor != null && (sensor.UsesVariable || sensor.Value != null);

        ComponentCache typedSensor3DCache;
        ComponentCache typedSensor2DCache;

        protected T1 sensor3D {
            get {
                if (isSpecificSensorMode()) {
                    return sensor.Value as T1;
                }
                var owner = Fsm.GetOwnerDefaultTarget(gameObject);
                return typedSensor3DCache.GetComponent<T1>(owner);
            }
        }

        protected T2 sensor2D {
            get {
                if (isSpecificSensorMode()) {
                    return sensor.Value as T2;
                }
                var owner = Fsm.GetOwnerDefaultTarget(gameObject);
                return typedSensor2DCache.GetComponent<T2>(owner);
            }
        }

        public override void OnEnter() {
            if (sensor3D != null) {
                OnEnter3D(sensor3D);
            } else if (sensor2D != null) {
                OnEnter2D(sensor2D);
            }
        }

        public override void OnExit() {
            if (sensor3D != null) {
                OnExit3D(sensor3D);
            } else if (sensor2D != null) {
                OnExit2D(sensor2D);
            }
        }

        public override void OnUpdate() {
            if (sensor3D != null) {
                OnUpdate3D(sensor3D);
            } else if (sensor2D != null) {
                OnUpdate2D(sensor2D);
            }
        }

        public abstract void OnEnter3D(T1 sensor);
        public abstract void OnEnter2D(T2 sensor);

        public abstract void OnExit3D(T1 sensor);
        public abstract void OnExit2D(T2 sensor);

        public abstract void OnUpdate3D(T1 sensor);
        public abstract void OnUpdate2D(T2 sensor);

        public override string ErrorCheck() {
            var sensor2DIsNull = ReferenceEquals(sensor2D, null);
            var sensor3DIsNull = ReferenceEquals(sensor3D, null);
            if (!isSpecificSensorMode() && sensor3DIsNull && sensor2DIsNull && Fsm.GetOwnerDefaultTarget(gameObject) != null) {
                return $"GameObject requires a sensor matching either {typeof(T1)} or {typeof(T2)}";
            }
            if (sensor.Value != null && sensor3DIsNull && sensor2DIsNull && sensor.Value != null) {
                return $"Sensor is incompatible with this action. Must be either {typeof(T1)} or {typeof(T2)}";
            }
            return base.ErrorCheck();
        }

    }

}

#endif