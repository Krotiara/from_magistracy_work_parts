using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator.Tools
{
    public static class Vector3Extension
    {
        public static Vector3 NearestTo(this IEnumerable<Vector3> vectorsFrom, Vector3 vectorTo)
        {
            var minDistance = float.MaxValue;
            var nearestVector = Vector3.zero;
            
            foreach (var vector in vectorsFrom)
            {
                var newDistance = (vector - vectorTo).magnitude;
                if (newDistance < minDistance)
                {
                    minDistance = newDistance;
                    nearestVector = vector;
                }
            }

            return nearestVector;
        }
    }
}
