using UnityEngine;
using System.Collections;

namespace Micosmo.SensorToolkit.Example
{
    [RequireComponent(typeof(Rigidbody))]
    public class Asteroid : MonoBehaviour
    {
        public float MaxRandomSpin;
        public float MaxRandomForce;
        public float BoundaryRadius;
        public float ReturnForce;
        public float ReturnForceLerpDistance;

        Rigidbody rb;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.AddTorque(randomVector() * Random.Range(0f, MaxRandomSpin));
            rb.AddForce(randomVector() * Random.Range(0f, MaxRandomForce));
        }

        void Update()
        {
            var distFromOrigin = transform.position.magnitude;
            if (distFromOrigin >= BoundaryRadius)
            {
                var f = Mathf.Lerp(0f, ReturnForce, (distFromOrigin - BoundaryRadius) / ReturnForceLerpDistance);
                rb.AddForce(f * -transform.position.normalized);
            }
        }

        Vector3 randomVector()
        {
            return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        }
    }
}