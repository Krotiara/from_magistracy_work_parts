using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CableWalker.Simulator.Tools.ObjectPool;
using UnityEngine;

namespace CableWalker.Simulator.Model
{
   
    public class Cable : Model
    {
        #region Properties
        public const float ShiftFromTheEnd = 5.0f;
        
        public InsulatorString Start { get; }
        public InsulatorString End { get; }
        public string Cypher { get; }
        public float SpanLength => Vector3.Distance(Start.Tower.Position, End.Tower.Position);
        public float HorizontalDistanceBetweenStrings
        {
            get
            {
                var v1 = new Vector3(End.CablePoint.position.x, 0, End.CablePoint.position.z);
                var v2 = new Vector3(Start.CablePoint.position.x, 0, Start.CablePoint.position.z);
                return (v2 - v1).magnitude;
            }
        }
        public float DeltaH => Mathf.Abs(CableStartPosition.y - CableEndPosition.y);
        public float DeltaYStart => Mathf.Abs(CableStartPosition.y - LowerstPointPosition.y);
        public float DeltaYEnd => Mathf.Abs(CableEndPosition.y - LowerstPointPosition.y);
        public float Length => CurrentMode.CableLength;
       


        public Dictionary<string, CableMode> WeatherCases { get; private set; }
        public List<string> IndexesForExtremeMode { get; private set; }
        public string CurrentModeName { get; private set; }
        public int CurrentModeIndex { get; private set; }
        public CableMode CalcMode { get; private set; }


        private Environment Environment;

        /// <summary>
        /// Содержит параметры текущего режима погодных условий.
        /// </summary>
        //public CableMode CurrentMode => WeatherCases[CurrentModeName];
        public CableMode CurrentMode { get;  set; }
        /// <summary>
        /// Содержит технические характеристики марки провода.
        /// </summary>
        public CableKind Kind { get; private set; }


        Func<float, float> f;
        Func<float, float> df;
      
        public float MinYInPointsPosition => GetMinYInPoints(CurrentMode.Points);
        public Vector3 LowestPoint => GetLowestPoint(CurrentMode);
        public string Phase { get; }

        public bool IsGroundCable => Phase != "A" && Phase != "B" && Phase != "C";

        public Dictionary<string,VibrationDamper> VibrationDampers { get; }
        public List<Vector3> Points => CurrentMode.Points;
        public List<int[]> IndexesForPrimitiveDisplay { get; private set; } 
        public List<GameObject> UsedPrimitiveObjects { get; }
        public CableDisplayMode DisplayMode { get; private set; }
        //public float Length { get { return CurrentMode.Length; } }

        private Material materialClean;
        private GameObject cleanCablePrimitive;
        private GameObject lubricatedCablePrimitive;
        private GameObject cablePrimitiveFolder;
        private Material cableColliderMaterial;
        private GameObject cableColliderPrefab;

        public string NameMaterialClean => materialClean.name;

        public Span Span { get; set; }
        public IEnumerable<(float, float)> LubricatedParts { get; private set; }

       
        private readonly float cableResolution = 0.3f;

        public Vector3 CableStartPosition { get; set; }
        public Vector3 CableEndPosition { get; set; }


        public Vector3 LowerstPointPosition { get; private set; }
        #region 09.02.2021
        public float initialStress;
        public float initialSpecificLoad;
        public float initialTCable;
        public float initialLCable;
        public float distanceToGround;
        public float distanceToGreen;
        public Line Line;
        public Line GreenLine;
        public CableMode prevMode;
        public bool IsFirstStringAboveSecond => CableStartPosition.y > CableEndPosition.y;
        #endregion


        private LineRenderer lineRendererComponent;
        private float CableDisplayWidth => Kind.Diameter;

        public bool IsLockRegenerate { get; private set; }
        
   

        public Color PhaseColor
        {
            get
            {               
                switch(Phase)
                {
                    case "":
                        return Color.white;
                    case "G1":
                        return Color.white;
                    case "G2":
                        return Color.white;
                    case "G":
                        return Color.white;
                    case "A":
                        return Color.yellow;
                    case "B":
                        return new Color(102/255f, 204/255f, 153/255f);
                    case "C":
                        return Color.red;
                }
                throw new Exception("There is no Color for Phase");
            }
        }


        #endregion

        #region distances

        public Dictionary<string, Vector3[]> distancesDict = new Dictionary<string, Vector3[]>()
        {
            {"ground", new Vector3[2]{ Vector3.zero, Vector3.zero } },
            { "green", new Vector3[2]{ Vector3.zero, Vector3.zero } },
            { "crossedCables", new Vector3[2]{ Vector3.zero, Vector3.zero } },
            { "roads", new Vector3[2]{ Vector3.zero, Vector3.zero } },
            { "buildings", new Vector3[2]{ Vector3.zero, Vector3.zero } }

        };
        #endregion

        public Cable() { }

        public Cable(InsulatorString start, InsulatorString end, string number, string cypher,
            string phase, string modeName, string tag, Environment e, bool isVibrationDamper)
        {
            Start = start;
            End = end;
          
            Cypher = cypher;
            Number = number;      
            Phase = phase;
            Name = Phase == string.Empty ? Number : Phase;
            Defects = new List<Defect>();
            VibrationDampers = new Dictionary<string, VibrationDamper>();
            DisplayMode = CableDisplayMode.LineRenderer;
            IndexesForPrimitiveDisplay = new List<int[]>();
            UsedPrimitiveObjects = new List<GameObject>();
            Kind = ModelCypherParams.GetCableParams(cypher);
            Environment = e;

            materialClean = (Material)Resources.Load("Materials/Cables/LineRendererCableMaterialClean", typeof(Material));
            cleanCablePrimitive = (GameObject)Resources.Load("Prefabs/CleanCablePrimitive", typeof(GameObject));
            lubricatedCablePrimitive = Resources.Load<GameObject>("Prefabs/LubricatedCablePrimitive");
            cableColliderPrefab = (GameObject)Resources.Load("Prefabs/CableColliderModel", typeof(GameObject));
            cableColliderMaterial = (Material)Resources.Load("Materials/CableCollider", typeof(Material));
            
            if (materialClean == null)
                throw new Exception("MaterialClean object is null");
            if (cleanCablePrimitive == null)
                throw new Exception("Primitive cable prefab is null");
            if (lubricatedCablePrimitive == null)
                throw new Exception("Lubricated primitive cable prefab is null");
            this.InitWeatherCases();
            var mode = WeatherCases[modeName];


            SetCurrentMode(mode); // todo - by actual
            Tag = tag;
            if(isVibrationDamper)
            {
                CreateVibrationDampers();
            }
            
        }

        #region PropertiesCalculationMethods       

        /// <summary>
        /// Расчет приведенного пролета анкерного участка
        /// </summary>
        private float CalculateReducedSpan()
        {
            //Есть зависимость от Towers
            List<float> spansLength = new List<float>();
            GetSpans(spansLength, Span);
            float sum = 0;
            float cubeSum = 0;
            foreach (var spanLength in spansLength)
            {
                sum += spanLength;
                cubeSum += Mathf.Pow(spanLength, 3);
            }
            return Mathf.Sqrt(cubeSum / sum);
        }

