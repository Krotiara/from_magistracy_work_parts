using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.Simulator.BorderCreator
{

    public class Edge : IEquatable<Edge>
    {
        public BorderPoint Start { get; private set; }
        public BorderPoint End { get; private set; }
        public SideEnum.BorderSide SideName { get; private set; }
        public List<BorderPoint> Points => new List<BorderPoint> { Start, End };
        public Vector2 MiddleVector => (Start.Position + End.Position) / 2;
        public Vector3 MiddlePoint => (Start.PositionInWorld + End.PositionInWorld) / 2;
        public Vector2 Vector => (End.Position - Start.Position);
        public float Length => Vector2.Distance(Start.Position, End.Position);
        public bool IsConnect { get; private set; }

        public double A { get; private set; }
        public double B { get; private set; }
        public double C { get; private set; }

        public List<Edge> RelativeEdges { get; set; }
        public Edge RegionTopEdge { get; set; }
        public Edge RegionBottomEdge { get; set; }
        //public List<Edge> CrossedEdges { get; private set; }
        public BorderRegion Region { get; set; }
        public Edge NextEdgeInRegion { get; set; }
        //public Edge PrevEdge { get; set; }

        public Edge(BorderPoint start, BorderPoint end, SideEnum.BorderSide sideName)
        {
            Start = start;
            End = end;
            CreateLineEquation(start, end);
            SideName = sideName;
            RelativeEdges = new List<Edge>();
            //CrossedEdges = new List<Edge>();
        }

        public Edge(BorderPoint start, BorderPoint end)
        {
            Start = start;
            End = end;
            CreateLineEquation(start, end);
            RelativeEdges = new List<Edge>();
            //CrossedEdges = new List<Edge>();
        }

        ///// <summary>
        ///// Находит точки пересечения луча (Start, End) и с ребрами из relativeEdges
        ///// </summary>
        ///// <returns></returns>
        //public List<(BorderPoint,Edge)> GetInterPointsWithRelativeEdges()
        //{
        //    var result = new List<(BorderPoint, Edge)>();
        //    foreach (var edge in RelativeEdges)
        //    {
        //        if (CrossedEdges.Contains(edge))
        //            continue;
        //        var point = GetIntersectionWith(edge);
        //        if (point == null) continue; //миникостыль
        //        if(point.OnRay(this) && (!point.OnSegment(this) ||point.IsVertexIn(this)))
        //        {
        //            if (!result.Contains((point,edge)))
        //            {
        //                var pairsWithSamePoint = result.Where(p => p.Item1.Equals(point)).ToList();
        //                if(pairsWithSamePoint.Count == 0 || this.Region.Name != edge.Region.Name)
        //                    result.Add((point, edge));
        //                else
        //                {
        //                    //Кандидат - результат пересечения ребер одного региона. Если найдется ребро из другого региона, то добавить его. Иначе кандидата
        //                    var candidateToAdd = (point, edge);
        //                    foreach(var pair in pairsWithSamePoint)
        //                        if(this.Region.Name != pair.Item2.Region.Name)
        //                            candidateToAdd = pair;
        //                    result.Add(candidateToAdd);                      
        //                }

        //            }
        //        }

        //        //1. Проверить, что точка лежит на луче
        //        //2. Проверить, что точка не принадлежит отрезку (Start, End), за исключением вершин отрезка
        //        //3. Проверить, что точка внешняя - надо ли?

        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Находит точку пересечения двух прямых. Возвращает null, если прямые параллельны.
        ///// </summary>
        ///// <param name="edge"></param>
        ///// <returns></returns>
        //public BorderPoint GetIntersectionWith(Edge edge)
        //{
        //    var a1 = A;
        //    var a2 = edge.A;
        //    var b1 = B;
        //    var b2 = edge.B;
        //    var c1 = C;
        //    var c2 = edge.C;

        //    var xNumenator = c2 * b1 - c1 * b2; ;
        //    var yNumerator = a2 * c1 - a1 * c2; ;
        //    var denominator = a1 * b2 - a2 * b1;


        //    //Частный случай, когда отрезки пересекаются по отрезку
        //    if (xNumenator == 0 && yNumerator == 0)
        //    {
        //        //Ближайшая к старту
        //        if ((Start.IsBelongsTo(edge) && End.IsBelongsTo(edge)) || (edge.Start.IsBelongsTo(this) && edge.End.IsBelongsTo(this)))
        //            throw new Exception("Неправильное построение точек. Отрезок полностью принадлежит другому отрезку. Wrong");
        //        else return End;
        //    }

        //    else if (denominator == 0) // прямые параллельны.
        //        return null;
        //    else
        //    {
        //        return new BorderPoint(new Vector3((float)(xNumenator / denominator), edge.Start.HeightInTheWorld, (float)(yNumerator / denominator)));
        //    }


        //}

        /// <summary>
        /// Пересекаются ли прямые.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public bool IsIntersectWith(Edge edge)
        {
            var a1 = A;
            var a2 = edge.A;
            var b1 = B;
            var b2 = edge.B;
            var c1 = C;
            var c2 = edge.C;

            var xNumenator = c2 * b1 - c1 * b2; ;
            var yNumerator = a2 * c1 - a1 * c2; ;
            var denominator = a1 * b2 - a2 * b1;
            return denominator != 0  || (denominator == 0 && xNumenator == 0 && yNumerator == 0);
        }


        /// <summary>
        /// Найти relative боковые стороны в regions для рассматриваемого ребра
        /// </summary>
        /// <param name="regions"></param>
        public void SetRelativeEdgesFrom(List<BorderRegion> regions)
        {
            foreach (var r in regions)
                foreach (var side in r.Sides)
                {
                    var interPoint = this.GetIntersectionWith(side);
                    if (interPoint != null &&
                        ((SideName == side.SideName) || (SideName != side.SideName && interPoint.OnSegment(this) && interPoint.OnSegment(side)))
                        && !RelativeEdges.Contains(side))
                        RelativeEdges.Add(side);
                        
                }
        }

        /// <summary>
        /// Возвращает точку пересечения прямых. Возвращает null, если прямые параллельны.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public BorderPoint GetIntersectionWith(Edge edge)
        {
            if (!IsIntersectWith(edge)) return null;

            var a1 = A;
            var a2 = edge.A;
            var b1 = B;
            var b2 = edge.B;
            var c1 = C;
            var c2 = edge.C;

            var xNumenator = c2 * b1 - c1 * b2; ;
            var yNumerator = a2 * c1 - a1 * c2; ;
            var denominator = a1 * b2 - a2 * b1;

            //Частный случай, когда прямые совпадают
            if (denominator == 0 && xNumenator == 0 && yNumerator == 0)
            {
                BorderPoint nearestToStart = null;
                float distance = float.MaxValue;
                var list = new List<BorderPoint> {edge.Start, edge.End };
                foreach(var p in list)
                {
                    var d = Vector2.Distance(p.Position, Start.Position);
                    if(d<distance)
                    {
                        nearestToStart = p;
                        distance = d;
                    }
                }
                return nearestToStart;
            }

            return new BorderPoint(new Vector3((float)(xNumenator / denominator), edge.Start.HeightInTheWorld, (float)(yNumerator / denominator)));

        }

        /// <summary>
        /// Пересекаются ли отрезки.
        /// </summary>
        /// <param name="segment"></param>
        /// <returns></returns>
        public bool IsIntersectBySegmentWith(Edge segment)
        {
            /* Start = p1
             * End = p2
             * segment.Start = p3
             * segment.End = p4;
             * Взаимное расположение отрезков можно проверить с помощью векторных произведений v1 = [p3p4,p3p1] v2 = [p3p4,p3p2], v3 = [p1p2,p1p3], v4 = [p1p2,p1p4]
             * Точка P1 лежит слева от прямой P3P4, для нее векторное произведение v1 > 0, так как векторы положительно ориентированы.
                Точка P2 расположена справа от прямой, для нее векторное произведение v2 < 0, так как векторы отрицательно ориентированы.
                Для того чтобы точки P1 и P2 лежали по разные стороны от прямой P3P4, достаточно, чтобы выполнялось условие v1v2 < 0 (векторные произведения имели противоположные знаки).
                Аналогичные рассуждения можно провести для отрезка P1P2 и точек P3 и P4.
                если v1v2 < 0 и v3v4 < 0, то отрезки пересекаются.
             */
            //if (segment.Equals(this)) return false;
            var p1 = Start;
            var p2 = End;
            var p3 = segment.Start;
            var p4 = segment.End;
            var v1 = VectorProduct(p4.X - p3.X, p4.Z - p3.Z, p1.X - p3.X, p1.Z - p3.Z);
            var v2 = VectorProduct(p4.X - p3.X, p4.Z - p3.Z, p2.X - p3.X, p2.Z - p3.Z);
            var v3 = VectorProduct(p2.X - p1.X, p2.Z - p1.Z, p3.X - p1.X, p3.Z - p1.Z);
            var v4 = VectorProduct(p2.X - p1.X, p2.Z - p1.Z, p4.X - p1.X, p4.Z - p1.Z);

            //Частный случай пересечения по концу или началу отрезка или пересечения отрезка по отрезку.
            if ((p1.IsBelongsTo(segment)) || (p2.IsBelongsTo(segment))
                    || (p3.IsBelongsTo(this)) || (p4.IsBelongsTo(this)))
            {
                return true;
            }

            if ((v1 * v2) < 0 && (v3 * v4) < 0)
                return true;
            return false;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Edge);
        }

        public bool Equals(Edge segment)
        {
            return (Start.Equals(segment.Start) && End.Equals(segment.End)
                || (Start.Equals(segment.End) && End.Equals(segment.Start)));
        }

        public void SetIsConnect(bool value)
        {
            IsConnect = value;
        }

        public bool IsIntersectWith(List<Edge> segments)
        {
            foreach (var segment in segments)
            {
                if (segment.Equals(this)) continue;
                if (IsIntersectWith(segment)) return true;
            }
            return false;
        }

        public bool IsIntersectBySegmentWith(List<Edge> segments)
        {
            foreach (var segment in segments)
            {
                if (segment.Equals(this)) continue;
                if (IsIntersectBySegmentWith(segment)) return true;
            }
            return false;
        }

        /// <summary>
        /// Возращает список внешних по отношению к соседним регионам точек пересечения стороны с боковыми сторонами соседних регионов
        /// </summary>
        /// <param name="regions"></param>
        /// <returns></returns>
        public List<BorderPoint> GetIntersectionPointsWith(List<BorderRegion> regions)
        {
            var result = new List<BorderPoint>();
            foreach (var region in regions)
                result.AddRange(GetIntersectionPointsWith(region.Sides));
            return result;
        }

        public List<BorderPoint> GetExtIntersectionPointsWith(List<BorderRegion> regions)
        {
            return GetIntersectionPointsWith(regions)
                .Where(point => !point.IsPointInternalInRegions(regions))
                .ToList();
        }


        public List<BorderPoint> GetIntersectionPointsWith(List<Edge> segments)
        {
            var intersectionPoints = new List<BorderPoint>();
            foreach (var segment in segments)
            {
                if (IsIntersectWith(segment))
                {
                    var intersectionPoint = GetIntersectionWith(segment);
                    if (!intersectionPoints.Contains(intersectionPoint))
                        intersectionPoints.Add(new BorderPoint(new Vector3(intersectionPoint.X, intersectionPoint.HeightInTheWorld, intersectionPoint.Z)));
                }
            }
            return intersectionPoints;
        }

        public float GetCountIntersectionWith(BorderRegion region)
        {
            float result = 0;
            var crossedPoints = new List<BorderPoint>();
            foreach (var segment in region.Edges)
            {
                if (IsIntersectWith(segment))
                {
                    var intersectionPoint = this.GetIntersectionWith(segment);
                    if (intersectionPoint.IsVertexIn(region) && !crossedPoints.Contains(intersectionPoint))
                    {
                        var neibs = intersectionPoint.GetNeighboringPointsIn(region);
                        if (neibs.Count != 2) throw new Exception("Неправильная работа функции ближайших вершин в регионе");
                        var p1 = neibs[0].Position;
                        var p2 = neibs[1].Position;
                        var vector1 = p1 - Start.Position;
                        var vector2 = p2 - Start.Position;
                        var v1 = VectorProduct(Vector, vector1);
                        var v2 = VectorProduct(Vector, vector2);
                        if (v1 * v2 < 0) result += 1;
                        crossedPoints.Add(intersectionPoint);
                        
                    }
                    else
                    {
                        result += 1;
                    }
                }
            }
            return result;
        }

        //public bool IsNeedToAdjust(Edge segment)
        //{
        //    var firstDeterminant = (segment.C * A - segment.A * C);
        //    var secondDeterminant = (segment.C * B - segment.B * C);
        //    return firstDeterminant == 0 && secondDeterminant == 0;
        //}

        //public BorderPoint GetIntersectionPointWith(Edge segment)
        //{
        //    // e-maxx.ru/algo/lines_intersection
        //    //grafika.me/node/237

        //    var firstDeterminant = (segment.C * A - segment.A * C);
        //    var secondDeterminant = (segment.C * B - segment.B * C);
        //    var p1 = Start;
        //    var p2 = End;
        //    var p3 = segment.Start;
        //    var p4 = segment.End;

        //    ////Частный случай, когда отрезки пересекаются по отрезку
        //    if (firstDeterminant == 0 && secondDeterminant == 0)
        //    {
        //        //Ближайшая к старту
        //        if ((p1.IsBelongsTo(segment) && p2.IsBelongsTo(segment)) || (p3.IsBelongsTo(this) && p4.IsBelongsTo(this)))
        //            throw new Exception("Неправильное построение точек. Отрезок полностью принадлежит другому отрезку. Wrong");
        //        else return p2;

        //    }
        //    else
        //    {
        //        //var denominator = A * segment.B - segment.A * B;
        //        //var x = secondDeterminant / denominator;
        //        //var z = firstDeterminant / denominator;
        //        //return new BorderPoint(new Vector3((float)x, segment.Start.HeightInTheWorld, (float)z));
        //        var d = (A * segment.B - B * segment.A);
        //        var dx = -C * segment.B + B * segment.C;
        //        var dy = (-A * segment.C + C * segment.A);
        //        return new BorderPoint(new Vector3((float)(dx / d), segment.Start.HeightInTheWorld, (float)(dy / d)));
        //    }
        //}

        private void CreateLineEquation(BorderPoint start, BorderPoint end)
        {
            A = end.Z - start.Z;
            B = start.X - end.X;
            C = start.Z * end.X - end.Z * start.X;
        }

        public static float VectorProduct(float ax, float ay, float bx, float by)
        {
            //ax и ay  - координаты первого вектора. bx и by - координаты второго вектора
            return ax * by - bx * ay;
        }

        public static float VectorProduct(Vector2 a, Vector2 b)
        {
            return a.x * b.y - b.x * a.y;
        }

       
    }
}
