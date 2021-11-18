using CableWalker.Simulator;
using CableWalker.Simulator.Model;
using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;


namespace CableWalker.Simulator.Model
{
    public class Span : Model
    {
        
        public Tower FirstTower { get; }
        public Tower SecondTower { get; }

        public List<Cable> Cables { get; }

        public List<Cable> GroundCables => Cables.Where(x => x.IsGroundCable && x.Start.Tower.Number != x.End.Tower.Number).ToList();
        public List<Cable> PowerCables => Cables.Where(x => !x.IsGroundCable && x.Start.Tower.Number != x.End.Tower.Number).ToList();

        private GameObject linePrefab;

        //public Cable LeftCable => Cables.OrderBy(x => x.LowerstPointPosition.x).First();
        //public Cable RightCable => Cables.OrderBy(x => x.LowerstPointPosition.x).Last();
        public Cable[] LeftRightCables
        {
            get
            {
                var res = new Cable[2];
                GameObject dir = new GameObject();
                dir.transform.position = (FirstTower.Position + SecondTower.Position);
                dir.transform.rotation = Quaternion.LookRotation(new Vector3(SecondTower.Position.x, 0, SecondTower.Position.z) - new Vector3(FirstTower.Position.x, 0, FirstTower.Position.z));
                var a = dir.transform.InverseTransformPoint(Cables[0].LowestPoint);
                res[0] = Cables.OrderBy(x => dir.transform.InverseTransformPoint(x.LowestPoint).x).First(); //Left
                res[1] = Cables.OrderBy(x => dir.transform.InverseTransformPoint(x.LowestPoint).x).Last(); //Right
                GameObject.DestroyImmediate(dir);
               UnityEngine.Debug.Log(string.Format("Span: {0}, left = {1}, right = {2}", Number, res[0].Phase, res[1].Phase));
                return res;
            }

        }
        //public Cable RightCable => Cables.OrderBy(x => x.LowerstPointPosition.x).Last();
        //public List<Cable> LeftRightCables => new List<Cable>() { LeftCable, RightCable };

        public KDTree PowerCablesKDTree { get
            {
                List<Vector3> points = new List<Vector3>();
                foreach(Cable c in PowerCables)
                {
                    points.AddRange(c.Points);
                }
                return new KDTree(points.ToArray());
            } }

        public Dictionary<string, VibrationDamper> VibrationDampers { get; } //TODO

        public List<GameObject> NearestObjects { get; private set; }

        public float Length
        {
            get
            {
                var v1 = new Vector3(FirstTower.Position.x, 0, FirstTower.Position.z);
                var v2 = new Vector3(SecondTower.Position.x, 0, SecondTower.Position.z);
                return (v2 - v1).magnitude;
            }
        }

        private IEnumerable<string> Phases => new[]
            {
            "B",
            "C",
            "A",
            "G1", //грозотрос
            "G2" //грозотрос
        };

        public SpanData SpanData {get;set;}

       


        public PointCloudMeshController PointCloudMeshController;
        Line groundLine;
        Line greenLine;
        Line crossedCablesLine;
        Line roadsLine;
        Line buildsLine;
        public Material lineMat;
        public SpanAreaTSVData SpanAreaTSVData { get; set; }
        public ThreatingTreesSpanData ThreatingTreesSpanData { get; set; }
        public Line SpanLine { get; set; }

        public Span(Tower firstTower, Tower secondTower, List<Cable> cables, string tag)
        {
            FirstTower = firstTower;
            SecondTower = secondTower;
            Number = $"{FirstTower.Number}-{SecondTower.Number}";
            Position = (FirstTower.Position + SecondTower.Position) / 2;
            Tag = tag;
            Cables = cables;
            VibrationDampers = new Dictionary<string, VibrationDamper>();
            Defects = new List<Defect>();
            foreach (Cable g in GroundCables)
            {
                foreach(var pair in g.VibrationDampers)
                {
                    VibrationDampers[pair.Key] = pair.Value;
                }
               
            }
            foreach (Cable p in PowerCables)
            {
                foreach (var pair in p.VibrationDampers)
                {
                    VibrationDampers[pair.Key] = pair.Value;
                }
            }
           


        }

