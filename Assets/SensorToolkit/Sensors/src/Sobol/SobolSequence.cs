using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

namespace Micosmo.SensorToolkit {

    public struct SobolSequence1D {
        uint currIndex;
        uint currPoint;
        float currFracPoint;
        public float Next() {
            var zeroBit = SobolSequence.RightmostZeroBitPosition(currIndex);
            if (zeroBit > SobolSequence.L) {
                // Sequence is exhausted, must restart.
                currIndex = 1;
                zeroBit = SobolSequence.RightmostZeroBitPosition(currIndex);
            }
            
            currPoint = currPoint ^ SobolSequence.V[0][zeroBit];
            currFracPoint = currPoint / (float)Math.Pow(2.0, 32);
            
            currIndex++;
            return currFracPoint;
        }
    }

    public struct SobolSequence2D {
        uint currIndex;
        uint currPoint1, currPoint2;
        float currFracPoint1, currFracPoint2;
        public Vector2 Next() {
            var zeroBit = SobolSequence.RightmostZeroBitPosition(currIndex);
            if (zeroBit > SobolSequence.L) {
                // Sequence is exhausted, must restart.
                currIndex = 1;
                zeroBit = SobolSequence.RightmostZeroBitPosition(currIndex);
            }

            currPoint1 = currPoint1 ^ SobolSequence.V[0][zeroBit];
            currFracPoint1 = currPoint1 / (float)Math.Pow(2.0, 32);

            currPoint2 = currPoint2 ^ SobolSequence.V[1][zeroBit];
            currFracPoint2 = currPoint2 / (float)Math.Pow(2.0, 32);

            currIndex++;
            return new Vector2(currFracPoint1, currFracPoint2);
        }
    }

    public struct SobolSequence3D {
        uint currIndex;
        uint currPoint1, currPoint2, currPoint3;
        float currFracPoint1, currFracPoint2, currFracPoint3;
        public Vector3 Next() {
            var zeroBit = SobolSequence.RightmostZeroBitPosition(currIndex);
            if (zeroBit > SobolSequence.L) {
                // Sequence is exhausted, must restart.
                currIndex = 1;
                zeroBit = SobolSequence.RightmostZeroBitPosition(currIndex);
            }

            currPoint1 = currPoint1 ^ SobolSequence.V[0][zeroBit];
            currFracPoint1 = currPoint1 / (float)Math.Pow(2.0, 32);

            currPoint2 = currPoint2 ^ SobolSequence.V[1][zeroBit];
            currFracPoint2 = currPoint2 / (float)Math.Pow(2.0, 32);

            currPoint3 = currPoint3 ^ SobolSequence.V[2][zeroBit];
            currFracPoint3 = currPoint3 / (float)Math.Pow(2.0, 32);

            currIndex++;
            return new Vector3(currFracPoint1, currFracPoint2, currFracPoint3);
        }
    }
    
    public struct NativeSobolSequence : IDisposable {
        NativeArray<uint> currIndex;
        NativeArray<uint> currPoint;
        NativeArray<float> currFracPoint;
        public bool IsCreated => currIndex.IsCreated;
        public NativeSobolSequence(int dim, Allocator allocator) {
            currIndex = new NativeArray<uint>(1, allocator);
            currPoint = new NativeArray<uint>(dim, allocator);
            currFracPoint = new NativeArray<float>(dim, allocator);
        }
        public void Dispose() {
            currIndex.Dispose();
            currPoint.Dispose();
            currFracPoint.Dispose();
        }
        public NativeArray<float> Next() {
            var zeroBit = SobolSequence.RightmostZeroBitPosition(currIndex[0]);
            if (zeroBit > SobolSequence.L) {
                // Sequence is exhausted, must restart.
                currIndex[0] = 1;
                zeroBit = SobolSequence.RightmostZeroBitPosition(currIndex[0]);
            }
            for (int i = 0; i < currPoint.Length; i++) {
                currPoint[i] = currPoint[i] ^ SobolSequence.V[i][zeroBit];
                currFracPoint[i] = currPoint[i] / (float)Math.Pow(2.0, 32);
            }
            currIndex[0]++;
            return currFracPoint;
        }
    }

    public class SobolSequence {
        
        internal const int L = 32;
        internal static uint[][] V = SobolData.GenerateDirectionVectors(SobolData.MaxDimensions, L);
        
        int dimension;
        uint currIndex;
        uint[] currPoint;
        double[] currFracPoint;

        public SobolSequence(int dimension) {
            this.dimension = dimension;
            currIndex = 1;
            currPoint = new uint[this.dimension];
            currFracPoint = new double[this.dimension];
        }

        public double[] Next() {
            var zeroBit = RightmostZeroBitPosition(currIndex);
            if (zeroBit > L) {
                // Sequence is exhausted, must restart.
                currIndex = 1;
                zeroBit = RightmostZeroBitPosition(currIndex);
            }
            for (int i = 0; i < dimension; i++) {
                currPoint[i] = currPoint[i] ^ V[i][zeroBit];
                currFracPoint[i] = (double)currPoint[i] / Math.Pow(2.0, 32);
            }
            currIndex++;
            return currFracPoint;
        }

        internal static uint RightmostZeroBitPosition(uint number) {
            uint i = 1;
            uint value = number;
            while ((value & 1) != 0) {
                value >>= 1;
                i++;
            }
            return i;
        }
        
    }

}