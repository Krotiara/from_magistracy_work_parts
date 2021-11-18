using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CableWalker.Simulator
{
    public class Obstacle : MonoBehaviour
    {
        public List<Vector3> Vertices { get; private set; }
        //public List<Vector3> Points { get; private set; }
        public Vector3 P000 { get; private set; } //So P000 is the left, bottom, back point as seen in local coordinates.And P101 is the right, bottom, front point.
        public Vector3 P001 { get; private set; }
        public Vector3 P010 { get; private set; }
        public Vector3 P011 { get; private set; }
        public Vector3 P100 { get; private set; }
        public Vector3 P101 { get; private set; }
        public Vector3 P110 { get; private set; }
        public Vector3 P111 { get; private set; }
        private BoxCollider obstacleCollider;
        private Dictionary<(int, int, int), List<(Vector3,Vector3)>> faces;


        private readonly string boxColliderObjectTag = "ObstacleBoxCollider";

        void Awake()
        {
            //Vertices = new List<Vector3>();
            //obstacleCollider = GetComponent<BoxCollider>();
            //if (obstacleCollider == null)
            //{
            //    foreach (Transform child in gameObject.transform)
            //    {
            //        if (child.tag == boxColliderObjectTag)
            //            obstacleCollider = child.GetComponent<BoxCollider>();
            //    }
            //}
            //if (obstacleCollider == null) return;
            //faces = new Dictionary<(int, int, int), List<(Vector3, Vector3)>>();
            //SetVertices(obstacleCollider);
            //SetFaces();
            //GetPoints();
           
          
        }

        public List<Vector3> GetPointsOnFaceByHit(Vector3 hitPos)
        {
            var localPoint = transform.InverseTransformPoint(hitPos);
            localPoint = localPoint - obstacleCollider.center;
            int xkey = 0;
            int yKey = 0;
            int zKey = 0;
            float error = 0.1f;
            //TODO - надо ли при пересечении грани по ребру брать обе смежные грани?
            if (Mathf.Abs(Mathf.Abs(localPoint.x) - obstacleCollider.size.x / 2) <= error)
                xkey = localPoint.x < 0 ? -1 : 1;
            else if (Mathf.Abs(Mathf.Abs(localPoint.y) - obstacleCollider.size.y / 2) <= error)
                yKey = localPoint.y < 0 ? -1 : 1;
            else if (Mathf.Abs(Mathf.Abs(localPoint.z) - obstacleCollider.size.z / 2) <= error)
                zKey = localPoint.z < 0 ? -1 : 1;
            var key = (xkey, yKey, zKey);
            var face = faces[key];
            return CalculatePointsOnFace(face, 10);

        }

        private List<Vector3> CalculatePointsOnFace(List<(Vector3,Vector3)> face, float nPoints)
        {

         
            var result = new List<Vector3>();
            foreach(var edge in face)
            {
                var start = edge.Item1;
                var end = edge.Item2;


                var step = 1 / nPoints;
                for(float t=0;t<=1;t+=step)
                {
                    var point = start * (1 - t) + end * t;
                    result.Add(point);
                }

            }
            return result;
        }

        private void SetFaces()
        {
            faces[(1, 0, 0)] = new List<(Vector3, Vector3)> { (P100, P101), (P100, P110), (P110, P111), (P101, P111) };
            faces[(-1, 0, 0)] = new List<(Vector3, Vector3)> { (P000, P001), (P000, P010), (P010, P011), (P001, P011) };
            faces[(0, 0, -1)] = new List<(Vector3, Vector3)> { (P000, P100), (P000, P010), (P100, P110), (P010, P110) };
            faces[(0, 0, 1)] = new List<(Vector3, Vector3)> { (P001, P101), (P001, P011), (P101, P111), (P011, P111) };
            faces[(0, 1, 0)] = new List<(Vector3, Vector3)> { (P010, P110), (P110, P111), (P111, P011), (P011, P010) };
            faces[(0, -1, 0)] = new List<(Vector3, Vector3)> { (P000, P100), (P100, P101), (P101, P001), (P001, P000) };
        }

        private void SetVertices(BoxCollider bc)
        {
            var pos = bc.transform.position;
            float error = 0.3f; //Обусловленно тем, что коллайдеры односторонние. В следсвтие чего если делать каст из вершины, то пересечения не будет
            P000 = transform.TransformPoint(bc.center + new Vector3(-bc.size.x, -bc.size.y, -bc.size.z) * 0.5f + new Vector3(-error, -error, -error));
            P001 = transform.TransformPoint(bc.center + new Vector3(-bc.size.x, -bc.size.y, bc.size.z) * 0.5f + new Vector3(-error, -error, error));
            P010 = transform.TransformPoint(bc.center + new Vector3(-bc.size.x, bc.size.y, -bc.size.z) * 0.5f + new Vector3(-error, error, -error));
            P011 = transform.TransformPoint(bc.center + new Vector3(-bc.size.x, bc.size.y, bc.size.z) * 0.5f + new Vector3(-error, error, error));
            P100 = transform.TransformPoint(bc.center + new Vector3(bc.size.x, -bc.size.y, -bc.size.z) * 0.5f + new Vector3(error, -error, -error));
            P101 = transform.TransformPoint(bc.center + new Vector3(bc.size.x, -bc.size.y, bc.size.z) * 0.5f + new Vector3(error, -error, error));
            P110 = transform.TransformPoint(bc.center + new Vector3(bc.size.x, bc.size.y, -bc.size.z) * 0.5f + new Vector3(error, error, -error));
            P111 = transform.TransformPoint(bc.center + new Vector3(bc.size.x, bc.size.y, bc.size.z) * 0.5f + new Vector3(error, error, error));
            Vertices = new List<Vector3> { P000, P001, P010, P011, P100, P101, P110, P111 };


        }

    }
}
