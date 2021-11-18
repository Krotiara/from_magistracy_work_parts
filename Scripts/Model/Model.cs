using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.Simulator.Model
{
    public abstract class Model
    {
        public string Number { get; set; }

        public string Name { get; set; }

        public string PhotoPath { get; set; }

        public GameObject ObjectOnScene { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 Rotation { get; set; }

        public Vector3 Size { get; set; }

        public string Tag { get; set; }

        public List<Defect> Defects { get; set; } = new List<Defect>();
        public float Condition { get; set; }

        public Vector3 CameraFollowPoint { get; set; }

        /// <summary>
        /// Метод для обработки входных данных конфига, здесь не должно быть свзяи с объектами на сцене
        /// </summary>
        /// <param name="args"></param>
        /// <param name="typeArgs"></param>
        /// <param name="infoHolder"></param>
        /// <returns></returns>
        public abstract Model Create(
            List<string> args,
            List<string> typeArgs,
            InformationHolder infoHolder, bool isEditorMode);
#if UNITY_EDITOR
        public abstract GameObject Instantiate();

        public abstract GameObject Instantiate2D();
#endif
        public abstract List<(string,string)> GetInfo();
        public abstract List<string> GetCellsNamesToTable();
        public abstract List<string> GetInfoForTable();

        public abstract void CalculateCondition();

        public void SetObjectOnScene(GameObject obj)
        {
            if (obj != null)
            {
                ObjectOnScene = obj;
                SetObjectOnSceneParams();
            }
        }

        public abstract void SetObjectOnSceneParams();
            

    }
}
