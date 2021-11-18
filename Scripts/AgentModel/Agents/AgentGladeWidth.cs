using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentGladeWidth : Agent
    {
        public float ActualValue { get; private set; }
        public float NormativeValue { get; private set; }


        public AgentGladeWidth(string number, float normativeValue)
        {
            Number = number;
            //Weight = 0.24999962f;
            Weight = 0.2874996f;
            NormativeValue = normativeValue;
            ActualValue = normativeValue;
            Connections = new List<Agent>();
            SetStateDiagram();
            ObjectName = "Glade width";
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
            if(message.Text == MessagesTexts.updateGladeWidth)
            {
                ActualValue = float.Parse(message.Args[0]);
                RecalculateTrackedParameter();
            }

        }

        public override void SetStateDiagram()
        {
            StateDiagram = new StateDiagram();
            StateDiagram.AddState("Standard values");
            StateDiagram.AddState("Not allowed values");
            StateDiagram.DetermineStateIndexByNewValue = () =>
            {
                if(ActualValue/NormativeValue < 1)
                {
                    TrackedParameter = 0;
                    return 1;
                }
                else
                {
                    TrackedParameter = 4;
                    return 0;
                }
            };
            StateDiagram.UpdateState();
        }

        public override void ProcessPrivateTransitions()
        {
            throw new System.NotImplementedException();
        }

        public override string GetParamsDescription()
        {
            string res = string.Format("Actual value = {0} m.;\nNormative value = {1} m.;\n", ActualValue, NormativeValue);
            res += string.Format("Tracked parameter (Status): {0};" +
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
                ("Actual value (m.)",ActualValue.ToString()),
                 ("Normative value (m.)",NormativeValue.ToString())
            };
        }

        public override List<string[]> GetDetailedParamsDescription()
        {
            throw new NotImplementedException();
        }
    }
}
