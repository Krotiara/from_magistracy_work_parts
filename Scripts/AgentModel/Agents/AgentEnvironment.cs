using CableWalker.Simulator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CableWalker.AgentModel
{
    public class AgentEnvironment : Agent
    {

        public Simulator.Environment Environment { get; private set; }
        private string currentIceParam;
        public string[] ParamsToSend => new string[6] { Environment.Temperature.ToString(), Environment.WindSpeed.ToString(), Environment.Humidity.ToString(), Environment.P.ToString(), ((int)Environment.WindDirection).ToString() , currentIceParam};
        public bool IsTMinus => Environment.Temperature < 0;
       
        public bool IsConditionForIce => IsTMinus && IsTDifferenceToLessZeroCondition;
        public bool IsConditionForBreakIce => !IsTMinus;

        public bool IsTDifferenceToLessZeroCondition { get; set; } = false;
        public bool IsStrongWindCondition => Environment.WindSpeed >= 10;
        public bool IsNormalCondition => !IsTDifferenceToLessZeroCondition && !IsStrongWindCondition;
        public bool IsDropPlusWindCondition => IsTDifferenceToLessZeroCondition && IsStrongWindCondition;

       
      

        public AgentEnvironment(string number, Simulator.Environment e)
        {
            currentIceParam = "0";
            Number = number;
            Environment = e;
            Connections = new List<Agent>();
            ObjectName = "Environment";
            
        }

        public void Instantiate(string number, Simulator.Environment e)
        {
            currentIceParam = "0";
            Number = number;
            Environment = e;
            Connections = new List<Agent>();
            SetStateDiagram();
            ObjectName = "Environment";
            
            ///по листам инициализация агентов
        }



        public override void CreateConnections(Dictionary<string, Agent> agents)
        {
            foreach (Agent a in agents.Values.ToList())
                //if (a.GetType().Equals(typeof(AgentCable)) || a.GetType().Equals(typeof(AgentGroundCable)))
                try
                {
                    if (a.GetType().Equals(typeof(AgentSpan)))
                        ConnectTo(a);
                }
                catch(NullReferenceException e)
                {
                    Debug.Log(e);
                }
        }

        public override void RecalculateTrackedParameter()
        {
            StateDiagram.UpdateState();
        }

        public override void ProcessMessage(Message message, Agent messenger)
        {
            if (message.Text == MessagesTexts.updateEnvironment)
            {
                IsTDifferenceToLessZeroCondition = Convert.ToBoolean(int.Parse(message.Args[0]));
                float t = float.Parse(message.Args[1]);
                float w = float.Parse(message.Args[2]);
                float h = float.Parse(message.Args[3]);
                float p = float.Parse(message.Args[4]);
                WindDirection windDirection = (WindDirection)Enum.Parse(typeof(WindDirection), (int.Parse(message.Args[5])).ToString());
                Environment.SetParams(t, w, h, p, windDirection);

                RecalculateTrackedParameter();
            }
        }

        public override void SetStateDiagram()
        {
            StateDiagram = new StateDiagram();
            StateDiagram.AddState("Normal");
            StateDiagram.AddState("Temperature drop with high humidity");
            StateDiagram.AddState("Temperature drop with high humidity + strong wind");
            StateDiagram.AddState("Strong wind");

            StateDiagram.DetermineStateIndexByNewValue = () =>
            {
                ProcessPrivateTransitions();
                if (IsNormalCondition)
                {
                    SendToAllConnected(new Message(Number, "all", MessagesTexts.normalEnvironmentConditions, ParamsToSend));
                    return 0;
                }
                else if (IsStrongWindCondition)
                {
                    SendToAllConnected(new Message(Number, "all", MessagesTexts.strongWind, ParamsToSend));
                    return 3;
                }
                else if (IsDropPlusWindCondition)
                {
                    SendToAllConnected(new Message(Number, "all", MessagesTexts.iceFormation, ParamsToSend.ToArray())); 
                    return 2;
                }
                else if (IsTDifferenceToLessZeroCondition)
                {
                    SendToAllConnected(new Message(Number, "all", MessagesTexts.iceFormation, ParamsToSend.ToArray()));
                    return 1;
                }
                else throw new System.Exception("Environment agent cant set his condition");
            };
            StateDiagram.UpdateState();
        }

        public override void ProcessPrivateTransitions()
        {
            if (IsConditionForIce)
                currentIceParam = "1";
            else if (IsConditionForBreakIce)
                currentIceParam = "0";
        }

        public override string GetParamsDescription()
        {
            /*public float[] ParamsToSend => new float[5] 
             * { Environment.Temperature, Environment.WindSpeed, Environment.Humidity, Environment.P, (int)Environment.WindDirection };*/
             string res = string.Format("Temperature = {0} Celsium;\nWind speed = {1} m/s;\nHumidity = {2} %;\nP = {3} mm Hg;\nWind direction = {4}\n",
                Environment.Temperature, Environment.WindSpeed, Environment.Humidity, Environment.P, Environment.WindDirection.ToString());
            res += string.Format("State: {0}.", StateDiagram.CurrentState.Name);
            return res;
        }

        public override Color GetStateColor()
        {
            switch (StateDiagram.CurrentStateIndex)
            {
                case 0:
                    return new Color(102/255f, 204/255f, 153/255f);
                case 1:
                    return Color.blue;
                case 2:
                    return Color.blue;
                case 3:
                    return Color.white;
               
                default:
                    return Color.white;
            }
        }

        public override List<(string, string)> GetParamsDescriptionForTable()
        {
            return new List<(string, string)>()
            {
                ("State",StateDiagram.CurrentState.Name),
                ("Temperature (Celsium)", Environment.Temperature.ToString()),
                ("Wind speed (m/s)", Environment.WindSpeed.ToString()),
                ("Humidity (%)", Environment.Humidity.ToString()),
                ("P (mm Hg)", Environment.P.ToString()),
                ("Wind direction", Environment.WindDirection.ToString())
            };
        }

        public override List<string[]> GetDetailedParamsDescription()
        {
            throw new NotImplementedException();
        }
    }
}
