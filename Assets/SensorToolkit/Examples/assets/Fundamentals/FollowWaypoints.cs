using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit.Example {

    public class FollowWaypoints : MonoBehaviour {
        public List<Transform> Waypoints;
        public float WaitTime = 1f;

        [Header("Runtime State")]
        public Transform NextWaypoint;

        ISteeringSensor steering;

        void OnEnable() {
            steering = GetComponent<ISteeringSensor>();
            StartCoroutine(FollowWaypointsRoutine());
        }

        IEnumerator FollowWaypointsRoutine() {
            var currWaypointIndex = NextWaypoint != null ? Waypoints.IndexOf(NextWaypoint) : 0;
            if (currWaypointIndex < 0) {
                currWaypointIndex = 0;
            }

            while (true) {
                yield return null;
                if (currWaypointIndex >= Waypoints.Count) {
                    currWaypointIndex = 0;
                    continue;
                }
                var currWaypoint = Waypoints[currWaypointIndex];
                yield return SeekRoutine(currWaypoint);
                currWaypointIndex += 1;
            }
        }

        IEnumerator SeekRoutine(Transform destination) {
            steering.ArriveTo(destination);
            while (!steering.IsDestinationReached) {
                NextWaypoint = destination;
                yield return null;
            }
            yield return new WaitForSeconds(WaitTime);
        }
    }

}