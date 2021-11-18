using CableWalker.Simulator;
using CableWalker.Simulator.Model;
using CableWalker.Simulator.UI.FileManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


namespace CableWalker.AgentModel
{

    public class PointCloudsWorker
    {

        public InformationHolder informationHolder;
        public GameObject pointCloudsParent;
        private GameObject localCoordsCalcObj;

        public List<(GameObject, Vector3, Vector3)> PointCloudMeshes { get; set; }


        public PointCloudsWorker(InformationHolder infoHolder, GameObject pointCloudsParent)
        {
            PointCloudMeshes = new List<(GameObject, Vector3, Vector3)>();
            localCoordsCalcObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            informationHolder = infoHolder;
            this.pointCloudsParent = pointCloudsParent;
        }

        private Vector3[] GetTxtData(FileInfo fileInfo)
        {
            //string path = string.Format("Assets/Resources/txts/{0}_threatingPoints.txt", pointCloudMeshWorkingV2.mesh.name.Replace("_changes_dkr", "").Replace(" Instance", ""));
            //try
            //{
            using (StreamReader sr =fileInfo.OpenText())
            {
                string lines = sr.ReadToEnd();
                SpanData spanData = DataLoader<SpanData>.LoadJson(lines);
                float[] split = spanData.SpanLengthData.Split(';').Select(x => float.Parse(x.Replace(',', '.'))).ToArray();
                return new Vector3[2] { new Vector3(split[0], split[1], split[2]), new Vector3(split[3], split[4], split[5]) };
                
            }
            throw new Exception("In GetTxtData s-g wrong");
            //}
            //catch (Exception e)
            //{
            //    // Let the user know what went wrong.
            //    Debug.Log("The file could not be read:");
            //    Debug.Log(e.Message);
            //}
        }

        private Vector3[] GetTxtData(SpanData spanData)
        {
            //string path = string.Format("Assets/Resources/txts/{0}_threatingPoints.txt", pointCloudMeshWorkingV2.mesh.name.Replace("_changes_dkr", "").Replace(" Instance", ""));
            //try
            //{

            float[] split = spanData.SpanLengthData.Split(';').Select(x => float.Parse(x.Replace(',', '.'))).ToArray();
            return new Vector3[2] { new Vector3(split[0], split[1], split[2]), new Vector3(split[3], split[4], split[5]) };


            //}
            //catch (Exception e)
            //{
            //    // Let the user know what went wrong.
            //    Debug.Log("The file could not be read:");
            //    Debug.Log(e.Message);
            //}
        }





        public void LoadPointClouds(string pointCloudsTxtsPath)
        {
            PointCloudMeshes.Clear();
            var spansData = informationHolder.LoadSpansData(pointCloudsTxtsPath);

            Dictionary<string, Vector3[]> txtsData = new Dictionary<string, Vector3[]>();

            foreach(var entry in spansData)
            {
                txtsData[entry.Key] = GetTxtData(entry.Value);
            }

            //DirectoryInfo dinfo = new DirectoryInfo(pointCloudsTxtsPath);
            //FileInfo[] Files = dinfo.GetFiles("*.txt");
            //foreach (FileInfo file in Files)
            //{
            //    string[] split = file.Name.Split('_');
            //    string number = string.Format("{0}_{1}_{2}", split[0], split[1], split[2]);
            //    txtsData[number] = GetTxtData(file);
                
            //}

            List<GameObject> pointClouds = new List<GameObject>();
            foreach (Transform p in pointCloudsParent.transform)
            {
                if (p.gameObject.GetComponent<PointCloudMeshController>() == null)
                {
                    p.gameObject.AddComponent<PointCloudMeshController>();
                }
                pointClouds.Add(p.gameObject);
                //string[] numberSplit = p.name.Split('_');
                //string number = string.Format("{0}_{1}_{2}", numberSplit[0], numberSplit[1], numberSplit[2]);
                //PointCloudMeshes.Add((p.gameObject, txtsData[number][0], txtsData[number][1]));
            }

            AdjustTowersPositionsByPointCloudMeshes(pointClouds, txtsData);
            new CablesRecalculatorByPointClouds().RecalculateCables(pointClouds, spansData, informationHolder, informationHolder.IsReverseSpan);
        }  


