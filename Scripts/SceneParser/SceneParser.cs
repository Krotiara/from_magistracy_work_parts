using CableWalker.Simulator.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using CableWalker.Simulator.UI.InfoShower;
using CableWalker.Simulator.BorderCreator;

namespace CableWalker.Simulator.SceneParser
{

    public class SceneParser : MonoBehaviour
    {
        private GameObject gameObjectActivator;
        private GameObject lineRendererFolder;
        private GameObject cablesLineFolder;
        private GameObject towersFolder;
        //private GameObject cablesCollidersFolder;
        private GameObject bordersFolder;
        private GameObject sceneNameFolder;
        private GameObject roadsFolder;
        private Material borderMaterial;
        //private Material cableColliderMaterial;
        private GameObject cleanCablePrimitive;
        private string sceneName;
        private string configPath;
        private Dictionary<Type, List<string[]>> preloadedTypes;
        //private Dictionary<Type, Dictionary<string, Model.Model>> preloadedObjects;

        private double centerLat;
        private double centerLon;
        private double terrainHeightInWorld;

        private static InformationHolder infoHolder;

        private static GameObject spanCablesWatchers;

#if UNITY_EDITOR

        public SceneParser(InformationHolder informationHolder)
        {
            infoHolder = informationHolder;
        }

        public SceneParser(string sceneName, string configPath, double centerLat, double centerLon, double terrainHeightInWorld)
        {
           
            var objects = new GameObject("Objects");
            objects.tag = "ObjectsFolder";
            lineRendererFolder = new GameObject("GreasedLinesFolder");
            cablesLineFolder = new GameObject("CablesLinesFolder");
            bordersFolder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bordersFolder.name = "Borders";
            bordersFolder.AddComponent<MeshCollider>();
            DestroyImmediate(bordersFolder.GetComponent<BoxCollider>());
            roadsFolder = new GameObject("Roads");
            towersFolder = new GameObject("Towers");

            borderMaterial = (Material)Resources.Load("Materials/Border", typeof(Material));
           
            cleanCablePrimitive = (GameObject)Resources.Load("Prefabs/CleanCablePrimitive", typeof(GameObject));

            if (borderMaterial == null)
                throw new Exception("BorderMaterial Load result = null");
            //if (cleanCablePrimitive == null)
            //    throw new Exception("cleanCablePrimitive Load result = null");

            
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
            infoHolder.InitializeHolder();
            sceneNameFolder = new GameObject(sceneName);
            sceneNameFolder.gameObject.tag = infoHolder.sceneNameTag; /*"savedDataName";*/

            lineRendererFolder.transform.parent = objects.transform;
            cablesLineFolder.transform.parent = objects.transform;
            bordersFolder.transform.parent = objects.transform;
            roadsFolder.transform.parent = objects.transform;
            towersFolder.transform.parent = objects.transform;
            sceneNameFolder.transform.parent = objects.transform;
            SetTags();
            this.sceneName = sceneName;
            this.configPath = configPath;
            this.centerLat = centerLat;
            this.centerLon = centerLon;
            TerrainUtils.CheckTerrainsLayers();
        }

        private void SetTags()
        {
            lineRendererFolder.tag = "GreasedLinesFolder";
            cablesLineFolder.tag = "CablesLinesFolder";
            bordersFolder.tag = "BordersFolder";
            roadsFolder.tag = "RoadsFolder";
            towersFolder.tag = "TowersFolder";
        }

    
        private static float GetRotationAngleY(InsulatorString start, InsulatorString end, bool isRelative)
        {
            int sign = isRelative ? -1 : 1;
            var tReference = start.ObjectOnScene.transform; // used for creating a rotation reference only in one axis
            var originalAngles = tReference.eulerAngles;
            tReference.eulerAngles =
                new Vector3(0, start.ObjectOnScene.transform.eulerAngles.y, 0); // only Y axis, meaning right & left turns.
            var dir = end.Position-start.Position;
            dir.y = 0;
            var angle = Vector3.SignedAngle(sign*tReference.forward, dir, tReference.up);
            start.ObjectOnScene.transform.eulerAngles = originalAngles;
            return angle;
        }

