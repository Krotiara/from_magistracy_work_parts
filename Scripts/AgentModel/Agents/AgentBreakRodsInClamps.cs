using CableWalker.Simulator;
using CableWalker.Simulator.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentBreakRodsInClamps : Agent
    {
        public List<Rod> Rods { get; } = new List<Rod>(); //InClamps = 0 - first clamp, 1 - second clamp
        private Cable cable;

        public AgentBreakRodsInClamps(string number, Cable cable)
        {
            Number = number;
            this.cable = cable;
            //Weight = 0.24585f;
            //Weight = 0.29291637f;
            Weight = 0.34083299f;
            Connections = new List<Agent>();
            SetStateDiagram();
            StateDiagram.UpdateState();
            ObjectName = "Break rods in clamps";
            TrackedParameterName = "Status";
        }

        public override void CreateConnections(Dictionary<string, Agent> agents)
        {
            return;
        }

        public override void RecalculateTrackedParameter()
        {
            StateDiagram.UpdateState();
        }

        public override void ProcessMessage(Message message, Agent messenger)
        {
            if(message.Text == MessagesTexts.breakRodInClamp)
            {
                //"Phase", "Distance from tower 1 [m.]", "Na", "Ns"
                Vector3 pointRelativelyLowestCablePoint = cable.GetPointRelativelyLowestByDistance(float.Parse(message.Args[1]), cable.CalcMode);
                if (int.Parse(message.Args[1]) == 1)
                    message.Args[1] = ((int)cable.Span.Length).ToString(); //Для обрывов проволок в зажиме аргумент дистанции = 0 или 1, что соответствует первому и второму зажиму
                int Na = int.Parse(message.Args[2]);
                int Ns = int.Parse(message.Args[3]);
                int number = 1;
                for (int i = 0; i < Na; i++)
                {
                    Rods.Add(new Rod(number, float.Parse(message.Args[1]), pointRelativelyLowestCablePoint.y, RodType.Aluminium));
                    cable.CalcMode.AddBreakRod(float.Parse(message.Args[1]), pointRelativelyLowestCablePoint.y, RodType.Aluminium);
                    number += 1;
                }
                for(int i = 0; i < Ns; i++)
                {
                    Rods.Add(new Rod(number, float.Parse(message.Args[1]), pointRelativelyLowestCablePoint.y, RodType.Steel));
                    cable.CalcMode.AddBreakRod(float.Parse(message.Args[1]), pointRelativelyLowestCablePoint.y, RodType.Steel);
                    number += 1;
                }
                //Rods.Add(new Rod(int.Parse(message.Args[1]), float.Parse(message.Args[2]), pointRelativelyLowestCablePoint.y, int.Parse(message.Args[3]) == 0 ? RodType.Aluminium : RodType.Steel));
                //cable.CalcMode.AddBreakRod(float.Parse(message.Args[2]), pointRelativelyLowestCablePoint.y, int.Parse(message.Args[3]) == 0 ? RodType.Aluminium : RodType.Steel);
            }
            else if (message.Text == MessagesTexts.breakRodInClampEliminated)
            {
                if (Rods.Count > 0)
                {
                    //Rods.RemoveAll(c => c.Number == int.Parse(message.Args[1]) && c.DistanceFromTower1 == float.Parse(message.Args[2]));
                    Rods.RemoveAll(c => c.DistanceFromTower1 == float.Parse(message.Args[1]));
                    cable.CalcMode.RemoveAllBreakRodsIn(float.Parse(message.Args[1]));
                    //cable.CalcMode.RemoveBreakRod(float.Parse(message.Args[2]), int.Parse(message.Args[3]) == 0 ? RodType.Aluminium : RodType.Steel);
                }
               
            }

            RecalculateTrackedParameter();
        }

        public override void SetStateDiagram()
        {
            StateDiagram = new StateDiagram();
            StateDiagram.AddState("No breaks");
            StateDiagram.AddState("Breaks");
            StateDiagram.DetermineStateIndexByNewValue = () =>
            {
                if(Rods.Count == 0)
                {
                    TrackedParameter = 4;
                    return 0;
                }
                else
                {
                    TrackedParameter = 0;
                    return 1;
                }
            };
        }

        public override void ProcessPrivateTransitions()
        {
            throw new System.NotImplementedException();
        }

        public override string GetParamsDescription()
        {
            return string.Format("Aluminium break rods in clamps count: {0};\n" +
               "Steel break rods in clamps count: {1};\n" +
               "Tracked parameter (Status): {2};\n" +
               "State: {3}.", Rods.Where(x => x.Type == RodType.Aluminium).Count(), Rods.Where(x => x.Type == RodType.Steel).Count(),
               Math.Round(TrackedParameter, 1).ToString(), StateDiagram.CurrentState.Name);

        }

        public override Color GetStateColor()
        {
            switch (StateDiagram.CurrentStateIndex)
            {
                case 0:
                    return new Color(102/255f, 204/255f, 153/255f);
                case 1:
                    return Color.red;
                default:
                    return Color.white;
            }
        }

        public override List<(string, string)> GetParamsDescriptionForTable()
        {
            return new List<(string, string)>()
            {
                ("Status",Math.Round(TrackedParameter, 1).ToString()),
                ("State", StateDiagram.CurrentState.Name),
                ("Aluminium break rods out clamps count", Rods.Where(x => x.Type == RodType.Aluminium).Count().ToString()),
                 ("Steel break rods out clamps count", Rods.Where(x => x.Type == RodType.Steel).Count().ToString())
            };
        }

        public override List<string[]> GetDetailedParamsDescription()
        {
            throw new NotImplementedException();
            //var res = new List<string[]>();
            //res.Add(new string[] {"Number", "X", "Type" });
            //foreach(Rod rod in Rods)
            //{
            //    ////// List = [sectionInPoint,y, na, ns, stress, breakingstress] na-ns = количество сломанных проволок.

            //    res.Add(new string[] { rod.Number.ToString(), rod.DistanceFromTower1.ToString(), rod.Type.ToString() }); 
            //}
            //return res;
        }
    }
}
