using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeparatedTreeCloud : ITreeCloud
{
    //public Transform Area0_1;
    public Transform Area1_3;
    public Transform Area3_4;
    public Transform Area4_6;
    public Transform Area6Plus;
    public Transform UnsafeTrees;

    public override void ChangeAreaVisibility(AreaRange range, bool visibility)
    {
        switch (range)
        {
            //case AreaRange.Other:
            //    Area0_1.gameObject.SetActive(visibility);
            //    break;
            case AreaRange.Area1_3:
                Area1_3.gameObject.SetActive(visibility);
                break;
            case AreaRange.Area3_4:
                Area3_4.gameObject.SetActive(visibility);
                break;
            case AreaRange.Area4_6:
                Area4_6.gameObject.SetActive(visibility);
                break;
            case AreaRange.Area6Plus:
                Area6Plus.gameObject.SetActive(visibility);
                break;
            case AreaRange.UnsafeTree:
                UnsafeTrees.gameObject.SetActive(visibility);
                break;
        }
    }
}
