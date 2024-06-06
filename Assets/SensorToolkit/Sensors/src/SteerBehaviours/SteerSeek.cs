using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    public enum SeekMode { Position, Direction, Wander, Stop }

    [System.Serializable]
    public struct SeekPosition {
        public enum DestinationTypes { Transform, Point }

        [Tooltip("Choose if the destination is a transform or a worldspace point.")]
        public DestinationTypes DestinationType;
        
        [DrawIf("DestinationType", DestinationTypes.Transform)]
        [Tooltip("The transform to seek to when the DestinationType is Transform.")]
        public Transform DestinationTransform;
        
        [DrawIf("DestinationType", DestinationTypes.Point)]
        [Tooltip("The point to seek to when the DestinationType is Point.")]
        public Vector3 DestinationPoint;

        [Tooltip("When this is greater than zero, the destination will be offset by this distance in the direction of the agent.")]
        public float DistanceOffset;

        [Tooltip("When true, the agent will stop when it reaches the destination.")]
        public bool StopAtDestination;

        public SeekPosition(Transform transform, bool stopAtDestination = false, float distanceOffset = 0f) {
            DestinationType = DestinationTypes.Transform;
            DestinationTransform = transform;
            DestinationPoint = Vector3.zero;
            DistanceOffset = distanceOffset;
            StopAtDestination = stopAtDestination;
        }

        public SeekPosition(Vector3 point, bool stopAtDestination = false, float distanceOffset = 0f) {
            DestinationType = DestinationTypes.Point;
            DestinationTransform = null;
            DestinationPoint = point;
            DistanceOffset = distanceOffset;
            StopAtDestination = stopAtDestination;
        }

        public bool IsNone => DestinationType == DestinationTypes.Transform && DestinationTransform == null;

        public Vector3 ResolvePosition(Transform from) {
            if (IsNone) {
                return from.position;
            }
            var pt = DestinationType == DestinationTypes.Transform ? DestinationTransform.position : DestinationPoint;
            if (DistanceOffset != 0f) {
                var dir = (from.position - pt).normalized;
                pt += dir * DistanceOffset;
            }
            return pt;
        }
    }

    [System.Serializable]
    public class SteerSeek {
        [Tooltip("Choose the specific seek behaviour to use.")]
        public SeekMode SeekMode;

        [Tooltip("Only relevant when SeekMode is Position. Defines the destination point to seek towards.")]
        public SeekPosition SeekPosition;

        [Tooltip("Only relevant when SeekMode is Direction. Defines the direction to seek towards.")]
        public Vector3 SeekDirection;

        [Min(0)]
        [Tooltip("Only relevant when SeekMode is Position. Defines the distance threshold when the destination is considered reached.")]
        public float ArriveDistanceThreshold;
        
        [Min(0)]
        [Tooltip("Only relevant when SeekMode is Position and StopAtDestination is true. Defines the distance threshold when the agent will start to slow down.")]
        public float StoppingDistance;
        
        public Vector3 GetDeltaToDestination(ISteeringSensor owner) {
            var destination = GetDestination(owner);
            return destination - owner.transform.position;
        }

        public float GetDistanceToDestination(ISteeringSensor owner) => GetDeltaToDestination(owner).magnitude;

        public bool GetIsDestinationReached(ISteeringSensor owner) {
            if (SeekMode == SeekMode.Position) {
                return GetDistanceToDestination(owner) <= ArriveDistanceThreshold;
            }
            return SeekMode == SeekMode.Stop;
        }
        
        public Vector3 GetSteeringVector(ISteeringSensor owner) {
            var chosenDirection = owner.Decision.GetCandidateDirection();
            if (chosenDirection == Vector3.zero) {
                return Vector3.zero;
            }
            var speed = owner.Velocity.GetSpeedCandidate(chosenDirection);
            return GetArriveVector(owner, speed * chosenDirection);
        }
        
        Vector3 GetDestination(ISteeringSensor owner) {
            if (SeekMode == SeekMode.Position) {
                return SeekPosition.ResolvePosition(owner.transform);
            } else if (SeekMode == SeekMode.Direction) {
                return owner.transform.position + SeekDirection;
            } else if (SeekMode == SeekMode.Wander) {
                return owner.transform.position + owner.transform.forward;
            }
            return owner.transform.position;
        }

        Vector3 GetArriveVector(ISteeringSensor owner, Vector3 velocity) {
            switch(SeekMode) {
                case SeekMode.Position:
                    if (!SeekPosition.StopAtDestination || StoppingDistance <= 0f) {
                        return velocity;
                    }
                    if (GetIsDestinationReached(owner)) {
                        return Vector3.zero;
                    }
                    var delta = GetDeltaToDestination(owner);
                    var distance = delta.magnitude;
                    return velocity * Mathf.Lerp(0f, 1f, distance / StoppingDistance);
                case SeekMode.Direction:
                    return SeekDirection != Vector3.zero ? velocity : Vector3.zero;
                case SeekMode.Wander:
                    return velocity;
                case SeekMode.Stop:
                    return Vector3.zero;
                default:
                    return velocity;
            }
        }
    }
}