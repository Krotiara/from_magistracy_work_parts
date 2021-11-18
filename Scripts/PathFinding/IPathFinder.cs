using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator.PathFinding
{
    public interface IPathFinder
    {
        IEnumerable<Vector3> Solution { get; }
        IEnumerable<Vector3> Visited { get; }
        IEnumerator FindPath(Vector3 start, Vector3 target, Graph visibilityGraph);
    }
}
