#if PLAYMAKER

using System.Collections;
using HutongGames.PlayMaker;

namespace Micosmo.SensorToolkit.PlayMaker {

    [ActionCategory("SensorToolkit")]
    [Tooltip("If the Steering Sensor has built-in locomotion enabled this will control the strafing behaviour. The target is a direction or GameObject the agent should face while it seeks it's destination.")]
    public class SteeringSensorStrafe : SensorToolkitAction3DOr2D<SteeringSensor, SteeringSensor2D> {

        public enum TargetMode { GameObject, Direction, None }

        [ActionSection("Inputs")]

        [ObjectType(typeof(TargetMode))]
        [Tooltip("How will the strafe target be specified.")]
        public FsmEnum targetMode;

        [UIHint(UIHint.Variable)]
        [HideIf("HideTargetGameObject")]
        [Tooltip("The gameobject that should be faced.")]
        public FsmGameObject targetGameObject;

        [HideIf("HideTargetDirection")]
        [Tooltip("The position that should be faced.")]
        public FsmVector3 targetDirection;

        [HideIf("HideClearOnExit")]
        [Tooltip("Clear the strafe target on exit.")]
        public bool clearOnExit;

        [Tooltip("Runs the action every frame.")]
        public bool everyFrame;

        TargetMode _targetMode => (TargetMode)targetMode.Value;
        public bool HideTargetGameObject() => _targetMode != TargetMode.GameObject;
        public bool HideTargetDirection() => _targetMode != TargetMode.Direction;
        public bool HideClearOnExit() => _targetMode == TargetMode.None;

        public override void Reset() {
            base.Reset();
            targetMode = TargetMode.GameObject;
            targetGameObject = null;
            targetDirection = null;
            clearOnExit = true;
            everyFrame = false;
        }

        public override void OnEnter2D(SteeringSensor2D sensor) {
            OnUpdate2D(sensor);
            if (!everyFrame) {
                Finish();
            }
        }

        public override void OnEnter3D(SteeringSensor sensor) {
            OnUpdate3D(sensor);
            if (!everyFrame) {
                Finish();
            }
        }

        public override void OnExit3D(SteeringSensor sensor) {
            if (clearOnExit && !HideClearOnExit()) {
                sensor.Locomotion.Strafing.Clear();
            }
        }

        public override void OnExit2D(SteeringSensor2D sensor) {
            if (clearOnExit && !HideClearOnExit()) {
                sensor.Locomotion.Strafing.Clear();
            }
        }

        public override void OnUpdate2D(SteeringSensor2D sensor) {
            if (_targetMode == TargetMode.GameObject) {
                sensor.Locomotion.Strafing.SetFaceTarget(targetGameObject.Value?.transform);
            } else if (_targetMode == TargetMode.Direction) {
                sensor.Locomotion.Strafing.SetFaceTarget(targetDirection.Value);
            } else {
                sensor.Locomotion.Strafing.Clear();
            }
        }

        public override void OnUpdate3D(SteeringSensor sensor) {
            if (_targetMode == TargetMode.GameObject) {
                sensor.Locomotion.Strafing.SetFaceTarget(targetGameObject.Value?.transform);
            } else if (_targetMode == TargetMode.Direction) {
                sensor.Locomotion.Strafing.SetFaceTarget(targetDirection.Value);
            } else {
                sensor.Locomotion.Strafing.Clear();
            }
        }
    }

}

#endif