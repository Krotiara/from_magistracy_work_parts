using CableWalker.Simulator;
using CableWalker.Simulator.Model;
using UnityEngine;

public class ShuntDevice : CableSuspensionDevice
{
    public GameObject ShuntWire;
    
    // public GameObject InstallerPlatform;
    public float PlatformHeight = 0.1f;
    string number = "";
    string shuntWireName = "shunt wire";
    string photoPath = "";
    public GameObject ClampPrefab;

    public override void InstallDevice(Cable cable, Vector3 cablePoint, Vector3 rotation, InformationHolder infoHolder)
    {
        throw new System.NotImplementedException();
    }

    public override void SaveDevice()
    {
        throw new System.NotImplementedException();
    }
}
