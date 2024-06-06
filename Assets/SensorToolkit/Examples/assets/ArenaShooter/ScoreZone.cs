using UnityEngine;
using System.Collections;

namespace Micosmo.SensorToolkit.Example
{
    /* When the pickup is detected in the score zone it's team wins a point. */
    public class ScoreZone : MonoBehaviour
    {
        public Sensor PickupSensor;
        public GameObject ScoreEffect;

        void Update()
        {
            var pickup = PickupSensor.GetNearestComponent<Holdable>();
            if (pickup != null)
            {
                // Pickup has been brought to score zone. It's team scores!
                if (ScoreEffect != null)
                {
                    Instantiate(ScoreEffect, pickup.transform.position, pickup.transform.rotation);
                }
                Destroy(pickup.gameObject);
            }
        }
    }
}