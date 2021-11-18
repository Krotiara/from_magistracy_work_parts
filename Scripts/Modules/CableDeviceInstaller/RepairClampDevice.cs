
using CableWalker.Simulator.Model;
using UnityEngine;


namespace CableWalker.Simulator
{
    public class RepairClampDevice : CableSuspensionDevice
    {
        public GameObject ClampCasing;
        public GameObject InstallerPlatform;
        public float PlatformHeight = 0.1f;
        string photoPath = "";
        public GameObject ClampPrefab;

        public override void InstallDevice(Cable cable, Vector3 cablePoint, Vector3 rotation, InformationHolder infoHolder)
        {
            // поднимаем установочную платформу
            InstallerPlatform.transform.localPosition += new Vector3(0, PlatformHeight, 0); //TODO сделать плавно
            // поворот кожуха муфты
            ClampCasing.transform.localRotation = Quaternion.Euler(0, 0, 180);//TODO сделать плавно
            var clamp = new RepairClamp($"{cablePoint.ToString()} clamp", $"{cablePoint.ToString()} clamp", photoPath, ClampPrefab, "RepairClamp", cable, cablePoint, rotation);
            clamp.InstansiateSafely();
            infoHolder.Set(clamp);
            ClampCasing.transform.localRotation = Quaternion.Euler(0, 0, -180); //TODO сделать плавно
            InstallerPlatform.transform.localPosition += new Vector3(0, -PlatformHeight, 0);//TODO сделать плавно

        }

        public override void SaveDevice()
        {
            return; //TODO Запись в файл конфига инфу
        }
    }
}
