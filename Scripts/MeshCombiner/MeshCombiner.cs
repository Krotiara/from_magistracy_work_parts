using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshCombiner 
{
    public static void CombineMeshesTo(GameObject obj)
    {
        MeshFilter[] meshFilters  = obj.GetComponentsInChildren<MeshFilter>();
    }
}
