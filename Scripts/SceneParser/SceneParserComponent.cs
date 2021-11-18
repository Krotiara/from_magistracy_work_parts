#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using CableWalker.Simulator.Tools;
using CableWalker.Simulator.Networking;
using CableWalker.Simulator.UI.FileManager;
using CableWalker.AgentModel;
using CableWalker.Simulator.UI.InfoShower;
using CableWalker.Simulator.Model;

namespace CableWalker.Simulator.SceneParser
{
    public class SceneParserComponent : EditorWindow
    {
        private static string login = "gohhinogamiz@gmail.com", password = "123";

        private float centerLatitude;
        private float centerLongitude;
        private static Dictionary<string, float[]> Areas;
        
        //private static List<string> AreasNames;
        //private int selected = 0;

        private static List<string> areasNames;
        private int selectedArea = 0;

        [MenuItem("Window/Scene Creator")]
        static void ShowWindow()
        {
            var window = GetWindow<SceneParserComponent>("SceneParserComponent");
            window.maxSize = new Vector2(500f, 600f);
            window.minSize = window.maxSize;
            window.Show();
            LoadAreas();
        }

        private static string GetStreamingAssetsPath()
        {
            return Application.platform == RuntimePlatform.Android ?
                Application.dataPath + "!assets/" : Application.streamingAssetsPath;
        }

        private static void LoadAreas()
        {
            Areas = new Dictionary<string, float[]>();
            areasNames = new List<string>();
            string pathToAreasCSV = Path.Combine(GetStreamingAssetsPath(), "Configs\\Areas.csv").Replace('\\', '/');
            //string pathToAreasCSV = Path.Combine(@"Assets/Resources/Configs", "Areas.csv");
            string pathToConfigs = Path.Combine(GetStreamingAssetsPath(), "Configs").Replace('\\', '/');// @"Assets/Resources/Configs";
            foreach (string folderPath in Directory.GetDirectories(pathToConfigs))
            {
                string configName = folderPath.Remove(0, pathToConfigs.Length + 1);
                areasNames.Add(configName);
                Areas[configName] = new float[2];
            }

            using (var parser = new StreamReader(pathToAreasCSV))
            {
                parser.ReadLine();
                while (!parser.EndOfStream)
                {
                    var args = parser.ReadLine().Split(';');
                    if (Areas.ContainsKey(args[0]))
                        Areas[args[0]] = new float[3] { float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[4]) };
                    else
                        Debug.Log(string.Format("There is no config with name {0}", args[0]));
                }
            }
        }

        private void AddMeshCollidersTo(GameObject g)
        {
            int layer = LayerMask.NameToLayer("MeshColliderLayer");
            if (layer == -1)
                throw new System.Exception("No layer with name MeshColliderLayer");
            foreach (Transform child in g.transform)
            {
                child.gameObject.AddComponent<MeshCollider>();
                child.gameObject.layer = layer;
            }
        }

        private void CreatePrefabsGUI(int selectedAreaIndex)
        {
            EditorGUILayout.LabelField("Prefab Creator", EditorStyles.boldLabel);
            if (GUILayout.Button($"Create Prefabs"))
            {
                CreatePrefabs(selectedAreaIndex);
            }
        }

        private void CreateAreaGetterGUI(int selectedAreaIndex)
        {
            EditorGUILayout.LabelField("GetGPSAreaByTowerConfig", EditorStyles.boldLabel);
            string area = "";
            
            if (GUILayout.Button($"GetArea for config {areasNames[selectedAreaIndex]}. (Result will be in Console)"))
            {
                string configPath = Application.dataPath + "/StreamingAssets/Configs/" + areasNames[selectedAreaIndex];
                string pathTowerConfig = Path.Combine(configPath, "Towers/Towers.csv");
                area = AreaGetter.GetAreaByGPS(pathTowerConfig);
                Debug.Log(area);
            }
        }

        private void CreateSceneGeneratorGUI(int selectedAreaIndex)
        {
            GUILayout.Label("First: Check that terain from WorldComposer is on scene", EditorStyles.boldLabel);
            string name;
            if (GUILayout.Button($"Generate 2D-model of power line with name {areasNames[selectedAreaIndex]}"))
            {
                var args = Areas[areasNames[selectedAreaIndex]];
                name = areasNames[selectedAreaIndex];
                string configPath = Application.dataPath + "/Resources/Configs/" + name;
                var sceneParser = new SceneParser2D(name, configPath, args[0], args[1], args[2]);
                sceneParser.Parse();
            }
                if (GUILayout.Button($"Generate 3D-model of power line with name {areasNames[selectedAreaIndex]}"))
            {
                var args = Areas[areasNames[selectedAreaIndex]];
                name = areasNames[selectedAreaIndex];
                string configPath = Application.dataPath + "/Resources/Configs/" + name;
                var sceneParser = new SceneParser(name, configPath, args[0], args[1], args[2]);
                sceneParser.Parse();
            }
        }

