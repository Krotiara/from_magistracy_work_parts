using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CableWalker.Simulator
{
    public class Graph
    {
        public List<Vertex> V { get; }
        public List<Edge> E { get; }

        public Graph(List<Vector3> verticesPos)
        {
            V = new List<Vertex>();
            foreach (var v in verticesPos)
                V.Add(new Vertex(v));
            V = V.Distinct().ToList();
            E = new List<Edge>();
        }

       
        public void AddEdge(Edge e)
        {
            if (E.Contains(e))
                return;
            E.Add(e);
        }


    }

    public class Vertex: IComparable<Vertex>
    {
        public Vector3 Position { get; }
        public List<Vertex> Neibs { get; set; }
        public Vertex Parent { get; set; }
        public float G { get; set; }
        public float H { get; set; }
        public float F { get { return G + H; } }

        public Vertex(Vector3 position)
        {
            this.Position = position;
            Neibs = new List<Vertex>();
        }

        public void AddNeib(Vertex neib)
        {
            Neibs.Add(neib);
        }

        public override bool Equals(object obj)
        {
            var vertex = obj as Vertex;
            return vertex != null &&
                   (Mathf.Abs(Position.x - vertex.Position.x) < 0.1) 
                   && (Mathf.Abs(Position.y - vertex.Position.y) < 0.1) 
                   && (Mathf.Abs(Position.z - vertex.Position.z) < 0.1);
        }

        public override int GetHashCode()
        {
            return 1206833562 + EqualityComparer<Vector3>.Default.GetHashCode(Position);
        }

        public int CompareTo(Vertex obj)
        {
            var compare = F.CompareTo(obj.F);
            if (compare == 0)
                compare = H.CompareTo(obj.H);
            return -compare;
        }
    }

    public class Edge
    {
        public Vertex start;
        public Vertex end;

        public Edge(Vertex start, Vertex end)
        {
            this.start = start;
            this.end = end;
        }

        public override bool Equals(object obj)
        {
            var edge = obj as Edge;
            return edge != null &&
                   start.Equals(edge.start) &&
                   end.Equals(edge.end);
        }

        public override int GetHashCode()
        {
            var hashCode = 1075529825;
            hashCode = hashCode * -1521134295 + EqualityComparer<Vertex>.Default.GetHashCode(start);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vertex>.Default.GetHashCode(end);
            return hashCode;
        }
    }
}
