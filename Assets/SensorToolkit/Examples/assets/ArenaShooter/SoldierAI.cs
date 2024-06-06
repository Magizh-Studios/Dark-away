using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Micosmo.SensorToolkit.Extras;

namespace Micosmo.SensorToolkit.Example {

    public class SoldierAI : StupidSimpleFSM {
        [Header("References")]
        public Sensor Sight;
        public SteeringSensor SteerSensor;
        public IgnoreProcessor IgnoreProc;
        public Holder Holder;
        public GunWithClip Gun;
        public TeamMember TeamMember;

        GameObject myBase;

        void OnEnable() {
            SetFSMState(DefaultState());
        }

        void Start() {
            myBase = GameObject.Find(TeamMember.Team == Teams.Yellow ? "YellowBase" : "MagentaBase");
        }

        void Update() {
            SteerSensor.Locomotion.MaxSpeedMultiplier = Holder.IsHolding ? .5f : 1f;
        }

        IEnumerator DefaultState() {
            while (true) {
                yield return null;

                if (Holder.Held != null) {
                    yield return SetFSMState(CarryToBaseState());
                }

                var nearestPickup = Sight.GetNearestComponent<Holdable>();
                if (nearestPickup != null) {
                    if (nearestPickup.IsHeld && TeamMember.IsEnemy(nearestPickup.Holder.gameObject)) {
                        yield return SetFSMState(AttackState(nearestPickup.Holder.gameObject));
                    } else if (Random.value > 0.9f) {
                        yield return SetFSMState(PickUpState(nearestPickup));
                    }
                }

                var nearestEnemy = Sight.GetNearestDetection(s => TeamMember.IsEnemy(s.Object));
                if (nearestEnemy != null) {
                    yield return SetFSMState(AttackState(nearestEnemy));
                }

                float countdown = 1f;
                while (countdown > 0f) {
                    // Make way to the center of the map
                    SteerSensor.ArriveTo(Vector3.zero);
                    SteerSensor.Locomotion.Strafing.Clear();
                    countdown -= Time.deltaTime;
                    yield return null;
                }
            }
        }

        IEnumerator AttackState(GameObject target) {
            if (target == null) {
                yield return SetFSMState(DefaultState());
            } else if ((target.transform.position - transform.position).magnitude > 10f) {
                yield return SetFSMState(ChargeState(target));
            } else {
                yield return SetFSMState(StrafeState(target));
            }
        }

        IEnumerator ChargeState(GameObject target) {
            var timer = Random.Range(0.5f, 2f);
            while (timer >= 0f) {
                timer -= Time.deltaTime;
                yield return null;

                if (target == null) {
                    break;
                }
                SteerSensor.SeekTo(target.transform);
                SteerSensor.Locomotion.Strafing.Clear();

                Gun.Fire();
                if (Gun.IsEmptyClip) {
                    Gun.Reload();
                }
            }
            yield return SetFSMState(DefaultState());
        }

        IEnumerator StrafeState(GameObject target) {
            var timer = Random.Range(0.5f, 2f);
            var strafeDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            while (timer > 0f) {
                timer -= Time.deltaTime;
                yield return null;

                if (target == null) {
                    break;
                }

                SteerSensor.SeekTo(transform.position + strafeDirection * 10);
                SteerSensor.Locomotion.Strafing.SetFaceTarget(target.transform);

                Gun.Fire();
                if (Gun.IsEmptyClip && !Gun.IsReloading) {
                    Gun.Reload();
                    if (Random.value > 0.5f) {
                        yield return SetFSMState(FleeState());
                    }
                }
            }
            yield return SetFSMState(DefaultState());
        }

        IEnumerator FleeState() {
            var timer = Random.Range(2f, 4f);

            SteerSensor.Locomotion.Strafing.Clear();

            while (timer > 0) {
                timer -= Time.deltaTime;
                yield return null;

                var nearestEnemy = Sight.GetNearestDetection(s => TeamMember.IsEnemy(s.Object));
                if (nearestEnemy == null) {
                    break;
                }

                SteerSensor.SeekTo(nearestEnemy.transform, 20f); // Seek position 20-units away from target. This will cause flee
            }

            SteerSensor.Stop();

            yield return SetFSMState(DefaultState());
        }

        IEnumerator PickUpState(Holdable pickup) {
            float timer = 4f;
            while (timer > 0f) {
                timer -= Time.deltaTime;
                yield return null;

                if (pickup == null || pickup.IsHeld) {
                    yield return SetFSMState(DefaultState());
                }

                IgnoreProc.ToIgnore = pickup.gameObject;
                SteerSensor.ArriveTo(pickup.transform);
                SteerSensor.Locomotion.Strafing.Clear();
                if (Holder.CanPickUp(pickup)) {
                    Holder.PickUp(pickup);
                    break;
                }
            }

            while (Holder.IsInteracting) {
                yield return null;
            }

            yield return SetFSMState(DefaultState());
        }

        IEnumerator CarryToBaseState() {
            while (true) {
                yield return null;

                if (Holder.Held == null) {
                    yield return SetFSMState(DefaultState());
                }

                SteerSensor.ArriveTo(myBase.transform);
                SteerSensor.Locomotion.Strafing.Clear();
            }
        }
    }
}