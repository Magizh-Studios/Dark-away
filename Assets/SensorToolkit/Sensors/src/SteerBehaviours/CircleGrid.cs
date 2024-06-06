using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

namespace Micosmo.SensorToolkit {

    public static class CircleGrid {
        
        public static void InitialiseDirections(DirectionalGrid grid) {
            var gridSize = grid.GridSize;
            var cellSize = 2 * Mathf.PI / grid.CellCount;
            for (var i = 0; i < gridSize; i++) {
                var a = (i + .5f) * cellSize;
                grid.Directions[i] = Quaternion.FromToRotation(Vector3.up, grid.Axis) * new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a));
            }
        }

        public static int GetCell(DirectionalGrid grid, Vector3 dir) {
            var rot = Quaternion.FromToRotation(Vector3.up, grid.Axis);
            var localForwards = rot * Vector3.forward;

            var localDir = Vector3.ProjectOnPlane(dir, grid.Axis);
            var a = Vector3.SignedAngle(localForwards, localDir, grid.Axis);
            if (a < 0) { a += 360; }
            var cellSize = 2 * Mathf.PI / grid.CellCount;
            return Mathf.FloorToInt(Mathf.Deg2Rad * a / cellSize);
        }

        public static float SampleDirection(DirectionalGrid grid, Vector3 dir) {
            var rot = Quaternion.FromToRotation(Vector3.up, grid.Axis);
            var localForwards = rot * Vector3.forward;

            var localDir = Vector3.ProjectOnPlane(dir, grid.Axis);
            var a = Vector3.SignedAngle(localForwards, localDir, grid.Axis);
            var cellSize = 2 * Mathf.PI / grid.CellCount;
            var cellFrac = Mathf.Deg2Rad * a / cellSize;
            var left = Mathf.FloorToInt(cellFrac - 0.5f) + 0.5f;
            var right = Mathf.FloorToInt(cellFrac + 0.5f) + 0.5f;
            var t = (cellFrac - left) / (right - left);

            if (left < 0) {
                left += grid.CellCount;
            }
            var leftVal = grid.Values[Mathf.FloorToInt(left)];
            if (right < 0) {
                right += grid.CellCount;
            }
            var rightVal = grid.Values[Mathf.FloorToInt(right)];
            return Mathf.Lerp(leftVal, rightVal, t);
        }

        public static Vector3 GetMaxContinuous(DirectionalGrid grid) {
            if (grid.CellCount == 0) {
                return Vector3.zero;
            }
            var max = grid.GetMaxCell();
            var value = grid.Values[max];
            var coords = new Coords(grid.CellCount, max);
            var offset = SubCellOffset(grid.GridSize, value, grid.Values[coords.ShiftLeft().I], grid.Values[coords.ShiftRight().I], out value);
            var dir = grid.Directions[max];
            var right = Vector3.Cross(grid.Axis, dir);
            var offdir = (dir + (right * offset)).normalized;
            return offdir * value;
        }

        static float SubCellOffset(int gridSize, float v, float prev, float next, out float subValue) {
            float xmin, xmax, xdir;
            if (prev < next) {
                xmin = prev; xmax = next; xdir = 1;
            } else {
                xmin = next; xmax = prev; xdir = -1;
            }
            var xinterp = Mathf.InverseLerp(0, v - xmin, xmax - xmin);
            subValue = Mathf.LerpUnclamped(xmin, v, 1f + (xinterp / 2f));
            var cellExtents = Mathf.PI / gridSize;
            var xoffset = xdir * xinterp * cellExtents;
            return xoffset;
        }

        public struct Coords {
            public int Size;
            public int I;
            public Coords(int size, int i) {
                Size = size; I = i;
            }
            public Coords ShiftLeft() {
                var i = I - 1;
                if (i < 0) {
                    i = Size - 1;
                }
                return new Coords(Size, i);
            }

            public Coords ShiftRight() {
                var i = I + 1;
                if (i >= Size) {
                    i = 0;
                }
                return new Coords(Size, i);
            }
        }

    }

}