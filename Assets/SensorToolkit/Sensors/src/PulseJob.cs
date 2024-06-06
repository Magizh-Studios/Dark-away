using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

namespace Micosmo.SensorToolkit {

    public struct PulseJob {

        public delegate JobHandle Step(bool isRun);
        public delegate bool CancelHandler(int stepIndex);

        public Step[] Steps;
        public CancelHandler OnCancel;
        public bool IsCreated => Steps != null;
        public PulseJob(Step[] steps, CancelHandler onCancel = null) {
            Steps = steps;
            OnCancel = onCancel;
        }
        public PulseHandle Schedule(PulseHandle dependsOn = default) => PulseHandle.Scheduler.Schedule(this, dependsOn);
        public void Run() => PulseHandle.Scheduler.Run(this);
    }


    public struct PulseHandle {
        int executionId;

        public bool IsRunning => Scheduler.IsRunning(this);
        public bool IsCompleted => Scheduler.IsCompleted(this);
        public void Tick() => Scheduler.Tick(this);
        public void Complete() => Scheduler.Complete(this);
        public bool TryCancel() => Scheduler.TryCancel(this);

        internal static class Scheduler {

            static int currExecId;
            static Dictionary<int, Execution> executions = new Dictionary<int, Execution>();

            public static PulseHandle Schedule(PulseJob job, PulseHandle dependsOn = default) {
                do {
                    currExecId += 1;
                } while (currExecId == 0 || executions.ContainsKey(currExecId));
                executions[currExecId] = new Execution(job, dependsOn);
                return new PulseHandle {
                    executionId = currExecId
                };
            }

            public static void Run(PulseJob job) {
                var execution = new Execution(job, default);
                execution.Complete();
            }

            public static bool IsRunning(PulseHandle handle) { 
                if (executions.TryGetValue(handle.executionId, out var execution)) {
                    return execution.IsRunning;
                }
                return false;
            }

            public static bool IsCompleted(PulseHandle handle) {
                if (executions.TryGetValue(handle.executionId, out var execution)) {
                    return execution.IsCompleted;
                }
                return true;
            }

            public static void Tick(PulseHandle handle) {
                if (executions.TryGetValue(handle.executionId, out var execution)) {
                    execution.Tick();
                    if (execution.IsCompleted) {
                        executions.Remove(handle.executionId);
                    } else {
                        executions[handle.executionId] = execution;
                    }
                }
            }

            public static void Complete(PulseHandle handle) {
                if (executions.TryGetValue(handle.executionId, out var execution)) {
                    execution.Complete();
                    executions.Remove(handle.executionId);
                }
            }

            public static bool TryCancel(PulseHandle handle) {
                if (executions.TryGetValue(handle.executionId, out var execution)) {
                    if (execution.TryCancel()) {
                        executions.Remove(handle.executionId);
                        return true;
                    }
                    return false;
                }
                return true;
            }

            struct Execution {
                public delegate JobHandle Step(bool isRun);

                PulseJob job;
                int nextStep;
                JobHandle currHandle;
                PulseHandle dependsOn;
                public bool IsCompleted { get; private set; }

                public Execution(PulseJob job, PulseHandle dependsOn) {
                    this.job = job;
                    nextStep = 0;
                    currHandle = default;
                    this.dependsOn = dependsOn;
                    IsCompleted = false;
                }
                public bool IsRunning => nextStep > 0 && !IsCompleted;
                public void Tick() {
                    dependsOn.Tick();
                    if (!dependsOn.IsCompleted) {
                        return;
                    }
                    if (IsCompleted) {
                        return;
                    }
                    while (currHandle.IsCompleted && !IsCompleted) {
                        ScheduleNext(false);
                    }
                }
                public void Complete() {
                    dependsOn.Complete();
                    while (!IsCompleted) {
                        ScheduleNext(true);
                    }
                }
                public bool TryCancel() {
                    if (!IsRunning) {
                        return true;
                    }
                    if (IsCompleted) {
                        return true;
                    }
                    if (job.OnCancel != null) {
                        return job.OnCancel(nextStep);
                    }
                    return false;
                }
                void ScheduleNext(bool isRun) {
                    if (!dependsOn.IsCompleted || IsCompleted) {
                        return;
                    }
                    currHandle.Complete();
                    if (nextStep >= (job.Steps?.Length ?? 0)) {
                        IsCompleted = true;
                        return;
                    }
                    currHandle = job.Steps[nextStep](isRun);
                    nextStep += 1;
                }
            }

        }
    }
}