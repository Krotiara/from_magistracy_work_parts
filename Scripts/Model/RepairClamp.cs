using System.Collections.Generic;

using UnityEngine;

namespace CableWalker.Simulator.Model
{
    public class RepairClamp : Model
    {
       
       


        public GameObject Prefab { get; }
 
        public Cable Cable { get; }
       

        public RepairClamp() { }

        ///TODO: заменить CablePoint на дистанцию от первой опоры в пролете
        public RepairClamp(string number, string name, string photoPath, GameObject prefab, string tag, Cable cable, Vector3 cablePoint, Vector3 rotation)
        {
            Number = number;
            Name = name;
            PhotoPath = photoPath;
            Prefab = prefab;
            Tag = tag;
            Cable = cable;
            Position = cablePoint;
            Rotation = rotation;
        }

        public override Model Create(List<string> args, List<string> typeArgs, InformationHolder infoHolder, bool isEditorMode)
        {
            //Создание экземпляра класса при парсинге конфигов
            var tag = infoHolder.GetTag(typeof(RepairClamp));
            throw new System.NotImplementedException();
        }

        public override List<(string, string)> GetInfo()
        {
            throw new System.NotImplementedException();
        }

        //Возник, так как в билде убирается метод Instansiate
        public GameObject InstansiateSafely()
        {
            GameObject clamp = GameObject.Instantiate(Prefab);
            clamp.transform.position = Position;
            clamp.transform.rotation = Quaternion.Euler(Rotation);
            clamp.transform.name = Number;
            this.ObjectOnScene = clamp;
            ObjectOnScene.tag = Tag;
            var indexHolder = this.ObjectOnScene.AddComponent<IndexHolder>();
            indexHolder.type = GetType().ToString();
            indexHolder.index = Number;
            ObjectOnScene.gameObject.layer = LayerMask.NameToLayer("PowerLineObjects");
            return this.ObjectOnScene;
        }

#if UNITY_EDITOR

        public override GameObject Instantiate()
        {
            GameObject clamp = GameObject.Instantiate(Prefab);
            clamp.transform.position = Position;
            clamp.transform.rotation = Quaternion.Euler(Rotation);
            clamp.transform.name = Number;
            this.ObjectOnScene = clamp;
            ObjectOnScene.tag = Tag;
            var indexHolder = this.ObjectOnScene.AddComponent<IndexHolder>();
            indexHolder.type = GetType().ToString();
            indexHolder.index = Number;
            ObjectOnScene.gameObject.layer = LayerMask.NameToLayer("PowerLineObjects");
            return this.ObjectOnScene;
        }

        public override GameObject Instantiate2D()
        {
            throw new System.NotImplementedException();
        }
#endif

       

        public override List<string> GetInfoForTable()
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

        public override void SetObjectOnSceneParams()
        {
            return;
        }
    }
}
