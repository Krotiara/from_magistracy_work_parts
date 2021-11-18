using CableWalker.Simulator;
using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;


public class ThreatingTreesSpanData
{

    public List<Transform> upPoints = new List<Transform>();
    public List<int> ThreatingIndexesInSecuredArea { get; set; } = new List<int>();
    public List<int> ThreatingIndexesOutSecuredArea { get; set; } = new List<int>();
    public bool IsThreatingThrees => ThreatingIndexesInSecuredArea.Count > 0 || ThreatingIndexesOutSecuredArea.Count > 0;
    public int ThreatingThreesCount => ThreatingIndexesInSecuredArea.Count + ThreatingIndexesOutSecuredArea.Count;
    private List<Line> lines = new List<Line>();
    private List<Line> groundLines = new List<Line>();
    //Tuple - координаты, высота, расстояние до крайнего провода, флаг - в охранной зоне
    public List<Tuple<Vector3, float, float, bool, float>> ThreatingThrees { get; set; } = new List<Tuple<Vector3, float, float, bool, float>>();
    public GameObject ThreatingTreePrefab;
    private PointCloudMeshController pointCloud;


    public void AddUpPoint(Vector3 position)
    {
        GameObject g = GameObject.Instantiate(ThreatingTreePrefab); //GameObject.CreatePrimitive(PrimitiveType.Sphere);
                                                                    // g.AddComponent<DragObject>();
        g.transform.position = position;
        upPoints.Add(g.transform);
    }

    public void RemoveLastPoint()
    {
        if (upPoints.Count == 0)
            return;
        Transform g = upPoints[upPoints.Count - 1];
        upPoints.RemoveAt(upPoints.Count - 1);
        GameObject.Destroy(g.gameObject);
    }

    public void ClearLines()
    {
        foreach (Line l in lines)
            l.DeleteLineFromScene();
        lines.Clear();
        foreach (Line l in groundLines)
            l.DeleteLineFromScene();
        groundLines.Clear();
    }

