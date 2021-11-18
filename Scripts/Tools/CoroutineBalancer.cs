using System;
using System.Collections;
using UnityEngine;

namespace CableWalker.Simulator.Tools
{
    public class CoroutineBalancer : Singleton<CoroutineBalancer>
    {
        public float TimeLimit;
        public FpsEncounter FpsEncounter;

        private void Start()
        {
            if (TimeLimit <= 0)
                TimeLimit = 0.001f;
        }

        private void Update()
        {
            if (FpsEncounter == null)
                return;
            
            if (FpsEncounter.Fps < 50)
                TimeLimit -= 0.001f;
            else
                TimeLimit += 0.001f;

            if (TimeLimit <= 0)
                TimeLimit = 0.001f;

            if (TimeLimit >= 0.1f)
                TimeLimit = 0.1f;
        }

        public Coroutine StartCoroutineBalance(IEnumerator routine)
        {
            return StartCoroutine(BalanceRoutine(routine));
        }

        private IEnumerator BalanceRoutine(IEnumerator routine)
        {
            var count = 1.0f;
            while (true)
            {
                for (var i = 0; i < count; i++)
                {
                    var startTime = Time.realtimeSinceStartup;

                    if (routine.MoveNext())
                    {
                        var current = routine.Current;
                        if (current != null)
                        {
                            if (current is IEnumerator coroutine)
                                yield return StartCoroutineBalance(coroutine);
                            else
                                yield return current;
                        }
                    }
                    else
                        yield break;
                    
                    var timePassed = Time.realtimeSinceStartup - startTime;
                    count = Mathf.Max(1, TimeLimit / timePassed);
                }
                yield return null;
            }
        }
    }
}
