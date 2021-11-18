using System;
using System.Collections;
using System.Collections.Generic;

namespace CableWalker.Simulator.Solvers
{
    public class PairFinder
    {
        public bool[,] Solution { get; private set; }
        
        public IEnumerator Solve(float[,] weights, float penalty)
        {
            Solution = null;
            
            var m = weights.GetLength(0);
            var n = weights.GetLength(1);
            var solution = new bool[m, n];
            
            var minValue = float.PositiveInfinity;
            var x = new int[n];
            var minX = new int[n];
            var solved = false;

            for (var k = 0; k < Math.Pow(m, n); k++)
            {
                var sum = 0.0f;

                var used = new HashSet<int>();
                for (var j = 0; j < n; j++)
                {
                    sum += weights[x[j], j];
                    used.Add(x[j]);
                }

                sum += used.Count * penalty;
                
                if (sum < minValue)
                {
                    minValue = sum;
                    Array.Copy(x, minX, n);
                    solved = true;
                }
                
                var digit = 0;
                while (digit < n && x[digit] == m - 1)
                {
                    x[digit] = 0;
                    digit++;
                }
                
                if (digit < n)
                    x[digit] += 1;

                yield return null;
            }

            if (solved)
            {
                for (var j = 0; j < n; j++)
                    solution[minX[j], j] = true;

                Solution = solution;
            }
        }
    }
}
