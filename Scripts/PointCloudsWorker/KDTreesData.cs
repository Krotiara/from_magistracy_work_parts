using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[Serializable]
public class KDTreesData 
{
    [SerializeField]
    public List<Vector3> GroundPoints = new List<Vector3>();
    [SerializeField]
    public List<Vector3> GreenPoints = new List<Vector3>();
    [SerializeField]
    public List<Vector3> CrossedCablesPoints = new List<Vector3>();
    [SerializeField]
    public List<Vector3> RoadsPoints = new List<Vector3>();
    [SerializeField]
    public List<Vector3> BuildingsPoints = new List<Vector3>();

    public KDTreesData()
    {

    }
}
