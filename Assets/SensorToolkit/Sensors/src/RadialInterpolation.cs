using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Micosmo.SensorToolkit {

    [System.Serializable]
    public struct RadialInterpolation {
        [Min(0), FormerlySerializedAs("EndRadius")]
        [Tooltip("The distance at which the interpolation will be 1.")]
        public float InnerRadius;

        [Min(0), FormerlySerializedAs("StartRadius")]
        [Tooltip("The distance at which the interpolation will be 0.")]
        public float OuterRadius;
        
        [Range(0.01f, 10f)]
        [Tooltip("A value of 1 is linear interpolation. Larger values push the midpoint towards the outer radius. Smaller values push the midpoint towards the inner radius.")]
        public float Power;

        public RadialInterpolation(float innerRadius, float outerRadius, float power = 1f) {
            OuterRadius = outerRadius;
            InnerRadius = innerRadius;
            Power = power;
        }

        public float Calculate(float r) {
            var t = Mathf.InverseLerp(OuterRadius, InnerRadius, r);
            if (Power != 1f) {
                t = Mathf.Pow(t, 1f / Power);
            }
            return t;
        }
    }

}