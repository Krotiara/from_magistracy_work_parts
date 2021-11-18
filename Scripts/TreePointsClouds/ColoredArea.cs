using System;
using System.Collections.Generic;

public enum AreaRange
{
    Other,
    Area1_3,
    Area3_4,
    Area4_6,
    Area6Plus,
    UnsafeTree
}

[Serializable]
public class ColoredArea
{
    public ColoredArea()
    {
        Area = new Dictionary<AreaRange, List<int>>
        {
            { AreaRange.Other, new List<int>()},
            { AreaRange.Area1_3, new List<int>()},
            { AreaRange.Area3_4,new List<int>()},
            { AreaRange.Area4_6, new List<int>() },
            { AreaRange.Area6Plus, new List<int>()},
            { AreaRange.UnsafeTree, new List<int>()}
        };
    }

    public Dictionary<AreaRange, List<int>> Area;

    public List<int> Area6Plus;
    public List<int> Area4_6;
    public List<int> Area3_4;
    public List<int> Area1_3;
    public List<int> Other;
    public List<int> UnsafeTree;

    public void Deserialize()
    {
        Area = new Dictionary<AreaRange, List<int>>
        {
            { AreaRange.Other, Other},
            { AreaRange.Area1_3, Area1_3},
            { AreaRange.Area3_4, Area3_4},
            { AreaRange.Area4_6, Area4_6 },
            { AreaRange.Area6Plus, Area6Plus},
            { AreaRange.UnsafeTree, UnsafeTree}
        };
    }

    public void Serialize()
    {
        Area6Plus = Area[AreaRange.Area6Plus];
        Area4_6 = Area[AreaRange.Area4_6];
        Area3_4 = Area[AreaRange.Area3_4];
        Area1_3 = Area[AreaRange.Area1_3];
        Other = Area[AreaRange.Other];
        UnsafeTree = Area[AreaRange.UnsafeTree];
    }
}
