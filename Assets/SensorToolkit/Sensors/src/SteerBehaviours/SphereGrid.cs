using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

namespace Micosmo.SensorToolkit {

    public static class SphereGrid {
        
        public static void InitialiseDirections(DirectionalGrid grid) {
            var cellCount = grid.CellCount;
            for (var i = 0; i < cellCount; i++) {
                grid.Directions[i] = new Coords(grid.GridSize, i).Direction;
            }
        }

        public static int GetCell(DirectionalGrid grid, Vector3 direction) => new Coords(grid.GridSize, direction).I;

        public static float SampleDirection(DirectionalGrid grid, Vector3 direction) {
            // Find the 3 closest cells
            float d1 = 0, d2 = 0, d3 = 0;
            int i1 = 0, i2 = 0, i3 = 0;
            for (var i = 0; i < grid.CellCount; i++) {
                var d = Vector3.Dot(direction, grid.Directions[i]);
                if (d > d1) {
                    d3 = d2; i3 = i2;
                    d2 = d1; i2 = i1;
                    d1 = d; i1 = i;
                } else if (d > d2) {
                    d3 = d2; i3 = i2;
                    d2 = d; i2 = i;
                } else if (d > d3) {
                    d3 = d; i3 = i;
                }
            }

            // Interpolate between the 3 closest cells
            var v1 = grid.Values[i1];
            var v2 = grid.Values[i2];
            var v3 = grid.Values[i3];
            var dir1 = grid.Directions[i1];
            var dir2 = grid.Directions[i2];
            var dir3 = grid.Directions[i3];
            var B1 = dir2 - dir1;
            var B2 = dir3 - dir1;
            var pp = direction - dir1;
            var x = Vector3.Dot(pp, B1)/B1.magnitude;
            var y = Vector3.Dot(pp, B2)/B2.magnitude;
            return (x * v2) + (y * v3) + ((1 - x - y) * v1);
        }

        public static Vector3 GetMaxContinuous(DirectionalGrid grid) {
            if (grid.GridSize == 0) {
                return Vector3.zero;
            }
            var max = grid.GetMaxCell();
            var value = grid.Values[max];
            var coords = new Coords(grid.GridSize, max);
            var xoffset = SubCellOffset(grid.GridSize, value, grid.Values[coords.ShiftLeft().I], grid.Values[coords.ShiftRight().I], out value);
            var yoffset = SubCellOffset(grid.GridSize, value, grid.Values[coords.ShiftDown().I], grid.Values[coords.ShiftUp().I], out value);
            
            var face = coords.Face;
            var cellSize = 2f / grid.GridSize;
            var direction = (face.GetCellCenter(coords.X, coords.Y, cellSize) + (xoffset * face.B1 + yoffset * face.B2)).normalized;
            return direction * value;
        }

        static float SubCellOffset(float gridSize, float v, float prev, float next, out float subValue) {
            float xmin, xmax, xdir;
            if (prev < next) {
                xmin = prev; xmax = next; xdir = 1;
            } else {
                xmin = next; xmax = prev; xdir = -1;
            }
            var xinterp = Mathf.InverseLerp(0, v - xmin, xmax - xmin);
            subValue = Mathf.LerpUnclamped(xmin, v, 1f + (xinterp / 2f));
            var cellExtents = 1f / gridSize;
            var xoffset = xdir * xinterp * cellExtents;
            return xoffset;
        }

        readonly static GridFace[] GridFaces = new GridFace[] {
            new GridFace(Vector3.up, Vector3.right, Vector3.forward),
            new GridFace(Vector3.down, Vector3.right, Vector3.forward),
            new GridFace(Vector3.forward, Vector3.right, Vector3.down),
            new GridFace(Vector3.back, Vector3.right, Vector3.up),
            new GridFace(Vector3.right, Vector3.forward, Vector3.up),
            new GridFace(Vector3.left, Vector3.forward, Vector3.up)
        };

        static int GetFaceIndex(Vector3 dir) {
            var adir = AbsVector(dir);
            if (adir.y >= adir.x && adir.y >= adir.z) {
                return dir.y > 0 ? 0 : 1;
            }
            if (adir.z >= adir.x && adir.z >= adir.y) {
                return dir.z > 0 ? 2 : 3;
            }
            return dir.x > 0 ? 4 : 5;
        }