        private static void CorrectRelativeStringsRotation()
        {
            var strings = infoHolder.GetList<InsulatorString>();
            foreach (InsulatorString insulatorString in strings)
            {
                if (insulatorString.InsStringRelativeNumber == ((int)CablesNumbers.PortalNumber).ToString())
                {
                    insulatorString.Rotate(new Vector3(0, 180, 0)); // поворот портальных подвесок
                    continue;
                }
                if (insulatorString.Tower.Number == insulatorString.RelativeInsulatorString.Tower.Number) continue;
                Vector3 startRotation = insulatorString.RelativeInsulatorString.IsIntermediate ? Vector3.zero : new Vector3(0, 180, 0);
                CorrectStringRotation(insulatorString.RelativeInsulatorString, insulatorString, startRotation, true);
                if (insulatorString.IsIntermediate) continue; //Промежуточные подвесы корректируется только в качестве relative     
                CorrectStringRotation(insulatorString, insulatorString.RelativeInsulatorString, Vector3.zero, false);
            }
        }

        private static void CorrectStringRotation(Model.InsulatorString firstString, Model.InsulatorString secondString,
            Vector3 startRotation, bool isRelative)
        {
            startRotation.y += GetRotationAngleY(firstString, secondString,isRelative);
            firstString.Rotate(startRotation);
        }

        //public void ParseByPointClouds()
        //{
        //    infoHolder.objectsLoader.LoadConfig(infoHolder);

        //}

        public void Parse()
        {
            infoHolder.objectsLoader.LoadConfig(infoHolder,true);
            InstansiateObjectsOnScene();

            var securedDistance = infoHolder.SecuredDistance; 
            var borderHeight = 2 * GetMaxHeightOfTowers(infoHolder.GetDict<Tower>().Values);           
            CorrectStringsAnglesForCables();
            SetColliders(borderHeight, securedDistance);
            NameGiver.GiveNames(infoHolder);
            TerrainUtils.OffNotActiveTerrains();
        }

        public void RecalculateBorders()
        {
            var securedDistance = infoHolder.SecuredDistance;
            var borderHeight = 2 * GetMaxHeightOfTowers(infoHolder.GetDict<Tower>().Values);
            var oldborders = GameObject.Find("Borders");
            if(oldborders != null)
            {
                GameObject.DestroyImmediate(oldborders);
               
            }
            var bordersFolder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bordersFolder.name = "Borders";
            bordersFolder.AddComponent<MeshCollider>();
            var borderMaterial = (Material)Resources.Load("Materials/Border", typeof(Material));
            DestroyImmediate(bordersFolder.GetComponent<BoxCollider>());
            CreateBorders(securedDistance, borderHeight, bordersFolder, borderMaterial);
        }

        private static void InstansiateObjectsOnScene()
        {
            var objects = infoHolder.GetAll();
            foreach (var type in infoHolder.dispatch.Values)
            {
                //if (type.Equals(typeof(InsulatorString)))
                //    CorrectRelativeTowersRotation();
                if (type.Equals(typeof(Cable)))
                {
                    CorrectRelativeTowersRotation();
                    CorrectRelativeStringsRotation();
                }
                foreach (var obj in objects[type].Values)
                    obj.Instantiate();
            }
        }

        private static void CorrectRelativeTowersRotation()
        {
            /*
                 * 1. У 3д моделей опор положительное направление (=направлению ВЛ) = -ось Z. Update 07.09.2020 - всё-таки наверное Z. Нужно обдумать
                 * 2. Поэтому они все развернуты на 180 градусов при генерации.
                 */
            var towers = infoHolder.GetList<Tower>();
            foreach(Tower t in towers)
            {
                if (t.NextTowers.Count > 0)
                {
                    //var dir = t.Position - t.NextTowers[0].Position ; //07.09.2020
                    var dir = t.NextTowers[0].Position- t.Position ;
                    dir = new Vector3(dir.x, 0, dir.z);
                    t.ObjectOnScene.transform.rotation = Quaternion.LookRotation(dir + t.Rotation);
                }
                else
                {
                    //var dir = t.PreviousTowers[0].Position - t.Position; //07.09.2020
                    var dir = t.Position - t.PreviousTowers[0].Position ;
                    dir = new Vector3(dir.x, 0, dir.z);
                    t.ObjectOnScene.transform.rotation = Quaternion.LookRotation(dir);
                }
            }
            //towers[towers.Count - 1].ObjectOnScene.transform.eulerAngles = new Vector3(0, -180, 0); //-180, потому что у 3д моделей опор положительное направление ( направление ВЛ) = -ось Z.
        }

