using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentCableCorrosion : Agent
    {

        private bool isCorrosion = false;

        public AgentCableCorrosion(string number)
        {
            Number = number;
            //Weight = 0.11891852f;
            //Weight = 0.13277726f;
            Weight = 0.16555495f;
            Connections = new List<Agent>();
            SetStateDiagram();
            ObjectName = "Corrosion";
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
            if (message.Text == MessagesTexts.cableCorrosion)
                isCorrosion = true;
            else if (message.Text == MessagesTexts.cableCorrosionEliminated)
                isCorrosion = false;
            RecalculateTrackedParameter();
        }

        public override void SetStateDiagram()
        {
            StateDiagram = new StateDiagram();
            StateDiagram.AddState("No corrosion");
            StateDiagram.AddState("Corrosion");
            StateDiagram.DetermineStateIndexByNewValue = () =>
            {
                if (!isCorrosion)
                {
                    TrackedParameter = 4;
                    return 0;
                }
                else
                {
                    TrackedParameter = 1;
                    return 1;
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
                ("State", StateDiagram.CurrentState.Name)
            };
        }

        public override List<string[]> GetDetailedParamsDescription()
        {
            throw new NotImplementedException();
        }
    }
}
