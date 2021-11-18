using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.Simulator.BorderCreator
{
    public class BorderRegion
    {
        
        public Edge LeftEdge { get; private set; }
        public Edge RightEdge { get; private set; }
        public Edge TopEdge { get; private set; }
        public Edge BottomEdge { get; private set; }
        public List<Edge> Edges { get; private set; }
        public string Name { get; private set; }
        public List<BorderPoint> Points
        {
            get
            {
                var result = new List<BorderPoint>();
                result.AddRange(LeftEdge.Points);
                result.AddRange(RightEdge.Points);
                return result;
            }
        }

        public List<BorderRegion> RelativeRegions { get; private set; }
        /// <summary>
        /// Боковые стороны
        /// </summary>
        public List<Edge> Sides { get { return new List<Edge> { LeftEdge, RightEdge }; } }
        /// <summary>
        /// Верхние и нижние стороны
        /// </summary>
        public List<Edge> Covers { get { return new List<Edge> { TopEdge, BottomEdge }; } }

       
        public BorderRegion(Edge leftSide, Edge rightSide, string name)
        {
            LeftEdge = leftSide;
            RightEdge = rightSide;
            BottomEdge = new Edge(RightEdge.End, LeftEdge.Start, SideEnum.BorderSide.Bottom);
            TopEdge = new Edge(LeftEdge.End, RightEdge.Start, SideEnum.BorderSide.Top);
            //BottomEdge.RelativeEdges.Add(LeftEdge);
            //BottomEdge.RelativeEdges.Add(RightEdge);
            //TopEdge.RelativeEdges.Add(LeftEdge);
            //TopEdge.RelativeEdges.Add(RightEdge);
            LeftEdge.RegionTopEdge = TopEdge;
            RightEdge.RegionTopEdge = TopEdge;
            LeftEdge.RegionBottomEdge = BottomEdge;
            RightEdge.RegionBottomEdge = BottomEdge;
            LeftEdge.NextEdgeInRegion = TopEdge;
            TopEdge.NextEdgeInRegion = RightEdge;
            RightEdge.NextEdgeInRegion = BottomEdge;
            BottomEdge.NextEdgeInRegion = LeftEdge;
            Edges = new List<Edge> { LeftEdge, TopEdge, RightEdge, BottomEdge };
            foreach (var edge in Edges)
                edge.Region = this;
            RelativeRegions = new List<BorderRegion>();
            Name = name;
        }

        //public BorderRegion(Edge leftSide, Edge rightSide, List<BorderRegion> relativeRegions)
        //    : this(leftSide, rightSide)
        //{
        //    RelativeRegions.AddRange(relativeRegions);
        //}


        public BorderRegion(Vector3 leftStart, Vector3 leftEnd, Vector3 rightStart, Vector3 rightEnd, string name)
            : this(new Edge(new BorderPoint(leftStart), new BorderPoint(leftEnd), SideEnum.BorderSide.Left),
                 new Edge(new BorderPoint(rightStart), new BorderPoint(rightEnd), SideEnum.BorderSide.Right),name)
        {
            
        }

        //public void SetRelationEdges()
        //{
        //    foreach(var edge in Edges)
        //    {
        //        edge.RelativeEdges.AddRange(Edges.Where(e => !e.Equals(edge)));
        //        foreach (var relRegion in RelativeRegions)
        //            edge.RelativeEdges.AddRange(relRegion.Sides);
        //    }
        //} 

        public void SetRelationRegionsFrom(List<BorderRegion> regions)
        {
            foreach (var region in regions)
            {
                if (this.Equals(region))
                {
                    continue;
                }
                if (this.IsIntersectWith(region) && !RelativeRegions.Contains(region))
                    RelativeRegions.Add(region);
            }
        }

        public bool IsIntersectWith(BorderRegion region)
        {
            foreach (var segment in Edges)
            {
                if (segment.IsIntersectBySegmentWith(region.Edges)) return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BorderRegion);
        }

        public bool Equals(BorderRegion region)
        {
            return Name == region.Name;
        }

        //public void SetBorder(Edge border)
        //{
        //    if (border.SideName == SideEnum.BorderSide.Left)
        //        LeftBorders.Add(border);
        //    else if (border.SideName == SideEnum.BorderSide.Right)
        //        RightBorders.Add(border);
        //}

        //public float GetMaxXFromPoints()
        //{
        //    return Mathf.Max(LeftEdge.Start.X, LeftEdge.End.X, RightEdge.Start.X, RightEdge.End.X);
        //}

        public float GetMaxZFromPoints()
        {
            return Mathf.Max(LeftEdge.Start.Z, LeftEdge.End.Z, RightEdge.Start.Z, RightEdge.End.Z);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
