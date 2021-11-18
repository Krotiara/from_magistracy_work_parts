using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static CableParametersCalculationsV2.CableParametersCalculationsV2;
using static CableParametersCalculationsV2Script.CableParametersCalculationsV2;


namespace CableWalker.Simulator.Model
{
    public class CableMode : ICloneable
    {
        public string Name { get; set; }
        /// <summary>
        /// Механическое напряжение в расчетном режиме в низшей точке провода [даН/мм^2 = кГ/мм^2].
        /// </summary>
        public float Stress { get; set; }
        /// <summary>
        /// Удельная нагрузка [кГс/(м*мм^2)]. кГс - киллограмм-сила
        /// </summary>
        public float SpecificLoad { get; set; }
        /// <summary>
        /// Результирующая единичная нагрузка на провод [даН/м]
        /// </summary>
        public float ResultingUnitLoad { get; set; }

        /// <summary>
        /// Температура поверхности провода [Цельсий].
        /// </summary>
        public float CableTemperature { get; set; }

        /// <summary>
        /// Наибольшее механическое напряжение провода [даН/мм^2 = кГ/мм^2].
        /// </summary>
        public float MaxStressInClamps { get; private set; }

        #region 15/03/2021
        public float GlobalMaxStress { get; private set; }
        public float BreakingForce { get; private set; }
        public float BreakingStress { get; private set; }
        public Dictionary<float, List<float>> BreakRodsPointsDict { get; } = new Dictionary<float, List<float>>(); // List = [sectionInPoint,y, na, ns, stress, breakingstress] na-ns = количество сломанных проволок.
        #endregion
        /// <summary>
        /// Наибольшее тяжение провода [даН]
        /// </summary>
        public float MaxT { get; private set; }
        /// <summary>
        /// Тяжение провода в наинизшей точке кривой провисания провода [даН]
        /// </summary>
        public float H { get; private set; }
        /// <summary>
        /// Стрела провеса  провода [м].
        /// </summary>
        public float Sag { get; private set; }
        /// <summary>
        /// Константа провисания.
        /// </summary>
        public float CatenaryConstant { get; private set; }
        /// <summary>
        /// Длина провода [м].
        /// </summary>
        public float CableLength { get; private set; }
        /// <summary>
        /// Толщина стенки гололеда в мм.
        /// </summary>
        public float IceThickness { get;  set; }


        public float delta;

        public List<Vector3> Points { get; set; }

        public bool IsLeftWindDirection { get; set; }

        public Environment Environment { get;  set; }

        public CableKind Kind { get; private set; }

        /// <summary>
        /// Ток [А].
        /// </summary>
        public float I { get; set; }

        public CableParametersCalculationsV2Script.CableParametersCalculationsV2 CableParametersCalculator { get; } = new CableParametersCalculationsV2Script.CableParametersCalculationsV2();

        //private float maxH;

        public object Clone()
        {
            return this.MemberwiseClone();
        }


        public CableMode(string name, float iceThickness, Environment e, CableKind kind, float i)
        {
            IceThickness = iceThickness;
            Name = name;
            Environment = new Environment(e.WindSpeedArea, e.IceThicknessArea);
            Environment.SetParams(e.Temperature, e.WindSpeed, e.Humidity,e.P,e.Q,e.WindDirection);
            Kind = kind;
            I = i;
        }