        public Span(Tower firstTower, Tower secondTower, string tag)
        {
            FirstTower = firstTower;
            SecondTower = secondTower;
            Defects = new List<Defect>();
            Number = $"{FirstTower.Number}-{SecondTower.Number}";
            Position = (FirstTower.Position + SecondTower.Position) / 2;
            Tag = tag;
           
        }

        public void InstantiateLines(GameObject linePrefab)
        {
            groundLine = new Line(Vector3.zero, Vector3.zero, lineMat, Color.blue, linePrefab, "ground");
            greenLine = new Line(Vector3.zero, Vector3.zero, lineMat, Color.green, linePrefab, "green");
            crossedCablesLine = new Line(Vector3.zero, Vector3.zero, lineMat, Color.yellow, linePrefab, "crossed cables");
            roadsLine = new Line(Vector3.zero, Vector3.zero, lineMat, Color.grey, linePrefab, "roads");
            buildsLine = new Line(Vector3.zero, Vector3.zero, lineMat, Color.grey, linePrefab, "builds");
        }

        public Dictionary<string, Vector3[]> CalculateCablesDistances()
        {

            //костыль
            //return new Dictionary<string, Vector3[]>();
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            if (PointCloudMeshController == null)
                throw new System.NotImplementedException();
          
            
            Dictionary<string, KDTree> calcDistancesPoints = PointCloudMeshController.CategoriesKDTrees;
            //KDQuery query = new KDQuery();
            //List<int> resultIndexes = new List<int>();
            //List<float> resultDistances = new List<float>();

            Vector3[] curPointsForGroundCategory = new Vector3[2] { Vector3.zero, Vector3.zero };
            Vector3[] curPointsForGreenCategory = new Vector3[2] { Vector3.zero, Vector3.zero };
            Vector3[] curPointsForCrossedCablesCategory = new Vector3[2] { Vector3.zero, Vector3.zero };
            Vector3[] curPointsForRoadsCategory = new Vector3[2] { Vector3.zero, Vector3.zero };
            Vector3[] curPointsForBuildingsCategory = new Vector3[2] { Vector3.zero, Vector3.zero };

            float dGround = float.MaxValue;
            float dGreen = float.MaxValue;
            float dCrossedCables = float.MaxValue;
            float dRoads = float.MaxValue;
            float dBuilds = float.MaxValue;
            float d;

            bool isGround = calcDistancesPoints.ContainsKey("ground");
            bool isGreen = calcDistancesPoints.ContainsKey("green");
            bool isCrossedCables = calcDistancesPoints.ContainsKey("crossedCables");
            bool isRoads = calcDistancesPoints.ContainsKey("roads");
            bool isBuildings = calcDistancesPoints.ContainsKey("buildings");

            Vector3[] points;

            Parallel.ForEach(Cables, new Action<Cable>(x => ProcessCable(x, calcDistancesPoints)));

            //foreach (cable cable in cables)
            //{
            //    var t = new thread(() => processcable(cable, calcdistancespoints));
            //    t.start();
                #region old
                //if (entry.Key.Contains("filter"))
                //    continue;
                //if (entry.Value.Number == IgnorePhaseTocalcDistances)
                //    continue;

                //if (isGround)
                //{
                //    points = GetMinDistForPoints(cable, calcDistancesPoints["ground"], query);
                //    points[1] = new Vector3(points[0].x, points[1].y, points[0].z);
                //    d = (points[0] - points[1]).magnitude;
                //    if (d < dGround)
                //    {
                //        curPointsForGroundCategory = points;
                //        dGround = d;
                //    }

                //}

                //if (isGreen)
                //{
                //    points = GetMinDistForPoints(cable, calcDistancesPoints["green"], query);
                //    d = (points[0] - points[1]).magnitude;
                //    if (d < dGreen)
                //    {
                //        curPointsForGreenCategory = points;
                //        dGreen = d;
                //    }
                //}

                //if (isCrossedCables)
                //{
                //    points = GetMinDistForPoints(cable, calcDistancesPoints["crossedCables"], query);
                //    d = (points[0] - points[1]).magnitude;
                //    if (d < dCrossedCables)
                //    {
                //        curPointsForCrossedCablesCategory = points;
                //        dCrossedCables = d;
                //    }
                //}

                //if (isRoads)
                //{
                //    points = GetMinDistForPoints(cable, calcDistancesPoints["roads"], query);
                //    d = (points[0] - points[1]).magnitude;
                //    if (d < dRoads)
                //    {
                //        curPointsForRoadsCategory = points;
                //        dRoads = d;
                //    }
                //}
                #endregion
            //}

            foreach (Cable cable in Cables)
            {
                if (isGround)
                {
                    d = (cable.distancesDict["ground"][0] - cable.distancesDict["ground"][1]).magnitude;
                    if(d < dGround)
                    {
                        dGround = d;
                        curPointsForGroundCategory = cable.distancesDict["ground"];
                    }    
                }
                if (isGreen)
                {
                    d = (cable.distancesDict["green"][0] - cable.distancesDict["green"][1]).magnitude;
                    if (d < dGreen)
                    {
                        dGreen = d;
                        curPointsForGreenCategory = cable.distancesDict["green"];
                    }
                }
                if (isCrossedCables)
                {
                    d = (cable.distancesDict["crossedCables"][0] - cable.distancesDict["crossedCables"][1]).magnitude;
                    if (d < dCrossedCables)
                    {
                        dCrossedCables = d;
                        curPointsForCrossedCablesCategory = cable.distancesDict["crossedCables"];
                    }
                }
                if (isRoads)
                {
                    d = (cable.distancesDict["roads"][0] - cable.distancesDict["roads"][1]).magnitude;
                    if (d < dRoads)
                    {
                        dRoads = d;
                        curPointsForRoadsCategory = cable.distancesDict["roads"];
                    }
                }
                if (isBuildings)
                {
                    d = (cable.distancesDict["buildings"][0] - cable.distancesDict["buildings"][1]).magnitude;
                    if (d < dBuilds)
                    {
                        dBuilds = d;
                        curPointsForBuildingsCategory = cable.distancesDict["buildings"];
                    }
                }
            }

            

            var result = new Dictionary<string, Vector3[]>();
            if (isGround)
                result["ground"] =  curPointsForGroundCategory;
            if (isGreen)
                result["green"] = curPointsForGreenCategory;
            if (isCrossedCables)
                result["crossedCables"] = curPointsForCrossedCablesCategory;
            if (isRoads)
                result["roads"] = curPointsForRoadsCategory;
            if (isBuildings)
                result["buildings"] = curPointsForBuildingsCategory;
            //stopwatch.Stop();
            //UnityEngine.Debug.Log("Потрачено секунд на выполнение: " + stopwatch.ElapsedMilliseconds * 0.001);
            return result;
            
        }



