using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

namespace Micosmo.SensorToolkit {

    public struct DirectionalGrid : IDisposable {
        public bool IsSpherical { get; }
        public NativeArray<float> Values;
        public NativeArray<Vector3> Directions;
        public int GridSize { get; }
        public Vector3 Axis { get; }
        public int CellCount => IsSpherical ? 6 * GridSize * GridSize : GridSize;
        public bool IsCreated { get; private set; }

        public DirectionalGrid(bool isSpherical, int gridSize, Vector3 axis, Allocator allocator) {
            IsSpherical = isSpherical;
            GridSize = gridSize;
            var cellCount = IsSpherical ? 6 * GridSize * GridSize : GridSize;
            Axis = axis;
            Values = new NativeArray<float>(cellCount, allocator);
            Directions = new NativeArray<Vector3>(cellCount, allocator);
            IsCreated = true;
            if (IsSpherical) {
                SphereGrid.InitialiseDirections(this);
            } else {
                CircleGrid.InitialiseDirections(this);
            }
        }

        public static DirectionalGrid CreateMatching(DirectionalGrid other, Allocator allocator) => new DirectionalGrid(other.IsSpherical, other.GridSize, other.Axis, allocator);
        public static DirectionalGrid CreateSphere(int gridSize, Allocator allocator) => new DirectionalGrid(true, gridSize, Vector3.up, allocator);
        public static DirectionalGrid CreateCircle(int gridSize, Vector3 axis, Allocator allocator) => new DirectionalGrid(false, gridSize, axis, allocator);

        public int GetCell(Vector3 direction) => IsSpherical ? SphereGrid.GetCell(this, direction) : CircleGrid.GetCell(this, direction);

        public float SampleDirection(Vector3 direction) => IsSpherical ? SphereGrid.SampleDirection(this, direction) : CircleGrid.SampleDirection(this, direction);

        public Vector3 GetMaxContinuous() => IsSpherical ? SphereGrid.GetMaxContinuous(this) : CircleGrid.GetMaxContinuous(this);

        public int GetMaxCell() {
            var res = 0;
            for (int i = 1; i < CellCount; i++) {
                if (Values[i] > Values[res]) {
                    res = i;
                }
            }
            return res;
        }

        public int GetMinCell() {
            var res = 0;
            for (int i = 1; i < CellCount; i++) {
                if (Values[i] < Values[res]) {
                    res = i;
                }
            }
            return res;
        }

        public void Fill(float v) {
            for (int i = 0; i < Values.Length; i++) {
                Values[i] = v;
            }
        }

        public void GradientFill(Vector3 value, float falloff) {
            if (value == Vector3.zero) {
                return;
            }
            var direction = value.normalized;
            var v = value.magnitude;
            for (int i = 0; i < CellCount; i++) {
                var dot = Vector3.Dot(direction, Directions[i]);
                var interp = Mathf.Lerp(0, 1, (dot - falloff) / (1f - falloff));
                Values[i] = Mathf.Max(Values[i], v * interp);
            }
        }

        public void GradientAdd(Vector3 value, float falloff) {
            if (value == Vector3.zero) {
                return;
            }
            var direction = value.normalized;
            var v = value.magnitude;
            for (int i = 0; i < CellCount; i++) {
                var dot = Vector3.Dot(direction, Directions[i]);
                var interp = Mathf.Lerp(0, 1, (dot - falloff) / (1f - falloff));
                Values[i] += v * interp;
            }
        }

        public void GradientFunction(Vector3 value, float falloff, System.Func<float, float, float> fn) {
            if (value == Vector3.zero) {
                return;
            }
            var direction = value.normalized;
            var v = value.magnitude;
            for (int i = 0; i < CellCount; i++) {
                var dot = Vector3.Dot(direction, Directions[i]);
                var interp = Mathf.Lerp(0, 1, (dot - falloff) / (1f - falloff));
                Values[i] = fn(Values[i], v * interp);
            }
        }

        /*List<float> convoluteValues = new List<float>();
        public void ConvoluteMin(float falloff, float max) {
            convoluteValues.Clear();
            for (int i = 0; i < CellCount; i++) {
                var cell = GetCell(i);
                var v = cell.Value;
                for (int j = 0; j < CellCount; j++) {
                    var other = GetCell(j);
                    var dot = Vector3.Dot(cell.Direction, other.Direction);
                    var interp = Mathf.Lerp(max, other.Value, (dot - falloff) / (1f - falloff));
                    v = Mathf.Min(v, interp);
                }
                convoluteValues.Add(v);
            }
            for (int i = 0; i < CellCount; i++) {
                Values[i] = convoluteValues[i];
            }
        }*/