        //private void CreateBordersGeneratorGUI(int selectedAreaIndex)
        //{
        //    if (GUILayout.Button("CreateBorders"))
        //    {
        //        if(IsCorrectRegeneration(selectedAreaIndex))
        //        {

        //        }
        //    }
        //}

        private void CreateSceneCleanerGUI(int selectedAreaIndex)
        {
            if(GUILayout.Button($"Clear {areasNames[selectedAreaIndex]} scene"))
            {
                if(IsCorrectRegeneration(selectedAreaIndex))
                {
                    var objects = GameObject.FindGameObjectWithTag("ObjectsFolder");
                    GameObject.DestroyImmediate(objects);
                }
            }
        }

        private bool IsCorrectRegeneration(int selectedAreaIndex)
        {
            GameObject currentConfigObject;
            try
            {
                currentConfigObject = GameObject.FindGameObjectWithTag("savedDataName");
            }
            catch (Exception)
            {
                Debug.Log("Error.Scene not created yet");
                return false;
            }
            if (currentConfigObject.name != areasNames[selectedAreaIndex])
            {
                Debug.Log(
                    $"You want to update defects on 3D-model of power line with name {areasNames[selectedAreaIndex]}, but now open 3D-model of power line with name{currentConfigObject.name}");
                return false;
            }
            return true;
        }

        //private void CreateDefectsUpdaterGUI(int selectedAreaIndex)
        //{
        //    GUILayout.Label(string.Format("Update Defects On 3D-model of power line with name {0}", areasNames[selectedAreaIndex], EditorStyles.boldLabel));
        //    if (GUILayout.Button("Update"))
        //    {
        //        if (IsCorrectRegeneration(selectedAreaIndex))
        //        {
        //            var currentConfigObject = GameObject.FindGameObjectWithTag("savedDataName");
        //            var defectsConfigPath = Application.dataPath + $"/Resources/Configs/{currentConfigObject.name}/Defects";
        //            var defectsParser = new DefectsParser(defectsConfigPath, currentConfigObject);
        //            defectsParser.Parse();
        //        }
        //    }
        //}

        void OnGUI()
        {
            EditorGUILayout.LabelField("Select Config", EditorStyles.boldLabel);
            selectedArea = EditorGUILayout.Popup(selectedArea, areasNames.ToArray());
            CreateSceneCleanerGUI(selectedArea);

            EditorGUILayout.LabelField("Update line data", EditorStyles.boldLabel);

            CreateLoginButton();
            CreateDownloadButtons(selectedArea);

            CreatePrefabsGUI(selectedArea);
            CreateAreaGetterGUI(selectedArea);
            CreateSceneGeneratorGUI(selectedArea);
            CreateOutlinesGeneratorGUI(selectedArea);
            CreateSceneParserByPointCloudsGUI(selectedArea);
            CreateCablesOutlinesGeneratorGUI(selectedArea);
            CreateCreateBordersGUI(selectedArea);
          //  CreateReinstantiateCablesGUI(selectedArea);
            //CreateDefectsUpdaterGUI(selectedArea);
        }

        private void CreateCreateBordersGUI(int selectedAreaIndex)
        {
            if (GUILayout.Button("ReCreate borders"))
            {
                var infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
                infoHolder.LoadObjects(true);
                new SceneParser(infoHolder).RecalculateBorders();

            }
        }

        private void CreateReinstantiateCablesGUI(int selectedAreaIndex)
        {
            if (GUILayout.Button("Reinstantiate cables"))
            {
                var infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
                infoHolder.LoadObjects(true);
                foreach (Cable c in infoHolder.GetList<Cable>())
                    c.ReInstantiateCable(c.CurrentMode);
            }
        }

        


