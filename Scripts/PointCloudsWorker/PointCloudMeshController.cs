using DataStructures.ViliWonka.KDTree;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class PointCloudMeshController : MonoBehaviour
{

    public Mesh mesh { get; set; }
    private Vector3[] vertices;
    private Color[] colors;

    private Color groundColor = new Color(255f / 255, 200f / 255, 200f / 255);
    private Color notColored = new Color(255f / 255, 255f / 255, 255f / 255);
    private Color powerLineColor = new Color(0f / 255, 0f / 255, 0f / 255);
    private Color crossedCablesColor = new Color(255f / 255, 255f / 255, 100f / 255);
    private Color area13Color = new Color(0f / 255, 200f / 255, 0f / 255);
    private Color area34Color = new Color(204f / 255, 0f / 255, 204f / 255);
    private Color area46Color = new Color(0f / 255, 0f / 255, 204f / 255);
    private Color area6plusColor = new Color(204f / 255, 102f / 255, 0f / 255);

    private List<int> groundIndexes = new List<int>();
    private List<int> notColoredIndexes = new List<int>();
    private List<int> prevNotColoredIndexes = new List<int>();
    private List<int> cablesIndexes = new List<int>();
    private List<int> crossedCablesIndexes = new List<int>();
    private List<int> noise = new List<int>();
    private List<int> roads = new List<int>();
    private List<int> builds = new List<int>();

    private List<int> points13Area = new List<int>();
    private List<int> points34Area = new List<int>();
    private List<int> points46Area = new List<int>();
    private List<int> points6plusArea = new List<int>();

    public bool IsFiltered { get; set; }

   // public KDTree GroundKDTree => GetKDTreeByIndexes(groundIndexes.ToArray()).Item1;

    public Dictionary<string, KDTree> CategoriesKDTrees { get; set; }

    public string SpanNumber { get
        {
            string[] nameSplit = gameObject.name.Split('_');
            string tower1 = nameSplit[0];
            string tower2 = nameSplit[1];
            string span = IsReverseSpan(tower1, tower2) ? string.Format("{0}-{1}", tower2, tower1).ToLower() :
                string.Format("{0}-{1}", tower1, tower2).ToLower();
            return span;
        } }

    public List<int> GreenIndexes
    {
        get
        {
            List<int> green = new List<int>();
            green.AddRange(points13Area);
            green.AddRange(points34Area);
            green.AddRange(points46Area);
            green.AddRange(points6plusArea);
            return green;
        }
    }

    public void LoadCategoriesKDTrees(SpanData spanData)
    {
        Instantiate();
        CategoriesKDTrees.Clear();
        if (spanData.GroundPoints.Count > 0)
            CategoriesKDTrees["ground"] = new KDTree(spanData.GroundPoints.Select(i => transform.TransformPoint(vertices[i])).ToArray());
        if (spanData.GreenPoints.Count > 0)
            CategoriesKDTrees["green"] = new KDTree(spanData.GreenPoints.Select(i => transform.TransformPoint(vertices[i])).ToArray());
        if (spanData.CrossedCablesPoints.Count > 0)
            CategoriesKDTrees["crossedCables"] = new KDTree(spanData.CrossedCablesPoints.Select(i => transform.TransformPoint(vertices[i])).ToArray());
        if (spanData.RoadsPoints.Count > 0)
            CategoriesKDTrees["roads"] = new KDTree(spanData.RoadsPoints.Select(i => transform.TransformPoint(vertices[i])).ToArray());
        if (spanData.BuildingsPoints.Count > 0)
            CategoriesKDTrees["buildings"] = new KDTree(spanData.BuildingsPoints.Select(i => transform.TransformPoint(vertices[i])).ToArray());
    }

    public (KDTree, List<int>) GetKDTreeByIndexes(int[] indexes)
    {
        //    private KDTree kDTree;
        //KDQuery query;
        var points = new List<Vector3>();
        var indexesConformity = new List<int>();
        foreach (int i in indexes)
        {
            points.Add(transform.TransformPoint(vertices[i]));
            indexesConformity.Add(i);
        }
        return (new KDTree(points.ToArray()), indexesConformity);
    }

    public void Instantiate()
    {
        if(mesh == null)
        {
            mesh = GetComponent<MeshFilter>().sharedMesh;
            vertices = mesh.vertices;
            colors = mesh.colors;
            CategoriesKDTrees = new Dictionary<string, KDTree>();
        }
       
    }

    public void SetCablesAndNoiseColorsTransparent()
    {
        //TODO
    }

    //public void FilterPointCloud()
    //{
        
    //    if (IsFiltered)
    //        return;
    //    Stopwatch stopwatch = new Stopwatch();
    //    stopwatch.Start();
    //    Instantiate();

    //    if (colors.Length == 0) return;
    //    Color verticeColor;
    //    points13Area.Clear();
    //    points34Area.Clear();
    //    points46Area.Clear();
    //    points6plusArea.Clear();
    //    for (int i = 0; i < colors.Length; i++)
    //    { 
    //        verticeColor = colors[i];

    //        if (ColorsEquals(colors[i], powerLineColor))
    //            cablesIndexes.Add(i);
    //        else if (ColorsEquals(colors[i], groundColor))
    //            groundIndexes.Add(i);
    //        else if (ColorsEquals(colors[i], area13Color))
    //            points13Area.Add(i);
    //        else if (ColorsEquals(colors[i], area34Color))
    //            points34Area.Add(i);
    //        else if (ColorsEquals(colors[i], area46Color))
    //            points46Area.Add(i);
    //        else if (ColorsEquals(colors[i], area6plusColor))
    //            points6plusArea.Add(i);
    //        else if (ColorsEquals(colors[i], crossedCablesColor))
    //            crossedCablesIndexes.Add(i);
    //    }
    //    IsFiltered = true;
    //    stopwatch.Stop();
    //    UnityEngine.Debug.Log("Потрачено секунд на фильтрацию точек облака: " + stopwatch.ElapsedMilliseconds * 0.001);
    //}

    bool ColorsEquals(Color c1, Color c2)
    {
        return Mathf.Abs(c1.r - c2.r) < 0.1 && Mathf.Abs(c1.g - c2.g) < 0.1 && Mathf.Abs(c1.b - c2.b) < 0.1;
    }


    private bool IsReverseSpan(string Tower1, string Tower2)
    {

        if (!Tower2.Contains("A") && !Tower1.Contains("A"))
        {
            return int.Parse(Tower2) < int.Parse(Tower1);
        }
        else if (Tower1.Contains("A") && !Tower2.Contains("A"))
            return int.Parse(Tower1.Remove(Tower1.Length - 1)) >= int.Parse(Tower2);
        else if (Tower2.Contains("A") && !Tower1.Contains("A"))
        {
            return int.Parse(Tower2.Remove(Tower2.Length - 1)) < int.Parse(Tower1);
        }
        else return false;

    }

    public bool IsReverseSpan()
    {
        string[] nameSplit = gameObject.name.Split('_');
        string Tower1 = nameSplit[0];
        string Tower2 = nameSplit[1];
        if (!Tower2.Contains("A") && !Tower1.Contains("A"))
        {
            return int.Parse(Tower2) < int.Parse(Tower1);
        }
        else if (Tower1.Contains("A") && !Tower2.Contains("A"))
            return int.Parse(Tower1.Remove(Tower1.Length - 1)) >= int.Parse(Tower2);
        else if (Tower2.Contains("A") && !Tower1.Contains("A"))
        {
            return int.Parse(Tower2.Remove(Tower2.Length - 1)) < int.Parse(Tower1);
        }
        else return false;

    }

    

    public Vector3 GetLocalPos(Vector3 globalPos)
    {
        return transform.InverseTransformPoint(globalPos);
    }

    //public Dictionary<string, KDTree> GetKDTreesToCableDistCalc()
    //{
    //    Stopwatch stopwatch = new Stopwatch();
    //    stopwatch.Start();
    //    var result = new Dictionary<string, KDTree>();
    //    if (groundIndexes.Count > 0)
    //        result["ground"] = GetKDTreeByIndexes(FilterByCenter(groundIndexes, -12, 12)).Item1;
    //    if (GreenIndexes.Count > 0)
    //        result["green"] = GetKDTreeByIndexes(FilterByCenter(GreenIndexes, -12, 12)).Item1;
    //    if (crossedCablesIndexes.Count > 0)
    //        result["crossedCables"] = GetKDTreeByIndexes(crossedCablesIndexes.ToArray()).Item1;
    //    if (roads.Count > 0)
    //        result["roads"] = GetKDTreeByIndexes(roads.ToArray()).Item1;
    //    stopwatch.Stop();
    //    UnityEngine.Debug.Log("Потрачено секунд на создание кд деревьев: " + stopwatch.ElapsedMilliseconds * 0.001);
    //    return result;


    //}

    //public int[] FilterByCenter(List<int> indexes, float xLeft, float xRight)
    //{
    //    return indexes.Where(index => vertices[index].x >= xLeft && vertices[index].x <= xRight).ToArray();
    //}

}
