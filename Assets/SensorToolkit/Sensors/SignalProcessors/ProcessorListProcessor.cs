using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    [AddComponentMenu("Sensors/Processors/Processor List")]
    public class ProcessorListProcessor : SignalProcessor {

        [SerializeField] List<SignalProcessor> processors = new List<SignalProcessor>();
        public List<SignalProcessor> Processors => processors;

        public override bool Process(ref Signal signal, Sensor sensor) {
            foreach (var processor in Processors) {
                if (!processor.Process(ref signal, sensor)) {
                    return false;
                }
            }
            return true;
        }
    }

}