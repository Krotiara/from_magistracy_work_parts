using System;
using UnityEngine;

namespace CableWalker.Simulator.PathFinding
{
    public class PathNode : IComparable<PathNode>
    {
        public Vector3 Position { get; }
        public PathNode Parent { get; set; }
        public float G { get; set; }
        public float H { get; set; }
        public float F { get { return G + H; } }

        public PathNode(Vector3 position, PathNode parent, float g, float h)
        {
            Position = position;
            Parent = parent;
            G = g;
            H = h;
        }

        public int CompareTo(PathNode obj)
        {
            var compare = F.CompareTo(obj.F);
            if (compare == 0)
                compare = H.CompareTo(obj.H);
            return -compare;
        }

        public override bool Equals(object obj)
        {
            return obj is PathNode another && Position.Equals(another.Position);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}
