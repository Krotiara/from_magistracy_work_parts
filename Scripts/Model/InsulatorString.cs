using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace CableWalker.Simulator.Model
{
    public class InsulatorString : Model
    {
        public Vector3 LocalPosition { get; private set; }
       
        public string InsulatorStringNumber { get; private set; }
        public string TypeForCable { get; private set; }
        public string TypeForPosition { get; private set; }
        public string InsulatorStringName { get; private set; }
        public float InsulatorStringLength { get; private set; }
        public Tower Tower { get; private set; }
        public string InsStringRelativeNumber { get; private set; }

        public InsulatorString RelativeInsulatorString { get; private set; }
        public Transform CablePoint { get;  set; }

        public bool IsAlreadyRotated { get; private set; }

        public bool IsForGroundCable { get { return (TypeForCable == ((int)TypesForCable.ForGroundCable).ToString()); } }

        public bool IsForCable { get { return (TypeForCable == ((int)TypesForCable.ForCable).ToString()); } }

        //public bool IsIntermediateOnAnchorTower { get { return (TypeForPosition == ((int)TypesForCablePosition.IntermediateOnAnchor).ToString()); } }

        public bool IsIntermediate => TypeForPosition == ((int)TypesForCablePosition.Intermediate).ToString();
        public bool IsAnchor => TypeForPosition == ((int)TypesForCablePosition.Anchor).ToString();
        private bool OnAnchor => TypeForPosition == ((int)TypesForCablePosition.IntermediateOnAnchor).ToString();

        private string prefabPath;

        
   
        public SuspensionKind Kind { get; private set; }

        public InsulatorString() { }

        public InsulatorString(Vector3 localPosition, string number, string typeForCable, string typeForPosition, string cypher, string relativeNumber, float length, Tower tower, string prefabPath, string tag)
        {
            LocalPosition = localPosition;
            Position = localPosition + tower.Position;
            InsulatorStringNumber = number;
            Number = InsulatorStringNumber;
            Name = Number.Split('.')[1];
            TypeForCable = typeForCable;
            TypeForPosition = typeForPosition;
            InsulatorStringName = cypher;
            InsulatorStringLength = length;
            Tower = tower;
            this.prefabPath = prefabPath;
            InsStringRelativeNumber = relativeNumber;
            Defects = new List<Defect>();
            Tag = tag;
            Kind = ModelCypherParams.GetSuspensionParams(cypher);
        }

        public void SetRelative(InsulatorString relativeInsString)
        {
            RelativeInsulatorString = relativeInsString;
        }

        public override Model Create(List<string> args, List<string> typeArgs, InformationHolder infoHolder, bool isEditorMode)
        {
            string stringNumber = args[0];
            string towerNumber = args[1];
            double mountNumber = double.Parse(args[2], CultureInfo.InvariantCulture);
            
            Vector3 localPosition = new Vector3(float.Parse(args[3], CultureInfo.InvariantCulture), float.Parse(args[4], CultureInfo.InvariantCulture), float.Parse(args[5], CultureInfo.InvariantCulture)) / 1000;   //входные данные в миллиметрах
            
            float stringLength = float.Parse(args[6], CultureInfo.InvariantCulture) / 1000;
            string relativeInsStringNumber = args[7];
            var cypher = typeArgs[0];
            var typeForCable = typeArgs[1];
            var typeForPosition = typeArgs[2];
            var prefabPath = typeArgs[3];
            Tower tower = infoHolder.Get<Tower>(towerNumber);
            if (tower == null)
                throw new System.Exception("StringsConfig is Incorrect");
            var tag = infoHolder.GetTag(typeof(InsulatorString));
            var strResult = new InsulatorString(localPosition, stringNumber,
                typeForCable, typeForPosition, cypher,
                relativeInsStringNumber, stringLength, tower, prefabPath, tag);
            tower.InsulatorStrings.Add(strResult);
            
            return strResult;
        }

#if UNITY_EDITOR
        public override GameObject Instantiate()
        {
            //SceneParser.SceneParser.CheckRegeneration(Number);
            var prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
            var str = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            str.transform.SetParent(Tower.ObjectOnScene.transform, false);
            str.transform.localPosition = LocalPosition;
            // Model = GameObject.Instantiate(Model);
            str.transform.position = Position;
            //str.transform.RotateAround(
            //Tower.ObjectOnScene.transform.position,
            //new Vector3(0.0f, 1.0f, 0.0f),
            //Tower.ObjectOnScene.transform.rotation.eulerAngles.y);
            //str.transform.SetParent(Tower.ObjectOnScene.transform, true);
            CablePoint = str.transform.Find("Cable");
            str.name = Number;
            ObjectOnScene = str;
            ObjectOnScene.tag = Tag;
            var indexHolder = ObjectOnScene.AddComponent<IndexHolder>();
            indexHolder.type = GetType().ToString();
            indexHolder.index = Number;
            this.CreateInsulatorStringCollider();
            ObjectOnScene.gameObject.layer = LayerMask.NameToLayer("PowerLineObjects");
            ObjectOnScene.AddComponent<Obstacle>();
            return ObjectOnScene;
        }

        public override GameObject Instantiate2D()
        {
            //var prefab = AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/2DScene/InsulatorStringLabel.prefab", typeof(GameObject)) as GameObject;
            var prefab = AssetDatabase.LoadAssetAtPath("Assets/Resources/Prefabs/2DScene/InsulatorStringLabelSimple.prefab", typeof(GameObject)) as GameObject;
            if (prefab == null) throw new System.Exception($"Prefab in prefab path {prefabPath} doesnt exsist");
            var str = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            str.transform.SetParent(Tower.ObjectOnScene.transform, false);
            int index = Convert.ToInt32(Number.Split('.')[1]);
            float x = LocalPosition.x;
            float z = 0;
            if (IsAnchor || OnAnchor)
            {
                z = index < 4 ? 3f : -3f;
                if (index > 6)
                {
                    x = index % 2 == 0 ? -3.6f : 1.5f;
                        z = 1.5f;
                    if (index > 8) z = -1.5f;
                }

            }
            //var z = IsAnchor || OnAnchor ? ((index < 4) || ((index > 6) && (index < 9)) ? 3f : -3f) : 0f;
            str.transform.localPosition = new Vector3(x, 0.5f, Tower.ObjectOnScene.transform.position.z + z);
            str.transform.position = new Vector3(Tower.ObjectOnScene.transform.position.x + x, 0.5f, Tower.ObjectOnScene.transform.position.z + z);

            LocalPosition = str.transform.localPosition;
            Position = str.transform.position;

            str.transform.RotateAround(
            Tower.ObjectOnScene.transform.position,
            new Vector3(0.0f, 1.0f, 0.0f),
            Tower.ObjectOnScene.transform.rotation.eulerAngles.y);
            CablePoint = str.transform;
            str.name = Number;
            ObjectOnScene = str;
            ObjectOnScene.tag = Tag;
            Position = ObjectOnScene.transform.position;
            var indexHolder = ObjectOnScene.AddComponent<IndexHolder>();
            indexHolder.type = GetType().ToString();
            indexHolder.index = Number;

            //str.GetComponentInChildren<TextMeshPro>().text = Number.Split('.')[1];

            ObjectOnScene.gameObject.layer = LayerMask.NameToLayer("PowerLineObjects");
            return ObjectOnScene;
        }
#endif

        private void CreateInsulatorStringCollider()
        {
            float savedDistance = 4;
            var newCollider = ObjectOnScene.AddComponent<BoxCollider>();
            newCollider.size = new Vector3(2 * InsulatorStringLength, 2 * InsulatorStringLength, 2 * InsulatorStringLength);
        }

        public void Rotate(Vector3 vector)
        {
            ObjectOnScene.transform.Rotate(vector);
            IsAlreadyRotated = true;
        }

        public override List<(string, string)> GetInfo()
        {
            int typeForCable = int.Parse(TypeForCable, CultureInfo.InvariantCulture);
            int typeForPosition = int.Parse(TypeForPosition, CultureInfo.InvariantCulture);
            string regInfo = Kind == null ? "" : Kind.GetInfo();
            return new List<(string, string)>
            {
                ("Number", Number),
                ("Tower", Tower.Number),
                ("TypeForCable",((TypesForCable)typeForCable).ToString() ),
                ("TypeForPosition",((TypesForCablePosition)typeForPosition).ToString() ),
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
                "Номер опоры",
                "Состояние",
                "Номер подвеса на опоре",
            };
            return result;
        }

        public override List<string> GetInfoForTable()
        {
            var result = new List<string>
            {
                Tower.Number,
                Condition.ToString(),
                Number,
            };
            return result;
        }

        

        public override void SetObjectOnSceneParams()
        {
            CablePoint = ObjectOnScene.transform.Find("Cable");
        }
    }
}
