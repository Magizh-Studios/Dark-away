using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace Micosmo.SensorToolkit {
    
    [System.Serializable]
    public class SteerVO : System.IDisposable {

        [Min(0)]
        [Tooltip("The speed to aim for in the absence of potential collisions.")]
        public float PreferredSpeed = 4f;

        [Min(0)]
        [Tooltip("The maximum speed the agent can move it. The sensor will produce candidate speeds up to this value that avoids collisions.")]
        public float MaxSpeed = 8f;

        [Space]

        [Tooltip("A list of sensors that detect velocity obstacles. Velocities are estimated for each detection by comparing their positions between pulses.")]
        public List<Sensor> Sensors = new List<Sensor>();

        [Tooltip("A list of signal processors that will be applied to the detections on the input sensors.")]
        public List<SignalProcessor> SignalProcessors = new List<SignalProcessor>();

        [Tooltip("The radius of the agent used to predict potential collisions.")]
        public float AgentRadius = 1f;

        [Tooltip("The time horizon used to predict potential collisions. The sensor will search for candidate velocities that avoids collisions within this time window.")]
        public float TimeHorizon = 1f;

        [Tooltip("The number of velocity candidates tested each time the sensor pulses.")]
        public int SamplesPerPulse = 400;

        // Stores candidate speeds that are between 0 and the prefered speed.
        public DirectionalGrid LowSpeedMap => lowSpeedMap;
        DirectionalGrid lowSpeedMap;

        // Stores candidate speed that are between the prefered speed and the max speed.
        public DirectionalGrid HighSpeedMap => highSpeedMap;
        DirectionalGrid highSpeedMap;

        Dictionary<GameObject, VelocityEstimator> velocities = new Dictionary<GameObject, VelocityEstimator>();
        Dictionary<GameObject, VelocityEstimator> nextVelocities = new Dictionary<GameObject, VelocityEstimator>();
        NativeArray<VelocityObstacle> sharedVelocityObstacles;
        DirectionalGrid sharedLowSpeed;
        DirectionalGrid sharedHighSpeed;
        NativeSobolSequence sobolCell;
        NativeSobolSequence sobolSpeed;

        // We store 'running' and 'complete' versions in case the user changes the values while the sensor is running
        float runningPreferredSpeed, runningMaxSpeed;
        float completePreferredSpeed, completeMaxSpeed;

        public float GetSpeedCandidate(Vector3 direction) {
            var lowSpeed = GetLowerSpeedCandidate(direction);
            var highSpeed = GetUpperSpeedCandidate(direction);
            if (highSpeed == completeMaxSpeed) {
                return lowSpeed; // When no candidate exists lowSpeed would be 0 and therefore we return 0
            }
            if (lowSpeed == 0f) {
                return highSpeed;
            }
            var xSlow = Mathf.Abs(lowSpeed - completePreferredSpeed) / completePreferredSpeed;
            var xFast = (completePreferredSpeed != completeMaxSpeed) 
                ? 1f - Mathf.Abs(highSpeed - completePreferredSpeed) / (completeMaxSpeed - completePreferredSpeed) 
                : xSlow;
            return xSlow < xFast ? lowSpeed : highSpeed;
        }

        public float GetLowerSpeedCandidate(Vector3 direction) {
            if (LowSpeedMap.IsCreated) {
                return LowSpeedMap.SampleDirection(direction);
            }
            return PreferredSpeed;
        }

        public float GetUpperSpeedCandidate(Vector3 direction) {
            if (HighSpeedMap.IsCreated) {
                return HighSpeedMap.SampleDirection(direction);
            }
            return PreferredSpeed;
        }

        public void RecreateGrids(int resolution, bool isSpherical, Vector3 up) {
            Dispose();
            if (isSpherical) {
                lowSpeedMap = DirectionalGrid.CreateSphere(resolution, Allocator.Persistent);
                highSpeedMap = DirectionalGrid.CreateSphere(resolution, Allocator.Persistent);
            } else {
                lowSpeedMap = DirectionalGrid.CreateCircle(resolution * 4, up, Allocator.Persistent);
                highSpeedMap = DirectionalGrid.CreateCircle(resolution * 4, up, Allocator.Persistent);
            }
        }
        
        public void Dispose() {
            if (lowSpeedMap.IsCreated) {
                lowSpeedMap.Dispose();
            }
            if (highSpeedMap.IsCreated) {
                highSpeedMap.Dispose();
            }
            if (sharedLowSpeed.IsCreated) {
                sharedLowSpeed.Dispose();
            }
            if (sharedHighSpeed.IsCreated) {
                sharedHighSpeed.Dispose();
            }
            if (sharedVelocityObstacles.IsCreated) {
                sharedVelocityObstacles.Dispose();
            }
            if (sobolCell.IsCreated) {
                sobolCell.Dispose();
            }
            if (sobolSpeed.IsCreated) {
                sobolSpeed.Dispose();
            }
        }

        public void Clear() {
            lowSpeedMap.Fill(0);
            highSpeedMap.Fill(0);
            runningPreferredSpeed = runningMaxSpeed = 0f;
            completePreferredSpeed = completeMaxSpeed = 0f;
        }

        public void PulseSensors() {
            if (Sensors == null) {
                return;
            }
            foreach (var sensor in Sensors) {
                sensor?.PulseAll();
            }
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

        public struct JobContext {
            public JobHandle Handle;
            public DirectionalGrid SharedLowSpeed;
            public DirectionalGrid SharedHighSpeed;
        }
        List<Signal> worklist = new List<Signal>();
        public JobContext ScheduleJob(ISteeringSensor owner) {
            nextVelocities.Clear();
            if (!sobolCell.IsCreated) {
                sobolCell = new NativeSobolSequence(1, Allocator.Persistent);
                sobolSpeed = new NativeSobolSequence(1, Allocator.Persistent);
            }
            worklist.Clear();
            if (Sensors != null) {
                foreach (var sensor in Sensors) {
                    if (sensor == null) {
                        continue;
                    }
                    foreach (var signal in sensor.Signals) {
                        var processedSignal = signal; 
                        if (!ProcessSignal(sensor, ref processedSignal)) {
                            continue;
                        }
                        worklist.Add(processedSignal);
                    }
                }
            }
            sharedVelocityObstacles = new NativeArray<VelocityObstacle>(worklist.Count, Allocator.TempJob);
            if (!sharedLowSpeed.IsCreated || lowSpeedMap.GridSize != sharedLowSpeed.GridSize) {
                sharedLowSpeed.Dispose();
                sharedLowSpeed = new DirectionalGrid(lowSpeedMap.IsSpherical, lowSpeedMap.GridSize, lowSpeedMap.Axis, Allocator.Persistent);
                sharedHighSpeed.Dispose();
                sharedHighSpeed = new DirectionalGrid(highSpeedMap.IsSpherical, highSpeedMap.GridSize, highSpeedMap.Axis, Allocator.Persistent);
            }
            sharedLowSpeed.Copy(lowSpeedMap);
            sharedHighSpeed.Copy(highSpeedMap);
            int i = 0;
            foreach (var signal in worklist) {
                if (!velocities.TryGetValue(signal.Object, out var velocityEstimator)) {
                    velocityEstimator = new VelocityEstimator();
                }
                velocityEstimator.Sample(signal.Object);
                nextVelocities[signal.Object] = velocityEstimator;

                var center = signal.Bounds.center - owner.transform.position;
                var extents = signal.Shape.extents;
                var radius = Mathf.Max(extents.x, Mathf.Max(extents.y, extents.z)) + AgentRadius;
                var distance = center.magnitude;
                if (radius > distance) {
                    radius = distance * 0.99f;
                }

                var velObstactle = new VelocityObstacle {
                    Center = center,
                    Radius = radius,
                    Velocity = velocityEstimator.Velocity
                };
                sharedVelocityObstacles[i] = velObstactle;
                i++;
            }
            runningPreferredSpeed = PreferredSpeed;
            runningMaxSpeed = MaxSpeed;
            var job = new SteerVOJob {
                VelocityObstacles = sharedVelocityObstacles,
                LowSpeed = sharedLowSpeed,
                HighSpeed = sharedHighSpeed,
                AgentRadius = AgentRadius,
                TimeHorizon = TimeHorizon,
                PreferredSpeed = PreferredSpeed,
                MaxSpeed = MaxSpeed,
                SamplesPerPulse = SamplesPerPulse,
                SobolCell = sobolCell,
                SobolSpeed = sobolSpeed
            };
            return new JobContext {
                Handle = job.Schedule(),
                SharedLowSpeed = sharedLowSpeed,
                SharedHighSpeed = sharedHighSpeed
            };
        }

        public void ManagedFinish() {
            if (!sharedVelocityObstacles.IsCreated) {
                return;
            }
            
            var temp = velocities;
            velocities = nextVelocities;
            nextVelocities = temp;

            lowSpeedMap.Copy(sharedLowSpeed);
            highSpeedMap.Copy(sharedHighSpeed);
            completePreferredSpeed = runningPreferredSpeed;
            completeMaxSpeed = runningMaxSpeed;

            sharedVelocityObstacles.Dispose();
        }

        public void DrawGizmos(ISteeringSensor owner, float offset, float scale, float width) {
            if (lowSpeedMap.IsCreated && highSpeedMap.IsCreated) {
                DirectionalGrid.DrawVelocityGizmos(owner.transform.position, offset, scale / completeMaxSpeed, width, completeMaxSpeed, lowSpeedMap, highSpeedMap);
            }
        }

    }

    public struct SteerVOJob : IJob {
        const int MaxSamplesPerDirection = 10;

        public NativeArray<VelocityObstacle> VelocityObstacles;
        public DirectionalGrid LowSpeed;
        public DirectionalGrid HighSpeed;
        public float AgentRadius;
        public float TimeHorizon;
        public float PreferredSpeed;
        public float MaxSpeed;
        public int SamplesPerPulse;
        public NativeSobolSequence SobolCell;
        public NativeSobolSequence SobolSpeed;

        int sampleCount;

        public void Execute() {
            var cellCount = LowSpeed.CellCount;

            sampleCount = 0;
            while (sampleCount < SamplesPerPulse) {
                var t = SobolCell.Next()[0];
                var cellIndex = Mathf.FloorToInt(t * cellCount);
                if (cellIndex >= cellCount) {
                    cellIndex = cellCount - 1;
                }
                var cellDir = LowSpeed.Directions[cellIndex];
                var slowTarget = LowSpeed.Values[cellIndex];
                var fastTarget = HighSpeed.Values[cellIndex];
                if (fastTarget < PreferredSpeed) {
                    fastTarget = PreferredSpeed;
                }
                GetTargetSpeed(cellDir, ref slowTarget, ref fastTarget);
                LowSpeed.Values[cellIndex] = slowTarget;
                HighSpeed.Values[cellIndex] = fastTarget;
            }
        }

        void GetTargetSpeed(Vector3 direction, ref float currSlowTarget, ref float currFastTarget) {
            if (TestVelocity(direction * PreferredSpeed)) {
                currSlowTarget = currFastTarget = PreferredSpeed;
                return;
            }

            var bestMinSpeed = (TestVelocity(direction * currSlowTarget)) ? currSlowTarget : 0f;
            var bestMaxSpeed = (TestVelocity(direction * currFastTarget)) ? currFastTarget : MaxSpeed;

            for (int i = 0; i < MaxSamplesPerDirection; i++) {
                var speed = Mathf.Lerp(bestMinSpeed, bestMaxSpeed, SobolSpeed.Next()[0]);
                if (TestVelocity(direction * speed)) {
                    if (speed < PreferredSpeed) {
                        bestMinSpeed = speed;
                    } else {
                        bestMaxSpeed = speed;
                    }
                }
            }

            currSlowTarget = bestMinSpeed;
            currFastTarget = bestMaxSpeed;
        }

        bool TestVelocity(Vector3 velocity) {
            sampleCount += 1;
            foreach (var velocityObstacle in VelocityObstacles) {
                if (velocityObstacle.ContainsVelocity(velocity, TimeHorizon)) {
                    return false;
                }
            }
            return true;
        }
    }

}