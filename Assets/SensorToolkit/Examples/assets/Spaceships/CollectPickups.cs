using UnityEngine;
using System.Collections;

namespace Micosmo.SensorToolkit.Example
{
    public class CollectPickups : MonoBehaviour
    {
        public Sensor PickupSensor;
        public Sensor InteractionRange;
        public SteeringSensor Steering;
        public IgnoreProcessor IgnoreProc;

        Holdable target;

        void Update()
        {
            // If we don't currently have a target pickup then target the nearest one. If we do
            // have a target and we are within detection range then pick it up.
            if (target == null)
            {
                target = PickupSensor.GetNearestComponent<Holdable>();
                if (target != null)
                {
                    Steering.SeekTo(target.transform);
                    IgnoreProc.ToIgnore = target.gameObject;
                }
            }
            else if (InteractionRange.IsDetected(target.gameObject))
            {
                // Pickup the target.. (Destroy it to show it has been picked up)
                Destroy(target.gameObject);
                target = null;
            }
        }
    }
}