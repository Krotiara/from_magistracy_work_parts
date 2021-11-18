using CableWalker.Simulator.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentPowerLine : Agent
    {

        private List<Span> spans;
        private Simulator.InformationHolder infoHolder;

        public AgentPowerLine(List<Span> spans, Simulator.InformationHolder infoHolder)
        {
            Instantiate(spans, infoHolder);
            ///по листам инициализация агентов
        }

        public void Instantiate(List<Span> spans, Simulator.InformationHolder infoHolder)
        {
            Number = "Power line";
            this.spans = spans;
            this.infoHolder = infoHolder;
            Connections = new List<Agent>();
            SetStateDiagram();
            ObjectName = "Power line";
            TrackedParameterName = "TCI";
            ///по листам инициализация агентов
        }

        public override void CreateConnections(Dictionary<string, Agent> agents)
        {
            //Dictionary<string, AgentSpan> spansGameObjects = new Dictionary<string, AgentSpan>();
            //Transform parent = GameObject.Find("Agents").transform;
            //foreach (Span span in spans)
            //{
            //    spansGameObjects[span.Number] = new GameObject(span.Number).AddComponent<AgentSpan>();
            //    spansGameObjects[span.Number].transform.parent = parent;
            //    var t = new Thread(() => InstantiateSpanAgent(span, agents, spansGameObjects[span.Number]));
            //    t.Start();
            //}

            //Parallel.ForEach(spans, new Action<Span>(x => InstantiateSpanAgent(x, agents, spansGameObjects[x.Number])));
            Transform parent = GameObject.Find("Agents").transform;
            foreach (Span span in spans)
            {
                AgentSpan agent = new GameObject(span.Number).AddComponent<AgentSpan>();
                agent.Instantiate(span, infoHolder.NormativeCablesDimensions, infoHolder.GladeWidthNormativeValue);
                agent.transform.parent = parent;
                // var agent = new AgentSpan(span,infoHolder.CablesDimansionGroundNormativeValue, infoHolder.CablesDimansionBuildingNormativeValue, infoHolder.GladeWidthNormativeValue);
                ConnectTo(agent);
                // agent.TopConnectTo(this);
                agents[agent.Number] = agent;
                agent.CreateConnections(agents);
            }
        }

        private void InstantiateSpanAgent(Span span, Dictionary<string, Agent> agents, AgentSpan agent)
        {
            //AgentSpan agent = new GameObject(span.Number).AddComponent<AgentSpan>();
            agent.Instantiate(span, infoHolder.NormativeCablesDimensions, infoHolder.GladeWidthNormativeValue);
           // agent.transform.parent = GameObject.Find("Agents").transform;
            // var agent = new AgentSpan(span,infoHolder.CablesDimansionGroundNormativeValue, infoHolder.CablesDimansionBuildingNormativeValue, infoHolder.GladeWidthNormativeValue);
            ConnectTo(agent);
            // agent.TopConnectTo(this);
            agents[agent.Number] = agent;
            agent.CreateConnections(agents);
        }

        
        private void Update()
        {
            RecalculateTrackedParameter();
        }

        public override void RecalculateTrackedParameter()
        {
            if (Connections != null && Connections.Count > 0)
            {
                float min = float.MaxValue;
                foreach (Agent agent in Connections)
                {
                    if (agent.TrackedParameter < min)
                        min = agent.TrackedParameter;
                }

                TrackedParameter = min;
            }
            StateDiagram.UpdateState();
        }

        public override void ProcessMessage(Message message, Agent messenger)
        {
            throw new System.NotImplementedException();
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
            //+Инфа из info holder
            return string.Format("Tracked parameter (TCI): {0};" +
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
            return new List<(string, string)>()
            {
                ("TCI",Math.Round(TrackedParameter, 1).ToString()),
                ("State", StateDiagram.CurrentState.Name)
            };
        }

        public override List<string[]> GetDetailedParamsDescription()
        {
            throw new NotImplementedException();
        }
    }
}
