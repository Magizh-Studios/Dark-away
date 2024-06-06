using UnityEngine;
using System.Collections;

namespace Micosmo.SensorToolkit.Example {
    public class PlayerInput : MonoBehaviour {

        public LocomotionSystem Locomotion;

        [Header("References")]
        public Rigidbody RB;
        public Sensor InteractionRange;
        public Holder Holder;

        void Update() {
            // Project mouse position onto worlds plane at y=0
            var mousePosScreen = Input.mousePosition;
            var camPosition = Camera.main.transform.position;
            var camForward = Camera.main.transform.forward;
            var camDepthToGroundPlane = -camPosition.y / camForward.y;
            mousePosScreen.z = camDepthToGroundPlane;
            var mousePosGroundPlane = Camera.main.ScreenToWorldPoint(mousePosScreen);
            Locomotion.Strafing.SetFaceTarget((mousePosGroundPlane - transform.position).normalized);

            // Pickup the pickup if its in range
            var pickup = InteractionRange.GetNearestComponent<Holdable>();
            if (pickup != null && !pickup.IsHeld) {
                Holder.PickUp(pickup);
            }
        }

        void FixedUpdate() {
            var horiz = Input.GetAxis("Horizontal");
            var vert = Input.GetAxis("Vertical");
            var vMove = new Vector3(horiz, 0, vert);
            Locomotion.CharacterSeek(RB, 10 * vMove, Vector3.up);
        }
    }
}