        /// <summary>
        /// Выбор исходного режима. Возвращает (натяжение в исходном режиме, удельная нагрузка в исходном режиме).
        /// </summary>
        /// <param name="specificLoad1"> Удельная нагрузка от собственного веса провода [кГс/(м*мм^2)]. кГс - киллограмм-сила</param>
        /// <param name="specificLoad">  Удельная нагрузка при гололеде с ветром, т.е. максимальная [кГс/(м*мм^2)]</param>
        /// /// <param name="redusedSpan">  Длина приведенного пролета анкерного участка</param>
        public CableMode SelectInitialMode(float redusedSpan, float specificLoad1, float specificLoad)
        {
            // mode_ - Исходный режим минимальных температур.
            // mode_r - Исходный режим максимальных нагрузок (гололед с ветром)  
            // mode_e -Исходный режим среднеэксплуатационных температур.

            var mode_ = new CableMode("", Kind.StressInMid, Kind.SpecificLoad1, 0, Environment, Kind,0);
            var mode_r = new CableMode("", Kind.StressInMax, specificLoad, 0, Environment, Kind,0);
            var mode_e = new CableMode("", Kind.StressInMid, Kind.SpecificLoad1, 0, Environment, Kind,0);
            mode_.Environment.SetParams(Kind.TInMin, 0, 0, 760, 0.07f, WindDirection.Perpendicular);
            mode_r.Environment.SetParams(-5, 0, 0, 760, 0.07f, WindDirection.Perpendicular);
            mode_e.Environment.SetParams(Kind.TInMiddle, 0, 0, 760, 0.07f, WindDirection.Perpendicular);
            mode_.CalculateDelta(redusedSpan, Kind.Lambda, Kind.E);
            mode_r.CalculateDelta(redusedSpan, Kind.Lambda, Kind.E);
            mode_e.CalculateDelta(redusedSpan, Kind.Lambda, Kind.E);
            var modes = new List<CableMode>() { mode_, mode_r, mode_e };
            var currentMode = modes[0];
            foreach (var mode in modes)
            {
                if (mode.delta < currentMode.delta)
                    currentMode = mode;
            }
            return currentMode;
        }


        #region 15/03/2021
        public void CalculateCalcModeParams(CableMode calcMode, Environment calcEnvironment)
        {
            var span = Span.Length;
            if (span == 0)
                throw new Exception("Towers positions are equals/ Something wrong with config");
            var redusedSpan = CalculateReducedSpan();
            var deltaH = Mathf.Abs(CableStartPosition.y - CableEndPosition.y);
            

            CalcMode = calcMode;
            CalcMode.Environment = calcEnvironment;
            CalcMode.CalculateModeParams(redusedSpan, CurrentMode.Stress, CurrentMode.SpecificLoad,
                CurrentMode.CableTemperature, CurrentMode.CableLength, deltaH, SpanLength, IsFirstStringAboveSecond);
            RecalculateCable(CalcMode);
        }

        public void ReturnStartCableMode()
        {
            RecalculateCable(CurrentMode);
        }


        #endregion

        public void CalculateModeParams(CableMode calcMode,float factStress, float factSpecificLoad, float factTCable, float factLCable)
        {
            var span = Span.Length;
            if (span == 0) throw new Exception("Towers positions are equals/ Something wrong with config");

            var redusedSpan = CalculateReducedSpan();
            var deltaH = Mathf.Abs(CableStartPosition.y - CableEndPosition.y);
           
            calcMode.CalculateModeParams(redusedSpan, factStress, factSpecificLoad, factTCable, factLCable, deltaH, SpanLength, IsFirstStringAboveSecond);
        }

        /// <summary>
        /// Высчитывает параметры расчетного режима провода
        /// </summary>
        /// <param name="mode"></param>
        public void CalculateCurrentModeParams(CableMode calcMode)
        {
            var span = Span.Length;
            if (span == 0) throw new Exception("Towers positions are equals/ Something wrong with config");

            var redusedSpan = CalculateReducedSpan();
            var initialMode = SelectInitialMode(redusedSpan, Kind.SpecificLoad1, calcMode.SpecificLoad);
            //{TcableM, LcableM}
            var list = initialMode.CalculateInitialModeParams(SpanLength);
            float TcableM = list[0]; //За температуру провода в исходном режиме принимается установленная температура окружающей среды в исходном режиме.
            float LcableM = list[1];
            
            var deltaH = Mathf.Abs(CableStartPosition.y - CableEndPosition.y);
           
            calcMode.CalculateModeParams(redusedSpan, initialMode.Stress,initialMode.SpecificLoad,TcableM,LcableM, deltaH, SpanLength, IsFirstStringAboveSecond);

            //CalcMode = new CableMode($"Calculation mode", 0, CurrentMode.Environment, CurrentMode.Kind, 0);
            CalcMode = (CableMode)CurrentMode.Clone();
            CalcMode.Name = "Calculation mode";
        }
        #endregion

        #region CablePointsCalculation


        /// <param name="stress"> Механическое напряжение в расчетном режиме [даН/мм^2 = кГ/мм^2].</param>
        /// <param name="specificLoad"> Общая удельная нагрузка [кГс/(м*мм^2)].</param>
        /// <param name="windSpeed"> Скорость ветра [м/c].</param>
        /// <param name="iceT"> Толщина стенки гололеда [мм].</param>
        public List<Vector3> CalculateCablePoints(CableMode mode)
        {
            try
            {
                return Start.Tower.Number == End.Tower.Number ?
                    CalculateCablePointsOnOneTower() : CalculateNewCablePoints(mode); /*stress,specificLoad);*/
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return new List<Vector3>();
            }
        }

        /// <summary>
        /// Получить точки провода, у которого подвески на одной опоре
        /// </summary>
        /// <returns></returns>
        private List<Vector3> CalculateCablePointsOnOneTower()
        {
            return new List<Vector3>
            {
                CableStartPosition,
                ((CableStartPosition + CableEndPosition) / 2) + new Vector3(0, -0.5f, 0),
                CableEndPosition
            };
        }

        /// <summary>
        /// Расчет новых точек провода в расчетном режиме.
        /// </summary>
        /// <param name="stress"> Механическое напряжение в расчетном режиме [даН/мм^2 = кГ/мм^2].</param>
        /// <param name="specificLoad"> Общая удельная нагрузка [кГс/(м*мм^2)].</param>
        /// <param name="windSpeed"> Скорость ветра [м/c].</param>
        /// <param name="iceT"> Толщина стенки гололеда [мм].</param>

