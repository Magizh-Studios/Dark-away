using System.Collections.Generic;
using UnityEngine;

namespace sc.terrain.vegetationspawner
{
    public partial class VegetationSpawner
    {
        public void RefreshGrassPrototypes()
        {
            foreach (Terrain terrain in terrains)
            {
                List<DetailPrototype> grassPrototypeCollection = new List<DetailPrototype>();
                int index = 0;
                foreach (GrassPrefab item in grassPrefabs)
                {
                    item.index = index;

                    DetailPrototype detailPrototype = new DetailPrototype();

                    UpdateGrassItem(item, detailPrototype);

                    grassPrototypeCollection.Add(detailPrototype);

                    index++;
                }
                if (grassPrototypeCollection.Count > 0) terrain.terrainData.detailPrototypes = grassPrototypeCollection.ToArray();
            }
        }

        public void AddGrassItemsFromTerrain(Terrain terrain)
        {
            GrassPrefab[] grassItems = new GrassPrefab[terrain.terrainData.detailPrototypes.Length];
            
            for (int i = 0; i < terrain.terrainData.detailPrototypes.Length; i++)
            {
                grassItems[i] = new GrassPrefab();
                
                grassItems[i].type = terrain.terrainData.detailPrototypes[i].usePrototypeMesh ? GrassType.Mesh : GrassType.Texture;
                grassItems[i].renderAsBillboard = terrain.terrainData.detailPrototypes[i].renderMode == DetailRenderMode.GrassBillboard;
                grassItems[i].billboard = terrain.terrainData.detailPrototypes[i].prototypeTexture;
                grassItems[i].minMaxHeight = new Vector2(terrain.terrainData.detailPrototypes[i].minHeight, terrain.terrainData.detailPrototypes[i].maxHeight);
                grassItems[i].minMaxWidth = new Vector2(terrain.terrainData.detailPrototypes[i].minWidth, terrain.terrainData.detailPrototypes[i].maxWidth);
                grassItems[i].noiseSize = terrain.terrainData.detailPrototypes[i].noiseSpread;
                grassItems[i].mainColor = terrain.terrainData.detailPrototypes[i].healthyColor;
                grassItems[i].secondaryColor = terrain.terrainData.detailPrototypes[i].dryColor;
                grassItems[i].prefab = terrain.terrainData.detailPrototypes[i].prototype;

                if(grassItems[i].billboard) grassItems[i].name = grassItems[i].billboard.name;
                if(grassItems[i].prefab) grassItems[i].name = grassItems[i].prefab.name;
                
                #if UNITY_2021_2_OR_NEWER
                grassItems[i].seed = terrain.terrainData.detailPrototypes[i].noiseSeed;
                grassItems[i].gpuInstancing = terrain.terrainData.detailPrototypes[i].useInstancing;
                grassItems[i].holeEdgePadding = terrain.terrainData.detailPrototypes[i].holeEdgePadding;
                #endif
                
                #if UNITY_2022_2_OR_NEWER
                grassItems[i].useDensityScaling = terrain.terrainData.detailPrototypes[i].useDensityScaling;
                grassItems[i].alignToGround = terrain.terrainData.detailPrototypes[i].alignToGround * 100f;
                grassItems[i].positionJitter = terrain.terrainData.detailPrototypes[i].positionJitter * 100f;
                grassItems[i].density = terrain.terrainData.detailPrototypes[i].density;
                #endif
            }

            this.grassPrefabs = new List<GrassPrefab>(grassItems);
        }

        public void AddNewGrassItem()
        {
            GrassPrefab newGrass = new GrassPrefab();
            grassPrefabs.Add(newGrass);

            newGrass.seed = Random.Range(0, 9999);
            newGrass.index = grassPrefabs.Count;

            RefreshGrassPrototypes();
            
            //Potentially mirror the operation to GPU Instancer, but functionality is too tightly woven into GPUI's editor code
        }
        
        public void RemoveGrassItem(int index)
        {
            grassPrefabs.RemoveAt(index);
            RefreshGrassPrototypes();
            
            //Potentially mirror the operation to GPU Instancer, but functionality is too tightly woven into GPUI's editor code
        }

        public void SpawnAllGrass(Terrain targetTerrain = null)
        {
            RefreshGrassPrototypes();

            InitializeSeed();
            
            foreach (GrassPrefab item in grassPrefabs)
            {
                SpawnGrass(item, targetTerrain);
            }
        }

