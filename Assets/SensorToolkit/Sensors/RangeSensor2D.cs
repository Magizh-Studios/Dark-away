using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Micosmo.SensorToolkit
{
    /*
     * The Range Sensor detects objects that are inside its detection volume. It uses the family of Overlap functions inside Physics or Physics2D. 
     * A detected object will have one or more Collider that overlaps the detection volume.
     */
    [AddComponentMenu("Sensors/2D Range Sensor")]
    [HelpURL("https://micosmo.com/sensortoolkit2/docs/manual/sensors/range")]
    public class RangeSensor2D : BaseAreaSensor, IPulseRoutine {

        #region Configurations
        public enum Shapes { Circle, Box, Capsule }

        // Determines which shape of Physics2D.Overlap[...] function to use
        public Shapes Shape;

        // Configurations for Circle shape.
        public SphereShape Circle = new SphereShape(1f);
        // Configurations for Box shape.
        public Box2DShape Box = new Box2DShape(Vector2.one * .5f);
        // Configurations for Capsule shape.
        public CapsuleShape Capsule = new CapsuleShape(.5f, 1f);

        [Tooltip("A layer mask specifying which physics layers objects will be detected on.")]
        public LayerMask DetectsOnLayers;

        [Tooltip("The sensor will not detect trigger colliders when this is set to true.")]
        public bool IgnoreTriggerColliders;

        [SerializeField]
        PulseRoutine pulseRoutine = new PulseRoutine();
        #endregion

        #region Events
        public override event Action OnPulsed;
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

        // Change at runtime if the sensor will pulse in Update or FixedUpdate
        public PulseRoutine.UpdateFunctions PulseUpdateFunction {
            get => pulseRoutine.UpdateFunction;
            set => pulseRoutine.UpdateFunction = value;
        }

        // The array size allocated for storing results from Physics.RaycastNonAlloc. Will automatically
        // be doubled in size when more space is needed.
        public int CurrentBufferSize => physics != null ? physics.Buffer.Length : 0;

        public void SetCircleShape(float radius) {
            Shape = Shapes.Circle;
            Circle.Radius = radius;
        }
        public void SetBoxShape(Vector2 halfExtents) {
            Shape = Shapes.Box;
            Box.HalfExtents = halfExtents;
        }
        public void SetCapsuleShape(float radius, float height) {
            Shape = Shapes.Capsule;
            Capsule.Radius = radius;
            Capsule.Height = height;
        }

        public override void PulseAll() => Pulse();
        #endregion

        #region Internals
        static OverlapCircleTest SphereTestInstance = new OverlapCircleTest();
        static OverlapBoxTest BoxTestInstance = new OverlapBoxTest();
        static OverlapCapsuleTest CapsuleTestInstance = new OverlapCapsuleTest();

        ITestNonAlloc<RangeSensor2D, Collider2D> physicsTest {
            get {
                switch (Shape) {
                    case Shapes.Circle:
                        return SphereTestInstance;
                    case Shapes.Box:
                        return BoxTestInstance;
                    case Shapes.Capsule:
                        return CapsuleTestInstance;
                    default:
                        return SphereTestInstance;
                }
            }
        }

        PhysicsNonAlloc<Collider2D> physics;

        // Pulses the sensor to update its list of detected objects
        protected override PulseJob GetPulseJob() {
            if (physics == null) {
                physics = new PhysicsNonAlloc<Collider2D>();
            }

            var saveQHT = Physics2D.queriesHitTriggers;
            Physics2D.queriesHitTriggers = !IgnoreTriggerColliders;
            int numberDetected = physics.PerformTest(this, physicsTest);
            Physics2D.queriesHitTriggers = saveQHT;

            ClearColliders();

            for (var i = 0; i < numberDetected; i++) {
                AddCollider(physics.Buffer[i], false);
            }

            UpdateAllSignals();
            OnPulsed?.Invoke();

            return default;
        }

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


        protected override void Awake() {
            base.Awake();

            if (pulseRoutine == null) {
                pulseRoutine = new PulseRoutine();
            }
            pulseRoutine.Awake(this, StartEachFrame, StartFixedInterval);
        }

        protected void OnEnable() {
            pulseRoutine.OnEnable();
        }

        protected override void OnDisable() {
            base.OnDisable();
            pulseRoutine.OnDisable();
        }

        void OnValidate() {
            pulseRoutine?.OnValidate();
        }

        protected override void OnDrawGizmosSelected() {
            base.OnDrawGizmosSelected();
            switch (Shape) {
                case Shapes.Circle:
                    DrawCircleGizmo();
                    break;
                case Shapes.Box:
                    DrawBoxGizmo();
                    break;
                case Shapes.Capsule:
                    DrawCapsuleGizmo();
                    break;
            }
        }

        class OverlapCircleTest : ITestNonAlloc<RangeSensor2D, Collider2D> {
            public int Test(RangeSensor2D sensor, Collider2D[] results) {
                return Physics2D.OverlapCircleNonAlloc(sensor.transform.position, sensor.Circle.Radius, results, sensor.DetectsOnLayers);
            }
        }
        void DrawCircleGizmo() {
            SensorGizmos.PushColor(STPrefs.RangeColour);
            SensorGizmos.CircleGizmo(transform.position, Circle.Radius);
            SensorGizmos.PopColor();
        }

        class OverlapBoxTest : ITestNonAlloc<RangeSensor2D, Collider2D> {
            public int Test(RangeSensor2D sensor, Collider2D[] results) {
                return Physics2D.OverlapBoxNonAlloc(sensor.transform.position, 2*sensor.Box.HalfExtents, sensor.transform.eulerAngles.z, results, sensor.DetectsOnLayers);
            }
        }
        void DrawBoxGizmo() {
            SensorGizmos.PushColor(STPrefs.RangeColour);
            SensorGizmos.PushMatrix(Matrix4x4.TRS(transform.position, Quaternion.AngleAxis(transform.eulerAngles.z, Vector3.forward), Vector3.one));
            Gizmos.DrawWireCube(Vector3.zero, Box.HalfExtents * 2f);
            SensorGizmos.PopMatrix();
            SensorGizmos.PopColor();
        }

        class OverlapCapsuleTest : ITestNonAlloc<RangeSensor2D, Collider2D> {
            public int Test(RangeSensor2D sensor, Collider2D[] results) {
                var pos = sensor.transform.position;
                var size = new Vector2(sensor.Capsule.Radius*2, sensor.Capsule.Height);
                var dir = CapsuleDirection2D.Vertical;
                var angle = sensor.transform.eulerAngles.z;
                return Physics2D.OverlapCapsuleNonAlloc(pos, size, dir, angle, results, sensor.DetectsOnLayers);
            }
        }
        void DrawCapsuleGizmo() {
            SensorGizmos.PushColor(STPrefs.RangeColour);
            SensorGizmos.PushMatrix(Matrix4x4.TRS(transform.position, Quaternion.AngleAxis(transform.eulerAngles.z, Vector3.forward), Vector3.one));
            SensorGizmos.Capsule2DGizmo(Vector3.zero, Capsule.Radius, Capsule.Height);
            SensorGizmos.PopMatrix();
            SensorGizmos.PopColor();
        }
        #endregion
    }
}