        private void ProcessCable(Cable cable, Dictionary<string, KDTree> calcDistancesPoints)
        {
            //Косяк с разделяемой средой calcDistancesPoints?
            KDQuery query = new KDQuery();
            List<int> resultIndexes = new List<int>();
            List<float> resultDistances = new List<float>();
            float dGround = float.MaxValue;
            float dGreen = float.MaxValue;
            float dCrossedCables = float.MaxValue;
            float dRoads = float.MaxValue;
            float dBuilds = float.MaxValue;
            float d;

            bool isGround = calcDistancesPoints.ContainsKey("ground");
            bool isGreen = calcDistancesPoints.ContainsKey("green");
            bool isCrossedCables = calcDistancesPoints.ContainsKey("crossedCables");
            bool isRoads = calcDistancesPoints.ContainsKey("roads");
            bool isBuildings = calcDistancesPoints.ContainsKey("buildings");


            Vector3[] points;

            if (isGround)
            {
                points = GetMinDistForPoints(cable, calcDistancesPoints["ground"], query);
                points[1] = new Vector3(points[0].x, points[1].y, points[0].z);
                d = (points[0] - points[1]).magnitude;
                if (d < dGround)
                {
                    cable.distancesDict["ground"] = points;
                    dGround = d;
                }

            }

            if (isGreen)
            {
                points = GetMinDistForPoints(cable, calcDistancesPoints["green"], query);
                d = (points[0] - points[1]).magnitude;
                if (d < dGreen)
                {
                    cable.distancesDict["green"] = points;
                    dGreen = d;
                }
            }

            if (isCrossedCables)
            {
                points = GetMinDistForPoints(cable, calcDistancesPoints["crossedCables"], query);
                d = (points[0] - points[1]).magnitude;
                if (d < dCrossedCables)
                {
                    cable.distancesDict["crossedCables"] = points;
                    dCrossedCables = d;
                }
            }

            if (isRoads)
            {
                points = GetMinDistForPoints(cable, calcDistancesPoints["roads"], query);
                d = (points[0] - points[1]).magnitude;
                if (d < dRoads)
                {
                    cable.distancesDict["roads"] = points;
                    dRoads = d;
                }
            }

            if (isBuildings)
            {
                points = GetMinDistForPoints(cable, calcDistancesPoints["buildings"], query);
                d = (points[0] - points[1]).magnitude;
                if (d < dBuilds)
                {
                    cable.distancesDict["buildings"] = points;
                    dBuilds = d;
                }
            }
        }
        


