using System;
using System.Collections;
using System.Collections.Generic;

namespace CableWalker.Simulator.PathFinding
{
    public class BinaryHeap<T> : IPriorityQueue<T> where T : IComparable<T>
    {
        private readonly List<T> elements = new List<T>();

        public int Count => elements.Count;

        public void Enqueue(T item)
        {
            elements.Add(item);
            Up(elements.Count - 1);
        }

        public T DequeueMin()
        {
            if (elements.Count > 0)
            {
                var item = elements[0];
                elements[0] = elements[elements.Count - 1];
                elements.RemoveAt(elements.Count - 1);
                Down(0);
                return item;
            }
            throw new InvalidOperationException("Queue is empty");
        }

        private void Up(int index)
        {
            var parentIndex = GetParent(index);
            if (parentIndex >= 0 && elements[parentIndex].CompareTo(elements[index]) < 0)
            {
                var temp = elements[index];
                elements[index] = elements[parentIndex];
                elements[parentIndex] = temp;
                Up(parentIndex);
            }
        }

        private void Down(int index)
        {
            var smallestIndex = index;
            var leftI = GetLeft(index);
            var rightI = GetRight(index);

            if (leftI < Count && elements[index].CompareTo(elements[leftI]) < 0)
                smallestIndex = leftI;

            if (rightI < Count && elements[smallestIndex].CompareTo(elements[rightI]) < 0)
                smallestIndex = rightI;

            if (smallestIndex != index)
            {
                var temp = elements[index];
                elements[index] = elements[smallestIndex];
                elements[smallestIndex] = temp;
                Down(smallestIndex);
            }
        }

        private int GetParent(int index)
        {
            if (index <= 0) return -1;
            return (index - 1) / 2;
        }

        private int GetLeft(int index)
        {
            return 2 * index + 1;
        }

        private int GetRight(int index)
        {
            return 2 * index + 2;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