    public void ClearUpPoints()
    {

        foreach (Transform p in upPoints)
        {
            try
            {
                GameObject.Destroy(p.gameObject);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }


        upPoints.Clear();
    }

    private bool IsUpPointInSecuredArea(Vector3 p, float leftSecuredDistanceX, float rightSecuredDistanceX)
    {
        return p.x > leftSecuredDistanceX && p.x < rightSecuredDistanceX;
    }

    public void LoadData(SpanData data, Transform pointCloud)
    {
        foreach (string tPointsData in data.ThreatingPointsData)
        {
            float[] split = tPointsData.Replace(',', '.').Split(';').Select(x => float.Parse(x)).ToArray();
            AddUpPoint(pointCloud.TransformPoint(new Vector3(split[0], split[1], split[2])));
        }
    }

    //public void CalculateThreatingThrees(KDTree grounds, KDTree cables, float radius, Material lineMaterial, float leftSecuredDistanceX, float rightSecuredDistanceX, Line spanLine)
    public void CalculateThreatingThrees(KDTree grounds, KDTree cables, Material lineMaterial, Line spanLine, GameObject linePrefab)
    {
        ClearLines();
        //ThreatingIndexes.Clear();
        ThreatingThrees.Clear();
        ThreatingIndexesInSecuredArea.Clear();
        ThreatingIndexesOutSecuredArea.Clear();

        KDQuery query = new KDQuery();
        List<int> resultIndexes = new List<int>();
        List<float> resultDistances = new List<float>();
        Vector3 nearestCablePoint;
        Vector3 nearestGroundPoint;
        float h;
        float r;
        for (int i = 0; i < upPoints.Count; i++)
        {

            Transform point = upPoints[i];
            resultIndexes.Clear();
            resultDistances.Clear();

            query.ClosestPoint(cables, point.position, resultIndexes, resultDistances);
            nearestCablePoint = cables.Points[resultIndexes[0]];
            Vector3 rVector = (new Vector3(point.position.x, nearestCablePoint.y, point.position.z) - nearestCablePoint);
            //r = rVector.magnitude - radius;
            r = rVector.magnitude;
            nearestGroundPoint = GetVerticalNearestGroundPoint(grounds, point.position);
            h = point.position.y - nearestGroundPoint.y;
            bool isThreating = r < h;

            nearestGroundPoint = new Vector3(point.position.x, nearestGroundPoint.y, point.position.z); //Костыль

            float dToAxis = spanLine.ProjectionDistanceToLine(point.position);

            if (isThreating)
            {
                //bool isInSecuredArea = IsUpPointInSecuredArea(point.position, leftSecuredDistanceX, rightSecuredDistanceX);
                bool isInSecuredArea = false;
                ThreatingThrees.Add(new Tuple<Vector3, float, float, bool, float>(upPoints[i].position, h, rVector.magnitude, isInSecuredArea, dToAxis));

                if (isInSecuredArea)
                    ThreatingIndexesInSecuredArea.Add(i);
                else
                    ThreatingIndexesOutSecuredArea.Add(i);
                point.Find("Number").gameObject.SetActive(true);
                point.Find("Number").Find("Text").GetComponent<TextMeshProUGUI>().text = string.Format("TT # {0}", ThreatingThrees.Count);                //ThreatingIndexes.Add(i);
            }

            lines.Add(CreateCableLine(isThreating, lineMaterial, nearestCablePoint, point.position, linePrefab));
            groundLines.Add(CreateGroundLine(isThreating, lineMaterial, nearestGroundPoint, point.position, linePrefab));

        }
    }

    public List<string[]> GetThreatingTreesInfo(CableWalker.Simulator.Model.Span span)
    {
        var result = new List<string[]>();
        result.Add(new string[] { "index", "Height", "X", "D to axis", "D to cable" });
        for (int i = 0; i < ThreatingThrees.Count; i++)
        {
            float x = span.PointCloudMeshController.IsReverseSpan() ?
                (float)span.Length - span.PointCloudMeshController.GetLocalPos(ThreatingThrees[i].Item1).z :
                span.PointCloudMeshController.GetLocalPos(ThreatingThrees[i].Item1).z;
            result.Add(new string[] { (i+1).ToString(),
                Math.Round(ThreatingThrees[i].Item2,2).ToString(),
                Math.Round(x,2).ToString(),
                Math.Round(ThreatingThrees[i].Item5,2).ToString(),
                Math.Round(ThreatingThrees[i].Item3,2).ToString() });
        }
        return result;
    }

    public Vector3 GetVerticalNearestGroundPoint(KDTree grounds, Vector3 upPoint)
    {
        try
        {
            KDQuery query = new KDQuery();
            //KDTree newKDTree = new KDTree(grounds.Points.Where(p => Math.Abs(p.x - upPoint.x) < 1 && Math.Abs(p.z - upPoint.z) < 1).ToArray());
            List<int> resultIndexes = new List<int>();
            List<float> resultDistances = new List<float>();
            query.ClosestPoint(grounds, upPoint, resultIndexes, resultDistances);
            return grounds.Points[resultIndexes[0]];
        }
        catch (Exception e)
        {
            Debug.Log(e); //Костыль
            return upPoint;
        }

    }

    public Line CreateCableLine(bool isThreating, Material lineMaterial, Vector3 nearestCablePoint, Vector3 upPoint, GameObject linePrefab)
    {
        Color color = isThreating ? Color.red : Color.green;
        return new Line(nearestCablePoint, new Vector3(upPoint.x, nearestCablePoint.y, upPoint.z), lineMaterial, color,linePrefab,"");
    }

    public Line CreateGroundLine(bool isThreating, Material lineMaterial, Vector3 nearestGroundPoint, Vector3 upPoint, GameObject linePrefab)
    {
        Color color = isThreating ? Color.red : Color.green;
        return new Line(upPoint, nearestGroundPoint, lineMaterial, color, linePrefab, "");
    }

    public ThreatingTreesSpanData(FileInfo file, Transform pointCloud, GameObject ThreatingTreePrefab)
    {
        SpanData spanData = null;
        this.ThreatingTreePrefab = ThreatingTreePrefab;
        using (StreamReader sr = file.OpenText())
        {
            string lines = sr.ReadToEnd();
            spanData = DataLoader<SpanData>.LoadJson(lines);
        }
        if (spanData != null)
            LoadData(spanData, pointCloud);
    }

    public ThreatingTreesSpanData(SpanData spanData, Transform pointCloud, GameObject ThreatingTreePrefab)
    {
        this.ThreatingTreePrefab = ThreatingTreePrefab;
        if (spanData != null)
            LoadData(spanData, pointCloud);
    }



}

public class ThreatingTreesRecalculatorByPointClouds
{
    public ThreatingTreesRecalculatorByPointClouds()
    {

    }

    public Dictionary<string, ThreatingTreesSpanData> LoadData(List<GameObject> pointClouds, Dictionary<string, SpanData> spansData, GameObject ThreatingTreePrefab)
    {

        Dictionary<string, ThreatingTreesSpanData> areasData = new Dictionary<string, ThreatingTreesSpanData>();
        //DirectoryInfo dinfo = new DirectoryInfo(pointCloudsTxtsPath);
        //FileInfo[] Files = dinfo.GetFiles("*.txt");
        List<string> pointCloudsSpansNames =
            pointClouds.Select(x => string.Format("{0}_{1}_{2}", x.name.Split('_')[0], x.name.Split('_')[1], x.name.Split('_')[2])).ToList();

        foreach(var entry in spansData)
        {
            int index = pointCloudsSpansNames.IndexOf(entry.Key);
            if (index != -1)
                areasData[entry.Key] = new ThreatingTreesSpanData(entry.Value, pointClouds[index].transform, ThreatingTreePrefab);
        }

        //foreach (FileInfo file in Files)
        //{
        //    string[] split = file.Name.Split('_');
        //    string number = string.Format("{0}_{1}_{2}", split[0], split[1], split[2]);
        //    int index = pointCloudsSpansNames.IndexOf(number);
        //    if (index != -1)
        //        areasData[number] = new ThreatingTreesSpanData(file, pointClouds[index].transform, ThreatingTreePrefab);
        //}
        return areasData;
    }
}
