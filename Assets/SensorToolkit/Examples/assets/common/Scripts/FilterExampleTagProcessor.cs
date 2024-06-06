using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit.Example {

    public class FilterExampleTagProcessor : SignalProcessor {

        public string[] AllowedTags;

        public override bool Process(ref Signal signal, Sensor sensor) {
            if (signal.Object != null) {
                var tag = signal.Object.GetComponent<ExampleTag>();
                if (tag != null) {
                    foreach (var allowedTag in AllowedTags) {
                        if (tag.Tag == allowedTag) {
                            return true;
                        }
                    }
                    return false;
                }
            }
            return false;
        }

    }

}