        public CableMode(string name, float stress, float specificLoad, float iceThickness, Environment e, CableKind kind, float i):this(name,iceThickness,e,kind,i)
        {
            
            Stress = stress;
            SpecificLoad = specificLoad;
            
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="redusedSpan"></param>
        /// <param name="Lambda"></param>
        /// <param name="E"></param>
        /// <param name="temperature">Температура окружающей среды [Цельсий].</param>
        public void CalculateDelta(float redusedSpan, float Lambda, float E)
        {
            this.delta = Stress
                - ((SpecificLoad * SpecificLoad * redusedSpan * redusedSpan * E) / (24 * Stress * Stress))
                + Lambda * E * Environment.Temperature;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="redusedSpan">Приведенный пролет анкерного участка [м].</param>
        /// <param name="initialMode">Исходный режим расчета.</param>
        /// <param name="deltaH">Разность высот точек подвеса [м].</param>
        /// <param name="spanLength">Длина пролета [м].</param>
        /// <param name="stressM">Мех.напряжение в исходном режиме [даН/мм^2 = кГ/мм^2].</param>
        /// <param name="sLoadM">Удельная нагрузка в исходном режиме [кГс/(м*мм^2)]. кГс - киллограмм-сила. </param>
        /// <param name="TcableM">Температура провода в исходном режиме [Цельсий].</param>
        /// <param name="LcableM">Длина провода в исходном режиме [м].</param>
        public void CalculateModeParams(float redusedSpan, float stressM, float sLoadM, float TcableM, float LcableM, float deltaH, float spanLength, bool isFirstStringAboveSecond)
        {
            ResultingUnitLoad = CableParametersCalculator.CalculateResultingUnitLoad(Kind.Diameter,Kind.Section,Kind.Weight,Environment.WindSpeed,IceThickness);
            SpecificLoad = CableParametersCalculator.GetSpecificLoad(ResultingUnitLoad, Kind.Section); /*CalculateNewSpecificLoad(Kind.Diameter, IceThickness, Environment.WindSpeed, Kind.Weight, Kind.Section);*/
            CableTemperature = CableParametersCalculator.CalculateCableTemperature(I, Environment.Temperature + 273.15f, Environment.P, Environment.Q, Kind.Diameter * 100, Kind.R0, Kind.Lambda, Environment.WindSpeed, Environment.WindDirection == WindDirection.Along);
            Stress = CableParametersCalculator.CalculateNewStress(stressM, sLoadM, TcableM, LcableM, SpecificLoad, CableTemperature, Kind.E, Kind.Lambda, redusedSpan, spanLength);

            var val1 = spanLength / 2 - (Stress * deltaH) / (SpecificLoad * spanLength);
            var val2 = spanLength / 2 + (Stress * deltaH) / (SpecificLoad * spanLength);
            var a = isFirstStringAboveSecond ? val2 : val1;
            var b = isFirstStringAboveSecond ? val1 : val2;

            var ha = (SpecificLoad * a * a) / (2 * Stress); //Ордината точки A относительно наинизшей точки провода O
            var hb = (SpecificLoad * b * b) / (2 * Stress); //Ордината точки B относительно наинизшей точки провода O

            float maxH = Mathf.Max(ha, hb);


            CableLength = CableParametersCalculator.CalculateCableLength(spanLength,SpecificLoad,Stress);
            H = CableParametersCalculator.CalculateH(Stress,Kind.Section);
            Sag = CableParametersCalculator.CalculateSag(spanLength,Stress,SpecificLoad);
            MaxStressInClamps = CableParametersCalculator.CalculateMaxStress(maxH,Stress,SpecificLoad);
            MaxT = CableParametersCalculator.CalculateMaxT(maxH,H,ResultingUnitLoad);
            CatenaryConstant = CableParametersCalculator.CalculateCatenaryConstant(H,Kind.Weight);

            BreakingForce = CableParametersCalculator.GetBreakingForceOfCable(Kind.AluminiumRodBreakingForce, Kind.NumberOfAluminiumRod, Kind.SteelRodBreakingForce, Kind.NumberOfSteelRod);
            BreakingStress = CableParametersCalculator.GetBreakingStress(BreakingForce, Kind.Section);
            CalculateStressAndBreakingStressInBreakRods();
            GlobalMaxStress = GetGlobalMaxStress();
        }

       

        public List<float> GetActualStressByHeightsВifferences(float spanLength,float deltaH, float deltaY, bool isStartAboveEnd)
        {
            ResultingUnitLoad = CableParametersCalculator.CalculateResultingUnitLoad(Kind.Diameter, Kind.Section, Kind.Weight, Environment.WindSpeed, IceThickness);
            SpecificLoad = CableParametersCalculator.GetSpecificLoad(ResultingUnitLoad, Kind.Section);
            return CableParametersCalculator.GetStressByHeightsDeltas(spanLength, deltaH, deltaY, SpecificLoad, isStartAboveEnd);
        }

        public void CalculateModeParams(float spanLength, float deltaH, bool isFirstStringAboveSecond, float actualStress)
        {
            ResultingUnitLoad = CableParametersCalculator.CalculateResultingUnitLoad(Kind.Diameter, Kind.Section, Kind.Weight, Environment.WindSpeed, IceThickness);
            CableTemperature = CableParametersCalculator.CalculateCableTemperature(I, Environment.Temperature + 273.15f, Environment.P, Environment.Q, Kind.Diameter * 100, Kind.R0, Kind.Lambda, Environment.WindSpeed, Environment.WindDirection == WindDirection.Along);
            SpecificLoad = CableParametersCalculator.GetSpecificLoad(ResultingUnitLoad, Kind.Section);
            Stress = actualStress;
            var val1 = spanLength / 2 - (Stress * deltaH) / (SpecificLoad * spanLength);
            var val2 = spanLength / 2 + (Stress * deltaH) / (SpecificLoad * spanLength);
            var a = isFirstStringAboveSecond ? val2 : val1;
            var b = isFirstStringAboveSecond ? val1 : val2;

            var ha = (SpecificLoad * a * a) / (2 * Stress); //Ордината точки A относительно наинизшей точки провода O
            var hb = (SpecificLoad * b * b) / (2 * Stress); //Ордината точки B относительно наинизшей точки провода O

            float maxH = Mathf.Max(ha, hb);

            CableLength = CableParametersCalculator.CalculateCableLength(spanLength, SpecificLoad, Stress);
            H = CableParametersCalculator.CalculateH(Stress, Kind.Section);
            Sag = CableParametersCalculator.CalculateSag(spanLength, Stress, SpecificLoad);
            MaxStressInClamps = CableParametersCalculator.CalculateMaxStress(maxH, Stress, SpecificLoad);
            MaxT = CableParametersCalculator.CalculateMaxT(maxH, H, ResultingUnitLoad);
            CatenaryConstant = CableParametersCalculator.CalculateCatenaryConstant(H, Kind.Weight);
            // public float BreakingForce => CableParametersCalculator.GetBreakingForceOfCable(Kind.AluminiumRodBreakingForce, Kind.NumberOfAluminiumRod, Kind.SteelRodBreakingForce, Kind.NumberOfSteelRod);
            //public float BreakingStress => CableParametersCalculator.GetBreakingStress(BreakingForce, Kind.Section);


            BreakingForce = CableParametersCalculator.GetBreakingForceOfCable(Kind.AluminiumRodBreakingForce, Kind.NumberOfAluminiumRod, Kind.SteelRodBreakingForce, Kind.NumberOfSteelRod);
            BreakingStress = CableParametersCalculator.GetBreakingStress(BreakingForce, Kind.Section);
            CalculateStressAndBreakingStressInBreakRods();
            GlobalMaxStress = GetGlobalMaxStress();

        }

        public void RemoveAllBreakRodsIn(float X)
        {
            if (BreakRodsPointsDict.ContainsKey(X))
            {
                BreakRodsPointsDict[X].Clear();
            }
            BreakRodsPointsDict.Remove(X);
            CalculateStressAndBreakingStressInBreakRods();
        }

        public void RemoveBreakRod(float X, RodType rodType)
        {
            if (BreakRodsPointsDict.ContainsKey(X))
            {
                if (BreakRodsPointsDict[X][2] == 1 && rodType == RodType.Aluminium && BreakRodsPointsDict[X][3] == 0
                    || BreakRodsPointsDict[X][3] == 1 && rodType == RodType.Steel && BreakRodsPointsDict[X][2] == 0)
                    BreakRodsPointsDict.Remove(X);
                else
                {
                    BreakRodsPointsDict[X][0] = rodType == RodType.Aluminium ? BreakRodsPointsDict[X][0] + Kind.AluminumRodSection : BreakRodsPointsDict[X][0] + Kind.SteelRodSection;
                    BreakRodsPointsDict[X][2] = rodType == RodType.Aluminium ? BreakRodsPointsDict[X][2] - 1 : BreakRodsPointsDict[X][2];
                    BreakRodsPointsDict[X][3] = rodType == RodType.Steel ? BreakRodsPointsDict[X][3] - 1 : BreakRodsPointsDict[X][3];
                }
                CalculateStressAndBreakingStressInBreakRods();
            }
        }

        public void AddBreakRod(float X, float y, RodType rodType)
        {
            if(BreakRodsPointsDict.ContainsKey(X))
            {
                BreakRodsPointsDict[X][0] = rodType == RodType.Aluminium ? BreakRodsPointsDict[X][0] - Kind.AluminumRodSection : BreakRodsPointsDict[X][0] - Kind.SteelRodSection;
                BreakRodsPointsDict[X][2] = rodType == RodType.Aluminium ? BreakRodsPointsDict[X][2] + 1 : BreakRodsPointsDict[X][2];
                BreakRodsPointsDict[X][3] = rodType == RodType.Steel ? BreakRodsPointsDict[X][3] + 1 : BreakRodsPointsDict[X][3];
            }
            else
            {
                float section = rodType == RodType.Aluminium ? Kind.Section - Kind.AluminumRodSection : Kind.Section - Kind.SteelRodSection;
                int na = rodType == RodType.Aluminium ? 1 : 0;
                int ns = rodType == RodType.Steel ? 1 : 0;
                BreakRodsPointsDict[X] = new List<float>() { section, y, na, ns, 0, 0 };
            }
            CalculateStressAndBreakingStressInBreakRods();
            //// List = [sectionInPoint,y, na, ns, stress, breakingstress] na-ns = количество сломанных проволок.
        }

        public List<float> GetBreakingRodsData(float X)
        {
            return BreakRodsPointsDict[X];
        }

        public float GetGlobalMaxStress()
        {
            float maxStress = MaxStressInClamps;
            foreach (KeyValuePair<float, List<float>> entry in BreakRodsPointsDict)
            {
                if (entry.Value[4] > maxStress)
                    maxStress = entry.Value[4];
            }
            return maxStress;
        }


        public void CalculateStressAndBreakingStressInBreakRods()
        {
            foreach (KeyValuePair<float, List<float>> entry in BreakRodsPointsDict)
            {
                // List = [sectionInPoint,y, na, ns, stress, breakingstress] na-ns = количество сломанных проволок.
                float stressInBreakPoint = CableParametersCalculator.GetStressInBreakRodsPoint(Stress, ResultingUnitLoad, entry.Value[0], entry.Value[1]);
                float breakStressInBreakPoint = CableParametersCalculator.GetBreakingStress(Kind.AluminiumRodBreakingForce,
                    Kind.NumberOfAluminiumRod - entry.Value[2], Kind.SteelRodBreakingForce, Kind.NumberOfSteelRod - entry.Value[3], Kind.Section);/* entry.Value[0]);*/
                entry.Value[4] = stressInBreakPoint;
                entry.Value[5] = breakStressInBreakPoint;
            }
        }

        public (float,float) GetPairStressAndBreakingStressWithMinDelta()
        {
            //List = [sectionInPoint,y, na, ns, stress, breakingstress] na-ns = количество сломанных проволок.
            //CalculateStressAndBreakingStressInBreakRods();
            float[] currentPair = new float[2] { MaxStressInClamps, BreakingStress };
            float curDelta = Mathf.Abs(MaxStressInClamps-BreakingStress);
            foreach(KeyValuePair<float, List<float>> entry in BreakRodsPointsDict)
            {
                if (entry.Value[4] < 0 || entry.Value[5] < 0) continue;
                if( Mathf.Abs(entry.Value[4] - entry.Value[5]) < curDelta)
                {
                    currentPair[0] = entry.Value[4];
                    currentPair[1] = entry.Value[5];
                    curDelta = Mathf.Abs(currentPair[0] - currentPair[1]);
                }
            }
            return (currentPair[0], currentPair[1]);
        }


        public string GetModeParamsInfo()
        {
            return 
                $"Stress - Mechanical stress at the lowest point of the cable [decaH/mm^2].\n" +
                $"\nMaxStress - Mechanical stress of the cable at the highest point of the suspension [decaH/mm^2].\n" +
                $"\nH - Cable traction at the lowest point of the cable sag curve [decaH].\n" +
                $"\nMaxT - Cable traction at the highest point of the suspension [decaH].\n" +
                $"\nSag - Cable sag [m].\n" +
                $"\nLength - Cable length [m].\n" +
                $"\nTemperature - Cable surface temperature [°C].\n";
        }

        public List<string> GetParamsNames()
        {
            //Должны совпадать с именами словаря из GetParamValueByKey

            return new List<string> { "Stress [decaH/mm^2]", "MaxStress [decaH/mm^2]", "H [decaH]", "MaxT [decaH]", "Sag [m]", "Length [m]", "Temperature [°C]" };
        }

        public List<(string,string)> GetInfo()
        {

            return new List<(string, string)>
            {
                ("Stress (daN/mm^2)",Math.Round(Stress,2).ToString()),
                ("Sag (m)",Math.Round(Sag,2).ToString()),
                 ("Cable length (m)",Math.Round(CableLength,2).ToString()),
                 ("Cable temperature (Celsium)",Math.Round(CableTemperature,2).ToString()),
                  ("H (daN)",Math.Round(H,2).ToString()),
                 ("Max stress in clamps (daN/mm^2)",Math.Round(MaxStressInClamps,2).ToString()),
                 ("Max T (daN)",Math.Round(MaxT,2).ToString()),
                 ("Catenary constant",Math.Round(CatenaryConstant,2).ToString()),
                 ("IceThickness (mm)",Math.Round(IceThickness,2).ToString()),
                  ("Specific load (daN/(m*mm^2))",Math.Round(SpecificLoad,2).ToString()),
                ("Resulting unit load (daN/m)",Math.Round(ResultingUnitLoad,2).ToString())
                // ("",Math.Round(,2).ToString()),
            };
        }

        public List<string[]> GetDetailedInfoAboutBreakRods()
        {
            var res = new List<string[]>();
            res.Add(new string[] { "Break rods" });
            res.Add(new string[] { "X", "Section in point", "Na", "Nb", "Stress", "Breaking stress" });
            foreach (float key in BreakRodsPointsDict.Keys)
            {
                ////// List = [sectionInPoint,y, na, ns, stress, breakingstress] na-ns = количество сломанных проволок.
                var breaksData = BreakRodsPointsDict[key];
                res.Add(new string[] {key.ToString(), Math.Round(breaksData[0],2).ToString(),
                breaksData[2].ToString(), breaksData[3].ToString(), Math.Round(breaksData[4],2).ToString(), Math.Round(breaksData[5],2).ToString()});
            }
            return res;
        }

        public float GetParamValueByKey(string key)
        {
            var dict = new Dictionary<string, double>
            {
                {"Stress [decaH/mm^2]", Math.Round(Stress,2) },
                {"MaxStress [decaH/mm^2]", Math.Round(MaxStressInClamps,2) },
                {"H [decaH]", Math.Round(H,2) },
                {"MaxT [decaH]", Math.Round(MaxT,2) },
                {"Sag [m]", Math.Round(Sag,2) },
                {"Length [m]", Math.Round(CableLength,2) },
                {"Temperature [°C]", Math.Round(CableTemperature,2) }
            };
            return (float)dict[key];
        }

        /// <summary>
        /// Расчитать параметры исходного режима. Не использовать в других случаях. Костыльная функция для удобства.
        /// </summary>
        /// <returns></returns>
        public List<float> CalculateInitialModeParams(float spanLength)
        {
            float TcableM = Environment.Temperature; /*needCalculateTemperature? CalculateCableTemperature(I, Environment.Temperature, Environment.P, Environment.Q, Kind.Diameter * 100, Kind.R0, Kind.Lambda, Environment.WindSpeed, Environment.WindDirection):*/
            float LcableM = CableParametersCalculator.CalculateCableLength(spanLength, SpecificLoad, Stress);
            return new List<float> { TcableM, LcableM };
            
        }

    }
}
