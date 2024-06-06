using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    public interface IPulseRoutine {
        PulseRoutine.Modes PulseMode { get; set; }
        float PulseInterval { get; set; }
    }

    [Serializable]
    public class PulseRoutine {
        public enum Modes { Manual, FixedInterval, EachFrame }
        public enum UpdateFunctions { Update, FixedUpdate }

        [Serializable]
        public class ObservableMode : Observable<Modes> { }

        public ObservableMode Mode = new ObservableMode() { Value = Modes.EachFrame };

        public UpdateFunctions UpdateFunction = UpdateFunctions.Update;

        public ObservableFloat Interval = new ObservableFloat() { Value = 1f };

        public float dt {
            get {
                if (Mode.Value == Modes.EachFrame) {
                    return Time.deltaTime;
                } else if (Mode.Value == Modes.FixedInterval) {
                    return Interval.Value;
                }
                return 0;
            }
        }

        BasePulsableSensor pulsable;
        Func<IEnumerator, Coroutine> startEachFrame;
        Func<IEnumerator, Coroutine> startFixedInterval;
        float steppedPulseDelay;
        float prevPulseTime;
        Coroutine pulseRoutine;
        PulseHandle pulseHandle;

        public void Awake(BasePulsableSensor pulsable, Func<IEnumerator, Coroutine> startEachFrame, Func<IEnumerator, Coroutine> startFixedInterval) {
            this.pulsable = pulsable;
            this.startEachFrame = startEachFrame;
            this.startFixedInterval = startFixedInterval;
            pulsable.OnPulsed += OnPulsedHandler;

            if (Mode == null) {
                Mode = new ObservableMode();
            }

            if (Interval == null) {
                Interval = new ObservableFloat();
            }

            steppedPulseDelay = UnityEngine.Random.Range(0f, 1f);
        }

        public void OnEnable() {
            Mode.OnChanged += PulseModeChangedHandler;
            Interval.OnChanged += PulseModeChangedHandler;

            PulseModeChangedHandler();
        }

        public void OnDisable() {
            Mode.OnChanged -= PulseModeChangedHandler;
            Interval.OnChanged -= PulseModeChangedHandler;
        }

        public void OnValidate() {
            Mode?.OnValidate();
            Interval?.OnValidate();
        }

        void PulseModeChangedHandler() {
            if (!Application.isPlaying) {
                return;
            }
            RunPulseMode(Mode.Value, Interval.Value);
        }

        void RunPulseMode(Modes mode, float interval = 0) {
            if (pulseRoutine != null) {
                pulseHandle.Complete();
                pulsable.StopCoroutine(pulseRoutine);
                pulseRoutine = null;
            }
            if (mode == Modes.EachFrame) {
                pulseRoutine = startEachFrame(PulseEachFrameRoutine());
            } else if (mode == Modes.FixedInterval) {
                pulseRoutine = startFixedInterval(PulseFixedIntervalRoutine(interval));
            }
        }

        WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
        IEnumerator PulseFixedIntervalRoutine(float interval) {
            var longWait = new WaitForSeconds(interval * 0.8f);
            var midWait = new WaitForSeconds(interval * 0.4f);
            var shortWait = new WaitForSeconds(interval * 0.2f);
            var tinyWait = new WaitForSeconds(interval * 0.1f);

            while (true) {
                var targetPulseTime = (Mathf.Floor(prevPulseTime / interval) + 1) * interval + (steppedPulseDelay * interval);
                var deltaTime = Time.time - targetPulseTime;
                if (deltaTime > 0) {
                    pulseHandle = pulsable.SchedulePulse();
                    int frames = 0;
                    while (!pulseHandle.IsCompleted) {
                        yield return null;
                        frames++;
                        if (frames > 2) {
                            pulseHandle.Complete();
                        } else {
                            pulseHandle.Tick();
                        }
                    }
                    // If pulse is finished imediately then we may have an infinite loop.
                    // Wait a frame here so this can't happen.
                    yield return null;
                    continue;
                }
                var xBehind = Mathf.Abs(deltaTime) / interval;
                if (xBehind > 0.8f) {
                    yield return longWait;
                } else if (xBehind > 0.4f) {
                    yield return midWait;
                } else if (xBehind > 0.2f) {
                    yield return shortWait;
                } else if (xBehind > 0.1f) {
                    yield return tinyWait;
                } else {
                    yield return null;
                }
            }

        }

        IEnumerator PulseEachFrameRoutine() {
            while (true) {
                yield return null;
                pulseHandle.Complete();
                if (UpdateFunction == UpdateFunctions.FixedUpdate) {
                    yield return waitForFixedUpdate;
                }
                pulseHandle = pulsable.SchedulePulse();
            }
        }

        void OnPulsedHandler() {
            prevPulseTime = Time.time;
        }
    }

}