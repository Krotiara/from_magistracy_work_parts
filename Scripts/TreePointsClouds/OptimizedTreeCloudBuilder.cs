#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OptimizedTreeCloudBuilder : MonoBehaviour
{
    public GameObject CreateSpheresCloud(TreeCloud treeCloud)
    {
        GameObject treeCloudObject = new GameObject(treeCloud.name + "_Opt");
        treeCloud.SetColoredArea();
        MakeSpheres(treeCloud, treeCloudObject);
        return treeCloudObject;
    }


    public GameObject Create2DCloud(TreeCloud treeCloud)
    {
        GameObject treeCloudObject = new GameObject(treeCloud.name + "_2D");
        treeCloud.SetColoredArea();
        Make2D(treeCloud, treeCloudObject); 
        return treeCloudObject;
    }

    public GameObject CreateTreesCloud(TreeCloud treeCloud)
    {
        GameObject treeCloudObject = new GameObject(treeCloud.name + "_Tree");
        treeCloud.SetColoredArea();
        MakeTrees(treeCloud, treeCloudObject);
        return treeCloudObject;
    }

    private void Make2D(TreeCloud treeCloud, GameObject treeCloudObject)
    {
        int areaStep = 50;
        int treeStep = 2500;

        string prefabPath = "Assets/Resources/PointCloud/";

        var prefab0_1 = AssetDatabase.LoadAssetAtPath(prefabPath + "Sphere0_12d.prefab", typeof(GameObject)) as GameObject;
        var prefab1_3 = AssetDatabase.LoadAssetAtPath(prefabPath + "Sphere1_32d.prefab", typeof(GameObject)) as GameObject;
        var prefab3_4 = AssetDatabase.LoadAssetAtPath(prefabPath + "Sphere3_42d.prefab", typeof(GameObject)) as GameObject;
        var prefab4_6 = AssetDatabase.LoadAssetAtPath(prefabPath + "Sphere4_62d.prefab", typeof(GameObject)) as GameObject;
        var prefab6Plus = AssetDatabase.LoadAssetAtPath(prefabPath + "Sphere6Plus2d.prefab", typeof(GameObject)) as GameObject;
        var unsafeTreePrefab = AssetDatabase.LoadAssetAtPath(prefabPath + "UnsafeTree.prefab", typeof(GameObject)) as GameObject;
       // unsafeTreePrefab = AssetDatabase.LoadAssetAtPath(prefabPath + "SphereUnsafe_tree.prefab", typeof(GameObject)) as GameObject;

        var separatedCloud = treeCloudObject.AddComponent<SeparatedTreeCloud>();

        var positions = treeCloud.Vertices;
        //separatedCloud.Area0_1 = MarkArea(treeCloud.ColoredArea.Area[AreaRange.Other], positions, prefab0_1, areaStep);
        //separatedCloud.Area0_1.name = "Area0_1";
        separatedCloud.Area1_3 = MarkAreaWithSphere(treeCloud.ColoredArea.Area[AreaRange.Area1_3], positions, prefab1_3, treeCloudObject, areaStep, true);
        separatedCloud.Area1_3.name = "Area1_3";
        separatedCloud.Area3_4 = MarkAreaWithSphere(treeCloud.ColoredArea.Area[AreaRange.Area3_4], positions, prefab3_4, treeCloudObject, areaStep, true);
        separatedCloud.Area3_4.name = "Area3_4";
        separatedCloud.Area4_6 = MarkAreaWithSphere(treeCloud.ColoredArea.Area[AreaRange.Area4_6], positions, prefab4_6, treeCloudObject, areaStep, true);
        separatedCloud.Area4_6.name = "Area4_6";
        separatedCloud.Area6Plus = MarkAreaWithSphere(treeCloud.ColoredArea.Area[AreaRange.Area6Plus], positions, prefab6Plus, treeCloudObject, areaStep, true);
        separatedCloud.Area6Plus.name = "Area6Plus";
        separatedCloud.UnsafeTrees = MarkAreaWithSphere(treeCloud.ColoredArea.Area[AreaRange.UnsafeTree], positions, unsafeTreePrefab, treeCloudObject, treeStep, true);
        separatedCloud.UnsafeTrees.name = "UnsafeTrees";
    }

    private void MakeSpheres(TreeCloud treeCloud, GameObject treeCloudObject)
    {
        int areaStep = 50;
        int treeStep = 50;

        string prefabPath = "Assets/Resources/PointCloud/";

        var prefab0_1 = AssetDatabase.LoadAssetAtPath(prefabPath + "Sphere0_1.prefab", typeof(GameObject)) as GameObject;
        var prefab1_3 = AssetDatabase.LoadAssetAtPath(prefabPath + "Sphere1_3.prefab", typeof(GameObject)) as GameObject;
        var prefab3_4 = AssetDatabase.LoadAssetAtPath(prefabPath + "Sphere3_4.prefab", typeof(GameObject)) as GameObject;
        var prefab4_6 = AssetDatabase.LoadAssetAtPath(prefabPath + "Sphere4_6.prefab", typeof(GameObject)) as GameObject;
        var prefab6Plus = AssetDatabase.LoadAssetAtPath(prefabPath + "Sphere6Plus.prefab", typeof(GameObject)) as GameObject;

        var unsafeTreePrefab = AssetDatabase.LoadAssetAtPath(prefabPath + "SphereUnsafe_tree.prefab", typeof(GameObject)) as GameObject;

        var separatedCloud = treeCloudObject.AddComponent<SeparatedTreeCloud>();

        var positions = treeCloud.Vertices;
        //separatedCloud.Area0_1 = MarkArea(treeCloud.ColoredArea.Area[AreaRange.Other], positions, prefab0_1, areaStep);
        //separatedCloud.Area0_1.name = "Area0_1";
        separatedCloud.Area1_3 = MarkAreaWithSphere(treeCloud.ColoredArea.Area[AreaRange.Area1_3], positions, prefab1_3, treeCloudObject, areaStep);
        separatedCloud.Area1_3.name = "Area1_3";
        separatedCloud.Area3_4 = MarkAreaWithSphere(treeCloud.ColoredArea.Area[AreaRange.Area3_4], positions, prefab3_4, treeCloudObject, areaStep);
        separatedCloud.Area3_4.name = "Area3_4";
        separatedCloud.Area4_6 = MarkAreaWithSphere(treeCloud.ColoredArea.Area[AreaRange.Area4_6], positions, prefab4_6, treeCloudObject, areaStep);
        separatedCloud.Area4_6.name = "Area4_6";
        separatedCloud.Area6Plus = MarkAreaWithSphere(treeCloud.ColoredArea.Area[AreaRange.Area6Plus], positions, prefab6Plus, treeCloudObject, areaStep);
        separatedCloud.Area6Plus.name = "Area6Plus";
        separatedCloud.UnsafeTrees = MarkAreaWithSphere(treeCloud.ColoredArea.Area[AreaRange.UnsafeTree], positions, unsafeTreePrefab, treeCloudObject, treeStep);
        separatedCloud.UnsafeTrees.name = "UnsafeTrees";
    }

    private Transform MarkAreaWithSphere(List<int> indexes, Vector3[] positions, GameObject markPrefab, GameObject treeCloudObject, int step, bool toGround = false)
    {
        GameObject go = new GameObject();

        for (int i = 0; i < indexes.Count; i += step)
        {
            int index = indexes[i];
            var mark = PrefabUtility.InstantiatePrefab(markPrefab) as GameObject;
            if (toGround)
                mark.transform.position = new Vector3( positions[index].x, 0, positions[index].z);
            else
                mark.transform.position = positions[index];

            mark.transform.SetParent(go.transform, false);
        }
        go.transform.SetParent(treeCloudObject.transform);
        return go.transform;
    }

    private void MakeTrees(TreeCloud treeCloud, GameObject treeCloudObject)
    {
        string prefabPath = "Assets/Resources/PointCloud/";

        var prefab1_3 = AssetDatabase.LoadAssetAtPath(prefabPath + "tree1_3.prefab", typeof(GameObject)) as GameObject;
        var prefab3_4 = AssetDatabase.LoadAssetAtPath(prefabPath + "tree3_4.prefab", typeof(GameObject)) as GameObject;
        var prefab4_6 = AssetDatabase.LoadAssetAtPath(prefabPath + "tree4_6.prefab", typeof(GameObject)) as GameObject;
        var prefab6Plus = AssetDatabase.LoadAssetAtPath(prefabPath + "tree6Plus.prefab", typeof(GameObject)) as GameObject;
        var unsafeTreePrefab = AssetDatabase.LoadAssetAtPath(prefabPath + "treeUnsafe_tree.prefab", typeof(GameObject)) as GameObject;
        int treeStep = 500;
        int bushStep = 500;

        var visualizedTree = treeCloudObject.AddComponent<VisualizedTreeCloud>();

        var positions = treeCloud.Vertices;

        visualizedTree.Area1_3 = MarkAreaWithTree(treeCloud.ColoredArea.Area[AreaRange.Area1_3], positions, prefab1_3, treeCloudObject, bushStep, 1.5f);
        visualizedTree.Area1_3.name = "Area1_3";
        visualizedTree.Area3_4 = MarkAreaWithTree(treeCloud.ColoredArea.Area[AreaRange.Area3_4], positions, prefab3_4, treeCloudObject, bushStep, 1.2f);
        visualizedTree.Area3_4.name = "Area3_4";
        visualizedTree.Area4_6 = MarkAreaWithTree(treeCloud.ColoredArea.Area[AreaRange.Area4_6], positions, prefab4_6, treeCloudObject, treeStep, 1.2f);
        visualizedTree.Area4_6.name = "Area4_6";
        visualizedTree.Area6Plus = MarkAreaWithTree(treeCloud.ColoredArea.Area[AreaRange.Area6Plus], positions, prefab6Plus, treeCloudObject, treeStep, 1.2f);
        visualizedTree.Area6Plus.name = "Area6Plus";
        visualizedTree.UnsafeTrees = MarkAreaWithTree(treeCloud.ColoredArea.Area[AreaRange.UnsafeTree], positions, unsafeTreePrefab, treeCloudObject, treeStep, 1.3f);
        visualizedTree.UnsafeTrees.name = "UnsafeTrees";
    }

    private Transform MarkAreaWithTree(List<int> indexes, Vector3[] positions, GameObject markPrefab, GameObject treeCloudObject, int step, float Maxmin)
    {
        GameObject tree = new GameObject();
        tree.transform.SetParent(treeCloudObject.transform, false);

        for (int i = 0; i < indexes.Count; i += step)
        {
            int index = indexes[i];
            var mark = (PrefabUtility.InstantiatePrefab(markPrefab) as GameObject);
            RandomRotate(mark);
            RandomScale(mark, Maxmin);

            mark.transform.position = new Vector3(positions[index].x, 0, positions[index].z);
            mark.transform.SetParent(tree.transform, false);
        }

        return tree.transform;
    }

    private void RandomRotate(GameObject go)
    {
        float y = Random.Range(0, 360);
        go.transform.eulerAngles = new Vector3(go.transform.eulerAngles.x, y, go.transform.eulerAngles.z);
    }

    private void RandomScale(GameObject go, float maxMin)
    {
        float scale = Random.Range(1, maxMin);
        go.transform.localScale *= scale;
    }
}
# endif
