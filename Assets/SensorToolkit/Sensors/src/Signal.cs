using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    public interface IAccumulated<REF, T> : IEquatable<T> 
        where REF : UnityEngine.Object
        where T : IAccumulated<REF, T> {
        REF Object { get; set; }
        T ChangeObject(REF to);
        T Combine(T other);
    }

    [Serializable]
    public struct Signal : IAccumulated<GameObject, Signal> {
        public GameObject Object { get; set; }
        public float Strength;
        public Bounds Shape;
        Vector3 safePosition => Object != null ? Object.transform.position : Vector3.zero;
        public Signal(GameObject obj, float strength, Bounds shape) {
            Object = obj;
            Strength = strength;
            Shape = shape;
        }
        public Signal(GameObject obj) {
            Object = obj;
            Strength = 1f;
            Shape = new Bounds();
        }
        public Signal ChangeObject(GameObject to) {
            return new Signal(to, Strength, new Bounds(Shape.center - (to.transform.position - Object.transform.position), Shape.size));
        }
        public void Expand(Collider c) {
            var b = c.bounds;
            var lb = new Bounds(b.center - safePosition, b.size);
            Shape.Encapsulate(lb);
        }
        public Signal Combine(Signal signal) {
            var combinedStrength = Mathf.Max(Strength, signal.Strength);
            var combinedBounds = Bounds;
            combinedBounds.Encapsulate(signal.Bounds);
            return new Signal(Object, combinedStrength, new Bounds(combinedBounds.center - safePosition, combinedBounds.size));
        }
        public Bounds Bounds {
            get => new Bounds(Shape.center + safePosition, Shape.size);
            set => Shape = new Bounds(value.center - safePosition, value.size);
        }
        public bool Equals(Signal other) {
            return ReferenceEquals(Object, other.Object) && Strength == other.Strength && Shape == other.Shape;
        }
        public float DistanceTo(Vector3 point) {
            return (Bounds.center - point).magnitude;
        }
    }

}
