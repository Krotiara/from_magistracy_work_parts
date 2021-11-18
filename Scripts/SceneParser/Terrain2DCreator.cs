#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;



namespace CableWalker.Simulator.SceneParser
{
    public class Terrain2DCreator : EditorWindow
    {
        private static string terrainHolderNameTag = "TerrainsHolder";
        //private static string pathToTerrainAssets;

        [MenuItem("Window/Create2DTerrainToTerrainOnScene")]
        public static void Create2DTerrain()
        {
            //var window = GetWindow<Terrain2DCreator>("Terrain2DCreator");
            //window.maxSize = new Vector2(500f, 400f);
            //window.minSize = window.maxSize;
            //window.Show();

            //pathToTerrainAssets = GUILayout.TextField(pathToTerrainAssets, 30);
            //TODO copy assets terrain

            //if (GUILayout.Button($"Create2DTerrain"))
            //{
                GameObject worldComposerTerrain = GameObject.FindGameObjectWithTag(terrainHolderNameTag);
                if (worldComposerTerrain == null)
                    return;
            Object parentObject = EditorUtility.GetPrefabParent(worldComposerTerrain);
            string path = AssetDatabase.GetAssetPath(parentObject);
            string fileName = Path.GetFileName(path);
            path = path.Replace(fileName, "");
            PrefabUtility.UnpackPrefabInstance(worldComposerTerrain, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                worldComposerTerrain.name += "2D";
                foreach (Transform terrain in worldComposerTerrain.transform)
                {
                    var t = terrain.GetComponent<Terrain>();
                    var tData = (TerrainData)Object.Instantiate(t.terrainData);
                    t.terrainData = tData;
                    TerrainCollider tc = t.gameObject.GetComponent<TerrainCollider>();
                    tc.terrainData = tData;

                    var xRes = tData.heightmapWidth;
                    var yRes = tData.heightmapHeight;
                    var heights = tData.GetHeights(0, 0, xRes, yRes);
                    for (int x = 0; x < tData.heightmapWidth; x++)
                        for (int y = 0; y < tData.heightmapHeight; y++)
                            heights[y, x] = 0;
                    tData.SetHeights(0, 0, heights);
                
                
                AssetDatabase.CreateAsset(tData, $"{path}/{terrain.name}2D.asset");
                AssetDatabase.SaveAssets();
                }
            //}
          
        }



    }
}
#endif
