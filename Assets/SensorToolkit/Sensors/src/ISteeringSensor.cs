using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {
    
    public interface ISteeringSensor {
        GameObject gameObject { get; }
        Transform transform { get; }
        SteerSeek Seek { get; }
        SteerInterest Interest { get; }
        SteerDanger Danger { get; }
        SteerVO Velocity { get; }
        SteerDecision Decision { get; }
        LocomotionSystem Locomotion { get; }
        
        bool IsDestinationReached { get; }
        bool IsSeeking { get; }

        void SeekTo(Transform destination, float distanceOffset = 0f);
        void SeekTo(Vector3 destination, float distanceOffset = 0f);
        void ArriveTo(Transform destination, float distanceOffset = 0f);
        void ArriveTo(Vector3 destination, float distanceOffset = 0f);
        void SeekDirection(Vector3 direction);
        void Wander();
        void Stop();
        Vector3 GetSteeringVector();
        float GetSpeedCandidate(Vector3 direction);
    }

}