using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator.Tools
{
    public class CircumscribedSphere : MonoBehaviour
    {
        public Vector3 Center;
        public float Radius;

        protected void Start()
        {
            //var minX = float.PositiveInfinity;
            //var maxX = float.NegativeInfinity;
            //var minY = float.PositiveInfinity;
            //var maxY = float.NegativeInfinity;
            //var minZ = float.PositiveInfinity;
            //var maxZ = float.NegativeInfinity;

            //var stack = new Stack<Transform>();
            //stack.Push(transform);

            //while (stack.Count > 0)
            //{
            //    var current = stack.Pop();
            //    var curRenderer = current.GetComponent<Renderer>();
            //    if (curRenderer != null)
            //    {
            //        var curCenter = curRenderer.bounds.center;
            //        var curExtents = curRenderer.bounds.extents;

            //        minX = Mathf.Min(minX, curCenter.x + curExtents.x, curCenter.x - curExtents.x);
            //        maxX = Mathf.Max(maxX, curCenter.x + curExtents.x, curCenter.x - curExtents.x);
            //        minY = Mathf.Min(minY, curCenter.y + curExtents.y, curCenter.y - curExtents.y);
            //        maxY = Mathf.Max(maxY, curCenter.y + curExtents.y, curCenter.y - curExtents.y);
            //        minZ = Mathf.Min(minZ, curCenter.z + curExtents.z, curCenter.z - curExtents.z);
            //        maxZ = Mathf.Max(maxZ, curCenter.z + curExtents.z, curCenter.z - curExtents.z);
            //    }

            //    foreach (var child in current)
            //        stack.Push(child as Transform);
            //}

            //Center = new Vector3(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2, minZ + (maxZ - minZ) / 2);
            //Radius = new Vector3(maxX - minX, maxY - minY, maxZ - minZ).magnitude / 2;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(Center, Radius);
        }
    }
}
