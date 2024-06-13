// Vegetation Spawner by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace sc.terrain.vegetationspawner
{
    public static class VegetationExtensions
    {
        #region Trees
        /// <summary>
        /// Validate if the given prefab is being used on the terrain
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public static bool ContainsTreePrefab(this Terrain terrain, Object prefab)
        {
            for (int i = 0; i < terrain.terrainData.treePrototypes.Length; i++)
            {
                TreePrototype tree = terrain.terrainData.treePrototypes[i];

                if (tree.prefab == prefab) return true;
            }

            return false;
        }

        /// <summary>
        /// Checks all the tree instances on the terrain, and removes the one's whose position falls within the radius of the given position
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        public static void RemoveTreesAtPosition(this Terrain terrain, Vector3 position, float radius)
        {
            TreeInstance[] instances = terrain.GetTreeInstances();
            
            List<TreeInstance> treeInstanceCollection = new List<TreeInstance>();

            float r = radius * radius;
            for (int i = 0; i < instances.Length; i++)
            {
                //Leave trees outside of the radius
                if ((instances[i].TransformTreePosition(terrain) - position).sqrMagnitude >= r)
                {
                    treeInstanceCollection.Add(instances[i]);
                }
            }
            
            Debug.Log($"Removed {instances.Length - treeInstanceCollection.Count} trees");
            
            terrain.terrainData.SetTreeInstances(treeInstanceCollection.ToArray(), false);
            
            #if UNITY_EDITOR
            EditorUtility.SetDirty(terrain.terrainData);
            #endif
        }
        
        /// <summary>
        /// Removes all tree instances of a given prefab
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="prefab"></param>
        public static void RemoveTrees(this Terrain terrain, Object prefab)
        {
            int prototypeIndex = terrain.GetTreePrototypeIndex(prefab);
            
            List<TreeInstance> treeInstanceCollection = new List<TreeInstance>(terrain.terrainData.treeInstances);
            //Clear all existing instances first, setting the tree instances is additive
            for (int i = 0; i < treeInstanceCollection.Count; i++)
            {
                treeInstanceCollection.RemoveAll(x => x.prototypeIndex == prototypeIndex);
            }
            
            terrain.terrainData.SetTreeInstances(treeInstanceCollection.ToArray(), false);
            
            #if UNITY_EDITOR
            EditorUtility.SetDirty(terrain.terrainData);
            #endif
        }

        /// <summary>
        /// Retrieve all tree instances present on the terrain. If the 'targetPrefab' parameter is not specified, all trees are returned
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="targetPrefab"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static TreeInstance[] GetTreeInstances(this Terrain terrain, Object targetPrefab = null)
        {
            TreeInstance[] instances = new TreeInstance[0];
            if (targetPrefab)
            {
                var prototypeIndex = -1;
                
                //Get the index of the given prefab
                for (int i = 0; i < terrain.terrainData.treePrototypes.Length; i++)
                {
                    if (terrain.terrainData.treePrototypes[i].prefab == targetPrefab)
                    {
                        prototypeIndex = i;
                    }
                }

                if (prototypeIndex >= 0)
                {
                    //Get all instances matching the prefab index
                    instances = terrain.terrainData.treeInstances.Where(x => x.prototypeIndex == prototypeIndex).ToArray();
                }
                else
                {
                    throw new ArgumentException($"Prefab \"{targetPrefab.name}\" could not be found on terrain \"{terrain.name}\"");
                }
            }
            else
            {
                instances = terrain.terrainData.treeInstances;
            }

            return instances;
        }

        /// <summary>
        /// A struct that represents a TreeInstance's position/scale/rotation in a GameObject-style manner
        /// </summary>
        [Serializable]
        public struct TreeTransform
        {
            /// <summary>
            /// Position in terrain-space
            /// </summary>
            public Vector3 localPosition;
            /// <summary>
            /// World-space position
            /// </summary>
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }

        /// <summary>
        /// Given an array of TreeInstances, create an array of TreeTransforms
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="instances"></param>
        /// <returns></returns>
        public static TreeTransform[] CreateTreeTransforms(this Terrain terrain, TreeInstance[] instances)
        {
            int instanceCount = instances.Length;
            TreeTransform[] transforms = new TreeTransform[instanceCount];

            for (int i = 0; i < instanceCount; i++)
            {
                transforms[i] = new TreeTransform
                {
                    localPosition = Vector3.Scale(instances[i].position, terrain.terrainData.size),
                    position = instances[i].TransformTreePosition(terrain),
                    rotation = instances[i].TransformTreeRotation(),
                    scale = new Vector3(instances[i].widthScale, instances[i].heightScale, instances[i].widthScale)
                };

            }

            return transforms;
        }
        
        /// <summary>
        /// Retrieves the current tree instances and converts them to world-space transforms. 
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="prefab">Filter by a specific tree prefab</param>
        /// <returns>An array of structs containing the position/rotation/scale</returns>
        /// <exception cref="ArgumentException">Tree prefab isn't present on the terrain</exception>
        public static TreeTransform[] GetTreeTransforms(this Terrain terrain, Object prefab = null)
        {
            TreeInstance[] instances = terrain.GetTreeInstances(prefab);
            
            TreeTransform[] transforms = terrain.CreateTreeTransforms(instances);

            return transforms;
        }

        /// <summary>
        /// Instantiate the terrain's tree prefabs as GameObjects in the scene
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="targetPrefab">Limit the operation to a specific tree prefab</param>
        /// <returns></returns>
        public static GameObject[] InstantiateTreePrefabs(this Terrain terrain, Object targetPrefab = null)
        {
            List<Object> prefabs = new List<Object>();
            if (!targetPrefab)
            {
                for (int i = 0; i < terrain.terrainData.treePrototypes.Length; i++)
                {
                    if (terrain.terrainData.treePrototypes[i].prefab) prefabs.Add(terrain.terrainData.treePrototypes[i].prefab);
                }
            }
            else
            {
                prefabs.Add(targetPrefab);
            }

            List<GameObject> objs = new List<GameObject>();
            
            foreach (Object prefab in prefabs)
            {
                TreeTransform[] transforms = terrain.GetTreeTransforms(prefab);
                
                for (int i = 0; i < transforms.Length; i++)
                {
                    #if UNITY_EDITOR
                    GameObject instance = PrefabUtility.InstantiatePrefab(prefab, terrain.transform) as GameObject;
                    #else
                    GameObject instance = (GameObject)Object.Instantiate(prefab, terrain.transform);
                    #endif

                    objs.Add(instance);
                    
                    instance.name = $"{prefab.name} ({i})";

                    instance.transform.SetPositionAndRotation(transforms[i].position, transforms[i].rotation);
                    instance.transform.localScale = transforms[i].scale;
                }
            }

            return objs.ToArray();
        }
        
        public static TreePrototype GetTreePrototype(this Terrain terrain, Object prefab)
        {
            //Get the index of the given prefab
            for (int i = 0; i < terrain.terrainData.treePrototypes.Length; i++)
            {
                if (terrain.terrainData.treePrototypes[i].prefab == prefab)
                {
                    return terrain.terrainData.treePrototypes[i];
                }
            }

            throw new ArgumentException($"Prefab \"{prefab.name}\" could not be found on terrain \"{terrain.name}\"");
        }
        
        public static int GetTreePrototypeIndex(this Terrain terrain, Object prefab)
        {
            //Get the index of the given prefab
            for (int i = 0; i < terrain.terrainData.treePrototypes.Length; i++)
            {
                if (terrain.terrainData.treePrototypes[i].prefab == prefab)
                {
                    return i;
                }
            }

            throw new ArgumentException($"Prefab \"{prefab.name}\" could not be found on terrain \"{terrain.name}\"");
        }

        /// <summary>
        /// Creates GameObjects with a capsule collider for every tree instance on the terrain
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="targetPrefab">If left unassigned, processes all tree types</param>
        /// <returns></returns>
        public static CapsuleCollider[] InstantiateTreeColliders(this Terrain terrain, Object targetPrefab = null)
        {
            //Terrain system only supports capsule colliders
            List<CapsuleCollider> colliders = new List<CapsuleCollider>();

            TreePrototype[] treeTypes;
            
            if (targetPrefab)
            {
                treeTypes = terrain.terrainData.treePrototypes.Where(x => x.prefab == targetPrefab).ToArray();
            }
            else
            {
                treeTypes = terrain.terrainData.treePrototypes;
            }
            
            //Debug.Log($"Extracting for {trees.Length} different trees");
            
            for (int i = 0; i < treeTypes.Length; i++)
            {
                if (treeTypes[i].prefab == null)
                {
                    throw new Exception($"Tree item at index {i} is missing a prefab");
                }
                
                CapsuleCollider prefabCollider = treeTypes[i].prefab.GetComponent<CapsuleCollider>();
                
                if(!prefabCollider) continue;
                
                //Get all instances matching the current prototype
                TreeInstance[] instances = terrain.terrainData.treeInstances.Where(x => x.prototypeIndex == i).ToArray();
                
                //Debug.Log($"Extracting {instances.Length} instances");
                for (int j = 0; j < instances.Length; j++)
                {
                    GameObject obj = new GameObject(treeTypes[i].prefab.name + j);
                    obj.name = $"{treeTypes[i].prefab.name} Collider ({j})";
                    
                    CapsuleCollider capsuleCollider = obj.AddComponent<CapsuleCollider>();

                    //Copy values
                    capsuleCollider.isTrigger = prefabCollider.isTrigger;
                    capsuleCollider.material = prefabCollider.material;
                    capsuleCollider.center = prefabCollider.center;
                    capsuleCollider.radius = prefabCollider.radius;
                    capsuleCollider.height = prefabCollider.height;
                    capsuleCollider.direction = prefabCollider.direction;

                    colliders.Add(capsuleCollider);

                    obj.layer = terrain.preserveTreePrototypeLayers ? treeTypes[i].prefab.layer : terrain.gameObject.layer;
 
                    obj.transform.localScale = Vector3.Scale(new Vector3(instances[j].widthScale, instances[j].heightScale, instances[j].widthScale), treeTypes[i].prefab.transform.localScale);

                    obj.transform.SetPositionAndRotation(instances[j].TransformTreePosition(terrain), instances[j].TransformTreeRotation());
                    obj.transform.parent = terrain.transform;
                    obj.isStatic = true;
                }
            }
            
            return colliders.ToArray();
        }
        
        #region Private methods
        /// <summary>
        /// Convert the local-space position of a tree instance into world-space
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        private static Vector3 TransformTreePosition(this TreeInstance tree, Terrain terrain)
        {
            //Terrains never have a rotation/scale, so scaling and offsetting suffices
            return Vector3.Scale(tree.position, terrain.terrainData.size) + terrain.GetPosition();
        }
        
        /// <summary>
        /// Convert the Y-rotation of a tree instance into a usable Quaternion
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="terrain"></param>
        /// <returns></returns>
        private static Quaternion TransformTreeRotation(this TreeInstance tree)
        {
            return Quaternion.Euler(0f, Mathf.Rad2Deg * tree.rotation, 0f);
        }
        #endregion
        #endregion
    }
}