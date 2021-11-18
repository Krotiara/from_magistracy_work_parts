using CableWalker.Simulator.SceneParser;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace CableWalker.Simulator.Model
{
    public class Road : Model
    {
       

        
        //private List<Vector3[]> regions;
        public double[] GPSStart { get; private set; }
        public double[] GPSEnd { get; private set; }
        public Vector3 StartPosition { get; private set; }
        public Vector3 EndPosition { get; private set; }
        public float Width { get; private set; }



        public Road() {}

        public Road(string number, double[] gpsStart, double[] gpsEnd, float width, string tag)
        {
            //this.regions = regions;
           Number = number;
            Width = width;
            GPSStart = gpsStart;
            GPSEnd = gpsEnd;
            StartPosition = GPSEncoder.GPSToUCS(GPSStart);
            EndPosition = GPSEncoder.GPSToUCS(GPSEnd);
            StartPosition += new Vector3(0, TerrainUtils.GetSampleHeight(StartPosition), 0);
            EndPosition += new Vector3(0, TerrainUtils.GetSampleHeight(EndPosition), 0);
            Tag = tag;
        }


        public override Model Create(List<string> args, List<string> typeArgs,
            InformationHolder infoHolder, bool isEditorMode)
        {
            var number = args[0];
            var gpsStart = new double[2] { double.Parse(args[1], CultureInfo.InvariantCulture), double.Parse(args[2], CultureInfo.InvariantCulture) };
            var gpsEnd = new double[2] { double.Parse(args[3], CultureInfo.InvariantCulture), double.Parse(args[4], CultureInfo.InvariantCulture) };
            var width = float.Parse(args[5], CultureInfo.InvariantCulture);
            var tag = infoHolder.GetTag(typeof(Road));

            return new Road(number, gpsStart, gpsEnd, width,tag);
        }

      

        private List<Vector3[]> GetRegionsByHeightDifference(GameObject start, GameObject end)
        {
            var result = new List<Vector3[]>();
            var points = new List<Vector3>();
            var distance = Vector3.Distance(start.transform.position, end.transform.position);

            Vector3 point = start.transform.position;
            point.y = TerrainUtils.GetSampleHeight(start.transform.position);
            points.Add(point);
            while (distance > 0)
            {
                start.transform.position += start.transform.forward;
                point = start.transform.position;
                point.y = TerrainUtils.GetSampleHeight(point);
                distance -= 1;
                points.Add(point);
            }

           

            Vector3 estimatingVector = points[1] - points[0];
            Vector3 currentVectorStart = points[0];
            Vector3 currentVectorEnd;
            for(int i = 1; i < points.Count-1;i++)
            {
                currentVectorEnd = points[i];
                if (Vector3.Cross(currentVectorEnd - currentVectorStart,estimatingVector) == Vector3.zero)
                {
                    continue;
                }
                else
                {
                    result.Add(new Vector3[2] { currentVectorStart, currentVectorEnd });
                    currentVectorStart = points[i];
                    currentVectorEnd = points[i + 1];
                    estimatingVector = currentVectorEnd - currentVectorStart;
                }
                

            }
            return result;
          
        }

        private void CorrectTerrainForRoad(GameObject start, GameObject end, float width)
        {
            float insurance = 1;
            float height;
            var distance = Vector3.Distance(start.transform.position, end.transform.position) + 1;
            height = TerrainUtils.GetHeightOnTerrain(start.transform.position);
            start.transform.position += start.transform.right * (-width / 2); // Перемещаем влево
            SetHeightOnWidthLine(start, width);
            start.transform.position += start.transform.forward;
            distance -= 1;
            while(distance > 0)
            {
                SetHeightOnWidthLine(start, width + 2 * insurance);
                start.transform.position += start.transform.forward;
                distance -= 1;
            }
        }

        private void CorrectTerrainForRoad(List<Vector3[]> regions, float width, GameObject obj1, GameObject obj2)
        {
            foreach (var region in regions)
            {
                obj1.transform.position = region[0];
                obj2.transform.position = region[1];
                CorrectTerrainForRoad(obj1, obj2, width);
            }
        }

        private void SetHeightOnWidthLine(GameObject start, float width)
        {
            // start должен находится на левом крае дороги
            // width - целое число
            // в конце метода start возращается на левый край дороги
            //GameObject a = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            //a.transform.position = start.transform.position;
            float height = GetMinHeightOnWidthLine(start, width);

            for (int i = 0; i < width; i++)
            {
                TerrainUtils.SetHeightOnTerrain(start.transform.position, height,true);
                start.transform.position += start.transform.right;
            }

            start.transform.position += start.transform.right * (-width); //Возращаем влево
        }

        private float GetMinHeightOnWidthLine(GameObject obj, float width)
        {
            //obj находится на левом крае дороги
            float result = float.MaxValue;
            float currentHeight = 0;
            for (int i=0; i<width;i++)
            {
                currentHeight = TerrainUtils.GetHeightOnTerrain(obj.transform.position);
                if (currentHeight < result) result = currentHeight;
                obj.transform.position += obj.transform.right;
            }
            obj.transform.position += obj.transform.right * (-width); //Возращаем влево
            return currentHeight;
        }

        private List<Vector3[]> GetRoadRegions()
        {
            var direction = EndPosition - StartPosition;

            GameObject obj1 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            GameObject obj2 = GameObject.CreatePrimitive(PrimitiveType.Capsule);

            obj1.transform.position = StartPosition; 
            obj2.transform.position = EndPosition; 
            obj1.transform.rotation = Quaternion.LookRotation(direction);
            obj2.transform.rotation = Quaternion.LookRotation(direction);
            //center
            float insurance = 1;
            Size = new Vector2(Width,
                Vector3.Distance(obj1.transform.position, obj2.transform.position) + insurance);
            Position = (obj1.transform.position + obj2.transform.position) / 2;
            List<Vector3[]> regions = GetRegionsByHeightDifference(obj1, obj2);       
            CorrectTerrainForRoad(regions, Width, obj1, obj2);
            GameObject.DestroyImmediate(obj1);
            GameObject.DestroyImmediate(obj2);
            return regions;
        }

#if UNITY_EDITOR
        public override GameObject Instantiate()
        {
            var regions = GetRoadRegions();
            float savedDistance = 5;
            var lengthInsurance = 1;
            var folder = GameObject.FindGameObjectWithTag("RoadsFolder");
            var prefab = Resources.Load("Prefabs/RoadPrefab",typeof(GameObject));
            ObjectOnScene = new GameObject();
            ObjectOnScene.AddComponent<MeshRenderer>();
            ObjectOnScene.AddComponent<MeshFilter>();
            ObjectOnScene.AddComponent<MeshCollider>();
            ObjectOnScene.name = Number;
            ObjectOnScene.transform.parent = folder.transform;
            ObjectOnScene.gameObject.tag = Tag;
            foreach (var region in regions)
            {
                var roadPart = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                roadPart.transform.position = (region[0] + region[1]) / 2;
                roadPart.transform.rotation = Quaternion.LookRotation(region[1] - region[0]);
                roadPart.transform.localScale = new Vector3(Width, roadPart.transform.localScale.y,
                    Vector3.Distance(region[0], region[1]) + lengthInsurance);
                roadPart.transform.parent = ObjectOnScene.transform;
                roadPart.gameObject.layer = LayerMask.NameToLayer("Objects");
                roadPart.gameObject.tag = Tag;
                roadPart.AddComponent<Obstacle>();
                var box = roadPart.GetComponent<BoxCollider>();
                box.size = new Vector3(box.size.x *2, savedDistance * 2, box.size.z); //так, потому что в отличие от других объектов у дороги меняется scale
            }           
            return ObjectOnScene;
        }

        public override GameObject Instantiate2D()
        {
            var folder = GameObject.FindGameObjectWithTag("RoadsFolder");
            ObjectOnScene = new GameObject();
            ObjectOnScene.AddComponent<MeshRenderer>();
            ObjectOnScene.AddComponent<MeshFilter>();
            ObjectOnScene.AddComponent<MeshCollider>();
            ObjectOnScene.name = Number;
            ObjectOnScene.transform.parent = folder.transform;
            ObjectOnScene.gameObject.tag = Tag;
            var lineRenderer = ObjectOnScene.AddComponent<LineRenderer>();
            lineRenderer.material= (Material)Resources.Load("Materials/2DRoadMaterial", typeof(Material));
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = Width;
            lineRenderer.endWidth = Width;
            lineRenderer.SetPosition(0, new Vector3(StartPosition.x, 0.1f, StartPosition.z));
            lineRenderer.SetPosition(1, new Vector3(EndPosition.x, 0.1f, EndPosition. z));

            return ObjectOnScene;
        }
#endif

        private void CombineMeshes(GameObject obj)
        {
            MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            Mesh finalMesh = new Mesh();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                combine[i].subMeshIndex = 0;

            }
            finalMesh.CombineMeshes(combine);
            obj.GetComponent<MeshFilter>().sharedMesh = finalMesh;
            obj.GetComponent<MeshCollider>().sharedMesh = finalMesh;
            obj.GetComponent<MeshRenderer>().sharedMaterial = obj.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().sharedMaterial;
            var childrens = new List<GameObject>();
            foreach (Transform t in obj.transform)
                childrens.Add(t.gameObject);
            foreach (var child in childrens)
                GameObject.DestroyImmediate(child);
        }

        public override List<(string, string)> GetInfo()
        {
            throw new System.NotImplementedException();
        }

        public override void CalculateCondition()
        {
           
        }

        public override List<string> GetCellsNamesToTable()
        {
            throw new System.NotImplementedException();
        }

        public override List<string> GetInfoForTable()
        {
            throw new System.NotImplementedException();
        }

        public override void SetObjectOnSceneParams()
        {
            return;
        }
    }
}
