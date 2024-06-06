using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Micosmo.SensorToolkit.Extras {

    public class NavMeshPathfinder : MonoBehaviour {

        enum PathfinderMode { None, SeekTransform, SeekPosition };

        [Header("Configuration")]
        public float AgentRadius = .5f;
        public float AgentHeight = 2f;
        public float RecalculateTime = 5f;

        [Header("Destination")]
        [SerializeField]
        ObservableTransform target = new ObservableTransform();
        public ObservableTransform Target => target;

        [Header("Runtime State (Dont touch)")]
        public bool IsPathReady;
        public float RemainingDistance;
        public bool IsDestinationReached;

        public Vector3 TargetPosition {
            get {
                if (mode == PathfinderMode.SeekTransform) {
                    return Target.Value != null ? Target.Value.position : transform.position;
                } else if (mode == PathfinderMode.SeekPosition) {
                    return targetPosition;
                }
                return transform.position;
            }
        }

        Vector3 targetPosition;
        PathfinderMode mode = PathfinderMode.None;

        public Vector3 NextCorner {
            get {
                if (!IsPathReady) {
                    return Vector3.zero;
                }
                storeCorners();
                var nextCorner = corners[1];
                if (pathLength > 2 && (nextCorner-transform.position).magnitude < AgentRadius) {
                    nextCorner = corners[2];
                }
                return nextCorner;
            }
        }

        NavMeshAgent agent;
        Vector3[] corners = new Vector3[100];
        Coroutine calculatePathRoutineInstance;
        int pathLength = 0;

        public void SetTargetTransform(Transform target) {
            Target.Value = target;
        }

        public void SetTargetPosition(Vector3 p) {
            Target.Value = null;
            if (mode != PathfinderMode.SeekPosition && targetPosition != p) {
                targetPosition = p;
                mode = PathfinderMode.SeekPosition;
                RestartPathfinderRoutine();
            }
        }

        public void StopAndClear() {
            Target.Value = null;
            mode = PathfinderMode.None;
            StopPathfinderRoutine();
        }

        void Awake() {
            GameObject agentGO = new GameObject("AINavigation Agent");
            agentGO.transform.SetParent(transform, false);
            agent = agentGO.AddComponent<NavMeshAgent>();
            agent.agentTypeID = 0;
            agent.baseOffset = 0;
            agent.speed = 0;
            agent.angularSpeed = 0;
            agent.acceleration = 0;
            agent.stoppingDistance = 0;
            agent.autoBraking = false;
            agent.radius = AgentRadius;
            agent.height = AgentHeight;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = 50;

            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        void OnDestroy() {
            Destroy(agent.gameObject);
        }

        void OnEnable() {
            RestartPathfinderRoutine();
            Target.OnChanged += TargetChangeHandler;
            TargetChangeHandler();
        }

        void OnDisable() {
            Target.OnChanged -= TargetChangeHandler;
        }

        void Update() {
            agent.nextPosition = transform.position;

            WarpOnStuck();

            RemainingDistance = IsPathReady ? agent.remainingDistance : 0f;
            IsDestinationReached = IsPathReady && (TargetPosition - transform.position).magnitude < AgentRadius;
        }

        void StopPathfinderRoutine() {
            IsPathReady = false;
            RemainingDistance = 0f;
            IsDestinationReached = false;
            if (calculatePathRoutineInstance != null) {
                StopCoroutine(calculatePathRoutineInstance);
            }
        }

        void RestartPathfinderRoutine() {
            StopPathfinderRoutine();
            calculatePathRoutineInstance = StartCoroutine(calculatePathRoutine());
        }

        float stuckTimer = 0f;
        void WarpOnStuck() {
            var delta = agent.nextPosition - transform.position;
            var xzDist = new Vector2(delta.x, delta.z).magnitude;
            var yDist = Mathf.Abs(delta.y);
            if (xzDist > AgentRadius || yDist > AgentHeight) {
                stuckTimer += Time.deltaTime;
            } else {
                stuckTimer = 0f;
            }

            if (stuckTimer >= 1f) {
                var isSuccess = agent.Warp(transform.position);
                Debug.LogWarning($"The NavMeshAgent's position is out-of-sync, attempted to warp to gameobject. Success: {isSuccess}", gameObject);
                stuckTimer = 0f;
                RestartPathfinderRoutine();
            }
        }

        IEnumerator calculatePathRoutine() {
            IsPathReady = false;
            agent.Warp(transform.position);

            while (true) {
                if (IsDestinationReached) {
                    yield return null;
                    continue;
                }

                agent.SetDestination(TargetPosition);

                while (agent.pathPending) {
                    IsPathReady = false;
                    yield return null;
                }

                storeCorners();
                IsPathReady = true;

                yield return new WaitForSeconds(RecalculateTime);
            }
        }

        void storeCorners() {
            while (true) {
                pathLength = agent.path.GetCornersNonAlloc(corners);
                if (pathLength < corners.Length) {
                    break;
                }

                // Gotta try again
                corners = new Vector3[corners.Length * 2];
            }
        }

        void TargetChangeHandler() {
            if (Target.Value != null) {
                mode = PathfinderMode.SeekTransform;
                RestartPathfinderRoutine();
            }
        }

        void OnDrawGizmosSelected() {
            if (!IsPathReady) {
                return;
            }

            for (int i = 0; i < pathLength; i++) {
                if (i == 0) {
                    continue;
                }

                var corner = corners[i];
                var prevCorner = corners[i - 1];

                SensorGizmos.PushColor(i == 1 ? Color.green : STPrefs.defaultCyan);

                Gizmos.DrawLine(prevCorner, corner);
                Gizmos.DrawCube(corner, Vector3.one * 0.2f);

                SensorGizmos.PopColor();
            }
        }
    }

}