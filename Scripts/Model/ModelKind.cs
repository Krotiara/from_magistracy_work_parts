using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CableWalker.Simulator.Model
{

    public class TowerKind
    {
        public string Cypher { get; }
        public string TypicalProjectCypher { get; }
        public string AllowedCableKinds { get; }
        public int ChainNumber { get; }
        /// <summary>
        /// Напряжение ВЛ, кВ
        /// </summary>
        public int Voltage { get; }
       

        public TowerKind(string cypher, string typicalProjectCypher, string allowedCableKinds, int chainNumber, int voltage)
        {
            Cypher = cypher;
            TypicalProjectCypher = typicalProjectCypher;
            AllowedCableKinds = allowedCableKinds;
            ChainNumber = chainNumber;
            Voltage = voltage;
                     
        }

        public string GetInfo()
        {
            return $"Cypher - {Cypher};\nTypical project cypher - {TypicalProjectCypher};\nAllowed cable kinds - {AllowedCableKinds};\nChain number - {ChainNumber};\nVoltage - {Voltage}.";
        }
    }

    public class SuspensionKind
    {
        public string Cypher { get; }
        public string TypicalProjectCypher { get; }
        public string Composition { get; }

        public SuspensionKind(string cypher, string typicalProjectCypher, string composition)
        {
            Cypher = cypher;
            TypicalProjectCypher = typicalProjectCypher;
            Composition = composition;
        }

        public string GetInfo()
        {
            return $"Cypher - {Cypher};\nTypical project cypher - {TypicalProjectCypher};\nCompositon:\n {Composition}.";
        }
    }


    public class CableKind
    {
        public CableKind(string cypher, bool isGround, float diameter, float section,
            float specificLoad1, float weight, float e, float lambda,
            float tinMin, float tinMiddle, float tinMax, float stressInMid, float stressInMax, float r0, float lamdaResistance, float maxI)
        {
            Cypher = cypher;
            Diameter = diameter;
            Section = section;
            SpecificLoad1 = specificLoad1;
            Weight = weight;
            E = e;
            Lambda = lambda;
            TInMin = tinMin;
            TInMiddle = tinMiddle;
            TInMax = tinMax;
            StressInMid = stressInMid;
            StressInMax = stressInMax;
            R0 = r0;
            LamdaResistance = lamdaResistance;
            IsGroundCable = isGround;
            MaxI = maxI;
        }

        public CableKind(string cypher, bool isGround, float diameter, float section,
            float weight, float e, float lambda,
            float tinMin, float tinMiddle, float tinMax, float stressInMid, float stressInMax, float r0, float lamdaResistance, float maxI)
        {
            Cypher = cypher;
            Diameter = diameter;
            Section = section;
            SpecificLoad1 = (float)(weight*0.001/section);
            Weight = weight;
            E = e;
            Lambda = lambda;
            TInMin = tinMin;
            TInMiddle = tinMiddle;
            TInMax = tinMax;
            StressInMid = stressInMid;
            StressInMax = stressInMax;
            R0 = r0;
            LamdaResistance = lamdaResistance;
            IsGroundCable = isGround;
            MaxI = maxI;
        }

        public bool IsGroundCable { get; }

        public string Cypher { get; }
        /// <summary>
        /// Диаметр провода [м].
        /// </summary>
        public float Diameter { get; }
        /// <summary>
        /// Площадь поперечного сечения провода [мм^2]
        /// </summary>
        public float Section { get; }
        /// <summary>
        /// Удельная нагрузка от собственного веса провода [кГс/(м*мм^2)]. кГс - киллограмм-сила. (В таблице Рудика значения нужно еще умножить на 10^-3)
        /// </summary>
        public float SpecificLoad1 { get; }
        /// <summary>
        /// Расчетная масса провода [кг/км]
        /// </summary>
        public float Weight { get; }
        /// <summary>
        /// Модуль упругости провода [кГ/мм^2]
        /// </summary>
        public float E { get; }
        /// <summary>
        /// Температурный коэффициент линейного расширения. Размерность 10^-6.
        /// </summary>
        public float Lambda { get; }
        /// <summary>
        /// Проектная минимальная температура провода [Цельсий]
        /// </summary>
        public float TInMin { get; }
        /// <summary>
        /// Проектная среднеэксплуатационная температура провода [Цельсий]
        /// </summary>
        public float TInMiddle { get; }
        /// <summary>
        /// Проектная максимальная температура провода [Цельсий]
        /// </summary>
        public float TInMax { get; }
        /// <summary>
        /// Допускаемое механическое напряжение при среднегодовой температуре [даН/мм^2 = кГ/мм^2]
        /// </summary>
        public float StressInMid { get; }
        /// <summary>
        /// Допускаемое механическое напряжение при наибольшей нагрузке [даН/мм^2 = кГ/мм^2]
        /// </summary>
        public float StressInMax { get; }
        /// <summary>
        /// Погонное активное сопротивление провода (при температуре 20 Цельсий) [Ом/м].
        /// </summary>
        public float R0 { get; }
        /// <summary>
        /// Температурный коэффициент сопротивления [К^-1].
        /// </summary>
        public float LamdaResistance { get; }

        /// <summary>
        /// Максимально допустимая токовая нагрузка [А].
        /// </summary>
        public float MaxI { get; }

        public float AllowableStress => StressInMax;

       

        #region 04.01.2021
        /// <summary>
        /// площадь поперечного сечения одной алюминиевой проволоки [мм^2]
        /// </summary>
        public float AluminumRodSection { get; private set; }
        /// <summary>
        /// В [мм]
        /// </summary>
        public float AluminiumRodDiameter { get; private set; }

        /// <summary>
        ///  площадь поперечного сечения одной стальной проволоки [мм^2]
        /// </summary>
        public float SteelRodSection { get; private set; }

        /// <summary>
        /// В [мм]
        /// </summary>
        public float SteelRodDiameter { get; private set; }



        /// <summary>
        /// временное сопротивление разрыву одной алюминиевой проволоки [даН/мм^2]
        /// </summary>
        private float TemporaryTearResistanceOfAluminiumRod => GetTearResistanceOfAluminiumRod();
        /// <summary>
        /// Разрывное усилие одной аллюминиевой проволоки [даН]
        /// </summary>
        public float AluminiumRodBreakingForce => TemporaryTearResistanceOfAluminiumRod * AluminumRodSection;

        /// <summary>
        /// временное сопротивление разрыву одной стальной проволоки [даН/мм^2]
        /// </summary>
        private float TemporaryTearResistanceOfSteelRod => GetTearResistanceOfSteelRod();
        /// <summary>
        /// Разрывное усилие одной стальной проволоки [даН]
        /// </summary>
        public float SteelRodBreakingForce => TemporaryTearResistanceOfSteelRod * SteelRodSection;

        public int NumberOfAluminiumRod { get; private set; }
        public int NumberOfSteelRod { get; private set; }
        #endregion

        public CableKind SetComponentsParams(int numberOfAluminiumRod, int numberOfSteelRod,
            float aluminumRodDiameter, float steelRodDiameter)
        {
            NumberOfAluminiumRod = numberOfAluminiumRod;
            NumberOfSteelRod = numberOfSteelRod;
            AluminiumRodDiameter = aluminumRodDiameter;
            SteelRodDiameter = steelRodDiameter;
            AluminumRodSection = 0.785f * AluminiumRodDiameter * AluminiumRodDiameter;
            SteelRodSection = 0.785f * SteelRodDiameter* SteelRodDiameter;
            return this;
        }

        /// <summary>
        /// Возвращает временное сопротивление разрыву аллюминиевой проволоки. [даН / мм^2]
        /// </summary>
        /// <returns></returns>
        private float GetTearResistanceOfAluminiumRod()
        {
            // https://meganorm.ru/Data/730/73055.pdf стр. 30
            // 1 МПА = 0.1 даН/мм^2
            float d = AluminiumRodDiameter;
            if (d <= 1.25f)
                return 20;
            else if (d > 1.25 && d <= 1.5)
                return 19.5f;
            else if (d > 1.5 && d <= 1.75)
                return 19;
            else if (d > 1.75 && d <= 2)
                return 18.5f;
            else if (d > 2 && d <= 2.25)
                return 18f;
            else if (d > 2.25 && d <= 2.5)
                return 17.5f;
            else if (d > 2.5 && d <= 3)
                return 17f;
            else if (d > 3 && d <= 3.5)
                return 16.5f;
            else return 16;
        }

        private float GetTearResistanceOfSteelRod()
        {
            ///https://files.stroyinf.ru/Data/730/73055.pdf стр 32
            float d = SteelRodDiameter;
            if (d > 1.27 && d <= 2.28)
                return 145f;
            else if (d > 2.28 && d <= 3.55)
                return 141;
            else return 139;
        }
    }
}
