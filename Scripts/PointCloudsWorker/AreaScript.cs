using DataStructures.ViliWonka.KDTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaScript : MonoBehaviour
{
    public Transform p1;
    public Transform p2;
    public Transform p3;
    public Transform p4;
    public Transform l1;
    public Transform l2;
    public Transform l3;
    public Transform l4;
    public Material areaMaterial;
   

    public float[] X => new float[4] { p1.position.x, p2.position.x, p3.position.x, p4.position.x };
    public float[] Z => new float[4] { p1.position.z, p2.position.z, p3.position.z, p4.position.z };
    public List<Transform> Points => new List<Transform>() { p1, p2, p3, p4 };
    public Vector3 p12 => p2.position - p1.position;
    public Vector3 p23 => p3.position - p2.position;
    public Vector3 p34 => p4.position - p3.position;
    public Vector3 p41 => p1.position - p4.position;
    public Transform yFilterPoint;
    public string AreaType { get; set; }
    public float Square => GetSquare();
    public List<Vector3> PointsInArea { get; set; } = new List<Vector3>();
    public KDTree PointsKdTree => new KDTree(PointsInArea.ToArray());
    public bool IsSummed { get; set; }
    public int Number { get; set; }
    public float StartSquare;



    //private void Awake()
    //{
    //    foreach(Transform t in transform)
    //    {
    //        var outline = t.gameObject.AddComponent<Outline>();
    //        outline.OutlineMode = Outline.Mode.OutlineAll;
    //        outline.OutlineColor = Color.yellow;
    //        outline.OutlineWidth = 5f;
    //        outline.enabled = false;
    //        t.gameObject.GetComponent<Renderer>().material = Instantiate(areaMaterial);
           
    //    }
        
    //}

    public void SetColor(Color color)
    {
        color = new Color(color.r, color.g, color.b, 120f/255f);
        foreach (Transform t in transform)
        {
            t.gameObject.GetComponent<Renderer>().material.color = color;
        }
    }

    public void SetOutlineActive(bool activeFlag)
    {
        foreach (Transform t in transform)
        {
            t.gameObject.GetComponent<Outline>().enabled = activeFlag;
            
        }
    }

   

    private void Start()
    {
        //yFilterPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<DragObject>().transform;
        //var outline = yFilterPoint.gameObject.AddComponent<Outline>();
        //outline.OutlineMode = Outline.Mode.OutlineAll;
        //outline.OutlineColor = Color.yellow;
        //outline.OutlineWidth = 5f;
        //outline.enabled = false;
    }

    //// Update is called once per frame
    //void Update()
    //{
    //    l1.position = (p1.position + p2.position) / 2;
    //    l1.rotation = Quaternion.LookRotation(p12);
    //    l1.localScale = new Vector3(0.1f, 5, p12.magnitude);
    //    l2.position = (p2.position + p3.position) / 2;
    //    l2.rotation = Quaternion.LookRotation(p23);
    //    l2.localScale = new Vector3(0.1f, 5, p23.magnitude);
    //    l3.position = (p3.position + p4.position) / 2;
    //    l3.rotation = Quaternion.LookRotation(p34);
    //    l3.localScale = new Vector3(0.1f, 5, p34.magnitude);
    //    l4.position = (p4.position + p1.position) / 2;
    //    l4.rotation = Quaternion.LookRotation(p41);
    //    l4.localScale = new Vector3(0.1f, 5, p41.magnitude);
    //}

    /// <summary>
    /// Возвращаемое значение в гектарах
    /// </summary>
    /// <returns></returns>
    public float GetSquare()
    {
        //https://www.mathopenref.com/coordpolygonarea2.html
        float res = 0;
        int j = 3;
        for(int i = 0; i < 4;i++)
        {
            res += (X[j] + X[i]) * (Z[j] - Z[i]);
            j = i;  //j is previous vertex to i
        }
        return (res / 2) / 10000;

    }

    public string GetAreaToSave()
    {
        return string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13}",
            p1.position.x, p1.position.y, p1.position.z,
            p2.position.x, p2.position.y, p2.position.z,
            p3.position.x, p3.position.y, p3.position.z,
            p4.position.x, p4.position.y, p4.position.z,
            AreaType, Number);
    }

    public bool IsPointInPolygon(Vector3 point)
    {
        bool isInside = false;
        for (int i = 0, j = 3; i < 4; j = i++)
        {
            if (((Z[i] > point.z) != (Z[j] > point.z)) &&
            (point.x < (X[j] - X[i]) * (point.z - Z[i]) / (Z[j] - Z[i]) + X[i]) && point.y <= yFilterPoint.transform.position.y)
            {
                isInside = !isInside;
            }
        }
        return isInside;
    }

    public override bool Equals(object obj)
    {
        var script = obj as AreaScript;
        return script != null &&
               base.Equals(obj) &&
               Number == script.Number;
    }

    public override int GetHashCode()
    {
        var hashCode = -2028225194;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + Number.GetHashCode();
        return hashCode;
    }
}
