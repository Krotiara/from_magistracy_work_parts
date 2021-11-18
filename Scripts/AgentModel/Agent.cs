using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CableWalker.AgentModel
{
    public abstract class Agent: MonoBehaviour
    {
        
        public string Number { get; set; }

        public float Weight { get; set; }

        public float TrackedParameter { get; set; } = 100;

        public StateDiagram StateDiagram { get; set; }

        public List<Agent> Connections { get; set; } = new List<Agent>();

        public List<Agent> TopConnections { get; set; } = new List<Agent>();

        public Dictionary<string,Parameter> Parameters { get; set; } = new Dictionary<string, Parameter>();

        public abstract string GetParamsDescription();

        public abstract List<(string, string)> GetParamsDescriptionForTable();

        public abstract List<string[]> GetDetailedParamsDescription();

        public abstract void ProcessMessage(Message message, Agent messenger);

        public abstract void ProcessPrivateTransitions();

        public abstract void RecalculateTrackedParameter();

        public abstract void SetStateDiagram();

        public abstract void CreateConnections(Dictionary<string, Agent> agents);

        public string ObjectName { get; set; }

        public string StateText => string.Format("State: {0}. ", StateDiagram.CurrentState.Name);

        public string TrackedParameterText => string.Format("{0} = {1}", TrackedParameterName, TrackedParameter);

        public string TrackedParameterName { get; set; } = "Tracked parameter";

        public string Description => string.Format("State: {0}.\nTracked parameter = {1}.", StateDiagram.CurrentState.Name, Math.Round(TrackedParameter, 2));

        public string NumberToTable => string.Format("Agent {0}:", Number);

        public void SendToAllConnected(Message message)
        {
            foreach (Agent agent in Connections)
            {
                message.To = agent.Number;
                SendMessage(message, agent);
            }
        }

        //public abstract void SendToAllNeighbors(Message message);

        public void SendMessage(Message message, Agent agent)
        {          
            agent.ReceiveMessage(message, this);
        }

        public void ReceiveMessage(Message message, Agent messenger)
        {
            if(CheckDestination(message))
            {
                ProcessMessage(message,messenger);
            }
        }

        public Dictionary<string, List<(string,string)>> GetConnectionsParamsInfo()
        {
            var result = new Dictionary<string, List<(string, string)>>();
            foreach (Agent c in Connections)
            {
                result[c.ObjectName] = c.GetParamsDescriptionForTable();
            }
            return result;
        }

        

        public bool CheckDestination(Message message)
        {
            return Number == message.To;
        }

        

        public abstract Color GetStateColor();

        

        


        public void ConnectTo(Agent agent)
        {
            Connections.Add(agent);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        public void CreateConnectionAgent<T>(T type) where T:Agent
        {
            //TODO
        }

       
        public void Disconnect(Agent agent)
        {
            Connections.Remove(agent);
            agent.TopConnections.Remove(this);
        }

        public override bool Equals(object obj)
        {
            return obj is Agent agent &&
                   Number == agent.Number;
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Number);
        }
    }
}