        public List<Vector3> CalculateNewCablePoints(CableMode mode) /*float stress, float specificLoad)*/
        {
            if (Span.Length == 0) throw new Exception("Towers positions are equals/ Something wrong with config");
            bool isNeedRotateByWind = mode.Environment.WindSpeed > 0;
            var deltaH0 = Mathf.Abs(CableStartPosition.y - CableEndPosition.y);
            var span = mode.Environment.WindSpeed == 0 ? HorizontalDistanceBetweenStrings : HorizontalDistanceBetweenStrings /*mode.GetNewSpanLength(mode.Environment.WindSpeed, mode.IceThickness, HorizontalDistanceBetweenStrings, deltaH0, Kind.Weight)*/;
            var deltaH = mode.Environment.WindSpeed == 0 ? deltaH0 : deltaH0 /*mode.GetNewDeltaH(mode.Environment.WindSpeed, mode.IceThickness, deltaH0, Kind.Weight)*/;
            var val1 = span / 2 - (mode.Stress * deltaH) / (mode.SpecificLoad * span);
            var val2 = span / 2 + (mode.Stress * deltaH) / (mode.SpecificLoad * span);
            var a = CableStartPosition.y > CableEndPosition.y ? val2 : val1;
            var b = CableStartPosition.y > CableEndPosition.y ? val1 : val2;

            var ha = (mode.SpecificLoad * a * a) / (2 * mode.Stress); //Ордината точки A относительно наинизшей точки провода O

            var yO = CableStartPosition.y - ha; //Ордината точки O относительно координат Unity
            var points = new List<Vector3>();

            var nPoints = (int)(span / cableResolution + 1);
            var h = 1.0f / nPoints;
            for (float t = 0; t <= 1; t += h)
            {
                var arg = -a * (1 - t) + t * b;
                var point = new Vector3(GetXt(t), yO + (mode.SpecificLoad * (arg * arg)) / (2 * mode.Stress), GetZt(t));
                //if (isNeedRotateByWind)
                //{
                //    point = RotatePointByWind(mode, point, GetPointOnLine(t));
                //}
                points.Add(point);
            }

            return points;
        }
        #endregion

        #region UnityGenerationMethods

#if UNITY_EDITOR

        private bool IsStartEndTowersEquals()
        {
            return Start.Tower.Number == End.Tower.Number || Start.Tower.Position == End.Tower.Position;
        }

        public override GameObject Instantiate()
        {
            //if (!(Span.FirstTower.Number == Span.SecondTower.Number || Span.FirstTower.Position == Span.SecondTower.Position))
            CableStartPosition = Start.CablePoint.position;
            CableEndPosition = End.CablePoint.position;
            if (!IsStartEndTowersEquals())
                CalculateCurrentModeParams(CurrentMode);
            InstansiateCablePoints(CurrentMode);
            ObjectOnScene = new GameObject(Number);
            ObjectOnScene.AddComponent<MeshFilter>();
            ObjectOnScene.AddComponent<MeshRenderer>();
            ObjectOnScene.AddComponent<MeshCollider>();
            ObjectOnScene.tag = Tag;
            var defectsObject = new GameObject("Defects");
            defectsObject.transform.parent = ObjectOnScene.transform;
            lineRendererComponent = ObjectOnScene.AddComponent<LineRenderer>();
            ObjectOnScene.transform.SetParent(GameObject.FindGameObjectWithTag("CablesLinesFolder").transform);
            //ObjectOnScene.transform.position = (cableStartPosition + cableEndPosition) / 2;
            ObjectOnScene.transform.localPosition = Vector3.zero;
            cablePrimitiveFolder = new GameObject("PrimitiveParts");
            cablePrimitiveFolder.layer = LayerMask.NameToLayer("Ignore Raycast");
            cablePrimitiveFolder.transform.parent = ObjectOnScene.transform;
            ObjectOnScene.gameObject.layer = LayerMask.NameToLayer("PowerLineObjects");
            var indexHolder = ObjectOnScene.AddComponent<IndexHolder>();
            indexHolder.type = GetType().ToString();
            indexHolder.index = Number;

            CreateLineRenderer(lineRendererComponent, CurrentMode.Points, materialClean);
            DisplayMode = CableDisplayMode.LineRenderer;

            CreateCableCollider(CurrentMode.Points, ObjectOnScene, Kind.Diameter + 0.2f);
            if (Start.Tower.Number != End.Tower.Number)
            {
                CreateObstacleCollider(1,CurrentMode);
            }
            return ObjectOnScene;
        }

        public override GameObject Instantiate2D()
        {
            ObjectOnScene = new GameObject(Number);
            ObjectOnScene.AddComponent<MeshFilter>();
            ObjectOnScene.AddComponent<MeshRenderer>();
            ObjectOnScene.AddComponent<MeshCollider>();
            ObjectOnScene.tag = Tag;

            lineRendererComponent = ObjectOnScene.AddComponent<LineRenderer>();
            var points = new List<Vector3>();
            points.Add(new Vector3(Start.ObjectOnScene.transform.position.x, 0.1f, Start.ObjectOnScene.transform.position.z));
            points.Add(new Vector3(End.ObjectOnScene.transform.position.x, 0.1f, End.ObjectOnScene.transform.position.z));
            CurrentMode.Points = points;

            //Material mat = (Material)Resources.Load("Materials/PhaseMaterial" + Phase, typeof(Material));
            //mat = mat != null ? mat : (Material)Resources.Load("Materials/PhaseMaterial", typeof(Material));

            Material mat = (Material)Resources.Load("Materials/PhaseMaterial", typeof(Material));

            DisplayMode = CableDisplayMode.LineRenderer;
            CreateLineRenderer(lineRendererComponent, CurrentMode.Points, mat);
            CreateCableCollider(CurrentMode.Points, ObjectOnScene, Kind.Diameter + 1f);
            lineRendererComponent.startWidth = 0.7f;
            lineRendererComponent.endWidth = 0.7f;

            ObjectOnScene.transform.SetParent(GameObject.FindGameObjectWithTag("CablesLinesFolder").transform);
            ObjectOnScene.transform.localPosition = Vector3.zero;

            ObjectOnScene.gameObject.layer = LayerMask.NameToLayer("PowerLineObjects");
            var indexHolder = ObjectOnScene.AddComponent<IndexHolder>();
            indexHolder.type = GetType().ToString();
            indexHolder.index = Number;

            if (Start.IsAnchor)
                Start.ObjectOnScene.transform.localPosition += new Vector3(0, 0, -0.1f);

            return ObjectOnScene;
        }
#endif

        public void LockRegenerate()
        {
            Regenerate(CableDisplayMode.LineRenderer);
            IsLockRegenerate = true;
        }

        public void UnlockRegenerate()
        {
            IsLockRegenerate = false;
        }

        
        public void InstansiateCablePoints(CableMode mode)
        {
            //if (Span.FirstTower.Number == Span.SecondTower.Number || Span.FirstTower.Position == Span.SecondTower.Position)
            if(IsStartEndTowersEquals())
            {
                mode.Points = CalculateCablePointsOnOneTower();
            }
            else
            {
                mode.Points = CalculateCablePoints(mode);
            }
            IndexesForPrimitiveDisplay = SetIndexesForPrimitiveDisplay(0.1f);
        }

        public void CreateCableCollider(List<Vector3> cablePoints, GameObject cable, float diameter)
        {
            if (IsStartEndTowersEquals()) 
            {
                return;
            }

            //if(cablePoints == null)
            //{
            //    Debug.Log(string.Format("Fuck {0}", Number));
            //    return;
                
            //}

            var cableSegmentsPoints = GetSegmentsPoints(cablePoints);
            var vertices = GenerateVertices(cableSegmentsPoints, diameter /*/ 2*/);
            var triangles = GenerateTriangles(vertices);
            Mesh mesh = new Mesh();
            mesh.name = "CableMesh_" + cable.name;
            cable.GetComponent<MeshFilter>().mesh = mesh;
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            try
            {
                cable.GetComponent<MeshCollider>().sharedMesh = mesh;
                cable.GetComponent<MeshRenderer>().material = cableColliderMaterial;
                cable.GetComponent<MeshRenderer>().enabled = false;
            }
            catch (Exception exp)
            {
                Debug.Log(exp.Message);
            }
        }

