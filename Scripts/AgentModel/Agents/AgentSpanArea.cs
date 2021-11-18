using CableWalker.Simulator.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentSpanArea : Agent
    {
        private Span span;
      
        public float B { get; private set; }

        public int MaxTreeHeightParam { get; set; } = 1; //Для растительности 0-1 м. MaxTreeHeight  = 1/ Для растительности 1-4 м. MaxTreeHeight  = 4./Для растительности > 4м. MaxTreeHeight  = 5.
        public bool IsUnsafeTrees { get; set; } = false;

        public float ActualGladeWidthValue { get; private set; }
        public float NormativeGladeWidthValue { get; private set; }



        public AgentSpanArea(Span span, string number, float gladeWidthNormativeValue)
        {
            this.span = span;
            Number = number; 
            Weight = 0.364f;
            NormativeGladeWidthValue = gladeWidthNormativeValue;
            ActualGladeWidthValue = NormativeGladeWidthValue;
            //B = 0.11428628750000003f;
            B = -0.02874938140624994f;    
            InitParameters();
            Connections = new List<Agent>();
            SetStateDiagram();
            ObjectName = "Span area";
            TrackedParameterName = "GTCI";
            ProcessMessage(new Message(Number, Number, MessagesTexts.updateTSVByPointCloud, new List<string>().ToArray()), this);
            RecalculateTrackedParameter();

        }

        private (string,float) RecalculateTSVParameter()
        {
            if (MaxTreeHeightParam == 1 && !IsUnsafeTrees)
                return ("Standard values", 4);
            else if (MaxTreeHeightParam == 4 && !IsUnsafeTrees)
                return ("Deviation from standard values", 3);
            else if (IsUnsafeTrees && MaxTreeHeightParam < 5)
                return ("Unsafe trees", 2);
            else if (MaxTreeHeightParam == 5)
                return ("Not allowed values", 0);

            throw new System.Exception(
            string.Format("In agent {0} wrong state determine, params - MaxTreeHeightParam - {1}, IsUnsafeTrees - {2}",
            Number, MaxTreeHeightParam.ToString(), IsUnsafeTrees.ToString()));
        }

        private (string, float) RecalculareGladeWidthParameter()
        {
            if (ActualGladeWidthValue / NormativeGladeWidthValue < 1)
                return ("Not allowed values", 0);
            else
                return ("Allowed values", 4);

        }

        private void InitParameters()
        { 
            Parameters["TSV"] = new Parameter("TSV", 0.73999926f, RecalculateTSVParameter);
            Parameters["Glade width"] = new Parameter("Glade width", 0.2874996f, RecalculareGladeWidthParameter);   
        }

        public override void CreateConnections(Dictionary<string, Agent> agents)
        {
            return;
            //gW = new AgentGladeWidth(gladeWidthAgentNumber, gladeWidthNormativeValue);
            //ConnectTo(gW);
            //agents[gW.Number] = gW;
            //gW.CreateConnections(agents);

            //tsv = new AgentTSV(TSVAgentNumber, span);
            //ConnectTo(tsv);
            //agents[tsv.Number] = tsv;
            //tsv.CreateConnections(agents);

            //RecalculateTrackedParameter();


        }

        public override void RecalculateTrackedParameter()
        {
            float result = 0;
            foreach(Parameter parameter in Parameters.Values)
            {
            //foreach (Agent agent in Connections)
            //{
               // result += agent.Weight * agent.TrackedParameter / 4;
                result += parameter.Weight * parameter.Status / 4;
            }
            TrackedParameter = 100 * (result+B);
            if (TrackedParameter < 0) TrackedParameter = 0;
            StateDiagram.UpdateState();
        }

        public override void ProcessMessage(Message message, Agent messenger)
        {
            if (message.Text == MessagesTexts.updateGladeWidth)
            {
                //message.To = gladeWidthAgentNumber;
                //SendMessage(message, gW);
                ActualGladeWidthValue = float.Parse(message.Args[0]);
                Parameters["Glade width"].RecalculateStatus();
            }
            else if (message.Text == MessagesTexts.updateTSV)
            {
                MaxTreeHeightParam = int.Parse(message.Args[0]);
                IsUnsafeTrees = Convert.ToBoolean(int.Parse(message.Args[1]));
                Parameters["TSV"].RecalculateStatus();
            }
            else if (message.Text == MessagesTexts.updateTSVByPointCloud)
            {
                MaxTreeHeightParam = UpdateMaxTreeHeightParam();
                IsUnsafeTrees = UpdateUnsafeTreesParams();
                Parameters["TSV"].RecalculateStatus();
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
            var res = new List<(string, string)>()
            {
                ("GTCI",Math.Round(TrackedParameter, 1).ToString()),
                ("State", StateDiagram.CurrentState.Name)
            };

            foreach (Parameter p in Parameters.Values)
            {
                res.Add((p.Name, p.Label));
            }

            res.AddRange(new List<(string, string)>()
            {
                ("Vegetation height", GetTSVDescription()),
                ("Unsafe trees", GetUnsafeTreesDescription()),
                ("Actual glade width value (m.)",ActualGladeWidthValue.ToString()),
                 ("Normative glade width value (m.)", NormativeGladeWidthValue.ToString())
            });
            return res;
        }

        public override List<string[]> GetDetailedParamsDescription()
        {
            var res = new List<string[]>();
            res.Add(new string[] { "Threating trees" });
            res.AddRange(span.ThreatingTreesSpanData.GetThreatingTreesInfo(span));
            //Add squares green
            return res;
        }


        #region TSV info
        private bool UpdateUnsafeTreesParams()
        {
            if (span.ThreatingTreesSpanData == null)
                return false;
            else
            {
                return span.ThreatingTreesSpanData.IsThreatingThrees;
            }
        }

        private int UpdateMaxTreeHeightParam()
        {
            if (span.SpanAreaTSVData == null)
                return 1;
            else
            {
                AreaCalculator ac = span.SpanAreaTSVData.GostFormatCalculator;
                if (ac.areasSquares["10+"] > 0 || ac.areasSquares["4_10"] > 0)
                    return 5;
                else if (ac.areasSquares["1_4"] > 0)
                    return 4;
                else return 1;
            }
        }

        public string GetTSVDescription()
        {
            string tsvText = "?-?";
            if (MaxTreeHeightParam == 1)
                tsvText = "1-3";
            else if (MaxTreeHeightParam == 4)
                tsvText = "4-10";
            else if (MaxTreeHeightParam == 5)
                tsvText = "10+";
            return string.Format("Vegetation height is within {0} m.\n", tsvText);

        }

        public string GetUnsafeTreesDescription()
        {
            //Можно расписать расположение
            return IsUnsafeTrees ? string.Format("Unsafe trees count: {0}.\n", span.ThreatingTreesSpanData.ThreatingThreesCount) : "No unsave trees\n";

        }
        #endregion

    }
}
