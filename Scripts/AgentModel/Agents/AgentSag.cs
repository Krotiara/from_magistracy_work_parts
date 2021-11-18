using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentSag : Agent
    {

        public float ActualValue { get; private set; }
        public float NormativeValue { get; private set; }
       // public bool isEqual => Mathf.Abs(ActualValue - NormativeValue) <= 0.15f; //Абсолютная ошибка в мат.модели

        public AgentSag(string number, float normativeValue, float actualValue)
        {
            Number = number;
            //Weight = 0.10444615f;
            //Weight = 0.14028806f;
            Weight = 0.1919226f;
            NormativeValue = normativeValue;
            ActualValue = actualValue;
            Connections = new List<Agent>();
            SetStateDiagram();
            ObjectName = "Sag";
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
            if(message.Text == MessagesTexts.updateSags)
            {
                ActualValue = float.Parse(message.Args[1]);
                NormativeValue = float.Parse(message.Args[2]);
                RecalculateTrackedParameter();
            }
        }

        public override void SetStateDiagram()
        {
            StateDiagram = new StateDiagram();
            StateDiagram.AddState("Standard values");
            StateDiagram.AddState("Near not allowed values");
            StateDiagram.AddState("Not allowed values");
            StateDiagram.DetermineStateIndexByNewValue = () =>
            {
                if(ActualValue/NormativeValue < 0.8f)
                {
                    TrackedParameter = 4;
                    return 0;
                }
                else if (ActualValue / NormativeValue >= 0.8f && ActualValue / NormativeValue <= 1)
                {
                    TrackedParameter = 3;
                    return 1;
                }
                else
                {
                    TrackedParameter = 0;
                    return 2;
                }
            };
            RecalculateTrackedParameter();
        }

        public override void ProcessPrivateTransitions()
        {
            throw new System.NotImplementedException();
        }

        public override string GetParamsDescription()
        {
            return string.Format("Actual value = {0} m.,\nNormative value = {1} m.;\n", ActualValue, NormativeValue) + string.Format("Tracked parameter (Status): {0};" +
                 "\nState: {1}.", Math.Round(TrackedParameter, 1).ToString(), StateDiagram.CurrentState.Name); ;
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
                ("State",StateDiagram.CurrentState.Name),
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