        private void CreateObstacleCollider(float permissibleDistToLiveParts, CableMode cableMode)
        {
            var savedDistance = 4;
            var middlePoint = (Start.CablePoint.position + End.CablePoint.position) / 2;
            var middleVerticalY = (middlePoint.y + GetLowestPoint(cableMode).y) / 2;
            var centerCollider = new Vector3(middlePoint.x, middleVerticalY, middlePoint.z);
            var size = new Vector3(2 * permissibleDistToLiveParts, 2 * permissibleDistToLiveParts + savedDistance, (End.CablePoint.position - Start.CablePoint.position).magnitude);
            Transform findA = ObjectOnScene.transform.Find("CableObstacle");
            if (findA != null)
                GameObject.DestroyImmediate(findA.gameObject);
            var a = new GameObject();
            a.transform.rotation = Quaternion.LookRotation(End.CablePoint.position - Start.CablePoint.position);
            var box = a.AddComponent<BoxCollider>();
            a.transform.position = centerCollider;
            box.size = size;
            a.transform.parent = ObjectOnScene.transform;
            a.tag = "ObstacleBoxCollider";
            a.AddComponent<Obstacle>();
            a.gameObject.layer = LayerMask.NameToLayer("PowerLineObjects");
            a.name = "CableObstacle";
            box.enabled = false;
        }

        private List<Vector3> GetSegmentsPoints(List<Vector3> cablePoints)
        {
            var result = new List<Vector3>();
            var segment = new int[2];
            int currentIndex = 0;
            segment = GetCableSegment(currentIndex, cablePoints, 0.02f);
            result.Add(cablePoints[segment[0]]);
            result.Add(cablePoints[segment[1]]);
            currentIndex = segment[1];
            while (currentIndex != cablePoints.Count - 1)
            {
                segment = GetCableSegment(currentIndex, cablePoints, 0.02f);
                result.Add(cablePoints[segment[1]]);
                currentIndex = segment[1]; // end = new start
            }
            return result;
        }

        private Vector3[] GenerateVertices(List<Vector3> segmentsPoints, float radius)
        {
            int nbVerticesPerLevel = 8; //Пока что не менять, иначе полетит GenerateTriangles
            var result = new List<Vector3>();
            var a = new GameObject();
            var stepAngle = 360 / nbVerticesPerLevel;
            for (int i = 0; i < segmentsPoints.Count; i++)
            {
                a.transform.position = segmentsPoints[i];
                var dir = i == segmentsPoints.Count - 1 ? segmentsPoints[i] - segmentsPoints[i - 1] : segmentsPoints[i + 1] - segmentsPoints[i];
                a.transform.rotation = Quaternion.LookRotation(dir);

                for (int j = 0; j < nbVerticesPerLevel; j++)
                {
                    result.Add(segmentsPoints[i] + a.transform.right * radius);
                    a.transform.Rotate(0, 0, stepAngle, Space.Self);

                }
            }
            GameObject.DestroyImmediate(a);
            return result.ToArray();
        }

        /// <summary>
        /// nbVerticesPerLevel забито на 8 пока что (16.12.2019)
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        private int[] GenerateTriangles(Vector3[] vertices)
        {
            if (vertices.Length % 8 != 0)
            {
                Debug.Log($"Wrong vertices number, must be divided into {8}");
                return null;
            }
            int nbLevelsCount = vertices.Length / 8;
            var triangles = new List<int>();
            for (int i = 0; i < nbLevelsCount - 1; i++) //iterate over the different Level (1 lvl = nbVerticesPerLevel vertices)
            {
                for (int j = 0; j < 7; j++) //iterate over the vertices from 0 to nbVerticesPerLevel-1
                {
                    // triangle 1 :
                    triangles.Add(i * 8 + j + 1);
                    triangles.Add(i * 8 + j);
                    triangles.Add(i * 8 + j + 8);

                    // triangle 2:
                    triangles.Add(i * 8 + j + 9);
                    triangles.Add(i * 8 + j + 1);
                    triangles.Add(i * 8 + j + 8);
                }
                //create the last 2 triangles of the lvl (vertex 7)
                // triangle 1 :
                triangles.Add(i * 8 + 0);
                triangles.Add(i * 8 + 7);
                triangles.Add(i * 8 + 15);

                // triangle 2:
                triangles.Add(i * 8 + 8);
                triangles.Add(i * 8 + 0);
                triangles.Add(i * 8 + 15);
            }
            return triangles.ToArray();
        }


        private static int[] GetCableSegment(int pointIndex, List<Vector3> points, float errorAngle)
        {
            //var errorAngle = 0.75f;
            var result = new int[2];
            result[0] = pointIndex;
            var endIndex = pointIndex;
            for (var i = pointIndex; i < points.Count; i += 1)
            {
                if (Math.Abs(points[pointIndex].y - points[i].y) <= errorAngle)
                {
                    if (i < points.Count)
                    {
                        endIndex = i;
                    }
                    else
                    {
                        result[1] = points.Count - 1;
                        return result;
                    }
                }
                else
                {
                    result[1] = i;
                    return result;
                }
            }
            result[1] = endIndex;
            return result;
        }

        private List<int[]> SetIndexesForPrimitiveDisplay(float errorAngle)
        {
            var result = new List<int[]>();
            if (CurrentMode.Points.Count == 0) return result;
            var currentIndex = 0;
            while (currentIndex != CurrentMode.Points.Count - 1)
            {
                var segment = GetCableSegment(currentIndex, errorAngle);
                result.Add(segment);
                currentIndex = segment[1];
            }
            return result;
        }

        private void CorrectIndexesPrimDisplay(int[] newPart)
        {
            //TODO- будет необходимо для корректировки отображения при смазке
        }

        private void CorrectIndexesPrimDisplay(int currentIndex)
        {
            //TODO- будет необходимо для корректировки отображения при смазке
        }

        private void CreateLineRenderer(LineRenderer component, List<Vector3> points, Material mat)
        {
            InitLineRenderer(component, mat);
            component.GetComponent<Renderer>().enabled = true;
            component.positionCount = points.Count;
            for (var i = 0; i < points.Count; i++)
                component.SetPosition(i, points[i]);
        }

       
        //private void CreateLineRenderer()
        //{
        //    CreateLineRenderer(objectOnScene.GetComponent<LineRenderer>(), CurrentMode);
        //    DisplayMode = CableDisplayMode.LineRenderer;
        //}

        private void InitLineRenderer(LineRenderer line, Material mat)
        {
            SetLineRendererWidth(line, CableDisplayWidth);
            line.positionCount = 800;
            line.material = mat == null ? materialClean : mat;
        }

        public void SetLineRendererWidth(LineRenderer line, float width)
        {
            line.startWidth = width;
            line.endWidth = width;
        }


        /// <summary>
        /// Создает для переданного режима расчета кривую провисания провода на сцене
        /// </summary>
        public void CreateCalculationLine(LineRenderer renderer, List<Vector3> points, Material mat)
        {
            CreateLineRenderer(renderer, points, mat);
        }

        public void RegenerateInHighDetail()
        {
            IndexesForPrimitiveDisplay = SetIndexesForPrimitiveDisplay(0.01f);
            Regenerate(CableDisplayMode.Primitive);
        }

