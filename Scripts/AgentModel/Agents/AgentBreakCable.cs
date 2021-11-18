using CableWalker.Simulator.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentBreakCable : Agent
    {

        //private Cable cable;
        private float k = 3;
        public float MaxStress { get; set; }
        public float BreakingStress { get; set; }

      

        private bool IsCriticalCondition => BreakingStress - MaxStress <= k/* && !IsBreakCondition*/;
        private bool IsNormalCondition => BreakingStress - MaxStress > k/* && !IsBreakCondition*/;
       // private bool IsBreakCondition => isBreak || BreakingStress <= MaxStress; //Надо обекспечить, что при получении сообщения об устранении обрыва это условие в false шло.


        public AgentBreakCable(string number, float maxStress, float breakingStress)
        {
            Number = number;
            //Weight = 0.32002963f;
            //Weight = 0.36499948f;
            Weight = 0.42888828f;
            MaxStress = maxStress;
            BreakingStress = breakingStress;
            Connections = new List<Agent>();
            SetStateDiagram();
            ObjectName = "Stresses values";
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
            if(message.Text == MessagesTexts.updateStresses)
            {
                MaxStress = float.Parse(message.Args[1]);
                BreakingStress = float.Parse(message.Args[2]);
                RecalculateTrackedParameter();
            }
        }

        public override void SetStateDiagram()
        {
            StateDiagram = new StateDiagram();
            StateDiagram.AddState("Normal");
            StateDiagram.AddState("Critical");
            //StateDiagram.AddState("Break");
            StateDiagram.DetermineStateIndexByNewValue = () =>
            {
                if (IsNormalCondition)
                {
                    TrackedParameter = 4;
                    return 0;
                }
                else if (IsCriticalCondition)
                {
                    TrackedParameter = 1;
                    return 1;
                }
                //else if (IsBreakCondition)
                //{
                //    TrackedParameter = 0;
                //    return 2;
                //}
                throw new System.Exception(
                    string.Format("In agent {0} wrong state determine, params - maxstress - {1}, P - {2}, k - {3}",
                    Number, MaxStress.ToString(), BreakingStress.ToString(), k.ToString()));
            };
            StateDiagram.UpdateState();

        }

        public override void ProcessPrivateTransitions()
        {
            throw new System.NotImplementedException();
        }

        public override string GetParamsDescription()
        {
            return string.Format("Max stress = {0};\nBreaking stress = {1};\n", MaxStress, BreakingStress) 
                 + string.Format("Tracked parameter (Status): {0};" +
                  "\nState: {1}.", Math.Round(TrackedParameter, 1).ToString(), StateDiagram.CurrentState.Name); ;
        }

        public override Color GetStateColor()
        {
            switch(StateDiagram.CurrentStateIndex)
            {
                case 0:
                    return new Color(102/255f, 204/255f, 153/255f);
                case 1:
                    return new Color(255f/255f,165f/255f,0);
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
                ("Max stress", MaxStress.ToString()),
                ("Breaking stress", BreakingStress.ToString())
            };
        }

        public override List<string[]> GetDetailedParamsDescription()
        {
            throw new NotImplementedException();
        }
    }
}
