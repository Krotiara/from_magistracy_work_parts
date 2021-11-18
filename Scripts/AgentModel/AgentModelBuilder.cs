using CableWalker.Simulator.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CableWalker.AgentModel
{
    public class AgentModelBuilder
    {

       // public Dictionary<string, Agent> Agents { get; set; } = new Dictionary<string, Agent>();

        public AgentModelBuilder()
        {

        }

        public Dictionary<string, Agent> Build(Simulator.InformationHolder informationHolder)
        {
            if (informationHolder == null)
                return null;
            Dictionary<string, Agent> Agents = new Dictionary<string, Agent>();
            List<Span> spans = informationHolder.GetList<Span>().Select(x => (Span)x).ToList();

            AgentPowerLine powerLineAgent = new GameObject("PowerLineAgent").AddComponent<AgentPowerLine>();
            powerLineAgent.transform.parent = GameObject.Find("Agents").transform;
            powerLineAgent.Instantiate(spans, informationHolder);
            Agents[powerLineAgent.Number] = powerLineAgent;
            powerLineAgent.CreateConnections(Agents);


            var environmentAgent = new GameObject("Environment").AddComponent<AgentEnvironment>();
            environmentAgent.Instantiate("Environment", informationHolder.Environment);
            Agents[environmentAgent.Number] = environmentAgent;
            environmentAgent.transform.parent = GameObject.Find("Agents").transform;
            environmentAgent.CreateConnections(Agents);
            environmentAgent.RecalculateTrackedParameter();


            return Agents;
        }
    }
}
