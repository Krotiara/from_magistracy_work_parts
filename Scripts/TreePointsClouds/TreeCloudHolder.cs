using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class TreeCloudHolder : MonoBehaviour
{
    public string TreeCloudSourcePrefab;
    Transform coloredAreaFolder;
    ITreeCloud treeCloud;

    //public bool IsVisible;

    //void OnBecameVisible()
    //{
    //    IsVisible = true;
    //}

    //void OnBecameInvisible()
    //{
    //    IsVisible = false;
    //}

    bool IsOn = false;
    TreeCloudsManager cloudsManager;

    public void Start()
    {
        cloudsManager = transform.parent.GetComponent<TreeCloudsManager>();
        if (cloudsManager.Static)
        {
            coloredAreaFolder = transform.GetChild(0);
            treeCloud = coloredAreaFolder.transform.GetComponent<ITreeCloud>();
            IsOn = true;
            foreach (AreaRange range in Enum.GetValues(typeof(AreaRange)))
            {
                treeCloud.ChangeAreaVisibility(range, cloudsManager.ActiveAreas[range]);
            }
        }
    }

    public void DisableTrees()
    {
        if (!IsOn)
            return;
        IsOn = false;
        StartCoroutine(DestroyTrees());
    }

    public async Task EnableTrees()
    {
        if (IsOn)
            return;
        IsOn = true;
        await LoadTrees();
    }

    private async Task LoadTrees()
    {
        var resourceRequest = (GameObject) await Resources.LoadAsync<GameObject>(TreeCloudSourcePrefab);

        if (resourceRequest != null)
        {
            coloredAreaFolder = GameObject.Instantiate(resourceRequest as GameObject).transform;
            coloredAreaFolder.SetParent(transform, false);
            coloredAreaFolder.transform.localPosition = new Vector3(0, 0);
            coloredAreaFolder.transform.localEulerAngles = new Vector3(0, 0);
            treeCloud = coloredAreaFolder.transform.GetComponent<ITreeCloud>();
            foreach (AreaRange range in Enum.GetValues(typeof(AreaRange)))
            {
                treeCloud.ChangeAreaVisibility(range, cloudsManager.ActiveAreas[range]);
            }
        }
    }

    public void ChangeAreaVisibility(AreaRange range, bool visibility)
    {
        if (IsOn && treeCloud != null)
            treeCloud.ChangeAreaVisibility(range, visibility);
    }

    private IEnumerator DestroyTrees()
    {
        yield return null;
        if (treeCloud != null)
            DestroyImmediate(coloredAreaFolder.gameObject);
    }
}
