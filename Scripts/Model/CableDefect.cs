using CableWalker.Simulator.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace CableWalker.Simulator.Model
{
    public class CableDefect : Defect
    {
        public string DefNumber { get; private set; }
       
        public (Tower, Tower) Span { get; private set; }
        public string Phase { get; private set; }

        //private Cable cable;
        public Vector3 LocalPosition { get; private set; }

        public float DistanceFromTower1 { get; private set; }
        public float DistanceFromTower2 { get; private set; }

        public string DescriptionByTypeRus { get; private set; }
        public string DescriptionByTypeEn { get; private set; }
        public int TypeNumberFromDataBase { get; private set; }

      

        public CableDefect((Tower, Tower) span, string phase, string description,
            string photoPath, Vector3 localPos, Cable cable, string number,
            float distanceFromTower1, float distanceFromTower2, string descriptionByTypeRus, string descriptionByTypeEn, string tag, string typeNumberFromDataBase, List<string> args)
        {
            Span = span;
            Phase = phase;
            Description = description;
            PhotoPath = photoPath;
            LocalPosition = localPos;
            DescriptionByTypeRus = descriptionByTypeRus;
            DescriptionByTypeEn = descriptionByTypeEn;
            DescriptionByType = DescriptionByTypeEn;
            DistanceFromTower1 = distanceFromTower1;
            DistanceFromTower2 = distanceFromTower2;
            DefNumber = number;
            Number = DefNumber;
            Model = cable;
           
            if (Model != null)
            {
                Model.Defects.Add(this);
            }
            Tag = tag;
            TypeFromDataBase = typeNumberFromDataBase;
            ArgsFromFiles = args;
        }

        
        public CableDefect()
        {
        }

       

        private GameObject objectOnScene;

        public override Model Create(List<string> args, List<string> typeArgs,
            InformationHolder infoHolder, bool isEditorMode)
        {
            string number = args[0];
            string firstTowerNum = args[1];
            string secondTowerNum = args[2];
            string phase = args[3];
            float distanceFromTower1 = args[4] == ""? 0:  float.Parse(args[4].Replace('.', ','), CultureInfo.InvariantCulture);
            float distanceFromTower2 = args[5] == "" ? 0 : float.Parse(args[5].Replace('.', ','), CultureInfo.InvariantCulture);
            string description = args[6];
            string photoPath = args[7];
            //25.02 - не актуально
           // string relativeTowerNum = args[8]; //Если дистанции в конфиге равны 0 или пролет не указан, то дефект связан с креплениями или началом проводов.Нужен номер опоры, где это
            string descriptionByTypeRus = typeArgs[0];
            string descriptionByTypeEn = typeArgs[1];
            string typeNumberFromDataBase = typeArgs[2];
            Tower firstTower;
            Tower secondTower;
            string cableNumber;
            Cable cable;

            firstTower = infoHolder.Get<Tower>(firstTowerNum);
            if (firstTower == null)
                //Первая опора в пролете должна быть указана всегда
                throw new Exception("Incorrect CableDefects config. FirstTower number is wrong");

            secondTower = infoHolder.Get<Tower>(secondTowerNum);
            var span = (firstTower, secondTower);
            if (secondTower == null)
                cableNumber = "-1";
            else
                cableNumber = string.Format("{0}-{1}.{2}", firstTower.Number, secondTower.Number, phase);
            if (cableNumber == "-1" || phase == "земля") //костыльно пока что
                cable = null;
            else
            {
                cable = infoHolder.Get<Cable>(cableNumber);
                if (cable == null)
                {
                    if (CheckCableWithReverseSpan(span, phase, infoHolder))
                    {
                        cable = infoHolder.Get<Cable>(string.Format("{0}-{1}.{2}", secondTower.Number, firstTower.Number, phase));
                        var a = distanceFromTower1;
                        distanceFromTower1 = distanceFromTower2;
                        distanceFromTower2 = a;
                        span = (secondTower, firstTower);
                    }
                    else throw new Exception(string.Format("Incorrect CableDefects config. There is no {0} or cable on scene",
                            cableNumber));
                }
            }
            if (distanceFromTower1 == 0)
                distanceFromTower1 = 1;
            
            var tag = infoHolder.GetTag(typeof(CableDefect));
            return new CableDefect(span, phase, description, photoPath,
                new Vector3(), cable, number, distanceFromTower1, distanceFromTower2,
                descriptionByTypeRus, descriptionByTypeEn,tag, typeNumberFromDataBase, args.Skip(9).ToList());
        }

        private bool CheckCableWithReverseSpan((Tower, Tower) span, string phase, InformationHolder infoHolder)
        {
            Debug.Log(string.Format("{0}-{1}.{2}", span.Item2, span.Item1, phase));
            return infoHolder.Get<Cable>(string.Format("{0}-{1}.{2}", span.Item2.Number, span.Item1.Number, phase)) != null;
        }

#if UNITY_EDITOR
        public override GameObject Instantiate()
        {
            GameObject parentObj = Model == null ? Span.Item1.ObjectOnScene : Model.ObjectOnScene;

            Transform parent = parentObj.transform.Find("Defects");
            var prefab = Resources.Load<GameObject>("Prefabs/DefectLabel");
            objectOnScene = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            objectOnScene.name = Number;
            objectOnScene.transform.position = Model == null ? SetLocalPosition() : ((Cable)Model).GetPointByDistance(DistanceFromTower1); /*cable.GetPointByDistance(DistanceFromTower1);*/ //TODO проблема с получением точки провода по расстоянию
            objectOnScene.transform.SetParent(parent, true);
            objectOnScene.tag = Tag;
            var indexHolder = objectOnScene.AddComponent<IndexHolder>();
            indexHolder.type = GetType().ToString();
            indexHolder.index = Number;
            indexHolder.photoPath = PhotoPath;
            objectOnScene.gameObject.layer = LayerMask.NameToLayer("PowerLineObjects");
            return objectOnScene;
        }

        public override GameObject Instantiate2D()
        {
            return null;
        }
#endif
        private Vector3 SetLocalPosition()
        {
            objectOnScene.transform.position = new Vector3();
            //var defectOffset = 0.6f;
            //var localPosition = new Vector3(defectOffset,
            //    RelativeTower.ObjectOnScene.GetComponent<BoxCollider>().size.y + defectOffset, 0);
            return new Vector3();
        }

        public override List<(string, string)> GetInfo()
        {
            (string, string) spanNumAndCableName = GetSpanNumberAndCableName();
            var spanNumber = spanNumAndCableName.Item1;
            var cableName = spanNumAndCableName.Item2;
            return new List<(string, string)>
            {
                ("Number", DefNumber),
                ("Span", spanNumber),
                ("Phase",Phase),
                ("Type",DescriptionByType),
                ("Description", Description)            
            };

        }

        public override List<string> GetCellsNamesToTable()
        {
            var result = new List<string>();
            result.Add("ID");
            result.Add("Description");
            result.Add("Span");
            result.Add("Distance From Tower 1");
            return result;
        }

        public override List<string> GetInfoForTable()
        {
            return new List<string> { DefNumber, DescriptionByType, GetSpanNumberAndCableName().Item1+" " + GetSpanNumberAndCableName().Item2, DistanceFromTower1.ToString() };
        }

        private (string, string) GetSpanNumberAndCableName()
        {
            string Number1 = Span.Item1 == null ? "земля" : Span.Item1.Number;
            string Number2 = Span.Item2 == null ? "земля" : Span.Item2.Number;
            string spanNumber = string.Format("{0}-{1}", Number1, Number2);
            string cableName;
            if (Span.Item1 != null && Span.Item2 != null)
            {
                cableName = Phase == "земля" || Phase == System.String.Empty
                    ? "грозотрос"
                    : string.Format("{0}-{1}.{2}", Number1, Number2, Phase);
            }
            else
            {
                cableName = "земля"; //хз
            }

            return (spanNumber, cableName);
        }

        public Vector3 GetCableDefectPosition()
        {
            var obj = new GameObject();
            obj.transform.position =((Cable)Model).Start.Position;
            obj.transform.rotation = Quaternion.LookRotation(((Cable)Model).End.Position - ((Cable)Model).Start.Position);
            obj.transform.position += obj.transform.forward * DistanceFromTower1;
            Vector3 position = obj.transform.position;
            GameObject.DestroyImmediate(obj);
            return position;
        }

        public override void CalculateCondition()
        {
          //  Condition = 9;
        }

        public override void SetObjectOnSceneParams()
        {
            return;
        }
    }
}
