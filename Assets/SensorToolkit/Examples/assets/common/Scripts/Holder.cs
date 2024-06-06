using UnityEngine;
using System.Collections;

namespace Micosmo.SensorToolkit.Example {
    
    public class Holder : MonoBehaviour {
        public Sensor InteractionRange;
        public GameObject HoldSlot;

        public bool IsInteracting { get; private set; }
        Holdable held;

        public Holdable Held => held;
        public bool IsHolding => held != null;

        public void PickUp(Holdable holdable) {
            if (held != null || IsInteracting || !InteractionRange.IsDetected(holdable.gameObject) || holdable.IsHeld) {
                return;
            } else {
                StartCoroutine(PickUpRoutine(holdable));
            }
        }

        public bool CanPickUp(Holdable holdable) => InteractionRange.IsDetected(holdable?.gameObject);

        IEnumerator PickUpRoutine(Holdable holdable) {
            float countdown = holdable.PickupTime;
            IsInteracting = true;

            while (countdown > 0f) {
                countdown -= Time.deltaTime;
                if (holdable.IsHeld || !InteractionRange.IsDetected(holdable.gameObject)) {
                    // Conditions have changed, holdable can no longer be picked up
                    IsInteracting = false;
                    yield break;
                }
                yield return null;
            }
            if (holdable.PickUp(this)) {
                held = holdable;
            }
            IsInteracting = false;
        }

        void Start() {
            IsInteracting = false;
            held = null;
        }
    }
}