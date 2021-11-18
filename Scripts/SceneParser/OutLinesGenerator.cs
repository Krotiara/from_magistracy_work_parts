using CableWalker.Simulator;
using CableWalker.Simulator.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutLinesGenerator 
{
    private static readonly string towerTag = "Tower";
    private static readonly string stringTag = "String";
    private static readonly string cableTag = "Cable";

    public static void GenerateOutLines(InformationHolder infoHolder)
    {
        var objects = infoHolder.GetAll();
        foreach(Dictionary<string,Model> objs in objects.Values)
            foreach(var model in objs.Values)
            {
                try
                {
                    // У дефектов траблы с ObjectOnScene
                    if (model.ObjectOnScene != null)
                    {
                        var outLine = model.ObjectOnScene.AddComponent<Outline>();
                        if (model.ObjectOnScene.tag == towerTag)
                            outLine.IgnoreTagsInChildren = new string[] { stringTag };
                        if(model.ObjectOnScene.tag == cableTag)
                        {
                            var split = model.ObjectOnScene.name.Split('.')[0].Split('-');
                            if (split[0] == split[1])
                                continue;
                        }
                        outLine.OutlineColor = Color.yellow;
                        outLine.OutlineWidth = 10;
                        outLine.enabled = false;
                        outLine.LoadSmoothNormals();
                        model.ObjectOnScene.AddComponent<OnMouseEnterSpectator>();
                    }
                }
                catch(System.NotImplementedException e)
                {
                    Debug.Log(e);
                    continue;
                }
            }       
    }

    public static void GenerateOutLinesToCables(InformationHolder infoHolder)
    {
        var cables = infoHolder.GetList<Cable>();
        foreach (Cable c in cables)

            try
            {
                // У дефектов траблы с ObjectOnScene
                if (c.ObjectOnScene != null)
                {
                    var split = c.ObjectOnScene.name.Split('.')[0].Split('-');
                    if (split[0] == split[1])
                        continue;
                    var outLine = c.ObjectOnScene.AddComponent<Outline>();
                   
                    outLine.OutlineColor = Color.black;
                    outLine.OutlineWidth = 10;
                    outLine.enabled = true;
                    outLine.LoadSmoothNormals();
                 
                }
            }
            catch (System.NotImplementedException e)
            {
                Debug.Log(e);
                continue;
            }

    }
}
