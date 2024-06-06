using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    [System.Serializable]
    public struct VelocityObstacle {

        public Vector3 Velocity; // Global velocity of obstacle.
        public Vector3 Center;   // Position of obstacle relative to agent.
        public float Radius;     // Obstacle Radius + Agent Radius.

        public VelocityObstacle(Vector3 velocity, Vector3 center, float radius) {
            Velocity = velocity;
            Center = center;
            Radius = radius;
        }

        /**
         * agentVel should be world space velocity of agent. Will return true if a collision will occur in the
         * future. The time until collision will be stored in 'time'.
         */
        public bool TryGetCollisionTime(Vector3 agentVel, out float time) {
            var agentRelVel = agentVel - Velocity;
            var cross = Vector3.Cross(agentRelVel, Center);
            var discr = -(cross.sqrMagnitude) + (Radius * Radius) * agentRelVel.sqrMagnitude;

            if (discr < 0) {
                time = float.PositiveInfinity;
                return false;
            }

            time = (Vector3.Dot(agentRelVel, Center) - Mathf.Sqrt(discr)) / agentRelVel.sqrMagnitude;
            return time >= 0;
        }

        public bool ContainsVelocity(Vector3 agentVel, float timeHorizon) {
            if (TryGetCollisionTime(agentVel, out var time)) {
                return time <= timeHorizon;
            }
            return false;
        }

        public void DrawGizmos() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Center, Radius);
            Gizmos.DrawLine(Center, Center + Velocity);
        }

    }

}
