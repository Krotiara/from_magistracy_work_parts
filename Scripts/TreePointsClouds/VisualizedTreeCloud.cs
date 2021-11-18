using System.Collections.Generic;
using UnityEngine;

public class VisualizedTreeCloud : ITreeCloud
{
    public Transform Area1_3;
    public Transform Area3_4;
    public Transform Area4_6;
    public Transform Area6Plus;
    public Transform UnsafeTrees;
    public Texture MainTexture;
    public Texture Texture1_3;
    public Texture Texture3_4;
    public Texture Texture4_6;
    public Texture Texture6Plus;
    public Texture TextureUnsafeTree;


    Dictionary<AreaRange, Color> areaColors = new Dictionary<AreaRange, Color>
    {
        { AreaRange.Other, Color.gray},
        { AreaRange.Area1_3, Color.green},
        { AreaRange.Area3_4,new Color(128f / 255, 0f / 255, 255f / 255)},
        { AreaRange.Area4_6, new Color(85f / 255, 170f / 255, 255f / 255) },
        { AreaRange.Area6Plus, new Color(255f / 255, 165f / 255, 0)},
        { AreaRange.UnsafeTree, Color.red}
    };

    void Awake()
    {
        string path = "PointCloud/TreeTextures/";
        MainTexture = Resources.Load<Texture>(path + "main");
        Texture1_3 = Resources.Load<Texture>(path + "green");
        Texture3_4 = Resources.Load<Texture>(path + "violet");
        Texture4_6 = Resources.Load<Texture>(path + "blue");
        Texture6Plus = Resources.Load<Texture>(path + "orange");
        TextureUnsafeTree = Resources.Load<Texture>(path + "red");
    }

    public override void ChangeAreaVisibility(AreaRange range, bool visibility)
    {
        var texture = MainTexture;
        switch (range)
        {
            case AreaRange.Area1_3:
                if (visibility)
                    texture = Texture1_3;
                AcceptTexture(Area1_3, texture);
                break;
            case AreaRange.Area3_4:
                if (visibility)
                    texture = Texture3_4;
                AcceptTexture(Area3_4, texture);
                break;
            case AreaRange.Area4_6:
                if (visibility)
                    texture = Texture4_6;
                AcceptTexture(Area4_6, texture);
                break;
            case AreaRange.Area6Plus:
                if (visibility)
                    texture = Texture6Plus;
                AcceptTexture(Area6Plus, texture);
                break;
            case AreaRange.UnsafeTree:
                if (visibility)
                    texture = TextureUnsafeTree;
                AcceptTexture(UnsafeTrees, texture);
                break;
        }
    }

    private void AcceptTexture(Transform areaFolder,Texture texture)
    {
        foreach (Transform tree in areaFolder)
        {
            tree.GetComponent<MeshRenderer>().material.mainTexture = texture;
        }
    }
}

