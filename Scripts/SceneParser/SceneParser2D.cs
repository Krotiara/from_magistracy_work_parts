using CableWalker.Simulator.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator.SceneParser
{
    public class SceneParser2D : MonoBehaviour
    {

        private static InformationHolder infoHolder;
        private GameObject sceneNameFolder;
        private GameObject bordersFolder;
        private GameObject roadsFolder;
        private GameObject cablesLineFolder;
        private GameObject towersFolder;

        private string sceneName;
        private string configPath;
        private double centerLat;
        private double centerLon;

#if UNITY_EDITOR
        public SceneParser2D(string sceneName, string configPath, double centerLat, double centerLon, double terrainHeightInWorld)
        {
            infoHolder = GameObject.FindGameObjectWithTag("InfoHolder").GetComponent<InformationHolder>();
            infoHolder.InitializeHolder();
            sceneNameFolder = new GameObject(sceneName);
            sceneNameFolder.gameObject.tag = infoHolder.sceneNameTag;
            CreateObjectsFolders();
            SetTags();
            this.sceneName = sceneName;
            this.configPath = configPath;
            this.centerLat = centerLat;
            this.centerLon = centerLon;

        }
#endif

        private void CreateObjectsFolders()
        {
            var objects = new GameObject("Objects");
            objects.tag = "ObjectsFolder";
            bordersFolder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bordersFolder.name = "Borders";
            bordersFolder.AddComponent<MeshCollider>();
            DestroyImmediate(bordersFolder.GetComponent<BoxCollider>());
            roadsFolder = new GameObject("Roads");
            towersFolder = new GameObject("Towers");
            cablesLineFolder = new GameObject("CablesLinesFolder");
            cablesLineFolder.transform.parent = objects.transform;
            bordersFolder.transform.parent = objects.transform;
            roadsFolder.transform.parent = objects.transform;
            towersFolder.transform.parent = objects.transform;
            sceneNameFolder.transform.parent = objects.transform;
        }

        private void SetTags()
        {
            cablesLineFolder.tag = "CablesLinesFolder";
            bordersFolder.tag = "BordersFolder";
            roadsFolder.tag = "RoadsFolder";
            towersFolder.tag = "TowersFolder";
        }

#if UNITY_EDITOR
        public void Parse()
        {
            infoHolder.objectsLoader.LoadConfig(infoHolder,true);
            var objects = infoHolder.GetAll();
            var securedDistance = infoHolder.SecuredDistance;

            foreach (var type in infoHolder.dispatch.Values)
            {
                if (type.Equals(typeof(Cable)))
                {
                    CorrectRelativeTowersRotation();
                    CorrectRelativeStringsRotation();
                }
                foreach (var obj in objects[type].Values)
                    obj.Instantiate2D();
            }
            //   CorrectStringsAnglesForCables();
            SetColliders(securedDistance);
        }
#endif
        private static void CorrectRelativeTowersRotation()
        {
            /*
                 * 1. У 3д моделей опор положительное направление (=направлению ВЛ) = -ось Z.
                 * 2. Поэтому они все развернуты на 180 градусов при генерации.
                 */
            var towers = infoHolder.GetList<Tower>();
            foreach (Tower t in towers)
            {
                if (t.NextTowers.Count > 0)
                {
                    var dir = t.Position - t.NextTowers[0].Position;
                    dir = new Vector3(dir.x, 0, dir.z);
                    t.ObjectOnScene.transform.rotation = Quaternion.LookRotation(dir);
                }
                else
                {
                    var dir = t.PreviousTowers[0].Position - t.Position;
                    dir = new Vector3(dir.x, 0, dir.z);
                    t.ObjectOnScene.transform.rotation = Quaternion.LookRotation(dir);
                }
            }
            //towers[towers.Count - 1].ObjectOnScene.transform.eulerAngles = new Vector3(0, -180, 0); //-180, потому что у 3д моделей опор положительное направление ( направление ВЛ) = -ось Z.
        }

        private static void CorrectRelativeStringsRotation()
        {
            var strings = infoHolder.GetList<InsulatorString>();
            foreach (InsulatorString insulatorString in strings)
            {
                if (insulatorString.InsStringRelativeNumber == ((int)CablesNumbers.PortalNumber).ToString())
                    continue;
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
            startRotation.y += GetRotationAngleY(firstString, secondString, isRelative);
            firstString.Rotate(startRotation);
        }

        private static float GetRotationAngleY(InsulatorString start, InsulatorString end, bool isRelative)
        {
            int sign = isRelative ? -1 : 1;
            var tReference = start.ObjectOnScene.transform; // used for creating a rotation reference only in one axis
            var originalAngles = tReference.eulerAngles;
            tReference.eulerAngles =
                new Vector3(0, start.ObjectOnScene.transform.eulerAngles.y, 0); // only Y axis, meaning right & left turns.
            var dir = end.Position - start.Position;
            dir.y = 0;
            var angle = Vector3.SignedAngle(sign * tReference.forward, dir, tReference.up);
            start.ObjectOnScene.transform.eulerAngles = originalAngles;
            return angle;
        }

        private void CorrectStringsAnglesForCables()
        {
            foreach (Cable cableModel in infoHolder.GetList<Cable>())
            {
                var points = new List<Vector3>();
                points.Add(new Vector3(cableModel.Start.Position.x, 0.1f, cableModel.Start.Position.z));
                points.Add(new Vector3(cableModel.End.Position.x, 0.1f, cableModel.End.Position.z));
                cableModel.CurrentMode.Points = points;


                if (cableModel.Points.Count == 0) continue;
                var startString = cableModel.Start;
                var endString = cableModel.End;
                if (startString == null || endString == null)
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

        private void SetColliders(float securedDistance)
        {
            List<Vector3> borderEdges = new List<Vector3>();
            foreach (var border in infoHolder.CreateBorders(infoHolder.SecuredDistance,true))
            {
                borderEdges.Add(border.Start.Position);
                borderEdges.Add(border.End.Position);
            }

            if (borderEdges.Count > 0)
            {
                CreateBorderIcons(borderEdges);
            }
        }

        private void CreateBorderIcons(List<Vector3> borderEdges)
        {
            GameObject border = new GameObject("BorderIcons");
            var lineRendererComponent = border.AddComponent<LineRenderer>();

            Material mat = (Material)Resources.Load("Materials/PhaseMaterialG2", typeof(Material));

            CreateLineRenderer(lineRendererComponent, borderEdges);
            lineRendererComponent.positionCount = borderEdges.Count;
            lineRendererComponent.material = mat;
            lineRendererComponent.startWidth = 1f;
            lineRendererComponent.endWidth = 1f;

            border.transform.SetParent(bordersFolder.transform);
            border.transform.localPosition = Vector3.zero;
        }

        private void CreateLineRenderer(LineRenderer component, List<Vector3> points)
        {
            component.GetComponent<Renderer>().enabled = true;
            component.positionCount = points.Count;
            for (var i = 0; i < points.Count; i++)
                component.SetPosition(i, new Vector3 (points[i].x, 0.2f, points[i].y));
        }
    }
}
