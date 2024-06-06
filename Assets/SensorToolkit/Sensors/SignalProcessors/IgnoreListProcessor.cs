using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    [AddComponentMenu("Sensors/Processors/Ignore List")]
    public class IgnoreListProcessor : SignalProcessor {

        [SerializeField] List<GameObject> ignoreList = new List<GameObject>();
        public List<GameObject> IgnoreList => ignoreList;

        public override bool Process(ref Signal signal, Sensor sensor) {
            return !ignoreList.Contains(signal.Object);
        }
    }

}