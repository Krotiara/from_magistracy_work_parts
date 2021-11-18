using System;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator.BorderCreator
{
    public class BorderPoint : IEquatable<BorderPoint>
    {
        public float X { get; private set; }
        public float Z { get; private set; }
        public Vector2 Position => new Vector2(X, Z);
        public Vector3 PositionInWorld => new Vector3(X, HeightInTheWorld, Z);
        public float HeightInTheWorld { get; private set; }
        public bool IsExternal { get; private set; }
        
        //public bool IsFree { get; private set; }

        public BorderPoint(Vector3 point)
        {
            X = point.x;
            HeightInTheWorld = point.y;
            Z = point.z;
            //IsFree = true;
        }

        public BorderPoint(float x, float z)
        {
            X = x;
            Z = z;
        }

        public void SetIsExternal(bool param)
        {
            IsExternal = param;
        }

        //public void SetIsFree(bool param)
        //{
        //    IsFree = param;
        //}

        public void SetPosition(Vector2 newPosition)
        {
            X = newPosition.x;
            Z = newPosition.y;
        }

        public Edge GetBorderSegmentByPoint(List<Edge> segments)
        {
            foreach (var segment in segments)
                if (Equals(segment.Start) || Equals(segment.End))
                    return segment;
            throw new Exception("There is no segment in the list with such a start or end");
        }


        public bool IsPointInternalInRegion(BorderRegion region)
        {
            foreach (var segment in region.Edges)
            {
                //Если точка принадлежит стороне региона, то она уже внешняя
                if (IsBelongsTo(segment))
                    return false;
            }
            //var rayVector = new Vector3(region.GetMaxXFromPoints() + 5, HeightInTheWorld, Z);
            var rayVector = new Vector3(X, HeightInTheWorld, region.GetMaxZFromPoints() + 5);
            var ray = new Edge(this, new BorderPoint(rayVector), SideEnum.BorderSide.Ray);
            var n = ray.GetCountIntersectionWith(region);
            return (n % 2 == 0) ? false : true;
        }

        public bool IsVertexIn(BorderRegion region)
        {
            foreach (var point in region.Points)
                if (Equals(point))
                    return true;
            return false;
        }

        public bool IsVertexIn(Edge edge)
        {
            return edge.Start.Equals(this) || edge.End.Equals(this);
        }

        public List<BorderPoint> GetNeighboringPointsIn(BorderRegion region)
        {
            var result = new List<BorderPoint>();
            foreach (var segment in region.Edges)
            {
                if (Equals(segment.Start))
                    result.Add(segment.End);
                if (Equals(segment.End))
                    result.Add(segment.Start);
            }
            return result;
        }

        public bool IsPointInternalInRegions(List<BorderRegion> regions)
        {
            foreach (var region in regions)
                if (IsPointInternalInRegion(region))
                    return true;
            return false;
        }


        /// <summary>
        /// Проверка принадлежности точки прямой
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public bool IsBelongsTo(Edge segment)
        {
            var epsilon = 0.15f;
            var sign = Vector2.Dot(segment.Start.Position - Position, segment.End.Position - Position);
            var distance = Math.Abs(segment.A * X + segment.B * Z + segment.C) / Math.Sqrt(segment.A * segment.A + segment.B * segment.B);
            //return ((segment.A * X + segment.B * Z + segment.C) <= epsilon && sign <= 0 && (segment.A * X + segment.B * Z + segment.C) >= 0);
            return (distance <= epsilon && sign <= 0);
        }

        public bool IsBelongsOneOf(List<Edge> segments)
        {
            foreach (var segment in segments)
                if (IsBelongsTo(segment)) return true;
            return false;
        }

        public bool IsBelongsOneOfCoversIn(List<BorderRegion> regions)
        {
            foreach (var region in regions)
                if (IsBelongsOneOf(region.Covers)) return true;
            return false;
        }

        public BorderPoint GetNearestPoint(List<BorderPoint> points)
        {
            var currentIndex = 0;
            var currentDistance = Vector2.Distance(Position, points[0].Position);
            for (var i = 1; i < points.Count; i++)
            {
                var distance = Vector2.Distance(Position, points[i].Position);
                if (distance < currentDistance)
                {
                    currentIndex = i;
                    currentDistance = distance;
                }
            }
            return points[currentIndex];
        }

        public bool IsOnLeftOf(BorderPoint start, BorderPoint end)
        {
            return ((X - start.X) * (end.Z - start.Z) 
                - (Z - start.Z) * (end.X - start.X)) < 0;
        }

        /// <summary>
        /// Проверяет принадлежность точки  лучу edge.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool OnRay(Edge edge)
        {
            //https://habr.com/ru/post/148325/
            var vector1 = edge.End.Position - edge.Start.Position;
            var vector2 = Position - edge.Start.Position;
            return this.Equals(edge.Start) || this.Equals(edge.End) || ScalarProduct(vector1, vector2) >= 0;
        }

        /// <summary>
        /// Проверяет принадлежность точки  отрезку edge.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool OnSegment(Edge edge)
        {
            //https://habr.com/ru/post/148325/
            var vector1 = edge.Start.Position - Position;
            var vector2 = edge.End.Position - Position;
            return this.Equals(edge.Start) || this.Equals(edge.End) || ScalarProduct(vector1, vector2) <= 0;
        }

        public float GetDistanceTo(Edge edge)
        {
            var numenator = Math.Abs(edge.A * X + edge.B * Z + edge.C);
            var denominator = Math.Sqrt(edge.A * edge.A + edge.B * edge.B);
            return (float)(numenator / denominator);
        }

        public static float ScalarProduct(Vector2 a, Vector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public  bool Equals(BorderPoint other)
        {
            //Check whether the compared object is null.  
            if (System.Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.  
            if (System.Object.ReferenceEquals(this, other)) return true;

            return Mathf.Abs(this.X - other.X) < 0.01 && Mathf.Abs(this.Z - other.Z) < 0.01;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Z.GetHashCode();
        }
    }
}