        public void RegenerateInLowDetail()
        {
            IndexesForPrimitiveDisplay = SetIndexesForPrimitiveDisplay(0.1f);
            Regenerate(CableDisplayMode.Primitive);
        }

        public void Regenerate()
        {
            Regenerate(DisplayMode);
        }

        public void Regenerate(CableDisplayMode mode)
        {
            if (IsLockRegenerate)
                return;

            foreach (var obj in UsedPrimitiveObjects)
            {
                obj.transform.eulerAngles = Vector3.zero;
                obj.GetComponent<PoolableObject>().BackToPool();
            }
            UsedPrimitiveObjects.Clear();
            if (mode == CableDisplayMode.Primitive)
                ReGenerateByPrimitive();
            else if (mode == CableDisplayMode.LineRenderer)
                RegenerateByLine();
        }

        public void InstantiateLubricatedPoint(float startT, float endT)
        {
            var start = GetPoint(startT);
            var end = GetPoint(endT);
            var lubricatedCylinder = ObjectPoolManager.Instance.GetObject(lubricatedCablePrimitive.name);
            lubricatedCylinder.GetComponent<MeshRenderer>().material =
                lubricatedCablePrimitive.GetComponent<MeshRenderer>().sharedMaterial;
            lubricatedCylinder.transform.position = start + (end - start) / 2;
            lubricatedCylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, end - start);
            lubricatedCylinder.transform.localScale =
                new Vector3(Kind.Diameter + 0.001f, (end - start).magnitude / 2.0f, Kind.Diameter + 0.001f);
            UsedPrimitiveObjects.Add(lubricatedCylinder);
        }


        private void RegenerateByLine()
        {
            DisplayMode = CableDisplayMode.LineRenderer;
            lineRendererComponent.enabled = true;
        }

        private void ReGenerateByPrimitive()
        {
            DisplayMode = CableDisplayMode.Primitive;
            lineRendererComponent.enabled = false;

            foreach (var part in IndexesForPrimitiveDisplay)
            {
                var cylinder = ObjectPoolManager.Instance.GetObject(cleanCablePrimitive.name);
                cylinder.GetComponent<MeshRenderer>().material =
                    cleanCablePrimitive.GetComponent<MeshRenderer>().sharedMaterial;
                cylinder.transform.position = (CurrentMode.Points[part[0]] + CurrentMode.Points[part[1]]) / 2;
                cylinder.transform.rotation = Quaternion.FromToRotation(Vector3.up, CurrentMode.Points[part[1]] - CurrentMode.Points[part[0]]);
                cylinder.transform.localScale =
                    new Vector3(CableDisplayWidth, (CurrentMode.Points[part[1]] - CurrentMode.Points[part[0]]).magnitude / 2.0f, CableDisplayWidth);
                UsedPrimitiveObjects.Add(cylinder);
            }
        }

        private int[] GetCableSegment(int pointIndex, float errorAngle)
        {
            var result = new int[2];
            result[0] = pointIndex;
            var endIndex = pointIndex;
            for (var i = pointIndex; i < CurrentMode.Points.Count; i += 1)
            {
                if (Math.Abs(CurrentMode.Points[pointIndex].y - CurrentMode.Points[i].y) < errorAngle)
                {
                    if (i < CurrentMode.Points.Count)
                    {
                        endIndex = i;
                    }
                    else
                    {
                        result[1] = CurrentMode.Points.Count - 1;
                        return result;
                    }
                }
                else
                {
                    result[1] = i;
                    return result;
                }
            }
            result[1] = endIndex;
            return result;
        }

        public void AddLubricatedPart(float start, float end)
        {
            if (start == end)
                return;

            var newPart = start < end ? (start, end) : (end, start);
            var parts = LubricatedParts == null ? new List<(float, float)> { newPart } : new List<(float, float)>(LubricatedParts) { newPart };
            parts.Sort();
            for (var i = 0; i < parts.Count - 1; i++)
            {
                var (partStart, partEnd) = parts[i];
                var (nextPartStart, nextPartEnd) = parts[i + 1];

                if (nextPartStart <= partEnd)
                {
                    parts[i] = (partStart, Mathf.Max(partEnd, nextPartEnd));
                    parts.RemoveAt(i + 1);
                    i--;
                }
            }

            LubricatedParts = parts;
        }


        /// <summary>
        /// 
        /// </summary>
        ///  <param name="windSpeed">Скорость ветра [м/c].</param>
        ///  <param name="iceT">Толщина стенки гололеда [мм].</param>
        /// <param name="point"></param>
        /// <param name="pivot"></param>
        /// <returns></returns>
        private Vector3 RotatePointByWind(CableMode mode, Vector3 point, Vector3 pivot)
        {
            float windSpeed = mode.Environment.WindSpeed;
            float iceT = mode.IceThickness;
            bool isLeftWindDir = mode.IsLeftWindDirection;
            var newSpanLength = mode.CableParametersCalculator.GetNewSpanLength(windSpeed,iceT, HorizontalDistanceBetweenStrings, DeltaH, Kind.Weight,Kind.Diameter);
            var ab = (End.Position - Start.Position).magnitude;
            var cosTeta = newSpanLength / ab;
            var teta = (180 / Mathf.PI) * Mathf.Acos(cosTeta); //угол поворота оси x в градусах
            var phi = mode.CableParametersCalculator.GetPhi(windSpeed, iceT, Kind.Weight,Kind.Diameter); //угол поворота оси y в градусах
            teta = isLeftWindDir ? teta : -teta;
            phi = isLeftWindDir ? phi : -phi;
            var angles = new Vector3(teta, phi, 0);
            return RotatePointAroundPivot(point, pivot, angles);
        }

