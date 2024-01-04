using System;
using System.Collections.Generic;

namespace FunnelAlgorithm.Utility
{
    /// <summary>
    /// Use heap to build a priority Queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> lst;

        public int Count
        {
            get => lst.Count;
        }

        public PriorityQueue(int capacity)
        {
            lst = new List<T>(capacity);
        }

        public void Enqueue(T value)
        {
            lst.Add(value);
            var childIndex = lst.Count - 1;
            HeapfiyUp(childIndex);
        }

        public T Dequeue()
        {
            if (lst.Count == 0) return default;
            var topIndex = 0;
            var value = lst[topIndex];
            var endIndex = lst.Count - 1;
            lst[topIndex] = lst[endIndex];
            lst.RemoveAt(endIndex);
            endIndex--;
            HeapfiyDown(topIndex, endIndex);
            return value;
        }

        public T RemoveAt(int rmvIndex)
        {
            if (lst.Count <= rmvIndex) return default;
            T value = lst[rmvIndex];
            var endIndex = lst.Count - 1;
            lst[rmvIndex] = lst[endIndex];
            lst.RemoveAt(endIndex);
            --endIndex;
            if (rmvIndex < endIndex)
            {
                var parentIndex = (rmvIndex - 1) / 2;
                if (parentIndex>0 && lst[rmvIndex].CompareTo(lst[parentIndex]) < 0)
                {
                    HeapfiyUp(rmvIndex);
                }
                else
                {
                    HeapfiyDown(rmvIndex, endIndex);
                }
            }

            return value;
        }

        public T RemoveItem(T t)
        {
            int index = IndexOf(t);
            var item = RemoveAt(index);
            return item;
        }

        public void Clear()
        {
            lst.Clear();
        }

        public bool Contains(T t)
        {
            return lst.Contains(t);
        }

        public bool IsEmpty()
        {
            return lst.Count == 0;
        }

        public List<T> ToList()
        {
            return lst;
        }

        public T[] ToArray()
        {
            return lst.ToArray();
        }

        public T Peek()
        {
            return lst.Count > 0 ? lst[0] : default;
        }

        public int IndexOf(T t)
        {
            return lst.IndexOf(t);
        }

        private void HeapfiyUp(int childIndex)
        {
            var parentIndex = (childIndex - 1) / 2;
            while (childIndex > 0 && lst[childIndex].CompareTo(lst[parentIndex]) < 0)
            {
                Swap(childIndex, parentIndex);
                childIndex = parentIndex;
                parentIndex = (childIndex - 1) / 2;
            }
        }

        private void HeapfiyDown(int topIndex, int endIndex)
        {
            while (true)
            {
                var minIndex = topIndex;
                //left
                var childIndex = topIndex * 2 + 1;
                if (childIndex <= endIndex && lst[childIndex].CompareTo(lst[topIndex]) < 0)
                {
                    minIndex = childIndex;
                }

                //right
                childIndex = topIndex * 2 + 2;
                if (childIndex <= endIndex && lst[childIndex].CompareTo(lst[minIndex]) < 0)
                {
                    minIndex = childIndex;
                }

                if (topIndex == minIndex) break;
                Swap(topIndex, minIndex);
                topIndex = minIndex;
            }
        }

        private void Swap(int a, int b)
        {
            (lst[a], lst[b]) = (lst[b], lst[a]);
        }
    }
}