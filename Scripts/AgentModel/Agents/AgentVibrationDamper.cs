using CableWalker.Simulator.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentVibrationDamper : Agent
    {

        private VibrationDamper vibrationDamper;
        //private AgentVibrationDamperAbsence absenceAgent;
        //private AgentVibrationDamperOffset offsetAgent;
        //private AgentVibrationDamperWeightsAbsence weightsAbsenceAgent;
       // private List<float> paramsWeights; // 0 - смещение, 1 - отсутствие, 2 - отсутствие грузов
       // private List<float> paramsStatuses;
        // private Dictionary<string, string> paramsStates;

      //  private StateDiagram vdOffsetStateDiagram;
      //  private StateDiagram vdAbsenceStateDiagram;
      //  private StateDiagram vdWeightsAbsenceStateDiagram;

        //private string vDAbsenceNumber => string.Format("{0}.Absence", vibrationDamper.Number);
        //private string vDOffsetNumber => string.Format("{0}.Offset", vibrationDamper.Number);
        //private string vDWeightsAbsenceNumber => string.Format("{0}.WeightsAbsence", vibrationDamper.Number);
        public float B { get; private set; }

        private bool isOffset;
        private bool isExist = true;
        private bool isWeightExist = true;

        public AgentVibrationDamper(VibrationDamper vibrationDamper)
        {
            this.vibrationDamper = vibrationDamper;
            Number = vibrationDamper.Number;
            Weight = 0.091f;
            //B = -0.40007987144818535f;
            B = -1.041657104861111f;

            

            Connections = new List<Agent>();
            SetStateDiagram();
            ObjectName = vibrationDamper.TextNumber;
            TrackedParameterName = "GTCI";
            InitParameters();
        }


        private void InitParameters()
        {
            Parameters["Vibration damper offset"] = new Parameter("Vibration damper offset", 0.9499902f, () =>
            {
                if (!isOffset)
                    return ("No offset", 4);
                else
                    return ("Offset", 3);
            });

            Parameters["Vibration damper absence"] = new Parameter("Vibration damper absence", 0.61249939f, () =>
            {
                if (isExist)
                    return ("Installed", 4);
                else
                    return ("Missing", 0);
            });

            Parameters["Vibration damper weights absence"] = new Parameter("Vibration damper weights absence", 0.51666558f, () =>
            {
                if (isWeightExist)
                {
                    return ("Installed", 4);
                }
                else
                {
                    return ("Missing", 1);
                }
            });

            foreach (Parameter p in Parameters.Values)
                p.RecalculateStatus();
        }

        public override void CreateConnections(Dictionary<string, Agent> agents)
        {
            return;
            //absenceAgent = new AgentVibrationDamperAbsence(vDAbsenceNumber);
            //ConnectTo(absenceAgent);
            //agents[absenceAgent.Number] = absenceAgent;
            //absenceAgent.CreateConnections(agents);

            //offsetAgent = new AgentVibrationDamperOffset(vDOffsetNumber);
            //ConnectTo(offsetAgent);
            //agents[offsetAgent.Number] = offsetAgent;
            //offsetAgent.CreateConnections(agents);

            //weightsAbsenceAgent = new AgentVibrationDamperWeightsAbsence(vDWeightsAbsenceNumber);
            //ConnectTo(weightsAbsenceAgent);
            //agents[weightsAbsenceAgent.Number] = weightsAbsenceAgent;
            //weightsAbsenceAgent.CreateConnections(agents);

            //RecalculateTrackedParameter();
        }

        public override void RecalculateTrackedParameter()
        {
            float result = 0;
            foreach (Parameter p in Parameters.Values)
            {
                result += p.Weight * p.Status / 4;
            }
            //foreach (Agent agent in Connections.ToList())
            //{
            //    result += agent.Weight * agent.TrackedParameter / 4;
            //}
            TrackedParameter = 100 * (result+B);
            if (TrackedParameter < 0) TrackedParameter = 0;
            if (TrackedParameter > 100) TrackedParameter = 100;
            StateDiagram.UpdateState();
        }

        public override void ProcessMessage(Message message, Agent messenger)
        {
            if(message.Text == MessagesTexts.vibrationDamperAbsence || message.Text == MessagesTexts.vibrationDamperAbsenceEliminated)
            {
                isExist = !(message.Text == MessagesTexts.vibrationDamperAbsence);
                Parameters["Vibration damper absence"].RecalculateStatus();
            }
            
            else if (message.Text == MessagesTexts.vibrationDamperOffset || message.Text == MessagesTexts.vibrationDamperOffsetEliminated)
            {
                isOffset = message.Text == MessagesTexts.vibrationDamperOffset;
                Parameters["Vibration damper offset"].RecalculateStatus();
            }
            else if (message.Text == MessagesTexts.vibrationDamperWeightsAbsence || message.Text == MessagesTexts.vibrationDamperWeightsAbsenceEliminated)
            {
                isWeightExist = !(message.Text == MessagesTexts.vibrationDamperWeightsAbsence);
                Parameters["Vibration damper weights absence"].RecalculateStatus();
            }
            RecalculateTrackedParameter();
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
            StateDiagram.UpdateState();
        }

        public override void ProcessPrivateTransitions()
        {
            throw new System.NotImplementedException();
        }

        public override string GetParamsDescription()
        {
           
            return string.Format("Tracked parameter (GTCI): {0};" +
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
            var res = new 
            List<(string, string)>()
            {
                ("GTCI",Math.Round(TrackedParameter, 1).ToString()),
                ("State", StateDiagram.CurrentState.Name)
            };
            foreach( Parameter p in Parameters.Values)
            {
                res.Add((p.Name, p.Label));
            }
            return res;
        }

        public override List<string[]> GetDetailedParamsDescription()
        {
            throw new NotImplementedException();
        }
    }
}
