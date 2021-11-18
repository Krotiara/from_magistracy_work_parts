using CableWalker.Simulator.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentSpanDimensions : Agent
    {

        private Span span;
      
        public float B { get; private set; }
        

        private bool IsNormalCondition(float actualValue, float normativeValue) => actualValue - normativeValue > nearNotAllowedParam;

        private bool IsNearNotAllowedCondition(float actualValue, float normativeValue) => actualValue - normativeValue >= 0 && actualValue - normativeValue <= nearNotAllowedParam;   // ActualValue / NormativeValue < 1 + nearNotAllowedParam && ActualValue / NormativeValue >= 1;

        private bool IsNotAllowedCondition(float actualValue, float normativeValue) => actualValue - normativeValue < 0;

        public Dictionary<string, float> NormativeDimentions { get; private set; }
        public Dictionary<string, Vector3[]> Dimentions { get; private set; }


        private float nearNotAllowedParam = 0.2f; //20 см

        private int stateFlag = 0; //0 - normal, 1 - is near not allowed, 2 - not allowed

       

        public AgentSpanDimensions(Span span, string number, Dictionary<string, float> normativeDims)
        {
            this.span = span;
            Number = number; 
            Weight = 0.09f;
            B = 0f;
            // this.normativeDims = normativeDims;
            NormativeDimentions = normativeDims;
             Connections = new List<Agent>();
            SetStateDiagram();
            ObjectName = "Span dimensions";
            TrackedParameterName = "GTCI";
            Dimentions = new Dictionary<string, Vector3[]>();

            Parameters["Cables dimensions"] = new Parameter("Cables dimensions", 1, () =>
            {
                if (stateFlag == 0)
                    return ("Standard values", 4);
                else if (stateFlag == 1)
                    return ("Near not allowed values", 2);
                else if (stateFlag == 2)  
                    return ("Not allowed values", 0);   
                else throw new System.Exception(
                   string.Format("In agent {0} wrong state determine",
                   Number));
            }

            );
            ProcessMessage(new Message(Number, Number, MessagesTexts.updateCableDimensions, new List<string>().ToArray()), this);
            RecalculateTrackedParameter();
        }

        public override void CreateConnections(Dictionary<string, Agent> agents)
        {
            //return
            //cd = new AgentCablesDimensions(cableDimsNumber, normativeDims, span);
            //ConnectTo(cd);
            //agents[cd.Number] = cd;
            //cd.CreateConnections(agents);
            //RecalculateTrackedParameter();
            //ProcessMessage(new Message(Number, cableDimsNumber, MessagesTexts.updateCableDimensions, new List<string>().ToArray()), this);

        }

        public override void RecalculateTrackedParameter()
        {
            float result = 0;
            foreach(Parameter param in Parameters.Values)
            //foreach (Agent agent in Connections.ToList())
            {
                result += param.Weight * param.Status / 4;
            }
            TrackedParameter = 100 * (result+B);
            if (TrackedParameter < 0) TrackedParameter = 0;
            StateDiagram.UpdateState();
        }

        public override void ProcessMessage(Message message, Agent messenger)
        {
            if(message.Text == MessagesTexts.updateCableDimensions)
            {

                try
                {
                    var distances = span.CalculateCablesDistances();
                    span.LogCablesDistances(distances);
                    Dimentions = distances;
                    stateFlag = GetStateFlagByDistances(distances);
                    Parameters["Cables dimensions"].RecalculateStatus();
                    RecalculateTrackedParameter();

                }
                catch (System.NotImplementedException)
                {
                    stateFlag = 0;
                    Parameters["Cables dimensions"].RecalculateStatus();
                    RecalculateTrackedParameter();
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
            var res = new List<(string, string)>()
            {
                ("GTCI",Math.Round(TrackedParameter, 1).ToString()),
                ("State", StateDiagram.CurrentState.Name),

            };
            foreach (Parameter p in Parameters.Values)
            {
                res.Add((p.Name, p.Label));
            }
            foreach (var entry in Dimentions)
            {
                double actualValue = Math.Round((entry.Value[0] - entry.Value[1]).magnitude, 2);
                double normativeValue = Math.Round(NormativeDimentions[entry.Key], 2);
                res.Add(($"Dimension to {entry.Key}", $"Actual value = {actualValue} m., Normative value = {normativeValue} m."));
            }
            return res;
        }

        public override List<string[]> GetDetailedParamsDescription()
        {
            throw new NotImplementedException();
        }

        private int GetStateFlagByDistances(Dictionary<string, Vector3[]> distances)
        {
            int flag = 0;
            foreach (KeyValuePair<string, Vector3[]> entry in distances)
            {
                float normativeValue;
                try
                {
                    normativeValue = NormativeDimentions[entry.Key];
                }
                catch (KeyNotFoundException e)
                {
                    Debug.Log(string.Format("no normative dimentions value for key {0}", entry.Key));
                    continue;
                }
                float actualValue = (entry.Value[0] - entry.Value[1]).magnitude;
                if (IsNotAllowedCondition(actualValue, normativeValue))
                    return 2;
                else if (IsNearNotAllowedCondition(actualValue, normativeValue))
                    flag = 1;
            }
            return flag;
        }

    }
}
