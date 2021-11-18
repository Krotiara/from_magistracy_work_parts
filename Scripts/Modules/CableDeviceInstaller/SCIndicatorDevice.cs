using CableWalker.Simulator;
using CableWalker.Simulator.Model;
using UnityEngine;

public class SCIndicatorDevice : CableSuspensionDevice
{
    

    string number = "";
    string clampName = "SCIndicator";
    string photoPath = "";
    public GameObject SCIndicatorPrefab;

    public override void InstallDevice(Cable cable, Vector3 cablePoint, Vector3 rotation, InformationHolder infoHolder)
    {
        throw new System.NotImplementedException();
    }

    public override void SaveDevice()
    {
        throw new System.NotImplementedException();
    }
}
