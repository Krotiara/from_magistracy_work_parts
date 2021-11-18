using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;
using CableWalker.Simulator.Modules;

namespace CableWalker.Simulator.LaserScanning
{
    public class LaserScanner : MonoBehaviour
    {
        public string MeshLayerName = "MeshColliderLayer";
        public int ScanningAngle = 240;
        public int ValuesPerScanning = 100;
        public int MaxDistance = 5600;
        public GameObject RayOnePrefab;

        private int LaserScanIteration = 120;
        private string fileHeader = "COFF";
        private string fileName = "Assets/Resources/PointCloud/";
        private string fileExtension = ".off";

        RaycastHit hit;
        Camera laserCamera;
        int hitNumber = 0;
        float offsetAngle;
        Vector3 distance;
        int layerMask;

        bool forcedStop;
        StreamWriter sr;


        public void StartScanning()
        {
            offsetAngle = (float)ScanningAngle / ValuesPerScanning;
            laserCamera = GetComponent<Camera>();
            layerMask   = LayerMask.GetMask(MeshLayerName);
            fileName += DateTime.Now.ToString("dd.MM.yyyy-HH.mm.ss") + fileExtension;
            sr = new StreamWriter(fileName, true);
        }


        public void Scann()
        {
            laserCamera.transform.rotation = Quaternion.Euler(0, -(ScanningAngle / 2), 0);
            for (int i = 0; i < ValuesPerScanning; i++)
            // преломление луча по горизонтали и позиция по вертекали 
            {
                if (forcedStop)
                    return;
                Ray ray = laserCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                if (Physics.Raycast(ray, out hit, MaxDistance, layerMask))
                {
                    hitNumber++;
                    Transform objectHit = hit.transform;
                    distance = hit.point;
                    sr.WriteLine(distance.x.ToString(CultureInfo.InvariantCulture) + ' '
                                + distance.y.ToString(CultureInfo.InvariantCulture) + ' '
                                + distance.z.ToString(CultureInfo.InvariantCulture));
                }
                //rotate ray
                laserCamera.transform.Rotate(0, offsetAngle, 0, Space.Self);
            }
        }

        public void EndScanning()
        {
            sr.Close();
            forcedStop = true; 
            //записать в начало файла заголовок и количество точек
            File.WriteAllText(fileName, string.Format("{0}\n{1}\n{2}", fileHeader, hitNumber, File.ReadAllText(fileName)));
        }
    }
}
