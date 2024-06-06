using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace Micosmo.SensorToolkit {
    
    [System.Serializable]
    public class SteerDanger : IDisposable {

        public enum MappingFunctions { RadialInterpolation, SignalStrength }

        [Tooltip("A list of sensors that detect dangerous targets to avoid. Each detection will produce a danger vector in its direction from the sensor.")]
        public List<Sensor> Sensors = new List<Sensor>();

        [Tooltip("A list of signal processors that will be applied to the detections on the input sensors.")]
        public List<SignalProcessor> SignalProcessors = new List<SignalProcessor>();

        [Tooltip("How to calculate the magnitude of danger for each input detection.")]
        public MappingFunctions MappingFunction = MappingFunctions.RadialInterpolation;
        
        [DrawIf("MappingFunction", MappingFunctions.RadialInterpolation)]
        [Tooltip("Only relevant when MappingFunction is RadialInterpolation. Danger is interpolated from 1 to 0 by distance to the sensor.")]
        public RadialInterpolation RadialInterpolation = new RadialInterpolation(0f, 2f);

        public DirectionalGrid DangerMap => dangerMap;
        DirectionalGrid dangerMap;

        NativeArray<DangerItem> sharedDangerItems;
        DirectionalGrid sharedDangerMap;

        public void RecreateGrids(int resolution, bool isSpherical, Vector3 up) {
            Dispose();
            if (isSpherical) {
                dangerMap = DirectionalGrid.CreateSphere(resolution, Allocator.Persistent);
            } else {
                dangerMap = DirectionalGrid.CreateCircle(resolution * 4, up, Allocator.Persistent);
            }
        }

        public void Dispose() {
            if (dangerMap.IsCreated) {
                dangerMap.Dispose();
            }
            if (sharedDangerMap.IsCreated) {
                sharedDangerMap.Dispose();
            }
            if (sharedDangerItems.IsCreated) {
                sharedDangerItems.Dispose();
            }
        }

        public void Clear() {
            dangerMap.Fill(0);
        }

        public void PulseSensors() {
            if (Sensors == null) {
                return;
            }
            foreach (var sensor in Sensors) {
                sensor?.PulseAll();
            }
        }

        List<DangerItem> workList = new List<DangerItem>();
        public struct JobContext {
            public JobHandle Handle;
            public DirectionalGrid SharedDangerMap;
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
                            workList.Add(new DangerItem {
                                Bounds = processed.Bounds,
                                Strength = processed.Strength,
                            });
                        }
                    }
                }
            }
            sharedDangerItems = new NativeArray<DangerItem>(workList.Count, Allocator.TempJob);
            for (int i = 0; i < workList.Count; i++) {
                sharedDangerItems[i] = workList[i];
            }
            if (!sharedDangerMap.IsCompatible(dangerMap)) {
                if (sharedDangerMap.IsCreated) {
                    sharedDangerMap.Dispose();
                }
                sharedDangerMap = DirectionalGrid.CreateMatching(dangerMap, Allocator.Persistent);
            }
            var job = new SteerDangerJob {
                Position = owner.transform.position,
                DangerItems = sharedDangerItems,
                DangerMap = sharedDangerMap,
                IsRadialMapping = MappingFunction == MappingFunctions.RadialInterpolation,
                RadialMapping = RadialInterpolation
            };
            return new JobContext {
                Handle = job.Schedule(),
                SharedDangerMap = sharedDangerMap
            };
        }
        
        public void ManagedFinish() {
            if (!sharedDangerItems.IsCreated) {
                return;
            }
            dangerMap.Copy(sharedDangerMap);
            sharedDangerItems.Dispose();
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
        public void DrawGizmos(ISteeringSensor owner, float offset, float scale, float width) {
            SensorGizmos.PushColor(STPrefs.AvoidColour);
            dangerMap.DrawGizmos(owner.transform.position, offset, scale, width);
            SensorGizmos.PopColor();
        }

        public struct DangerItem {
            public Bounds Bounds;
            public float Strength;
        }

        public struct SteerDangerJob : IJob {
            public Vector3 Position;
            public NativeArray<DangerItem> DangerItems;
            public DirectionalGrid DangerMap;
            public bool IsRadialMapping;
            public RadialInterpolation RadialMapping;

            public void Execute() {
                DangerMap.Fill(0);
                for (int i = 0; i < DangerItems.Length; i++) {
                    var item = DangerItems[i];
                    var point = item.Bounds.ClosestPoint(Position);
                    var delta = point - Position;
                    if (delta == Vector3.zero) {
                        // We're inside the bounds of the signal. Avoid NaNs.
                        delta = (item.Bounds.center - Position).normalized * .01f;
                    }
                    var dist = delta.magnitude;
                    var dir = delta / dist;
                    var xDanger = IsRadialMapping ? RadialMapping.Calculate(dist) : item.Strength;
                    if (xDanger == 0) {
                        continue;
                    }
                    var vDanger = xDanger * dir;
                    DangerMap.GradientFill(vDanger, 0f);
                }
            }
        }
    }

}