        public void UpdateProperties(GrassPrefab item)
        {
            foreach (Terrain terrain in terrains)
            {
                //Note only works when creating these copies :/
                DetailPrototype[] detailPrototypes = terrain.terrainData.detailPrototypes;
                DetailPrototype detailPrototype = GetGrassPrototype(item, terrain);

                //Could have been removed
                if(detailPrototype == null) continue;
                
                UpdateGrassItem(item, detailPrototype);

                detailPrototypes[item.index] = detailPrototype;
                terrain.terrainData.detailPrototypes = detailPrototypes;

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(terrain);
                UnityEditor.EditorUtility.SetDirty(terrain.terrainData);
#endif
            }

        }

        public void SetDetailResolution()
        {
            foreach (Terrain terrain in terrains)
            {
                terrain.terrainData.SetDetailResolution(detailResolution, grassPatchSize);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(terrain.terrainData);
#endif
            }
        }

        public void SpawnGrass(GrassPrefab item, Terrain targetTerrain = null)
        {
            if (item.collisionCheck) RebuildCollisionCacheIfNeeded();

            item.instanceCount = 0;

            if (targetTerrain == null)
            {
                List<Terrain> targetTerrains = GetTargetTerrains();
                    
                showProgressBar = false;
                var progress = true;
                int counter = 0;
                
                foreach (Terrain terrain in targetTerrains)
                {
                    counter++;
                    
                    #if UNITY_EDITOR
                    if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("Vegetation Spawner", $"Spawning \"{item.name}\" on {terrain.name} ({counter}/{targetTerrains.Count})", (float)counter / (float)targetTerrains.Count))
                    {
                        progress = false;
                    }
                    #endif
                    
                    if(progress) SpawnGrassOnTerrain(terrain, item);
                }
                
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.ClearProgressBar();
                #endif
            }
            else
            {
                showProgressBar = true;
                SpawnGrassOnTerrain(targetTerrain, item);
            }
            
            onGrassRespawn?.Invoke(item);
        }

