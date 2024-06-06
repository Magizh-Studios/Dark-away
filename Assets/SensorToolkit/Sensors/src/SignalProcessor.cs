using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    public abstract class SignalProcessor : MonoBehaviour {
        public abstract bool Process(ref Signal signal, Sensor sensor);
    }

    public struct MapToRigidBodyFilter<REF, T>
        where REF : UnityEngine.Object
        where T : IAccumulated<REF, T> {
        public DetectionModes Mode;
        public bool Is2D;
        
        public void Configure(DetectionModes mode, bool is2D) {
            Mode = mode;
            Is2D = is2D;
        }

        public bool ProcessOutput(ref T signal) {
            if (Mode == DetectionModes.Colliders) { 
                return true;
            }
            GameObject rbGo = null;
            var target = signal.Object as GameObject;
            if (Is2D) {
                if (target.TryGetComponent<Collider2D>(out var col)) {
                    rbGo = col.attachedRigidbody?.gameObject;
                }
            } else {
                if (target.TryGetComponent<Collider>(out var c)) {
                    rbGo = c.attachedRigidbody?.gameObject;
                }
            }

            if (rbGo != null) {
                signal = signal.ChangeObject(rbGo as REF);
                return true;
            }
            return Mode == DetectionModes.Either;
        }
    }

    public struct MapToSignalProxyFilter<REF, T>
        where REF : UnityEngine.Object
        where T : IAccumulated<REF, T> {
        public bool IsEnabled;

        public void Configure(bool isEnabled) {
            IsEnabled = isEnabled;
        }

        public bool ProcessOutput(ref T signal) {
            if (!IsEnabled) {
                return true;
            }
            var targetObject = SignalProxy.GetProxyTarget(signal.Object as GameObject);
            signal = signal.ChangeObject(targetObject as REF);
            return true;
        }
    }
}