using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    [AddComponentMenu("Sensors/Processors/Ignore")]
    public class IgnoreProcessor : SignalProcessor {

        public GameObject ToIgnore;

        public override bool Process(ref Signal signal, Sensor sensor) {
            return !ReferenceEquals(signal.Object, ToIgnore);
        }

    }

}