        private static float GetMaxHeightOfTowers(Dictionary<string, Model.Model>.ValueCollection towers)
        {
            float maxHeight = 0;
            foreach (Tower tower in towers)
            {
                var height = tower.Position.y + tower.Size.y * 2; //*2 - коэффициент для Расставления ограничений
                if (height > maxHeight) maxHeight = height;
            }

            return maxHeight;
        }

        private void SetColliders(float borderHeight, float securedDistance)
        {
            var towers = infoHolder.GetList<Tower>();
            CreateBorders(securedDistance, borderHeight);
            foreach (Tower tower in towers)
                CreateGroundCollider(tower, securedDistance);
        }

        private void CreateBorders(float securedDistance,
            float borderHeight)
        {
            CreateBorders(securedDistance, borderHeight, bordersFolder, borderMaterial);
        }


        private void CreateBorders(float securedDistance,
            float borderHeight,GameObject bordersFolder,Material borderMaterial)
        {
            var borderCreator = new BordersCreator(bordersFolder, borderMaterial, borderHeight);
            var edges = infoHolder.CreateBorders(securedDistance,true);
            borderCreator.InstansiateBorders(edges);
        }

        private void CreateSkyBorder(Vector3 start, Vector3 end, float width)
        {
            var border = GameObject.CreatePrimitive(PrimitiveType.Cube);
            border.layer = LayerMask.NameToLayer("Ignore Raycast");
            border.tag = "Border";
            border.name = "Sky";
            border.transform.position = (start + end) / 2;
            border.transform.rotation = Quaternion.LookRotation(end - start);
            border.transform.localScale = new Vector3(width, 1, Vector3.Distance(start, end));
            border.transform.parent = bordersFolder.transform;
        }

      
        private static void CreateGroundCollider(Tower tower, float securedDistance)
        {
            foreach (var nextTower in tower.NextTowers)
            {
                var direction = nextTower.Position - tower.Position;
                var cablesBetween = infoHolder.GetCablesBetweenTowers(tower, nextTower);
                var colliderSize = new Vector3(securedDistance * 2, 4,
                    Vector3.Distance(tower.Position, nextTower.Position) + 4);
                if (tower.PreviousTowers.Count == 0 || nextTower.NextTowers.Count == 0) colliderSize.z += 6;
                var size = new Vector3(1, 1, 1);
                var middlePoint = (tower.Position + nextTower.Position) / 2;
                
                BordersCreator.CreateCollider(size, colliderSize, direction, middlePoint, "Ground", true, "Ground", "Ground");
            }
        }


        private void CorrectStringsAnglesForCables()
        {
            foreach (Cable cableModel in infoHolder.GetList<Cable>())
            {
                if (cableModel.Points.Count == 0) continue;
                var startString = cableModel.Start;
                var endString = cableModel.End;
                if(startString == null || endString == null)
                {
                    throw new Exception($"On Cable {cableModel.Number} no start or end string objects");
                }
                if (startString.Tower.IsAnchor)
                {
                    var cablePoint = cableModel.Points[1];
                    var verticalAngle = Vector3.Angle(startString.Position, cablePoint);
                    startString.ObjectOnScene.transform.Rotate(-verticalAngle, 0, 0, Space.Self);
                }

                if (endString.Tower.IsAnchor)
                {
                    var cablePoint = cableModel.Points[cableModel.Points.Count - 2];
                    var verticalAngle = Vector3.Angle(endString.Position, cablePoint);
                    endString.ObjectOnScene.transform.Rotate(-verticalAngle, 0, 0, Space.Self);
                }
            }
        }
#endif
    }
}
