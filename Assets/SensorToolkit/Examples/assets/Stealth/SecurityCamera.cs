using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Micosmo.SensorToolkit.Extras;

namespace Micosmo.SensorToolkit.Example {
    public class SecurityCamera : StupidSimpleFSM {
        public float RotationSpeed;
        public float ScanTime;
        public float TrackTime;
        public float ScanArcAngle;
        public Color ScanColour;
        public Color TrackColour;
        public Color AlarmColour;

        [Header("References")]
        public TeamMember TeamMember;
        public Light SpotLight;
        public Sensor Sensor;

        Quaternion leftExtreme;
        Quaternion rightExtreme;
        Quaternion targetRotation;

        void Awake() {
            leftExtreme = Quaternion.AngleAxis(ScanArcAngle / 2f, Vector3.up) * transform.rotation;
            rightExtreme = Quaternion.AngleAxis(-ScanArcAngle / 2f, Vector3.up) * transform.rotation;
        }

        void OnEnable() {
            targetRotation = transform.rotation;
            transform.rotation = rightExtreme;
            SetFSMState(ChooseScanDirectionState());
        }

        void Update() {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }

        IEnumerator ChooseScanDirectionState() {
            if (targetRotation == leftExtreme) {
                targetRotation = rightExtreme;
            } else {
                targetRotation = leftExtreme;
            }
            yield return SetFSMState(ScanState());
        }

        IEnumerator ScanState() {
            var timer = ScanTime;
            SpotLight.color = ScanColour;
            while (timer >= 0) {
                timer -= Time.deltaTime;
                yield return null;

                if (AlarmController.Instance.IsAlarmState) {
                    yield return SetFSMState(AlarmState());
                }
                var nearestEnemy = Sensor.GetNearestDetection(s => TeamMember.IsEnemy(s.Object));
                if (nearestEnemy != null) {
                    yield return SetFSMState(TrackState(nearestEnemy));
                }
            }

            yield return SetFSMState(ChooseScanDirectionState());
        }

        IEnumerator TrackState(GameObject target) {
            SpotLight.color = TrackColour;
            var timer = TrackTime;

            while (timer > 0f) {
                timer -= Time.deltaTime;
                yield return null;

                if (!Sensor.IsDetected(target)) {
                    yield return SetFSMState(ChooseScanDirectionState());
                }

                targetRotation = Quaternion.LookRotation(target.transform.position - transform.position, Vector3.up);
            }

            AlarmController.Instance.StartAlarm(target);
            yield return SetFSMState(AlarmState());
        }

        IEnumerator AlarmState() {
            targetRotation = transform.rotation;
            SpotLight.color = AlarmColour;
            yield return null;
        }
    }
}