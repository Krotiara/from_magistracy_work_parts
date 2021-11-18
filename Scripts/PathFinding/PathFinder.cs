using CableWalker.Simulator.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CableWalker.Simulator.PathFinding
{
    public class PathFinder 
    {     
        private readonly IPathFinder pathFinder;
        private string visibilityGraphBuilderTag = "VisibilityGraphBuilder";
        private VisibilityGraphBuilder visibilityGraphBuilder;
        public IEnumerable<Vector3> Solution { get; private set; }
        public IEnumerable<Vector3> Visited => pathFinder.Visited;

        public PathFinder(IPathFinder pathFinder)
        {
            this.pathFinder = pathFinder;
            visibilityGraphBuilder = GameObject.FindGameObjectWithTag(visibilityGraphBuilderTag).GetComponent<VisibilityGraphBuilder>();
        }
        
        public IEnumerator FindPath(Vector3 start, Vector3 target)
        {
            Solution = null;
            Graph visibilityGraph = visibilityGraphBuilder.BuildVisibilityGraph(start, target);
            yield return pathFinder.FindPath(start, target, visibilityGraph);
            if (pathFinder.Solution == null)
                yield break;
            ShowSolution(); ///////////////////////
            Solution = pathFinder.Solution.ToList();
        }

        public void ShowSolution()
        {
            var p = GameObject.Find("pathFinderDemo");
            if (p != null)
                GameObject.DestroyImmediate(p);
            var pathFinderDemo = new GameObject("pathFinderDemo");

            foreach (var v in pathFinder.Solution)
            {
                var a = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                a.transform.position = v;
                a.transform.parent = pathFinderDemo.transform;
                a.GetComponent<SphereCollider>().enabled = false;
            }
            var s = new List<Vector3>();
            s.AddRange(pathFinder.Solution);

            for (int i = 0; i < s.Count - 1; i++)
            {
                var b = new GameObject();
                var line = b.AddComponent<LineRenderer>();
                b.transform.parent = pathFinderDemo.transform;
              //  line.material = testBuildMaterial;
                line.SetPosition(0, s[i]);
                line.SetPosition(1, s[i + 1]);
                line.startWidth = 0.05f;
                line.endWidth = 0.05f;
            }

        }
    }
}
