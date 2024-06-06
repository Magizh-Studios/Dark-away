using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    [System.Serializable]
    public struct ReferenceFrame {
        public Vector3 Forward, Right, Up;
        public ReferenceFrame(Vector3 forward, Vector3 right, Vector3 up) {
            Forward = forward; Right = right; Up = up;
        }
        public ReferenceFrame(Transform transform) {
            Forward = transform.forward; Right = transform.right; Up = transform.up;
        }
    }

    public static class MotionUtils {

        public static float SeekAccel(float maxSpeed, float vSteer, float velocity) {
            var targetVelocity = Mathf.Clamp(vSteer, -maxSpeed, maxSpeed);
            var accel = (targetVelocity - velocity) / Time.deltaTime;
            return accel;
        }

        public static Vector2 SeekAccel(float maxSpeed, Vector2 vSteer, Vector2 velocity) {
            var targetVelocity = Vector2.ClampMagnitude(vSteer, maxSpeed);
            var accel = (targetVelocity - velocity) / Time.deltaTime;
            return accel;
        }

        public static Vector3 SeekAccel(float maxSpeed, Vector3 vSteer, Vector3 velocity) {
            var targetVelocity = Vector3.ClampMagnitude(vSteer, maxSpeed);
            var accel = (targetVelocity - velocity) / Time.deltaTime;
            return accel;
        }

        public static Vector3 SeekAngularAccel(float maxAccel, float maxSpeed, Vector3 angularVelocity, Quaternion currRot, Quaternion targetRot) {
            var delta = targetRot * Quaternion.Inverse(currRot);
            delta.ToAngleAxis(out var angle, out var axis);
            if (float.IsNaN(axis.sqrMagnitude)) {
                axis = angularVelocity.normalized;
            }
            angle = Mathf.DeltaAngle(0f, angle);
            var distance = Mathf.Abs(angle);
            var stoppingDistance = StoppingDistance(maxSpeed, maxAccel);
            var targetAngularSpeed = Mathf.Min(maxSpeed, maxSpeed * (distance / stoppingDistance));
            var targetAngularVelocity = Mathf.Sign(angle) * targetAngularSpeed * axis;
            var torque = (targetAngularVelocity - angularVelocity) / Time.deltaTime;
            return Vector3.ClampMagnitude(torque, maxAccel);
        }

        public static float SeekAngularAccel2D(float maxAccel, float maxSpeed, float angularVelocity, Vector2 currDir, Vector2 targetDir) {
            var dAngle = Vector2.SignedAngle(currDir, targetDir);
            var distance = Mathf.Abs(dAngle);
            var stoppingDistance = StoppingDistance(maxSpeed, maxAccel);
            var arrive = Mathf.Clamp01(distance / stoppingDistance);
            return SeekAccel(maxSpeed, (arrive * Mathf.Sign(dAngle)) * maxSpeed, angularVelocity);
        }

        public static float StoppingDistance(float velocity, float accel) {
            if (accel == 0) return 0;
            var d = velocity * velocity / (2 * accel);
            return d * 1.1f;
        }

    }

}