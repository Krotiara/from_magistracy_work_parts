using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CableWalker.Simulator;
using System;
using System.IO;
using System.Linq;
using CableWalker.Simulator.Model;


namespace CableWalker.AgentModel
{

    public class CableTxtData
    {
        public Dictionary<string, string[]> dataByPhases;
        public string pointCloudEnvironmentData;

        public List<Vector3> GetPoints(string phase)
        {
            try
            {
                var res = new List<Vector3>();
                string[] split = dataByPhases[phase];
                Vector3 startPoint = new Vector3(float.Parse(split[2]), float.Parse(split[3]), float.Parse(split[4]));
                Vector3 lowest = new Vector3(float.Parse(split[5]), float.Parse(split[6]), float.Parse(split[7]));
                Vector3 end = new Vector3(float.Parse(split[8]), float.Parse(split[9]), float.Parse(split[10]));
                return new List<Vector3>() { startPoint, lowest, end };
            }
            catch(Exception e)
            {
                string k = "";
                foreach (var key in dataByPhases.Keys)
                    k += key + ";";
                Debug.Log(string.Format("Keys:{0}. Phase = {1}", k, phase));
                throw e;
            }
        }

        public CableTxtData(FileInfo file)
        {
            dataByPhases = new Dictionary<string, string[]>();
            using (StreamReader sr = file.OpenText())
            {

                string lines = sr.ReadToEnd();
                SpanData spanData = DataLoader<SpanData>.LoadJson(lines);
                foreach(string line in spanData.CablesData)
                {
                    string[] split = line.Split(';').Select(x => x.Replace(',', '.')).ToArray();
                    dataByPhases[split[0]] = split.ToArray();
                }
                pointCloudEnvironmentData = spanData.EnvironmentData;
            }     
        }

        public CableTxtData(SpanData spanData)
        {
            dataByPhases = new Dictionary<string, string[]>();
            foreach (string line in spanData.CablesData)
            {
                string[] split = line.Split(';').Select(x => x.Replace(',', '.')).ToArray();
                dataByPhases[split[0]] = split.ToArray();
            }
            pointCloudEnvironmentData = spanData.EnvironmentData;

        }
    }

    public class CablesRecalculatorByPointClouds
    {
        public CablesRecalculatorByPointClouds()
        {

        }


        public void RecalculateCables(List<GameObject> pointClouds, Dictionary<string, SpanData> spansData,
            InformationHolder infoHolder, Func<string, string, bool> isReverseSpanFunc)
        {

            foreach (Cable c in infoHolder.GetList<Cable>())
                c.ReInstantiateCable(c.CurrentMode);

            string oneEnvironmentTxtData = "";
            bool isEnvironmentDataSet = false;
            foreach (GameObject p in pointClouds)
            {
                string[] numberSplit = p.name.Split('_');
                string number = string.Format("{0}_{1}_{2}", numberSplit[0], numberSplit[1], numberSplit[2]);
                string Tower2 = numberSplit[1];
                string Tower1 = numberSplit[0];
                string phase = numberSplit[2];
                bool isReverseSpan = isReverseSpanFunc(Tower1, Tower2);
                Tower2 = Tower2.ToLower();
                Tower1 = Tower1.ToLower();
                string spanNumber = isReverseSpan ? string.Format("{0}-{1}", Tower2, Tower1) : string.Format("{0}-{1}", Tower1, Tower2);
                CableTxtData txtData = new CableTxtData(spansData[number]);
                PointCloudMeshController pCMC = p.GetComponent<PointCloudMeshController>();


                foreach (string key in txtData.dataByPhases.Keys)
                {
                    string cableNumber = string.Format("{0}.{1}", spanNumber, key);
                    Cable cable = infoHolder.Get<Cable>(cableNumber);
                    if (cable == null)
                    {
                        Debug.Log(string.Format("No cable in span {0} with phase {1}", spanNumber, key));
                        continue;
                    }
                    List<Vector3> pointCloudsPoints = txtData.GetPoints(key);

                    cable.SetPoints(p.transform.TransformPoint(pointCloudsPoints[0]),
                        p.transform.TransformPoint(pointCloudsPoints[1]),
                        p.transform.TransformPoint(pointCloudsPoints[2]));
                    cable.RecalculateByNewPoints(txtData.pointCloudEnvironmentData);
                    if (!isEnvironmentDataSet)
                    {
                        oneEnvironmentTxtData = txtData.pointCloudEnvironmentData;
                        isEnvironmentDataSet = true;
                    }
                }
            }

            
            if(oneEnvironmentTxtData != "")
            {
                float[] newEnvironment = oneEnvironmentTxtData.Replace(',', '.').Split(';').Select(x => float.Parse(x)).ToArray();
                infoHolder.Environment.SetParams(newEnvironment[0], newEnvironment[1], newEnvironment[2], newEnvironment[3], WindDirection.Perpendicular);
                RecalculateByOneEnvironment(infoHolder.GetCablesList(), infoHolder.Environment);
            }
            
        }

        private void RecalculateByOneEnvironment(List<Cable> cables, Simulator.Environment environment)
        {
            foreach (Cable c in cables)
            {
                c.CalculateCalcModeParams(c.CalcMode, environment);
                c.CurrentMode = (CableMode)c.CalcMode.Clone();
            }
        }
    }
}
