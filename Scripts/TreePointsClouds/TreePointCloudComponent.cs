#if UNITY_EDITOR
using CableWalker.Simulator;
using UnityEditor;
using UnityEngine;

public class TreePointCloudComponent : EditorWindow
{
    public string PointCloudFolderTag = "PointCloudFolder";
    public string CameraTag = "SpectatorCamera";
    public string PointCloudHoldersTag = "PointCloudHoldersFolder";
    static bool staticCloud = false;

    [MenuItem("Window/Optimize Point Clouds")]

    static void ShowWindow()
    {
        var window = GetWindow<TreePointCloudComponent>("Tree Point Cloud Component");
        window.maxSize = new Vector2(500f, 200f);
        window.minSize = window.maxSize;
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Tag clouds folder on scene with tag '"+ PointCloudFolderTag+"'", EditorStyles.boldLabel);
        CreatOptimizeCloudsGUI();
    }

    private void CreatOptimizeCloudsGUI()
    {
        if (GUILayout.Button("Add TreeCloud Component to objects from tagged folder"))
        {
            AddTreeCloudComponent();
        }

        EditorGUILayout.LabelField("Select cloud Options:");
        staticCloud = EditorGUILayout.ToggleLeft("Static Cloud (always on scene)", staticCloud);

        if (GUILayout.Button("Optimize Clouds from tagged folder"))
        {
            Optimize();
        }

        if (GUILayout.Button("Create 2D version"))
        {
            Create2D();
        }

        if (GUILayout.Button("Create clouds with tree visualization"))
        {
            CreateTrees();
        }

        if (GUILayout.Button("Create holders of non optimized clouds"))
        {
            CreateHolders();
        }

        if (GUILayout.Button("Load point clouds of active holders"))
        {
            ShowTreeClouds();
        }

        if (GUILayout.Button("Disable point clouds of active holders"))
        {
            DisableTreeClouds();
        }

        if (GUILayout.Button("Create Prefabs For Cloud Holders"))
        {
            CreatePrefabsForHolders();
        }
    }

    private void AddTreeCloudComponent()
    {
        var cloudFolder = GameObject.FindGameObjectWithTag(PointCloudFolderTag);
        foreach (Transform cloud in cloudFolder.transform)
        {
            cloud.gameObject.AddComponent<TreeCloud>();
        }
    }

    private void Optimize()
    {
        var optimizedCloudsFolder = GameObject.Find("TreeCloudsOpt");
        if (optimizedCloudsFolder == null)
        {
            optimizedCloudsFolder = new GameObject("TreeCloudsOpt");
            optimizedCloudsFolder.tag = PointCloudHoldersTag;
            var cloudManagerComponent = optimizedCloudsFolder.AddComponent<TreeCloudsManager>();
            cloudManagerComponent.Distance = 500;
            cloudManagerComponent.CameraTransform = GameObject.FindGameObjectWithTag(CameraTag).transform;
            cloudManagerComponent.Static = staticCloud;
        }
        var optimizer = new OptimizedTreeCloudBuilder();
        var cloudFolder = GameObject.FindGameObjectWithTag(PointCloudFolderTag);
        foreach (Transform cloud in cloudFolder.transform)
        {
            if (!cloud.gameObject.activeSelf)
                continue;

            var treeCloud = cloud.GetComponent<TreeCloud>();
            if (treeCloud==null)
            {
                Debug.Log($"Object {cloud.name} dont'have TreeCloud component");
                continue;
            }

            var optimizedCloud = optimizer.CreateSpheresCloud(treeCloud);
            var treeCloudHolder = new GameObject(optimizedCloud.name);
            var treeCloudComponent = treeCloudHolder.AddComponent<TreeCloudHolder>();

            treeCloudHolder.transform.SetParent(optimizedCloudsFolder.transform, false);
            treeCloudHolder.transform.position = cloud.transform.position;
            treeCloudHolder.transform.rotation = cloud.transform.rotation;
            var prefabPath = "PointCloud/Clouds/OptimizedPointsClouds/" + cloud.name;   
            treeCloudComponent.TreeCloudSourcePrefab = prefabPath;
            
            optimizedCloud.transform.SetParent(treeCloudHolder.transform, false);
            CreatePrefab(optimizedCloud, prefabPath);
            if (!staticCloud)
                DestroyImmediate(optimizedCloud);
        }
    }

    private void Create2D()
    {

        var optimizedCloudsFolder = GameObject.Find("TreeClouds2D");
        if (optimizedCloudsFolder == null)
        {
            optimizedCloudsFolder = new GameObject("TreeClouds2D");
            optimizedCloudsFolder.tag = PointCloudHoldersTag;
            var cloudManagerComponent = optimizedCloudsFolder.AddComponent<TreeCloudsManager>();
            cloudManagerComponent.Distance = 500;
            cloudManagerComponent.CameraTransform = GameObject.FindGameObjectWithTag(CameraTag).transform;
            cloudManagerComponent.Static = staticCloud;
        }
        var optimizer = new OptimizedTreeCloudBuilder();
        var cloudFolder = GameObject.FindGameObjectWithTag(PointCloudFolderTag);
        foreach (Transform cloud in cloudFolder.transform)
        {
            if (!cloud.gameObject.activeSelf)
                continue;

            var treeCloud = cloud.GetComponent<TreeCloud>();
            if (treeCloud == null)
            {
                Debug.Log($"Object {cloud.name} dont'have TreeCloud component");
                continue;
            }

            var optimizedCloud = optimizer.Create2DCloud(treeCloud);
            var treeCloudHolder = new GameObject(optimizedCloud.name);
            var treeCloudComponent = treeCloudHolder.AddComponent<TreeCloudHolder>();

            treeCloudHolder.transform.SetParent(optimizedCloudsFolder.transform, false);
            treeCloudHolder.transform.position = cloud.transform.position;
            treeCloudHolder.transform.rotation = cloud.transform.rotation;
            var prefabPath = "PointCloud/Clouds/OptimizedPointsClouds/2D/" + cloud.name;
            treeCloudComponent.TreeCloudSourcePrefab = prefabPath;

            optimizedCloud.transform.SetParent(treeCloudHolder.transform, false);
            CreatePrefab(optimizedCloud, prefabPath);
            if (!staticCloud)
                DestroyImmediate(optimizedCloud);

        }
    }

