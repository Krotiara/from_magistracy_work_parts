using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace CableWalker.Simulator.Model
{
    public class TowerDefect : Defect
    {
        public string DefNumber { get; private set; }
        
        
        
        public Vector3 LocalPosition { get; private set; }
        
        public string DescriptionByTypeRus { get; private set; }
        public string DescriptionByTypeEn { get; private set; }

        public TowerDefect(Tower tower, string description,
            Vector3 localPosition,string photoPath, string number,
            string descriptionByTypeRus, string descriptionByTypeEn, string tag, string typeNumberFromDataBase, List<string> args)
        {
            Model = tower;
            DescriptionByTypeRus = descriptionByTypeRus;
            DescriptionByTypeEn = descriptionByTypeEn;
            DescriptionByType = DescriptionByTypeEn;
            Description = description;
            LocalPosition = localPosition;
            PhotoPath = photoPath;
            Number = number;
            Model.Defects.Add(this);
            Tag = tag;
            
            TypeFromDataBase = typeNumberFromDataBase;
            ArgsFromFiles = args;
        }

        private GameObject parentObjectOnScene;

        public TowerDefect() { }

        

        public override List<string> GetCellsNamesToTable()
        {
            var result = new List<string>();
            result.Add("ID");
            result.Add("Tower Number");
            result.Add("DescriptionByType");
            result.Add("Description");
            return result;
        }

        public override Model Create(List<string> args, List<string> typeArgs, InformationHolder infoHolder, bool isEditorMode)
        {
            var number = args[0];
            var towerNum = args[1];
            var description = args[2];
            var photoPath = args[3];
            //var localPosition = new Vector3(float.Parse(args[4]), float.Parse(args[5]), float.Parse(args[6]));
            var descriptionByTypeRus = typeArgs[0];
            var descriptionByTypeEn = typeArgs[1];
            Tower tower = infoHolder.Get<Tower>(towerNum);
            var tag = infoHolder.GetTag(typeof(TowerDefect));
            string typeNumberFromDataBase = typeArgs[2];
            return new TowerDefect(tower, description, new Vector3(), photoPath, number, descriptionByTypeRus,descriptionByTypeEn,tag, typeNumberFromDataBase,args.Skip(4).ToList());
        }
        
#if UNITY_EDITOR
        public override GameObject Instantiate()
        {
            Transform parent = Model.ObjectOnScene.transform.Find("Defects");
            var prefab = Resources.Load<GameObject>("Prefabs/DefectLabel");
            ObjectOnScene = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            ObjectOnScene.transform.SetParent(parent.transform, true);
            ObjectOnScene.name = Number;
            LocalPosition = SetLocalPosition();// tower.Position+ new Vector3(0, tower.ObjectOnScene.GetComponent<BoxCollider>().size.y, 0);
           // objectOnScene.transform.localPosition = LocalPosition; //надо координаты
            var indexHolder = ObjectOnScene.AddComponent<IndexHolder>();
            indexHolder.type = GetType().ToString();
            indexHolder.index = Number;
            indexHolder.photoPath = PhotoPath;
            ObjectOnScene.gameObject.layer = LayerMask.NameToLayer("PowerLineObjects");
            ObjectOnScene.gameObject.tag = Tag;
            return ObjectOnScene;
        }

        public override GameObject Instantiate2D()
        {
            return null;
        }
#endif
        private Vector3 SetLocalPosition()
        {
            var parent = ObjectOnScene.transform.parent;
            ObjectOnScene.transform.parent = parent.parent.parent;
            ObjectOnScene.transform.position = new Vector3();
            return new Vector3();

            //var defectOffset = 5f;
            //var localPosition = new Vector3(defectOffset, 
            //    tower.ObjectOnScene.GetComponent<BoxCollider>().size.y + defectOffset, 0);
            //return localPosition;
        }

        public override List<(string, string)> GetInfo()
        {
            return new List<(string, string)>
            {
                ("Number", DefNumber),
                ("Tower", Model.Number),
                ("Type",DescriptionByType),
                ("Description",Description)
            };
        }

        public override List<string> GetInfoForTable()
        {
            //return args from GetInfo()
            return new List<string> {DefNumber, DescriptionByType,""};
        }

        public override void CalculateCondition()
        {
            
        }

        public override void SetObjectOnSceneParams()
        {
            return;
        }
    }
}