        static Vector3 AbsVector(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

        public struct Coords {
            public int Size;
            public int FaceIndex;
            public int X;
            public int Y;
            public int I => (X * Size + Y) + (FaceIndex * Size * Size);
            public GridFace Face => GridFaces[FaceIndex];
            public Vector3 Direction => Face.GetCellCenter(X, Y, 2f / Size).normalized;
            public Coords(int size, int fi, int x, int y) {
                Size = size; FaceIndex = fi; X = x; Y = y;
            }
            public Coords(int size, int i) {
                Size = size;
                var gi = i % (Size * Size);
                FaceIndex = i / (Size * Size);
                X = gi / Size;
                Y = gi % Size;
            }
            public Coords(int size, Vector3 dir) {
                Size = size;
                FaceIndex = GetFaceIndex(dir);
                var face = GridFaces[FaceIndex];
                var absN = AbsVector(face.N);
                var invN = new Vector3(absN.x == 1 ? 0 : 1, absN.y == 1 ? 0 : 1, absN.z == 1 ? 0 : 1);
                var pDir = Vector3.Scale(dir, invN);
                var interval = 2f / Size;
                X = Mathf.FloorToInt((Vector3.Dot(pDir, face.B1) + 1f) / interval);
                Y = Mathf.FloorToInt((Vector3.Dot(pDir, face.B2) + 1f) / interval);
            }

            int gmod => Size - 1;

            public Coords ShiftRight() {
                var x = X + 1;
                var y = Y;
                var fi = FaceIndex;
                if (x > gmod) {
                    if (fi == 0) { fi = 4; x = y; y = gmod; } else if (fi == 1) { fi = 4; x = y; y = 0; } else if (fi == 2) { fi = 4; x = gmod; y = gmod - y; } else if (fi == 3) { fi = 4; x = 0; } else if (fi == 4) { fi = 2; x = gmod; y = gmod - y; } else if (fi == 5) { fi = 2; x = 0; y = gmod - y; }
                }
                return new Coords() { Size = Size, FaceIndex = fi, X = x, Y = y };
            }

            public Coords ShiftLeft() {
                var x = X - 1;
                var y = Y;
                var fi = FaceIndex;
                if (x < 0) {
                    if (fi == 0) { fi = 5; x = y; y = gmod; } else if (fi == 1) { fi = 5; x = y; y = 0; } else if (fi == 2) { fi = 5; x = gmod; y = gmod - y; } else if (fi == 3) { fi = 5; x = 0; } else if (fi == 4) { fi = 3; x = gmod; } else if (fi == 5) { fi = 3; x = 0; }
                }
                return new Coords() { Size = Size, FaceIndex = fi, X = x, Y = y };
            }

            public Coords ShiftUp() {
                var x = X;
                var y = Y + 1;
                var fi = FaceIndex;
                if (y > gmod) {
                    if (fi == 0) { fi = 2; y = 0; } else if (fi == 1) { fi = 2; y = gmod; } else if (fi == 2) { fi = 1; y = gmod; } else if (fi == 3) { fi = 0; y = 0; } else if (fi == 4) { fi = 0; y = x; x = gmod; } else if (fi == 5) { fi = 0; y = x; x = 0; }
                }
                return new Coords() { Size = Size, FaceIndex = fi, X = x, Y = y };
            }

            public Coords ShiftDown() {
                var x = X;
                var y = Y - 1;
                var fi = FaceIndex;
                if (y < 0) {
                    if (fi == 0) { fi = 3; y = gmod; } else if (fi == 1) { fi = 3; y = 0; } else if (fi == 2) { fi = 0; y = gmod; } else if (fi == 3) { fi = 1; y = 0; } else if (fi == 4) { fi = 1; y = x; x = gmod; } else if (fi == 5) { fi = 1; y = x; x = 0; }
                }
                return new Coords() { Size = Size, FaceIndex = fi, X = x, Y = y };
            }
        }

        public struct GridFace {
            public Vector3 N;
            public Vector3 B1;
            public Vector3 B2;
            public Vector3 Origin => N - B1 - B2;
            public GridFace(Vector3 n, Vector3 b1, Vector3 b2) {
                N = n; B1 = b1; B2 = b2;
            }
            public Vector3 GetCellCenter(float x, float y, float interval) =>
                Origin + ((x + .5f) * B1 * interval) + ((y + .5f) * B2 * interval);
        }
    }

}