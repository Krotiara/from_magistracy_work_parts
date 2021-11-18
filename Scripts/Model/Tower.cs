using CableWalker.Simulator.BorderCreator;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace CableWalker.Simulator.Model
{
    public class Tower : Model
    {
       
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double[] WorldPosition => new double[] { Latitude, Longitude };

       
        public string TowerNumber { get; private set; }
        public string TowerType { get; private set; }
        public string TowerName { get; private set; }
        public Vector3 PrefabRotation { get; private set; }
        public List<InsulatorString> InsulatorStrings = new List<InsulatorString>();
        
        public List<Tower> NextTowers { get; private set; }
        public List<Tower> PreviousTowers { get; private set; }
        
        public List<string> PreviousTowerNumbers { get; private set; }
        public List<BorderRegion> Regions { get; private set; }
        private string prefabPath;
        
     

        public TowerKind Kind { get; private set; }

        private string tag;

        public bool IsAnchor
        {
            get
            {
                return (TowerType != ((int)TowerTypes.Intermediate).ToString()
                    && TowerType != ((int)TowerTypes.IntermediateCorner).ToString());
            }
        }

        public bool IsIntermediate
        {
            get
            {
                return (TowerType == ((int)TowerTypes.Intermediate).ToString()
                    || TowerType == ((int)TowerTypes.IntermediateCorner).ToString());
            }
        }

        public void SetPosition(Vector3 pos)
        {
            Position = pos;
            ObjectOnScene.transform.position = pos;
        }

        public Tower() { }

        public Tower(Vector3 position, Vector3 rotation, string prefabPath, string name, string number, string type, Vector3 size, double latitude, double longitude, string tag)
        {
            Position = position;
            Rotation = rotation;
            TowerName = name;
            this.prefabPath = prefabPath;
            //Model = model;
            TowerNumber = number;
            Number = number;
            Name = Number;
            PhotoPath = "";
            TowerType = type;
            Size = size;
            PreviousTowerNumbers = new List<string>();
            PreviousTowers = new List<Tower>();
            NextTowers = new List<Tower>();
            Regions = new List<BorderRegion>();
            Defects = new List<Defect>();
            Latitude = latitude;
            Longitude = longitude;
            Tag = tag;
            CameraFollowPoint = new Vector3(0, Size.y, 0);
            Kind = ModelCypherParams.GetTowerParams(name);        
        }

        public void SetPrevNextTowers(InformationHolder infoHolder)
        {
            //CHECK
            foreach (var insulatorString in InsulatorStrings)
            {
                if (insulatorString.InsStringRelativeNumber == ((int)CablesNumbers.PortalNumber).ToString()) continue;
                try
                {
                    var relativeInsulatorString = infoHolder.Get<InsulatorString>(insulatorString.InsStringRelativeNumber);
                    if (!Contains(NextTowers, relativeInsulatorString.Tower.Number) && relativeInsulatorString.Tower.Number != TowerNumber)
                        SetNextTower(relativeInsulatorString.Tower);
                    if (!Contains(relativeInsulatorString.Tower.PreviousTowers, TowerNumber) && relativeInsulatorString.Tower.Number != TowerNumber)
                        relativeInsulatorString.Tower.SetPreviousTower(this);
                }
                catch (KeyNotFoundException)
                {
                    continue;
                }
            }
        }

        private bool Contains(List<Tower> towers, string towerNumber)
        {
            foreach (var nextTower in towers)
                if (nextTower.Number == towerNumber) return true;
            return false;
        }

        private bool Contains(List<string> towersNumbers, string towerNumber)
        {
            foreach (var number in towersNumbers)
                if (number == towerNumber) return true;
            return false;
        }

        public void SetNextTower(Tower nextTower)
        {
            NextTowers.Add(nextTower);
        }

        public void SetPreviousTower(Tower prevTower)
        {
            PreviousTowers.Add(prevTower);
            if (!Contains(PreviousTowerNumbers, prevTower.Number))
                PreviousTowerNumbers.Add(prevTower.Number);
        }

        public override Model Create(List<string> args, List<string> typeArgs, InformationHolder infoHolder, bool isEditorMode)
        {
            string towerNumber = args[0];
            double latitude = double.Parse(args[1], CultureInfo.InvariantCulture);
            double longitude = double.Parse(args[2], CultureInfo.InvariantCulture);
            double utmX = double.Parse(args[3], CultureInfo.InvariantCulture);
            double utmY = double.Parse(args[4], CultureInfo.InvariantCulture);
            double angle = double.Parse(args[5], CultureInfo.InvariantCulture);
            string prevTowerNumber = args[6];
            var towerName = typeArgs[0];
            var towerType = typeArgs[1];
            var size = new Vector3(float.Parse(typeArgs[3], CultureInfo.InvariantCulture) / 1000, 
                float.Parse(typeArgs[2], CultureInfo.InvariantCulture) / 1000, 
                float.Parse(typeArgs[4], CultureInfo.InvariantCulture) / 1000);
            var prefabPath = typeArgs[5];
            double prevTowerRotation = 0;
            var prevTower = infoHolder.Get<Tower>(prevTowerNumber);
            var position = GPSEncoder.GPSToUCS(new double[2] { latitude, longitude });
            if (isEditorMode)
            {
                var currentTerrain = TerrainUtils.GetCurrentTerrain(Terrain.activeTerrain, position);
                var terrainHeight = currentTerrain.SampleHeight(position);
                position.y = terrainHeight;
            }
            var rotation = new Vector3((float)(angle + prevTowerRotation), 0.0f, 0.0f);
            byte[] bytes = Encoding.Default.GetBytes(towerName);
            var tag = infoHolder.GetTag(typeof(Tower));
            return new Tower(position, rotation, prefabPath, Encoding.UTF8.GetString(bytes), towerNumber, towerType, size, latitude, longitude, tag);
        }

#if UNITY_EDITOR
        public override GameObject Instantiate()
        {
            var objectsFolder = GameObject.FindGameObjectWithTag("TowersFolder");
            var prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
            if (prefab == null) throw new System.Exception($"Prefab in prefab path {prefabPath} doesnt exsist");
            var tower = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            tower.transform.position = Position;
            tower.transform.name = Number;
            tower.transform.parent = objectsFolder.transform;
            ObjectOnScene = tower;
            ObjectOnScene.tag = Tag;
            var defectsObject = ObjectOnScene.transform.Find("Defects");
           // var defectsCollector = defectsObject.gameObject.AddComponent<DefectsCollector>();
            var indexHolder = ObjectOnScene.AddComponent<IndexHolder>();
            indexHolder.type = GetType().ToString();
            indexHolder.index = Number;
            CreateTowerCollider();
            ObjectOnScene.gameObject.layer = LayerMask.NameToLayer("PowerLineObjects");
            ObjectOnScene.AddComponent<Obstacle>();
            return ObjectOnScene;
        }

        public override GameObject Instantiate2D()
        {
            var objectsFolder = GameObject.FindGameObjectWithTag("TowersFolder");

            var prefab = AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/2DScene/TowerLabelSimple.prefab", typeof(GameObject)) as GameObject;
            if (prefab == null) throw new System.Exception($"Prefab in prefab path {prefabPath} doesnt exsist");
            var tower = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            tower.transform.position = Position;
           // tower.transform.rotation = Quaternion.Euler(Rotation);
            tower.transform.name = Number;
            tower.transform.parent = objectsFolder.transform;
            ObjectOnScene = tower;
            ObjectOnScene.tag = Tag;
            tower.transform.Find("Label").GetComponent<TextMeshPro>().text = Number;
            var indexHolder = ObjectOnScene.AddComponent<IndexHolder>();
            indexHolder.type = GetType().ToString();
            indexHolder.index = Number;
            ObjectOnScene.gameObject.layer = LayerMask.NameToLayer("PowerLineObjects");
  
            return ObjectOnScene;
        }

#endif
        private void CreateTowerCollider()
        {
            float savedDistance = 4;
            var colliderTower = ObjectOnScene.GetComponent<BoxCollider>();
            colliderTower.center = new Vector3(colliderTower.center.x, Size.y / 2, colliderTower.center.z);
            var size = new Vector3(Size.x + 2*savedDistance, Size.y + 2*savedDistance, Size.z + 2*savedDistance);
            colliderTower.size = size;
        }

        public (Vector3, Vector3) GetLeftRightPosOnTower()
        {
            Vector3 left = ObjectOnScene.transform.position + ObjectOnScene.transform.right * Size.x / 2;
            Vector3 right = ObjectOnScene.transform.position - ObjectOnScene.transform.right * Size.x / 2;
            return (left, right);
        }

        public InsulatorString GetLeftMostStringOnTower()
        {
            var currentString = InsulatorStrings[0];
            foreach (var str in InsulatorStrings)
                if (str.Position.x < currentString.Position.x) currentString = str;
            return currentString;
        }

        public InsulatorString GetRightMostStringOnTower()
        {
            var currentString = InsulatorStrings[0];
            foreach (var str in InsulatorStrings)
                if (str.Position.x > currentString.Position.x) currentString = str;
            return currentString;
        }

        public override List<(string, string)> GetInfo()
        {
            int towerType = int.Parse(TowerType);
            string regInfo = Kind == null ? "" : Kind.GetInfo();
            return new List<(string, string)>
            {
                ("Number", Number),
                ("GPS", $"Latitude:{Latitude}; Longitude:{Longitude}"),
                ("Type", ((TowerTypes)towerType).ToString()),
                ("Number of insulator strings", InsulatorStrings.Count.ToString()),
                ("Regulatory information", regInfo)
            };           
        }

        
       

        public override void CalculateCondition()
        {
            foreach (var defect in Defects)
            {
                Condition += defect.Criticality;
            }
        }

        public override List<string> GetCellsNamesToTable()
        {
            var result = new List<string>
            {
                "Tower Number",
                "Condition",
            };
            return result;
        }

        public override List<string> GetInfoForTable()
        {
            CalculateCondition();
            return new List<string> { Number, Condition.ToString() };
        }

        public override void SetObjectOnSceneParams()
        {
            Position = ObjectOnScene.transform.position;
        }
    }
}
