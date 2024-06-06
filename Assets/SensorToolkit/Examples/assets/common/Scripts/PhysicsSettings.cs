using UnityEngine;
using System.Collections;

namespace Micosmo.SensorToolkit.Example {
    public class PhysicsSettings : MonoBehaviour {

        public Vector3 GravityForce = Vector3.down * 9.81f;

        void Awake() {
            Physics.autoSyncTransforms = false;
            Physics2D.autoSyncTransforms = false;

            Physics.gravity = GravityForce;
            Physics2D.gravity = GravityForce;

            // Make the IgnoreRaycast layer ignore itself, all trigger sensor volumes are put on this layer in the examples.
            Physics.IgnoreLayerCollision(2, 2, true);
            Physics2D.IgnoreLayerCollision(2, 2, true);
        }
    }
}