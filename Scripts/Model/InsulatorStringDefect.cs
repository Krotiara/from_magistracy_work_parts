using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace CableWalker.Simulator.Model
{
    public class InsulatorStringDefect : Defect
    {
       
       
        

        public Vector3 LocalPosition { get; private set; }
       

        public string DescriptionByTypeRus { get; private set; }
        public string DescriptionByTypeEn { get; private set; }

        public Tower Tower { get; private set; }


        

        public InsulatorStringDefect(InsulatorString strModel, string description, Vector3 localPosition,
            string photoPath, string number, string descriptionByTypeRus, string descriptionByTypeEn, Tower tower, string tag, string typeNumberFromDataBase, List<string> args)
        {
            Model = strModel;
            Description = description;
            DescriptionByTypeRus = descriptionByTypeRus;
            DescriptionByTypeEn = descriptionByTypeEn;
            LocalPosition = localPosition;
            PhotoPath = photoPath;
            Number = number;
            if (Model != null)
            {
                Model.Defects.Add(this);
                //InsulatorString.Tower.Defects.Add(this);
            }
            Tower = tower;
            Tag = tag;
            TypeFromDataBase = typeNumberFromDataBase;
            ArgsFromFiles = args;
        }

        public InsulatorStringDefect()
        {
        } 

        public override Model Create(List<string> args, List<string> typeArgs,
            InformationHolder infoHolder, bool isEditorMode)
        {
            var number = args[0];
            string towerNumber = args[1];
            string stringNumber = args[2]; // Пока что будет номер подвеса //wtf
            string description = args[3];
            string photoPath = args[4];
            //Vector3 localPosition = new Vector3(float.Parse(args[5]), float.Parse(args[6]), float.Parse(args[7]));
            string descriptionByTypeRus = typeArgs[0];
            string descriptionByTypeEn = typeArgs[1];
            string typeNumberFromDataBase = typeArgs[2];
            Tower tower;
            InsulatorString insulatorString = infoHolder.Get<InsulatorString>(stringNumber);
            tower = infoHolder.Get<Tower>(towerNumber);
            if (tower == null)
                throw new System.Exception("Incorrect StringDefects config: towerNum is null");

            var tag = infoHolder.GetTag(typeof(InsulatorStringDefect));

            return new InsulatorStringDefect(insulatorString, description, new Vector3(),
                photoPath, number, descriptionByTypeRus, descriptionByTypeEn, tower,tag, typeNumberFromDataBase, args.Skip(5).ToList());
        }

#if UNITY_EDITOR
        public override GameObject Instantiate()
        {
            GameObject parentObj = Model == null ? Tower.ObjectOnScene : Model.ObjectOnScene;
            Transform parent = parentObj.transform.Find("Defects");
            var prefab = Resources.Load<GameObject>("Prefabs/DefectLabel");
            ObjectOnScene = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            ObjectOnScene.transform.SetParent(parent, true);
            ObjectOnScene.name = Number;
            //ObjectOnScene.transform.position = insulatorString == null ? SetLocalPosition() : insulatorString.Position; //надо координаты
            ObjectOnScene.transform.position = SetLocalPosition();
            ObjectOnScene.tag = Tag;
            var indexHolder = ObjectOnScene.AddComponent<IndexHolder>();
            indexHolder.type = GetType().ToString();
            indexHolder.index = Number;
            indexHolder.photoPath = PhotoPath;
            ObjectOnScene.gameObject.layer = LayerMask.NameToLayer("PowerLineObjects");
            return ObjectOnScene;
        }

        public override GameObject Instantiate2D()
        {
            return null;
        }
#endif

       

        public override void CalculateCondition()
        {
            throw new System.NotImplementedException();
        }

        private Vector3 SetLocalPosition()
        {
            var parent = ObjectOnScene.transform.parent;
            ObjectOnScene.transform.parent = parent.parent.parent;
            ObjectOnScene.transform.position = new Vector3();
            return new Vector3();

            //var defectOffset = 0.7f;
            //var localPosition = new Vector3(defectOffset,
            //    Tower.ObjectOnScene.GetComponent<BoxCollider>().size.y + defectOffset, 0);
            //return localPosition;
        }

        public override List<(string, string)> GetInfo()
        {
            string stringNum = Model == null ? System.String.Empty : Model.Number;
            return new List<(string, string)>
            {
                ("Number",Number),
                ("Tower",Tower.Number),
                ("Insulator", stringNum),
                ("Type",DescriptionByType),
                ("Description",Description)
            };
        }

        public override List<string> GetCellsNamesToTable()
        {
            var result = new List<string>
            {
                "ID",
                "Tower Number",
                "String Number",
                "DescriptionByType",
                "Description"
            };
            return result;
        }

        public override List<string> GetInfoForTable()
        {
            return new List<string> { Number, DescriptionByType, "" };
        }

        public override void SetObjectOnSceneParams()
        {
            return;
        }
    }
}
