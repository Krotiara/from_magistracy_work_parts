using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.Simulator.PathFinding
{
    public class AStar<T> : IPathFinder where T : IPriorityQueue<Vertex>, new()
    {
        public IEnumerable<Vector3> Solution { get; private set; }
        public IEnumerable<Vector3> Visited => visited?.Select(n => n.Position);
        
        private HashSet<Vertex> visited;

        public IEnumerator FindPath(Vector3 start, Vector3 target, Graph visibilityGraph)
        {
            Solution = null;

            visited = new HashSet<Vertex>();

            var priorityQueue = new T();

            Vertex startFind = new Vertex(start);
            Vertex endFind = new Vertex(target);
            Vertex startNode = null;
            Vertex targetNode = null;
            foreach (var vertex in visibilityGraph.V)
            {
                if (vertex.Equals(startFind))
                    startNode = vertex;
                if (vertex.Equals(endFind))
                    targetNode = vertex;
            }

            if (startNode == null || targetNode == null)
            {
                Debug.LogError("In visibility graph no startPoint or target.");
                yield break;
            }
            startNode.G = 0;
            startNode.H = Heuristic(startNode.Position, targetNode.Position);
            startNode.Parent = null;

            priorityQueue.Enqueue(startNode);
            
            while (priorityQueue.Count > 0)
            {
                var currentNode = priorityQueue.DequeueMin();
                if(currentNode.Equals(targetNode))
                {
                    Solution = GetPath(currentNode, startNode, targetNode);
                    yield break;
                }

                visited.Add(currentNode);

                foreach (var neighbourNode in currentNode.Neibs) /*GetNeighbours(currentNode, roundTarget, xStep, yStep, zStep))*/
                {

                    neighbourNode.G = currentNode.G + Vector3.Distance(currentNode.Position, neighbourNode.Position);
                    neighbourNode.H = Heuristic(neighbourNode.Position, targetNode.Position);
                    neighbourNode.Parent = currentNode;

                    if (visited.Contains(neighbourNode))
                        continue;
                
                    var openNode = Find(neighbourNode, priorityQueue);
                    if (openNode == null)
                        priorityQueue.Enqueue(neighbourNode);
                    else if (openNode.G > neighbourNode.G)
                    {
                        openNode.Parent = currentNode;
                        openNode.G = neighbourNode.G;
                        openNode.H = neighbourNode.H;
                    }
                }

                yield return null;
            }
        }


        private static Vertex Find(Vertex node, IEnumerable<Vertex> items)
        {
            foreach (Vertex item in items)
                if (node.Equals(item))
                    return item;
            return null;
        }

        //private static Vector3 RoundVector(Vector3 vector)
        //{
        //    return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
        //}

        private static float Heuristic(Vector3 from, Vector3 to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y) + Mathf.Abs(from.z - to.z);
        }

        private static IEnumerable<Vector3> GetPath(Vertex node, Vertex start, Vertex target)
        {
            var result = new List<Vector3>();
            var currentNode = node;
            while(currentNode != null)
            {
                result.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }         
            result.Reverse();   
            return result;
        }

    }
}