        private void CreateSceneParserByPointCloudsGUI(int selectedAreaIndex)
        {
            if (GUILayout.Button("Correct scene by point clouds"))
            {
                var infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
                infoHolder.LoadObjects(true);
                name = areasNames[selectedAreaIndex];
                string configPath = Application.dataPath + "/StreamingAssets/Configs/" + name;
                string pointCloudsTxts = Path.Combine(configPath, "PointCloudsTxts");
               
                var pW = new PointCloudsWorker(infoHolder, GameObject.Find("PointCloudsParent"));
                pW.LoadPointClouds(pointCloudsTxts);
                NameGiver.ReGiveNames(infoHolder);
                TerrainUtils.OffTerrains();
            }
        }

        private void CreateOutlinesGeneratorGUI(int selectedArea)
        {
            if (GUILayout.Button("AddOutLinesToObjects"))
            {
                var infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
                infoHolder.LoadObjects(true);
                var objects = infoHolder.GetAll();
                OutLinesGenerator.GenerateOutLines(infoHolder);             
            }
        }

        private void CreateCablesOutlinesGeneratorGUI(int selectedArea)
        {
            if (GUILayout.Button("AddOutLinesToCables"))
            {
                var infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
                infoHolder.LoadObjects(true);
                var objects = infoHolder.GetAll();
                OutLinesGenerator.GenerateOutLinesToCables(infoHolder);
            }
        }

        private void CreatePrefabs(int selected)
        {
            var path = "Assets/Resources/Models/";
            var configPath = $"{path}{areasNames[selected]}";
            var modelsDirectories = Directory.GetDirectories(path);
            foreach (var directory in modelsDirectories)
            {
               // var modelsPath = directory + "/Models";
                var directoryRep = directory.Replace("\\", "/");
                var directoryName = directoryRep.Split('/').Last();
                try
                {
                    var files = Directory.GetFiles(directory);
                    foreach (var fileAssetPath in files)
                    {
                        if (Path.GetExtension(fileAssetPath) == ".meta" || Path.GetExtension(fileAssetPath) == ".mtl") { continue; }
                        var file = fileAssetPath.Replace("Assets/Resources/", "").Replace("\\", "/");
                        file = Path.ChangeExtension(file, null);
                        var prefabPath = "Assets/Resources/Prefabs/Models/" + directoryName + "/" + Path.GetFileName(file);
                        prefabPath = Path.ChangeExtension(prefabPath, ".prefab");
                        prefabPath = prefabPath.Replace("\\", "/");


                        var g = Instantiate(Resources.Load(file)) as GameObject;
                        if (!File.Exists(prefabPath))
                        {
                            var boxCollider = g.AddComponent<BoxCollider>();
                            var defectsObject = new GameObject("Defects");
                            defectsObject.transform.parent = g.transform;
                            AddMeshCollidersTo(g);
                            LodGroupInstaller.InstallLodGroupRenderers(g);
                            g.AddComponent<CircumscribedSphere>();
                            PrefabUtility.SaveAsPrefabAsset(g, prefabPath);
                            DestroyImmediate(g);
                        }
                        else
                            Debug.Log($"Prefab with path {prefabPath} already exist");
                    }
                }
                catch (Exception ex)/* DirectoryNotFoundException)*/
                {
                    if (ex is DirectoryNotFoundException || ex is ArgumentException)
                    {
                        Debug.Log(ex.Message);
                        continue;
                    }
                    throw;
                }
            }
        }

        private void CreateLoginButton()
        {
            login = EditorGUILayout.TextField("Login:", login);
            password = EditorGUILayout.TextField("Password:", password);
            if (GUILayout.Button("Login"))
            {
                Networking.Network.InstantiateNetworkAsync(login, password);
            }
        }

        private void CreateDownloadButtons(int selected)
        {
            string name = areasNames[selected];
            string localConfigPath = Application.dataPath + "/StreamingAssets/Configs/" + name +"/";
            string localPhotoPath = Application.dataPath + "/StreamingAssets/Photos/" + name +"/";

            if (GUILayout.Button("Get Power Line Info"))
            {
                Networking.Network.LoadTowersAsync("Красноуфимская", localConfigPath + "Towers/towers1.csv", localPhotoPath);
            }

            if (GUILayout.Button("Get Defects"))
            {
                // Networking.Network.LoadDefectsAsync(areasNames[selected], localConfigPath  + "/defects.json");
                Networking.Network.LoadDefectsAsync("Красноуфимская", localConfigPath  + "TowerDefects/defects.csv", localPhotoPath);
            }
        }
    }
}
#endif
