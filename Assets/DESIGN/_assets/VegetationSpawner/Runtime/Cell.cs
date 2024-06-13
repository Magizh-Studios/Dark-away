// Vegetation Spawner by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace sc.terrain.vegetationspawner
{
    [Serializable]
    public class Cell
    {
        public Bounds bounds;
        public Cell[,] subCells;

        public void Subdivide(int divisions)
        {
            subCells = new Cell[divisions, divisions];

            float cellSize = (bounds.size.x / divisions);

            for (int x = 0; x < divisions; x++)
            {
                for (int z = 0; z < divisions; z++)
                {
                    Vector3 subCellCenterPos = new Vector3(
                        bounds.min.x + (x * (cellSize)) + cellSize * 0.5f,
                        bounds.center.y,
                        bounds.min.z + (z * (cellSize)) + cellSize * 0.5f
                        );
                    Cell subCell = Cell.New(subCellCenterPos, cellSize);

                    subCells[x, z] = subCell;
                }
            }
        }
        public static Cell New(Vector3 wPos, float size)
        {
            Cell cell = new Cell();

            cell.bounds = new Bounds(wPos, Vector3.one * size);

            return cell;
        }

        public bool InsideXZ(Vector3 wPos)
        {
            return (wPos.x >= bounds.min.x && wPos.x <= bounds.max.x && wPos.z >= bounds.min.z && wPos.z <= bounds.max.z);
        }

        public static Vector2Int PositionToCellIndex(Terrain terrain, Vector2 normalizedPos, int cellSize)
        {
            int x = Mathf.FloorToInt((terrain.terrainData.size.x / cellSize) * normalizedPos.x);
            int y = Mathf.FloorToInt((terrain.terrainData.size.z / cellSize) * normalizedPos.y);

            return new Vector2Int(x, y);
        }

        public Cell GetSubcell(Vector3 worldPos, float cellSize, int subDivisions)
        {
            if (subCells == null) return null;

            Vector2 localCellPos = new Vector2(
                (worldPos.x - bounds.min.x) / cellSize,
                (worldPos.z - bounds.min.z) / cellSize);

            Vector2Int subCellIndex = new Vector2Int(
                Mathf.FloorToInt(subDivisions * localCellPos.x),
                Mathf.FloorToInt(subDivisions * localCellPos.y));

            return subCells[subCellIndex.x, subCellIndex.y];
        }
    }
}