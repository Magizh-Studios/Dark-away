#if PLAYMAKER

using System.Collections;
using HutongGames.PlayMaker;

namespace Micosmo.SensorToolkit.PlayMaker {

    [ActionCategory("SensorToolkit")]
    [Tooltip("Manually pulses the sensor.")]
    public class SensorPulse : SensorToolkitAction<BasePulsableSensor> {

        [Tooltip("Also pulse any input sensors.")]
        public bool pulseInputs;

        [Tooltip("Pulse sensor each frame.")]
        public bool everyFrame;

        public override void Reset() {
            base.Reset();
            pulseInputs = false;
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
            if (pulseInputs) {
                typedSensor.PulseAll();
            } else {
                typedSensor.Pulse();
            }
        }
    }

}

#endif