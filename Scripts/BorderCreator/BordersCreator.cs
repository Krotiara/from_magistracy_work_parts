using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.Simulator.BorderCreator
{

    public class BordersCreator
    {
        public static GameObject BordersFolder { get; private set; }
        public static Material BordersMaterial { get; private set; }
        public static float BordersHeight { get; private set; }
        private List<BorderRegion> regions;
       

        public BordersCreator(GameObject bordersFolder, Material bordersMaterial, float bordersHeight)
        {
            BordersFolder = bordersFolder;
            BordersFolder.layer = LayerMask.NameToLayer("Borders");
            BordersMaterial = bordersMaterial;
            BordersHeight = bordersHeight;
            regions = new List<BorderRegion>();
        }

        public BordersCreator()
        {
            regions = new List<BorderRegion>();
        }

        public void AddRegion(BorderRegion region)
        {
            regions.Add(region);
        }

        public BorderRegion CreateRegion(Vector3 startPos, Vector3 endPos,Vector3 leftPos,
            Vector3 rightPos, float securedDistance, Transform rotationParent, string name)
        {
            var colRotationObject = new GameObject();
            colRotationObject.transform.position = startPos;
            colRotationObject.transform.rotation = Quaternion.LookRotation(endPos - startPos);
            colRotationObject.transform.parent = rotationParent;
            var zDistance = Vector3.Distance(startPos, endPos);
            var zLocalPosStart = 0;
            var zLocalPosEnd = zDistance;
            var leftStart = CreatePoint(colRotationObject, "LeftStart", new Vector3(-securedDistance + leftPos.x, 0, zLocalPosStart));
            var leftEnd = CreatePoint(colRotationObject, "LeftEnd", new Vector3(-securedDistance + leftPos.x, 0, zLocalPosEnd));
            var rightStart = CreatePoint(colRotationObject, "RightStart", new Vector3(securedDistance + rightPos.x, 0, zLocalPosStart));
            var rightEnd = CreatePoint(colRotationObject, "RightEnd", new Vector3(securedDistance + rightPos.x, 0, zLocalPosEnd));
            var region = new BorderRegion(leftStart.position, leftEnd.position, rightEnd.position, rightStart.position, name);
            GameObject.DestroyImmediate(colRotationObject);
            GameObject.DestroyImmediate(leftStart);
            GameObject.DestroyImmediate(leftEnd);
            GameObject.DestroyImmediate(rightStart);
            GameObject.DestroyImmediate(rightEnd);
            return region;
        }

        public List<Edge> CreateBorders()
        {
            SetRelations();
            //foreach (var region in regions)
            //    InstantiateRegion(region);
            return GetBordersEdges();
        }

        private void InstantiateRegion(BorderRegion region)
        {
            CreateBorder(region.LeftEdge, region.Name);
            CreateBorder(region.RightEdge, region.Name);
            CreateBorder(region.TopEdge, region.Name);
            CreateBorder(region.BottomEdge, region.Name);
        }

        public void InstansiateBorders(List<Edge> edges)
        {
            foreach (var edge in edges)
                CreateBorder(edge,"");
            MeshFilter[] meshFilters = BordersFolder.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            Mesh finalMesh = new Mesh();
            for(int i = 0; i< meshFilters.Length;i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                combine[i].subMeshIndex = 0;

            }
            finalMesh.CombineMeshes(combine);
            BordersFolder.GetComponent<MeshFilter>().sharedMesh = finalMesh;
            BordersFolder.GetComponent<MeshCollider>().sharedMesh = finalMesh;
            BordersFolder.GetComponent<MeshRenderer>().material = BordersMaterial;
            var childrens = new List<GameObject>();
            foreach (Transform t in BordersFolder.transform)
                childrens.Add(t.gameObject);
            foreach (var child in childrens)
                GameObject.DestroyImmediate(child);

        }

        private List<Edge> GetBordersEdges()
        {
            var result = new List<Edge>();

            //Костыль для одного региона
            if(regions.Count == 1)
            {
                result.Add(regions[0].LeftEdge);
                result.Add(regions[0].RightEdge);
                result.Add(regions[0].TopEdge);
                result.Add(regions[0].BottomEdge);
                return result;
            }

            var currentEdge = regions[0].LeftEdge;
            var endPoint = currentEdge.Start;
            BorderPoint stepPoint1 = currentEdge.Start;
            BorderPoint stepPoint2 = currentEdge.End;
            //currentEdge = (stepPoint1, stepPoint2)
            while (true)
            {
                if (currentEdge.RelativeEdges.Count == 0)
                {
                    result.Add(new Edge(stepPoint2, currentEdge.End));
                    var edge = currentEdge.NextEdgeInRegion; //У боковой стороны currentEdge значение NextEdgeInRegion = Top или Bottom
                    result.Add(edge);
                    if (edge.End.Equals(endPoint))
                        break;
                    currentEdge = edge.NextEdgeInRegion;
                    stepPoint1 = currentEdge.Start;
                    stepPoint2 = currentEdge.End;
                }
                else
                {
                    var interPairs = GetIntersectionPairs(currentEdge);
                    var pair = GetNearestPair(stepPoint1, currentEdge, interPairs);
                    stepPoint2 = pair.Item1;
                    result.Add(new Edge(stepPoint1, stepPoint2));
                    stepPoint1 = stepPoint2;
                    pair.Item2.RelativeEdges.Remove(currentEdge);
                    currentEdge = pair.Item2;
                }
            }
            return result;
        }

        

        private (BorderPoint,Edge) GetNearestPair(BorderPoint from, Edge currentEdge, List<(BorderPoint, Edge)> pairs)
        {
            (BorderPoint, Edge) currentPair = (null,null);
            var currentDistance = float.MaxValue;
            foreach(var pair in pairs)
            {
                var d = Vector2.Distance(from.Position, pair.Item1.Position);
                if(!pair.Item1.Equals(from) && !pair.Item1.Equals(currentEdge.Start) && d < currentDistance)
                {
                    currentPair = pair;
                    currentDistance = d;
                }
            }
            return currentPair;
        }

        /// <summary>
        /// Возращает пары (a,b), где a - точка пересечения currentEdge и relative edge, которая лежит на луче edge, b = relative edge. Гарантируется, что currentEdge.RelativeEdges.Count !=0 
        /// </summary>
        /// <param name="currentEdge"></param>
        /// <returns></returns>
        private List<(BorderPoint,Edge)> GetIntersectionPairs(Edge currentEdge)
        {
            var result = new List<(BorderPoint, Edge)>();
            
            foreach(var rE in currentEdge.RelativeEdges)
            {
                var interPoint = currentEdge.GetIntersectionWith(rE);
                if (interPoint != null
                    && interPoint.OnRay(currentEdge)
                    && Vector2.Distance(currentEdge.Start.Position, interPoint.Position) <= currentEdge.Length + rE.Length)
                {
                    result.Add((interPoint, rE));
                }
                else
                    result.Add((rE.Start, rE));
            }
            
            return result;
            
        }

        private void SetRelations()
        {
            foreach (var region in regions)
            {
                region.SetRelationRegionsFrom(regions);
                foreach(var side in region.Sides)
                {
                    side.SetRelativeEdgesFrom(region.RelativeRegions);
                } 
            }
        }

        

        /// <summary>
        /// Возвращает расстояние между двумя точками.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private float D(BorderPoint p, BorderPoint q)
        {
            return Vector2.Distance(p.Position, q.Position);
        }

        /// <summary>
        /// Возвращает минимальное расстояние от точки p до набора точек q.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private float DD(BorderPoint p, List<BorderPoint> q)
        {
            //DD(p, q) = min{D(p, q1), D(p, q2), D(p, q3), …, D(p, qn)}. 
            var result = float.MaxValue;
            foreach (var point in q)
            {
                var d = D(p, point);
                if (d < result)
                    result = d;
            }
            return result;
        }

        /// <summary>
        /// Возвращает минимальное расстояние от точки p до ребра edge.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        private float DE(BorderPoint p, Edge edge)
        {
            return Mathf.Min(D(p,edge.Start), D(p, edge.End));
        }

        
        /// <summary>
        /// C какой стороны от вектора ab находится точка c. "+" ~ слева. "-" ~ справа. Значение z координаты векторного произведения векторов ab и bc.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private float Rotate(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.x-a.x) * (c.y-b.y) - (b.y-a.y) * (c.x-b.x);
        }
        
        private Transform CreatePoint(GameObject colRotationObject, string name, Vector3 localPos)
        {
            var point = new GameObject(name);
            point.transform.rotation = colRotationObject.transform.rotation;
            point.transform.parent = colRotationObject.transform;
            point.transform.localPosition = localPos;
            return point.transform;
        }

        //private static void CreateSkyBorder(BorderRegion region)
        //{
        //    var insurance = 4;
        //    var direction = region.TopEdge.MiddlePoint - region.BottomEdge.MiddlePoint;
        //    var distanceZ = Vector3.Distance(region.TopEdge.MiddlePoint, region.BottomEdge.MiddlePoint) + 2 * insurance;
        //    var distanceX = 3 * Vector3.Distance(region.LeftEdge.MiddlePoint, region.RightEdge.MiddlePoint);
        //    var size = new Vector3(distanceX, 1, distanceZ);
        //    var colliderSize = new Vector3(1, 3, 1);
        //    var middlePoint = (region.LeftEdge.MiddlePoint + region.RightEdge.MiddlePoint) / 2;
        //    middlePoint = new Vector3(middlePoint.x, BordersHeight / 2, middlePoint.z);
        //    CreateCollider(size, colliderSize, direction, middlePoint, "Sky", true);
        //}

        public static void CreateCollider(Vector3 size, Vector3 colliderSize, Vector3 direction, Vector3 middlePoint, string name, bool disableDisplay, string tag, string layerName )
        {
            var objectCollider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            objectCollider.name = name;
            //objectCollider.layer = LayerMask.NameToLayer("Ignore Raycast");
            objectCollider.tag =tag;
            objectCollider.layer = LayerMask.NameToLayer(layerName);
            var mesh = objectCollider.GetComponent<MeshRenderer>();
            var collider = objectCollider.GetComponent<BoxCollider>();
            mesh.sharedMaterial = BordersMaterial;
            objectCollider.transform.position = middlePoint;
            objectCollider.transform.rotation = Quaternion.LookRotation(direction);
            objectCollider.transform.localScale = new Vector3(size.x, size.y, size.z);
            objectCollider.transform.parent = BordersFolder.transform;
            if (disableDisplay) mesh.enabled = false;
            collider.size = colliderSize;
        }

        private static void CreateBorder(BorderPoint start, BorderPoint end, string name)
        {
            var middlePoint = (start.Position + end.Position) / 2;
            var direction2d = end.Position - start.Position;
            var direction = new Vector3(direction2d.x, 0, direction2d.y);
            if (direction.Equals(Vector3.zero))
            {
                direction.z += 1;
            }
            var insuranceMeter = 2;
            var zDistance = Vector2.Distance(start.Position, end.Position);
            if (zDistance < 0.05f) return; //Не надо строить очень мелкие соединения
            var border = GameObject.CreatePrimitive(PrimitiveType.Cube);
            border.name = name;
            border.tag = "Border";
            var mesh = border.GetComponent<MeshRenderer>();
            var collider = border.GetComponent<BoxCollider>();
            mesh.sharedMaterial = BordersMaterial;
            border.AddComponent<MeshCollider>();
            border.transform.position = new Vector3(middlePoint.x, Mathf.Min(start.HeightInTheWorld, end.HeightInTheWorld), middlePoint.y);
            border.transform.rotation = Quaternion.LookRotation(direction);
            border.transform.localScale = new Vector3(0.1f, BordersHeight, zDistance); //0.1f - чтобы жирными не были
            border.transform.parent = BordersFolder.transform;
            //collider.size = new Vector3(insuranceMeter * 2 * 10, collider.size.y, collider.size.z); //*10 - компенсация за 0.1f
        }

        private static void CreateBorder(Edge segment, string name)
        {
            CreateBorder(segment.Start, segment.End, name);
        }

    }
}
