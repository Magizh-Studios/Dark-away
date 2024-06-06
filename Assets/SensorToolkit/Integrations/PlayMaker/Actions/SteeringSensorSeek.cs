#if PLAYMAKER

using System.Collections;
using HutongGames.PlayMaker;

namespace Micosmo.SensorToolkit.PlayMaker {

    [ActionCategory("SensorToolkit")]
    [Tooltip("Sets the target position to be seeked by the Steering Sensor. When the 'Target Distance' is set to a large number it behaves like a Flee behaviour. If the built-in locomotion is enabled the sensor will start moving to the target. Otherwise you can store the steering vector and move towards it with your own locomotion system.")]
    public class SteeringSensorSeek : SensorToolkitAction3DOr2D<SteeringSensor, SteeringSensor2D> {

        public enum DestinationMode { GameObject, Position, Direction, Wander, Stop }

        [ActionSection("Inputs")]

        [ObjectType(typeof(DestinationMode))]
        [Tooltip("How will the seek destination be specified.")]
        public FsmEnum seekMode;

        [HideIf("HideDestinationGameObject")]
        [Tooltip("The gameobject that should be moved to.")]
        public FsmGameObject destinationGameObject;

        [HideIf("HideDestinationPosition")]
        [Tooltip("The position that should be moved to.")]
        public FsmVector3 destinationPosition;

        [HideIf("HideDestinationDirection")]
        [Tooltip("The direction that should be moved towards.")]
        public FsmVector3 destinationDirection;

        [HideIf("HideTargetDistance")]
        [Tooltip("The target distance from the destination.")]
        public FsmFloat targetDistance;

        [HideIf("HideStopAtDestination")]
        [Tooltip("Stop when the destination has been reached.")]
        public bool stopAtDestination;

        [HideIf("HideStopOnExit")]
        [Tooltip("Clear the destination when the state exits.")]
        public bool stopOnExit;

        [Tooltip("Runs the action every frame.")]
        public bool everyFrame;

        [ActionSection("Outputs")]

        [UIHint(UIHint.Variable)]
        [Tooltip("Stores the Steering Vector calculated by the sensor.")]
        public FsmVector3 storeSteeringVector;

        [ActionSection("Events")]

        [Tooltip("Fires this event if the destination position has been reached.")]
        public FsmEvent destinationReachedEvent;

        DestinationMode _seekMode => (DestinationMode)seekMode.Value;
        public bool HideDestinationGameObject() => _seekMode != DestinationMode.GameObject;
        public bool HideDestinationPosition() => _seekMode != DestinationMode.Position;
        public bool HideDestinationDirection() => _seekMode != DestinationMode.Direction;
        public bool HideTargetDistance() => !(_seekMode == DestinationMode.GameObject || _seekMode == DestinationMode.Position);
        public bool HideStopAtDestination() => !(_seekMode == DestinationMode.GameObject || _seekMode == DestinationMode.Position);
        public bool HideStopOnExit() => _seekMode == DestinationMode.Stop;

        public override void Reset() {
            base.Reset();
            seekMode = DestinationMode.GameObject;
            destinationGameObject = null;
            destinationPosition = null;
            destinationDirection = null;
            targetDistance = 0f;
            stopAtDestination = true;
            stopOnExit = true;
            storeSteeringVector = null;
            destinationReachedEvent = null;
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

        public override void OnUpdate2D(SteeringSensor2D sensor) {
            OnUpdate(sensor);
        }

        public override void OnExit3D(SteeringSensor sensor) {
            if (stopOnExit && !HideStopOnExit()) {
                sensor.Stop();
            }
        }

        public override void OnExit2D(SteeringSensor2D sensor) {
            if (stopOnExit && !HideStopOnExit()) {
                sensor.Stop();
            }
        }

        public override void OnUpdate3D(SteeringSensor sensor) {
            OnUpdate(sensor);
        }

        void OnUpdate(ISteeringSensor sensor) {
            var targetDistance = !HideTargetDistance() ? this.targetDistance.Value : 0f;

            if (_seekMode == DestinationMode.GameObject) {
                if (destinationGameObject.Value != null) {
                    if (stopAtDestination) {
                        sensor.ArriveTo(destinationGameObject.Value.transform, targetDistance);
                    } else {
                        sensor.SeekTo(destinationGameObject.Value.transform, targetDistance);
                    }
                }
            } else if (_seekMode == DestinationMode.Position) {
                if (stopAtDestination) {
                    sensor.ArriveTo(destinationPosition.Value, targetDistance);
                } else {
                    sensor.SeekTo(destinationPosition.Value, targetDistance);
                }
            } else if (_seekMode == DestinationMode.Direction) {
                sensor.SeekDirection(destinationDirection.Value);
            }  else if (_seekMode == DestinationMode.Wander) {
                sensor.Wander();
            } else {
                sensor.Stop();
            }
            if (!storeSteeringVector.IsNone) {
                storeSteeringVector.Value = sensor.GetSteeringVector();
            }
            if (sensor.IsDestinationReached) {
                Fsm.Event(destinationReachedEvent);
            }
        }
    }

}

#endif