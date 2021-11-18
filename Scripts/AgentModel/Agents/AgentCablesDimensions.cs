using CableWalker.Simulator.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentCablesDimensions : Agent
    {
     
        public Dictionary<string, float> NormativeDimentions { get; private set; }
        public Dictionary<string, Vector3[]> Dimentions { get; private set; }


        private float nearNotAllowedParam = 0.2f; //20 см

        private int stateFlag = 0; //0 - normal, 1 - is near not allowed, 2 - not allowed


        private bool IsNormalCondition(float actualValue, float normativeValue) => actualValue - normativeValue > nearNotAllowedParam;

        private bool IsNearNotAllowedCondition(float actualValue, float normativeValue) => actualValue - normativeValue >= 0 && actualValue - normativeValue <= nearNotAllowedParam;   // ActualValue / NormativeValue < 1 + nearNotAllowedParam && ActualValue / NormativeValue >= 1;

        private bool IsNotAllowedCondition(float actualValue, float normativeValue) => actualValue - normativeValue < 0;

        private Span span;

        public AgentCablesDimensions(string number, Dictionary<string,float> normativeDims, Span span)
        {
            Number = number;
            //Weight = 0.5999994f;
            Weight = 1f;
            NormativeDimentions = normativeDims;
            Dimentions = new Dictionary<string, Vector3[]>();
            Connections = new List<Agent>();
            this.span = span;
            SetStateDiagram();
            ObjectName = "Cables dimensions";
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

        private int GetStateFlagByDistances(Dictionary<string, Vector3[]> distances)
        {
            int flag = 0;
            foreach (KeyValuePair<string,Vector3[]> entry in distances)
            {
                float normativeValue;
                try
                {
                    normativeValue = NormativeDimentions[entry.Key];
                }
                catch(KeyNotFoundException e)
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

        public override void ProcessMessage(Message message, Agent messenger)
        {
            if (message.Text == MessagesTexts.updateCableDimensions)
            {
                try
                {
                    var distances = span.CalculateCablesDistances();
                    span.LogCablesDistances(distances);
                    Dimentions = distances;
                    stateFlag = GetStateFlagByDistances(distances);
                    RecalculateTrackedParameter();

                }
                catch(System.NotImplementedException)
                {
                    stateFlag = 0;
                    RecalculateTrackedParameter();
                }

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
                if(stateFlag == 0)
                {
                    TrackedParameter = 4;
                    return 0;
                }
                else if(stateFlag == 1)
                {
                    TrackedParameter = 2;
                    return 1;
                }
                else if(stateFlag == 2)
                {
                    TrackedParameter = 0;
                    return 2;
                }
                else throw new System.Exception(
                   string.Format("In agent {0} wrong state determine",
                   Number));
            };
            StateDiagram.UpdateState();
        }

        public override void ProcessPrivateTransitions()
        {
            throw new System.NotImplementedException();
        }

        public override string GetParamsDescription()
        {
            string res = "";
            foreach(var entry in Dimentions)
            {
                double actualValue = Math.Round((entry.Value[0] - entry.Value[1]).magnitude,2);
                double normativeValue = Math.Round(NormativeDimentions[entry.Key],2);
                res += string.Format("Dimensions to {0}: actual value = {1} m., normative value = {2} m.\n", entry.Key, actualValue, normativeValue);
            }
            res += string.Format("\nTracked parameter (Status): {0};" +
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
                    return Color.yellow;
                case 2:
                    return Color.red;
                default:
                    return Color.white;
            }
        }

        public override List<(string, string)> GetParamsDescriptionForTable()
        {
           var res = new List<(string, string)>()
            {
                ("Status",Math.Round(TrackedParameter, 1).ToString()),
                ("State", StateDiagram.CurrentState.Name)
            };
            foreach(var entry in Dimentions)
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
    }
}
