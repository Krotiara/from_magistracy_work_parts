using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowersNumberComparer : MonoBehaviour
{
    public class TowersComparer : IComparer<string>
    {
        public int Compare(string Tower1, string Tower2)
        {
            int res = 0;
            if (!Tower2.Contains("A") && !Tower1.Contains("A"))
            {
                res = int.Parse(Tower2) < int.Parse(Tower1) ? 1 : -1;  
            }
            else if (Tower1.Contains("A") && !Tower2.Contains("A"))
                res = int.Parse(Tower1.Remove(Tower1.Length - 1)) >= int.Parse(Tower2) ? 1 : -1;
            else if (Tower2.Contains("A") && !Tower1.Contains("A"))
            {
                res = int.Parse(Tower2.Remove(Tower2.Length - 1)) < int.Parse(Tower1) ? 1 : -1;
            }
            return res;
        }
    }

    //public class SpansComparer : IComparer<string>
    //{
    //    public int Compare(string Tower1, string Tower2)
    //    {
    //        int res = 0;
    //        if (!Tower2.Contains("A") && !Tower1.Contains("A"))
    //        {
    //            res = int.Parse(Tower2) < int.Parse(Tower1) ? 1 : -1;
    //        }
    //        else if (Tower1.Contains("A") && !Tower2.Contains("A"))
    //            res = int.Parse(Tower1.Remove(Tower1.Length - 1)) >= int.Parse(Tower2) ? 1 : -1;
    //        else if (Tower2.Contains("A") && !Tower1.Contains("A"))
    //        {
    //            res = int.Parse(Tower2.Remove(Tower2.Length - 1)) < int.Parse(Tower1) ? 1 : -1;
    //        }
    //        return res;
    //    }
    //}



}