        public void LogCablesDistances(Dictionary<string, Vector3[]> distances)
        {
            //if (distances != null)
            //{
            //    prevDistances = distances;
            //}
            //distances = CalculateDistances();
            //distancesLogPart = "Distances\n";
            //foreach (KeyValuePair<string, Vector3[]> entry in distances)
            //{
            //    //firstTowerInMesh
            //    if (prevDistances == null)
            //    {
            //        float x = pointCloudMeshWorkingV2.isReverseSpan ? (float)Math.Round(ControlPanelScriptV2.SpanLength - entry.Value[1].z, 2) : (float)Math.Round(entry.Value[1].z, 2);
            //        string tower = pointCloudMeshWorkingV2.isReverseSpan ? pointCloudMeshWorkingV2.Tower2 : pointCloudMeshWorkingV2.Tower1;

            //        distancesLogPart += string.Format("{0}: {1}, X = {2} м. от опоры {3}.\n Расстояние от оси ВЛ = {4} м.\n",
            //        entry.Key, Math.Round((entry.Value[0] - entry.Value[1]).magnitude, 2), x, tower, Math.Round(ControlPanelScriptV2.PowerLineAxisLine.ProjectionDistanceToLine(entry.Value[1]), 2));
            //    }
            //    else
            //    {
            //        float prevX = pointCloudMeshWorkingV2.isReverseSpan ? (float)Math.Round(ControlPanelScriptV2.SpanLength - prevDistances[entry.Key][1].z, 2) : (float)Math.Round(prevDistances[entry.Key][1].z, 2);
            //        string tower = pointCloudMeshWorkingV2.isReverseSpan ? pointCloudMeshWorkingV2.Tower2 : pointCloudMeshWorkingV2.Tower1;
            //        double prevDist = Math.Round((prevDistances[entry.Key][0] - prevDistances[entry.Key][1]).magnitude, 2);
            //        float x = pointCloudMeshWorkingV2.isReverseSpan ? (float)Math.Round(ControlPanelScriptV2.SpanLength - entry.Value[1].z, 2) : (float)Math.Round(entry.Value[1].z, 2);

            //        distancesLogPart += string.Format("{0}: {1}, X = {2} м. от опоры {3}.\n Расстояние от оси ВЛ = {6} м.\n ({4}, X = {5} м. от опоры {3}.\n Расстояние от оси ВЛ = {7} м.).\n",
            //            entry.Key, prevDist, prevX, FirstTowerInMesh, Math.Round((entry.Value[0] - entry.Value[1]).magnitude, 2), x,
            //            Math.Round(ControlPanelScriptV2.PowerLineAxisLine.ProjectionDistanceToLine(prevDistances[entry.Key][1]), 2),
            //           Math.Round(ControlPanelScriptV2.PowerLineAxisLine.ProjectionDistanceToLine(entry.Value[1]), 2));
            //    }
            //}

            if (distances.ContainsKey("ground"))
                groundLine.UpdateLine(distances["ground"][0], distances["ground"][1], $"ground: {Math.Round((distances["ground"][0]- distances["ground"][1]).magnitude,2)} m.");
            if (distances.ContainsKey("green"))
                greenLine.UpdateLine(distances["green"][0], distances["green"][1], $"green: {Math.Round((distances["green"][0] - distances["green"][1]).magnitude,2)} m.");
            if (distances.ContainsKey("crossedCables"))
                crossedCablesLine.UpdateLine(distances["crossedCables"][0],
                    distances["crossedCables"][1],
                    $"crossed cables: {Math.Round((distances["crossedCables"][0] - distances["crossedCables"][1]).magnitude,2)} m.");
            if (distances.ContainsKey("roads"))
                roadsLine.UpdateLine(distances["roads"][0], distances["roads"][1], $"roads: {Math.Round((distances["roads"][0] - distances["roads"][1]).magnitude,2)} m.");
            if (distances.ContainsKey("buildings"))
                buildsLine.UpdateLine(distances["buildings"][0], distances["buildings"][1], $"builds: {Math.Round((distances["buildings"][0] - distances["buildings"][1]).magnitude,2)} m.");

        }

