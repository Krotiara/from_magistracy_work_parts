using System.Collections.Generic;
using System.Linq;
using CableWalker.Simulator.Mission.Parser;
using UnityEngine;

namespace CableWalker.Simulator.Mission
{
    public class MissionVisualizer : MonoBehaviour
    {
        
        public ParserCommandProvider commandProvider;
       // public InteractiveParserCommandProvider interactiveCommandProvider;
        public GameObject interactiveConsole;
        //public bool IsInteractiveMode => interactiveConsole.activeSelf;

        public GameObject WayPointPrefab;
        
        private void Start()
        {
            //if (commandProvider == null || interactiveCommandProvider == null || interactiveConsole == null)
            //{
            //    Debug.LogError("Command provider not specified.");
            //    enabled = false;
            //}
        }

        public void Visualize()
        {
            //var points = new List<Vector3>();

            //var commands = IsInteractiveMode? interactiveCommandProvider.GetCommands(): commandProvider.GetCommands();
            //var timeline = new MissionTimeline(commands);

            //for (var i = 0; i < commands.Count(); i++)
            //    points.AddRange(timeline.GetPositions(i));

            //if (WayPointPrefab != null)
            //{
            //    foreach (var point in points)
            //    {
            //        var obj = Instantiate(WayPointPrefab, transform, true);
            //        obj.transform.position = point;
            //    }
            //}

            //if (points.Count > 1)
            //    PathVisualizer.Instance.Draw(points, Color.green, 0.1f);
        }
    }
}
