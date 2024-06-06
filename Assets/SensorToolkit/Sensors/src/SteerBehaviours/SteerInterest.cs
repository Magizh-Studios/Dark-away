using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace Micosmo.SensorToolkit {

    [System.Serializable]
    public class SteerInterest : IDisposable {

        public enum MappingFunctions { RadialInterpolation, SignalStrength }

        [Min(0)]
        [Tooltip("An interest vector weighted by this will be applied to the forward direction of the agent. Encourages the agent to keep its current direction.")]
        public float StabilizationWeight = 0.2f;

        [Tooltip("The local forward direction that the stabilization vector will be applied to.")]
        public Vector3 LocalForwardDirection = Vector3.forward;

        [Space]

        [Tooltip("A list of sensors that detect targets of interest. Each detection will produce an interest vector in its direction from the sensor.")]
        public List<Sensor> Sensors = new List<Sensor>();

        [Tooltip("A list of signal processors that will be applied to the detections on the input sensors.")]
        public List<SignalProcessor> SignalProcessors = new List<SignalProcessor>();

        [Tooltip("How to calculate the magnitude of interest for each input detection.")]
        public MappingFunctions MappingFunction = MappingFunctions.RadialInterpolation;
        
        [DrawIf("MappingFunction", MappingFunctions.RadialInterpolation)]
        [Tooltip("Only relevant when MappingFunction is RadialInterpolation. Interest is interpolated from 1 to 0 by distance to the sensor.")]
        public RadialInterpolation RadialInterpolation = new RadialInterpolation(0f, 2f);

        public DirectionalGrid InterestMap => interestMap;
        DirectionalGrid interestMap;

        NativeArray<InterestItem> sharedInterestItems;
        DirectionalGrid sharedInterestMap;

        public void RecreateGrids(int resolution, bool isSpherical, Vector3 up) {
            Dispose();
            if (isSpherical) {
                interestMap = DirectionalGrid.CreateSphere(resolution, Allocator.Persistent);
            } else {
                interestMap = DirectionalGrid.CreateCircle(resolution * 4, up, Allocator.Persistent);
            }
        }

        public void Dispose() {
            if (interestMap.IsCreated) {
                interestMap.Dispose();
            }
            if (sharedInterestMap.IsCreated) {
                sharedInterestMap.Dispose();
            }
            if (sharedInterestItems.IsCreated) {
                sharedInterestItems.Dispose();
            }
        }

        public void Clear() {
            interestMap.Fill(0);
        }

        public void PulseSensors() {
            if (Sensors == null) {
                return;
            }
            foreach (var sensor in Sensors) {
                sensor?.PulseAll();
            }
        }

        List<InterestItem> workList = new List<InterestItem>();
        public struct JobContext {
            public JobHandle Handle;
            public DirectionalGrid SharedInterestMap;
        }
        public JobContext ScheduleJob(ISteeringSensor owner) {
            workList.Clear();
            if (Sensors != null) {
                foreach (var sensor in Sensors) {
                    if (sensor == null) {
                        continue;
                    }
                    foreach (var signal in sensor.Signals) {
                        var processed = signal;
                        if (ProcessSignal(sensor, ref processed)) {
                            workList.Add(new InterestItem {
                                Bounds = processed.Bounds,
                                Strength = processed.Strength,
                            });
                        }
                    }
                }
            }
            sharedInterestItems = new NativeArray<InterestItem>(workList.Count, Allocator.TempJob);
            for (int i = 0; i < workList.Count; i++) {
                sharedInterestItems[i] = workList[i];
            }
            if (!sharedInterestMap.IsCompatible(interestMap)) {
                sharedInterestMap.Dispose();
                sharedInterestMap = DirectionalGrid.CreateMatching(interestMap, Allocator.Persistent);
            }
            var job = new SteerInterestJob {
                Position = owner.transform.position,
                Direction = owner.transform.TransformDirection(LocalForwardDirection.normalized),
                SeekForce = owner.Seek.SeekMode != SeekMode.Wander ? owner.Seek.GetDeltaToDestination(owner).normalized : Vector3.zero,
                StabilizationWeight = StabilizationWeight,
                InterestItems = sharedInterestItems,
                InterestMap = sharedInterestMap,
                IsRadialMapping = MappingFunction == MappingFunctions.RadialInterpolation,
                RadialMapping = RadialInterpolation
            };
            return new JobContext { 
                Handle = job.Schedule(),
                SharedInterestMap = sharedInterestMap
            };
        }

        public void ManagedFinish() {
            if (!sharedInterestItems.IsCreated) {
                return;
            }
            interestMap.Copy(sharedInterestMap);
            sharedInterestItems.Dispose();
        }

        public void DrawGizmos(ISteeringSensor owner, float offset, float scale, float width) {
            SensorGizmos.PushColor(STPrefs.SeekColour);
            interestMap.DrawGizmos(owner.transform.position, offset, scale, width);
            SensorGizmos.PopColor();
        }

        bool ProcessSignal(Sensor sensor, ref Signal signal) {
            if (SignalProcessors == null) {
                return true;
            }
            foreach (var processor in SignalProcessors) {
                if (processor == null) {
                    continue;
                }
                if (!processor.Process(ref signal, sensor)) {
                    return false;
                }
            }
            return signal.Object != null;
        }

        public struct InterestItem {
            public Bounds Bounds;
            public float Strength;
        }

        public struct SteerInterestJob : IJob {
            public Vector3 Position;
            public Vector3 Direction;
            public Vector3 SeekForce;
            public float StabilizationWeight;
            public NativeArray<InterestItem> InterestItems;
            public DirectionalGrid InterestMap;
            public bool IsRadialMapping;
            public RadialInterpolation RadialMapping;

            public void Execute() {
                InterestMap.Fill(0);

                if (SeekForce != Vector3.zero) {
                    InterestMap.GradientAdd(SeekForce, -1);
                }

                foreach (var item in InterestItems) {
                    GetInterest(item, out var point, out var vInterest);
                    if (vInterest != Vector3.zero) {
                        InterestMap.GradientFill(vInterest, 0f);
                    }
                }

                if (StabilizationWeight > 0) {
                    InterestMap.GradientFunction(Direction * StabilizationWeight, 0, (prevCell, v) => prevCell + v);
                }

                InterestMap.ClampRange01();
            }
            
            void GetInterest(InterestItem item, out Vector3 point, out Vector3 vInterest) {
                point = item.Bounds.ClosestPoint(Position);
                var delta = point - Position;
                if (delta == Vector3.zero) {
                    // We're inside the bounds of the signal. Avoid NaNs.
                    delta = (item.Bounds.center - Position).normalized * .01f;
                }
                var dist = delta.magnitude;
                var dir = delta / dist;
                var xInterest = IsRadialMapping ? RadialMapping.Calculate(dist) : item.Strength;
                vInterest = xInterest * dir;
            }
        }
        
    }

}