        private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            var dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; //calculate rotated point
            return point;
        }

        
        #endregion

        #region UnityCablePointsMethods
        //TODO Обработка крайних точек
        public float GetNextPointTTo(float t)
        {
            var index = GetIndexByT(t);
            return GetTByIndex(index + 1);
        }

        ////TODO Обработка крайних точек
        public float GetPrevPointTTo(float t)
        {
            var index = GetIndexByT(t);
            return GetTByIndex(index - 1);
        }

        private float GetMinYInPoints(List<Vector3> points)
        {
            if (CurrentMode.Points.Count == 0) return 0; //Костыль
            var min = points[0].y;
            for (var i = 1; i < points.Count; i++)
                if (points[i].y < min) min = points[i].y;
            return min;
        }

        private Vector3 GetLowestPoint(CableMode cableMode)
        {
            var points = cableMode.Points;
            if (points.Count == 0) return Vector3.zero; //Костыль
            var min = points[0];
            for (var i = 1; i < points.Count; i++)
                if (points[i].y < min.y) min = points[i];
            return min;
        }

        public Vector3 GetPoint(float t)
        {
            return GetPoint(t, CurrentMode);
        }

        public Vector3 GetPoint(float t, CableMode mode)
        {
            if (t < 0 || t > 1)
            {
                Debug.Log($"Argument t must be in [0,1], but was {t}");
                return Vector3.zero;
            }
            var span = Span.Length;
            var deltaH = Mathf.Abs(CableStartPosition.y - CableEndPosition.y);
            var val1 = span / 2 - (mode.Stress * deltaH) / (mode.SpecificLoad * span);
            var val2 = span / 2 + (mode.Stress * deltaH) / (mode.SpecificLoad * span);
            var a = Start.Position.y > End.Position.y ? val2 : val1;
            var b = Start.Position.y > End.Position.y ? val1 : val2;
            var ha = (mode.SpecificLoad * a * a) / (2 * mode.Stress); //Ордината точки A относительно наинизшей точки провода O
            var yO = CableStartPosition.y - ha; //Ордината точки O относительно координат Unity
            var arg = -a * (1 - t) + t * b;
            return new Vector3(GetXt(t), yO + (mode.SpecificLoad * (arg * arg)) / (2 * mode.Stress), GetZt(t));
        }

        public Vector3 GetPointWithOffset(float t, CableMode mode)
        {
            if (t < 0 || t > 1)
                throw new ArgumentException($"Argument t must be in [0,1], but was {t}");
            var tShift = ShiftFromTheEnd / mode.CableLength;
            t = t * (1 - 2 * tShift) + tShift;
            return GetPoint(t, mode);

        }

        public float GetDistanceFromFirstTower(Vector3 point)
        {
            var v1 = new Vector3(Start.CablePoint.position.x, 0, Start.CablePoint.position.z);
            var v2 = new Vector3(point.x, 0, point.z);
            return (v2 - v1).magnitude;
        }

        public Vector3 GetPointRelativelyLowestByDistance(float distanceFromFirstTower, CableMode cableMode)
        {
            return GetPointByDistance(distanceFromFirstTower, cableMode) - GetLowestPoint(cableMode);
        }

        public Vector3 GetPointByDistance(float distanceFromFirstTower)
        {
            return GetPointByDistance(distanceFromFirstTower, CurrentMode);
        }

        public float GetNearestCablePointTTo(Vector3 point)
        {
            float dist = GetDistanceFromFirstTower(point);
            return GetTByDistance(dist);
        }

        public float GetTByDistance(float distanceFromFirstTower)
        {
            return GetTByPoint(GetPointByDistance(distanceFromFirstTower));
        }

        public Vector3 GetPointByDistance(float distanceFromFirstTower, CableMode mode)
        {
            return GetPoint(GetTByIndex(distanceFromFirstTower / cableResolution, mode), mode);
        }

        public Vector3 GetNearestPointTo(Vector3 pos, List<Vector3> points)
        {
            Vector3 result = points[0];
            var curDistance = (pos - result).magnitude;
            foreach (var point in points)
            {
                var d = (pos - point).magnitude;
                if (d < curDistance)
                {
                    curDistance = d;
                    result = point;
                }
            }
            return result;
        }

        public Vector3 GetNearestPointTo(Vector3 pos)
        {
            return GetNearestPointTo(pos, CurrentMode.Points);
        }

        public Vector3 GetNearestPointTo(Vector3 pos, CableMode mode)
        {
            return GetNearestPointTo(pos, mode.Points);
        }





        public int GetIndexByT(float t)
        {
            return GetIndexByT(t, CurrentMode);
        }

        public int GetIndexByT(float t, CableMode mode)
        {
            if (t < 0 || t > 1)
                throw new ArgumentException($"Argument t must be in [0,1], but was {t}");

            var distance = t * mode.CableLength;
            return (int)Mathf.Round(distance / cableResolution);
        }

        public float GetTByIndex(float index)
        {
            return GetTByIndex(index, CurrentMode);
        }

        public float GetTByPoint(Vector3 point)
        {
            int index = -1;
            for (int i = 0; i < CurrentMode.Points.Count; i++)
            {
                if ((point - CurrentMode.Points[i]).magnitude < 0.05)
                {
                    index = i;
                }
            }
            if (index == -1)
                Debug.Log("Incorrect cable point");
            return GetTByIndex(index, CurrentMode);
        }

        public float GetTByIndex(float index, CableMode mode)
        {
            var distance = index * cableResolution;
            return distance / mode.CableLength;
        }

        private float GetXt(float t)
        {
            return CableEndPosition.x * t + CableStartPosition.x * (1 - t);
        }

        private float GetZt(float t)
        {
            return CableEndPosition.z * t + CableStartPosition.z * (1 - t);
        }

        /// <summary>
        /// Получить точку на прямой, соединяющей точки подвесок провода
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private Vector3 GetPointOnLine(float t)
        {
            return new Vector3()
            {
                x = GetXt(t),
                y = CableEndPosition.y * t + CableStartPosition.y * (1 - t),
                z = GetZt(t)
            };
        }

        #endregion

        public void SetCurrentMode(string modeName)
        {
            CurrentModeName = modeName;
            CurrentModeIndex = WeatherCases.Values.ToList().FindIndex(x => x.Name == modeName);
            CurrentMode = WeatherCases[modeName];
        }

        public void SetCurrentMode(CableMode mode)
        {
            CurrentMode = mode;
            if (CalcMode == null)
            {
                //CalcMode = new CableMode($"Calculation mode", 0, CurrentMode.Environment, CurrentMode.Kind, 0);
                CalcMode = (CableMode)CurrentMode.Clone();
                CalcMode.Name = "Calculation mode";
            }
        }

        public void SetCalcMode(string modeName)
        {
            CalcMode = WeatherCases[modeName];
        }

        private void CreateVibrationDampers()
        {         
            VibrationDampers.Add(string.Format("{0}.vd1", Number), new VibrationDamper(string.Format("{0}.vd1", Number), string.Format("{0}.vd1", Number), this));
            VibrationDampers.Add(string.Format("{0}.vd2",Number), new VibrationDamper(string.Format("{0}.vd2", Number), string.Format("{0}.vd2", Number), this));
        }

        private void RecalculateCable(CableMode mode)
        {
            if (Span.Length != 0)
            {
                InstansiateCablePoints(mode);
                LineRenderer component = ObjectOnScene.GetComponent<LineRenderer>();
                component.GetComponent<Renderer>().enabled = true;
                component.positionCount = mode.Points.Count;
                for (var i = 0; i < mode.Points.Count; i++)
                    component.SetPosition(i, mode.Points[i]);
                CreateCableCollider(mode.Points, ObjectOnScene, Kind.Diameter + 0.2f);
                if (Start.Tower.Number != End.Tower.Number)
                {
                    CreateObstacleCollider(1,mode);
                }
            }
        }

        public float GetNormativeSag(CableMode mode)
        {
            return ModelCypherParams.GetNormativeCableSag(Cypher, Span.Length, mode.Environment.Temperature);
        }


        public void InitWeatherCases()
        {
            var HtMode = new CableMode($"Highest temperature.", 0, Environment, Kind, 0);
            HtMode.Environment.SetParams(Kind.TInMax, 0, 0, 760, 0.07f, WindDirection.Perpendicular);
            var LtMode = new CableMode($"Lowest temperature.", 0, Environment, Kind, 0);
            LtMode.Environment.SetParams(Kind.TInMin, 0, 0, 760, 0.07f, WindDirection.Perpendicular);
            var AtMode = new CableMode($"Average temperature.", 0, Environment, Kind, 0);
            AtMode.Environment.SetParams(Kind.TInMiddle, 0, 0, 760, 0.07f, WindDirection.Perpendicular);
            var SwwMode = new CableMode($"Sleet without wind.", Environment.AreaIceThicknessValue, Environment, Kind, 0);
            SwwMode.Environment.SetParams(-5, 0, 0, 760, 0.07f, WindDirection.Perpendicular);
            var EwMode = new CableMode($"Extreme wind.", 0, Environment, Kind, 0);
            EwMode.Environment.SetParams(-5, Environment.AreaWindSpeedValue, 0, 760, 0.07f, WindDirection.Perpendicular);

            float wind = Environment.AreaIceThicknessValue > 15 && Environment.AreaWindSpeedValue / 2 < 15 ? 15 : Environment.AreaWindSpeedValue / 2;
            var SwithwMode = new CableMode($"Sleet with wind.", Environment.AreaIceThicknessValue, Environment, Kind, 0);
            SwithwMode.Environment.SetParams(-5, wind, 0, 760, 0.07f, WindDirection.Perpendicular);

            var iMaxMode = new CableMode($"Max I value mode.", 0, Environment, Kind, Kind.MaxI);
            iMaxMode.Environment.SetParams(Kind.TInMiddle, 0, 0, 760, 0.07f, WindDirection.Perpendicular);


            //Добавить силу тока (10.06.2020)
            var customMode = new CableMode($"Custom.", 0, Environment, Kind, 0); //Здесь Environment - из Info holder с выбранными пользователем параметрами

            var actualMode = new CableMode($"Actual.", 0, Environment, Kind, 0); //Здесь Environment - из Info holder с выбранными пользователем параметрами

            WeatherCases = new Dictionary<string, CableMode>
            {
                { $"Highest temperature.", HtMode },
                { $"Lowest temperature.", LtMode},
                { $"Average temperature.", AtMode},
                { $"Sleet without wind.", SwwMode },
                { $"Extreme wind.", EwMode},
                { $"Sleet with wind.", SwithwMode},
                { $"Max I value mode.",iMaxMode},
                { $"Custom.",customMode},
                { $"Actual.", actualMode }

            };

            IndexesForExtremeMode = new List<string>() {
            $"Average temperature.",
            $"Lowest temperature.",
            $"Sleet without wind.",
            $"Sleet with wind."};
        }

        public string GetWeatherCasesInfo()
        {
            float wind = Environment.AreaIceThicknessValue > 15 && Environment.AreaWindSpeedValue / 2 < 15 ? 15 : Environment.AreaWindSpeedValue / 2;
            return 
                $"Mode 1: Highest temperature.\n" +
                $"-temperature = {Kind.TInMax} °C.\n" +
                $"\nMode 2: Lowest temperature.\n" +
                $"-temperature = {Kind.TInMin} °C.\n" +
                $"\nMode 3: Average temerature.\n" +
                $"-temperature = {Kind.TInMiddle} °C.\n" +
                $"\nMode 4: Sleet without wind.\n" +
                $"-ice = {(int)CurrentMode.Environment.IceThicknessArea} mm.\n" +
                $"\nMode 5: Extreme wind.\n" +
                $"-wind speed = {Environment.AreaWindSpeedValue} m/s.\n" +
                $"\nMode 6: Sleet with wind.\n" +
                $"-ice = {(int)CurrentMode.Environment.IceThicknessArea} mm.\n" +
                $"-wind speed = {wind} m/s.\n" +
                $"\nMode 7: Max I value mode.\n" +
                $"-I = {Kind.MaxI} A.\n" +
                $"-temperature = {Kind.TInMiddle} °C.";
        }

        public override Model Create(List<string> args, List<string> typeArgs, InformationHolder infoHolder, bool isEditorMode)
        {
            var dataBaseNumber = float.Parse(args[0], CultureInfo.InvariantCulture);
            var startStringNumber = args[1];
            var endStringNumber = args[2];  
            var phase = args[3];

            bool isVibrationDamper = false;
            try
            {
                isVibrationDamper = Convert.ToBoolean(Convert.ToInt16(args[4]));
            }
            catch(ArgumentOutOfRangeException e)
            {
                Debug.Log(e); //Костыль под старые конфиги
            }

            var cypher = typeArgs[0];

            var kind = ModelCypherParams.GetCableParams(cypher);
            InsulatorString startInsulatorString = null;
            InsulatorString endInsulatorString = null;
            if (endStringNumber == ((int)CablesNumbers.PortalNumber).ToString())
                return null; // Миникостыль для портальных проводов
            
            startInsulatorString = infoHolder.Get<InsulatorString>(startStringNumber);               
            endInsulatorString = infoHolder.Get<InsulatorString>(endStringNumber);
           
            if (startInsulatorString == null || endInsulatorString == null)
            {
                Debug.Log($"CableConfig Is Incorrect in start {startStringNumber}, end {endStringNumber}");
                throw new Exception("Incorrect Config");
            }

            string number;
            if (phase == string.Empty)
                number = startInsulatorString.Number + "-" + endInsulatorString.Number;
            else
                number = $"{startInsulatorString.Tower.Number}-{endInsulatorString.Tower.Number}.{phase}";
           
            
            var middleModeName = $"Average temperature.";
            var tag = infoHolder.GetTag(typeof(Cable));
            return new Cable(startInsulatorString, endInsulatorString, number, cypher,
                phase, middleModeName,tag,infoHolder.Environment, isVibrationDamper);
        }

        public string GetInfoInString()
        {
            return
                $"1)Cable number: {Number}\n" +
                $"2)Cypher: {Cypher}\n" +
                $"3)Span: {Span.Number}\n" +
                $"4)Phase: {Phase}\n" +
                $"5)Diameter: {Kind.Diameter} m.\n" +
                $"6)Section: {Kind.Section} mm^2\n" +
                $"7)Specific load 1: {Kind.SpecificLoad1} decaH/(m * mm^2)\n" +
                $"8)Weight: {Kind.Weight} kg/km\n" +
                $"9)Linear expansion temperature coefficient: {Kind.Lambda} 1/K\n" +
                $"10)E: {Kind.E} decaH/mm^2\n" +
                $"11)Operating Temperature Range: from {Kind.TInMin}°C to {Kind.TInMax}°C\n";
                
        }

        public override List<(string, string)> GetInfo()
        {
            return new List<(string, string)>
            {
                ("Tables values", ""),
               // ("Number", Number),
                //("Span", Span.Number),
               // ("Cypher", Cypher),
               // ("Phase", Phase),
                ("Diameter [m.]", Kind.Diameter.ToString()),
                ("Section [mm^2]", Kind.Section.ToString() ),
               // ("SpecificLoad1 [daH/(m * mm^2)]", Kind.SpecificLoad1.ToString()),
               // ("Weight [kg/km]", Kind.Weight.ToString()),
              //  ("Linear expansion temperature coefficient [1/K]", Kind.Lambda.ToString()),
               // ("E [daH/mm^2]", Kind.E.ToString())
                //("Temperature Range", $"from {Kind.TInMin}°C to {Kind.TInMax}°C")
            };
            
               
                 
        }

        public string GetInfoDistanceToGround(float x, float distance)
        {
            return string.Format("Расстояние до земли в точке провода на расстоянии {0} м от опоры {1} составляет {2} м", x, Span.FirstTower.Number, distance);
        }

        public override List<string> GetCellsNamesToTable()
        {
            var result = new List<string>
            {
                "Номер провода",
                "Шифр",
                "Пролет",
                "Фаза",
                "Состояние",
            };
            return result;
        }

        public override List<string> GetInfoForTable()
        {
            var result = new List<string>
            {

              Number,
              Cypher,
              Span.Number,
              Phase,
              Condition.ToString(),
            };
            return result;
        }

        public override void CalculateCondition()
        {
            foreach (var defect in Defects)
            {
                Condition += defect.Criticality;
            }
        }

        private void MovePrev(List<float> spansLength, (Tower, Tower) currentSpan)
        {
            if (currentSpan.Item1.PreviousTowers.Count != 0)
            {
                if (currentSpan.Item1.PreviousTowers.Count > 1 || currentSpan.Item1.IsAnchor)
                    return;
                else
                {
                    currentSpan = (currentSpan.Item1.PreviousTowers[0], currentSpan.Item1);
                    spansLength.Add(Vector3.Distance(currentSpan.Item1.Position, currentSpan.Item2.Position));
                    MovePrev(spansLength, currentSpan);
                }
            }
            else return;
        }

        private void MoveNext(List<float> spansLength, (Tower, Tower) currentSpan)
        {
            if (currentSpan.Item2.NextTowers.Count != 0)
            {
                if (currentSpan.Item2.NextTowers.Count > 1 || currentSpan.Item2.IsAnchor)
                    return;
                else
                {
                    currentSpan = (currentSpan.Item2, currentSpan.Item2.NextTowers[0]);
                    spansLength.Add(Vector3.Distance(currentSpan.Item1.Position, currentSpan.Item2.Position));
                    MoveNext(spansLength, currentSpan);
                }
            }
            else return;
        }

        private void GetSpans(List<float> spansLength, Span span)
        {
            var currentSpan = (span.FirstTower, span.SecondTower);
            spansLength.Add(Vector3.Distance(currentSpan.Item1.Position, currentSpan.Item2.Position));
            if (currentSpan.Item1.IsAnchor && currentSpan.Item2.IsAnchor)
                return;
            MovePrev(spansLength, currentSpan);
            MoveNext(spansLength, currentSpan);
        }


        #region 14/01/2021
        public void CalculateModeParamsByStressAndEnvironment(float temp, float wind, float humidity, float p, float stress, float spanLength)
        {
            Environment actualEnvironment = new Environment(Environment.WindSpeedArea, Environment.IceThicknessArea);
            actualEnvironment.SetParams(temp, wind, humidity, p, WindDirection.Perpendicular);
            var mode = new CableMode("ByActualStress", 0, actualEnvironment, Kind, 0);
            mode.Environment = actualEnvironment;
            mode.CalculateModeParams(spanLength, DeltaH, IsFirstStringAboveSecond, stress);
            WeatherCases["Actual."] = mode;
            SetCurrentMode("Actual.");

            initialStress = mode.Stress;
            initialSpecificLoad = mode.SpecificLoad;
            initialTCable = mode.CableTemperature;
            initialLCable = mode.CableLength;

        }

        public List<float> GetStressByPoints()
        {
            ///Здесь, кажется, бага, нужно как-то варьировать deltaYStart и End
            //float deltaY;
            //if (IsFirstStringAboveSecond)
            //    deltaY = DeltaYEnd;
            //else
            //    deltaY = DeltaYStart;
            float a = (new Vector3(CableStartPosition.x, 0, CableStartPosition.z) - new Vector3(LowerstPointPosition.x, 0, LowerstPointPosition.z)).magnitude;
            float b = (new Vector3(CableEndPosition.x, 0, CableEndPosition.z) - new Vector3(LowerstPointPosition.x, 0, LowerstPointPosition.z)).magnitude;
            //return CurrentMode.GetActualStressByPoints(HorizontalDistanceBetweenStrings, DeltaH, IsFirstStringAboveSecond, a, b);
            return CurrentMode.GetActualStressByHeightsВifferences(HorizontalDistanceBetweenStrings, DeltaH, DeltaYStart, IsFirstStringAboveSecond);
        }
        #endregion

        /// <summary>
        /// Высчитывает параметры расчетного режима провода. Перед этим в calcMode должна быть настроена Environment
        /// </summary>
        /// <param name="mode"></param>
        public void CalculateModeParams(CableMode calcMode, float redusedSpan)
        {
            // var redusedSpan = CalculateReducedSpan();
            //var initialMode = SelectInitialMode(HorizontalDistanceBetweenStrings, Kind.SpecificLoad1, calcMode.SpecificLoad);
            //{TcableM, LcableM}
            // var list = initialMode.CalculateInitialModeParams(HorizontalDistanceBetweenStrings);
            // float TcableM = list[0]; //За температуру провода в исходном режиме принимается установленная температура окружающей среды в исходном режиме.
            // float LcableM = list[1];
            calcMode.CalculateModeParams(redusedSpan, initialStress, initialSpecificLoad, initialTCable, initialLCable, DeltaH, HorizontalDistanceBetweenStrings, IsFirstStringAboveSecond);
        }

        public void SetPoints(Vector3 start, Vector3 lowest, Vector3 end)
        {
            CableStartPosition = start;
            CableEndPosition = end;
            LowerstPointPosition = lowest;
        }

        public void RecalculateByNewPoints(string pointCloudEnvironmentData)
        {
            try
            {
                float stress = GetStressByPoints().Where(x => x > 0).Min();
                float[] newEnvironment = pointCloudEnvironmentData.Replace(',', '.').Split(';').Select(x => float.Parse(x)).ToArray();
                CurrentMode.Environment.SetParams(newEnvironment[0], newEnvironment[1], newEnvironment[2], newEnvironment[3], WindDirection.Perpendicular);
                CalculateModeParamsByStressAndEnvironment(CurrentMode.Environment.Temperature,
                    CurrentMode.Environment.WindSpeed, CurrentMode.Environment.Humidity, CurrentMode.Environment.P, stress, SpanLength);
                ReInstantiateCable(CurrentMode);
            }
            catch(InvalidOperationException e)
            {
                return;
            }
        }

        public void ReInstantiateCable(CableMode mode)
        {
            InstansiateCablePoints(mode);
            LineRenderer component = ObjectOnScene.GetComponent<LineRenderer>();
            component.GetComponent<Renderer>().enabled = true;
            component.positionCount = mode.Points.Count;
            for (var i = 0; i < mode.Points.Count; i++)
                component.SetPosition(i, mode.Points[i]);
            //CreateCableCollider(mode.Points, objectOnScene, Kind.Diameter + 0.2f);
            //if (Start.Tower.Number != End.Tower.Number)
            //{
            //    CreateObstacleCollider(1);
            //}

        }

        public override void SetObjectOnSceneParams()
        {
            CameraFollowPoint = (Start.Position + End.Position)/ 2;
            lineRendererComponent = ObjectOnScene.GetComponent<LineRenderer>();

            if (Start.CablePoint == null)
                Start.CablePoint = Start.ObjectOnScene.transform;
            if (End.CablePoint == null)
                End.CablePoint = End.ObjectOnScene.transform;
            CableStartPosition = Start.CablePoint.position;
            CableEndPosition = End.CablePoint.position;
            Vector3[] points = new Vector3[lineRendererComponent.positionCount];
            lineRendererComponent.GetPositions(points);
            CurrentMode.Points = points.ToList();
            IndexesForPrimitiveDisplay = SetIndexesForPrimitiveDisplay(0.1f);
            if (!IsStartEndTowersEquals()) /*Span.FirstTower.Number != Span.SecondTower.Number && Span.FirstTower.Position != Span.SecondTower.Position)*/
                CalculateCurrentModeParams(CurrentMode);
        }
    }
}
