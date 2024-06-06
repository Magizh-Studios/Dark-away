using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

namespace Micosmo.SensorToolkit.Example {

    public class DirectionalGridTester : MonoBehaviour {

        public enum SamplingMode { NearestCell, Sample, MaxContinuous };

        public bool IsSpherical;
        public int Resolution;
        public List<Transform> SeekTargets;

        [Space]
        public bool AvoidDanger;
        public List<Transform> DangerTargets;
        public float DangerThreshold = 0.5f;

        [Space]
        public SamplingMode Mode;
        public Transform SampleDirection;

        DirectionalGrid grid;
        DirectionalGrid dangerGrid;

        void OnDrawGizmos() {
            if (IsSpherical) {
                grid = DirectionalGrid.CreateSphere(Resolution, Allocator.Persistent);
                dangerGrid = DirectionalGrid.CreateSphere(Resolution, Allocator.Persistent);
            } else {
                grid = DirectionalGrid.CreateCircle(Resolution * 4, Vector3.up, Allocator.Persistent);
                dangerGrid = DirectionalGrid.CreateCircle(Resolution * 4, Vector3.up, Allocator.Persistent);
            }
            if (SeekTargets != null) {
                foreach (var target in SeekTargets) {
                    var delta = target.position - transform.position;
                    grid.GradientFill(delta, -1f);
                }
            }
            if (AvoidDanger && DangerTargets != null) {
                foreach (var target in DangerTargets) {
                    var delta = target.position - transform.position;
                    dangerGrid.GradientFill(delta, 0f);
                }
                grid.MergeDanger(dangerGrid, DangerThreshold);
            }

            Gizmos.color = Color.yellow;
            grid.DrawGizmos(transform.position, 0f, 1f, 5f);

            Gizmos.color = Color.red;
            if (Mode != SamplingMode.MaxContinuous) {
                if (SampleDirection != null) {
                    var direction = (SampleDirection.position - transform.position).normalized;
                    var value = Mode == SamplingMode.Sample ? grid.SampleDirection(direction) : grid.Values[grid.GetCell(direction)];
                    Gizmos.DrawRay(transform.position, direction * value);
                }
            } else {
                var max = grid.GetMaxContinuous();
                Gizmos.DrawRay(transform.position, max);
            }
            
        }

    }

}
