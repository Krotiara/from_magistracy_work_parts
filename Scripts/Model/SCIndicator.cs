using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator.Model
{
    public class SCIndicator : Model
    {
        

        public GameObject Prefab { get; }

        
        private GameObject objectOnScene;
        private Vector3 position;
        private Vector3 rotation;


        private string tag;

        public SCIndicator() { }

        public SCIndicator(string number, string name, string photoPath, GameObject prefab, string tag)
        {
            Number = number;
            Name = name;
            PhotoPath = photoPath;
            Prefab = prefab;
            Tag = tag;
        }

        public override Model Create(List<string> args, List<string> typeArgs, InformationHolder infoHolder, bool isEditorMode)
        {
            //Создание экземпляра класса при парсинге конфигов
            var tag = infoHolder.GetTag(typeof(SCIndicator));
            throw new System.NotImplementedException();
        }

        public override List<(string, string)> GetInfo()
        {
            throw new System.NotImplementedException();
        }
        
#if UNITY_EDITOR
        public override GameObject Instantiate()
        {
            //Инициализация 3д-модели на сцене
            GameObject clamp = GameObject.Instantiate(Prefab);
            clamp.transform.position = Position;
            clamp.transform.rotation = Quaternion.Euler(Rotation);
            clamp.transform.name = Number;
            objectOnScene = clamp;
            objectOnScene.tag = tag;
            var indexHolder = ObjectOnScene.AddComponent<IndexHolder>();
            indexHolder.type = GetType().ToString();
            indexHolder.index = Number;
            objectOnScene.gameObject.layer = LayerMask.NameToLayer("PowerLineObjects");
            return ObjectOnScene;
        }

        public override GameObject Instantiate2D()
        {
            throw new System.NotImplementedException();
        }
#endif

        public override void CalculateCondition()
        {
            throw new System.NotImplementedException();
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