        public Vector3[] GetMinDistForPoints(Cable cable, KDTree kdTree, KDQuery query)
        {
            List<int> resultIndexes = new List<int>();
            List<float> resultDistances = new List<float>();

            Vector3 curCablePoint = Vector3.zero;
            Vector3 curKDTreePoint = Vector3.zero;
            float curDist = float.MaxValue;

           
            for (int i = 40; i < cable.CalcMode.Points.Count - 40; i+=3) //Костыльно
            {
                resultIndexes.Clear();
                resultDistances.Clear();
               
                query.ClosestPoint(kdTree, cable.CalcMode.Points[i], resultIndexes, resultDistances);
                Vector3 nearestPointToGround = kdTree.Points[resultIndexes[0]];
                float d = resultDistances[0];
                if (d < curDist)
                {
                    curDist = d;
                    curCablePoint = cable.CalcMode.Points[i];
                    curKDTreePoint = nearestPointToGround;
                }

            }
            return new Vector3[2] { curCablePoint, curKDTreePoint };
        }

        private void ProcessCablePointsToCaclDistances()
        {

        }



        //public List<GameObject> FindNearestObjects()
        //{
        //    int layerMask = LayerMask.GetMask("Objects");
        //    Collider[] hitColliders = Physics.OverlapSphere(Position, Vector3.Distance(FirstTower.Position, SecondTower.Position) / 2, layerMask);
        //    var objects = hitColliders.Select(hit => hit.gameObject).ToList();
        //    var road = objects.First(x => x.tag == "Road");
        //    if (road != null)
        //    {
        //        objects = objects.Where(x => x.tag != "Road").ToList();
        //        objects.Add(road.transform.parent.gameObject);
        //    }
        //    return objects;
        //}

        public float GetDistanceToStart(Vector3 pos)
        {
            // Расчет не по расстоянию по оси пролета, а просто по 2д проекции, потом надо доработать
            Vector3 toPos = FirstTower.Position;
            toPos.y = 0;
            pos.y = 0;
            return ((toPos - pos).magnitude);
        }

        public float GetDistanceToEnd(Vector3 pos)
        {
            // Расчет не по расстоянию по оси пролета, а просто по 2д проекции, потом надо доработать
            Vector3 toPos = SecondTower.Position;
            toPos.y = 0;
            pos.y = 0;
            return ((toPos - pos).magnitude);
        }


        public override void CalculateCondition()
        {
            throw new System.NotImplementedException();
        }

        public override Model Create(List<string> args, List<string> typeArgs, InformationHolder infoHolder, bool isEditorMode)
        {
            throw new System.NotImplementedException();
        }

        public override List<string> GetCellsNamesToTable()
        {
            throw new System.NotImplementedException();
        }

        public override List<(string, string)> GetInfo()
        {
            return new List<(string, string)>()
           {
               ("Span length", Mathf.Round(Length).ToString())
           };
        }

        public override List<string> GetInfoForTable()
        {
            throw new System.NotImplementedException();
        }
       

#if UNITY_EDITOR
        public override GameObject Instantiate()
        {
            throw new System.NotImplementedException();
        }

        public override GameObject Instantiate2D()
        {
            throw new System.NotImplementedException();
        }

        public override void SetObjectOnSceneParams()
        {
            return;
        }


#endif
    }
}
