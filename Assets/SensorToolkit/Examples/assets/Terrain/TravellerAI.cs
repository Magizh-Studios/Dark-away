using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Micosmo.SensorToolkit.Extras;

namespace Micosmo.SensorToolkit.Example {

    public class TravellerAI : MonoBehaviour {

        public List<Transform> Waypoints;

        public SteeringSensor Steering;
        public NavMeshPathfinder Pathfinder;

        int i = 0;

        void Start() {
            ChooseNextWaypoint();
        }

        void Update() {
            var wp = Waypoints[i];
            Pathfinder.Target.Value = wp;

            if (!Pathfinder.IsPathReady) {
                Steering.Stop();
                return;
            }

            Steering.ArriveTo(Pathfinder.NextCorner);

            if (Pathfinder.IsDestinationReached) {
                ChooseNextWaypoint();
            }
        }

        void ChooseNextWaypoint() {
            Pathfinder.StopAndClear();
            i = Random.Range(0, Waypoints.Count);
        }
    }

}