        public void Copy(DirectionalGrid other) {
            if (!CheckIsCompatible(this, other)) { return; }
            NativeArray<float>.Copy(other.Values, Values);
            NativeArray<Vector3>.Copy(other.Directions, Directions);
        }

        public void MergeVelocity(DirectionalGrid slowGrid, DirectionalGrid fastGrid, float preferredSpeed, float maxSpeed, float power) {
            if (!CheckIsCompatible(this, slowGrid) || !CheckIsCompatible(this, fastGrid)) { return; }
            for (int i = 0; i < Values.Length; i++) {
                var vSlow = slowGrid.Values[i];
                var vFast = fastGrid.Values[i];
                var xSlow = 1f - Mathf.Clamp01(Mathf.Abs(vSlow - preferredSpeed) / preferredSpeed);
                if (float.IsNaN(xSlow)) {
                    // This will happen when preferredSpeed is 0
                    xSlow = 0f;
                }
                var xFast = (preferredSpeed != maxSpeed) ?
                    1f - Mathf.Clamp01(Mathf.Abs(vFast - preferredSpeed) / (maxSpeed - preferredSpeed)) 
                    : xSlow;
                var x = Mathf.Max(xSlow, xFast);
                if (power != 1f) {
                    x = Mathf.Pow(x, power);
                }
                Values[i] *= x;
            }
        }

        public void MergeDanger(DirectionalGrid danger, float maxDanger) {
            if (!CheckIsCompatible(this, danger)) { return; }
            var safeMax = Mathf.Max(maxDanger, 0.001f);
            for (int i = 0; i < Values.Length; i++) {
                var x = danger.Values[i] / safeMax;
                Values[i] *= (1f - x);
            }
        }

        public void InterpolateTo(DirectionalGrid target, float t) {
            if (!CheckIsCompatible(this, target)) { return; }
            for (int i = 0; i < Values.Length; i++) {
                Values[i] = Mathf.Lerp(Values[i], target.Values[i], 1f - Mathf.Pow(0.5f, t));
            }
        }

        public void MultiplyScalar(float x) {
            for (int i = 0; i < this.Values.Length; i++) {
                Values[i] = Values[i] * x;
            }
        }

        public void ClampRange01() {
            var i = GetMaxCell();
            var v = Values[i];
            if (v < 1) { return; }
            MultiplyScalar(1f / v);
        }

        public bool IsCompatible(DirectionalGrid other) {
            return IsCreated && IsSpherical == other.IsSpherical && GridSize == other.GridSize && Axis == other.Axis;
        }
        
        public static bool CheckIsCompatible(DirectionalGrid g1, DirectionalGrid g2) {
            if (!g1.IsCompatible(g2)) {
                Debug.LogError("Incompatible directional grids");
                return false;
            }
            return true;
        }

        public void Dispose() {
            if (Values.IsCreated) {
                Values.Dispose();
            }
            if (Directions.IsCreated) {
                Directions.Dispose();
            }
            IsCreated = false;
        }
        
        public void DrawGizmos(Vector3 p, float rayOffset, float rayScale, float rayWidth) {
            if (!IsCreated) {
                return;
            }
            for (int i = 0; i < CellCount; i++) {
                var dir = Directions[i];
                var start = p + dir * rayOffset;
                var value = Values[i] > 0 ? Values[i] : 0;
                SensorGizmos.ThickLineNoZTest(start, start + (dir * value * rayScale), rayWidth);
            }
        }

        public static void DrawVelocityGizmos(Vector3 p, float rayOffset, float rayScale, float rayWidth, float maxSpeed, DirectionalGrid slowGrid, DirectionalGrid fastGrid) {
            if (!slowGrid.IsCreated || !fastGrid.IsCreated) {
                return;
            }
            for (int i = 0; i < slowGrid.CellCount; i++) {
                var minVal = slowGrid.Values[i];
                var maxVal = fastGrid.Values[i];
                var dir = slowGrid.Directions[i];
                var minStart = p + dir * rayOffset;
                var minEnd = minStart + (dir * minVal * rayScale);
                SensorGizmos.PushColor(Color.blue);
                SensorGizmos.ThickLineNoZTest(minStart, minEnd, rayWidth);
                SensorGizmos.PopColor();

                var maxStart = minStart + (dir * maxVal * rayScale);
                var maxEnd = minStart + (dir * maxSpeed * rayScale);
                SensorGizmos.PushColor(new Color(0.8f, 1f, 1f));
                SensorGizmos.ThickLineNoZTest(maxStart, maxEnd, rayWidth);
                SensorGizmos.PopColor();

                SensorGizmos.PushColor(new Color(69f / 255, 6f / 255, 46f / 255));
                SensorGizmos.ThickLineNoZTest(minEnd, maxStart, rayWidth);
                SensorGizmos.PopColor();
            }
        }
    }

}