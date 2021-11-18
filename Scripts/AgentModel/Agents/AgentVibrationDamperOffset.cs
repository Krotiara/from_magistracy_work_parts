using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentVibrationDamperOffset : Agent
    {
        private bool isOffset = false;

        public AgentVibrationDamperOffset(string number)
        {
            Number = number;
            //Weight = 0.43043773f;
            Weight = 0.9499902f;
            Connections = new List<Agent>();
            SetStateDiagram();
            ObjectName = "Vibration damper offset";
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
            if (message.Text == MessagesTexts.vibrationDamperOffset)
                isOffset = true;
            else if (message.Text == MessagesTexts.vibrationDamperOffsetEliminated)
                isOffset = false;
            RecalculateTrackedParameter();
        }

        public override void SetStateDiagram()
        {
            StateDiagram = new StateDiagram();
            StateDiagram.AddState("No offset");
            StateDiagram.AddState("Offset");
            StateDiagram.DetermineStateIndexByNewValue = () =>
            { 
                if(!isOffset)
                {
                    TrackedParameter = 4;
                    return 0;
                }
                else
                {
                    TrackedParameter = 3;
                    return 1;
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
            return string.Format("Tracked parameter (Status): {0};" +
                "\nState: {1}.", Math.Round(TrackedParameter, 1).ToString(), StateDiagram.CurrentState.Name);
        }

        public override Color GetStateColor()
        {
            switch (StateDiagram.CurrentStateIndex)
            {
                case 0:
                    return new Color(102/255f, 204/255f, 153/255f);
                case 1:
                    return new Color(102/255f, 204/255f, 153/255f);
                
                default:
                    return Color.white;
            }
        }

        public override List<(string, string)> GetParamsDescriptionForTable()
        {
            return new List<(string, string)>()
            {
                ("Status",Math.Round(TrackedParameter, 1).ToString()),
                ("State", StateDiagram.CurrentState.Name)
            };
        }

        public override List<string[]> GetDetailedParamsDescription()
        {
            throw new NotImplementedException();
        }
    }
}
