using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parameter
{
    public float Status { get; private set; } = 4;
    public string Name { get; private set; }
    public string Label { get; private set; }
    public float Weight { get; private set; }
    private Func<(string, float)>  labelAndStatusRecalculator;

    public Parameter(string name, float weight,  Func<(string,float)> labelAndStatusRecalculator)
    {
        Name = name;
        Weight = weight;
        this.labelAndStatusRecalculator = labelAndStatusRecalculator;
    }

    public void RecalculateStatus()
    {
        var entry = labelAndStatusRecalculator();
        Label = entry.Item1;
        Status = entry.Item2;
    }
}
