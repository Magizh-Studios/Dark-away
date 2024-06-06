using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    public abstract class BasePulsableSensor : MonoBehaviour {

        PulseHandle runningPulse;
        PulseHandle pendingPulse;

        // Should cause the sensor to perform it's 'sensing' routine, so that its list of detected objects
        // is up to date at the time of calling. Each sensor can be configured to pulse automatically at
        // fixed intervals or each timestep, however, if you need more control over when this occurs then
        // you can call this method manually.
        public void Pulse() {
            if (!isActiveAndEnabled) {
                return;
            }
            if (!runningPulse.TryCancel()) {
                runningPulse.Complete();
            }
            if (!pendingPulse.TryCancel()) {
                pendingPulse.Complete();
            }
            GetPulseJob().Run();
        }

        // If this sensor has input sensors, then the inputs are pulsed first and then this one is pulsed.
        public abstract void PulseAll();

        public PulseHandle SchedulePulse() {
            if (!isActiveAndEnabled) {
                return default;
            }
            if (runningPulse.IsCompleted && !pendingPulse.IsCompleted) {
                runningPulse = pendingPulse;
                pendingPulse = default;
            }
            
            if (!runningPulse.IsCompleted) {
                if (!pendingPulse.IsCompleted) {
                    return pendingPulse;
                }
                pendingPulse = GetPulseJob().Schedule(runningPulse);
                pendingPulse.Tick();
                return pendingPulse;
            }

            runningPulse = GetPulseJob().Schedule();
            runningPulse.Tick();
            return runningPulse;
        }

        public abstract event System.Action OnPulsed;

        public abstract void Clear();

        public bool ShowDetectionGizmos { get; set; }

        protected abstract PulseJob GetPulseJob();

        protected void ClearPendingPulse() {
            if (!runningPulse.TryCancel()) {
                runningPulse.Complete();
            }
            if (!pendingPulse.TryCancel()) {
                pendingPulse.Complete();
            }
        }

        protected virtual void OnDisable() {
            ClearPendingPulse();
        }

    }

}