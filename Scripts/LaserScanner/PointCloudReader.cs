using UnityEngine;
using System.Collections;
using System.IO;
using System.Globalization;

namespace CableWalker.Simulator.LaserScanning
{
    public class PointCloudReader : MonoBehaviour
    {
        public Transform PointCloudFolder;
        public Material matVertex;
        public float scale = 1f;
        public bool invertYZ = false;
        public string laserPointCloudMask = "LaserPointCloud";
        public int numPoints;
        public int numPointGroups;
        private int limitPoints = 65000;
        private GameObject pointCloud;
        private Vector3[] points;
        private Color[] colors;
        private Vector3 minValue;
        private string cloudName;

        public GameObject LoadPointCloud(string filePath)
        {
            cloudName = Path.GetFileName(filePath);
            pointCloud = new GameObject(cloudName);
            pointCloud.tag = "PointCloudFolder";
            if (File.Exists(filePath))
            {
                StartCoroutine("LoadOFF", filePath);
                return pointCloud;
            }
            else
                return null;
        }

        // Start Coroutine of reading the points from the OFF file and creating the meshes
        IEnumerator LoadOFF(string dPath)
        {
            // Read file
            StreamReader sr = new StreamReader(dPath);
            sr.ReadLine(); 
            string[] buffer = sr.ReadLine().Split(); // nPoints, nFaces
            int nunPoints = 0;
            numPoints = int.Parse(buffer[0]);
            points = new Vector3[numPoints];
            colors = new Color[numPoints];
            minValue = new Vector3();

            for (int i = 0; i < numPoints; i++)
            {
                try
                {
                    buffer = sr.ReadLine().Split();

                    if (!invertYZ)
                        points[i] = new Vector3(float.Parse(buffer[0], CultureInfo.InvariantCulture) * scale,
                            float.Parse(buffer[1], CultureInfo.InvariantCulture) * scale,
                            float.Parse(buffer[2], CultureInfo.InvariantCulture) * scale);
                    else
                        points[i] = new Vector3(float.Parse(buffer[0], CultureInfo.InvariantCulture) * scale,
                            float.Parse(buffer[2], CultureInfo.InvariantCulture) * scale,
                            float.Parse(buffer[1], CultureInfo.InvariantCulture) * scale);

                    if (buffer.Length >= 5)
                        colors[i] = new Color(int.Parse(buffer[3], CultureInfo.InvariantCulture) / 255.0f,
                            int.Parse(buffer[4], CultureInfo.InvariantCulture) / 255.0f,
                            int.Parse(buffer[5], CultureInfo.InvariantCulture) / 255.0f);
                    else
                        colors[i] = Color.red;
                }
                catch
                {
                    nunPoints++;
                }
            }

            // Instantiate Point Groups
            numPointGroups = Mathf.CeilToInt(numPoints * 1.0f / limitPoints * 1.0f);
            
            for (int i = 0; i < numPointGroups - 1; i++)
            {
                InstantiateMesh(i, limitPoints, cloudName);
            }
            InstantiateMesh(numPointGroups - 1, numPoints - (numPointGroups - 1) * limitPoints, cloudName);
            yield return null;
        }

        GameObject InstantiateMesh(int meshInd, int nPoints, string filename)
        {          
            GameObject pointGroup = new GameObject(filename + meshInd);
            var pointGroupMeshFilter = pointGroup.AddComponent<MeshFilter>();
            pointGroup.AddComponent<MeshRenderer>();
            pointGroup.GetComponent<Renderer>().material = matVertex;
            pointCloud.transform.parent = PointCloudFolder;
            pointGroupMeshFilter.mesh = CreateMesh(meshInd, nPoints, limitPoints);
            pointGroup.transform.parent = pointCloud.transform;
            pointGroup.layer = LayerMask.NameToLayer(laserPointCloudMask);
            return pointGroup;
        }

        Mesh CreateMesh(int id, int nPoints, int limitPoints)
        {
            Mesh mesh = new Mesh();
            Vector3[] myPoints = new Vector3[nPoints];
            int[] indecies = new int[nPoints];
            Color[] myColors = new Color[nPoints];

            for (int i = 0; i < nPoints; ++i)
            {
                myPoints[i] = points[id * limitPoints + i] - minValue;
                indecies[i] = i;
                myColors[i] = colors[id * limitPoints + i];
            }

            mesh.vertices = myPoints;
            mesh.colors = myColors;
            mesh.SetIndices(indecies, MeshTopology.Points, 0);
            mesh.uv = new Vector2[nPoints];
            mesh.normals = new Vector3[nPoints];

            return mesh;
        }

        void CalculateMin(Vector3 point)
        {
            if (minValue.magnitude == 0)
                minValue = point;
            if (point.x < minValue.x)
                minValue.x = point.x;
            if (point.y < minValue.y)
                minValue.y = point.y;
            if (point.z < minValue.z)
                minValue.z = point.z;
        }
    }
}
