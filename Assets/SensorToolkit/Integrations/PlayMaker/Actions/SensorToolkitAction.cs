#if PLAYMAKER

using System.Collections;
using HutongGames.PlayMaker;

namespace Micosmo.SensorToolkit.PlayMaker {

    public abstract class SensorToolkitAction<T> : FsmStateAction where T : class {

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

        ComponentCache typedSensorCache;
        
        protected T typedSensor {
            get {
                if (isSpecificSensorMode()) {
                    return sensor.Value as T;
                }
                var owner = Fsm.GetOwnerDefaultTarget(gameObject);
                return typedSensorCache.GetComponent<T>(owner);
            } 
        }

        public override string ErrorCheck() {
            var sensorIsNull = ReferenceEquals(typedSensor, null);
            if (!isSpecificSensorMode() && sensorIsNull && Fsm.GetOwnerDefaultTarget(gameObject) != null) {
                return $"GameObject requires a sensor matching {typeof(T)}";
            }
            if (sensor.Value != null && sensorIsNull && sensor.Value != null) {
                return $"Sensor is incompatible with this action. Must be {typeof(T)}";
            }
            return base.ErrorCheck();
        }

    }

}

#endif