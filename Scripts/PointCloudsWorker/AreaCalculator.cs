using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AreasCalculatorType
{
    Gost,
    NotGost,
    Object
}

public class AreaCalculator
{
    public Dictionary<string, float> areasSquares;
    public List<AreaScript> Areas { get; private set; }
    public Dictionary<string, Transform> Lines { get; set; }

    public AreaCalculator(Dictionary<string, float> areasDict)
    {
        areasSquares = areasDict;
        Areas = new List<AreaScript>();
    }

    public void AddSquare(string squareType, float value)
    {
        try
        {
            areasSquares[squareType] += value;
        }
        catch (KeyNotFoundException e)
        {
            Debug.Log(string.Format("No key {0} in areas calculator", squareType));
            return;
        }
    }

    public void AddArea(AreaScript area)
    {
        Areas.Add(area);
    }

    public void AddArea(AreaScript area, string squareType, float value)
    {
        try
        {
            areasSquares[squareType] += value;
            Areas.Add(area);
        }
        catch(KeyNotFoundException e)
        {
            Debug.Log(string.Format("No key {0} in areas calculator", squareType));
            return;
        }
        
    }

    public void RemoveArea(AreaScript area)
    {
        if (area.IsSummed)
            areasSquares[area.AreaType] -= area.Square;
        Areas.Remove(area);
        GameObject.Destroy(area.gameObject);
    }

    public void Clear()
    {
        foreach (AreaScript area in Areas.ToList())
        {
            RemoveArea(area);
        }

        foreach (string key in areasSquares.Keys.ToList())
            areasSquares[key] = 0;
    }
}