        private void AdjustTowersPositionsByPointCloudMeshes(List<GameObject> pointClouds, Dictionary<string, Vector3[]> txtData)
        {
            //Пока что принимаем, что pointClouds отсортированы и там нет развернутых
            string[] numberSplit = pointClouds[0].name.Split('_');
            string number = string.Format("{0}_{1}_{2}", numberSplit[0], numberSplit[1], numberSplit[2]);

            foreach (GameObject pointCloud in pointClouds)
            {
                numberSplit = pointCloud.name.Split('_');
                number = string.Format("{0}_{1}_{2}", numberSplit[0], numberSplit[1], numberSplit[2]);

                Vector3 spanStart = txtData[number][0];
                Vector3 spanEnd = txtData[number][1];
                float d = (new Vector3(spanEnd.x,0, spanEnd.z) - new Vector3(spanStart.x,0, spanStart.z)).magnitude;


                string Tower2 = numberSplit[1];
                string Tower1 = numberSplit[0];
                Vector3 tower1LocalStartPos = spanStart.z < spanEnd.z ? spanStart : spanEnd;
                Vector3 tower2LocalStartPos = spanStart.z < spanEnd.z ? spanEnd : spanStart;

                bool isReverseSpan = informationHolder.IsReverseSpan(Tower1, Tower2);
                Tower tower1 = informationHolder.Get<Tower>(Tower1.Replace('A','a'));
                Tower tower2 = informationHolder.Get<Tower>(Tower2.Replace('A', 'a'));
                Vector3 dir = new Vector3(tower2.Position.x, 0, tower2.Position.z) - new Vector3(tower1.Position.x, 0, tower1.Position.z);
                pointCloud.transform.rotation = Quaternion.LookRotation(dir);
                //dir = isReverseSpan ? new Vector3(tower1.Position.x, 0, tower1.Position.z) - new Vector3(tower2.Position.x, 0, tower2.Position.z) :
                //     new Vector3(tower2.Position.x, 0, tower2.Position.z) - new Vector3(tower1.Position.x, 0, tower1.Position.z);
                localCoordsCalcObj.transform.rotation = Quaternion.LookRotation(dir);
                if (!isReverseSpan)
                {

                    localCoordsCalcObj.transform.parent = tower1.ObjectOnScene.transform;
                    localCoordsCalcObj.transform.localPosition = new Vector3(-tower1LocalStartPos.x, -tower1LocalStartPos.y, -tower1LocalStartPos.z);
                    pointCloud.transform.position = localCoordsCalcObj.transform.position;
                    localCoordsCalcObj.transform.parent = pointCloud.transform;
                    //localCoordsCalcObj.transform.localPosition = localCoordsCalcObj.transform.localPosition + new Vector3(tower2LocalStartPos.x, tower2LocalStartPos.y, tower2LocalStartPos.z);
                    localCoordsCalcObj.transform.localPosition = new Vector3(tower2LocalStartPos.x,tower2LocalStartPos.y, tower2LocalStartPos.z);
                    Vector3 pdGlobalEndPos = localCoordsCalcObj.transform.position;
                    tower2.SetPosition(pdGlobalEndPos);
                }
                else
                {
                    //Вроде работает...
                    localCoordsCalcObj.transform.parent = tower2.ObjectOnScene.transform;
                    localCoordsCalcObj.transform.localPosition = new Vector3(tower2LocalStartPos.x, -tower2LocalStartPos.y, d); //возможно тут косяк
                    pointCloud.transform.position = localCoordsCalcObj.transform.position;
                    localCoordsCalcObj.transform.parent = pointCloud.transform;
                    localCoordsCalcObj.transform.localPosition = new Vector3(tower1LocalStartPos.x, tower1LocalStartPos.y, tower1LocalStartPos.z); //надо понять, почему не надо корректировать х
                    Vector3 pdGlobalEndPos = localCoordsCalcObj.transform.position;
                    tower1.SetPosition(pdGlobalEndPos);
                }
            }
        }


        public float HorD(Vector3 a,Vector3 b)
        {
            return (new Vector3(a.x, 0, a.z) - new Vector3(b.x, 0, b.z)).magnitude;
        }
       

       

        //// Start is called before the first frame update
        //void Start()
        //{
        //    PointCloudMeshes = new List<(GameObject, Vector3, Vector3)>();
        //    localCoordsCalcObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //}

        //// Update is called once per frame
        //void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.L))
        //        LoadPointClouds();
        //}
    }
}
