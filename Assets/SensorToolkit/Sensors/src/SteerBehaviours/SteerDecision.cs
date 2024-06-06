using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
namespace Micosmo.SensorToolkit {

    [System.Serializable]
    public class SteerDecision : IDisposable {
        
        [Min(0)]
        [Tooltip("Always avoid directions with a danger value above this threshold, unless all directions are above it.")]
        public float MaxDangerThreshold = 0.5f;
        
        [Range(0, 1f)]
        [Tooltip("How much the velocity candidates influences the decision. Greater values will make the sensor prefer directions where it can travel closer to the preferred speed.")]
        public float PreferredVelocityInfluence = 0.5f;

        [Space]
        
        [Tooltip("How quickly the sensor interpolates its state. Slower interpolation will reduce erratic behaviour, but may make the sensor slow to react.")]
        public float InterpolationSpeed = 8f;

        DirectionalGrid rawDecisionMap;
        public DirectionalGrid DecisionMap => decisionMap;
        DirectionalGrid decisionMap;

        DirectionalGrid sharedDecisionMap;

        public Vector3 GetCandidateDirection() {
            if (!decisionMap.IsCreated) {
                return Vector3.zero;
            }
            var chosenDirection = decisionMap.GetMaxContinuous().normalized;
            return chosenDirection;
        }

        public void RecreateGrids(int resolution, bool isSpherical, Vector3 up) {
            Dispose();
            if (isSpherical) {
                rawDecisionMap = DirectionalGrid.CreateSphere(resolution, Allocator.Persistent);
                decisionMap = DirectionalGrid.CreateSphere(resolution, Allocator.Persistent);
            } else {
                rawDecisionMap = DirectionalGrid.CreateCircle(resolution * 4, up, Allocator.Persistent);
                decisionMap = DirectionalGrid.CreateCircle(resolution * 4, up, Allocator.Persistent);
            }
        }

        public void Dispose() {
            if (rawDecisionMap.IsCreated) {
                rawDecisionMap.Dispose();
            }
            if (decisionMap.IsCreated) {
                decisionMap.Dispose();
            }
            if (sharedDecisionMap.IsCreated) {
                sharedDecisionMap.Dispose();
            }
        }

        public void Clear() {
            rawDecisionMap.Fill(0);
            decisionMap.Fill(0);
        }

        public JobHandle ScheduleJob(ISteeringSensor owner, SteerInterest.JobContext interestContext, SteerDanger.JobContext dangerContext, SteerVO.JobContext voContext) {
            if (!sharedDecisionMap.IsCompatible(rawDecisionMap)) {
                if (sharedDecisionMap.IsCreated) {
                    sharedDecisionMap.Dispose();
                }
                sharedDecisionMap = DirectionalGrid.CreateMatching(rawDecisionMap, Allocator.Persistent);
            }
            var job = new SteerDecisionJob {
                InterestMap = interestContext.SharedInterestMap,
                DangerMap = dangerContext.SharedDangerMap,
                LowSpeedMap = voContext.SharedLowSpeed,
                HighSpeedMap = voContext.SharedHighSpeed,
                DecisionMap = sharedDecisionMap,
                MaxDangerThreshold = MaxDangerThreshold,
                PreferredVelocityInfluence = PreferredVelocityInfluence,
                PreferredSpeed = owner.Velocity.PreferredSpeed,
                MaxSpeed = owner.Velocity.MaxSpeed
            };
            var deps = JobHandle.CombineDependencies(
                interestContext.Handle, 
                dangerContext.Handle, 
                voContext.Handle);
            return job.Schedule(deps);
        }
        
        public void ManagedFinish() {
            rawDecisionMap.Copy(sharedDecisionMap);
            if (!Application.isPlaying) {
                decisionMap.Copy(sharedDecisionMap);
            }
        }

        public void Interpolate(float dt) {
            decisionMap.InterpolateTo(rawDecisionMap, dt * InterpolationSpeed);
        }

        public void DrawGizmos(ISteeringSensor owner, float offset, float scale, float width) {
            SensorGizmos.PushColor(Color.green);
            decisionMap.DrawGizmos(owner.transform.position, offset, scale, width);
            SensorGizmos.PopColor();
        }
        
        public struct SteerDecisionJob : IJob {
            public DirectionalGrid InterestMap;
            public DirectionalGrid DangerMap;
            public DirectionalGrid LowSpeedMap;
            public DirectionalGrid HighSpeedMap;
            public DirectionalGrid DecisionMap;
            public float MaxDangerThreshold;
            public float PreferredVelocityInfluence;
            public float PreferredSpeed;
            public float MaxSpeed;
            
            public void Execute() {
                DecisionMap.Copy(InterestMap);
                if (PreferredVelocityInfluence > 0) {
                    DecisionMap.MergeVelocity(LowSpeedMap, HighSpeedMap, PreferredSpeed, MaxSpeed, PreferredVelocityInfluence);
                }
                DecisionMap.MergeDanger(DangerMap, MaxDangerThreshold);
            }
        }
    }
    
}
