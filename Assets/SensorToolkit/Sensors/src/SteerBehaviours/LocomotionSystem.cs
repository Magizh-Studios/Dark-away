using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    public enum LocomotionMode { None, RigidBodyFlying, RigidBodyCharacter, UnityCharacterController }
    public enum LocomotionMode2D { None, RigidBody2D }

    [System.Serializable]
    public class LocomotionSystem {

        public float MaxForwardSpeed = 2f;
        public float MaxStrafeSpeed = 0.5f;
        public float MaxTurnSpeedDegrees = 360f;
        public float MaxAccel = 4f;
        public float MaxAngularAccelDegrees = 360f;

        public float MaxSpeedMultiplier = 1f;

        public LocomotionStrafeSettings Strafing;

        bool constrainMotion => true;

        float attenMaxSpeed => MaxForwardSpeed * MaxSpeedMultiplier;
        float attenMaxAccel => MaxAccel * MaxSpeedMultiplier;
        float attenMaxStrafeSpeed => MaxStrafeSpeed * MaxSpeedMultiplier;
        float attenMaxTurnSpeed => MaxTurnSpeedDegrees * MaxSpeedMultiplier;
        float attenMaxTurnAccel => MaxAngularAccelDegrees * MaxSpeedMultiplier;

        Vector3 kinematicVelocity;
        Vector3 kinematicAngularVelocity; // Should be degrees/second

        Vector2 kinematicVelocity2D;
        float kinematicAngularVelocity2D; // Should be degrees/second

        public void FlyableSeek(Rigidbody rb, Vector3 vSteer) {
            var dir = Strafing.GetFaceTarget(rb.transform, vSteer);
            if (rb.isKinematic) {
                FlyableSeekKinematic(rb, vSteer, dir, Vector3.up);
            } else {
                FlyableSeekWithForces(rb, vSteer, dir, Vector3.up);
            }
        }

        public void CharacterSeek(Rigidbody rb, Vector3 vSteer, Vector3 up) {
            var dir = Vector3.ProjectOnPlane(Strafing.GetFaceTarget(rb.transform, vSteer), up).normalized;
            if (rb.isKinematic) {
                CharacterSeekKinematic(rb, vSteer, dir, up);
            } else {
                CharacterSeekWithForces(rb, vSteer, dir, up);
            }
        }

        public void RigidBody2DSeek(Rigidbody2D rb, Vector2 vSteer) {
            var dir = Vector3.ProjectOnPlane(Strafing.GetFaceTarget(rb.transform, vSteer), Vector3.back).normalized;
            if (rb.isKinematic) {
                RigidBody2DSeekKinematic(rb, vSteer, dir);
            } else {
                RigidBody2DSeekWithForces(rb, vSteer, dir);
            }
        }

        public void CharacterSeek(CharacterController cc, Vector3 vSteer, Vector3 up) {
            var dir = Vector3.ProjectOnPlane(Strafing.GetFaceTarget(cc.transform, vSteer), up).normalized;
            CharacterControllerSeek(cc, vSteer, dir, up);
        }

        void FlyableSeekWithForces(Rigidbody rb, Vector3 vSteer, Vector3 tdir, Vector3 tup) {
            var angularAccel = MotionUtils.SeekAngularAccel(attenMaxTurnAccel, attenMaxTurnSpeed, Mathf.Rad2Deg * rb.angularVelocity, rb.rotation, Quaternion.LookRotation(tdir, tup));
            var transAccel = MotionUtils.SeekAccel(attenMaxSpeed, vSteer, rb.velocity);
            AccelerateForces(rb, angularAccel, transAccel);
        }

        void FlyableSeekKinematic(Rigidbody rb, Vector3 vSteer, Vector3 tdir, Vector3 tup) {
            var angularAccel = MotionUtils.SeekAngularAccel(attenMaxTurnAccel, attenMaxTurnSpeed, kinematicAngularVelocity, rb.rotation, Quaternion.LookRotation(tdir, tup));
            var transAccel = MotionUtils.SeekAccel(attenMaxSpeed, vSteer, kinematicVelocity);
            AccelerateKinematic(rb, angularAccel, transAccel);
        }

        void CharacterSeekWithForces(Rigidbody rb, Vector3 vSteer, Vector3 tdir, Vector3 tup) {
            var angularAccel = MotionUtils.SeekAngularAccel(attenMaxTurnAccel, attenMaxTurnSpeed, Mathf.Rad2Deg * rb.angularVelocity, rb.rotation, Quaternion.LookRotation(tdir, tup));
            var transAccel = MotionUtils.SeekAccel(attenMaxSpeed, vSteer, rb.velocity);
            AccelerateForces(rb, angularAccel, transAccel);
        }

        void CharacterSeekKinematic(Rigidbody rb, Vector3 vSteer, Vector3 tdir, Vector3 tup) {
            var angularAccel = MotionUtils.SeekAngularAccel(attenMaxTurnAccel, attenMaxTurnSpeed, kinematicAngularVelocity, rb.rotation, Quaternion.LookRotation(tdir, tup));
            var transAccel = MotionUtils.SeekAccel(attenMaxSpeed, vSteer, kinematicVelocity);
            AccelerateKinematic(rb, angularAccel, transAccel);
        }

        void CharacterControllerSeek(CharacterController cc, Vector3 vSteer, Vector3 tdir, Vector3 tup) {
            var angularAccel = MotionUtils.SeekAngularAccel(attenMaxTurnAccel, attenMaxTurnSpeed, kinematicAngularVelocity, cc.transform.rotation, Quaternion.LookRotation(tdir, tup));
            var transAccel = MotionUtils.SeekAccel(attenMaxSpeed, vSteer, kinematicVelocity);
            AccelerateCharacterController(cc, angularAccel, transAccel);
        }

        void RigidBody2DSeekWithForces(Rigidbody2D rb, Vector2 vSteer, Vector2 tdir) {
            var angularAccel = MotionUtils.SeekAngularAccel2D(attenMaxTurnAccel, attenMaxTurnSpeed, rb.angularVelocity, rb.transform.up, tdir);
            var transAccel = MotionUtils.SeekAccel(attenMaxSpeed, vSteer, rb.velocity);
            AccelerateForces(rb, angularAccel, transAccel);
        }

        void RigidBody2DSeekKinematic(Rigidbody2D rb, Vector2 vSteer, Vector2 tdir) {
            var angularAccel = MotionUtils.SeekAngularAccel2D(attenMaxTurnAccel, attenMaxTurnSpeed, kinematicAngularVelocity2D, rb.transform.up, tdir);
            var transAccel = MotionUtils.SeekAccel(attenMaxSpeed, vSteer, kinematicVelocity2D);
            AccelerateKinematic(rb, angularAccel, transAccel);
        }

        void AccelerateForces(Rigidbody rb, Vector3 angularAccel, Vector3 transAccel) {
            if (constrainMotion) {
                angularAccel = Vector3.ClampMagnitude(angularAccel, attenMaxTurnAccel);
                transAccel = Vector3.ClampMagnitude(transAccel, attenMaxAccel);
            }

            // check if angularAccel is NaN
            if (angularAccel.sqrMagnitude > 0) {
                rb.AddTorque(Mathf.Deg2Rad * angularAccel * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }

            rb.AddForce(transAccel * rb.mass);

            if (constrainMotion) {
                rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, Mathf.Deg2Rad * attenMaxTurnSpeed);

                var vel = rb.velocity;
                var dirDotForward = Vector3.Dot(vel.normalized, rb.transform.forward);
                var maxVel = Mathf.Abs(dirDotForward) * attenMaxSpeed + (1f - Mathf.Abs(dirDotForward)) * attenMaxStrafeSpeed;
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVel);
            }
        }

        void AccelerateKinematic(Rigidbody rb, Vector3 angularAccel, Vector3 transAccel) {
            if (constrainMotion) {
                angularAccel = Vector3.ClampMagnitude(angularAccel, attenMaxTurnAccel);
                transAccel = Vector3.ClampMagnitude(transAccel, attenMaxAccel);
            }

            // check if angularAccel is NaN
            if (angularAccel.sqrMagnitude > 0) {
                kinematicAngularVelocity += angularAccel * Time.fixedDeltaTime;
            }
            
            kinematicVelocity += transAccel * Time.fixedDeltaTime;

            if (constrainMotion) {
                kinematicAngularVelocity = Vector3.ClampMagnitude(kinematicAngularVelocity, attenMaxTurnSpeed);

                var dirDotForward = Vector3.Dot(kinematicVelocity.normalized, rb.transform.forward);
                var maxVel = Mathf.Abs(dirDotForward) * attenMaxSpeed + (1f - Mathf.Abs(dirDotForward)) * attenMaxStrafeSpeed;
                kinematicVelocity = Vector3.ClampMagnitude(kinematicVelocity, maxVel);
            }

            rb.rotation = Quaternion.AngleAxis(kinematicAngularVelocity.magnitude * Time.fixedDeltaTime, kinematicAngularVelocity.normalized) * rb.rotation;
            rb.position = rb.position + kinematicVelocity * Time.fixedDeltaTime;
        }

        void AccelerateCharacterController(CharacterController cc, Vector3 angularAccel, Vector3 transAccel) {
            if (constrainMotion) {
                angularAccel = Vector3.ClampMagnitude(angularAccel, attenMaxTurnAccel);
                transAccel = Vector3.ClampMagnitude(transAccel, attenMaxAccel);
            }
            kinematicAngularVelocity += angularAccel * Time.deltaTime;
            kinematicVelocity = Vector3.Lerp(kinematicVelocity, cc.velocity, Time.deltaTime * 8f);
            kinematicVelocity += transAccel * Time.deltaTime;

            if (constrainMotion) {
                kinematicAngularVelocity = Vector3.ClampMagnitude(kinematicAngularVelocity, attenMaxTurnSpeed);

                var dirDotForward = Vector3.Dot(kinematicVelocity.normalized, cc.transform.forward);
                var maxVel = Mathf.Abs(dirDotForward) * attenMaxSpeed + (1f - Mathf.Abs(dirDotForward)) * attenMaxStrafeSpeed;
                kinematicVelocity = Vector3.ClampMagnitude(kinematicVelocity, maxVel);
            }

            cc.transform.rotation = Quaternion.AngleAxis(kinematicAngularVelocity.magnitude * Time.deltaTime, kinematicAngularVelocity.normalized) * cc.transform.rotation;
            cc.SimpleMove(kinematicVelocity);
        }

        void AccelerateForces(Rigidbody2D rb, float angularAccel, Vector2 transAccel) {
            if (constrainMotion) {
                angularAccel = Mathf.Clamp(angularAccel, -attenMaxTurnAccel, attenMaxTurnAccel);
                transAccel = Vector3.ClampMagnitude(transAccel, attenMaxAccel);
            }
            rb.AddTorque(angularAccel * rb.mass);
            rb.AddForce(transAccel * rb.mass);

            if (constrainMotion) {
                rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -attenMaxTurnSpeed, attenMaxTurnSpeed);

                var vel = rb.velocity;
                var dirDotForward = Vector3.Dot(vel.normalized, rb.transform.up);
                var maxVel = Mathf.Abs(dirDotForward) * attenMaxSpeed + (1f - Mathf.Abs(dirDotForward)) * attenMaxStrafeSpeed;
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVel);
            }
        }

        void AccelerateKinematic(Rigidbody2D rb, float angularAccel, Vector2 transAccel) {
            if (constrainMotion) {
                angularAccel = Mathf.Clamp(angularAccel, -attenMaxTurnAccel, attenMaxTurnAccel);
                transAccel = Vector3.ClampMagnitude(transAccel, attenMaxAccel);
            }
            kinematicAngularVelocity2D += angularAccel * Time.fixedDeltaTime;
            kinematicVelocity2D += transAccel * Time.fixedDeltaTime;

            if (constrainMotion) {
                kinematicAngularVelocity2D = Mathf.Clamp(kinematicAngularVelocity2D, -attenMaxTurnSpeed, attenMaxTurnSpeed);

                var dirDotForward = Vector2.Dot(kinematicVelocity2D.normalized, rb.transform.up);
                var maxVel = Mathf.Abs(dirDotForward) * attenMaxSpeed + (1f - Mathf.Abs(dirDotForward)) * attenMaxStrafeSpeed;
                kinematicVelocity2D = Vector3.ClampMagnitude(kinematicVelocity2D, maxVel);
            }

            rb.rotation = rb.rotation + kinematicAngularVelocity2D * Time.fixedDeltaTime;
            rb.position = rb.position + kinematicVelocity2D * Time.fixedDeltaTime;
        }
    }

    [System.Serializable]
    public struct LocomotionStrafeSettings {
        [SerializeField]
        Transform targetTransform;
        [SerializeField]
        Vector3 targetDirection;

        public void SetFaceTarget(Vector3 direction) {
            targetTransform = null;
            targetDirection = direction;
        }

        public void SetFaceTarget(Transform target) {
            targetTransform = target;
            targetDirection = Vector3.zero;
        }

        public void Clear() {
            targetTransform = null;
            targetDirection = Vector3.zero;
        }

        public Vector3 GetFaceTarget(Transform forTransform, Vector3 vSteer) {
            if (targetTransform != null) {
                return (targetTransform.position - forTransform.position).normalized;
            } else if (targetDirection != Vector3.zero) {
                return targetDirection.normalized;
            }
            if (vSteer != Vector3.zero) {
                return vSteer.normalized;
            }
            return forTransform.forward;
        }
    }
}