using CableWalker.Simulator;
using CableWalker.Simulator.Model;
using UnityEngine;

public abstract class CableSuspensionDevice: MonoBehaviour 
{
    public float DeviceHeight;
    public float UpAfterRotate = 0;

    public abstract void InstallDevice(Cable cable, Vector3 cablePoint, Vector3 rotation, InformationHolder infoHolder);

    public abstract void SaveDevice();
}
