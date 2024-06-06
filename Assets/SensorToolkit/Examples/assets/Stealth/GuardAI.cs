using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Micosmo.SensorToolkit.Extras;

namespace Micosmo.SensorToolkit.Example {
    public class GuardAI : StupidSimpleFSM {
        public Transform[] PatrolPath;
        public float WaypointArriveDistance;
        public float PauseTime;
        public float WanderDistance;
        public float SoundAlarmTime;

        [Header("References")]
        public GameObject GunPivot;
        public SteeringSensor Steering;
        public NavMeshPathfinder Pathfinder;
        public Sensor Sight;
        public GunWithClip gun;
        public TeamMember team;

        bool ascending = true;

        void OnEnable() {
            SetFSMState(PatrolState());
        }

        Coroutine CheckHostileTransitions() {
            var nearestEnemy = Sight.GetNearestDetection(s => team.IsEnemy(s.Object));
            if (nearestEnemy != null) {
                return SetFSMState(AttackState(nearestEnemy));
            }
            if (AlarmController.Instance.IsAlarmState) {
                return SetFSMState(Chase());
            }
            return null;
        }

        IEnumerator PatrolState() {
            var nextWaypoint = GetNearestWaypointIndex();

            while (true) {
                yield return CheckHostileTransitions();

                Steering.ArriveTo(PatrolPath[nextWaypoint]);
                if ((transform.position - PatrolPath[nextWaypoint].position).magnitude < WaypointArriveDistance) {
                    // We've arrived at our target waypoint. Select the next waypoint.
                    nextWaypoint = ascending ? nextWaypoint + 1 : nextWaypoint - 1;
                    // If this was the last waypoint in the sequence then pause for a moment before following
                    // the waypoints in reverse.
                    if (nextWaypoint >= PatrolPath.Length || nextWaypoint < 0) {
                        ascending = !ascending;
                        yield return SetFSMState(PauseState());
                    }
                }
            }
        }

        IEnumerator PauseState() {
            Steering.ArriveTo(transform.position + WanderVector());
            float timer = PauseTime;
            while (timer > 0f) {
                timer -= Time.deltaTime;
                yield return CheckHostileTransitions();
            }
            yield return SetFSMState(PatrolState());
        }

        IEnumerator AttackState(GameObject ToAttack) {
            Steering.Stop();
            Steering.Locomotion.Strafing.SetFaceTarget(ToAttack.transform);
            var alarmTimer = SoundAlarmTime;

            while (true) {
                yield return null;

                if (ToAttack == null) {
                    yield return SetFSMState(PauseState());
                }
                if (!Sight.IsDetected(ToAttack)) {
                    break;
                }

                alarmTimer -= Time.deltaTime;
                if (alarmTimer <= 0f) {
                    AlarmController.Instance.StartAlarm(ToAttack);
                }

                // Rotate the gun in hand to face the enemy, reload if empty, otherwise fire the gun.
                GunPivot.transform.LookAt(new Vector3(ToAttack.transform.position.x, GunPivot.transform.position.y, ToAttack.transform.position.z));
                if (gun.IsEmptyClip) {
                    gun.Reload();
                } else {
                    gun.Fire();
                }
            }

            Steering.Locomotion.Strafing.Clear();
            GunPivot.transform.localRotation = Quaternion.identity; // Return gun rotation back to resting position
            yield return SetFSMState(Investigate(ToAttack.transform.position));
        }

        IEnumerator Investigate(Vector3 position) {
            Steering.ArriveTo(position);
            float timer = 5f;

            while (timer > 0f) {
                timer -= Time.deltaTime;
                yield return CheckHostileTransitions();

                if (Steering.IsDestinationReached) {
                    break;
                }
            }

            yield return SetFSMState(PauseState());
        }

        IEnumerator Chase() {
            while (true) {
                yield return CheckHostileTransitions();

                if (AlarmController.Instance.WhoTrippedAlarm == null) {
                    yield return SetFSMState(PauseState());
                }

                Pathfinder.SetTargetTransform(AlarmController.Instance.WhoTrippedAlarm.transform);
                if (Pathfinder.IsPathReady) {
                    Steering.SeekTo(Pathfinder.NextCorner);
                }
            }
        }

        int GetNearestWaypointIndex() {
            float nearestDist = 0f;
            int nearest = -1;
            for (int i = 0; i < PatrolPath.Length; i++) {
                var dist = (transform.position - PatrolPath[i].position).sqrMagnitude;
                if (dist < nearestDist || nearest == -1) {
                    nearest = i;
                    nearestDist = dist;
                }
            }
            return nearest;
        }

        Vector3 WanderVector() {
            var rv = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            return rv * WanderDistance;
        }
    }
    
}