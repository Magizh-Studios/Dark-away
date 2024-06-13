using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace sc.terrain.vegetationspawner
{
    public class VegetationUtilitiesWindow : EditorWindow
    {
        //[MenuItem("Tools/Vegetation Utilities")]
        public static void ShowWindow()
        {
            VegetationUtilitiesWindow editorWindow = GetWindow<VegetationUtilitiesWindow>(true, " Vegetation Utilities", true);
            
            //Open somewhat in the center of the screen
            #if !UNITY_EDITOR_OSX //DPI Scaling prevents this from properly working
            editorWindow.position = new Rect((Screen.currentResolution.width / 2f) - (550 * 0.5f), (Screen.currentResolution.height / 2f)  - (450 * 0.8f), 550, 450);
            #endif
            
            editorWindow.Show();
        }

        private Terrain[] terrains = new Terrain[0];
        private Vector2 terrainScrollPos;
        public Object targetPrefab;

        private bool deleteTrees = true;
        
        private void OnEnable()
        {
            RefreshTerrains();
        }

        private void OnFocus()
        {
            RefreshTerrains();
        }

        private void RefreshTerrains()
        {
            if (VegetationSpawner.Current) terrains = VegetationSpawner.Current.terrains.ToArray();
            else terrains = Terrain.activeTerrains;
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Target terrains ({terrains.Length})", EditorStyles.boldLabel);
                if (GUILayout.Button("Refresh", GUILayout.MaxWidth(75f)))
                {
                    RefreshTerrains();
                }
            }

            terrainScrollPos = EditorGUILayout.BeginScrollView(terrainScrollPos, EditorStyles.textArea, GUILayout.Height(100f));
            {
                foreach (var t in terrains)
                {
                    DrawTerrainInto(t);
                }

            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Convert trees to GameObjects", EditorStyles.whiteLargeLabel);

                    if (GUILayout.Button("Execute"))
                    {
                        var progress = true;
                        int counter = 0;
                        foreach (Terrain terrain in terrains)
                        {
                            if (EditorUtility.DisplayCancelableProgressBar("Vegetation Utilities", $"Converting vegetation for {terrain.name} ({counter}/{terrains.Length})", (float)counter / (float)terrains.Length))
                            {
                                progress = false;
                            }
                            
                            if (progress)
                            {
                                terrain.InstantiateTreePrefabs(targetPrefab);
                                if (deleteTrees) terrain.terrainData.SetTreeInstances(new TreeInstance[0], false);

                                counter++;
                            }

                            EditorUtility.ClearProgressBar();

                        }
                    }
                    if (GUILayout.Button(new GUIContent("Delete Objects", "Destroys all child objects of a terrain." +
                                                                          "\n\nWarning: this may include other objects not created by this tool!" +
                                                                          "\n\nYou can undo the deletion through CTRL+Z just in case!")))
                    {
                        foreach (Terrain terrain in terrains)
                        {
                            terrain.drawTreesAndFoliage = true;

                        }

                        DeleteChildObjects();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    deleteTrees = EditorGUILayout.ToggleLeft("Delete terrain tree data", deleteTrees);
                }
                EditorGUILayout.EndHorizontal();

            
                EditorGUILayout.HelpBox("Instantiates all tree prefabs at the same position as they appear on the terrain, and makes them child objects of a terrain", MessageType.Info);
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Extract tree capsule colliders", EditorStyles.whiteLargeLabel);

                    if (GUILayout.Button("Execute"))
                    {
                        var progress = true;
                        int counter = 0;

                        foreach (Terrain terrain in terrains)
                        {
                            if (EditorUtility.DisplayCancelableProgressBar("Vegetation Utilities", $"Extracting colliders from {terrain.name} ({counter}/{terrains.Length})", (float)counter / (float)terrains.Length))
                            {
                                progress = false;
                            }
                            
                            if (progress)
                            {
                                terrain.InstantiateTreeColliders(targetPrefab);

                                counter++;
                            }

                            EditorUtility.ClearProgressBar();
                        }
                    }
                    if (GUILayout.Button(new GUIContent("Delete Colliders", "Destroys all child objects of a terrain that use a Capsule Collider")))
                    {
                        foreach (Terrain terrain in terrains)
                        {
                            DeleteColliders(terrain);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.HelpBox("Instantiates a GameObject with an identical Capsule Collider, copied from the related tree prefab. Use this to include trees in navmesh data.", MessageType.Info);
            EditorGUILayout.EndVertical();
            
        }

        private void DeleteChildObjects()
        {
            List<Object> children = new List<Object>();
            
            foreach (Terrain terrain in terrains)
            {
                foreach (Transform child in terrain.transform)
                {
                    //Skip the first, since its the terrain itself
                    if(child == terrain.transform) continue;
                
                    children.Add(child.gameObject);
                }
            }
 
            Undo.RecordObjects(children.ToArray(), "Deleted terrain child objects");
            
            foreach (GameObject child in children)
            {
                //Remove all previously created objects first
                DestroyImmediate(child);
            }
        }

        private void DeleteColliders(Terrain terrain)
        {
            CapsuleCollider[] colliders = terrain.GetComponentsInChildren<CapsuleCollider>();

            foreach (CapsuleCollider col in colliders)
            {
                DestroyImmediate(col.gameObject);
            }
        }
        
        private void DrawTerrainInto(Terrain t)
        {
            if (!t) return;

            string size = t.terrainData.size.ToString();
            EditorGUILayout.LabelField(new GUIContent(" " + t.name + " " + size, EditorGUIUtility.IconContent("Terrain Icon").image));
        }
    }
}