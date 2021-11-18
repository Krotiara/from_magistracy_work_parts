using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Line
{
    public Vector3 Start { get; private set; }
    public Vector3 End { get; private set; }
    public float Distanсe => (End - Start).magnitude;
    public float ProjectionDistance => (new Vector3(End.x, 0, End.z) - new Vector3(Start.x, 0, Start.z)).magnitude;
    public Vector3 ProjectionDirection => (new Vector3(End.x, 0, End.z) - new Vector3(Start.x, 0, Start.z));
    public Vector3 Direction => (End - Start);
    public LineRenderer lineRenderer;
    private Material material;
    private Transform label;
    private CapsuleCollider capsuleCollider;

    public Line(Vector3 start, Vector3 end, Material mat, Color color, GameObject prefab, string text)
    {
        Start = start;
        End = end;
        material = mat;
        InstantiateLine(color, prefab, text, (start+end)/2, end-start);
    }

    public void InstantiateLine(Color color, GameObject prefab, string text, Vector3 pos, Vector3 dir)
    {
        if (lineRenderer != null)
            GameObject.DestroyImmediate(lineRenderer.gameObject);

        lineRenderer = GameObject.Instantiate(prefab).GetComponent<LineRenderer>();
        capsuleCollider = lineRenderer.transform.GetComponent<CapsuleCollider>();
        lineRenderer.gameObject.transform.position = pos;
        //lineRenderer.gameObject.transform.rotation = Quaternion.LookRotation(dir); //Проблема с поворотом
        //capsuleCollider.height = Distanсe;
        //capsuleCollider.radius = 0.06f;
        if (text != "")
        {
            label = lineRenderer.transform.Find("Label");
            label.gameObject.SetActive(true);
            label.GetComponentInChildren<TextMeshProUGUI>().text = text;
        }
        else
        {
            label = lineRenderer.transform.Find("Label");
            label.gameObject.SetActive(false);
        }

        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.SetPositions(new Vector3[] { Start, End });
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.material = material;

    }

    public void UpdateLine(Vector3 newStart, Vector3 newEnd, string text)
    {
        Start = newStart;
        End = newEnd;
        if (lineRenderer != null)
        {
            lineRenderer.SetPositions(new Vector3[] { newStart, newEnd });
            label.GetComponentInChildren<TextMeshProUGUI>().text = text;
            lineRenderer.gameObject.transform.position = (newStart + newEnd) / 2;
            //lineRenderer.gameObject.transform.rotation = Quaternion.LookRotation(newEnd- newStart);
            //capsuleCollider.height = Distanсe;
            //capsuleCollider.radius = 0.06f;
        }

    }

    public void DeleteLineFromScene()
    {
        if (lineRenderer != null)
            GameObject.DestroyImmediate(lineRenderer.gameObject);
    }

    public  float ProjectionDistanceToLine(Vector3 point)
    {
        return Vector3.Cross(ProjectionDirection.normalized, (new Vector3(point.x,0,point.z) - new Vector3(Start.x,0,Start.z))).magnitude;
    }
}

