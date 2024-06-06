using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    [AddComponentMenu("Sensors/Processors/Map Strength By Tag")]
    public class MapStrengthByTagProcessor : SignalProcessor {

        [System.Serializable]
        public struct Mapping {
            [TagSelector]
            public string Tag;
            public float Value;
        }
        
        public enum OperationType { Multiply, Set };

        public OperationType Operation = OperationType.Multiply;
        public List<Mapping> Tags => tags;
        [SerializeField] List<Mapping> tags = new List<Mapping>();
        public float DefaultValue = 1f;
        [Range(0, 1)] public float MinimumStrength = 0f;

        public override bool Process(ref Signal signal, Sensor sensor) {
            var value = DefaultValue;
            foreach (var map in Tags) {
                if (signal.Object.CompareTag(map.Tag)) {
                    value = map.Value;
                    break;
                }
            }
            if (Operation == OperationType.Multiply) {
                signal.Strength *= value;
            } else {
                signal.Strength = value;
            }
            return signal.Strength > MinimumStrength;
        }
    }

}