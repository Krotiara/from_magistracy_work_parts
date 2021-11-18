using CableWalker.Simulator.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentGroundCable : Agent
    {

        private Cable cable;
        //private AgentSag agentSag;
        //private AgentCableCorrosion agentCorrosion;
        //private AgentBreakRodsInClamps agentRodsClamps;
        //private AgentBreakRodsOutClamps agentRodsOutClamps;
        //private AgentBreakCable agentCableBreak;
        private float sagNormativeValue;
        private float currentMaxStress;
        private float currentBreakingStress;
        private bool isCorrosion; //Может в calcMode запихнуть, хотчя там мех.параметры только

        private float k = 3;
        private bool IsCriticalCableBreakCondition => stressesWithMinDelta.Item2 - stressesWithMinDelta.Item1 <= k/* && !IsBreakCondition*/;
        private bool IsNormalCableBreakCondition => stressesWithMinDelta.Item2 - stressesWithMinDelta.Item1 > k/* && !IsBreakCondition*/;
        private (float, float) stressesWithMinDelta; // item1 - stress, item2 - breaking stress

        public List<Rod> BreakRodsOutClamps { get; } = new List<Rod>();
        public List<Rod> BreakRodsInClamps { get; } = new List<Rod>();

        //private string agentSagNumber => string.Format("{0}.Sag", cable.Number);
        //private string agentCorrosionNumber => string.Format("{0}.Corrosion", cable.Number);

        //private string agentBreakRodsInClampsNumber => string.Format("{0}.BreakRodsInClamps", cable.Number);

        //private string agentBreakRodsOutClampsNumber => string.Format("{0}.BreakRodsOutClamps", cable.Number);

        //private string agentCableBreakNumber => string.Format("{0}.CableBreak", cable.Number);

        public float B { get; private set; }

        private AgentSpan agentSpan;


        public AgentGroundCable(Cable gCable, float sagNormativeValue, float currentMaxStress, float currentBreakingStress, AgentSpan agentSpan)
        {
            this.cable = gCable;
            Number = gCable.Number;
            Weight = 0.091f;
            this.sagNormativeValue = sagNormativeValue;
            this.currentMaxStress = currentMaxStress;
            this.currentBreakingStress = currentBreakingStress;
            B = -0.5272306803703704f;
            Connections = new List<Agent>();
            this.agentSpan = agentSpan;
            InitParameters();
            SetStateDiagram();
            ObjectName = string.Format("Ground cable, phase {0}", gCable.Phase);
            TrackedParameterName = "GTCI"; 

        }

        private void InitParameters()
        {
            Parameters["Sag"] = new Parameter("Sag", 0.1919226f, () =>
            {
                float actualSag = cable.CalcMode.Sag;
                float normativeSag = cable.GetNormativeSag(cable.CalcMode);
                if (actualSag / normativeSag < 0.8f)
                    return ("Standard values", 4);
                else if (actualSag / normativeSag >= 0.8f && actualSag / normativeSag <= 1)
                    return ("Near not allowed values", 3);
                else
                    return ("Not allowed values", 0);
            });

            Parameters["Corrosion"] = new Parameter("Corrosion", 0.16555495f, () =>
            {
                if (!isCorrosion)
                    return ("No corrosion", 4);
                else
                    return ("Corrosion", 1);
            });

            //Parameters["Stresses values"] = new Parameter("Stresses values", 0.42888828f, () =>
            //{
            //    if (IsNormalCableBreakCondition)
            //    {
            //        return ("Normal", 4);

            //    }
            //    else if (IsCriticalCableBreakCondition)
            //    {
            //        return ("Danger of breakage", 1);
            //    }
            //    //else if (IsBreakCondition)
            //    //{
            //    //    TrackedParameter = 0;
            //    //    return 2;
            //    //}
            //    throw new System.Exception(
            //        string.Format("In parameter stresses values {0} wrong state determine",
            //        Number));
            //});

            Parameters["Break rods out clamps"] = new Parameter("Break rods out clamps", 0.4016653f, () =>
            {
                if (BreakRodsOutClamps.Count == 0)
                    return ("No breaks", 4);
                else
                {
                    return ("Breaks", 2);
                }
            });

            Parameters["Break rods in clamps"] = new Parameter("Break rods in clamps", 0.34083299f, () =>
            {
                if (BreakRodsInClamps.Count == 0)
                    return ("No breaks", 4);
                else
                {
                    return ("Breaks", 0);
                }
            });

            foreach (Parameter p in Parameters.Values)
                p.RecalculateStatus();

        }

        public override void CreateConnections(Dictionary<string, Agent> agents)
        {
            return;
            //agentSag = new AgentSag(agentSagNumber, sagNormativeValue, cable.CurrentMode.Sag);
            //ConnectTo(agentSag);
            //agents[agentSag.Number] = agentSag;
            //agentSag.CreateConnections(agents);

            //agentCorrosion = new AgentCableCorrosion(agentCorrosionNumber);
            //ConnectTo(agentCorrosion);
            //agents[agentCorrosion.Number] = agentCorrosion;
            //agentCorrosion.CreateConnections(agents);

            //agentRodsClamps = new AgentBreakRodsInClamps(agentBreakRodsInClampsNumber,cable);
            //ConnectTo(agentRodsClamps);
            //agents[agentRodsClamps.Number] = agentRodsClamps;
            //agentRodsClamps.CreateConnections(agents);

            //agentRodsOutClamps = new AgentBreakRodsOutClamps(agentBreakRodsOutClampsNumber,cable);
            //ConnectTo(agentRodsOutClamps);
            //agents[agentRodsOutClamps.Number] = agentRodsOutClamps;
            //agentRodsOutClamps.CreateConnections(agents);

            //agentCableBreak = new AgentBreakCable(agentCableBreakNumber, currentMaxStress, currentBreakingStress);
            //ConnectTo(agentCableBreak);
            //agents[agentCableBreak.Number] = agentCableBreak;
            //agentCableBreak.CreateConnections(agents);

            //RecalculateTrackedParameter();
        }

        public override void RecalculateTrackedParameter()
        {
            if (Parameters != null && Parameters.Count > 0)
            {
                float result = 0;
                foreach (Parameter param in Parameters.Values)
                {

                    result += param.Weight * param.Status / 4;
                }
                result += 0.32002963f * 1; // Костыль: Для грозотроса отключил  AgentBreakCable из-за того, что похоже формула расчета только для стале-аллюминиевых проводов. Поставил костыль
                TrackedParameter = 100 * (result + B);
                if (TrackedParameter < 0) TrackedParameter = 0;
                if (TrackedParameter > 100) TrackedParameter = 100;
            }
            StateDiagram.UpdateState();
        }

       

        private void RecalculateCable()
        {
            cable.CalculateCalcModeParams(cable.CalcMode, cable.CalcMode.Environment);
            ProcessPrivateTransitions();
        }

        public override void ProcessPrivateTransitions()
        {
            //Для грозотроса отключил из-за того, что похоже формула расчета только для стале-аллюминиевых проводов. Поставил костыль
            // var stresses = cable.CalcMode.GetPairStressAndBreakingStressWithMinDelta();
            stressesWithMinDelta = cable.CalcMode.GetPairStressAndBreakingStressWithMinDelta();
            Parameters["Sag"].RecalculateStatus();
           // Parameters["Stresses values"].RecalculateStatus();
            //SendMessage(new Message(Number, agentCableBreak.Number, MessagesTexts.updateStresses, new string[3] {cable.Phase, stresses.Item1.ToString(), stresses.Item2.ToString() }), agentCableBreak);
            // SendMessage(new Message(Number, agentSag.Number, MessagesTexts.updateSags, new string[3] {cable.Phase, cable.CalcMode.Sag.ToString(), cable.GetNormativeSag(cable.CalcMode).ToString() }),agentSag);
            RecalculateTrackedParameter();
            //SendMessage(new Message(Number, agentCableBreak.Number, MessagesTexts.updateStresses, new string[3] {cable.Phase, stresses.Item1.ToString(), stresses.Item2.ToString() }), agentCableBreak);
            //SendMessage(new Message(Number, agentSag.Number, MessagesTexts.updateSags, new string[3] {cable.Phase, cable.CalcMode.Sag.ToString(), cable.GetNormativeSag(cable.CalcMode).ToString() }), agentSag);
            //RecalculateTrackedParameter();
        }

        public override void ProcessMessage(Message message, Agent messenger)
        {
            //public Vector3 GetPointByDistance(float distanceFromFirstTower, CableMode mode)
            //{
            //    return GetPoint(GetTByIndex(distanceFromFirstTower / cableResolution, mode), mode);
            //}
            //public void AddBreakRod(float X, float y, RodType rodType)
            //ReturnStartCableMode
            //public void CalculateModeParams(CableMode calcMode, Environment calcEnvironment)

            if (message.Text == MessagesTexts.iceFormation)
            {
                cable.CalcMode.Environment.SetParams(float.Parse(message.Args[0]), float.Parse(message.Args[1]),
                    float.Parse(message.Args[2]), float.Parse(message.Args[3]), int.Parse(message.Args[4]) == 0 ? Simulator.WindDirection.Perpendicular : Simulator.WindDirection.Along);
                cable.CalcMode.IceThickness = int.Parse(message.Args[5]) == 1 ? cable.CalcMode.Environment.AreaIceThicknessValue : 0;
                RecalculateCable();
            }
            else if (message.Text == MessagesTexts.normalEnvironmentConditions || message.Text == MessagesTexts.strongWind)
            {
                cable.CalcMode.Environment.SetParams(float.Parse(message.Args[0]), float.Parse(message.Args[1]),
                    float.Parse(message.Args[2]), float.Parse(message.Args[3]), int.Parse(message.Args[4]) == 0 ? Simulator.WindDirection.Perpendicular : Simulator.WindDirection.Along);
                cable.CalcMode.IceThickness = int.Parse(message.Args[5]) == 1 ? cable.CalcMode.Environment.AreaIceThicknessValue : 0;
                RecalculateCable();
            }

            else if (message.Text == MessagesTexts.breakRodInClamp ||
                message.Text == MessagesTexts.breakRodInClampEliminated ||
                message.Text == MessagesTexts.breakRodOutClamp ||
                message.Text == MessagesTexts.breakRodOutClampEliminated)
            {
                //message.To = agentBreakRodsInClampsNumber;
                //SendMessage(message, agentRodsClamps);
                ProcessBreakRodsMessage(message);
                Parameters["Break rods out clamps"].RecalculateStatus();
                Parameters["Break rods in clamps"].RecalculateStatus();
                RecalculateCable();
            }
            //else if (message.Text == MessagesTexts.breakRodOutClamp || message.Text == MessagesTexts.breakRodOutClampEliminated)
            //{
            //    //+AddBreakRod
            //    message.To = agentBreakRodsOutClampsNumber;
            //    SendMessage(message, agentRodsOutClamps);
            //    RecalculateCable();
            //}

            else if (message.Text == MessagesTexts.cableCorrosion || message.Text == MessagesTexts.cableCorrosionEliminated)
            {
                isCorrosion = message.Text == MessagesTexts.cableCorrosion;
                Parameters["Corrosion"].RecalculateStatus();
                RecalculateCable();
            }
            RecalculateTrackedParameter();
        }

        private void ProcessBreakRodsMessage(Message message)
        {
            List<Rod> breakRodsDict = message.Text == MessagesTexts.breakRodOutClamp ? BreakRodsOutClamps : BreakRodsInClamps;
            if (message.Text == MessagesTexts.breakRodOutClamp || message.Text == MessagesTexts.breakRodInClamp)
            {
                Vector3 pointRelativelyLowestCablePoint = cable.GetPointRelativelyLowestByDistance(float.Parse(message.Args[1]), cable.CalcMode);
                if (message.Text == MessagesTexts.breakRodInClamp)
                {
                    if (int.Parse(message.Args[1]) == 1)
                        message.Args[1] = ((int)cable.Span.Length).ToString(); //Для обрывов проволок в зажиме аргумент дистанции = 0 или 1, что соответствует первому и второму зажиму
                }
                int Na = int.Parse(message.Args[2]);
                int Ns = int.Parse(message.Args[3]);
                int number = 1;
                for (int i = 0; i < Na; i++)
                {
                    breakRodsDict.Add(new Rod(number, float.Parse(message.Args[1]), pointRelativelyLowestCablePoint.y, Simulator.RodType.Aluminium));
                    cable.CalcMode.AddBreakRod(float.Parse(message.Args[1]), pointRelativelyLowestCablePoint.y, Simulator.RodType.Aluminium);
                    number += 1;
                }
                for (int i = 0; i < Ns; i++)
                {
                    breakRodsDict.Add(new Rod(number, float.Parse(message.Args[1]), pointRelativelyLowestCablePoint.y, Simulator.RodType.Steel));
                    cable.CalcMode.AddBreakRod(float.Parse(message.Args[1]), pointRelativelyLowestCablePoint.y, Simulator.RodType.Steel);
                    number += 1;
                }
                //Rods.Add(new Rod(int.Parse(message.Args[1]), float.Parse(message.Args[2]), pointRelativelyLowestCablePoint.y, int.Parse(message.Args[3]) == 0 ? RodType.Aluminium : RodType.Steel));
                //cable.CalcMode.AddBreakRod(float.Parse(message.Args[2]), pointRelativelyLowestCablePoint.y, int.Parse(message.Args[3]) == 0 ? RodType.Aluminium : RodType.Steel);
            }
            else if (message.Text == MessagesTexts.breakRodOutClampEliminated || message.Text == MessagesTexts.breakRodInClampEliminated)
            {
                if (breakRodsDict.Count > 0)
                {
                    //Rods.RemoveAll(c => c.Number == int.Parse(message.Args[1]) && c.DistanceFromTower1 == float.Parse(message.Args[2]));
                    breakRodsDict.RemoveAll(c => c.DistanceFromTower1 == float.Parse(message.Args[1]));
                    cable.CalcMode.RemoveAllBreakRodsIn(float.Parse(message.Args[1]));
                    //cable.CalcMode.RemoveBreakRod(float.Parse(message.Args[2]), int.Parse(message.Args[3]) == 0 ? RodType.Aluminium : RodType.Steel);
                }
            }
        }

        public override void SetStateDiagram()
        {
            StateDiagram = new StateDiagram();
            StateDiagram.AddState("Very Good");
            StateDiagram.AddState("Good");
            StateDiagram.AddState("Satisfactory");
            StateDiagram.AddState("Not satisfactory");
            StateDiagram.AddState("Critical");
            StateDiagram.DetermineStateIndexByNewValue = () =>
            {
                if (TrackedParameter <= 25)
                    return 4;
                if (TrackedParameter > 25 && TrackedParameter <= 50)
                    return 3;
                if (TrackedParameter > 50 && TrackedParameter <= 70)
                    return 2;
                if (TrackedParameter > 70 && TrackedParameter <= 85)
                    return 1;
                else return 0;
            };
            RecalculateTrackedParameter();
        }

        public override string GetParamsDescription()
        {
            string res = cable.GetInfoInString(); //Дополнить значениями из CurrentMode
            res += string.Format("Tracked parameter (GTCI): {0};" +
                 "\nState: {1}.", Math.Round(TrackedParameter, 1).ToString(), StateDiagram.CurrentState.Name);
            return res;
        }

        public override Color GetStateColor()
        {
            switch (StateDiagram.CurrentStateIndex)
            {
                case 0:
                    return new Color(102/255f, 204/255f, 153/255f);
                case 1:
                    return new Color(102/255f, 204/255f, 153/255f);
                case 2:
                    return Color.yellow;
                case 3:
                    return new Color(255f/255f,165f/255f,0);
                case 4:
                    return Color.red;
                default:
                    return Color.white;
            }
        }

        public override List<(string, string)> GetParamsDescriptionForTable()
        {
            var res = new List<(string, string)>()
            {
                ("GTCI",Math.Round(TrackedParameter, 1).ToString()),
                ("State", StateDiagram.CurrentState.Name)
            };

            foreach (Parameter p in Parameters.Values)
            {
                res.Add((p.Name, p.Label));
            }
            res.AddRange(new List<(string, string)>()
            {
                ("Aluminium break rods out clamps count", BreakRodsOutClamps.Where(x => x.Type == Simulator.RodType.Aluminium).Count().ToString()),
                 ("Steel break rods out clamps count", BreakRodsOutClamps.Where(x => x.Type == Simulator.RodType.Steel).Count().ToString()),
                 ("Aluminium break rods in clamps count", BreakRodsInClamps.Where(x => x.Type == Simulator.RodType.Aluminium).Count().ToString()),
                 ("Steel break rods out clamps count", BreakRodsInClamps.Where(x => x.Type == Simulator.RodType.Steel).Count().ToString())
            }
            );

            res.AddRange(cable.CalcMode.GetInfo());
            res.AddRange(cable.GetInfo());
            return res;

        }

        public override List<string[]> GetDetailedParamsDescription()
        {
            return cable.CalcMode.GetDetailedInfoAboutBreakRods();
        }
    }
}
