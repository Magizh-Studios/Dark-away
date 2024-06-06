using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    [AddComponentMenu("Sensors/Processors/Map Strength By Distance")]
    public class MapStrengthByDistanceProcessor : SignalProcessor {

        public enum OperationType { Multiply, Set };

        public OperationType Operation = OperationType.Multiply;
        public RadialInterpolation RadialRange = new RadialInterpolation(0f, 20f);
        [Range(0,1)] public float MinimumStrength = 0f;
        
        public override bool Process(ref Signal signal, Sensor sensor) {
            var distance = Mathf.Sqrt(signal.Bounds.SqrDistance(sensor.transform.position));
            var t = RadialRange.Calculate(distance);
            if (Operation == OperationType.Multiply) {
                signal.Strength *= t;
            } else {
                signal.Strength = t;
            }
            return signal.Strength > MinimumStrength;
        }
    }

}