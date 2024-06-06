using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Micosmo.SensorToolkit {

    public class NavMeshMaskAttribute : PropertyAttribute { }

    /**
     * The NavMesh Sensor is a simple component that can detect certain features in a Unity NavMesh. It's a simple 
     * wrapper around the built-in navmesh functions: NavMesh.Raycast, NavMesh.FindClosestEdge and 
     * NavMesh.SamplePosition. It doesn't detect Signals and therefore it's not derived from Sensor.
     */
    [AddComponentMenu("Sensors/NavMesh Sensor")]
    [HelpURL("https://micosmo.com/sensortoolkit2/docs/manual/sensors/navmesh")]
    public class NavMeshSensor : BasePulsableSensor, IRayCastingSensor, IPulseRoutine {

        #region Configurations
        public enum TestType { Ray, Sample, ClosestEdge }

        [Tooltip("Which nav mesh function to use.")]
        public TestType Test;

        // Configurations for ray-shaped navmesh tests
        public RayShape Ray = new RayShape(5f, Vector3.forward, false);

        // Configurations for sphere-shaped navmesh tests
        public SphereShape Sphere = new SphereShape(4f);

        [NavMeshMask]
        [Tooltip("Bitmask over the navmesh area ids.")]
        public int AreaMask;

        [SerializeField]
        PulseRoutine pulseRoutine = new PulseRoutine();
        #endregion

        #region Events
        [SerializeField]
        ObstructionEvent onObstruction;
        public ObstructionEvent OnObstruction => onObstruction;

        [SerializeField]
        ObstructionEvent onClear;
        public ObstructionEvent OnClear => onClear;

        public override event System.Action OnPulsed;
        #endregion

        #region Public
        // Change the pulse mode at runtime
        public PulseRoutine.Modes PulseMode {
            get => pulseRoutine.Mode.Value;
            set => pulseRoutine.Mode.Value = value;
        }

        // Change the pulse interval at runtime
        public float PulseInterval {
            get => pulseRoutine.Interval.Value;
            set => pulseRoutine.Interval.Value = value;
        }

        // Boolean specifying if the ray is currently obstructed.
        public bool IsObstructed => GetObstructionRayHit().IsObstructing;

        // Will always return RayHit.None since this sensor doesn't detect objects. Use GetObstructionRayHit() instead.
        public RayHit GetDetectionRayHit(GameObject detectedGameObject) => RayHit.None;

        // Returns RayHit data for the current obstruction.
        public RayHit GetObstructionRayHit() {
            if (!isObstructed) {
                return RayHit.None;
            }
            return new RayHit() {
                IsObstructing = true,
                Point = hit.position,
                Normal = hit.normal,
                Distance = hit.distance,
                DistanceFraction = hit.distance / (Test == TestType.Ray ? Ray.Length : Sphere.Radius)
            };
        }

        public override void PulseAll() => Pulse();

        public override void Clear() {
            hit = default;
            if (isObstructed) {
                isObstructed = false;
                OnClear.Invoke(this);
            }
        }
        #endregion

        #region Internals
        // Query the navmesh and update the obstruction RayHit data
        protected override PulseJob GetPulseJob() {
            var prevIsObstructed = isObstructed;
            if (Test == TestType.Ray) {
                isObstructed = TestRay();
            } else if (Test == TestType.Sample) {
                isObstructed = TestSample();
            } else {
                isObstructed = TestClosestEdge();
            }

            if (isObstructed && !prevIsObstructed) {
                OnObstruction.Invoke(this);
            } else if (!isObstructed && prevIsObstructed) {
                OnClear.Invoke(this);
            }

            OnPulsed?.Invoke();

            return default;
        }

        Vector3 direction => Ray.WorldSpace 
            ? Ray.Direction.normalized 
            : transform.rotation * Ray.Direction.normalized;

        NavMeshHit hit;
        bool isObstructed;

        bool TestRay() => NavMesh.Raycast(
            transform.position, 
            transform.position + direction * Ray.Length, 
            out hit, 
            AreaMask);

        bool TestSample() => NavMesh.SamplePosition(
            transform.position,
            out hit,
            Sphere.Radius,
            AreaMask);

        bool TestClosestEdge() => NavMesh.FindClosestEdge(
            transform.position, 
            out hit, 
            AreaMask);

        Coroutine StartEachFrame(IEnumerator routine) => StartCoroutine(PulseEachFrame(routine));
        IEnumerator PulseEachFrame(IEnumerator routine) {
            while (routine.MoveNext()) {
                yield return routine.Current;
            }
        }
        Coroutine StartFixedInterval(IEnumerator routine) => StartCoroutine(PulseFixedInterval(routine));
        IEnumerator PulseFixedInterval(IEnumerator routine) {
            while (routine.MoveNext()) {
                yield return routine.Current;
            }
        }


        void Awake() {
            if (onObstruction == null) {
                onObstruction = new ObstructionEvent();
            }

            if (onClear == null) {
                onClear = new ObstructionEvent();
            }

            if (pulseRoutine == null) {
                pulseRoutine = new PulseRoutine();
            }
            pulseRoutine.Awake(this, StartEachFrame, StartFixedInterval);
        }

        void OnEnable() {
            pulseRoutine.OnEnable();    
        }

        protected override void OnDisable() {
            base.OnDisable();
            pulseRoutine.OnDisable();    
        }

        void OnValidate() {
            pulseRoutine?.OnValidate();    
        }

        void OnDrawGizmosSelected() {
            if (Test == TestType.Ray) {
                DrawRayGizmo();
            } else if (Test == TestType.Sample) {
                DrawSphereGizmo();
            } else if (Test == TestType.ClosestEdge) {
                DrawClosestEdgeGizmo();
            }
        }

        void DrawRayGizmo() {
            if (ShowDetectionGizmos && isObstructed) {
                Gizmos.color = STPrefs.CastingBlockedRayColour;
                Vector3 endPosition = transform.position + direction * hit.distance;
                Gizmos.DrawLine(transform.position, endPosition);
                SensorGizmos.RaycastHitGizmo(hit.position, hit.normal, true);
            } else {
                Gizmos.color = STPrefs.CastingRayColour;
                Vector3 endPosition = transform.position + direction * Ray.Length;
                Gizmos.DrawLine(transform.position, endPosition);
            }
        }

        void DrawSphereGizmo() {
            SensorGizmos.PushColor(STPrefs.CastingRayColour);
            SensorGizmos.SphereGizmo(transform.position, Sphere.Radius);
            SensorGizmos.PopColor();
            if (ShowDetectionGizmos && isObstructed) {
                var obs = GetObstructionRayHit();
                SensorGizmos.PushColor(STPrefs.CastingBlockedRayColour);
                Gizmos.DrawLine(transform.position, obs.Point);
                Gizmos.DrawCube(obs.Point, Vector3.one * 0.1f);
                SensorGizmos.PopColor();
            }
        }

        void DrawClosestEdgeGizmo() {
            if (ShowDetectionGizmos && isObstructed) {
                var obs = GetObstructionRayHit();
                SensorGizmos.PushColor(STPrefs.CastingBlockedRayColour);
                Gizmos.DrawLine(transform.position, obs.Point);
                SensorGizmos.RaycastHitGizmo(hit.position, obs.Normal, true);
                SensorGizmos.PopColor();
            }
        }
        #endregion
    }
}
 