using UnityEngine;
using System.Collections;

namespace Micosmo.SensorToolkit.Example {
    public class Bullet : MonoBehaviour {
        public float Speed;
        public float Damage;
        public float MaxAge;
        public float ImpactForce;
        [Header("References")]
        public RaySensor RaySensor;
        public GameObject HitEffect;

        float age;

        void Start() {
            RaySensor.PulseMode = PulseRoutine.Modes.Manual;
            age = 0;
        }

        void Update() {
            age += Time.deltaTime;
            if (age > MaxAge) {
                explode(Vector3.up);
                return;
            }

            var deltaPos = transform.forward * Speed * Time.deltaTime;
            RaySensor.Length = deltaPos.magnitude;
            RaySensor.Pulse();

            var nearestDetection = RaySensor.GetNearestDetection();
            if (nearestDetection != null) {
                HitObject(nearestDetection);
            } else if (RaySensor.IsObstructed) {
                HitWall();
            } else {
                transform.position += deltaPos;
            }
        }

        void HitObject(GameObject g) {
            var health = g.GetComponent<Health>();
            if (health != null) {
                health.Impact(Damage, transform.forward * ImpactForce, RaySensor.GetDetectionRayHit(g).Point);
            }
            explode(RaySensor.GetDetectionRayHit(g).Normal);
        }

        void HitWall() {
            explode(RaySensor.GetObstructionRayHit().Normal);
        }

        void explode(Vector3 direction) {
            if (HitEffect != null) {
                Instantiate(HitEffect, transform.position, Quaternion.LookRotation(direction));
            }
            Destroy(gameObject);
        }
    }
}