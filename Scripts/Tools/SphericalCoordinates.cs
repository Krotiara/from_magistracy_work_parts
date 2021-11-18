using UnityEngine;

namespace CableWalker.Simulator.Tools
{
    public static class SphericalCoordinates
    {
        /// <summary>
        /// Возвращает зенитный угол точки в сферической системе координат.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static float ZenithAngle(Vector3 point)
        {
            return Mathf.PI / 2 - Mathf.Acos(point.y / point.magnitude);
        }

        /// <summary>
        /// Возвращает азимутальный угол точки в сферической системе координат.
        /// </summary>
        /// <param name="point"></param>
        /// <returns>Угол в радианах</returns>
        public static float AzimuthAngle(Vector3 point)
        {
            return Mathf.Atan(point.z / point.x) + Mathf.PI + (point.x >= 0 ? Mathf.PI : 0);
        }
    }
}
