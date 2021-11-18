using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TreeCloud : ITreeCloud 
{
    Mesh mesh;
    Color[] colors;
    public ColoredArea ColoredArea = new ColoredArea();
    public Color InactiveColor = Color.gray; // сейчас шейдер не понимает прозрачность. Пока используется перекраска в серый
    public Vector3[] Vertices { get; private set; }

    Dictionary<AreaRange, Color> areaColors = new Dictionary<AreaRange, Color>
    {
        { AreaRange.Other, Color.gray},
        { AreaRange.Area1_3, Color.green},
        { AreaRange.Area3_4,new Color(128f / 255, 0f / 255, 255f / 255)},
        { AreaRange.Area4_6, new Color(85f / 255, 170f / 255, 255f / 255) },
        { AreaRange.Area6Plus, new Color(255f / 255, 165f / 255, 0)},
        { AreaRange.UnsafeTree, Color.red}
    };

    //Color color6Plus = new Color(255f / 255, 165f / 255, 0);
    //Color color4_6 = new Color(85f / 255, 170f / 255, 255f / 255);
    //Color color3_4 = new Color(128f / 255, 0f / 255, 255f / 255);
    //Color color1_3 = Color.green;
    //Color color0_1 = Color.white;
    //Color unsafeTreeColor = Color.red;

    //private Color cableColor = new Color(1, 0.502f, 0);
    //private Color otherColor = new Color(0.043f, 0.855f, 0.318f);
    //private Color groundColor = new Color(0.353f, 0, 0.616f);

    public override void ChangeAreaVisibility(AreaRange range, bool visibility)
    {
        var color = visibility ? areaColors[range] : InactiveColor;
        AcceptColor(ColoredArea.Area[range], color);
    }

    public void Awake()
    {
       // SetColoredArea();
    }

    public void SetColoredArea()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        Vertices = mesh.vertices;
        colors = mesh.colors;

        string pathToColoredArea = Application.dataPath + "/StreamingAssets/TreePointClouds/"
            + name + ".json";
        if (File.Exists(pathToColoredArea))
            ColoredArea = LoadColoredArea(pathToColoredArea);
        else
        {
            ColoredArea = GetColoredArea();
            SavedColoredArea(pathToColoredArea);
        }
    }

    private ColoredArea LoadColoredArea(string path)
    {
        var json = CableWalker.Simulator.Networking.Network.LoadData(path);
        var area = JsonUtility.FromJson<ColoredArea>(json);
        area.Deserialize();
        return area;
    }

    private ColoredArea GetColoredArea()
    {
        ColoredArea = new ColoredArea();
        for (int i = 0; i < colors.Length; i++)
        {
            AreaRange areaRange = GetMarkByColor(colors[i]);
            ColoredArea.Area[areaRange].Add(i);
        }
        return ColoredArea;
    }

    private void SavedColoredArea(string path)
    {
        ColoredArea.Serialize();
        CableWalker.Simulator.Networking.Network.SaveData(JsonUtility.ToJson(ColoredArea), path);
    }

    private AreaRange GetMarkByColor(Color color)
    {
        AreaRange range = AreaRange.Other;
        if (areaColors.Values.Contains<Color>(color))
            range = areaColors.First(x => x.Value == color).Key;
        return range;
    }

    private bool ColorsEquals(Color c1, Color c2)
    {
        return Mathf.Abs(c1.r - c2.r) < 0.1 && Mathf.Abs(c1.g - c2.g) < 0.1 && Mathf.Abs(c1.b - c2.b) < 0.1;
    }

    private void AcceptColor(List<int> indexes, Color color)
    {
        var colors = mesh.colors;
        foreach (int index in indexes)
            colors[index] = color;
        mesh.colors = colors;
    }
}
