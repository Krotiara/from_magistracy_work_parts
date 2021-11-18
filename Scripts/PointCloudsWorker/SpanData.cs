using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SpanData
{
    [SerializeField]
    public List<string> CablesData = new List<string>();
    [SerializeField]
    public List<string> DKRAreasData = new List<string>();
    [SerializeField]
    public List<string> ThreatingPointsData = new List<string>();
    [SerializeField]
    public string SpanLengthData  = "";
    [SerializeField]
    public string EnvironmentData = "";

    [SerializeField]
    public string IgnorePhase = "";

    [SerializeField]
    public List<int> GroundPoints = new List<int>();
    [SerializeField]
    public List<int> GreenPoints = new List<int>();
    [SerializeField]
    public List<int> CrossedCablesPoints = new List<int>();
    [SerializeField]
    public List<int> RoadsPoints = new List<int>();
    [SerializeField]
    public List<int> BuildingsPoints = new List<int>();
    [SerializeField]
    public List<int> NoiseIndexes = new List<int>();

    public SpanData()
    {

    }
}
