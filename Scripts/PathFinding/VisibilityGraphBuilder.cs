using CableWalker.Simulator.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CableWalker.Simulator.PathFinding
{
    public class VisibilityGraphBuilder : MonoBehaviour
    {
        public InformationHolder infoHolder;
        public Material testBuildMaterial;

        private int layerMask;

        private readonly string cableTag = "Cable";
        private readonly string bordersTag = "BordersFolder";
        private readonly string groundTag = "Ground";
        private readonly string insStringTag = "String";
        private readonly string cableObstacleTag = "ObstacleBoxCollider";

        private List<WayPoint> wayPoints;

        void Start()
        {
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
            layerMask = 1 << LayerMask.NameToLayer("PowerLineObjects")
            | 1 << LayerMask.NameToLayer("Borders")
            | 1 << LayerMask.NameToLayer("Ground")
            | 1 << LayerMask.NameToLayer("Objects"); //В маске будет учитыватсья слой объектов вл
                                                     //при добавлении еще маски нужно будет сделать побитовое OR в виде a|b
            SetWayPoints();
        }

        private void SetWayPoints()
        {
            var wayPointsDict = new Dictionary<string, WayPoint>();
            var towers = infoHolder.GetList<Tower>();
            List<string> viewedTowers = new List<string>();
            foreach (Tower tower in towers)
            {
                var boxCollider = tower.ObjectOnScene.GetComponent<BoxCollider>();
                var pos = boxCollider.transform.position;
                var wayPointPosition = tower.ObjectOnScene.transform.TransformPoint(boxCollider.center + new Vector3(0, boxCollider.size.y, 0) * 0.5f + new Vector3(0, 3, 0));
                wayPointsDict[tower.Number] = new WayPoint(wayPointPosition, tower.Number);
            }

            foreach (KeyValuePair<string, WayPoint> entry in wayPointsDict)
            {
                var tower = infoHolder.Get<Tower>(entry.Key);
                foreach (Tower nT in tower.NextTowers)
                {
                    entry.Value.NextWayPoints.Add(wayPointsDict[nT.Number]);
                    wayPointsDict[nT.Number].PrevWayPoints.Add(entry.Value);
                }
            }

            wayPoints = wayPointsDict.Values.ToList();
        }

        public Graph BuildVisibilityGraph(Vector3 startPoint, Vector3 targetPoint)
        {
            // TODO-nexusasx10: Заменить на коллайдер платформы
            var colliders = Physics.OverlapSphere(startPoint, 0.01f);
            if (colliders.Length > 0)
            {
                throw new System.Exception("Start point is in obstacle.");
            }

            // TODO-nexusasx10: Заменить на коллайдер платформы
            colliders = Physics.OverlapSphere(targetPoint, 0.01f);
            if (colliders.Length > 0)
            {
                throw new System.Exception("Target point is in obstacle.");
            }

            foreach (var w in wayPoints)
            {
                w.IsVisited = false;
                w.Parent = null;
            }
            bool isBordersCrossed = false;
            var dict = GetCrossedObstacles(startPoint, targetPoint);
            foreach (var obj in dict.Keys)
            {
                //if (obj.tag == groundTag)
                //    return;
                if (obj.tag == bordersTag)
                    isBordersCrossed = true;
            }
            List<Vector3> vertices = isBordersCrossed ? GetVerticesByWayPoints(startPoint, targetPoint) : GetVerticesByObstacleRaycast(startPoint, targetPoint);
            Graph graph = BuildGraph(vertices, targetPoint);
            //TestBFSToVisibilirtGraph(graph, new Vertex(startPoint), new Vertex(targetPoint));
            TestBuildV(graph);
            return graph;
        }

        private Graph BuildGraph(List<Vector3> vertices, Vector3 targetPos)
        {
            var graph = new Graph(vertices);
            foreach (Vertex v1 in graph.V)
            {
                foreach (Vertex v2 in graph.V)
                {
                    if (v1.Equals(v2))
                        continue;
                    if (IsFreeWay(v1, v2) && IsDirectionToTarget(v1.Position, v2.Position, targetPos)) // пока что не могу доказать корректность ISDirectionToTarget на текущем виде графа(по вершинам и по середнным точкам объектов). Могут быть ошибки..
                    {
                        graph.AddEdge(new Edge(v1, v2));
                        v1.AddNeib(v2);
                    }
                }
            }
            return graph;
        }

        private bool TestBFSToVisibilirtGraph(Graph g, Vertex start, Vertex end)
        {
            Vertex startVertex = null;
            Vertex targetVertex = null;
            foreach(var v in g.V)
            {
                if (v.Equals(start))
                    startVertex = v;
                if (v.Equals(end))
                    targetVertex = v;
            }

            if (startVertex == null || targetVertex == null)
                return false;

            var stack = new Stack<Vertex>();
            stack.Push(startVertex);
            var visited = new List<Vertex>();
            while(stack.Count != 0)
            {
                var s = stack.Pop();
                if (s.Equals(end))
                    return true;
                if(!visited.Contains(s))
                {
                    visited.Add(s);
                }
                foreach (var neib in s.Neibs)
                {
                    if(!visited.Contains(neib))
                        stack.Push(neib);
                }
            }
            return false;
           
        }

        private void TestBuildV(Graph g)
        {
            var p = GameObject.Find("pathFinderTest");
            if (p != null)
                DestroyImmediate(p);
            var pathFinderTest = new GameObject("pathFinderTest");

            foreach (var v in g.V)
            {
                var a = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                a.transform.position = v.Position;
                a.transform.parent = pathFinderTest.transform;

            }

            foreach (var e in g.E)
            {
                var b = new GameObject();
                var line = b.AddComponent<LineRenderer>();
                b.transform.parent = pathFinderTest.transform;
                line.material = testBuildMaterial;
                line.SetPosition(0, e.start.Position);
                line.SetPosition(1, e.end.Position);
                line.startWidth = 0.05f;
                line.endWidth = 0.05f;
            }
        }

        private bool IsDirectionToTarget(Vector3 vertexStart, Vector3 vertexEnd, Vector3 targetPoint)
        {
            return (targetPoint - vertexEnd).magnitude < (targetPoint - vertexStart).magnitude;
        }

        //private bool IsVertexBehindStart(Vector3 vertex, Vector3 startPoint, Vector3 targetPoint)
        //{
        //    return (vertex - targetPoint).magnitude > (startPoint - targetPoint).magnitude;
        //}

        private bool IsFreeWay(Vertex vertexStart, Vertex vertexEnd)
        {
            return GetCrossedObstacles(vertexStart.Position, vertexEnd.Position).Count == 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="u">StartPoint</param>
        /// <param name="v">EndPoint</param>
        /// <returns></returns>
        private List<Vector3> GetVerticesByWayPoints(Vector3 u, Vector3 v)
        {
            var result = new List<Vector3>();
            ///uw - nearest wayPoint to u
            ///vw - nearest wayPoint to v
            WayPoint uw = GetNearestWayPointTo(u);
            WayPoint vw = GetNearestWayPointTo(v);
            var wpoints = GetWayPoints(uw, vw); //Возвращает список waypoints, начиная с u, заканчивая с v
            WayPoint p = vw;
            while (p != uw)
            {
                var start = p.Parent.Position;
                var end = p.Position;
                result.AddRange(GetVerticesByObstacleRaycast(start, end));
                p = p.Parent;
            }
            result.AddRange(GetVerticesByObstacleRaycast(u, uw.Position));
            result.AddRange(GetVerticesByObstacleRaycast(vw.Position, v));
            foreach (var w in wayPoints)
            {
                w.IsVisited = false;
                w.Parent = null;
            }
            return result;
        }

        /// <summary>
        /// Метод получения вершин пересекаемых объектов лучом uv, причем uv не пересекает ground и borders
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        private List<Vector3> GetVerticesByObstacleRaycast(Vector3 start, Vector3 target)
        {
            var result = new List<Vector3>();
            var objects = GetCrossedObstacles(start, target);
            result.Add(start);
            result.Add(target);
            foreach (KeyValuePair<GameObject,Vector3[]> pair in objects)
            {
                var obstacle = pair.Key;
                Vector3 forwardHitPoint = pair.Value[0];
                Vector3 backwardHitPoint = pair.Value[1]; 
                if (obstacle.tag == bordersTag || obstacle.tag == groundTag)
                    throw new System.Exception("uv ray cross border or ground. In method GetVertices this is not allowed");
                Obstacle comp = obstacle.GetComponent<Obstacle>();
                if (comp == null)
                    throw new System.Exception("On obstacle object dosent exist component Obstacle");

                var face1Points = comp.GetPointsOnFaceByHit(forwardHitPoint);
                var face2Points = comp.GetPointsOnFaceByHit(backwardHitPoint);

                foreach (var point in face1Points)
                {
                    var hits = Physics.OverlapSphere(point, 0.001f);
                    if (hits.Count() == 0 || (hits.Count() == 1 && hits[0].transform.gameObject.name == comp.gameObject.name) /*&& !IsVertexBehindStart(point,start,target)*/)
                        result.Add(point);
                }
                foreach (var point in face2Points)
                {
                    var hits = Physics.OverlapSphere(point, 0.001f);
                    if (hits.Count() == 0 || (hits.Count() == 1 && hits[0].transform.gameObject.name == comp.gameObject.name) /*&& !IsVertexBehindStart(point, start, target)*/)
                        result.Add(point);
                }
            }
            return result;
        }

        private List<WayPoint> GetWayPoints(WayPoint from, WayPoint to)
        {
            var stack = new Stack<WayPoint>();
            stack.Push(from);
            from.IsVisited = true;
            WayPoint p;
            while (stack.Count > 0)
            {
                p = stack.Pop();
                if (p.Equals(to))
                    return GetPathFromDFS(p, from);
                var neibs = p.Neibs;
                foreach (WayPoint n in neibs)
                {
                    if (n.IsVisited) continue;
                    stack.Push(n);
                    n.Parent = p;
                    n.IsVisited = true;
                }
            }
            return new List<WayPoint>();
        }

        private List<WayPoint> GetPathFromDFS(WayPoint p, WayPoint from)
        {
            var result = new List<WayPoint>();
            result.Add(p);
            while (p != from)
            {
                result.Add(p.Parent);
                p = p.Parent;
            }
            result.Reverse();
           
            return result;
        }

        private WayPoint GetNearestWayPointTo(Vector3 position)
        {
            WayPoint currentWayPoint = null;
            float currentDistance = float.MaxValue;
            float distance;
            foreach (WayPoint w in wayPoints)
            {
                distance = (position - w.Position).magnitude;
                if (distance < currentDistance)
                {
                    currentWayPoint = w;
                    currentDistance = distance;
                }
            }
            return currentWayPoint;
        }

        public Dictionary<GameObject, Vector3[]> GetCrossedObstacles(Vector3 startPoint, Vector3 targetPoint)
        {
            var result = new Dictionary<GameObject, Vector3[]>(); //(forwardHit, backwardHit)
            Vector3 direction = targetPoint - startPoint;
            var forwardHits = Physics.RaycastAll(startPoint, (targetPoint - startPoint).normalized, (targetPoint - startPoint).magnitude, layerMask);
            var backwardHits = Physics.RaycastAll(targetPoint, (startPoint - targetPoint).normalized, (startPoint - targetPoint).magnitude, layerMask);

            foreach (var hit in forwardHits)
            {
                if (hit.transform.gameObject.tag == cableTag || hit.transform.gameObject.tag == insStringTag)
                    continue; //Костыль из-за привязки для провода layer PowerLineObjects  и к объекту и к отдельному объекту-коллайдеру + не надо подвески, они перекрываются опорой 100 процентов
                result[hit.transform.gameObject] = new Vector3[2] { hit.point, Vector3.zero };
                //if (!result.Contains(hit.transform.gameObject))
                //    result.Add(hit.transform.gameObject);
            }
            foreach(var hit in backwardHits)
            {
                if (hit.transform.gameObject.tag == cableTag || hit.transform.gameObject.tag == insStringTag)
                    continue;
                if(result.Keys.Contains(hit.transform.gameObject))
                    result[hit.transform.gameObject][1] = hit.point;
                
            }
            return result;
        }
    }
}