        private void SpawnGrassOnTerrain(Terrain terrain, GrassPrefab item)
        {
            //int[,] map = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, item.index);
            int[,] map = new int[terrain.terrainData.detailWidth, terrain.terrainData.detailHeight];

            int counter = 0;
            int cellCount = terrain.terrainData.detailWidth * terrain.terrainData.detailHeight;
            
            if (item.enabled)
            {
                for (int x = 0; x < terrain.terrainData.detailWidth; x++)
                {
                    for (int y = 0; y < terrain.terrainData.detailHeight; y++)
                    {
                        counter++;
                        
#if UNITY_EDITOR
                        if (showProgressBar)
                        {
                            //Show progress bar every 10%
                            if (counter % (cellCount / 10) == 0)
                            {
                                UnityEditor.EditorUtility.DisplayProgressBar("Vegetation Spawner", $"Spawning \"{item.name}\" on {terrain.name} ({counter}/{cellCount})", (float)counter / (float)cellCount);
                            }
                        }
#endif
                        InitializeSeed(x * y + item.seed);

                        //Default
                        int instanceCount = 1;

                        //XZ world position
                        Vector3 wPos = terrain.DetailToWorld(y, x);

                        Vector2 normalizedPos = terrain.GetNormalizedPosition(wPos);

                        //Skip if failing probability check
                        if (((Random.value * 100f) <= item.probability) == false)
                        {
                            instanceCount = 0;
                            continue;
                        }

                        if (item.collisionCheck)
                        {
                            //Check for collision
                            if (InsideOccupiedCell(terrain, wPos, normalizedPos))
                            {
                                instanceCount = 0;
                                continue;
                            }
                        }

                        terrain.SampleHeight(normalizedPos, out _, out wPos.y, out _);

                        if (item.rejectUnderwater && wPos.y < waterHeight)
                        {
                            instanceCount = 0;
                            continue;
                        }
                        //Check height
                        if (wPos.y < item.heightRange.x || wPos.y > item.heightRange.y)
                        {
                            instanceCount = 0;
                            continue;
                        }

                        if (item.slopeRange.x > 0 || item.slopeRange.y < 90)
                        {
                            float slope = terrain.GetSlope(normalizedPos);
                            //Reject if slope check fails
                            if (slope < item.slopeRange.x || slope > item.slopeRange.y)
                            {
                                instanceCount = 0;
                                continue;
                            }
                        }

                        if (item.curvatureRange.x > 0 || item.curvatureRange.y < 1f)
                        {
                            float curvature = terrain.SampleConvexity(normalizedPos);
                            //0=concave, 0.5=flat, 1=convex
                            curvature = TerrainSampler.ConvexityToCurvature(curvature);
                            if (curvature < item.curvatureRange.x || curvature > item.curvatureRange.y)
                            {
                                instanceCount = 0;
                                continue;
                            }
                        }

                        //Reject based on layer masks
                        float spawnChance = 0f;
                        if (item.layerMasks.Count == 0)
                        {
                            spawnChance = 100f;
                        }
                        else
                        {
                            splatmapTexelIndex = terrain.SplatmapTexelIndex(normalizedPos);
                        }

                        foreach (TerrainLayerMask layer in item.layerMasks)
                        {
                            Texture2D splat = terrain.terrainData.GetAlphamapTexture(GetSplatmapID(layer.layerID));

                            m_splatmapColor = splat.GetPixel(splatmapTexelIndex.x, splatmapTexelIndex.y);

                            int channel = layer.layerID % 4;
                            float value = m_splatmapColor[channel];

                            if (value > 0)
                            {
                                value = Mathf.Clamp01(value - layer.threshold);
                            }
                            value *= 100f;

                            spawnChance += value;
                        }
                        InitializeSeed(x * y + item.seed);
                        if ((Random.value <= spawnChance) == false)
                        {
                            instanceCount = 0;
                        }

                        //if (instanceCount == 1) DebugPoints.Instance.Add(wPos, true, 0f);
                        item.instanceCount += instanceCount;
                        
                        #if UNITY_2022_2_OR_NEWER
                        if(terrain.terrainData.detailScatterMode == DetailScatterMode.CoverageMode) instanceCount *= 255;
                        #endif
                        
                        //Passed all conditions, spawn one instance here
                        map[x, y] = instanceCount;
                    }
                }
            }
            
            terrain.terrainData.SetDetailLayer(0, 0, item.index, map);

#if UNITY_EDITOR
            if(showProgressBar) UnityEditor.EditorUtility.ClearProgressBar();
#endif
        }

        private DetailPrototype GetGrassPrototype(GrassPrefab item, Terrain terrain)
        {
            if (item.index >= terrain.terrainData.detailPrototypes.Length) return null;
            
            return terrain.terrainData.detailPrototypes[item.index];
        }

        private void UpdateGrassItem(GrassPrefab item, DetailPrototype d)
        {
            d.healthyColor = item.mainColor;
            d.dryColor = item.linkColors ? item.mainColor : item.secondaryColor;

            d.minHeight = item.minMaxHeight.x;
            d.maxHeight = item.minMaxHeight.y;

            d.minWidth = item.minMaxWidth.x;
            d.maxWidth = item.minMaxWidth.y;

            #if UNITY_2021_2_OR_NEWER
            d.noiseSeed = item.seed;
            d.holeEdgePadding = item.holeEdgePadding;
            #endif
            d.noiseSpread = item.noiseSize;
            
            #if UNITY_2022_2_OR_NEWER
            d.alignToGround = item.alignToGround * 0.01f;
            d.positionJitter = item.positionJitter * 0.01f;
            d.useDensityScaling = item.useDensityScaling;
            d.density = item.density;
            #endif
            
            if (item.type == GrassType.Mesh)
            {
                d.renderMode = DetailRenderMode.Grass; //Actually a mesh
                d.usePrototypeMesh = true;
                #if UNITY_2021_2_OR_NEWER
                d.useInstancing = item.gpuInstancing;
                if(item.gpuInstancing) d.renderMode = DetailRenderMode.VertexLit;
                #endif
                d.prototype = item.prefab;
                d.prototypeTexture = null;

            }
            if (item.type == GrassType.Texture && item.billboard)
            {
                #if UNITY_2021_2_OR_NEWER
                d.useInstancing = false;
                #endif
                d.renderMode = item.renderAsBillboard ? DetailRenderMode.GrassBillboard : DetailRenderMode.Grass;
                d.usePrototypeMesh = false;
                d.prototypeTexture = item.billboard;
                d.prototype = null;
            }
        }
    }
}