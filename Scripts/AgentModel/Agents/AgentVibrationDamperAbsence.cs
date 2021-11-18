using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentVibrationDamperAbsence : Agent
    {

        private bool isExist = true; //По-умолчанию считается установленным

        public AgentVibrationDamperAbsence(string number)
        {
            Number = number;
           // Weight = 0.56627736f;
            Weight = 0.61249939f;
            Connections = new List<Agent>();
            SetStateDiagram();
            ObjectName = "Vibration damper absence";
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
            if (message.Text == MessagesTexts.vibrationDamperAbsence)
                isExist = false;
            else if (message.Text == MessagesTexts.vibrationDamperAbsenceEliminated)
                isExist = true;
            RecalculateTrackedParameter();
        }

        public override void SetStateDiagram()
        {
            StateDiagram = new StateDiagram();
            StateDiagram.AddState("Installed");
            StateDiagram.AddState("Missing");
            StateDiagram.DetermineStateIndexByNewValue = () =>
            {
                if (isExist)
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
                ("State", StateDiagram.CurrentState.Name)
            };
        }

        public override List<string[]> GetDetailedParamsDescription()
        {
            throw new NotImplementedException();
        }
    }
}
