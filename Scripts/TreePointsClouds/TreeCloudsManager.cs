using System.Collections.Generic;
using UnityEngine;

public class TreeCloudsManager : MonoBehaviour
{
    List<TreeCloudHolder> treeClouds;
    public Transform CameraTransform;
    public float Distance;
    public bool Static = false; // dont load clouds 
    public Dictionary<AreaRange, bool>  ActiveAreas = new Dictionary<AreaRange, bool>
        {
            { AreaRange.Other, false},
            { AreaRange.Area1_3, true},
            { AreaRange.Area3_4, true},
            { AreaRange.Area4_6, true},
            { AreaRange.Area6Plus, true},
            { AreaRange.UnsafeTree, true}
        };

    public void Start()
    {
        GetHolders();
    }

    public void GetHolders()
    {
        treeClouds = new List<TreeCloudHolder>();
        foreach (Transform child in transform)
        {
            var treeCloud = child.GetComponent<TreeCloudHolder>();
            if (treeCloud != null)
                treeClouds.Add(treeCloud);
        }
    }

    public void SetAreaActivity(AreaRange range, bool activity)
    {
        foreach (var treeCloud in treeClouds)
        {
            treeCloud.ChangeAreaVisibility(range, activity);
        }
        ActiveAreas[range] = activity;
    }

    public void Update()
    {
        LoadVisibleTrees();
    }

    private async void LoadVisibleTrees()
    {
        if (Static)
            return;
        foreach (var cloud in treeClouds)
        {
            var position = cloud.transform.position;
            if (Vector3.Distance(position, CameraTransform.position) < Distance)
                await cloud.EnableTrees();
            else
                cloud.DisableTrees();
        }
    }

    public async void LoadActiveTrees()
    {
        foreach (var cloud in treeClouds)
        {
            var position = cloud.transform.position;
            if (cloud.gameObject.activeSelf)
                await cloud.EnableTrees();
        }
    }

    public void DisableActiveTrees()
    {
        foreach (var cloud in treeClouds)
        {
            var position = cloud.transform.position;
            if (cloud.gameObject.activeSelf)
                cloud.DisableTrees();
        }
    }
}
