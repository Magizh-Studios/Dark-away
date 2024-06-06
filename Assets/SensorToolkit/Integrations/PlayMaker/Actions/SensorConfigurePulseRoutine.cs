#if PLAYMAKER

using System.Collections;
using HutongGames.PlayMaker;

namespace Micosmo.SensorToolkit.PlayMaker {

    [ActionCategory("SensorToolkit")]
    [Tooltip("Configure how often a sensor should pulse.")]
    public class SensorConfigurePulseRoutine : SensorToolkitAction<IPulseRoutine> {

        [ObjectType(typeof(PulseRoutine.Modes))]
        public FsmEnum pulseMode;

        [HideIf("HidePulseInterval")]
        public FsmFloat pulseInterval;

        [Tooltip("Configure the pulse routine each frame.")]
        public bool everyFrame;
        PulseRoutine.Modes _pulseMode => (PulseRoutine.Modes)pulseMode.Value;

        public bool HidePulseInterval() => _pulseMode != PulseRoutine.Modes.FixedInterval;

        public override void Reset() {
            base.Reset();
            pulseMode = null;
            pulseInterval = 1;
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
            typedSensor.PulseMode = _pulseMode;
            typedSensor.PulseInterval = pulseInterval.Value;
        }
    }

}

#endif