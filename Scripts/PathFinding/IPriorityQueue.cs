using System;
using System.Collections.Generic;

namespace CableWalker.Simulator.PathFinding
{
    public interface IPriorityQueue<T> : IEnumerable<T> where T : IComparable<T>
    {
        int Count { get; }
        void Enqueue(T item);
        T DequeueMin();
    }
}
