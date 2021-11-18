using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CableWalker.Simulator.PathFinding
{
    public class WayPoint
    {
        public Vector3 Position { get; private set; }
        public List<WayPoint> NextWayPoints { get; set; }
        public List<WayPoint> PrevWayPoints { get; set; }
        public string Name { get; private set; }
        public WayPoint Parent { get; set; }
        public bool IsVisited { get; set; }
        public List<WayPoint> Neibs {
            get
            {
                var list = new List<WayPoint>();
                list.AddRange(NextWayPoints);
                list.AddRange(PrevWayPoints);
                return list;
            } }

        public WayPoint(Vector3 position, string name)
        {
            Position = position;
            NextWayPoints = new List<WayPoint>();
            PrevWayPoints = new List<WayPoint>();
            Name = name;
        }

        public override bool Equals(object obj)
        {
            var point = obj as WayPoint;
            return point != null &&
                   Name == point.Name;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
    }
}
