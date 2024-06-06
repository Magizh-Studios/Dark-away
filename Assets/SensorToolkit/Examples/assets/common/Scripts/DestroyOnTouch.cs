using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit.Example {

    public class DestroyOnTouch : MonoBehaviour {

        public Sensor TouchSensor;

        void OnEnable() {
            TouchSensor.OnDetected.AddListener(OnDetectionHandler);
        }

        void OnDisable() {
            TouchSensor.OnDetected.RemoveListener(OnDetectionHandler);
        }

        void OnDetectionHandler(GameObject detectedObject, Sensor sensor) {
            detectedObject.transform.position = new Vector3(10000, 10000, 10000); // In case were using a TriggerSensor. See manual why is this recomended
            Destroy(detectedObject);
        }

    }

}