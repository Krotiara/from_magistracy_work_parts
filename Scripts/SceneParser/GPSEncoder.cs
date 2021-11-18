using System;
using UnityEngine;

namespace CableWalker.Simulator
{
    public sealed class GPSEncoder
    {
        /// <summary>
        /// Convert UCS (X,Y,Z) coordinates to GPS (Lat, Lon) coordinates
        /// </summary>
        /// <returns>
        /// Returns Vector2 containing Latitude and Longitude
        /// </returns>
        /// <param name='position'>
        /// (X,Y,Z) Position Parameter
        /// </param>
        public static double[] USCToGPS(Vector3 position)
        {
            return GetInstance().ConvertUCStoGPS(position);
        }

        /// <summary>
        /// Convert GPS (Lat, Lon) coordinates to UCS (X,Y,Z) coordinates
        /// </summary>
        /// <returns>
        /// Returns a Vector3 containing (X, Y, Z)
        /// </returns>
        /// <param name='gps'>
        /// (Lat, Lon) as Vector2
        /// </param>
        public static Vector3 GPSToUCS(double[] gps)
        {
            return GetInstance().ConvertGPStoUCS(gps);
        }

        /// <summary>
        /// Convert GPS (Lat, Lon) coordinates to UCS (X,Y,Z) coordinates
        /// </summary>
        /// <returns>
        /// Returns a Vector3 containing (X, Y, Z)
        /// </returns>
        public static Vector3 GPSToUCS(double latitude, double longitude)
        {
            return GetInstance().ConvertGPStoUCS(new double[2] { latitude, longitude });
        }

        /// <summary>
        /// Change the relative GPS offset (Lat, Lon), Default (0,0), 
        /// used to bring a local area to (0,0,0) in UCS coordinate system
        /// </summary>
        /// <param name='localOrigin'>
        /// Referance point.
        /// </param>
        public static void SetLocalOrigin(double[] localOrigin)
        {
            GetInstance()._localOrigin = localOrigin;
        }

        #region Singleton
        private static GPSEncoder _singleton;

        private GPSEncoder() {}

        private static GPSEncoder GetInstance()
        {
            if (_singleton == null)
                _singleton = new GPSEncoder();
            return _singleton;
        }
        #endregion

        #region Instance Variables

        private double[] _localOrigin = new double[2] { 0, 0 };
        private double _LatOrigin => _localOrigin[0];
        private double _LonOrigin => _localOrigin[1];

        private double metersPerLat;
        private double metersPerLon;
        #endregion

        #region Instance Functions


        private void FindMetersPerLat(double lat) // Compute lengths of degrees
        {
            var m1 = 111132.954;    // latitude calculation term 1 
            var m2 = -559.822;        // latitude calculation term 2
            var m3 = 1.175;      // latitude calculation term 3
            var m4 = -0.0023;
            var p1 = 111412.84;    // longitude calculation term 1
            var p2 = -93.5;      // longitude calculation term 2
            var p3 = 0.118;      // longitude calculation term 3

            lat *= Mathf.Deg2Rad;

            // Calculate the length of a degree of latitude and longitude in meters
            metersPerLat = m1 + (m2 * Math.Cos(2 * lat)) + (m3 * Math.Cos(4 * lat)) + (m4*Math.Cos(6*lat));
            metersPerLon = (p1 * Math.Cos(lat)) + (p2 * Math.Cos(3 * lat)) + (p3 * Math.Cos(5 * lat));
        }

        private Vector3 ConvertGPStoUCS(double[] gps)
        {
            FindMetersPerLat(_LatOrigin);
            var zPosition = metersPerLat * (gps[0] - _LatOrigin); //Calc current lat
            var xPosition = metersPerLon * (gps[1] - _LonOrigin);

            return new Vector3((float)xPosition, 0, (float)zPosition);
        }

        private double[] ConvertUCStoGPS(Vector3 position)
        {
            FindMetersPerLat(_LatOrigin);
            var geoLocation = new double[2] { 0, 0 };
            geoLocation[0] = (_LatOrigin + (position.z) / metersPerLat); //Calc current lat
            geoLocation[1] = (_LonOrigin + (position.x) / metersPerLon); //Calc current lon
            return geoLocation;
        }
        #endregion
    }
}
