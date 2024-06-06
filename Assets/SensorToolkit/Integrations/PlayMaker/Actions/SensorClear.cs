#if PLAYMAKER

using System.Collections;
using HutongGames.PlayMaker;

namespace Micosmo.SensorToolkit.PlayMaker {

    [ActionCategory("SensorToolkit")]
    [Tooltip("Clears the sensor of its detections.")]
    public class SensorClear : SensorToolkitAction<BasePulsableSensor> {

        [Tooltip("Pulse sensor each frame.")]
        public bool everyFrame;

        public override void Reset() {
            base.Reset();
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
            typedSensor.Clear();
        }

    }

}

#endif