    private void CreateTrees()
    {
        var optimizedCloudsFolder = GameObject.Find("VisualizedTrees");
        if (optimizedCloudsFolder == null)
        {
            optimizedCloudsFolder = new GameObject("VisualizedTrees");
            optimizedCloudsFolder.tag = PointCloudHoldersTag;
            var cloudManagerComponent = optimizedCloudsFolder.AddComponent<TreeCloudsManager>();
            cloudManagerComponent.Distance = 500;
            cloudManagerComponent.CameraTransform = GameObject.FindGameObjectWithTag(CameraTag).transform;
            cloudManagerComponent.Static = staticCloud;
        }
        var optimizer = new OptimizedTreeCloudBuilder();
        var cloudFolder = GameObject.FindGameObjectWithTag(PointCloudFolderTag);
        foreach (Transform cloud in cloudFolder.transform)
        {
            if (!cloud.gameObject.activeSelf)
                continue;

            var treeCloud = cloud.GetComponent<TreeCloud>();
            if (treeCloud == null)
            {
                Debug.Log($"Object {cloud.name} dont'have TreeCloud component");
                continue;
            }

            var treeCloudHolder = new GameObject(cloud.name);
            var treeCloudComponent = treeCloudHolder.AddComponent<TreeCloudHolder>();
            treeCloudHolder.transform.position = cloud.transform.position;
            treeCloudHolder.transform.rotation = cloud.transform.rotation;
            treeCloudHolder.transform.SetParent(optimizedCloudsFolder.transform, false);


            var visualizedCloud = optimizer.CreateTreesCloud(treeCloud);


            var prefabPath = "PointCloud/Clouds/VisualizedTrees/" + cloud.name;
            treeCloudComponent.TreeCloudSourcePrefab = prefabPath;

            visualizedCloud.transform.SetParent(treeCloudHolder.transform, false);

            foreach (Transform area in visualizedCloud.transform)
            {
                foreach (Transform tree in area)
                {
                    var a = new Vector3(tree.position.x, 0, tree.position.z);
                    var currentTerrain = TerrainUtils.GetCurrentTerrain(Terrain.activeTerrain, a);
                    var pos = currentTerrain.SampleHeight(a);
                    tree.position = new Vector3(tree.position.x, pos, tree.position.z);
                }
            }

            CreatePrefab(visualizedCloud, prefabPath);
            if (!staticCloud)
                DestroyImmediate(visualizedCloud);
        }
    }


    private void CreateHolders()
    {
        var cloudsFolder = new GameObject("TreeClouds");
        var cloudManagerComponent = cloudsFolder.AddComponent<TreeCloudsManager>();
        cloudsFolder.tag = PointCloudHoldersTag;
        cloudManagerComponent.Distance = 200;
        cloudManagerComponent.CameraTransform = GameObject.FindGameObjectWithTag(CameraTag).transform;
        cloudManagerComponent.Static = staticCloud;

        var cloudFolder = GameObject.FindGameObjectWithTag(PointCloudFolderTag);
        
        foreach (Transform cloud in cloudFolder.transform)
        {
            if (!cloud.gameObject.activeSelf)
                continue;

            var treeCloudHolder = new GameObject(cloud.name);
            var treeCloudComponent = treeCloudHolder.AddComponent<TreeCloudHolder>();

            treeCloudHolder.transform.SetParent(cloudsFolder.transform, false);
            treeCloudHolder.transform.position = cloud.transform.position;
            treeCloudHolder.transform.rotation = cloud.transform.rotation;
            var prefabPath = "PointCloud/PointsCloudsPrefabs/" + cloud.name;
            treeCloudComponent.TreeCloudSourcePrefab = prefabPath;

            CreatePrefab(cloud.gameObject, prefabPath);
        }
    }

    private void ShowTreeClouds()
    {
        var cloudFolder = GameObject.FindObjectsOfType<TreeCloudsManager>();
        cloudFolder[0].GetHolders();
        cloudFolder[0].LoadActiveTrees();
    }

    private void DisableTreeClouds()
    {
        var cloudFolder = GameObject.FindGameObjectWithTag(PointCloudHoldersTag).GetComponent<TreeCloudsManager>();
        cloudFolder.GetHolders();
      //  cloudFolder.LoadactiveTrees();
    }

    private void CreatePrefabsForHolders()
    {
        var optimizedCloudsFolder = GameObject.Find("TreeCloudsOpt");
        
        foreach (Transform cloud in optimizedCloudsFolder.transform)
        {
            if (!cloud.gameObject.activeSelf)
                continue;

            var prefabPath = "PointCloud/OptimizedPointsClouds/WithPosition/" + cloud.name;

            CreatePrefab(cloud.gameObject, prefabPath);
        }
    }


    private void CreatePrefab(GameObject gameObject, string prefabPath)
    {
        prefabPath = "Assets/Resources/" + prefabPath +".prefab";
        PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
    }
}
#endif
