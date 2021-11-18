using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CableWalker.AgentModel
{
    public class State
    {
        public State(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }

    public class StateDiagram  //Живет только во время жизни агента
    {
        public List<State> States { get; }
        public int CurrentStateIndex { get; private set; }
        public State CurrentState => States[CurrentStateIndex];

        //public delegate int DetermineStateIndexByNewValue(float newValue);
        //public event DetermineStateIndexByNewValue OnRefreshStateIndex;
        public Func<int> DetermineStateIndexByNewValue { get; set; }

        public StateDiagram()
        {
            CurrentStateIndex = 0;
            States = new List<State>();
           // DetermineStateIndexByNewValue = refreshStatusFunc;
        }

        public void AddState(string name)
        {
            States.Add(new State(name));
        }

        public void UpdateState()
        {
            CurrentStateIndex = DetermineStateIndexByNewValue();
        }
    }
}