using CableWalker.Simulator.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentSpan : Agent
    {
        public int nCables; //проектное количество
        public int nGroundCables; //проектное количество
        public int nVibrationDamper; //проектное количество

        private Dictionary<string, float> normativeDims;
        private float gladeWidthNormativeValue;

        private List<AgentCable> agentCables;
        private List<AgentGroundCable> agentGroundCables;

        private AgentSpanDimensions agentSpanDimensions;
       


        public Span span;

        private Dictionary<string, Agent> agents; //Не хорошо прокидывать ссылку, но всё же 


        public AgentSpan(Span span, Dictionary<string, float> normativeDims, float gladeWidthNormativeValue)
        {
            Instantiate(span, normativeDims, gladeWidthNormativeValue);
        }

        public void Instantiate(Span span, Dictionary<string, float> normativeDims, float gladeWidthNormativeValue)
        {
            Number = span.Number;
            agentCables = new List<AgentCable>();
            agentGroundCables = new List<AgentGroundCable>();
            Connections = new List<Agent>();
            SetStateDiagram();
            this.span = span;
            nCables = span.PowerCables.Count;
            nGroundCables = span.GroundCables.Count;
            nVibrationDamper = span.VibrationDampers.Count;
            this.normativeDims = normativeDims;
            this.gladeWidthNormativeValue = gladeWidthNormativeValue;
            ObjectName = string.Format("Span # {0}", span.Number);
            TrackedParameterName = "NTCI";

        }


        public void Update()
        {
            RecalculateTrackedParameter();
        }

        public override void RecalculateTrackedParameter()
        {
            if (Connections != null && Connections.Count > 0)
            {
                float result = 0;
                foreach (Agent agent in Connections.ToList())
                {
                    result += GetWeight(agent) * agent.TrackedParameter;
                }
                TrackedParameter = result;
            }
            StateDiagram.UpdateState();
        }

        private float GetWeight(Agent agent)
        {
            var type = agent.GetType();

            if (type.Equals(typeof(AgentCable)))
                return agent.Weight / nCables;
            else if (type.Equals(typeof(AgentGroundCable)))
                return agent.Weight / nGroundCables;
            else if (type.Equals(typeof(AgentVibrationDamper)))
                return agent.Weight / nVibrationDamper;
            else if (type.Equals(typeof(AgentSpanArea)) || type.Equals(typeof(AgentSpanDimensions)))
                return agent.Weight;
            else throw new Exception("Error types for agent Span connections");
        }

        public override void ProcessMessage(Message message, Agent messenger)
        {
            if (message.Text == MessagesTexts.cableCorrosion 
                || message.Text == MessagesTexts.cableCorrosionEliminated 
                || message.Text == MessagesTexts.updateStresses 
                || message.Text == MessagesTexts.breakRodOutClamp 
                || message.Text == MessagesTexts.breakRodOutClampEliminated || message.Text == MessagesTexts.breakRodInClamp 
                || message.Text == MessagesTexts.breakRodInClampEliminated
                || message.Text == MessagesTexts.updateSags)
            {
                message.To = string.Format("{0}.{1}", span.Number, message.Args[0]);
                SendMessage(message, agents[string.Format("{0}.{1}", span.Number, message.Args[0])]);
            }
            if (message.Text == MessagesTexts.updateGladeWidth || message.Text == MessagesTexts.updateTSV || message.Text == MessagesTexts.updateTSVByPointCloud)
            {
                message.To = string.Format("{0}.{1}", span.Number, "area");
                SendMessage(message, agents[string.Format("{0}.{1}", span.Number, "area")]);
            }
            if(message.Text == MessagesTexts.updateCableDimensions)
            {
                message.To = string.Format("{0}.{1}", span.Number, "dimensions");
                SendMessage(message, agents[string.Format("{0}.{1}", span.Number, "dimensions")]);
            }
            if(message.Text == MessagesTexts.vibrationDamperOffsetEliminated || message.Text == MessagesTexts.vibrationDamperOffset
                || message.Text == MessagesTexts.vibrationDamperAbsence || message.Text == MessagesTexts.vibrationDamperAbsenceEliminated
                || message.Text == MessagesTexts.vibrationDamperWeightsAbsence || message.Text == MessagesTexts.vibrationDamperWeightsAbsenceEliminated)
            {
                message.To = string.Format("{0}.{1}.vd{2}", span.Number, message.Args[0], message.Args[1]);
                SendMessage(message, agents[string.Format("{0}.{1}.vd{2}", span.Number, message.Args[0], message.Args[1])]);
            }

            if (message.Text == MessagesTexts.iceFormation ||
                message.Text == MessagesTexts.normalEnvironmentConditions ||
                message.Text == MessagesTexts.strongWind)
            {
                foreach(var agent in agentCables)
                {
                    message.To = agent.Number;
                    SendMessage(message, agent);
                }

                foreach(var agent in agentGroundCables)
                {
                    message.To = agent.Number;
                    SendMessage(message, agent);
                }

                SendMessage(new Message(Number, agentSpanDimensions.Number, MessagesTexts.updateCableDimensions, new List<string>().ToArray()), agentSpanDimensions);

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

        public override void CreateConnections(Dictionary<string, Agent> agents)
        {
            this.agents = agents;

            //Parallel.ForEach(span.PowerCables, new Action<Cable>(cable =>
            //{
            //    (float, float) stressAndBreakingStressPair = cable.CurrentMode.GetPairStressAndBreakingStressWithMinDelta();
            //    var agent = new AgentCable(cable, cable.GetNormativeSag(cable.CurrentMode), stressAndBreakingStressPair.Item1, stressAndBreakingStressPair.Item2, this);
            //    this.ConnectTo(agent);
            //    // agent.TopConnectTo(this);
            //    agents[agent.Number] = agent;
            //    agent.CreateConnections(agents);
            //    agentCables.Add(agent);
            //}));

            foreach (Cable cable in span.PowerCables)
            {
                (float, float) stressAndBreakingStressPair = cable.CurrentMode.GetPairStressAndBreakingStressWithMinDelta();
                var agent = new AgentCable(cable, cable.GetNormativeSag(cable.CurrentMode), stressAndBreakingStressPair.Item1, stressAndBreakingStressPair.Item2, this);
                this.ConnectTo(agent);
                // agent.TopConnectTo(this);
                agents[agent.Number] = agent;
                agent.CreateConnections(agents);
                agentCables.Add(agent);
            }

            //Parallel.ForEach(span.GroundCables, new Action<Cable>(gc =>
            //{
            //    (float, float) stressAndBreakingStressPair = gc.CurrentMode.GetPairStressAndBreakingStressWithMinDelta();
            //    var agent = new AgentGroundCable(gc, gc.GetNormativeSag(gc.CurrentMode), stressAndBreakingStressPair.Item1, stressAndBreakingStressPair.Item2, this);
            //    this.ConnectTo(agent);
            //    //  agent.TopConnectTo(this);
            //    agents[agent.Number] = agent;
            //    agent.CreateConnections(agents);
            //    agentGroundCables.Add(agent);
            //}));

            foreach (Cable gc in span.GroundCables)
            {
                (float, float) stressAndBreakingStressPair = gc.CurrentMode.GetPairStressAndBreakingStressWithMinDelta();
                var agent = new AgentGroundCable(gc, gc.GetNormativeSag(gc.CurrentMode), stressAndBreakingStressPair.Item1, stressAndBreakingStressPair.Item2, this);
                this.ConnectTo(agent);
                //  agent.TopConnectTo(this);
                agents[agent.Number] = agent;
                agent.CreateConnections(agents);
                agentGroundCables.Add(agent);
            }

            //Parallel.ForEach(span.VibrationDampers, new Action<KeyValuePair<string,VibrationDamper>>(vd =>
            //{
            //    var agent = new AgentVibrationDamper(vd.Value);
            //    this.ConnectTo(agent);
            //    // agent.TopConnectTo(this);
            //    agents[agent.Number] = agent;
            //    agent.CreateConnections(agents);
            //}));
            foreach (var vd in span.VibrationDampers)
            {
                var agent = new AgentVibrationDamper(vd.Value);
                this.ConnectTo(agent);
                // agent.TopConnectTo(this);
                agents[agent.Number] = agent;
                agent.CreateConnections(agents);
            }

            var agentArea = new AgentSpanArea(span, string.Format("{0}.area", span.Number), gladeWidthNormativeValue);
            this.ConnectTo(agentArea);
          //  agentArea.TopConnectTo(this);
            agents[agentArea.Number] = agentArea;
            agentArea.CreateConnections(agents);

            agentSpanDimensions = new AgentSpanDimensions(span, string.Format("{0}.dimensions", span.Number), normativeDims);
            this.ConnectTo(agentSpanDimensions);
           // agentDims.TopConnectTo(this);
            agents[agentSpanDimensions.Number] = agentSpanDimensions;
            agentSpanDimensions.CreateConnections(agents);
            
            RecalculateTrackedParameter();
        }

        public override void ProcessPrivateTransitions()
        {
            throw new NotImplementedException();
        }

        public override string GetParamsDescription()
        {
            return string.Format("Cables project number: {0};" +
                "\nGround cables project number: {1};" +
                "\nVibration dampers project number: {2};" +
                "\n\nTracked parameter (NTCI): {3};" +
                "\nState: {4}.", nCables, nGroundCables, nVibrationDamper, Math.Round(TrackedParameter, 1).ToString(), StateDiagram.CurrentState.Name);
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
                ("NTCI",Math.Round(TrackedParameter, 1).ToString()),
                ("State", StateDiagram.CurrentState.Name),
                ("Cables project number", nCables.ToString()),
                ("Ground cables project number", nGroundCables.ToString()),
                ("Vibration dampers project number", nVibrationDamper.ToString()),
            };
            res.AddRange(span.GetInfo());
            return res;
        }

        public override List<string[]> GetDetailedParamsDescription()
        {
            throw new NotImplementedException();
        }
    }
}
