using UnityEngine;
using System.Collections;

namespace Micosmo.SensorToolkit.Example {

    public class Holdable : MonoBehaviour {
        public float PickupTime;

        public Holder Holder { get; private set; }
        public bool IsHeld => Holder != null;

        public bool PickUp(Holder holder) {
            if (IsHeld || holder == null) {
                return false;
            }

            Drop();

            Holder = holder;

            transform.position = holder.HoldSlot.transform.position;
            transform.rotation = holder.HoldSlot.transform.rotation;

            joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = holder.GetComponent<Rigidbody>();

            return true;
        }

        public void Drop() {
            Holder = null;
            if (joint != null) {
                Destroy(joint);
            }
        }

        FixedJoint joint;

        void Update() {
            if (!IsHeld && joint != null) {
                Drop();
            }
        }
    }
}