using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentTSV : Agent
    {

        public int MaxTreeHeightParam { get; set; } = 1; //Для растительности 0-1 м. MaxTreeHeight  = 1/ Для растительности 1-4 м. MaxTreeHeight  = 4./Для растительности > 4м. MaxTreeHeight  = 5.
        public bool IsUnsafeTrees { get; set; } = false;
        private Simulator.Model.Span span;


        public AgentTSV(string number, Simulator.Model.Span span)
        {
            Number = number;
            //Weight = 0.6857136f;
            Weight = 0.73999926f;
            Connections = new List<Agent>();
            this.span = span;
            MaxTreeHeightParam = UpdateMaxTreeHeightParam();
            IsUnsafeTrees = UpdateUnsafeTreesParams();
            SetStateDiagram();
            ObjectName = $"TSV in span # {span.Number}";
            TrackedParameterName = "Status";
            ProcessMessage(new Message(Number, Number, MessagesTexts.updateTSVByPointCloud, new List<string>().ToArray()), this);
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
            if(message.Text == MessagesTexts.updateTSV)
            {
                MaxTreeHeightParam = int.Parse(message.Args[0]);
                IsUnsafeTrees = Convert.ToBoolean(int.Parse(message.Args[1]));
                RecalculateTrackedParameter();
            }
            else if(message.Text == MessagesTexts.updateTSVByPointCloud)
            {
                MaxTreeHeightParam = UpdateMaxTreeHeightParam();
                IsUnsafeTrees = UpdateUnsafeTreesParams();
                RecalculateTrackedParameter();
            }
        }

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

        public override void SetStateDiagram()
        {
            StateDiagram = new StateDiagram();
            StateDiagram.AddState("Standard values");
            StateDiagram.AddState("Deviation from standard values");
            StateDiagram.AddState("Unsafe trees");
            StateDiagram.AddState("Not allowed values");
            StateDiagram.DetermineStateIndexByNewValue = () =>
            {
                
                if (MaxTreeHeightParam == 1 && !IsUnsafeTrees)
                {
                    TrackedParameter = 4;
                    return 0;
                }
                else if (MaxTreeHeightParam == 4 && !IsUnsafeTrees)
                {
                    TrackedParameter = 3;
                    return 1;
                }

                else if (IsUnsafeTrees && MaxTreeHeightParam < 5)
                {
                    TrackedParameter = 2;
                    return 2;
                }
                else if (MaxTreeHeightParam == 5)
                {
                    TrackedParameter = 0;
                    return 3;
                }
                
                throw new System.Exception(
                string.Format("In agent {0} wrong state determine, params - MaxTreeHeightParam - {1}, IsUnsafeTrees - {2}",
                Number, MaxTreeHeightParam.ToString(), IsUnsafeTrees.ToString()));
                
            };
            StateDiagram.UpdateState();

        }

        public override void ProcessPrivateTransitions()
        {
            throw new System.NotImplementedException();
        }

        public override string GetParamsDescription()
        {
            /*public int MaxTreeHeightParam { get; set; } = 1; //Для растительности 0-1 м. MaxTreeHeight  = 1/ Для растительности 1-4 м. MaxTreeHeight  = 4./Для растительности > 4м. MaxTreeHeight  = 5.
        public bool IsUnsafeTrees { get; set; } = false;*/
            string res = GetTSVDescription() + GetUnsafeTreesDescription();
            res += string.Format("Tracked parameter (Status): {0};" +
                 "\nState: {1}.", Math.Round(TrackedParameter, 1).ToString(), StateDiagram.CurrentState.Name);
            return res;
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
            return string.Format("Vegetation height is within {0} m.\n",tsvText);
          
        }

        public string GetUnsafeTreesDescription()
        {
            //Можно расписать расположение
            return IsUnsafeTrees ? string.Format("Unsafe trees count: {0}.\n", span.ThreatingTreesSpanData.ThreatingThreesCount) : "No unsave trees\n";
            
        }

        public override Color GetStateColor()
        {
            switch (StateDiagram.CurrentStateIndex)
            {
                case 0:
                    return new Color(102 / 255f, 204 / 255f, 153 / 255f);
                case 1:
                    return Color.yellow;
                case 2:
                    return Color.white;
                case 3:
                    return new Color(255f / 255f, 165f / 255f, 0);
                case 4:
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
                ("Vegetation height", GetTSVDescription()),
                ("Unsafe trees", GetUnsafeTreesDescription())
            };
        }

        public override List<string[]> GetDetailedParamsDescription()
        {
            throw new NotImplementedException();
           
        }
    }
}
