#if PLAYMAKER

using System.Collections;
using HutongGames.PlayMaker;

namespace Micosmo.SensorToolkit.PlayMaker {

    [ActionCategory("SensorToolkit")]
    [Tooltip("Exposes some configuration settings on the Steering Sensor that control its behaviour.")]
    public class SteeringSensorConfigure : SensorToolkitAction3DOr2D<SteeringSensor, SteeringSensor2D> {

        [ActionSection("Seek")]

        public bool setArrivalDistanceThreshold;
        public bool HideArrivalDistanceThreshold() => !setArrivalDistanceThreshold;
        [Tooltip("The distance from target when it is reached.")]
        [HideIf("HideArrivalDistanceThreshold")]
        public FsmFloat arrivalDistanceThreshold;

        [ActionSection("Velocity")]

        public bool setVelocity;
        public bool HideVelocity() => !setVelocity;
        [Tooltip("Speed sensor should aim for in absence of obstacles.")]
        [HideIf("HideVelocity")]
        public FsmFloat preferredSpeed;
        [Tooltip("Max speed that can be taken to avoid collision.")]
        [HideIf("HideVelocity")]
        public FsmFloat maxSpeed;

        [Tooltip("Set steering configurations each frame.")]
        public bool everyFrame;

        public override void Reset() {
            base.Reset();
            setArrivalDistanceThreshold = false;
            arrivalDistanceThreshold = 1f;
            setVelocity = false;
            preferredSpeed = 1f;
            maxSpeed = 1f;
            everyFrame = false;
        }

        public override void OnEnter3D(SteeringSensor sensor) {
            OnUpdate3D(sensor);
            if (!everyFrame) {
                Finish();
            }
        }

        public override void OnEnter2D(SteeringSensor2D sensor) {
            OnUpdate2D(sensor);
            if (!everyFrame) {
                Finish();
            }
        }

        public override void OnExit3D(SteeringSensor sensor) { }

        public override void OnExit2D(SteeringSensor2D sensor) { }

        public override void OnUpdate3D(SteeringSensor sensor) {
            OnUpdate(sensor);
        }

        public override void OnUpdate2D(SteeringSensor2D sensor) {
            OnUpdate(sensor);
        }

        void OnUpdate(ISteeringSensor sensor) {
            if (setArrivalDistanceThreshold) {
                sensor.Seek.ArriveDistanceThreshold = arrivalDistanceThreshold.Value;
            }
            if (setVelocity) {
                sensor.Velocity.PreferredSpeed = preferredSpeed.Value;
                sensor.Velocity.MaxSpeed = maxSpeed.Value;
            }
        }
    }

}

#endif